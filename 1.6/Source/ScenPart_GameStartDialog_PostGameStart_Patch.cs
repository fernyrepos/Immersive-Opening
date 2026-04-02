using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ImmersiveOpening
{
    [HarmonyPatch(typeof(ScenPart_GameStartDialog), "PostGameStart")]
    public static class ScenPart_GameStartDialog_PostGameStart_Patch
    {
        public static bool Prefix(ScenPart_GameStartDialog __instance)
        {
            if (Find.GameInitData.startedFromEntry)
            {
                Find.WindowStack.Notify_GameStartDialogOpened();
                string rawText = __instance.text.NullOrEmpty() ? __instance.textKey.TranslateSimple() : __instance.text;
                Find.Archive.Add(new ArchivedDialog(rawText));

                var sentences = Regex.Split(rawText, @"(?<=[.!?])\s+")
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (sentences.Count == 0)
                {
                    sentences.Add(rawText);
                }

                Find.WindowStack.Add(new Window_ImmersiveOpening(sentences, __instance.closeSound));
                return false;
            }
            return true;
        }
    }
}
