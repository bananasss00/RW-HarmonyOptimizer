using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using Verse;

namespace OptimizerHarmony
{
    [HarmonyPatch(typeof(AccessTools), nameof(AccessTools.TypeByName))]
    class HarmonyTypeByNameCacher
    {
        static readonly Dictionary<string, Type> TypeByString_FullName = new Dictionary<string, Type>();
        static readonly Dictionary<string, Type> TypeByString_Name = new Dictionary<string, Type>();
        static readonly Dictionary<string, Type> TypeByString_New = new Dictionary<string, Type>();

        private static bool _initialized = false;

        static void Initialize()
        {
            var sw = Stopwatch.StartNew();
            var mem = GC.GetTotalMemory(false);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
            var types = assemblies.SelectMany(a => AccessTools.GetTypesFromAssembly(a));
            foreach (var type in types)
            {
                var fullName = type.FullName;
                if (!TypeByString_FullName.ContainsKey(fullName))
                    TypeByString_FullName.Add(fullName, type);

                var name = type.Name;
                if (!TypeByString_Name.ContainsKey(name))
                    TypeByString_Name.Add(name, type);
            }
            Log.Warning($"[OptimizerHarmony:Initialize] Elapsed time: {sw.ElapsedTicks.TicksToMs(4)}ms. Allocated memory: {(int)((GC.GetTotalMemory(false) - mem) / 1024)}kb");
        }

        [HarmonyPrefix]
        static bool TypeByNamePrefix(string name, ref Type __result)
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            if (!TypeByString_FullName.TryGetValue(name, out __result))
            {
                if (!TypeByString_Name.TryGetValue(name, out __result))
                {
                    if (!TypeByString_New.TryGetValue(name, out __result)) // try call original
                    {
                        __result = TypeByName(name);
                        if (__result != null) TypeByString_New.Add(name, __result);
                    }
                    Log.Warning($"[OptimizerHarmony:TypeByName] can't find type {name}");
                }
            }

            if (__result is null && Harmony.DEBUG)
                FileLog.Log($"AccessTools.TypeByName: Could not find type named {name}");
            return false;
        }

        static Type TypeByName(string name)
        {
            var type = Type.GetType(name, false);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
            if (type is null)
                type = assemblies
                    .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                    .FirstOrDefault(t => t.FullName == name);
            if (type is null)
                type = assemblies
                    .SelectMany(a => AccessTools.GetTypesFromAssembly(a))
                    .FirstOrDefault(t => t.Name == name);
            if (type is null && Harmony.DEBUG)
                FileLog.Log($"AccessTools.TypeByName: Could not find type named {name}");
            return type;
        }
    }
}