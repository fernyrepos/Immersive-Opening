using HarmonyLib;
using Verse.Profile;

namespace ImmersiveOpening
{
    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class MemoryUtility_ClearAllMapsAndWorld_Patch
    {
        public static void Prefix()
        {
            Root_OnGUI_Patch.isImmersiveOpeningActive = false;
            Root_OnGUI_Patch.Unpatch();
        }
    }
}