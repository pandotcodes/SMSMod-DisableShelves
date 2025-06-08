using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace DisableShelves
{
    public static partial class ShelfLabelPatch
    {
        [HarmonyPatch(typeof(DisplayManager), "GetLabeledEmptyDisplaySlots")]
        public static class DisplayManager_GetLabeledEmptyDisplaySlots_Patch
        {
            public static void Postfix(ref List<DisplaySlot> __result)
            {
                __result = __result.Where(x => x.GetLabel().IsEnabled()).ToList();
            }
        }
    }
}
