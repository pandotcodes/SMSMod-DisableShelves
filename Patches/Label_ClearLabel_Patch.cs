using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace DisableShelves
{
    public static partial class ShelfLabelPatch
    {
        [HarmonyPatch(typeof(Label), "SetProductIcon")]
        public static class Label_SetProductIcon_Patch
        {
            public static void Prefix(Label __instance, int productID)
            {
                __instance.IsEnabled();
            }
        }
        [HarmonyPatch(typeof(Label), "ClearLabel")]
        [HarmonyPriority(10000)]
        public static class Label_ClearLabel_Patch
        {
            public static bool Prefix(Label __instance)
            {
                Plugin.StaticLogger.LogInfo("Label: " + __instance.ToGUID());
                Plugin.StaticLogger.LogInfo("Running prefix patcher");
                var fi_m_DisplaySlot = typeof(Label).GetField("m_DisplaySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var displaySlot = ((DisplaySlot)fi_m_DisplaySlot.GetValue(__instance));
                if (displaySlot != null)
                {
                    if (displaySlot.HasProduct)
                    {
                        bool all = Plugin.MultiModeShortcut.Value.IsPressed();
                        Plugin.StaticLogger.LogInfo("Multi Mode Shortcut pressed: " + all);
                        Plugin.StaticLogger.LogInfo("Label is shelf");
                        if (__instance.IsEnabled())
                            __instance.Disable(all);
                        else
                            __instance.Enable(all);

                        Plugin.StaticLogger.LogWarning("Returning false");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
