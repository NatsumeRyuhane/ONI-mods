using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterRadiationLamp
{
    public class Patch
    {
        private const short OVERRIDE_RADIUS = 16;
        
        [HarmonyPatch(typeof(RadiationLightConfig), nameof(RadiationLightConfig.CreateBuildingDef))]
        public class RadiationLightConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = 10f;
                __result.PermittedRotations = PermittedRotations.R360;
                __result.BuildLocationRule = BuildLocationRule.Anywhere;
                __result.ContinuouslyCheckFoundation = false;
            }
        }
        
        [HarmonyPatch(typeof(RadiationLightConfig), nameof(RadiationLightConfig.ConfigureBuildingTemplate))]
        public class RadiationLightConfig_ConfigureBuildingTemplate_Patch
        {
            public static Tag getFuelElement()
            {
                FieldInfo fuelElementField =
                    typeof(RadiationLightConfig).GetField("FUEL_ELEMENT", BindingFlags.NonPublic | BindingFlags.Static);
                if (fuelElementField != null)
                {
                    return (Tag)fuelElementField.GetValue(null);
                }
                else
                {
                    return SimHashes.UraniumOre.CreateTag();
                }
            }
            
            public static SimHashes getWasteElement()
            {
                FieldInfo wasteElementField =
                    typeof(RadiationLightConfig).GetField("WASTE_ELEMENT", BindingFlags.NonPublic | BindingFlags.Static);
                if (wasteElementField != null)
                {
                    return (SimHashes)wasteElementField.GetValue(null);
                }
                else
                {
                    return SimHashes.DepletedUranium;
                }
            }

            public static void Postfix(GameObject go, RadiationLightConfig __instance)
            {
                const float consumptionRateOverride = 0.001f;
                const float conversionRateOverride = 0.9f;
                
                ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
                elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
                {
                    new ElementConverter.ConsumedElement(getFuelElement(), consumptionRateOverride)
                };
                elementConverter.outputElements = new ElementConverter.OutputElement[1]
                {
                    new ElementConverter.OutputElement( consumptionRateOverride*conversionRateOverride, getWasteElement(), 0.0f, storeOutput: true, diseaseWeight: 0.5f)
                };
                RadiationLight radiationLight = go.AddOrGet<RadiationLight>();
                radiationLight.consumptionRate = consumptionRateOverride;
            }
        }
        
        [HarmonyPatch(typeof(RadiationLight.StatesInstance), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(RadiationLight) })]
        public class RadiationLight_StatesInstance_StateInstance_Patch
        {
            public enum Facing
            {
                UP,
                RIGHT,
                DOWN,
                LEFT
            }

            public static void RotateRadiationEmission(RadiationEmitter re, Facing facing)
            {
                re.emitRads = 500f;
                re.radiusProportionalToRads = false;
                re.emitType = RadiationEmitter.RadiationEmitterType.Constant;
                switch (facing)
                {
                    case Facing.UP:
                    case Facing.DOWN:
                        re.emitAngle = 180f;
                        // 1 (center) + 2*(left/right)
                        re.emitRadiusX = 3;
                        // (how many tiles tall)
                        re.emitRadiusY = 3;
                        break;
                    case Facing.LEFT:
                    case Facing.RIGHT:
                    default:
                        re.emitAngle = 180f;
                        // (how many tiles wide)
                        re.emitRadiusX = 16;
                        // 1 (center) + 2*(up/down)
                        re.emitRadiusY = 2;
                        break;
                }
                // Set direction and offset as before
                switch (facing)
                {
                    case Facing.UP:
                        re.emitDirection = 90f;
                        re.emissionOffset = Vector3.up;
                        break;
                    case Facing.RIGHT:
                        re.emitDirection = 0f;
                        re.emissionOffset = Vector3.right;
                        break;
                    case Facing.DOWN:
                        re.emitDirection = 270f;
                        re.emissionOffset = Vector3.down;
                        break;
                    case Facing.LEFT:
                        re.emitDirection = 180f;
                        re.emissionOffset = Vector3.left;
                        break;
                }
            }
            
            public static void Postfix(RadiationLight smi, RadiationLight.StatesInstance __instance)
            {
                RadiationEmitter component = __instance.master.GetComponent<RadiationEmitter>();
                switch (__instance.GetComponent<Rotatable>().GetOrientation()) 
                {
                    case Orientation.Neutral:
                        RotateRadiationEmission(component, Facing.RIGHT);
                        break;
                    case Orientation.R90:
                        RotateRadiationEmission(component, Facing.DOWN);
                        break;
                    case Orientation.R180:
                        RotateRadiationEmission(component, Facing.LEFT);
                        break;
                    case Orientation.R270:
                        RotateRadiationEmission(component, Facing.UP);
                        break;
                    case Orientation.FlipH:
                        RotateRadiationEmission(component, Facing.LEFT);
                        break;
                    case Orientation.FlipV:
                        RotateRadiationEmission(component, Facing.RIGHT);
                        break;
                }
            }
        }
    }
}