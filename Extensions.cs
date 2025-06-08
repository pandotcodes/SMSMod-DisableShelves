using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace DisableShelves
{
    public static class Extensions
    {
        public static string GetColor(this Label label)
        {
            return label.gameObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color.ToHex();
        }
        public static object GetField(this object obj, string field)
        {
            return obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj);
        }
        public static Label GetLabel(this DisplaySlot slot)
        {
            return ((Label)typeof(DisplaySlot).GetField("m_Label", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(slot));
        }
        public static string ToGUID(this Label label)
        {
            return label.transform.position.x.ToString("0.0000") + ":" + label.transform.position.y.ToString("0.0000") + ":" + label.transform.position.z.ToString("0.0000");
        }
        public static void Disable(this Label __instance, bool all = false)
        {
            if (all)
            {
                ((DisplaySlot[])__instance.DisplaySlot.Display.GetField("m_DisplaySlots")).ForEach(x => x.GetLabel().Disable(false));
                return;
            }

            Plugin.StaticLogger.LogInfo("Disabling " + __instance.ToGUID());
            var cube = __instance.gameObject.transform.GetChild(0).GetChild(0);
            var canvasbg = __instance.gameObject.transform.GetChild(1).GetChild(0);

            var mat = cube.GetComponent<MeshRenderer>().material;
            mat.color = Plugin.RedColor.ToUnityColor();

            List<string> disabled = Plugin.DisabledLabelsGUIDs;
            if (!disabled.Contains(__instance.ToGUID()))
                disabled.Add(__instance.ToGUID());
            Plugin.DisabledLabelsGUIDs = disabled;
        }
        public static void Enable(this Label __instance, bool all = false)
        {
            if (all)
            {
                ((DisplaySlot[])__instance.DisplaySlot.Display.GetField("m_DisplaySlots")).ForEach(x => x.GetLabel().Enable(false));
                return;
            }

            Plugin.StaticLogger.LogInfo("Enabling " + __instance.ToGUID());
            var cube = __instance.gameObject.transform.GetChild(0).GetChild(0);
            var canvasbg = __instance.gameObject.transform.GetChild(1).GetChild(0);

            var mat = cube.GetComponent<MeshRenderer>().material;
            mat.color = Plugin.YellowColor.ToUnityColor();

            List<string> disabled = Plugin.DisabledLabelsGUIDs;
            disabled.Remove(__instance.ToGUID());
            Plugin.DisabledLabelsGUIDs = disabled;
        }
        public static bool IsEnabled(this Label __instance)
        {
            var fi_m_DisplaySlot = typeof(Label).GetField("m_DisplaySlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var displaySlot = ((DisplaySlot)fi_m_DisplaySlot.GetValue(__instance));
            if (displaySlot != null)
            {
                if (displaySlot.HasProduct)
                {
                    var cube = __instance.gameObject.transform.GetChild(0).GetChild(0);
                    var canvasbg = __instance.gameObject.transform.GetChild(1).GetChild(0);

                    var mat = cube.GetComponent<MeshRenderer>().material;
                    var color = mat.color.ToHex();
                    Plugin.StaticLogger.LogInfo(__instance.ToGUID() + ": Color is " + color);
                    var enabled = color != Plugin.RedColor;
                    if (enabled && Plugin.DisabledLabelsGUIDs.Contains(__instance.ToGUID()))
                    {
                        __instance.Disable();
                        enabled = false;
                    }
                    return enabled;
                }
            }
            return true;
        }
    }
}
