using System.Numerics;
using System.Text;
using Brutal.ImGuiAPI;
using StellarForge.Export;
using StellarForge.Generation;
using StellarForge.Generation.Models;
using StellarForge.Runtime;
using StellarForge.Textures;

namespace StellarForge;

public class GeneratorUi
{
    private readonly GeneratorConfig _config = new();
    private readonly byte[] _seedBuffer = new byte[256];
    private readonly byte[] _nameBuffer = new byte[256];

    private bool _generating;
    private float _progress;
    private string _statusText = "Ready";
    private SystemData? _lastGenerated;
    private string? _errorText;
    private string _outputPath = "";

    public GeneratorUi()
    {
        Encoding.UTF8.GetBytes(_config.SeedText, _seedBuffer);
        Encoding.UTF8.GetBytes(_config.SystemName, _nameBuffer);
    }

    public void SetOutputPath(string path) => _outputPath = path;

    public void Draw()
    {
        if (!ImGui.Begin((ImString)"Stellar Forge - Star System Generator", ImGuiWindowFlags.None))
        {
            ImGui.End();
            return;
        }

        DrawConfiguration();
        ImGui.Separator();
        DrawParameters();
        ImGui.Separator();
        DrawTextureOptions();
        ImGui.Separator();
        DrawGenerateButton();
        DrawProgress();
        DrawPreview();
        DrawErrors();

        ImGui.End();
    }

    private void DrawConfiguration()
    {
        ImGui.Text((ImString)"Configuration");

        ImGui.InputText((ImString)"Seed", _seedBuffer, (uint)_seedBuffer.Length);
        _config.SeedText = GetStringFromBuffer(_seedBuffer);

        ImGui.InputText((ImString)"System Name", _nameBuffer, (uint)_nameBuffer.Length);
        _config.SystemName = GetStringFromBuffer(_nameBuffer);
    }

    private void DrawParameters()
    {
        ImGui.Text((ImString)"Generation Parameters");

        int min = _config.MinPlanets;
        int max = _config.MaxPlanets;
        float hab = _config.HabitabilityBias;
        float moon = _config.MoonFrequency;
        float gas = _config.GasGiantChance;

        ImGui.SliderInt((ImString)"Min Planets", ref min, 1, 12);
        ImGui.SliderInt((ImString)"Max Planets", ref max, 2, 12);
        ImGui.SliderFloat((ImString)"Habitability Bias", ref hab, 0f, 1f);
        ImGui.SliderFloat((ImString)"Moon Frequency", ref moon, 0f, 1f);
        ImGui.SliderFloat((ImString)"Gas Giant Chance", ref gas, 0f, 1f);

        _config.MinPlanets = min;
        _config.MaxPlanets = Math.Max(max, min);
        _config.HabitabilityBias = hab;
        _config.MoonFrequency = moon;
        _config.GasGiantChance = gas;
    }

    private void DrawTextureOptions()
    {
        ImGui.Text((ImString)"Texture Generation");

        int res = _config.TextureResolution;
        bool gpu = _config.UseGpu;

        ImGui.SliderInt((ImString)"Resolution", ref res, 256, 4096);
        ImGui.Checkbox((ImString)"Use GPU (ILGPU)", ref gpu);

        _config.TextureResolution = res;
        _config.UseGpu = gpu;
    }

    private void DrawGenerateButton()
    {
        if (_generating) ImGui.BeginDisabled();

        if (ImGui.Button((ImString)"Generate Star System"))
        {
            StartGeneration();
        }

        if (_generating) ImGui.EndDisabled();
    }

    private void DrawProgress()
    {
        if (!_generating && _lastGenerated == null) return;

        ImGui.ProgressBar(_progress);
        ImGui.Text((ImString)_statusText);
    }

