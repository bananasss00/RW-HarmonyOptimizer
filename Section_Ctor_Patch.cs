using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using Verse;

namespace OptimizerHarmony
{
    // simpleWiri optimize load map patch
    [HarmonyPatch(typeof(Section))]
    class Section_Ctor_Patch
    {
        static List<Type> Sections;

        public static void ResetCaches()
        {
            Sections = null;
        }

        [HarmonyPatch(MethodType.Constructor, typeof(IntVec3), typeof(Map))]
        [HarmonyPrefix]
        public static bool CtorReplacement(Section __instance, IntVec3 sectCoords, Map map)
        {
            __instance.botLeft = sectCoords * 17;
            __instance.map = map;
            __instance.layers = new List<SectionLayer>();

            foreach (var t in Sections ?? (Sections = typeof(SectionLayer).AllSubclassesNonAbstract().ToList()))
            {
                __instance.layers.Add((SectionLayer)Activator.CreateInstance(t, __instance));
            }

            return false;
        }
    }
}