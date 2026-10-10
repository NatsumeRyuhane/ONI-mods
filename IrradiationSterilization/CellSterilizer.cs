namespace IrradiationSterilization
{
    // Germs in cells live in the native sim, whose radiation kill is the vanilla fixed count and can't be patched.
    // Once a second, this removes the germs the exponential model kills beyond that count. It assumes the sim kills
    // radiationKillRate * rads germs per second, the rate vanilla's details panel shows.
    public class CellSterilizer : KMonoBehaviour, ISim1000ms
    {
        public void Sim1000ms(float dt)
        {
            if (!Sim.IsRadiationEnabled())
            {
                return;
            }
            var diseases = Db.Get().Diseases;
            for (int cell = 0; cell < Grid.CellCount; cell++)
            {
                float rads = Grid.Radiation[cell];
                if (rads <= 0f)
                {
                    continue;
                }
                byte diseaseIdx = Grid.DiseaseIdx[cell];
                if (diseaseIdx == byte.MaxValue)
                {
                    continue;
                }
                int count = Grid.DiseaseCount[cell];
                if (count <= 0)
                {
                    continue;
                }
                var disease = diseases[diseaseIdx];
                if (!Sterilization.Affects(disease))
                {
                    continue;
                }

                int kill;
                if (Sterilization.IsInstant(rads))
                {
                    kill = count;
                }
                else
                {
                    float vanillaKill = disease.radiationKillRate * rads * dt;
                    kill = Sterilization.RoundRandomly(Sterilization.ProportionalKill(disease, count, rads, dt) - vanillaKill);
                }
                if (kill > 0)
                {
                    SimMessages.ModifyDiseaseOnCell(cell, diseaseIdx, -System.Math.Min(kill, count));
                }
            }
        }
    }
}
