using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DisableShelves
{
    public static class Extensions
    {
        public static string GetColor(this Label label)
        {
            return label.gameObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color.ToHex();
        }
        public static Label GetLabel(this DisplaySlot slot)
        {
            return ((Label)typeof(DisplaySlot).GetField("m_Label", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(slot));
        }
    }
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static List<Label> RedLabels = new();
        public static ManualLogSource StaticLogger;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! Applying patch...");
            Harmony harmony = new Harmony("com.orpticon.DisableShelves");
            harmony.PatchAll();
            StaticLogger = Logger;
        }
        public void Update()
        {
            var mi_ClearLabel = typeof(Label).GetMethod("ClearLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fi_m_DisplaySlot = typeof(Label).GetField("m_DisplaySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var label in RedLabels)
            {
                var displaySlot = ((DisplaySlot)fi_m_DisplaySlot.GetValue(label));
                if (!displaySlot.HasProduct) { 
                    mi_ClearLabel.Invoke(label, null);
                    var cube = label.gameObject.transform.GetChild(0).GetChild(0);
                    var canvasbg = label.gameObject.transform.GetChild(1).GetChild(0);

                    var mat = cube.GetComponent<MeshRenderer>().material;
                    mat.color = Color.red;
                    canvasbg.GetComponent<RawImage>().enabled = false;
                    Plugin.RedLabels.Remove(label);
                }
            }
        }
    }
    public static class ShelfLabelPatch
    {
        [HarmonyPatch(typeof(DisplayManager), "GetLabeledEmptyDisplaySlots")]
        public static class DisplayManager_GetLabeledEmptyDisplaySlots_Patch
        {
            public static void Postfix(ref List<DisplaySlot> __result)
            {
                __result = __result.Where(x => x.GetLabel().GetColor() != "#FF0000").ToList();
            }
        }
        [HarmonyPatch(typeof(Restocker), "IsDisplaySlotAvailableToRestock")]
        public static class Restocker_IsDisplaySlotAvailableToRestock_Patch
        {
            public static void Postfix(DisplaySlot displaySlot, ref bool __result)
            {
                if (!__result) return;
                var color = displaySlot.GetLabel().GetColor();
                //if (!(color == "#FFD53A" || color == "#FFEB04")) Plugin.StaticLogger.LogError("color is " + color);
                if (color == "#FF0000") __result = false;
            }
        }
        [HarmonyPatch(typeof(Label), "ClearLabel")]
        [HarmonyPriority(10000)]
        public static class Label_ClearLabel_Patch
        {
            public static bool Prefix(Label __instance)
            {
                //Plugin.StaticLogger.LogInfo("Running prefix patcher");
                var fi_m_DisplaySlot = typeof(Label).GetField("m_DisplaySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var displaySlot = ((DisplaySlot)fi_m_DisplaySlot.GetValue(__instance));
                if (displaySlot != null)
                {
                    if (displaySlot.HasProduct)
                    {
                        //Plugin.StaticLogger.LogInfo("Label is shelf");
                        var cube = __instance.gameObject.transform.GetChild(0).GetChild(0);
                        var canvasbg = __instance.gameObject.transform.GetChild(1).GetChild(0);

                        var mat = cube.GetComponent<MeshRenderer>().material;
                        var color = mat.color.ToHex();
                        //Plugin.StaticLogger.LogInfo("Color is " + color);
                        if (color == "#FFD53A" || color == "#FFEB04")
                        {
                            //Plugin.StaticLogger.LogInfo("Making label red");
                            mat.color = Color.red;
                            canvasbg.GetComponent<RawImage>().enabled = false;
                            Plugin.RedLabels.Add(__instance);
                        }
                        else
                        {
                            mat.color = Color.yellow;
                            canvasbg.GetComponent<RawImage>().enabled = true;
                            Plugin.RedLabels.Remove(__instance);
                        }
                        //Plugin.StaticLogger.LogWarning("Returning true");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
