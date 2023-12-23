using LethalLib.Extras;
using LethalLib.Modules;
using LethalThings.Extensions;
using LethalThings.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Netcode.Components;
using Unity.Netcode.Samples;
using UnityEngine;

namespace LethalThings
{
    public class Content
    {
        public static AssetBundle MainAssets;
        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
        public static GameObject devMenuPrefab;

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

        public class CustomUnlockable
        {
            public string name = "";
            public string unlockablePath = "";
            public string infoPath = "";
            public Action<UnlockableItem> unlockableAction = (item) => { };
            public bool enabled = true;
            public int unlockCost = -1;
            //public StoreType storeType = StoreType.None;

            public CustomUnlockable(string name, string unlockablePath, string infoPath, Action<UnlockableItem> action = null, int unlockCost = -1)
            {
                this.name = name;
                this.unlockablePath = unlockablePath;
                this.infoPath = infoPath;
                if (action != null)
                {
                    unlockableAction = action;
                }
                this.unlockCost = unlockCost;
                //this.storeType = storeType;
            }

            public static CustomUnlockable Add(string name, string unlockablePath, string infoPath = null, Action<UnlockableItem> action = null, int unlockCost = -1, bool enabled = true)
            {
                CustomUnlockable item = new CustomUnlockable(name, unlockablePath, infoPath, action, unlockCost);
                item.enabled = enabled;
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

        public class CustomPlainItem : CustomItem
        {
            public CustomPlainItem(string name, string itemPath, Action<Item> action = null) : base(name, itemPath, null, action)
            {
            }

            public static CustomPlainItem Add(string name, string itemPath, Action<Item> action = null)
            {
                CustomPlainItem item = new CustomPlainItem(name, itemPath, action);
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
            public bool enabled = true;

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

            public static CustomEnemy Add(string name, string enemyPath, int rarity, Levels.LevelTypes levelFlags, Enemies.SpawnType spawnType, string infoKeyword, string infoNode, bool enabled = true)
            {
                CustomEnemy enemy = new CustomEnemy(name, enemyPath, rarity, levelFlags, spawnType, infoKeyword, infoNode);
                enemy.enabled = enabled;
                return enemy;
            }
        }

        public class CustomMapObject
        {
            public string name;
            public string objectPath;
            public Levels.LevelTypes levelFlags;
            public Func<SelectableLevel, AnimationCurve> spawnRateFunction;
            public bool enabled = true;

            public CustomMapObject(string name, string objectPath, Levels.LevelTypes levelFlags, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null, bool enabled = false)
            {
                this.name = name;
                this.objectPath = objectPath;
                this.levelFlags = levelFlags;
                this.spawnRateFunction = spawnRateFunction;
                this.enabled = enabled;
            }

            public static CustomMapObject Add(string name, string objectPath, Levels.LevelTypes levelFlags, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null, bool enabled = false)
            {
                CustomMapObject mapObject = new CustomMapObject(name, objectPath, levelFlags, spawnRateFunction, enabled);
                return mapObject;
            }
        }


        public static void TryLoadAssets()
        {
            if (MainAssets == null)
            {
                MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));
                Plugin.logger.LogInfo("Loaded asset bundle");
            }
        }

