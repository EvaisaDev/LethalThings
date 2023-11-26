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
        public static List<string> batteryUsageFix = new List<string>();

        public class CustomItem
        {
            public string name = "";
            public bool batteryFix = false;
            public string itemPath = "";
            public string infoPath = "";
            public Action<Item> itemAction = (item) => { };

            public CustomItem(string name, string itemPath, string infoPath, bool batteryFix = false, Action<Item> action = null)
            {
                this.name = name;
                this.itemPath = itemPath;
                this.infoPath = infoPath;
                this.batteryFix = batteryFix;
                if(action != null)
                {
                    itemAction = action;
                }
            }

            public static CustomItem Add(string name, string itemPath, string infoPath = null, bool batteryFix = false, Action<Item> action = null)
            {
                CustomItem item = new CustomItem(name, itemPath, infoPath, batteryFix, action);
                return item;
            }
        }

        public class CustomShopItem : CustomItem
        {
            public int itemPrice = 0;

            public CustomShopItem(string name, string itemPath, string infoPath = null, int itemPrice = 0, bool batteryFix = false, Action<Item> action = null) : base(name, itemPath, infoPath, batteryFix, action)
            {
                this.itemPrice = itemPrice;
            }

            public static CustomShopItem Add(string name, string itemPath, string infoPath = null, int itemPrice = 0, bool batteryFix = false, Action<Item> action = null)
            {
                CustomShopItem item = new CustomShopItem(name, itemPath, infoPath, itemPrice, batteryFix, action);
                return item;
            }
        }

        public class CustomScrap : CustomItem
        {
            public Levels.LevelTypes levelType = Levels.LevelTypes.All;
            public int rarity = 0;

            public CustomScrap(string name, string itemPath, Levels.LevelTypes levelType, int rarity, bool batteryFix = false, int itemPrice = 0, Action<Item> action = null) : base(name, itemPath, null, batteryFix, action)
            {
                this.levelType = levelType;
                this.rarity = rarity;
            }

            public static CustomScrap Add(string name, string itemPath, Levels.LevelTypes levelType, int rarity, bool batteryFix = false, Action<Item> action = null)
            {
                CustomScrap item = new CustomScrap(name, itemPath, levelType, rarity, batteryFix, 0, action);
                return item;
            }
        }


        static List<CustomItem> customItems = new List<CustomItem>()
        {
            CustomScrap.Add("Arson", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset", Levels.LevelTypes.All, 20, true),
            CustomScrap.Add("Cookie", "Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset", Levels.LevelTypes.All, 40, true),
            CustomScrap.Add("Bilka", "Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset", Levels.LevelTypes.All, 40),
            CustomScrap.Add("Hamis", "Assets/Custom/LethalThings/Scrap/Hamis/HamisPlush.asset", Levels.LevelTypes.All, 40),
            CustomScrap.Add("ArsonDirty", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlushDirty.asset", Levels.LevelTypes.All, 20, true),
            CustomShopItem.Add("RocketLauncher", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncher.asset", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncherInfo.asset", action: (item) => {
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<RocketLauncher>().missilePrefab);
            }, itemPrice: 400),
            CustomShopItem.Add("ToyHammer", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammerInfo.asset", 80),
            CustomScrap.Add("Maxwell", "Assets/Custom/LethalThings/Scrap/Maxwell/Dingus.asset", Levels.LevelTypes.All, 3),
            CustomShopItem.Add("RemoteRadar", "Assets/Custom/LethalThings/Items/Radar/HandheldRadar.asset", "Assets/Custom/LethalThings/Items/Radar/HandheldRadarInfo.asset", 240),
            CustomShopItem.Add("PouchyBelt", "Assets/Custom/LethalThings/Items/Pouch/Pouch.asset", "Assets/Custom/LethalThings/Items/Pouch/PouchInfo.asset", 290),
        };

        public static void Load()
        {
            MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));

            foreach (var item in customItems)
            {
                var itemAsset = MainAssets.LoadAsset<Item>(item.itemPath);

                Prefabs.Add(item.name, itemAsset.spawnPrefab);
                NetworkPrefabs.RegisterNetworkPrefab(itemAsset.spawnPrefab);
                item.itemAction(itemAsset);

                if (item.batteryFix)
                {
                    batteryUsageFix.Add(itemAsset.itemName);
                }

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

            // audio scrap battery patch guh
            On.NoisemakerProp.ItemActivate += NoisemakerProp_ItemActivate;

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

        private static void NoisemakerProp_ItemActivate(On.NoisemakerProp.orig_ItemActivate orig, NoisemakerProp self, bool used, bool buttonDown)
        {
            orig(self, used, buttonDown);
            if (batteryUsageFix.Contains(self.itemProperties.itemName) && self.insertedBattery.charge >= 0)
            {
                self.insertedBattery.charge -= 5;
            }
        }
    }
}
