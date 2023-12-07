using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalThings
{
    public class Config
    {
        public static ConfigEntry<int> arsonSpawnWeight;
        public static ConfigEntry<int> dirtyArsonSpawnWeight;
        public static ConfigEntry<int> toimariSpawnWeight;
        public static ConfigEntry<int> hamisSpawnWeight;
        public static ConfigEntry<int> cookieSpawnWeight;
        public static ConfigEntry<int> maxwellSpawnWeight;
        public static ConfigEntry<bool> maxwellPlayMusicDefault;

        public static ConfigEntry<bool> toyHammerEnabled;
        public static ConfigEntry<int> toyHammerPrice;
        public static ConfigEntry<bool> pouchyBeltEnabled;
        public static ConfigEntry<int> pouchyBeltPrice;
        public static ConfigEntry<bool> remoteRadarEnabled;
        public static ConfigEntry<int> remoteRadarPrice;
        public static ConfigEntry<bool> rocketLauncherEnabled;
        public static ConfigEntry<int> rocketLauncherPrice;
        public static ConfigEntry<bool> hackingToolEnabled;
        public static ConfigEntry<int> hackingToolPrice;

        public static ConfigEntry<int> boombaSpawnWeight;

        public static ConfigEntry<bool> rugsEnabled;
        public static ConfigEntry<int> smallRugPrice;
        public static ConfigEntry<int> largeRugPrice;
        public static ConfigEntry<bool> fatalitiesSignEnabled;
        public static ConfigEntry<int> fatalitiesSignPrice;

        public static ConfigEntry<bool> teleporterTrapsEnabled;

        public static ConfigEntry<bool> enableItemChargerElectrocution;
        public static ConfigEntry<int> itemChargerElectrocutionDamage;
        public static ConfigEntry<bool> disableOverlappingModContent;

        public static void Load()
        {
            arsonSpawnWeight = Plugin.config.Bind<int>("Scrap", "Arson", 10, "How much does Arson spawn, higher = more common");
            dirtyArsonSpawnWeight = Plugin.config.Bind<int>("Scrap", "DirtyArson", 10, "How much does Arson (Dirty) spawn, higher = more common");
            toimariSpawnWeight = Plugin.config.Bind<int>("Scrap", "Toimari", 20, "How much does Toimari spawn, higher = more common");
            hamisSpawnWeight = Plugin.config.Bind<int>("Scrap", "Hamis", 20, "How much does Hamis spawn, higher = more common");
            cookieSpawnWeight = Plugin.config.Bind<int>("Scrap", "Cookie", 20, "How much does Cookie spawn, higher = more common");
            maxwellSpawnWeight = Plugin.config.Bind<int>("Scrap", "Maxwell", 3, "How much does Maxwell spawn, higher = more common");
            maxwellPlayMusicDefault = Plugin.config.Bind<bool>("Scrap", "MaxwellPlayMusicDefault", true, "Does Maxwell play music by default?");


            toyHammerEnabled = Plugin.config.Bind<bool>("Items", "ToyHammer", true, "Is Toy Hammer enabled?");
            toyHammerPrice = Plugin.config.Bind<int>("Items", "ToyHammerPrice", 80, "How much does Toy Hammer cost?");
            pouchyBeltEnabled = Plugin.config.Bind<bool>("Items", "PouchyBelt", true, "Is Pouchy Belt enabled?");
            pouchyBeltPrice = Plugin.config.Bind<int>("Items", "PouchyBeltPrice", 290, "How much does Pouchy Belt cost?");
            remoteRadarEnabled = Plugin.config.Bind<bool>("Items", "RemoteRadar", true, "Is Remote Radar enabled?");
            remoteRadarPrice = Plugin.config.Bind<int>("Items", "RemoteRadarPrice", 240, "How much does Remote Radar cost?");
            rocketLauncherEnabled = Plugin.config.Bind<bool>("Items", "RocketLauncher", true, "Is Rocket Launcher enabled?");
            rocketLauncherPrice = Plugin.config.Bind<int>("Items", "RocketLauncherPrice", 260, "How much does Rocket Launcher cost?");
            hackingToolEnabled = Plugin.config.Bind<bool>("Items", "HackingTool", true, "Is Hacking Tool enabled?");
            hackingToolPrice = Plugin.config.Bind<int>("Items", "HackingToolPrice", 190, "How much does Hacking Tool cost?");

            boombaSpawnWeight = Plugin.config.Bind<int>("Enemies", "Boomba", 20, "How much does Boomba spawn, higher = more common");

            rugsEnabled = Plugin.config.Bind<bool>("Decor", "Rugs", true, "Are rugs enabled?");
            smallRugPrice = Plugin.config.Bind<int>("Decor", "SmallRugPrice", 80, "How much does a small rug cost?");
            largeRugPrice = Plugin.config.Bind<int>("Decor", "LargeRugPrice", 110, "How much does a large rug cost?");
            fatalitiesSignEnabled = Plugin.config.Bind<bool>("Decor", "FatalitiesSign", true, "Is Fatalities Sign enabled?");
            fatalitiesSignPrice = Plugin.config.Bind<int>("Decor", "FatalitiesSignPrice", 100, "How much does Fatalities Sign cost?");

            teleporterTrapsEnabled = Plugin.config.Bind<bool>("Traps", "TeleporterTraps", true, "Are teleporter traps enabled?");

            enableItemChargerElectrocution = Plugin.config.Bind<bool>("Misc", "EnableItemChargerElectrocution", true, "Do players get electrocuted when stuffing conductive objects in the item charger.");
            itemChargerElectrocutionDamage = Plugin.config.Bind<int>("Misc", "ItemChargerElectrocutionDamage", 20, "How much damage does the item charger electrocution do.");
            disableOverlappingModContent = Plugin.config.Bind<bool>("Misc", "DisableOverlappingModContent", true, "Disable content from other mods which exists in this one (e.g. maxwell).");

        }
    }
}
