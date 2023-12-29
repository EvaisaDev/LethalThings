using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
//using static LethalLib.Modules.ContentLoader;
using LethalLib.Modules;
using UnityEngine;
using static LethalLib.Modules.ContentLoader;

namespace LethalThings
{
    // This code is fucked up and evil and i hate it.
    // I regret writing it but people wanted config sync and i was too lazy to come up with a better solution
    public class NetworkConfig : NetworkBehaviour
    {
        public static NetworkConfig Instance;

        public void Awake()
        {
            Instance = this;
        }

        // scrap, no need to sync weight
        public static ConfigEntry<int> arsonSpawnChance;
        public static ConfigEntry<int> dirtyArsonSpawnChance;
        public static ConfigEntry<int> toimariSpawnChance;
        public static ConfigEntry<int> hamisSpawnChance;
        public static ConfigEntry<int> cookieSpawnChance;
        public static ConfigEntry<int> maxwellSpawnChance;
        public static ConfigEntry<int> glizzySpawnChance;
        public static ConfigEntry<int> revolverSpawnChance;
        public static ConfigEntry<int> gremlinSodaSpawnChance;

        public static ConfigEntry<float> evilMaxwellChance;

        // special case that needs sync
        public static ConfigEntry<bool> maxwellPlayMusicDefault;
        public NetworkVariable<bool> maxwellPlayMusicDefaultNetVar = new NetworkVariable<bool>(true);

        // Shop items
        public static ConfigEntry<bool> toyHammerEnabled;
        public NetworkVariable<bool> toyHammerEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> toyHammerPrice;
        public NetworkVariable<int> toyHammerPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> pouchyBeltEnabled;
        public NetworkVariable<bool> pouchyBeltEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> pouchyBeltPrice;
        public NetworkVariable<int> pouchyBeltPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> remoteRadarEnabled;
        public NetworkVariable<bool> remoteRadarEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> remoteRadarPrice;
        public NetworkVariable<int> remoteRadarPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> rocketLauncherEnabled;
        public NetworkVariable<bool> rocketLauncherEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> rocketLauncherPrice;
        public NetworkVariable<int> rocketLauncherPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> hackingToolEnabled;
        public NetworkVariable<bool> hackingToolEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> hackingToolPrice;
        public NetworkVariable<int> hackingToolPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> flareGunEnabled;
        public NetworkVariable<bool> flareGunEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> flareGunPrice;
        public NetworkVariable<int> flareGunPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> flareGunAmmoPrice;
        public NetworkVariable<int> flareGunAmmoPriceNetVar = new NetworkVariable<int>(0);

        // Enemies, no need to sync weight
        public static ConfigEntry<int> boombaSpawnWeight;

        // Decor
        public static ConfigEntry<bool> rugsEnabled;
        public NetworkVariable<bool> rugsEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> smallRugPrice;
        public NetworkVariable<int> smallRugPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> largeRugPrice;
        public NetworkVariable<int> largeRugPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> fatalitiesSignEnabled;
        public NetworkVariable<bool> fatalitiesSignEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> fatalitiesSignPrice;
        public NetworkVariable<int> fatalitiesSignPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> dartBoardEnabled;
        public NetworkVariable<bool> dartBoardEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> dartBoardPrice;
        public NetworkVariable<int> dartBoardPriceNetVar = new NetworkVariable<int>(0);

        // Hazards
        public static ConfigEntry<bool> teleporterTrapsEnabled;
        public NetworkVariable<bool> teleporterTrapsEnabledNetVar = new NetworkVariable<bool>(true);

        // Misc
        public static ConfigEntry<bool> enableItemChargerElectrocution;
        public NetworkVariable<bool> enableItemChargerElectrocutionNetVar = new NetworkVariable<bool>(true);


        public static ConfigEntry<int> itemChargerElectrocutionDamage;

        public static ConfigEntry<bool> disableOverlappingModContent;
        public NetworkVariable<bool> disableOverlappingModContentNetVar = new NetworkVariable<bool>(true);


        // unsynced
        public static ConfigFile VolumeConfig;

        public static ConfigEntry<string> version;


