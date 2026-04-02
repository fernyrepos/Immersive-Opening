using Verse;

namespace ImmersiveOpening
{
    public class ImmersiveOpeningSettings : ModSettings
    {
        public float timeBetweenSentences = 8f;
        public bool letterboxing = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeBetweenSentences, "timeBetweenSentences", 8f);
            Scribe_Values.Look(ref letterboxing, "letterboxing", true);
        }
    }
}
