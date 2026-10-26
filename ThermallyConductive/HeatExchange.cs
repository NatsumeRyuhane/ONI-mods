using Klei.AI;
using STRINGS;

namespace ThermallyConductive
{
    // The Heat Exchange Efficiency building attribute. It multiplies a building's BuildingDef.ThermalConductivity
    // wherever the game hands that value to the heat sim, so the bonus lives on each building as a named modifier
    // instead of in the shared def.
    public static class HeatExchange
    {
        public const string AttributeId = "HeatExchangeEfficiency";

        // Attribute reads its name and description from this key when it is constructed.
        private const string StringsKey = "STRINGS.DUPLICANTS.ATTRIBUTES.HEATEXCHANGEEFFICIENCY";

        public static Attribute Efficiency { get; private set; }

        // Base 0, so a building without the attribute keeps its vanilla conductivity and one with it gets exactly the
        // sum of its modifiers. Display.Details lists it in the Properties tab, under the element's thermal values.
        public static void Register(Db db)
        {
            Strings.Add(StringsKey + ".NAME", "Heat Exchange Efficiency");
            Strings.Add(StringsKey + ".DESC",
                "How quickly this building exchanges " + UI.PRE_KEYWORD + "Heat" + UI.PST_KEYWORD
                + " with its surroundings.");
            Efficiency = db.BuildingAttributes.Add(new Attribute(AttributeId, false, Attribute.Display.Details, false));
            Efficiency.SetFormatter(new MultiplierFormatter());
        }

        public static AttributeModifier Modifier(float multiplier, string label)
        {
            // TimeSlice.None also stops GetFormattedString from adding a "+" in front of the "x".
            return new AttributeModifier(AttributeId, multiplier, label)
            {
                OverrideTimeSlice = GameUtil.TimeSlice.None
            };
        }

        public static float Scale(float conductivity, Building building)
        {
            if (building == null || Efficiency == null) return conductivity;
            AttributeInstance instance = building.GetAttributes()?.Get(Efficiency);
            return instance == null ? conductivity : conductivity * instance.GetTotalValue();
        }

        public static float Scale(float conductivity, ref StructureTemperaturePayload payload)
        {
            return Scale(conductivity, payload.building);
        }

        public static float Scale(float conductivity, HandleVector<int>.Handle structureTemperatureHandle)
        {
            return Scale(conductivity, GameComps.StructureTemperatures.GetPayload(structureTemperatureHandle).building);
        }

        // Shows values as "x1,000", the way the game writes multipliers elsewhere.
        private class MultiplierFormatter : StandardAttributeFormatter
        {
            public MultiplierFormatter() : base(GameUtil.UnitClass.SimpleFloat, GameUtil.TimeSlice.None)
            {
            }

            public override string GetFormattedValue(float value, GameUtil.TimeSlice timeSlice = GameUtil.TimeSlice.None)
            {
                return "x" + GameUtil.GetFormattedSimple(value);
            }
        }
    }
}
