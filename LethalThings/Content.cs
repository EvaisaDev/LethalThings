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
            public bool isScrap = false;
            public bool batteryFix = false;
            public string itemPath = "";
            public string infoPath = "";
            public int itemPrice = 0;

            public CustomItem(string name, bool isScrap, string itemPath, string infoPath, bool batteryFix = false, int itemPrice = 0)
            {
                this.name = name;
                this.isScrap = isScrap;
                this.itemPath = itemPath;
                this.infoPath = infoPath;
                this.batteryFix = batteryFix;
                this.itemPrice = itemPrice;
            }

            public static CustomItem Add(string name, bool isScrap, string itemPath, string infoPath = null, bool batteryFix = false, int itemPrice = 0)
            {
                CustomItem item = new CustomItem(name, isScrap, itemPath, infoPath, batteryFix, itemPrice);
                return item;
            }
        }


        static List<CustomItem> customItems = new List<CustomItem>() 
        { 
            CustomItem.Add("Arson", true, "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset", null, true),
            CustomItem.Add("Cookie", true, "Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset", null, true),
            CustomItem.Add("Bilka", true, "Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset"),
            CustomItem.Add("Hamis", true, "Assets/Custom/LethalThings/Scrap/Hamis/HamisPlush.asset"),
            CustomItem.Add("ArsonDirty", true, "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlushDirty.asset", null, true, itemPrice: 400),
            CustomItem.Add("RocketLauncher", false, "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncher.asset", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncherInfo.asset"),
            CustomItem.Add("ToyHammer", false, "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammerInfo.asset", itemPrice: 80),
            CustomItem.Add("Maxwell", true, "Assets/Custom/LethalThings/Scrap/Maxwell/Dingus.asset"),
            CustomItem.Add("PouchyBelt", false, "Assets/Custom/LethalThings/Items/Pouch/Pouch.asset", "Assets/Custom/LethalThings/Items/Pouch/PouchInfo.asset", itemPrice: 100),
        };

        public static void Load()
        {
            MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));

            foreach (var item in customItems)
            {
                var itemAsset = MainAssets.LoadAsset<Item>(item.itemPath);

                Prefabs.Add(item.name, itemAsset.spawnPrefab);
                NetworkPrefabs.RegisterNetworkPrefab(itemAsset.spawnPrefab);
                if (item.batteryFix)
                {
                    batteryUsageFix.Add(itemAsset.itemName);
                }

                if(item.infoPath != null)
                {
                    var itemInfo = MainAssets.LoadAsset<TerminalNode>(item.infoPath);
                    Items.RegisterShopItem(itemAsset, null, null, itemInfo, item.itemPrice);
                }
                else
                {
                    Items.RegisterScrap(itemAsset, 100, Levels.LevelTypes.All);
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
