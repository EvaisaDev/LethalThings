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
            FatalitiesSign.Init();
            GremlinEnergy.Init();
            Arson.Init();
            MaggieSpawner.Init();
            ForcedPing.Init();
            ThrowableNoisemaker.Init();

            Debug.Load();
        }
    }
}
