# Stellar Forge

A seeded star system generator mod for [Kitten Space Agency](https://ahwoo.com/app/100000/kitten-space-agency) (KSA). Generates entire star systems procedurally from a seed, with GPU-accelerated texture generation via ILGPU.

## Features

- **Procedural Star Systems** — Deterministic generation from any seed string. Same seed always produces the same system.
- **Realistic Astrophysics** — IMF-weighted spectral type distribution, Titius-Bode orbital spacing, Hill sphere-constrained moons, habitable zone and frost line placement.
- **GPU Texture Generation** — ILGPU-powered Perlin noise on the GPU with automatic CPU fallback. Generates diffuse, normal, and height maps for every body.
- **In-Game ImGui UI** — Configure seed, planet count, habitability bias, moon frequency, gas giant chance, and texture resolution.
- **KSA XML Export** — Outputs `Astronomicals.xml` and `System.xml` in the exact format KSA expects.
- **Runtime Injection** — Best-effort live injection into `CelestialSystem` via reflection, with clean fallback to XML + restart.

## Building

### Requirements

- .NET 8+ SDK
- No game DLLs needed for development — game types are stubbed

### Build & Test

```bash
dotnet build
dotnet test
```

The project compiles against stub types when `Import/KSA.dll` is not present. When building against the real game, place `KSA.dll`, `Brutal.ImGuiAPI.dll`, and `StarMap.API.dll` in `StellarForge/Import/` — this defines `KSA_REAL` and excludes the stubs.

## Installing in KSA

1. Build in Release: `dotnet build -c Release`
2. Copy `StellarForge/bin/Release/net8.0/` contents to `Documents/My Games/Kitten Space Agency/Content/StellarForge/`
3. Copy the `Content/` folder alongside
4. Add `StellarForge` to your `manifest.toml`
5. Launch KSA via StarMap

## Project Structure

```
StellarForge/
├── StellarForge.cs              # [StarMapMod] entry point
├── GeneratorUi.cs               # ImGui window
├── mod.toml                     # KSA mod manifest
├── Generation/
│   ├── Models/                  # Data models (StarData, PlanetData, etc.)
│   ├── SeededRandom.cs          # Deterministic RNG
│   ├── OrbitalMechanics.cs      # Hill sphere, stability, habitable zone
│   ├── SystemGenerator.cs       # Orchestrator
│   ├── StarGenerator.cs         # Spectral type → star properties
│   ├── PlanetGenerator.cs       # Titius-Bode spacing, type selection
│   ├── MoonGenerator.cs         # Hill sphere-constrained moons
│   ├── AtmosphereGenerator.cs   # Rayleigh/Mie scattering by type
│   └── NameGenerator.cs         # Procedural syllable-based naming
├── Textures/
│   ├── TextureGenerator.cs      # ILGPU orchestrator
│   ├── NoiseGenerator.cs        # 3D Perlin noise (GPU + CPU)
│   ├── ColorMapper.cs           # Height-to-color gradients
│   ├── NormalMapper.cs          # Sobel filter → tangent-space normals
│   └── PngWriter.cs             # Zero-dependency PNG encoder
├── Export/
│   ├── XmlExporter.cs           # Astronomicals.xml
│   ├── SystemXmlExporter.cs     # System.xml
│   └── ModTomlWriter.cs         # mod.toml updater
├── Runtime/
│   └── RuntimeInjector.cs       # Live CelestialSystem injection
└── Stubs/                       # Compile-time stubs (#if !KSA_REAL)
```

## How It Works

1. User enters a seed and configures parameters in the ImGui UI
2. `SystemGenerator` orchestrates: star → planets → atmospheres → moons
3. `TextureGenerator` produces height/diffuse/normal PNGs per body (GPU or CPU)
4. `XmlExporter` writes KSA-compatible XML files
5. `RuntimeInjector` attempts live injection; falls back to "restart to load"

## Generation Details

| Component | Algorithm |
|-----------|-----------|
| Star type | IMF-weighted (M=65%, K=15%, G=10%, F=5%, A=2%, B=1%, O=0.3%) |
| Planet spacing | Modified Titius-Bode: `a_n = a_0 × k^n × jitter` |
| Planet type | Zone-based: hot → Rocky/Dwarf, HZ → OceanWorld/Rocky, frost+ → GasGiant/IceGiant |
| Moons | Log-spaced between 3× parent radius and ⅓ Hill sphere |
| Stability | Mutual Hill radius × 3.46 criterion, iterative push-apart |
| Textures | Spherical FBM Perlin noise, per-type color gradients, Sobel normals |

## License

MIT
