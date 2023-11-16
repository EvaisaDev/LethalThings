using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Enemies;

namespace LethalThings
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethalthings";
        public const string ModName = "LethalThings";
        public const string ModVersion = "0.1.0";

        public static AssetBundle MainAssets;

        private void Awake()
        {
           
            // find the file named arsonplush in the dll's folder
            MainAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("LethalThings.dll", "lethalthings"));

            var plush = MainAssets.LoadAsset<Item>("Assets/Custom/LethalThings/Scrap/ArsonPlush.asset");
            var enemy = MainAssets.LoadAsset<EnemyType>("Assets/Custom/LethalThings/Enemies/Boomba.asset");
            var terminalKeyword = MainAssets.LoadAsset<TerminalKeyword>("Assets/Custom/LethalThings/Enemies/BoombaTerminal.asset");
            var terminalNode = MainAssets.LoadAsset<TerminalNode>("Assets/Custom/LethalThings/Enemies/BoombaFile.asset");

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(plush.spawnPrefab);
            Items.RegisterScrap(plush, 80, Levels.LevelTypes.All);
            
            /*Enemies.RegisterEnemy(enemy, 80, Levels.LevelTypes.All, SpawnType.Default, terminalKeyword, terminalNode);
            Enemies.RegisterEnemy(enemy, 100, Levels.LevelTypes.All, SpawnType.Outside);
            Enemies.RegisterEnemy(enemy, 100, Levels.LevelTypes.All, SpawnType.Daytime);*/
        }


    }
}