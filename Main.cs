using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace OptimizerHarmony
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        public static readonly Harmony H;
        static List<Type> sections = new List<Type>();

        static Main()
        {
            H = new Harmony("pirateby.prepatcher.harmonyoptimizer");
            sections = typeof(SectionLayer).AllSubclassesNonAbstract().ToList();
            H.Patch(AccessTools.Constructor(typeof(Section), new Type[] {typeof(IntVec3), typeof(Map)}),
                prefix: new HarmonyMethod(typeof(Main), nameof(CtorReplacement)));
            H.PatchAll();
        }

        // simpleWiri optimize load map patch
        public static bool CtorReplacement(Section __instance, IntVec3 sectCoords, Map map)
        {
            __instance.botLeft = sectCoords * 17;
            __instance.map = map;
            __instance.layers = new List<SectionLayer>();

            foreach (var t in sections)
            {
                __instance.layers.Add((SectionLayer)Activator.CreateInstance(t, __instance));
            }

            return false;
        }

        public static double TicksToMs(this long ticks, int digits)
        {
            return Math.Round(((double)ticks / Stopwatch.Frequency) * 1000L, digits);
        }
    }
}
