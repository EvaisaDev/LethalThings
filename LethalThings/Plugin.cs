using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LethalThings
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethalthings";
        public const string ModName = "LethalThings";
        public const string ModVersion = "0.1.0";

        public static AssetBundle MainAssets;

        [Flags]
        public enum Levels
        {
            None = 1 << 0,
            ExperimentationLevel = 1 << 1,
            AssuranceLevel = 1 << 2,
            VowLevel = 1 << 3,
            OffenseLevel = 1 << 4,
            MarchLevel = 1 << 5,
            RendLevel = 1 << 6,
            DineLevel = 1 << 7,
            TitanLevel = 1 << 8,
            All = ExperimentationLevel | AssuranceLevel | VowLevel | OffenseLevel | MarchLevel | RendLevel | DineLevel | TitanLevel
        }

        public class ScrapItem
        {
            public Item item;
            public int rarity;
            public Levels spawnLevels;

            public ScrapItem(Item item, int rarity, Levels spawnLevels)
            {
                this.item = item;
                this.rarity = rarity;
                this.spawnLevels = spawnLevels;
            }
        }

        public List<ScrapItem> scrapItems = new List<ScrapItem>();

        private void Awake()
        {

            // find the file named arsonplush in the dll's folder
            MainAssets = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("LethalThings.dll", "arsonplush"));

            var plush = MainAssets.LoadAsset<Item>("Assets/Custom/LethalThings/Scrap/ArsonPlush.asset");

            FixShaders(plush.spawnPrefab);

            RegisterScrap(plush, 80, Levels.All);

            On.StartOfRound.Awake += StartOfRound_Awake;

            // Plugin startup logic
            Logger.LogInfo($"Lethal things loaded!!");
        }

        public void FixShaders(GameObject gameObject)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                foreach (var material in renderer.materials)
                {
                    if (material.shader.name.Contains("Standard"))
                    {
                        // ge
                        material.shader = Shader.Find("HDRP/Lit");
                    }
                }
            }
        }

        private void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {
            orig(self);

            foreach(SelectableLevel level in self.levels)
            {
                var name = level.name;

                if (Enum.IsDefined(typeof(Levels), name))
                {
                    var levelEnum = (Levels)Enum.Parse(typeof(Levels), name);
                    foreach (ScrapItem scrapItem in scrapItems)
                    {
                        if (scrapItem.spawnLevels.HasFlag(levelEnum))
                        {
                            var spawnableÍtemWithRarity = new SpawnableItemWithRarity() { 
                                spawnableItem = scrapItem.item,
                                rarity = scrapItem.rarity
                            };

                            // make sure spawnableScrap does not already contain item
                            Logger.LogInfo($"Checking if {scrapItem.item.name} is already in {name}");

                            if (!level.spawnableScrap.Any(x => x.spawnableItem == scrapItem.item))
                            {
                                level.spawnableScrap.Add(spawnableÍtemWithRarity);
                                Logger.LogInfo($"Added {scrapItem.item.name} to {name}");
                            }
                        }
                    }
                }
            }



            foreach (ScrapItem scrapItem in scrapItems)
            {
                if(!self.allItemsList.itemsList.Contains(scrapItem.item))
                {
                    Logger.LogInfo($"Item registered: {scrapItem.item.name}");
                    self.allItemsList.itemsList.Add(scrapItem.item);
                }
            }

         }

        public void RegisterScrap(Item spawnableItem, int rarity, Levels levelFlags)
        {
            var scrapItem = new ScrapItem(spawnableItem, rarity, levelFlags);
            scrapItems.Add(scrapItem);
        }
    }
}