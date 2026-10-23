using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace FoodGenerator
{
    public class ModLoad : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            // Applies every [HarmonyPatch] class in this assembly.
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(Config));
        }
    }
}