        public static void Load()
        {

            
            arsonSpawnChance = Plugin.config.Bind<int>("Scrap", "Arson", 5, "How much does Arson spawn, higher = more common");
            dirtyArsonSpawnChance = Plugin.config.Bind<int>("Scrap", "DirtyArson", 5, "How much does Arson (Dirty) spawn, higher = more common");
            toimariSpawnChance = Plugin.config.Bind<int>("Scrap", "Toimari", 10, "How much does Toimari spawn, higher = more common");
            hamisSpawnChance = Plugin.config.Bind<int>("Scrap", "Hamis", 10, "How much does Hamis spawn, higher = more common");
            cookieSpawnChance = Plugin.config.Bind<int>("Scrap", "Cookie", 10, "How much does Cookie spawn, higher = more common");
            glizzySpawnChance = Plugin.config.Bind<int>("Scrap", "GlizzySpawnChance", 5, "How much do glizzies spawn, higher = more common");
            revolverSpawnChance = Plugin.config.Bind<int>("Scrap", "Revolver", 10, "How much do revolvers spawn, higher = more common");
            gremlinSodaSpawnChance = Plugin.config.Bind<int>("Scrap", "GremlinEnergyDrink", 10, "How much does Gremlin Energy Drink spawn, higher = more common");
            maxwellSpawnChance = Plugin.config.Bind<int>("Scrap", "Maxwell", 3, "How much does Maxwell spawn, higher = more common");
            evilMaxwellChance = Plugin.config.Bind<float>("Scrap", "MaxwellEvilChance", 10, "Chance for maxwell to be evil, percentage.");
            maxwellPlayMusicDefault = Plugin.config.Bind<bool>("Scrap", "MaxwellPlayMusicDefault", true, "Does Maxwell play music by default?");

            toyHammerEnabled = Plugin.config.Bind<bool>("Items", "ToyHammer", true, "Is Toy Hammer enabled?");
            toyHammerPrice = Plugin.config.Bind<int>("Items", "ToyHammerPrice", 15, "How much does Toy Hammer cost?");
            pouchyBeltEnabled = Plugin.config.Bind<bool>("Items", "PouchyBelt", true, "Is Pouchy Belt enabled?");
            pouchyBeltPrice = Plugin.config.Bind<int>("Items", "PouchyBeltPrice", 290, "How much does Pouchy Belt cost?");
            remoteRadarEnabled = Plugin.config.Bind<bool>("Items", "RemoteRadar", true, "Is Remote Radar enabled?");
            remoteRadarPrice = Plugin.config.Bind<int>("Items", "RemoteRadarPrice", 150, "How much does Remote Radar cost?");
            rocketLauncherEnabled = Plugin.config.Bind<bool>("Items", "RocketLauncher", true, "Is Rocket Launcher enabled?");
            rocketLauncherPrice = Plugin.config.Bind<int>("Items", "RocketLauncherPrice", 300, "How much does Rocket Launcher cost?");
            hackingToolEnabled = Plugin.config.Bind<bool>("Items", "HackingTool", true, "Is Hacking Tool enabled?");
            hackingToolPrice = Plugin.config.Bind<int>("Items", "HackingToolPrice", 120, "How much does Hacking Tool cost?");
            flareGunEnabled = Plugin.config.Bind<bool>("Items", "FlareGun", true, "Is Flare Gun enabled?");
            flareGunPrice = Plugin.config.Bind<int>("Items", "FlareGunPrice", 100, "How much does Flare Gun cost?");
            flareGunAmmoPrice = Plugin.config.Bind<int>("Items", "FlareGunAmmoPrice", 20, "How much does Flare Gun ammo cost?");


            boombaSpawnWeight = Plugin.config.Bind<int>("Enemies", "Boomba", 20, "How much does Boomba spawn, higher = more common");

            rugsEnabled = Plugin.config.Bind<bool>("Decor", "Rugs", true, "Are rugs enabled?");
            smallRugPrice = Plugin.config.Bind<int>("Decor", "SmallRugPrice", 80, "How much does a small rug cost?");
            largeRugPrice = Plugin.config.Bind<int>("Decor", "LargeRugPrice", 110, "How much does a large rug cost?");
            fatalitiesSignEnabled = Plugin.config.Bind<bool>("Decor", "FatalitiesSign", true, "Is Fatalities Sign enabled?");
            fatalitiesSignPrice = Plugin.config.Bind<int>("Decor", "FatalitiesSignPrice", 100, "How much does Fatalities Sign cost?");
            dartBoardEnabled = Plugin.config.Bind<bool>("Decor", "DartBoard", true, "Is Dart Board enabled?");
            dartBoardPrice = Plugin.config.Bind<int>("Decor", "DartBoardPrice", 120, "How much does Dart Board cost?");

            teleporterTrapsEnabled = Plugin.config.Bind<bool>("Traps", "TeleporterTraps", true, "Are teleporter traps enabled?");

            enableItemChargerElectrocution = Plugin.config.Bind<bool>("Misc", "EnableItemChargerElectrocution", true, "Do players get electrocuted when stuffing conductive objects in the item charger.");
            itemChargerElectrocutionDamage = Plugin.config.Bind<int>("Misc", "ItemChargerElectrocutionDamage", 20, "How much damage does the item charger electrocution do.");
            disableOverlappingModContent = Plugin.config.Bind<bool>("Misc", "DisableOverlappingModContent", true, "Disable content from other mods which exists in this one (e.g. maxwell).");





            version = Plugin.config.Bind<string>("Misc", "Version", "1.0.0", "Version of the mod config.");


            VolumeConfig = new ConfigFile(Paths.ConfigPath + "\\LethalThings.AudioVolume.cfg", true);

            On.StartOfRound.Start += StartOfRound_Start;
            On.StartOfRound.Awake += StartOfRound_Awake;
            
        }

        
        private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {
            Plugin.logger.LogInfo($"NetworkConfig: IsHost: {NetworkManager.Singleton.IsHost}");
            if (Instance == null && NetworkManager.Singleton.IsHost)
            {
                var configManager = Instantiate(Content.configManagerPrefab, self.transform.parent);
                configManager.GetComponent<NetworkObject>().Spawn();

                Plugin.logger.LogInfo("Spawning config manager");
            }
            orig(self);
        }
        
        
        private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
        {
            orig(self);


            foreach (var key in Content.ContentLoader.LoadedContent.Keys)
            {
                Plugin.logger.LogInfo(key);
            }

            Plugin.logger.LogInfo($"{Instance}");


            ShopItem toyHammer = Content.ContentLoader.LoadedContent["ToyHammer"] as ShopItem;
            if (!Instance.toyHammerEnabledNetVar.Value) toyHammer.RemoveFromShop();
            toyHammer.SetPrice(Instance.toyHammerPriceNetVar.Value);

            ShopItem pouchyBelt = Content.ContentLoader.LoadedContent["PouchyBelt"] as ShopItem;
            if (!Instance.pouchyBeltEnabledNetVar.Value) pouchyBelt.RemoveFromShop();
            pouchyBelt.SetPrice(Instance.pouchyBeltPriceNetVar.Value);

            ShopItem remoteRadar = Content.ContentLoader.LoadedContent["RemoteRadar"] as ShopItem;
            if (!Instance.remoteRadarEnabledNetVar.Value) remoteRadar.RemoveFromShop();
            remoteRadar.SetPrice(Instance.remoteRadarPriceNetVar.Value);

            ShopItem rocketLauncher = Content.ContentLoader.LoadedContent["RocketLauncher"] as ShopItem;
            if (!Instance.rocketLauncherEnabledNetVar.Value) rocketLauncher.RemoveFromShop();
            rocketLauncher.SetPrice(Instance.rocketLauncherPriceNetVar.Value);

            ShopItem hackingTool = Content.ContentLoader.LoadedContent["HackingTool"] as ShopItem;
            if (!Instance.hackingToolEnabledNetVar.Value) hackingTool.RemoveFromShop();
            hackingTool.SetPrice(Instance.hackingToolPriceNetVar.Value);

            ShopItem flareGun = Content.ContentLoader.LoadedContent["Flaregun"] as ShopItem;
            if (!Instance.flareGunEnabledNetVar.Value) flareGun.RemoveFromShop();
            flareGun.SetPrice(Instance.flareGunPriceNetVar.Value);

            ShopItem flareGunAmmo = Content.ContentLoader.LoadedContent["FlaregunAmmo"] as ShopItem;
            if (!Instance.flareGunEnabledNetVar.Value) flareGunAmmo.RemoveFromShop();
            flareGunAmmo.SetPrice(Instance.flareGunAmmoPriceNetVar.Value);

            Unlockable smallRug = Content.ContentLoader.LoadedContent["SmallRug"] as Unlockable;
            if (!Instance.rugsEnabledNetVar.Value) smallRug.RemoveFromShop();
            smallRug.SetPrice(Instance.smallRugPriceNetVar.Value);

            Unlockable largeRug = Content.ContentLoader.LoadedContent["LargeRug"] as Unlockable;
            if (!Instance.rugsEnabledNetVar.Value) largeRug.RemoveFromShop();
            largeRug.SetPrice(Instance.largeRugPriceNetVar.Value);

            Unlockable fatalitiesSign = Content.ContentLoader.LoadedContent["FatalitiesSign"] as Unlockable;
            if (!Instance.fatalitiesSignEnabledNetVar.Value) fatalitiesSign.RemoveFromShop();
            fatalitiesSign.SetPrice(Instance.fatalitiesSignPriceNetVar.Value);

            Unlockable dartBoard = Content.ContentLoader.LoadedContent["Dartboard"] as Unlockable;
            if (!Instance.dartBoardEnabledNetVar.Value) dartBoard.RemoveFromShop();
            dartBoard.SetPrice(Instance.dartBoardPriceNetVar.Value);

            MapHazard teleporterTrap = Content.ContentLoader.LoadedContent["TeleporterTrap"] as MapHazard;
            if (!Instance.teleporterTrapsEnabledNetVar.Value) teleporterTrap.RemoveFromLevels(Levels.LevelTypes.All);
           
        }
  
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                maxwellPlayMusicDefaultNetVar.Value = maxwellPlayMusicDefault.Value;

