using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalThings
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethalthings";
        public const string ModName = "LethalThings";
        public const string ModVersion = "0.1.0";

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;

            Content.Load();
            Patches.Patches.Load();

            Logger.LogInfo("LethalThings loaded");
        }
    }
}