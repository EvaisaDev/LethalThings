using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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


            Logger.LogInfo("LethalThings loaded");
            // find the file named arsonplush in the dll's folder
            MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));

            var arson = MainAssets.LoadAsset<Item>("Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset");
            var cookie = MainAssets.LoadAsset<Item>("Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset");
            var bilka = MainAssets.LoadAsset<Item>("Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset");

            // Register scraps
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(arson.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(cookie.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(bilka.spawnPrefab);

            Items.RegisterScrap(cookie, 40, Levels.LevelTypes.All);
            Items.RegisterScrap(arson, 40, Levels.LevelTypes.All);
            Items.RegisterScrap(bilka, 40, Levels.LevelTypes.All);

            Items.RegisterShopItem(cookie, 0);


            // Register enemies
            /*var enemy = MainAssets.LoadAsset<EnemyType>("Assets/Custom/LethalThings/Enemies/Roomba/Boomba.asset");
            var terminalKeyword = MainAssets.LoadAsset<TerminalKeyword>("Assets/Custom/LethalThings/Enemies/Roomba/BoombaTerminal.asset");
            var terminalNode = MainAssets.LoadAsset<TerminalNode>("Assets/Custom/LethalThings/Enemies/Roomba/BoombaFile.asset");

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(enemy.enemyPrefab);

            Enemies.RegisterEnemy(enemy, 80, Levels.LevelTypes.All, SpawnType.Default, terminalKeyword, terminalNode);
            Enemies.RegisterEnemy(enemy, 100, Levels.LevelTypes.All, SpawnType.Outside);
            Enemies.RegisterEnemy(enemy, 100, Levels.LevelTypes.All, SpawnType.Daytime);*/

            // Funny power socket stun
            On.GrabbableObject.Start += GrabbableObject_Start;
            On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
            On.ItemCharger.Update += ItemCharger_Update;

            // NetworkBehaviour patching

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }


            // debug

            //On.RoundManager.SpawnScrapInLevel += RoundManager_SpawnScrapInLevel;
        }


        // Function override for debugging
        private void RoundManager_SpawnScrapInLevel(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
        {
            List<Item> ScrapToSpawn = new List<Item>();
            List<int> list = new List<int>();
            int num = 0;
            List<int> list2 = new List<int>(self.currentLevel.spawnableScrap.Count);
            for (int j = 0; j < self.currentLevel.spawnableScrap.Count; j++)
            {
                list2.Add(self.currentLevel.spawnableScrap[j].rarity);
                Logger.LogInfo(j + ": " + self.currentLevel.spawnableScrap[j].spawnableItem.itemName + " rarity: " + self.currentLevel.spawnableScrap[j].rarity);
            }
            int[] weights = list2.ToArray();
            int num2 = (int)((float)self.AnomalyRandom.Next(self.currentLevel.minScrap, self.currentLevel.maxScrap) * self.scrapAmountMultiplier);
            for (int k = 0; k < num2; k++)
            {
                ScrapToSpawn.Add(self.currentLevel.spawnableScrap[self.GetRandomWeightedIndex(weights)].spawnableItem);
            }
            //Debug.Log($"Number of scrap to spawn: {ScrapToSpawn.Count}. minTotalScrapValue: {currentLevel.minTotalScrapValue}. Total value of items: {num}.");
            RandomScrapSpawn randomScrapSpawn = null;
            RandomScrapSpawn[] source = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
            List<NetworkObjectReference> list3 = new List<NetworkObjectReference>();
            List<RandomScrapSpawn> usedSpawns = new List<RandomScrapSpawn>();
            int i;
            for (i = 0; i < ScrapToSpawn.Count; i++)
            {
                if (ScrapToSpawn[i] == null)
                {
                    //Debug.Log("Error!!!!! Found null element in list ScrapToSpawn. Skipping it.");
                    continue;
                }
                List<RandomScrapSpawn> list4 = ((ScrapToSpawn[i].spawnPositionTypes != null && ScrapToSpawn[i].spawnPositionTypes.Count != 0) ? source.Where((RandomScrapSpawn x) => ScrapToSpawn[i].spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed).ToList() : source.ToList());
                if (list4.Count <= 0)
                {
                    //Debug.Log("No tiles containing a scrap spawn with item type: " + ScrapToSpawn[i].itemName);
                    continue;
                }
                if (usedSpawns.Count > 0 && list4.Contains(randomScrapSpawn))
                {
                    list4.RemoveAll((RandomScrapSpawn x) => usedSpawns.Contains(x));
                    if (list4.Count <= 0)
                    {
                        usedSpawns.Clear();
                        i--;
                        continue;
                    }
                }
                randomScrapSpawn = list4[self.AnomalyRandom.Next(0, list4.Count)];
                usedSpawns.Add(randomScrapSpawn);
                Vector3 position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    randomScrapSpawn.spawnUsed = true;
                    position = randomScrapSpawn.transform.position;
                }
                else
                {
                    position = self.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, self.navHit) + Vector3.up * ScrapToSpawn[i].verticalOffset;
                }
                Logger.LogInfo($"Spawning {ScrapToSpawn[i].itemName} at {position}");

                GameObject obj = UnityEngine.Object.Instantiate(ScrapToSpawn[i].spawnPrefab, position, Quaternion.identity, self.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                list.Add((int)((float)self.AnomalyRandom.Next(ScrapToSpawn[i].minValue, ScrapToSpawn[i].maxValue) * self.scrapValueMultiplier));
                num += list[list.Count - 1];
                component.scrapValue = list[list.Count - 1];
                NetworkObject component2 = obj.GetComponent<NetworkObject>();
                component2.Spawn();
                list3.Add(component2);
            }
            //StartCoroutine(self.waitForScrapToSpawnToSync(list3.ToArray(), list.ToArray()));
        }

        private void ItemCharger_Update(On.ItemCharger.orig_Update orig, ItemCharger self)
        {
            orig(self);
            if (self.updateInterval == 0f)
            {
                if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    self.triggerScript.interactable = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null && (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.requiresBattery || GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.isConductiveMetal);
                    return;
                }
            }
        }

        private void GrabbableObject_Start(On.GrabbableObject.orig_Start orig, GrabbableObject self)
        {
            orig(self);
            if (self.itemProperties.isConductiveMetal)
            {
                var comp = self.gameObject.AddComponent<PowerOutletStun>();
            }
        }

        private void ItemCharger_ChargeItem(On.ItemCharger.orig_ChargeItem orig, ItemCharger self)
        {
            GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer == null)
            {
                return;
            }
            if (!currentlyHeldObjectServer.itemProperties.requiresBattery)
            {
                if (currentlyHeldObjectServer.itemProperties.isConductiveMetal)
                {
                    currentlyHeldObjectServer.GetComponent<PowerOutletStun>().Electrocute(self);
                }
                return;
            }
            orig(self);
        }
    }
}