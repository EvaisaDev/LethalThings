using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;
using BepInEx.Configuration;
using LethalThings.MonoBehaviours;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalThings
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethalthings";
        public const string ModName = "LethalThings";
        public const string ModVersion = "0.3.0";

        public static ManualLogSource logger;
        public static ConfigFile config;

        private void Awake()
        {
            logger = Logger;
            config = Config;

            Content.Load();
            Patches.Patches.Load();

            Logger.LogInfo("LethalThings loaded guh");
        }
    }
}