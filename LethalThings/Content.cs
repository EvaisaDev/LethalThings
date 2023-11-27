using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LethalThings
{
    public class Content
    {
        public static AssetBundle MainAssets;
        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        public class CustomItem
        {
            public string name = "";
            public string itemPath = "";
            public string infoPath = "";
            public Action<Item> itemAction = (item) => { };
            public bool enabled = true;

            public CustomItem(string name, string itemPath, string infoPath, Action<Item> action = null)
            {
                this.name = name;
                this.itemPath = itemPath;
                this.infoPath = infoPath;
                if(action != null)
                {
                    itemAction = action;
                }
            }

            public static CustomItem Add(string name, string itemPath, string infoPath = null, Action<Item> action = null)
            {
                CustomItem item = new CustomItem(name, itemPath, infoPath, action);
                return item;
            }
        }

        public class CustomShopItem : CustomItem
        {
            public int itemPrice = 0;

            public CustomShopItem(string name, string itemPath, string infoPath = null, int itemPrice = 0, Action<Item> action = null) : base(name, itemPath, infoPath, action)
            {
                this.itemPrice = itemPrice;
            }

            public static CustomShopItem Add(string name, string itemPath, string infoPath = null, int itemPrice = 0, Action<Item> action = null, bool enabled = true)
            {
                CustomShopItem item = new CustomShopItem(name, itemPath, infoPath, itemPrice, action);
                item.enabled = enabled;
                return item;
            }
        }

        public class CustomScrap : CustomItem
        {
            public Levels.LevelTypes levelType = Levels.LevelTypes.All;
            public int rarity = 0;

            public CustomScrap(string name, string itemPath, Levels.LevelTypes levelType, int rarity, Action<Item> action = null) : base(name, itemPath, null, action)
            {
                this.levelType = levelType;
                this.rarity = rarity;
            }

            public static CustomScrap Add(string name, string itemPath, Levels.LevelTypes levelType, int rarity, Action<Item> action = null)
            {
                CustomScrap item = new CustomScrap(name, itemPath, levelType, rarity, action);
                return item;
            }
        }

        public class CustomEnemy
        {
            public string name;
            public string enemyPath;
            public int rarity;
            public Levels.LevelTypes levelFlags;
            public Enemies.SpawnType spawnType;
            public string infoKeyword;
            public string infoNode;

            public CustomEnemy(string name, string enemyPath, int rarity, Levels.LevelTypes levelFlags, Enemies.SpawnType spawnType, string infoKeyword, string infoNode)
            {
                this.name = name;
                this.enemyPath = enemyPath;
                this.rarity = rarity;
                this.levelFlags = levelFlags;
                this.spawnType = spawnType;
                this.infoKeyword = infoKeyword;
                this.infoNode = infoNode;
            }

            public static CustomEnemy Add(string name, string enemyPath, int rarity, Levels.LevelTypes levelFlags, Enemies.SpawnType spawnType, string infoKeyword, string infoNode)
            {
                CustomEnemy enemy = new CustomEnemy(name, enemyPath, rarity, levelFlags, spawnType, infoKeyword, infoNode);
                return enemy;
            }
        }

        static List<CustomItem> customItems = new List<CustomItem>()
        {
            CustomScrap.Add("Arson", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset", Levels.LevelTypes.All, Config.arsonSpawnWeight.Value),
            CustomScrap.Add("Cookie", "Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset", Levels.LevelTypes.All, Config.cookieSpawnWeight.Value),
            CustomScrap.Add("Bilka", "Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset", Levels.LevelTypes.All,  Config.toimariSpawnWeight.Value),
            CustomScrap.Add("Hamis", "Assets/Custom/LethalThings/Scrap/Hamis/HamisPlush.asset", Levels.LevelTypes.All,  Config.hamisSpawnWeight.Value),
            CustomScrap.Add("ArsonDirty", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlushDirty.asset", Levels.LevelTypes.All, Config.dirtyArsonSpawnWeight.Value),
            CustomScrap.Add("Maxwell", "Assets/Custom/LethalThings/Scrap/Maxwell/Dingus.asset", Levels.LevelTypes.All, Config.maxwellSpawnWeight.Value),
            CustomShopItem.Add("RocketLauncher", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncher.asset", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncherInfo.asset", action: (item) => {
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<RocketLauncher>().missilePrefab);
            }, itemPrice:  Config.rocketLauncherPrice.Value, enabled: Config.rocketLauncherEnabled.Value),
            CustomShopItem.Add("ToyHammer", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammerInfo.asset", Config.toyHammerPrice.Value, enabled: Config.toyHammerEnabled.Value),
            CustomShopItem.Add("RemoteRadar", "Assets/Custom/LethalThings/Items/Radar/HandheldRadar.asset", "Assets/Custom/LethalThings/Items/Radar/HandheldRadarInfo.asset", Config.remoteRadarPrice.Value, enabled: Config.remoteRadarEnabled.Value),
            CustomShopItem.Add("PouchyBelt", "Assets/Custom/LethalThings/Items/Pouch/Pouch.asset", "Assets/Custom/LethalThings/Items/Pouch/PouchInfo.asset", Config.pouchyBeltPrice.Value, enabled: Config.pouchyBeltEnabled.Value),
        };

        static List<CustomEnemy> customEnemies = new List<CustomEnemy>()
        {
            //CustomEnemy.Add("Boomba", "Assets/Custom/LethalThings/Enemies/Roomba/Boomba.asset", 1000, Levels.LevelTypes.All, Enemies.SpawnType.Default, "Assets/Custom/LethalThings/Enemies/Roomba/BoombaTerminal.asset", "Assets/Custom/LethalThings/Enemies/Roomba/BoombaFile.asset"),
        };

        public static void Load()
        {
            MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));

            foreach (var item in customItems)
            {
                if(!item.enabled)
                {
                    continue;
                }

                var itemAsset = MainAssets.LoadAsset<Item>(item.itemPath);

                Prefabs.Add(item.name, itemAsset.spawnPrefab);
                NetworkPrefabs.RegisterNetworkPrefab(itemAsset.spawnPrefab);
                item.itemAction(itemAsset);


                if(item is CustomShopItem)
                {

                    var itemInfo = MainAssets.LoadAsset<TerminalNode>(item.infoPath);
                    Plugin.logger.LogInfo($"Registering shop item {item.name} with price {((CustomShopItem)item).itemPrice}");
                    Items.RegisterShopItem(itemAsset, null, null, itemInfo, ((CustomShopItem)item).itemPrice);
                }
                else if(item is CustomScrap)
                {
                    Items.RegisterScrap(itemAsset, ((CustomScrap)item).rarity, ((CustomScrap)item).levelType);
                }
            }

            foreach (var enemy in customEnemies)
            {
                var enemyAsset = MainAssets.LoadAsset<EnemyType>(enemy.enemyPath);
                var enemyInfo = MainAssets.LoadAsset<TerminalNode>(enemy.infoNode);
                var enemyTerminal = MainAssets.LoadAsset<TerminalKeyword>(enemy.infoKeyword);

                NetworkPrefabs.RegisterNetworkPrefab(enemyAsset.enemyPrefab);

                Enemies.RegisterEnemy(enemyAsset, enemy.rarity, enemy.levelFlags, enemy.spawnType, enemyInfo);
            }


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



            Plugin.logger.LogInfo("Custom content loaded!");
        }

    }
}