        public static List<CustomUnlockable> customUnlockables;
        public static List<CustomItem> customItems;
        public static List<CustomEnemy> customEnemies;
        public static List<CustomMapObject> customMapObjects;
        public static GameObject ConfigManagerPrefab;
        public static void Load()
        {
            TryLoadAssets();

            customItems = new List<CustomItem>()
            {
                CustomScrap.Add("Arson", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset", Levels.LevelTypes.All, Config.arsonSpawnWeight.Value),
                CustomScrap.Add("Cookie", "Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset", Levels.LevelTypes.All, Config.cookieSpawnWeight.Value),
                CustomScrap.Add("Bilka", "Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset", Levels.LevelTypes.All,  Config.toimariSpawnWeight.Value),
                CustomScrap.Add("Hamis", "Assets/Custom/LethalThings/Scrap/Hamis/HamisPlush.asset", Levels.LevelTypes.All,  Config.hamisSpawnWeight.Value),
                CustomScrap.Add("ArsonDirty", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlushDirty.asset", Levels.LevelTypes.All, Config.dirtyArsonSpawnWeight.Value),
                CustomScrap.Add("Maxwell", "Assets/Custom/LethalThings/Scrap/Maxwell/Dingus.asset", Levels.LevelTypes.All, Config.maxwellSpawnWeight.Value),
                CustomScrap.Add("Glizzy", "Assets/Custom/LethalThings/Scrap/glizzy/glizzy.asset", Levels.LevelTypes.All, Config.glizzySpawnChance.Value),
                CustomScrap.Add("Revolver", "Assets/Custom/LethalThings/Scrap/Flaggun/Toygun.asset", Levels.LevelTypes.All, Config.revolverSpawnChance.Value),
                CustomShopItem.Add("RocketLauncher", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncher.asset", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncherInfo.asset", action: (item) => {
                    NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<RocketLauncher>().missilePrefab);
                }, itemPrice:  Config.rocketLauncherPrice.Value, enabled: Config.rocketLauncherEnabled.Value),
                CustomShopItem.Add("Flaregun", "Assets/Custom/LethalThings/Items/Flaregun/Flaregun.asset", "Assets/Custom/LethalThings/Items/Flaregun/FlaregunInfo.asset", action: (item) => {
                    NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<ProjectileWeapon>().projectilePrefab);
                }, itemPrice:  10, enabled: true),
                CustomShopItem.Add("FlaregunAmmo", "Assets/Custom/LethalThings/Items/Flaregun/FlaregunAmmo.asset", "Assets/Custom/LethalThings/Items/Flaregun/FlaregunAmmoInfo.asset", 10, enabled: true),
                CustomShopItem.Add("ToyHammer", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammerInfo.asset", Config.toyHammerPrice.Value, enabled: Config.toyHammerEnabled.Value),
                CustomShopItem.Add("RemoteRadar", "Assets/Custom/LethalThings/Items/Radar/HandheldRadar.asset", "Assets/Custom/LethalThings/Items/Radar/HandheldRadarInfo.asset", Config.remoteRadarPrice.Value, enabled: Config.remoteRadarEnabled.Value),
                CustomShopItem.Add("PouchyBelt", "Assets/Custom/LethalThings/Items/Pouch/Pouch.asset", "Assets/Custom/LethalThings/Items/Pouch/PouchInfo.asset", Config.pouchyBeltPrice.Value, enabled: Config.pouchyBeltEnabled.Value),
                CustomShopItem.Add("HackingTool", "Assets/Custom/LethalThings/Items/HackingTool/HackingTool.asset", "Assets/Custom/LethalThings/Items/HackingTool/HackingToolInfo.asset", Config.hackingToolPrice.Value, enabled: Config.hackingToolEnabled.Value),
                CustomPlainItem.Add("Dart", "Assets/Custom/LethalThings/Unlockables/dartboard/Dart.asset"),
                CustomScrap.Add("GremlinEnergy", "Assets/Custom/LethalThings/Scrap/GremlinEnergy/GremlinEnergy.asset", Levels.LevelTypes.All, Config.gremlinSodaSpawnChance.Value),
            };

            customEnemies = new List<CustomEnemy>()
            {
                CustomEnemy.Add("Boomba", "Assets/Custom/LethalThings/Enemies/Roomba/Boomba.asset", Config.boombaSpawnWeight.Value, Levels.LevelTypes.All, Enemies.SpawnType.Default, null, "Assets/Custom/LethalThings/Enemies/Roomba/BoombaFile.asset", enabled: true),
            };

            customUnlockables = new List<CustomUnlockable>
            {
                CustomUnlockable.Add("SmallRug", "Assets/Custom/LethalThings/Unlockables/Rug/SmallRug.asset", "Assets/Custom/LethalThings/Unlockables/Rug/RugInfo.asset", null, Config.smallRugPrice.Value, enabled: Config.rugsEnabled.Value),
                CustomUnlockable.Add("LargeRug", "Assets/Custom/LethalThings/Unlockables/Rug/LargeRug.asset", "Assets/Custom/LethalThings/Unlockables/Rug/RugInfo.asset", null, Config.largeRugPrice.Value, enabled: Config.rugsEnabled.Value),
                CustomUnlockable.Add("FatalitiesSign", "Assets/Custom/LethalThings/Unlockables/Sign/Sign.asset", "Assets/Custom/LethalThings/Unlockables/Sign/SignInfo.asset", null, Config.fatalitiesSignPrice.Value, enabled: Config.fatalitiesSignEnabled.Value),
                CustomUnlockable.Add("Dartboard", "Assets/Custom/LethalThings/Unlockables/dartboard/Dartboard.asset", "Assets/Custom/LethalThings/Unlockables/dartboard/DartboardInfo.asset", null, 0, true),
            
            };

            customMapObjects = new List<CustomMapObject>()
            {
                CustomMapObject.Add("TeleporterTrap", "Assets/Custom/LethalThings/hazards/TeleporterTrap/TeleporterTrap.asset", Levels.LevelTypes.All, (level) => { 
                    // debug
                    return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 4));
                    /*
                    if(!level.spawnableMapObjects.Any(x => x.prefabToSpawn.name == "Landmine"))
                    {
                        // no landmines, no teleporter traps
                        return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
                    }

                    AnimationCurve spawnRate = level.spawnableMapObjects.First(x => x.prefabToSpawn.name == "Landmine").numberToSpawn;
                    // cut the value of each keyframe in half

                    for (int i = 0; i < spawnRate.keys.Length; i++)
                    {
                        spawnRate.MoveKey(i, new Keyframe(spawnRate.keys[i].time, spawnRate.keys[i].value / 2));
                    }

                    return spawnRate;*/
                }, Config.teleporterTrapsEnabled.Value)
            };



