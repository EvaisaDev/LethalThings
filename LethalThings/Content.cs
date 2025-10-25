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
using static LethalLib.Modules.ContentLoader;

namespace LethalThings
{
    public class Content
    {
        public static AssetBundle MainAssets;
        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
        public static ContentLoader ContentLoader;
        public static GameObject devMenuPrefab;
        public static GameObject configManagerPrefab;

        public static void Init()
        {
            MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lethalthings"));

            configManagerPrefab = MainAssets.LoadAsset<GameObject>("Assets/Custom/LethalThings/LTNetworkConfig.prefab");

            NetworkPrefabs.RegisterNetworkPrefab(configManagerPrefab);  

            ContentLoader = new ContentLoader(Plugin.pluginInfo, MainAssets, (content, prefab) => { 
                Prefabs.Add(content.ID, prefab);
            });

            List<CustomContent> content =
            [
                // scrap
                new ScrapItem("Arson", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlush.asset", NetworkConfig.arsonSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Cookie", "Assets/Custom/LethalThings/Scrap/Cookie/CookieFumo.asset", NetworkConfig.cookieSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Bilka", "Assets/Custom/LethalThings/Scrap/Toimari/ToimariPlush.asset", NetworkConfig.toimariSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Hamis", "Assets/Custom/LethalThings/Scrap/Hamis/HamisPlush.asset", NetworkConfig.hamisSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("ArsonDirty", "Assets/Custom/LethalThings/Scrap/Arson/ArsonPlushDirty.asset", NetworkConfig.dirtyArsonSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Maxwell", "Assets/Custom/LethalThings/Scrap/Maxwell/Dingus.asset", NetworkConfig.maxwellSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Glizzy", "Assets/Custom/LethalThings/Scrap/glizzy/glizzy.asset", NetworkConfig.glizzySpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Revolver", "Assets/Custom/LethalThings/Scrap/Flaggun/Toygun.asset", NetworkConfig.revolverSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("GremlinEnergy", "Assets/Custom/LethalThings/Scrap/GremlinEnergy/GremlinEnergy.asset", NetworkConfig.gremlinSodaSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("ToyHammerScrap", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", NetworkConfig.toyHammerScrapSpawnChance.Value, Levels.LevelTypes.All),
                new ScrapItem("Gnarpy", "Assets/Custom/LethalThings/Scrap/Gnarpy/GnarpyPlush.asset", NetworkConfig.gnarpySpawnChance.Value, Levels.LevelTypes.All),
                // shop
                new ShopItem("RocketLauncher", "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncher.asset", NetworkConfig.rocketLauncherPrice.Value, null, null, "Assets/Custom/LethalThings/Items/RocketLauncher/RocketLauncherInfo.asset", (item) => {
                    NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<RocketLauncher>().missilePrefab);
                }),
                new ShopItem("Flaregun", "Assets/Custom/LethalThings/Items/Flaregun/Flaregun.asset", NetworkConfig.flareGunPrice.Value, null, null, "Assets/Custom/LethalThings/Items/Flaregun/FlaregunInfo.asset", (item) => {
                    NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<ProjectileWeapon>().projectilePrefab);
                }),
                new ShopItem("FlaregunAmmo", "Assets/Custom/LethalThings/Items/Flaregun/FlaregunAmmo.asset", NetworkConfig.flareGunAmmoPrice.Value, null, null, "Assets/Custom/LethalThings/Items/Flaregun/FlaregunAmmoInfo.asset"),
                new ShopItem("ToyHammerShop", "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammer.asset", NetworkConfig.toyHammerPrice.Value, null, null, "Assets/Custom/LethalThings/Items/ToyHammer/ToyHammerInfo.asset"),
                new ShopItem("RemoteRadar", "Assets/Custom/LethalThings/Items/Radar/HandheldRadar.asset", NetworkConfig.remoteRadarPrice.Value, null, null, "Assets/Custom/LethalThings/Items/Radar/HandheldRadarInfo.asset"),
                new ShopItem("PouchyBelt", "Assets/Custom/LethalThings/Items/Pouch/Pouch.asset", NetworkConfig.pouchyBeltPrice.Value, null, null, "Assets/Custom/LethalThings/Items/Pouch/PouchInfo.asset"),
                new ShopItem("HackingTool", "Assets/Custom/LethalThings/Items/HackingTool/HackingTool.asset", NetworkConfig.hackingToolPrice.Value, null, null, "Assets/Custom/LethalThings/Items/HackingTool/HackingToolInfo.asset"),
                new ShopItem("Pinger", "Assets/Custom/LethalThings/Items/PingingTool/PingTool.asset", NetworkConfig.pingerPrice.Value, null, null, "Assets/Custom/LethalThings/Items/PingingTool/PingToolInfo.asset", (item) => {
                    NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab.GetComponent<Pinger>().pingMarkerPrefab);
                }),
      

