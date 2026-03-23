#if !KSA_REAL
using System.Collections.Generic;

namespace KSA
{
    public class Astronomical
    {
        public Astronomical(CelestialSystem system, AstronomicalTemplate bodyTemplate, string id = "") { }

        public VkDescriptorSet TextureSet { get; set; }
        public bool RenderDataLoaded { get; set; }
        public CelestialRenderData RenderData { get; set; } = default!;
    }

    public class AstronomicalTemplate { }

    public class CelestialSystem
    {
        public CelestialList All { get; set; } = default!;
    }

    public class CelestialList
    {
        public List<Astronomical> GetList() => new();
    }

    public static class Universe
    {
        public static CelestialSystem CelestialSystem { get; set; } = default!;
    }

    public class Mod { }

    public static class ModLibrary
    {
        public static T Get<T>(string id) => default!;
    }

    public static class Program { }

    public struct VkDescriptorSet { }

    public class CelestialRenderData { }

    public class SoundReference { }
}
#endif
