using HarmonyLib;

namespace DisableShelves
{
    public static partial class ShelfLabelPatch
    {
        [HarmonyPatch(typeof(Restocker), "IsDisplaySlotAvailableToRestock")]
        public static class Restocker_IsDisplaySlotAvailableToRestock_Patch
        {
            public static void Postfix(DisplaySlot displaySlot, ref bool __result)
            {
                if (!__result) return;
                if (!displaySlot.GetLabel().IsEnabled()) __result = false;
            }
        }
    }
}
