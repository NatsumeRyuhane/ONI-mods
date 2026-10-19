using HarmonyLib;
using PeterHan.PLib.Options;
using STRINGS;
using UnityEngine;

namespace SublimationSconce
{
    public class Patch
    {
        // Db.Initialize runs before the game registers buildings (LegacyModMain.Load) and before Techs.PostProcess,
        // so strings, the build menu entry and the research unlock are all in place in time.
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Config config = SingletonOptions<Config>.Instance;
                string key = "STRINGS.BUILDINGS.PREFABS." + SublimationSconceConfig.ID.ToUpperInvariant();
                Strings.Add(key + ".NAME", UI.FormatAsLink("Sublimation Sconce", SublimationSconceConfig.ID));
                Strings.Add(key + ".DESC", "A finely tuned chamber that keeps a low-pressure draft over its contents, "
                    + "coaxing any sublimating material to give off gas as readily as "
                    + UI.FormatAsLink("Oxylite", "OXYROCK") + " does.");
                Strings.Add(key + ".EFFECT", "Holds up to " + GameUtil.GetFormattedMass(config.CapacityKg)
                    + " of a chosen material that sublimates, such as " + UI.FormatAsLink("Slime", "SLIMEMOLD") + ", "
                    + UI.FormatAsLink("Bleach Stone", "BLEACHSTONE") + " or "
                    + UI.FormatAsLink("Polluted Dirt", "TOXICSAND") + ", and lets it release its gas"
                    + (config.MatchOxyliteRate
                        ? " as fast as " + UI.FormatAsLink("Oxylite", "OXYROCK") + " would."
                        : ".")
                    + "\n\nStops while the gas around it is too dense.");
                Strings.Add(SublimationSconceConfig.FilterHeaderKey, "Accepted Materials");
                ModUtil.AddBuildingToPlanScreen("Oxygen", SublimationSconceConfig.ID, "producers",
                    OxysconceConfig.ID);
                Db.Get().Techs.TryGet("PortableGasses")?.unlockedItemIDs.Add(SublimationSconceConfig.ID);
            }
        }

        // Items in a Sublimation Sconce sublimate with Oxylite's rate, minimum emission and mass scaling, unless their
        // own rate is already faster. Sublimates reads its info only inside Sim200ms, so the prefix swaps it in and the
        // finalizer puts the item's own info back, even if the tick throws. The cap on the gas around the item and
        // the gas it gives off stay the item's own.
        [HarmonyPatch(typeof(Sublimates), nameof(Sublimates.Sim200ms))]
        public static class Sublimates_Sim200ms_Patch
        {
            private static bool oxyliteLoaded;

            private static Sublimates.Info oxylite;

            public static bool Prepare()
            {
                return SingletonOptions<Config>.Instance.MatchOxyliteRate;
            }

            public static void Prefix(Sublimates __instance, out Sublimates.Info? __state)
            {
                __state = null;
                Pickupable pickupable = __instance.GetComponent<Pickupable>();
                Storage storage = pickupable != null ? pickupable.storage : null;
                if (storage == null || storage.GetComponent<SublimationSconce>() == null || !LoadOxylite())
                {
                    return;
                }
                Sublimates.Info info = __instance.info;
                float mass = pickupable.PrimaryElement.Mass;
                if (Rate(info, mass) >= Rate(oxylite, mass))
                {
                    return;
                }
                __state = info;
                info.sublimationRate = oxylite.sublimationRate;
                info.minSublimationAmount = oxylite.minSublimationAmount;
                info.massPower = oxylite.massPower;
                __instance.info = info;
            }

            public static void Finalizer(Sublimates __instance, Sublimates.Info? __state)
            {
                if (__state.HasValue)
                {
                    __instance.info = __state.Value;
                }
            }

            // Sublimation per second at a mass, as Sublimates.Sim200ms works it out.
            private static float Rate(Sublimates.Info info, float mass)
            {
                return Mathf.Max(info.sublimationRate, info.sublimationRate * Mathf.Pow(mass, info.massPower));
            }

            private static bool LoadOxylite()
            {
                if (!oxyliteLoaded)
                {
                    GameObject prefab = Assets.TryGetPrefab(SimHashes.OxyRock.CreateTag());
                    Sublimates sublimates = prefab != null ? prefab.GetComponent<Sublimates>() : null;
                    if (sublimates == null)
                    {
                        return false;
                    }
                    oxylite = sublimates.info;
                    oxyliteLoaded = true;
                }
                return true;
            }
        }
    }
}
