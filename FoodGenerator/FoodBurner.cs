using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FoodGenerator
{
    // The order queue: a fabricator no Duplicant works, with one recipe per food. The fabricator fetches the food for
    // the queued orders and moves an order's food into its build storage when the order starts, as it does for the
    // Kiln. FoodGenerator then burns the order in progress through Burn. Patch.ComplexFabricator_Sim200ms_Patch
    // stops the fabricator from running the order on its own timer and from setting the building active, which
    // FoodGenerator decides. While FetchPaused is set, Patch.ComplexFabricator_GetRecipePrefetchCount_Patch asks for
    // no food, like a Coal Generator's paused fuel delivery.
    public class FoodBurner : ComplexFabricator
    {
        public const string BurningStatusId = "FoodGeneratorBurning";

        // Float sums can stop just short of 1, which would leave an order that never finishes.
        private const float DoneProgress = 0.99999f;

        private static readonly MethodInfo ShowProgressBarMethod =
            AccessTools.Method(typeof(ComplexFabricator), "ShowProgressBar");

        private static StatusItem burningStatusItem;

        private bool fetchPaused;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            // The vanilla working status names the first result, which here is Carbon Dioxide. Name the food.
            if (burningStatusItem == null)
            {
                burningStatusItem = new StatusItem(BurningStatusId, "BUILDING", "", StatusItem.IconType.Info,
                    NotificationType.Neutral, false, OverlayModes.None.ID);
                burningStatusItem.resolveStringCallback = ResolveBurning;
                burningStatusItem.resolveTooltipCallback = ResolveBurning;
            }
            workingStatusItem = burningStatusItem;
        }

        // Set by FoodGenerator while every battery on the circuit is at or above its Fuel Request Threshold. An order
        // whose food is already in still starts.
        public bool FetchPaused
        {
            get { return fetchPaused; }
            set
            {
                if (fetchPaused != value)
                {
                    fetchPaused = value;
                    // The fabricator plans its fetches again on its next Sim1000ms, cancelling them when paused.
                    SetQueueDirty();
                }
            }
        }

        // Burns up to a share of the order in progress, never more than is left of it, and returns the share burned.
        // Finishing the order consumes its food, counts it down in the queue and starts the next one.
        public float Burn(float share)
        {
            if (CurrentWorkingOrder == null)
            {
                return 0f;
            }
            float burned = Mathf.Clamp(share, 0f, Mathf.Max(0f, 1f - OrderProgress));
            OrderProgress += burned;
            if (OrderProgress >= DoneProgress)
            {
                // What ComplexFabricator.Sim200ms does when an order finishes.
                ShowProgressBarMethod.Invoke(this, new object[] { false });
                CompleteWorkingOrder();
            }
            return burned;
        }

        private static string ResolveBurning(string text, object data)
        {
            FoodBurner burner = data as FoodBurner;
            ComplexRecipe order = burner != null ? burner.CurrentWorkingOrder : null;
            return order != null ? text.Replace("{Item}", order.ingredients[0].material.ProperName()) : text;
        }
    }
}
