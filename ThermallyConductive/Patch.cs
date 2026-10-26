using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ThermallyConductive
{
    public static class Patch
    {
        // Heat Exchange Efficiency each building gets. It multiplies the building's vanilla ThermalConductivity.
        private const float Multiplier = 1000f;

        // Buildings whose job is to move heat, with the modifier label shown in the attribute's tooltip. Vanilla
        // ThermalConductivity: radiant pipes and Conduction Panels 2, Temperature Shift Plates 1.
        private static readonly Dictionary<Type, string> Labels = new Dictionary<Type, string>
        {
            { typeof(ThermalBlockConfig), "Vapor Chamber Core" },
            { typeof(LiquidConduitRadiantConfig), "Finned Tubing" },
            { typeof(GasConduitRadiantConfig), "Finned Tubing" },
            { typeof(ContactConductivePipeBridgeConfig), "Thermal Interface Pads" }
        };

        private static readonly FieldInfo ConductivityField =
            AccessTools.Field(typeof(BuildingDef), nameof(BuildingDef.ThermalConductivity));

        private static readonly MethodInfo BuildingScale = AccessTools.Method(typeof(HeatExchange),
            nameof(HeatExchange.Scale), new[] { typeof(float), typeof(Building) });

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix(Db __instance)
            {
                HeatExchange.Register(__instance);
            }
        }

        // BuildingComplete.OnPrefabInit adds def.attributeModifiers to each building before its structure temperature
        // registers with the sim, so the first registration already sees the bonus.
        [HarmonyPatch]
        public static class CreateBuildingDef_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return Labels.Keys.Select(type => (MethodBase)AccessTools.Method(type, nameof(IBuildingConfig.CreateBuildingDef), Type.EmptyTypes));
            }

            public static void Postfix(IBuildingConfig __instance, BuildingDef __result)
            {
                __result.attributeModifiers.Add(HeatExchange.Modifier(Multiplier, Labels[__instance.GetType()]));
            }
        }

        // Building <-> cell exchange. SimRegister sends the conductivity when the building registers with the sim,
        // UpdateSimState whenever its temperature state changes.
        [HarmonyPatch]
        public static class StructureTemperatureComponents_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(StructureTemperatureComponents), "SimRegister");
                yield return AccessTools.Method(typeof(StructureTemperatureComponents), "UpdateSimState");
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                MethodInfo scale = AccessTools.Method(typeof(HeatExchange), nameof(HeatExchange.Scale),
                    new[] { typeof(float), typeof(StructureTemperaturePayload).MakeByRefType() });
                return ScaleConductivity(instructions, original, () => new[]
                {
                    LoadArgument(original, "payload"),
                    new CodeInstruction(OpCodes.Call, scale)
                });
            }
        }

        // Pipe contents <-> pipe exchange, the part of a radiant pipe that matters most. Also scales the "insulated"
        // check next to it, which stays false for anything the mod boosts.
        [HarmonyPatch(typeof(ConduitTemperatureManager), nameof(ConduitTemperatureManager.Allocate))]
        public static class ConduitTemperatureManager_Allocate_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                MethodInfo scale = AccessTools.Method(typeof(HeatExchange), nameof(HeatExchange.Scale),
                    new[] { typeof(float), typeof(HandleVector<int>.Handle) });
                return ScaleConductivity(instructions, original, () => new[]
                {
                    LoadArgument(original, "conduit_structure_temperature_handle"),
                    new CodeInstruction(OpCodes.Call, scale)
                });
            }
        }

        // Liquid <-> panel exchange in the Conduction Panel, which the panel works out in managed code.
        [HarmonyPatch(typeof(ContactConductivePipeBridge.Instance), "ExchangeStorageTemperatureWithBuilding")]
        public static class ContactConductivePipeBridge_Exchange_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                FieldInfo building = AccessTools.Field(typeof(ContactConductivePipeBridge.Instance),
                    nameof(ContactConductivePipeBridge.Instance.building));
                return ScaleConductivity(instructions, original, () => new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, building),
                    new CodeInstruction(OpCodes.Call, BuildingScale)
                });
            }
        }

        // The Thermal Conductivity line in the Properties tab, so it shows the rate the sim actually uses. The panel
        // reads building.Def.ThermalConductivity from a local Building, so that local is loaded again.
        [HarmonyPatch(typeof(AdditionalDetailsPanel), "RefreshDetailsPanel")]
        public static class AdditionalDetailsPanel_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                FieldInfo defField = AccessTools.Field(typeof(Building), nameof(Building.Def));
                List<CodeInstruction> codes = instructions.ToList();
                int scaled = 0;
                for (int i = 2; i < codes.Count; i++)
                {
                    if (!codes[i].LoadsField(ConductivityField) || !codes[i - 1].LoadsField(defField) || !codes[i - 2].IsLdloc())
                    {
                        continue;
                    }
                    codes.InsertRange(i + 1, new[]
                    {
                        new CodeInstruction(codes[i - 2].opcode, codes[i - 2].operand),
                        new CodeInstruction(OpCodes.Call, BuildingScale)
                    });
                    scaled++;
                }
                WarnIfNone(scaled, original);
                return codes;
            }
        }

        // Follows every load of BuildingDef.ThermalConductivity with instructions that push the building (or
        // something that leads to it) and call a HeatExchange.Scale overload, which leaves the scaled float on the
        // stack in place of the vanilla one.
        private static IEnumerable<CodeInstruction> ScaleConductivity(IEnumerable<CodeInstruction> instructions,
            MethodBase original, Func<CodeInstruction[]> scale)
        {
            int scaled = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (!instruction.LoadsField(ConductivityField)) continue;
                foreach (CodeInstruction added in scale()) yield return added;
                scaled++;
            }
            WarnIfNone(scaled, original);
        }

        private static CodeInstruction LoadArgument(MethodBase method, string name)
        {
            int index = Array.FindIndex(method.GetParameters(), p => p.Name == name);
            if (index < 0) throw new MissingMemberException(method.DeclaringType?.Name + "." + method.Name, name);
            return CodeInstruction.LoadArgument(method.IsStatic ? index : index + 1);
        }

        private static void WarnIfNone(int scaled, MethodBase original)
        {
            if (scaled == 0)
            {
                Debug.LogWarning("[ThermallyConductive] No ThermalConductivity read found in "
                    + original.DeclaringType?.Name + "." + original.Name + ", it stays at vanilla there");
            }
        }
    }
}
