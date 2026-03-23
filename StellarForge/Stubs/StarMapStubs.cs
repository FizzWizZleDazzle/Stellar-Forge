#if !KSA_REAL
using System;

namespace StarMap.API
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StarMapModAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class StarMapImmediateLoadAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class StarMapBeforeGuiAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class StarMapAfterGuiAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class StarMapAllModsLoadedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class StarMapUnloadAttribute : Attribute { }
}
#endif
