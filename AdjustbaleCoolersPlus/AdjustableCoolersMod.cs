using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace AdjustableCoolersPlus
{
    public class AdjustableCoolersMod : UserMod2
    {

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            LocString.CreateLocStringKeys(typeof(UI));
            PUtil.InitLibrary();
            new POptions().RegisterOptions((UserMod2) this, typeof(Config));
        }

    }
}
