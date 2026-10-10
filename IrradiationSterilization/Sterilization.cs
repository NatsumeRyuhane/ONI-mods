using System;
using Klei.AI;
using PeterHan.PLib.Options;

namespace IrradiationSterilization
{
    // The kill model is target theory: N = N0 * e^(-k * D). Grid.Radiation is a dose rate in rads per cycle (duplicants
    // absorb Grid.Radiation / 600 rads per second, see RadiationMonitor), so dt seconds deliver D = rads * dt / 600.
    // Each germ's k comes from its D50, the dose that kills half: k = ln 2 / D50. The surviving fraction is computed
    // exactly, so it stays between 0 and 1 for any dose. Vanilla's own kill (radiationKillRate germs per rad per second,
    // a fixed count) stays as a floor, which clears small populations the exponential leaves behind. At or above
    // Instant-Kill Radiation, every germ dies at once.
    public static class Sterilization
    {
        private const double SecondsPerCycle = 600.0;

        private static readonly Random Rng = new Random();

        private static Config Options => SingletonOptions<Config>.Instance;

        // 0 means no D50: the germ keeps vanilla behavior. Germs added by other mods have none.
        public static float HalfKillRads(Disease disease)
        {
            var diseases = Db.Get().Diseases;
            if (disease == diseases.FoodGerms)
            {
                return Options.FoodPoisoningHalfKillRads;
            }
            if (disease == diseases.SlimeGerms)
            {
                return Options.SlimelungHalfKillRads;
            }
            if (disease == diseases.ZombieSpores)
            {
                return Options.ZombieSporesHalfKillRads;
            }
            if (disease == diseases.PollenGerms)
            {
                return Options.FloralScentHalfKillRads;
            }
            return 0f;
        }

        public static bool Affects(Disease disease)
        {
            return HalfKillRads(disease) > 0f;
        }

        public static bool IsInstant(float rads)
        {
            float instantKillRads = Options.InstantKillRads;
            return instantKillRads > 0f && rads >= instantKillRads;
        }

        // Germs the exponential model kills out of count over dt seconds, not counting the vanilla floor.
        public static float ProportionalKill(Disease disease, int count, float rads, float dt)
        {
            double halvings = rads * (double)dt / (HalfKillRads(disease) * SecondsPerCycle);
            return (float)(count * (1.0 - Math.Pow(2.0, -halvings)));
        }

        // Time for half the germs to die at this radiation, matching vanilla's details panel: vanilla's linear kill
        // takes count * 0.5 / (rate * rads) seconds, the exponential model takes D50 / rads cycles, and the faster of
        // the two wins.
        public static float HalfLife(Disease disease, int count, float rads)
        {
            if (IsInstant(rads))
            {
                return 0f;
            }
            float proportional = (float)(HalfKillRads(disease) * SecondsPerCycle / rads);
            float vanillaRate = disease.radiationKillRate;
            if (vanillaRate <= 0f)
            {
                return proportional;
            }
            return Math.Min(proportional, count * 0.5f / (vanillaRate * rads));
        }

        // Germ counts are whole numbers. Rounding down would leave small populations immortal, so round up with a
        // probability equal to the fraction.
        public static int RoundRandomly(float amount)
        {
            if (amount <= 0f)
            {
                return 0;
            }
            int whole = (int)amount;
            return Rng.NextDouble() < amount - whole ? whole + 1 : whole;
        }
    }
}
