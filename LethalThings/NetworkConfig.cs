﻿using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
//using static LethalLib.Modules.ContentLoader;
using LethalLib.Modules;
using UnityEngine;
using static LethalLib.Modules.ContentLoader;
using System.Collections;

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
        public static ConfigEntry<int> gnarpySpawnChance;

        public static ConfigEntry<float> evilMaxwellChance;

        // special case that needs sync
        public static ConfigEntry<bool> maxwellPlayMusicDefault;
        [HideInInspector]
        public NetworkVariable<bool> maxwellPlayMusicDefaultNetVar = new NetworkVariable<bool>(true);

        // Shop items
        public static ConfigEntry<bool> toyHammerIsScrap;
        [HideInInspector]
        public NetworkVariable<bool> toyHammerIsScrapNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<bool> toyHammerInShop;
        [HideInInspector]
        public NetworkVariable<bool> toyHammerInShopNetVar = new NetworkVariable<bool>(false);

        public static ConfigEntry<int> toyHammerPrice;
        [HideInInspector]
        public NetworkVariable<int> toyHammerPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> toyHammerScrapSpawnChance;

        public static ConfigEntry<bool> pouchyBeltEnabled;
        [HideInInspector]
        public NetworkVariable<bool> pouchyBeltEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> pouchyBeltPrice;
        [HideInInspector]
        public NetworkVariable<int> pouchyBeltPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> remoteRadarEnabled;
        [HideInInspector]
        public NetworkVariable<bool> remoteRadarEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> remoteRadarPrice;
        [HideInInspector]
        public NetworkVariable<int> remoteRadarPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> rocketLauncherEnabled;
        [HideInInspector]
        public NetworkVariable<bool> rocketLauncherEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> rocketLauncherPrice;
        [HideInInspector]
        public NetworkVariable<int> rocketLauncherPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> hackingToolEnabled;
        [HideInInspector]
        public NetworkVariable<bool> hackingToolEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> hackingToolPrice;
        [HideInInspector]
        public NetworkVariable<int> hackingToolPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> flareGunEnabled;
        [HideInInspector]
        public NetworkVariable<bool> flareGunEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> flareGunPrice;
        [HideInInspector]
        public NetworkVariable<int> flareGunPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> flareGunAmmoPrice;
        [HideInInspector]
        public NetworkVariable<int> flareGunAmmoPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> pingerPrice;
        [HideInInspector]
        public NetworkVariable<int> pingerPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> pingerEnabled;
        [HideInInspector]
        public NetworkVariable<bool> pingerEnabledNetVar = new NetworkVariable<bool>(true);

        // Enemies, no need to sync weight
        public static ConfigEntry<int> boombaSpawnWeight;
        public static ConfigEntry<int> maggieSpawnWeight;
        public static ConfigEntry<int> crystalRaySpawnWeight;

        public static ConfigEntry<float> maggieTeleporterChance;
        [HideInInspector]
        public NetworkVariable<float> maggieTeleporterChanceNetVar = new NetworkVariable<float>(0.3f);

        // Decor
        public static ConfigEntry<bool> rugsEnabled;
        [HideInInspector]
        public NetworkVariable<bool> rugsEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> smallRugPrice;
        [HideInInspector]
        public NetworkVariable<int> smallRugPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<int> largeRugPrice;
        [HideInInspector]
        public NetworkVariable<int> largeRugPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> fatalitiesSignEnabled;
        [HideInInspector]
        public NetworkVariable<bool> fatalitiesSignEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> fatalitiesSignPrice;
        [HideInInspector]
        public NetworkVariable<int> fatalitiesSignPriceNetVar = new NetworkVariable<int>(0);

        public static ConfigEntry<bool> dartBoardEnabled;
        [HideInInspector]
        public NetworkVariable<bool> dartBoardEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> dartBoardPrice;
        [HideInInspector]
        public NetworkVariable<int> dartBoardPriceNetVar = new NetworkVariable<int>(0);

        /*public static ConfigEntry<bool> deliveryRoverEnabled;
        [HideInInspector]
        public NetworkVariable<bool> deliveryRoverEnabledNetVar = new NetworkVariable<bool>(true);

        public static ConfigEntry<int> deliveryRoverPrice;
        [HideInInspector]
        public NetworkVariable<int> deliveryRoverPriceNetVar = new NetworkVariable<int>(0);*/


        // Hazards
        public static ConfigEntry<bool> teleporterTrapsEnabled;
        [HideInInspector]
        public NetworkVariable<bool> teleporterTrapsEnabledNetVar = new NetworkVariable<bool>(true);

        // Misc
        public static ConfigEntry<bool> enableItemChargerElectrocution;
        [HideInInspector]
        public NetworkVariable<bool> enableItemChargerElectrocutionNetVar = new NetworkVariable<bool>(true);


        public static ConfigEntry<int> itemChargerElectrocutionDamage;

        public static ConfigEntry<bool> disableOverlappingModContent;
        [HideInInspector]
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
            gnarpySpawnChance = Plugin.config.Bind<int>("Scrap", "Gnarpy", 10, "How much does Gnarpy spawn, higher = more common");
            maxwellSpawnChance = Plugin.config.Bind<int>("Scrap", "Maxwell", 3, "How much does Maxwell spawn, higher = more common");
            evilMaxwellChance = Plugin.config.Bind<float>("Scrap", "MaxwellEvilChance", 10, "Chance for maxwell to be evil, percentage.");
            maxwellPlayMusicDefault = Plugin.config.Bind<bool>("Scrap", "MaxwellPlayMusicDefault", true, "Does Maxwell play music by default?");

            toyHammerIsScrap = Plugin.config.Bind<bool>("Items", "ToyHammerIsScrap", true, "Does the Toy Hammer spawn as a scrap item?");
            toyHammerInShop = Plugin.config.Bind<bool>("Items", "ToyHammerInShop", false, "Is the Toy Hammer in the shop?");
            toyHammerPrice = Plugin.config.Bind<int>("Items", "ToyHammerPrice", 15, "How much do Toy Hammers cost?");
            toyHammerScrapSpawnChance = Plugin.config.Bind<int>("Items", "ToyHammerScrapSpawnChance", 10, "How much does the Toy Hammer spawn as scrap, higher = more common");

            pouchyBeltEnabled = Plugin.config.Bind<bool>("Items", "PouchyBelt", true, "Is the Utility Belt enabled?");
            pouchyBeltPrice = Plugin.config.Bind<int>("Items", "PouchyBeltPrice", 290, "How much do Utility Belts cost?");
            remoteRadarEnabled = Plugin.config.Bind<bool>("Items", "RemoteRadar", true, "Is the Remote Radar enabled?");
            remoteRadarPrice = Plugin.config.Bind<int>("Items", "RemoteRadarPrice", 150, "How much do Remote Radars cost?");
            rocketLauncherEnabled = Plugin.config.Bind<bool>("Items", "RocketLauncher", true, "Is the Rocket Launcher enabled?");
            rocketLauncherPrice = Plugin.config.Bind<int>("Items", "RocketLauncherPrice", 300, "How much do Rocket Launchers cost?");
            hackingToolEnabled = Plugin.config.Bind<bool>("Items", "HackingTool", true, "Is the Hacking Tool enabled?");
            hackingToolPrice = Plugin.config.Bind<int>("Items", "HackingToolPrice", 120, "How much do Hacking Tools cost?");
            flareGunEnabled = Plugin.config.Bind<bool>("Items", "FlareGun", true, "Is Flare Gun enabled?");
            flareGunPrice = Plugin.config.Bind<int>("Items", "FlareGunPrice", 100, "How much do Flare Guns cost?");
            flareGunAmmoPrice = Plugin.config.Bind<int>("Items", "FlareGunAmmoPrice", 20, "How much does Flare Gun ammo cost?");
            pingerEnabled = Plugin.config.Bind<bool>("Items", "Pinger", true, "Is the Pinger enabled?");
            pingerPrice = Plugin.config.Bind<int>("Items", "PingerPrice", 100, "How much do Pingers cost?");


            boombaSpawnWeight = Plugin.config.Bind<int>("Enemies", "Boomba", 20, "How much do Boombas spawn, higher = more common");
            maggieSpawnWeight = Plugin.config.Bind<int>("Enemies", "Maggie", 0, "How much does Maggie spawn, higher = more common");
            maggieTeleporterChance = Plugin.config.Bind<float>("Enemies", "MaggieTeleporterChance", 0f, "Chance for players to become lost in transit, percentage.");
            crystalRaySpawnWeight = Plugin.config.Bind<int>("Enemies", "CrystalRay", 0, "How much do Crystal Rays spawn, higher = more common");

            rugsEnabled = Plugin.config.Bind<bool>("Decor", "Rugs", true, "Are rugs enabled?");
            smallRugPrice = Plugin.config.Bind<int>("Decor", "SmallRugPrice", 80, "How much does a small rug cost?");
            largeRugPrice = Plugin.config.Bind<int>("Decor", "LargeRugPrice", 110, "How much does a large rug cost?");
            fatalitiesSignEnabled = Plugin.config.Bind<bool>("Decor", "FatalitiesSign", true, "Is the Fatalities Sign enabled?");
            fatalitiesSignPrice = Plugin.config.Bind<int>("Decor", "FatalitiesSignPrice", 100, "How much does the Fatalities Sign cost?");
            dartBoardEnabled = Plugin.config.Bind<bool>("Decor", "DartBoard", true, "Is the Dart Board enabled?");
            dartBoardPrice = Plugin.config.Bind<int>("Decor", "DartBoardPrice", 120, "How much does the Dart Board cost?");
            //deliveryRoverEnabled = Plugin.config.Bind<bool>("Decor", "DeliveryRover", true, "Is the Delivery Rover enabled?");
            //deliveryRoverPrice = Plugin.config.Bind<int>("Decor", "DeliveryRoverPrice", 600, "How much does the Delivery Rover cost?");

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
            //Plugin.logger.LogInfo($"NetworkConfig: IsHost: {NetworkManager.Singleton.IsHost}");
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

            Instance.StartCoroutine(Instance.SyncConfigs());
        }

        private IEnumerator SyncConfigs()
        {
            // make sure network initialized
            yield return new WaitUntil(() => networkSpawned);
            yield return new WaitForSeconds(3);

            Plugin.logger.LogInfo("Syncing configs!");

            ShopItem toyHammer = Content.ContentLoader.LoadedContent["ToyHammerShop"] as ShopItem;
            toyHammer.SetPrice(Instance.toyHammerPriceNetVar.Value);
            if (!Instance.toyHammerInShopNetVar.Value) toyHammer.RemoveFromShop();

            ScrapItem toyHammerScrap = Content.ContentLoader.LoadedContent["ToyHammerScrap"] as ScrapItem;
            if (!Instance.toyHammerIsScrapNetVar.Value) toyHammerScrap.RemoveFromLevels(Levels.LevelTypes.All);

            ShopItem pouchyBelt = Content.ContentLoader.LoadedContent["PouchyBelt"] as ShopItem;
            pouchyBelt.SetPrice(Instance.pouchyBeltPriceNetVar.Value);
            if (!Instance.pouchyBeltEnabledNetVar.Value) pouchyBelt.RemoveFromShop();

            ShopItem remoteRadar = Content.ContentLoader.LoadedContent["RemoteRadar"] as ShopItem;
            remoteRadar.SetPrice(Instance.remoteRadarPriceNetVar.Value);
            if (!Instance.remoteRadarEnabledNetVar.Value) remoteRadar.RemoveFromShop();

            ShopItem rocketLauncher = Content.ContentLoader.LoadedContent["RocketLauncher"] as ShopItem;
            rocketLauncher.SetPrice(Instance.rocketLauncherPriceNetVar.Value);
            if (!Instance.rocketLauncherEnabledNetVar.Value) rocketLauncher.RemoveFromShop();

            ShopItem hackingTool = Content.ContentLoader.LoadedContent["HackingTool"] as ShopItem;
            hackingTool.SetPrice(Instance.hackingToolPriceNetVar.Value);
            if (!Instance.hackingToolEnabledNetVar.Value) hackingTool.RemoveFromShop();

            ShopItem flareGun = Content.ContentLoader.LoadedContent["Flaregun"] as ShopItem;
            flareGun.SetPrice(Instance.flareGunPriceNetVar.Value);
            if (!Instance.flareGunEnabledNetVar.Value) flareGun.RemoveFromShop();

            ShopItem flareGunAmmo = Content.ContentLoader.LoadedContent["FlaregunAmmo"] as ShopItem;
            flareGunAmmo.SetPrice(Instance.flareGunAmmoPriceNetVar.Value);
            if (!Instance.flareGunEnabledNetVar.Value) flareGunAmmo.RemoveFromShop();

            ShopItem pinger = Content.ContentLoader.LoadedContent["Pinger"] as ShopItem;
            pinger.SetPrice(Instance.pingerPriceNetVar.Value);
            if (!Instance.pingerEnabledNetVar.Value) pinger.RemoveFromShop();

            Unlockable smallRug = Content.ContentLoader.LoadedContent["SmallRug"] as Unlockable;
            smallRug.SetPrice(Instance.smallRugPriceNetVar.Value);
            if (!Instance.rugsEnabledNetVar.Value) smallRug.RemoveFromShop();

            Unlockable largeRug = Content.ContentLoader.LoadedContent["LargeRug"] as Unlockable;
            largeRug.SetPrice(Instance.largeRugPriceNetVar.Value);
            if (!Instance.rugsEnabledNetVar.Value) largeRug.RemoveFromShop();

            Unlockable fatalitiesSign = Content.ContentLoader.LoadedContent["FatalitiesSign"] as Unlockable;
            fatalitiesSign.SetPrice(Instance.fatalitiesSignPriceNetVar.Value);
            if (!Instance.fatalitiesSignEnabledNetVar.Value) fatalitiesSign.RemoveFromShop();

            Unlockable dartBoard = Content.ContentLoader.LoadedContent["Dartboard"] as Unlockable;
            dartBoard.SetPrice(Instance.dartBoardPriceNetVar.Value);
            if (!Instance.dartBoardEnabledNetVar.Value) dartBoard.RemoveFromShop();

            //Unlockable deliveryRover = Content.ContentLoader.LoadedContent["DeliveryRover"] as Unlockable;
            //deliveryRover.SetPrice(Instance.deliveryRoverPriceNetVar.Value);
            //if (!Instance.deliveryRoverEnabledNetVar.Value) deliveryRover.RemoveFromShop();

            MapHazard teleporterTrap = Content.ContentLoader.LoadedContent["TeleporterTrap"] as MapHazard;
            if (!Instance.teleporterTrapsEnabledNetVar.Value) teleporterTrap.RemoveFromLevels(Levels.LevelTypes.All);
        }

        public bool networkSpawned = false;
  
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            networkSpawned = true;

            if (IsHost)
            {
                maxwellPlayMusicDefaultNetVar.Value = maxwellPlayMusicDefault.Value;

                toyHammerPriceNetVar.Value = toyHammerPrice.Value;
                toyHammerIsScrapNetVar.Value = toyHammerIsScrap.Value;
                toyHammerInShopNetVar.Value = toyHammerInShop.Value;
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
                pingerEnabledNetVar.Value = pingerEnabled.Value;
                pingerPriceNetVar.Value = pingerPrice.Value;

                rugsEnabledNetVar.Value = rugsEnabled.Value;
                smallRugPriceNetVar.Value = smallRugPrice.Value;
                largeRugPriceNetVar.Value = largeRugPrice.Value;
                fatalitiesSignEnabledNetVar.Value = fatalitiesSignEnabled.Value;
                fatalitiesSignPriceNetVar.Value = fatalitiesSignPrice.Value;
                dartBoardEnabledNetVar.Value = dartBoardEnabled.Value;
                dartBoardPriceNetVar.Value = dartBoardPrice.Value;
                //deliveryRoverEnabledNetVar.Value = deliveryRoverEnabled.Value;
                //deliveryRoverPriceNetVar.Value = deliveryRoverPrice.Value;

                teleporterTrapsEnabledNetVar.Value = teleporterTrapsEnabled.Value;

                enableItemChargerElectrocutionNetVar.Value = enableItemChargerElectrocution.Value;
                disableOverlappingModContentNetVar.Value = disableOverlappingModContent.Value;

                maggieTeleporterChanceNetVar.Value = maggieTeleporterChance.Value;
            }
        }


    }
}
