using KMod;

namespace CheapWiresAndPipes
{
    // Mod entry point. The base OnLoad applies every [HarmonyPatch] class in this assembly.
    public class ModLoad : UserMod2
    {
        public static void Log(string msg)
        {
            Debug.Log("[CheapWiresAndPipes] " + msg);
        }
    }
}
