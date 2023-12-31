﻿using BepInEx;
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
        public const string ModVersion = "0.8.0";

        public static ManualLogSource logger;
        public static ConfigFile config;

        public static bool devMode = false;

        private void Awake()
        {
            logger = Logger;
            config = Config;

           

            Utilities.Init();
            LethalThings.Config.Load();
            Content.Load();
            Patches.Patches.Load();

            if (InputCompat.Enabled)
                InputCompat.Init();
            //Logger.LogInfo("LethalThings loaded guh");

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