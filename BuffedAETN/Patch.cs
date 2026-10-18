using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BuffedAETN
{
    // Patches the Anti Entropy Thermo-Nullifier (AETN, MassiveHeatSinkConfig). Vanilla removes 80 kW (64 kW self,
    // 16 kW exhaust), stops below 100 K, burns 10 g/s of Hydrogen, buffers 100 g of it and can be flooded. Here each
    // AETN gets its own cooling power and minimum temperature (see AdjustableAetn).
    public static class Patch
    {
        // The def is shared by every AETN, so its heat values stay at vanilla and the game's structure temperature
        // code reads each building's own values instead.
        private static BuildingDef aetnDef;

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Strings.Add(AdjustableAetn.TitleKeyString, "AETN Settings");
                Strings.Add(AdjustableAetn.CoolingTitleKeyString, "Cooling Power");
                Strings.Add(AdjustableAetn.CoolingTooltipKeyString,
                    "Removes <b>{0:F0} kW</b> of heat while active, 80% from itself and 20% from the cells it covers."
                    + "\n\nBurns <b>{1}</b> of Hydrogen and buffers {2}."
                    + "\n\nThe minimum temperature can't go below <b>{3}</b> at this power.");
                Strings.Add(AdjustableAetn.MinTemperatureTitleKeyString, "Minimum Temperature");
                Strings.Add(AdjustableAetn.MinTemperatureTooltipKeyString,
                    "Stops when it or a cell it covers is colder than <b>{0}</b>."
                    + "\n\nThe lowest setting is {1} at {2:F0} kW. More cooling power raises it, so the AETN stops"
                    + " before it can cool anything below absolute zero.");
            }
        }

        [HarmonyPatch(typeof(MassiveHeatSinkConfig), nameof(MassiveHeatSinkConfig.CreateBuildingDef))]
        public static class MassiveHeatSinkConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.Floodable = false;
                aetnDef = __result;
            }
        }

        [HarmonyPatch(typeof(MassiveHeatSinkConfig), nameof(MassiveHeatSinkConfig.DoPostConfigureComplete))]
        public static class MassiveHeatSinkConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<CopyBuildingSettings>();
                go.AddOrGet<AdjustableAetn>();
            }
        }

        // Swaps the self heat and exhaust getters for ones that use an AETN's own cooling power. A transpiler on the
        // callers instead of a patch on the getters, since the JIT can inline such small getters.
        [HarmonyPatch]
        public static class StructureTemperatureComponents_Heat_Patch
        {
            private static readonly MethodInfo ExhaustGetter =
                AccessTools.PropertyGetter(typeof(StructureTemperaturePayload),
                    nameof(StructureTemperaturePayload.ExhaustKilowatts));

            private static readonly MethodInfo OperatingGetter =
                AccessTools.PropertyGetter(typeof(StructureTemperaturePayload),
                    nameof(StructureTemperaturePayload.OperatingKilowatts));

            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(StructureTemperatureComponents),
                    nameof(StructureTemperatureComponents.Sim200ms));
                yield return AccessTools.Method(typeof(StructureTemperatureComponents), "UpdateSimState");
                yield return AccessTools.Method(typeof(StructureTemperatureComponents), "SimRegister");
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                MethodBase original)
            {
                int replaced = 0;
                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.Calls(ExhaustGetter))
                    {
                        instruction.operand = AccessTools.Method(typeof(StructureTemperatureComponents_Heat_Patch),
                            nameof(ExhaustKilowatts));
                        replaced++;
                    }
                    else if (instruction.Calls(OperatingGetter))
                    {
                        instruction.operand = AccessTools.Method(typeof(StructureTemperatureComponents_Heat_Patch),
                            nameof(OperatingKilowatts));
                        replaced++;
                    }
                    yield return instruction;
                }
                if (replaced == 0)
                {
                    Debug.LogWarning("[BuffedAETN] No heat getters found in " + original.Name
                        + ", AETN cooling stays at vanilla there");
                }
            }

            // Same stack shape as the getters: the payload's address in, kW out.
            public static float ExhaustKilowatts(ref StructureTemperaturePayload payload)
            {
                AdjustableAetn aetn = Find(payload.building);
                return aetn != null ? aetn.ExhaustKw : payload.ExhaustKilowatts;
            }

            public static float OperatingKilowatts(ref StructureTemperaturePayload payload)
            {
                AdjustableAetn aetn = Find(payload.building);
                if (aetn == null)
                {
                    return payload.OperatingKilowatts;
                }
                return payload.operational != null && payload.operational.IsActive ? aetn.SelfHeatKw : 0f;
            }

            private static AdjustableAetn Find(Building building)
            {
                if (building == null || aetnDef == null || !ReferenceEquals(building.Def, aetnDef))
                {
                    return null;
                }
                return building.GetComponent<AdjustableAetn>();
            }
        }
    }
}
