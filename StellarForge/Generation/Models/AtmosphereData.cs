namespace StellarForge.Generation.Models;

public class AtmosphereData
{
    // Visual - Rayleigh
    public float RayleighR { get; set; }
    public float RayleighG { get; set; }
    public float RayleighB { get; set; }
    public float RayleighScaleHeightKm { get; set; }

    // Visual - Mie
    public float MieR { get; set; }
    public float MieG { get; set; }
    public float MieB { get; set; }
    public float MieScaleHeightKm { get; set; }
    public float MiePhaseAsymmetry { get; set; }
    public float MieAbsorptionMultiplier { get; set; }

    // Visual - Ozone (optional)
    public float OzoneR { get; set; }
    public float OzoneG { get; set; }
    public float OzoneB { get; set; }
    public float OzoneAltitudeKm { get; set; }
    public float OzoneExtentKm { get; set; }

    // Visual
    public float StartHeightKm { get; set; }

    // Physical
    public float SeaLevelPressureAtm { get; set; }
    public float SeaLevelDensityKgPerM3 { get; set; }
    public float PhysicalScaleHeightKm { get; set; }
}
