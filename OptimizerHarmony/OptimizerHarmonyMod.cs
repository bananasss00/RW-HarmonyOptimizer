using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace OptimizerHarmony
{
    public class OptimizerHarmonyMod : Mod
    {
        public OptimizerHarmonyMod(ModContentPack content) : base(content)
        {
            var h = new Harmony("pirateby.harmony.optimizer");
            h.PatchAll();
            Log.Msg($"Initialized");
        }

#if DEBUG
        public override string SettingsCategory() => "OptimizerHarmonyMod";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var l = new Listing_Standard();
            l.Begin(inRect);
            GenTypes_Patch.DoSettingsWindowContents(l);
            AccessTools_TypeByName_Patch.DoSettingsWindowContents(l);
            l.End();
        }
#endif
    }
}
