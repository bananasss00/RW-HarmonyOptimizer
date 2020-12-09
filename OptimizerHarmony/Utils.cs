using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using Verse;

namespace OptimizerHarmony
{
    static class Utils
    {
        public static double TicksToMs(this long ticks, int digits)
        {
            return Math.Round(((double)ticks / Stopwatch.Frequency) * 1000L, digits);
        }
    }
}