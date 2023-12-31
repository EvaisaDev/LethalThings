using LethalLib.Modules;
using LethalThings.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalThings.Patches
{
    public class Patches
    {
        public static void Load()
        {
            SaveData.Init();
            PowerOutletStun.Load();
            Miscellaneous.Load();
            PouchyBelt.Initialize();
            HandheldRadar.Load();
            HackingTool.Load();
            FlareController.Init();
            DecalRandomizer.Init();
            Dart.Init();
            FatalitiesSign.Init();
            GremlinEnergy.Init();
            Arson.Init();
            MaggieSpawner.Init();

            // i will surely not forget to remove this when i release.
            Debug.Load();
        }
    }
}
