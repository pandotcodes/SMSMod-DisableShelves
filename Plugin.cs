using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DisableShelves
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string YellowColor = "#FFDF00";
        public const string RedColor = "#FF0000";
        public static List<Label> RedLabels = new();
        public static ManualLogSource StaticLogger;
        public static ConfigEntry<string> DisabledLabels { get; private set; }
        public static List<string> DisabledLabelsGUIDs { get => DisabledLabels.Value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(); set => DisabledLabels.Value = string.Join(";", value); }
        public static ConfigEntry<KeyboardShortcut> MultiModeShortcut { get; private set; }
        private void Dumbass() => Awake();
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! Applying patch...");
            Harmony harmony = new Harmony("com.orpticon.DisableShelves");
            harmony.PatchAll();
            StaticLogger = Logger;
            DisabledLabels = Config.Bind("Storage", "Disabled Labels (Do not edit)", "", "Semicolon separated list of labels to disable. This is maintained automatically and should not be manually altered.");
            MultiModeShortcut = Config.Bind("General", "Multi Mode Shortcut", new KeyboardShortcut(KeyCode.LeftControl), "What key to hold while clicking in order to enable/disable entire shelves at once.");
            //SceneManager.sceneLoaded += (a, b) =>
            //{
            //    if (Singleton<DisplayManager>.Instance != null)
            //    {
            //        var disabled = DisabledLabelsGUIDs;
            //        Singleton<DisplayManager>.Instance.DisplayedProducts.Values.SelectMany(x => x).Where(x =>
            //        {
            //            bool disable = disabled.Contains(x.GetLabel().ToGUID());
            //            StaticLogger.LogInfo(x.GetLabel().ToGUID() + ": " + disable);
            //            return disable;
            //        }).ForEach(x => x.GetLabel().Disable());
            //    }
            //};
        }
        public void Update()
        {
            var mi_ClearLabel = typeof(Label).GetMethod("ClearLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fi_m_DisplaySlot = typeof(Label).GetField("m_DisplaySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var label in RedLabels)
            {
                var displaySlot = ((DisplaySlot)fi_m_DisplaySlot.GetValue(label));
                if (!displaySlot.HasProduct)
                {
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
}
