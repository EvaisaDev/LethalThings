using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;
using BepInEx.Configuration;
using LethalThings.MonoBehaviours;
using UnityEngine;
using System.Reflection;
using System;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalThings
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    [BepInDependency(LethalCompanyInputUtils.LethalCompanyInputUtilsPlugin.ModId, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethalthings";
        public const string ModName = "LethalThings";
        public const string ModVersion = "0.8.8";

        public static ManualLogSource logger;
        public static ConfigFile config;

        public static bool devMode = false;


        private void Awake()
        {
            logger = Logger;
            config = Config;

            // check lethallib version
            var version = BepInEx.Bootstrap.Chainloader.PluginInfos[LethalLib.Plugin.ModGUID].Metadata.Version;


            // check if major version is over 0 or minor verison is over 6
            if (version.Major > 0 || version.Minor > 6)
            {
                logger.LogInfo("LethalLib version is " + version.ToString() + ", which is compatible with LethalThings 0.8.0+");
            }
            else
            {
                logger.LogError("LethalLib version is " + version.ToString() + ", which is not compatible with LethalThings 0.8.0+");
                logger.LogError("Please update LethalLib to version 0.7.1 or newer");
                return;
            }

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils"))
            {
                var iuVersion = BepInEx.Bootstrap.Chainloader.PluginInfos["com.rune580.LethalCompanyInputUtils"].Metadata.Version;

                if (iuVersion.Major > 0 || (iuVersion.Minor == 4 && iuVersion.Build >= 3) || iuVersion.Minor > 4)
                {
                    logger.LogInfo("LethalCompanyInputUtils version is " + iuVersion.ToString() + ", which is compatible with LethalThings 0.8.0+");
                }
                else
                {
                    logger.LogError("LethalCompanyInputUtils version is " + iuVersion.ToString() + ", which is not compatible with LethalThings 0.8.0+");
                    logger.LogError("Please update LethalCompanyInputUtils to version 0.4.3 or newer");
                    return;
                }
            }
           

            Utilities.Init();
            LethalThings.Config.Load();
            Content.Load();
            Patches.Patches.Load();

            if (InputCompat.Enabled)
                InputCompat.Init();

            Logger.LogInfo("LethalThings loaded successfully!!!");

            //On.RoundManager.Awake += RoundManager_Awake;
        }

        /*
        static bool first = true;

        private void RoundManager_Awake(On.RoundManager.orig_Awake orig, RoundManager self)
        {
            if (first)
            {
                var dungeon = self.dungeonFlowTypes[0];

                // clone the dungeon flow
                var newDungeon = Instantiate(dungeon);

                newDungeon.name = "LethalThingsDungeon";

                AudioClip audioClip = Content.MainAssets.LoadAsset<AudioClip>("Assets/Custom/LethalThings/brap.mp3");

                LethalLib.Modules.Dungeon.AddDungeon(newDungeon, 600, LethalLib.Modules.Levels.LevelTypes.All, audioClip);
            }
            orig(self);
        }*/     
    }
}