            foreach (var item in customItems)
            {
                if(!item.enabled)
                {
                    continue;
                }

                var itemAsset = MainAssets.LoadAsset<Item>(item.itemPath);
                if(itemAsset.spawnPrefab.GetComponent<NetworkTransform>() == null && itemAsset.spawnPrefab.GetComponent<CustomNetworkTransform>() == null)
                {
                    var networkTransform = itemAsset.spawnPrefab.AddComponent<NetworkTransform>();
                    networkTransform.SlerpPosition = false;
                    networkTransform.Interpolate = false;
                    networkTransform.SyncPositionX = false;
                    networkTransform.SyncPositionY = false;
                    networkTransform.SyncPositionZ = false;
                    networkTransform.SyncScaleX = false;
                    networkTransform.SyncScaleY = false;
                    networkTransform.SyncScaleZ = false;
                    networkTransform.UseHalfFloatPrecision = true;
                }
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
                else if(item is CustomPlainItem)
                {
                    Items.RegisterItem(itemAsset);
                }
            }

            foreach (var unlockableItem in customUnlockables)
            {
                if (!unlockableItem.enabled)
                {
                    continue;
                }

                var unlockable = MainAssets.LoadAsset<UnlockableItemDef>(unlockableItem.unlockablePath).unlockable;
                if(unlockable.prefabObject != null)
                {
                    // register
                    NetworkPrefabs.RegisterNetworkPrefab(unlockable.prefabObject);
                }

                Prefabs.Add(unlockableItem.name, unlockable.prefabObject);

                TerminalNode info = null;
                if(unlockableItem.infoPath != null) { 
                    info = MainAssets.LoadAsset<TerminalNode>(unlockableItem.infoPath);
                }

                Unlockables.RegisterUnlockable(unlockable, StoreType.Decor, null, null, info, price: unlockableItem.unlockCost);
            }

            foreach (var enemy in customEnemies)
            {
                if (!enemy.enabled)
                {
                    continue;
                }

                var enemyAsset = MainAssets.LoadAsset<EnemyType>(enemy.enemyPath);
                var enemyInfo = MainAssets.LoadAsset<TerminalNode>(enemy.infoNode);
                TerminalKeyword enemyTerminal = null;
                if (enemy.infoKeyword != null)
                {
                    enemyTerminal = MainAssets.LoadAsset<TerminalKeyword>(enemy.infoKeyword);
                }

                NetworkPrefabs.RegisterNetworkPrefab(enemyAsset.enemyPrefab);

                Prefabs.Add(enemy.name, enemyAsset.enemyPrefab);

                Enemies.RegisterEnemy(enemyAsset, enemy.rarity, enemy.levelFlags, enemy.spawnType, enemyInfo, enemyTerminal);
            }

            foreach (var mapObject in customMapObjects)
            {
                if (!mapObject.enabled)
                {
                    continue;
                }
                var mapObjectAsset = MainAssets.LoadAsset<SpawnableMapObjectDef>(mapObject.objectPath);
                NetworkPrefabs.RegisterNetworkPrefab(mapObjectAsset.spawnableMapObject.prefabToSpawn);

                Prefabs.Add(mapObject.name, mapObjectAsset.spawnableMapObject.prefabToSpawn);

                MapObjects.RegisterMapObject(mapObjectAsset, mapObject.levelFlags, mapObject.spawnRateFunction);
            }


            // loop through prefabs
            foreach (var prefabSet in Prefabs)
            {
                var prefab = prefabSet.Value;

                // get prefab name
                var prefabName = prefabSet.Key;


                // get all AudioSources
                var audioSources = prefab.GetComponentsInChildren<AudioSource>();

                // if has any AudioSources

                if( audioSources.Length > 0)
                {
                    var configValue = Config.VolumeConfig.Bind<float>("Volume", $"{prefabName}", 100f, $"Audio volume for {prefabName} (0 - 100)");

                    // loop through AudioSources, adjust volume by multiplier
                    foreach (var audioSource in audioSources)
                    {
                        audioSource.volume *= (configValue.Value / 100);
                    }
                }
            }


            var devMenu = MainAssets.LoadAsset<GameObject>("Assets/Custom/LethalThings/DevMenu.prefab");

            NetworkPrefabs.RegisterNetworkPrefab(devMenu);

            devMenuPrefab = devMenu;

            try
            {
                var types = Assembly.GetExecutingAssembly().GetLoadableTypes();
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
            }
            catch (Exception e)
            {
                
            }



            Plugin.logger.LogInfo("Custom content loaded!");
        }



    }
}
