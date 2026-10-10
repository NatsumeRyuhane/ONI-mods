using System;
using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace IrradiationSterilization
{
    // Makes radiation kill a share of the germs each cycle, set by each germ's D50 (see Sterilization), with vanilla's
    // fixed-count kill as a floor. Vanilla kill rates (TUNING.DISEASE.RADIATION_KILL_RATE, germs per rad per second):
    // Food Poisoning and Slimelung 2.5, Zombie Spores 1, Floral Scent 0 (immune). Vanilla radiation never touches germs
    // in pipes. Radiation only exists with Spaced Out.
    public static class Patch
    {
        // Germs on items and buildings. Vanilla subtracts rads * radiationKillRate on every call, not scaled by dt, so
        // in the 200 ms tick it kills five times what the details panel shows. That term stays as the floor.
        [HarmonyPatch(typeof(DiseaseContainers), nameof(DiseaseContainers.CalculateDelta),
            typeof(int), typeof(ushort), typeof(float), typeof(int), typeof(float), typeof(float), typeof(Disease),
            typeof(float), typeof(bool))]
        public static class DiseaseContainers_CalculateDelta_Patch
        {
            public static void Postfix(int disease_count, int environment_cell, Disease disease, float dt,
                bool radiation_enabled, ref float __result)
            {
                if (!radiation_enabled || disease_count <= 0 || !Grid.IsValidCell(environment_cell))
                {
                    return;
                }
                float rads = Grid.Radiation[environment_cell];
                if (rads <= 0f || !Sterilization.Affects(disease))
                {
                    return;
                }
                float vanillaKill = rads * disease.radiationKillRate;
                float kill = Sterilization.IsInstant(rads)
                    ? disease_count
                    : Math.Max(vanillaKill, Sterilization.ProportionalKill(disease, disease_count, rads, dt));
                __result += vanillaKill - kill;
            }
        }

        // Germs in gas and liquid pipes. ConduitDiseaseManager holds the live counts and ticks every 200 ms, just
        // before the conduit flows read them. Its entries don't know their cell, so walk each flow's conduits, which
        // pair a cell with a germ handle. SOAInfo.diseaseHandles is private, so it's named by string.
        [HarmonyPatch(typeof(ConduitDiseaseManager), nameof(ConduitDiseaseManager.Sim200ms))]
        public static class ConduitDiseaseManager_Sim200ms_Patch
        {
            private static readonly AccessTools.FieldRef<ConduitFlow.SOAInfo, List<HandleVector<int>.Handle>>
                DiseaseHandles =
                    AccessTools.FieldRefAccess<ConduitFlow.SOAInfo, List<HandleVector<int>.Handle>>("diseaseHandles");

            public static void Postfix(ConduitDiseaseManager __instance, float dt)
            {
                if (!Sim.IsRadiationEnabled() || Game.Instance == null)
                {
                    return;
                }
                Sterilize(__instance, Game.Instance.gasConduitFlow, dt);
                Sterilize(__instance, Game.Instance.liquidConduitFlow, dt);
            }

            private static void Sterilize(ConduitDiseaseManager manager, ConduitFlow flow, float dt)
            {
                if (flow == null)
                {
                    return;
                }
                var soaInfo = flow.soaInfo;
                var handles = DiseaseHandles(soaInfo);
                var diseases = Db.Get().Diseases;
                for (int i = 0; i < handles.Count; i++)
                {
                    var handle = handles[i];
                    if (!handle.IsValid())
                    {
                        continue;
                    }
                    float rads = Grid.Radiation[soaInfo.GetCell(i)];
                    if (rads <= 0f)
                    {
                        continue;
                    }
                    var data = manager.GetData(handle);
                    if (data.diseaseIdx == byte.MaxValue || data.diseaseCount <= 0)
                    {
                        continue;
                    }
                    var disease = diseases[data.diseaseIdx];
                    if (!Sterilization.Affects(disease))
                    {
                        continue;
                    }

                    int kill;
                    if (Sterilization.IsInstant(rads))
                    {
                        kill = data.diseaseCount;
                    }
                    else
                    {
                        // Same floor as in cells, although vanilla itself kills nothing in pipes.
                        float vanillaKill = disease.radiationKillRate * rads * dt;
                        float proportional = Sterilization.ProportionalKill(disease, data.diseaseCount, rads, dt);
                        kill = Sterilization.RoundRandomly(Math.Max(vanillaKill, proportional));
                    }
                    if (kill > 0)
                    {
                        manager.ModifyDiseaseCount(handle, -kill);
                    }
                }
            }
        }

        // Germs in cells: adds the component that runs the once-a-second pass. Game.OnPrefabInit is protected, so it's
        // named by string.
        [HarmonyPatch(typeof(Game), "OnPrefabInit")]
        public static class Game_OnPrefabInit_Patch
        {
            public static void Postfix(Game __instance)
            {
                __instance.gameObject.AddOrGet<CellSterilizer>();
            }
        }

        // The germ details panel's Radiation line shows vanilla's linear half-life. Rewrite it with the new one. The
        // other lines, including the total rate of change, come from DiseaseContainers.CalculateDelta, patched above.
        // BuildFactorsStrings and GetFormattedHalfLife are private, so they're named by string.
        [HarmonyPatch(typeof(AdditionalDetailsPanel), "BuildFactorsStrings")]
        public static class AdditionalDetailsPanel_BuildFactorsStrings_Patch
        {
            private static readonly Func<float, string> GetFormattedHalfLife =
                AccessTools.MethodDelegate<Func<float, string>>(
                    AccessTools.Method(typeof(AdditionalDetailsPanel), "GetFormattedHalfLife"));

            public static void Postfix(CollapsibleDetailContentPanel targetPanel, int diseaseCount, int environmentCell,
                Disease disease)
            {
                if (!Sim.IsRadiationEnabled() || !Grid.IsValidCell(environmentCell))
                {
                    return;
                }
                float rads = Grid.Radiation[environmentCell];
                if (rads <= 0f || !Sterilization.Affects(disease))
                {
                    return;
                }
                float halfLife = Sterilization.HalfLife(disease, diseaseCount, rads);
                targetPanel.SetLabel("radiation",
                    string.Format(UI.DETAILTABS.DISEASE.DETAILS.GROWTH_FACTORS.RADIATION.TITLE, Mathf.RoundToInt(rads),
                        GetFormattedHalfLife(halfLife)),
                    UI.DETAILTABS.DISEASE.DETAILS.GROWTH_FACTORS.RADIATION.DIE_TOOLTIP);
            }
        }
    }
}
