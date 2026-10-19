namespace Laser
{
    // Shows the beam in the Light overlay while placing a LASER. The vanilla LightShapePreview has a fixed direction
    // and always uses the default falloff, so this one follows the placement rotation and lights every cell at the
    // default brightness, like the built LASER.
    public class LaserPreview : KMonoBehaviour
    {
#pragma warning disable CS0649
        [MyCmpGet]
        private Rotatable rotatable;
#pragma warning restore CS0649

        private int previousCell = -1;

        private Orientation previousOrientation;

        private void Update()
        {
            int cell = Grid.PosToCell(transform.GetPosition());
            Orientation orientation = rotatable != null ? rotatable.GetOrientation() : Orientation.Neutral;
            if (cell == previousCell && orientation == previousOrientation)
            {
                return;
            }
            previousCell = cell;
            previousOrientation = orientation;
            LightGridManager.DestroyPreview();
            if (!Grid.IsValidCell(cell))
            {
                return;
            }
            int lux = LaserConfig.DefaultLux;
            ListPool<int, LaserPreview>.PooledList cells = ListPool<int, LaserPreview>.Allocate();
            DiscreteShadowCaster.GetVisibleCells(cell, cells, LaserConfig.BeamLength, 1, Laser.GetDirection(orientation),
                LightShape.Quad);
            foreach (int litCell in cells)
            {
                if (Grid.IsValidCell(litCell))
                {
                    LightGridManager.previewLightCells.Add(new Tuple<int, int>(litCell, lux));
                    LightGridManager.previewLux[litCell] = lux;
                }
            }
            cells.Recycle();
        }

        protected override void OnCleanUp()
        {
            LightGridManager.DestroyPreview();
            base.OnCleanUp();
        }
    }
}
