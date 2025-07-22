using RimWorld;

namespace ASA_Yautja_Xenotype
{
    public class Thought_MarkedKillDynamic : Thought_Memory
    {
        public float moodPowerFactor = 0f;
        public override float MoodOffset()
        {
            return moodPowerFactor;
        }

        public override string LabelCap
        {
            get
            {
                if (moodPowerFactor >= 1.0f)
                    return "Proud of mighty kill";
                else if (moodPowerFactor >= 0.5f)
                    return "Satisfied with a worthy kill";
                else if (moodPowerFactor >= 0.1f)
                    return "Barely satisfied with weak prey";
                else
                    return "Disappointed with the kill";
            }
        }

        public override string Description
        {
            get
            {
                if (moodPowerFactor >= 1.0f)
                    return "They marked the corpse of a powerful beast and feel immense pride.";
                else if (moodPowerFactor >= 0.5f)
                    return "They feel a modest sense of accomplishment for the kill.";
                else if (moodPowerFactor >= 0.1f)
                    return "The kill was unremarkable.";
                else
                    return "The kill was pathetic and gave no satisfaction.";
            }
        }
    }
}