                toyHammerEnabledNetVar.Value = toyHammerEnabled.Value;
                toyHammerPriceNetVar.Value = toyHammerPrice.Value;
                pouchyBeltEnabledNetVar.Value = pouchyBeltEnabled.Value;
                pouchyBeltPriceNetVar.Value = pouchyBeltPrice.Value;
                remoteRadarEnabledNetVar.Value = remoteRadarEnabled.Value;
                remoteRadarPriceNetVar.Value = remoteRadarPrice.Value;
                rocketLauncherEnabledNetVar.Value = rocketLauncherEnabled.Value;
                rocketLauncherPriceNetVar.Value = rocketLauncherPrice.Value;
                hackingToolEnabledNetVar.Value = hackingToolEnabled.Value;
                hackingToolPriceNetVar.Value = hackingToolPrice.Value;
                flareGunEnabledNetVar.Value = flareGunEnabled.Value;
                flareGunPriceNetVar.Value = flareGunPrice.Value;
                flareGunAmmoPriceNetVar.Value = flareGunAmmoPrice.Value;

                rugsEnabledNetVar.Value = rugsEnabled.Value;
                smallRugPriceNetVar.Value = smallRugPrice.Value;
                largeRugPriceNetVar.Value = largeRugPrice.Value;
                fatalitiesSignEnabledNetVar.Value = fatalitiesSignEnabled.Value;
                fatalitiesSignPriceNetVar.Value = fatalitiesSignPrice.Value;
                dartBoardEnabledNetVar.Value = dartBoardEnabled.Value;
                dartBoardPriceNetVar.Value = dartBoardPrice.Value;

                teleporterTrapsEnabledNetVar.Value = teleporterTrapsEnabled.Value;

                enableItemChargerElectrocutionNetVar.Value = enableItemChargerElectrocution.Value;
                disableOverlappingModContentNetVar.Value = disableOverlappingModContent.Value;
            }
        }


    }
}
