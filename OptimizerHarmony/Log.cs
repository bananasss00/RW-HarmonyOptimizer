namespace OptimizerHarmony
{
    public class Log
    {
        public static void Msg(string message) => Verse.Log.Message($"[HarmonyOptimizer] {message}");
        public static void Warn(string message) => Verse.Log.Warning($"[HarmonyOptimizer] {message}");
        public static void Err(string message) => Verse.Log.Error($"[HarmonyOptimizer] {message}");
        
    }
}