using HarmonyLib;
using UnityEngine;
using Verse;

namespace ImmersiveOpening
{
    public class ImmersiveOpeningMod : Mod
    {
        public static ImmersiveOpeningSettings settings;

        public ImmersiveOpeningMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<ImmersiveOpeningSettings>();
            new Harmony("ImmersiveOpeningMod").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            settings.timeBetweenSentences = listing.SliderLabeled("IO_TimeBetweenSentences".Translate(settings.timeBetweenSentences), settings.timeBetweenSentences, 2f, 20f);

            listing.CheckboxLabeled("IO_Letterboxing".Translate(), ref settings.letterboxing);

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
}