                // plain items
                new CustomItem("Dart", "Assets/Custom/LethalThings/Unlockables/dartboard/Dart.asset"),

                // unlockables
                new Unlockable("SmallRug", "Assets/Custom/LethalThings/Unlockables/Rug/SmallRug.asset", NetworkConfig.smallRugPrice.Value, null, null, "Assets/Custom/LethalThings/Unlockables/Rug/RugInfo.asset", StoreType.Decor),
                new Unlockable("LargeRug", "Assets/Custom/LethalThings/Unlockables/Rug/LargeRug.asset", NetworkConfig.largeRugPrice.Value, null, null, "Assets/Custom/LethalThings/Unlockables/Rug/RugInfo.asset", StoreType.Decor),
                new Unlockable("FatalitiesSign", "Assets/Custom/LethalThings/Unlockables/Sign/Sign.asset", NetworkConfig.fatalitiesSignPrice.Value, null, null, "Assets/Custom/LethalThings/Unlockables/Sign/SignInfo.asset", StoreType.Decor),
                new Unlockable("Dartboard", "Assets/Custom/LethalThings/Unlockables/dartboard/Dartboard.asset", NetworkConfig.dartBoardPrice.Value, null, null, "Assets/Custom/LethalThings/Unlockables/dartboard/DartboardInfo.asset", StoreType.Decor),
                //new Unlockable("DeliveryRover", "Assets/Custom/LethalThings/Unlockables/Dog/Dog.asset", NetworkConfig.deliveryRoverPrice.Value, null, null, "Assets/Custom/LethalThings/Unlockables/Dog/DogInfo.asset", StoreType.ShipUpgrade),
                

                // map objects
                new MapHazard("TeleporterTrap", "Assets/Custom/LethalThings/hazards/TeleporterTrap/TeleporterTrap.asset", Levels.LevelTypes.All, null, (level) => { 
                    // spawn curve that ensures a maximum of 4 per level
                    return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 4));
                })
            ];

            // enemies
            if (NetworkConfig.boombaSpawnWeight.Value > 0)
            {
                content.Add(new CustomEnemy("Boomba", "Assets/Custom/LethalThings/Enemies/Roomba/Boomba.asset",
                    NetworkConfig.boombaSpawnWeight.Value, Levels.LevelTypes.All, Enemies.SpawnType.Default, null,
                    "Assets/Custom/LethalThings/Enemies/Roomba/BoombaFile.asset"));
            }

            if (NetworkConfig.maggieSpawnWeight.Value > 0)
            {
                content.Add(new CustomEnemy("Maggie", "Assets/Custom/LethalThings/Enemies/Maggie/Maggie.asset",
                    NetworkConfig.maggieSpawnWeight.Value, Levels.LevelTypes.All, Enemies.SpawnType.Default, null,
                    "Assets/Custom/LethalThings/Enemies/Maggie/MaggieFile.asset", null, (enemyType) =>
                    {
                        var goopRagdoll =
                            MainAssets.LoadAsset<GameObject>(
                                "Assets/Custom/LethalThings/Enemies/Maggie/PlayerRagdollGoop.prefab");
                        Player.RegisterPlayerRagdoll("LTGoopRagdoll", goopRagdoll);
                    }));
            }

            if (NetworkConfig.crystalRaySpawnWeight.Value > 0)
            {
                content.Add(new CustomEnemy("CrystalRay",
                    "Assets/Custom/LethalThings/Enemies/CrystalRay/CrystalRay.asset",
                    NetworkConfig.crystalRaySpawnWeight.Value, Levels.LevelTypes.All, Enemies.SpawnType.Default, null,
                    "Assets/Custom/LethalThings/Enemies/CrystalRay/CrystalRayFile.asset"));
            }
            

            ContentLoader.RegisterAll(content);


            // loop through prefabs
            foreach (var prefabSet in Prefabs)
            {
                var prefab = prefabSet.Value;

                // get prefab name
                var prefabName = prefabSet.Key;


                // get all AudioSources
                var audioSources = prefab.GetComponentsInChildren<AudioSource>();

                // if has any AudioSources

                if (audioSources.Length > 0)
                {
                    var configValue = NetworkConfig.VolumeConfig.Bind<float>("Volume", $"{prefabName}", 100f, $"Audio volume for {prefabName} (0 - 100)");

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
        }


    }
}
