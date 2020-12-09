using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace OptimizerHarmony
{
    [HarmonyPatch(typeof(AppDomain))]
    class AppDomain_Load_Patch
    {
        [HarmonyPatch(nameof(AppDomain.Load), typeof(byte[]))]
        [HarmonyPostfix]
        static void Load(AppDomain __instance, byte[] rawAssembly)
        {
            if (__instance == AppDomain.CurrentDomain)
            {
                GenTypes_Patch.ResetCaches();
                Section_Ctor_Patch.ResetCaches();
                AccessTools_TypeByName_Patch.ResetCaches();
                Log.Msg($"New assembly loaded. Size: {rawAssembly.Length}");
            }
        }
    }
}