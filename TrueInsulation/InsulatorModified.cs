namespace TrueInsulation
{
    // Sets the sim's insulation for every cell a building occupies to the building's ThermalConductivity (0 for the doors
    // this mod patches), and puts it back to 1 when the building is removed. Pressure doors keep Door.insulationModifier at
    // 1, so Door never writes their cells' insulation and this value stays in place whether the door is open or closed.
    // The component has no saved state, so nothing is lost when an older save stored it under its previous type name.
    public class InsulatorModified : KMonoBehaviour
    {
        // The game assigns [MyCmpReq] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpReq]
        private Building building;
#pragma warning restore CS0649

        protected override void OnSpawn()
        {
            base.OnSpawn();
            ApplyInsulation();
        }

        protected override void OnCleanUp()
        {
            foreach (var cell in building.PlacementCells)
            {
                SimMessages.SetInsulation(cell, 1f);
            }

            base.OnCleanUp();
        }

        public void ApplyInsulation()
        {
            foreach (var cell in building.PlacementCells)
            {
                SimMessages.SetInsulation(cell, building.Def.ThermalConductivity);
            }
        }
    }
}
