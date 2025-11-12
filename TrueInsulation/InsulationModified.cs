using System.Collections.Generic;
using UnityEngine;

public class InsulatorModified : KMonoBehaviour
{
    [MyCmpReq]
    public Building building;

    protected override void OnSpawn()
    {
        foreach(var cell in building.PlacementCells)
        {
            SimMessages.SetInsulation(cell, building.Def.ThermalConductivity);
        }
    }

    protected override void OnCleanUp()
    {
        foreach (var cell in building.PlacementCells)
        {
            SimMessages.SetInsulation(cell, 1);
        }
    }
    
    public void SetInsulation(GameObject go, float insulation)
    {
        IList<int> cells = go.GetComponent<Building>().PlacementCells;
        foreach (var c in cells)
        {
            SimMessages.SetInsulation(c, insulation);
        }
    }
}