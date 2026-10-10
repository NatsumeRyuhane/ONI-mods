using System;
using Klei.AI;
using PeterHan.PLib.Options;

namespace IrradiationSterilization
{
    // The kill model. Each Half-Kill Radiation of rads halves the germs once per cycle, so the surviving fraction after
    // dt seconds is 2^(-rads * dt / (HalfKillRads * 600)). At the defaults, 100 rads kills 50% per cycle and 200 rads
    // kills 75%. Vanilla's own kill (radiationKillRate germs per rad per second, a fixed count) stays as a floor, so
    // germs never die slower than in vanilla. At or above Instant-Kill Radiation every germ dies at once.
    public static class Sterilization
    {
        private const double SecondsPerCycle = 600.0;

        private static readonly Random Rng = new Random();

        private static Config Options => SingletonOptions<Config>.Instance;

        // Vanilla gives Floral Scent (and the Radiation Sickness stat, which is not a real germ) a kill rate of 0. Only
        // Floral Scent can be opted in.
        public static bool Affects(Disease disease)
        {
            if (disease.radiationKillRate > 0f)
            {
                return true;
            }
            return Options.AffectFloralScent && disease.Id == "PollenGerms";
        }

        public static bool IsInstant(float rads)
        {
            return rads >= Options.InstantKillRads;
        }

        // Germs the exponential model kills out of count over dt seconds, not counting the vanilla floor.
        public static float ProportionalKill(int count, float rads, float dt)
        {
            double halvings = rads * (double)dt / (Options.HalfKillRads * SecondsPerCycle);
            return (float)(count * (1.0 - Math.Pow(2.0, -halvings)));
        }

        // Time for half the germs to die at this radiation, matching vanilla's details panel: vanilla's linear kill
        // takes count * 0.5 / (rate * rads) seconds, the exponential model takes Half-Kill Radiation cycles divided by
        // the radiation, and the faster of the two wins.
        public static float HalfLife(int count, float rads, float vanillaRate)
        {
            if (IsInstant(rads))
            {
                return 0f;
            }
            float proportional = (float)(Options.HalfKillRads * SecondsPerCycle / rads);
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