    private void DrawPreview()
    {
        if (_lastGenerated == null) return;

        ImGui.Separator();
        ImGui.Text((ImString)$"System: {_lastGenerated.DisplayName} (Seed: {_lastGenerated.Seed})");

        var star = _lastGenerated.Star;
        ImGui.TextColored(new Vector4(star.ColorR, star.ColorG, star.ColorB, 1),
            (ImString)$"Star: {star.Name} ({star.SpectralType}-class, {star.MeanRadiusKm:N0} km)");

        foreach (var planet in _lastGenerated.Planets)
        {
            string moonInfo = planet.Moons.Count > 0 ? $", {planet.Moons.Count} moon(s)" : "";
            string atmoInfo = planet.HasAtmosphere ? ", atmosphere" : "";
            double smaAU = OrbitalMechanics.KmToAu(planet.SemiMajorAxisKm);

            if (ImGui.TreeNode((ImString)$"{planet.Name} [{planet.PlanetType}] - {smaAU:F2} AU, R={planet.MeanRadiusKm:N0} km{moonInfo}{atmoInfo}"))
            {
                foreach (var moon in planet.Moons)
                {
                    ImGui.Text((ImString)$"  Moon: {moon.Name} - R={moon.MeanRadiusKm:N0} km, SMA={moon.SemiMajorAxisKm:N0} km");
                }
                ImGui.TreePop();
            }
        }
    }

    private void DrawErrors()
    {
        if (_errorText == null) return;
        ImGui.Separator();
        ImGui.TextColored(new Vector4(1, 0, 0, 1), (ImString)_errorText);
    }

    private void StartGeneration()
    {
        _generating = true;
        _progress = 0;
        _statusText = "Starting generation...";
        _errorText = null;
        _lastGenerated = null;

        var configCopy = new GeneratorConfig
        {
            SeedText = _config.SeedText,
            SystemName = _config.SystemName,
            MinPlanets = _config.MinPlanets,
            MaxPlanets = _config.MaxPlanets,
            HabitabilityBias = _config.HabitabilityBias,
            MoonFrequency = _config.MoonFrequency,
            GasGiantChance = _config.GasGiantChance,
            TextureResolution = _config.TextureResolution,
            UseGpu = _config.UseGpu,
        };

        string outputPath = _outputPath;

        Task.Run(() =>
        {
            try
            {
                // Generate system data
                var system = SystemGenerator.Generate(configCopy, (p, s) =>
                {
                    _progress = p * 0.5f;
                    _statusText = s;
                });

                // Generate textures
                using var texGen = new TextureGenerator();
                texGen.Initialize(configCopy.UseGpu);
                texGen.GenerateAllTextures(system, outputPath, configCopy.TextureResolution, (p, s) =>
                {
                    _progress = 0.5f + p * 0.3f;
                    _statusText = s;
                });

                // Export XML
                _statusText = "Exporting XML...";
                _progress = 0.85f;

                string astroPath = Path.Combine(outputPath, "GeneratedAstronomicals.xml");
                string sysPath = Path.Combine(outputPath, "GeneratedSystem.xml");
                string tomlPath = Path.Combine(outputPath, "mod.toml");

                XmlExporter.Save(system, astroPath);
                SystemXmlExporter.Save(system, sysPath);
                ModTomlWriter.Write(tomlPath, "GeneratedAstronomicals.xml", "GeneratedSystem.xml");

                // Try runtime injection
                _statusText = "Attempting runtime injection...";
                _progress = 0.95f;

                bool injected = RuntimeInjector.TryInject(system);

                _lastGenerated = system;
                _progress = 1.0f;
                _statusText = injected
                    ? "System injected into game!"
                    : "Files written - restart game to load system";
            }
            catch (Exception ex)
            {
                _errorText = $"Generation failed: {ex.Message}";
                _statusText = "Failed";
            }
            finally
            {
                _generating = false;
            }
        });
    }

    private static string GetStringFromBuffer(byte[] buffer)
    {
        int end = Array.IndexOf(buffer, (byte)0);
        if (end < 0) end = buffer.Length;
        return Encoding.UTF8.GetString(buffer, 0, end);
    }
}
