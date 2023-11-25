﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LethalThings.Patches
{
    public class Patches
    {
        public static void Load()
        {
            PowerOutletStun.Load();
            Miscellaneous.Load();
            PouchyBelt.Initialize();

            // i will surely not forget to remove this when i release.
            Debug.Load();
        }
    }
}
