using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using UnityEngine;

public class Patch : UserMod2
{
    public static void Log(string msg)
    {
        Debug.Log("[TrueInsulation] " + msg);
    }
    
    public static List<Type> GetBuildingConfigTypes()
    {
        var targetBuildingConfigTypes = new List<Type>
        {
            typeof(InsulatedGasConduitConfig),
            typeof(InsulatedLiquidConduitConfig),
            typeof(InsulationTileConfig),
            typeof(PressureDoorConfig),
            typeof(ManualPressureDoorConfig)
        };

        return targetBuildingConfigTypes;
    }

    public override void OnLoad(Harmony harmony)
    {
        Patch.Log("Loaded");
        harmony.PatchAll();
        var targetBuildingConfigTypes = GetBuildingConfigTypes();

        // apply patches
        foreach (var buildingConfigType in targetBuildingConfigTypes)
        {
            try
            {
                var method = buildingConfigType.GetMethod("CreateBuildingDef", Type.EmptyTypes);
                if (method != null)
                {
                    Patch.Log("Patching " + buildingConfigType.Name);
                    var postfix =
                        new HarmonyMethod(
                            typeof(Patch).GetMethod("Insulate"));
                    harmony.Patch(method, null, postfix);
                }

                Patch.Log("Ignored " + buildingConfigType.Name + ": no `CreateBuildingDef` method");
            }
            catch (Exception e)
            {
                Patch.Log("Failed to patch " + buildingConfigType.Name + ": " + e.Message);
            }
        }

        Patch.Log("Complete");
    }
    
    public static void Insulate(BuildingDef __result)
    {
        __result.ThermalConductivity = 0f;
    }

    [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
    public class InsulatedDoor_BuildingComplete_OnSpawn
    {
        public static void Postfix(ref BuildingComplete __instance)
        {

            if (string.Compare(__instance.name, "ManualPressureDoorComplete") == 0)
            {
                __instance.gameObject.AddOrGet<InsulatorModified>();
            }
            if (string.Compare(__instance.name, "PressureDoorComplete") == 0)
            {
                __instance.gameObject.AddOrGet<InsulatorModified>();
            }
            
            InsulatorModified insulator = __instance.gameObject.GetComponent<InsulatorModified>();
            
            if (insulator != null)
            {
                insulator.SetInsulation(__instance.gameObject, insulator.building.Def.ThermalConductivity);
            }
        }
    }
}