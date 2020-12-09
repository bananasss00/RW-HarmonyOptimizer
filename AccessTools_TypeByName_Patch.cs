using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace OptimizerHarmony
{
    /// <summary>
    /// Access to original not patched AccessTools methods
    /// </summary>
    [HarmonyPatch(typeof(AccessTools))]
    public class AccessTools_Original
    {
        [HarmonyReversePatch]
        [HarmonyPatch(nameof(AccessTools.TypeByName))]
        public static Type TypeByName(string name) => throw new NotImplementedException("It's a stub");
    }

    [HarmonyPatch(typeof(AccessTools))]
    class AccessTools_TypeByName_Patch
    {
        static Dictionary<string, Type> TypeByString;

        public static void ResetCaches()
        {
            TypeByString = new Dictionary<string, Type>();

            var sw = Stopwatch.StartNew();
            var mem = GC.GetTotalMemory(false);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
            var types = assemblies.SelectMany(AccessTools.GetTypesFromAssembly);
            foreach (var type in types)
            {
                var fullName = type.FullName;
                if (!TypeByString.ContainsKey(fullName))
                    TypeByString.Add(fullName, type);

                var name = type.Name;
                if (!TypeByString.ContainsKey(name))
                    TypeByString.Add(name, type);
            }
            Log.Msg($"Create TypeByName cache... Elapsed time: {sw.ElapsedTicks.TicksToMs(4)}ms. Allocated memory: {(int)((GC.GetTotalMemory(false) - mem) / 1024)}kb");
        }

        [HarmonyPatch(nameof(AccessTools.TypeByName))]
        [HarmonyPrefix]
        static bool TypeByNamePrefix(string name, ref Type __result)
        {
            if (TypeByString == null) ResetCaches();

            StartStopwatch();

            if (!TypeByString.TryGetValue(name, out __result))
            {
                __result = AccessTools_Original.TypeByName(name);
                if (__result != null)
                {
                    TypeByString.Add(name, __result);
                    Log.Warn($"[TypeByName] can't find type {name}. But from original method found!");
                }
                // else Log.Error($"[OptimizerHarmony:TypeByName] can't find type {name}!");
            }

            StopStopwatch(name);

            if (__result is null && Harmony.DEBUG)
                FileLog.Log($"AccessTools.TypeByName: Could not find type named {name}");
            return false;
        }

        #region PERFOMANCE CHECK
        private static List<(string, double, double)> _impacts = new List<(string, double, double)>();
        private static Stopwatch _sw;

        [Conditional("DEBUG")]
        static void StartStopwatch()
        {
            _sw = Stopwatch.StartNew();
        }

        [Conditional("DEBUG")]
        static void StopStopwatch(string typeName)
        {
            var dictImpact = _sw.Elapsed.TotalMilliseconds;

            _sw = Stopwatch.StartNew();
            _ = AccessTools_Original.TypeByName(typeName);
            var originalImpact = _sw.Elapsed.TotalMilliseconds;

            _impacts.Add((typeName, dictImpact, originalImpact));
        }

        [Conditional("DEBUG")]
        public static void DoSettingsWindowContents(Listing_Standard l)
        {
            l.Label("=== Harmony ===");
            l.Label($"TypeByName from dict: {_impacts.Sum(x => x.Item2)}ms");
            l.Label($"TypeByName original: {_impacts.Sum(x => x.Item3)}ms");
            l.Label($"TypeByName methods:");
            foreach (var tuple in _impacts)
            {
                l.Label($"  {tuple.Item1}");
            }
        }
        #endregion
    }
}