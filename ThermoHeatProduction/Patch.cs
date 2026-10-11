using HarmonyLib;

namespace ThermoHeatProduction
{
    public static class Patch
    {
        // Thermo Regulators and Thermo Aquatuners (both AirConditioner) already report the heat they move out of their
        // pipe contents through StructureTemperatureComponents.ProduceEnergy, under "Transferred from pipes", the same
        // way the Steam Turbine reports its steam heat. Vanilla never shows it: StructureTemperatureComponents only sets
        // its "Heat Production" status item on buildings with self heat or exhaust heat, and both are 0 kW on these.
        // Flipping that flag would also add a 0 kW "Normal operation" line to the tooltip, so this postfix sets the
        // status item itself, while the building is active, the same way StructureTemperatureComponents.Sim200ms does.
        //
        // UpdateStatus is private and named by string. It runs on every Sim200ms while the building is operational and
        // whenever it turns on or off.
        [HarmonyPatch(typeof(AirConditioner), "UpdateStatus")]
        public static class AirConditioner_UpdateStatus_Patch
        {
            // Private, named by string. StructureTemperatureComponents creates it when the first building with a
            // structure temperature is added, so it exists before any AirConditioner spawns.
            private static readonly AccessTools.FieldRef<StructureTemperatureComponents, StatusItem> OperatingEnergyStatusItem =
                AccessTools.FieldRefAccess<StructureTemperatureComponents, StatusItem>("operatingEnergyStatusItem");

            public static void Postfix(
                Operational ___operational,
                KSelectable ___selectable,
                HandleVector<int>.Handle ___structureTemperature)
            {
                StatusItemCategory category = Db.Get().StatusItemCategories.OperatingEnergy;
                StatusItem item = OperatingEnergyStatusItem(GameComps.StructureTemperatures);
                // The handle is set in AirConditioner.OnSpawn, but an active change can call UpdateStatus earlier.
                // The status item looks up the building by its sim handle, which is only valid once the sim has
                // registered the building. Until then, UpdateStatus runs again on the next Sim200ms.
                int simHandle = ___structureTemperature.IsValid()
                    ? GameComps.StructureTemperatures.GetPayload(___structureTemperature).simHandleCopy
                    : -1;
                if (___operational.IsActive && item != null && Sim.IsValidHandle(simHandle))
                {
                    // SetStatusItem does nothing if the item is already set in this category.
                    ___selectable.SetStatusItem(category, item, simHandle);
                }
                else
                {
                    ___selectable.SetStatusItem(category, null);
                }
            }
        }
    }
}
