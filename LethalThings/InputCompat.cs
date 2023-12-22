using LTInputUtilsCompat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LethalThings
{
    public class InputCompat
    {
        public static object KeybindsInstance;
        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils"))
            {
                Assembly assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LTInputUtilsCompat.dll"));

                Type type = assembly.GetType("LTInputUtilsCompat.Keybinds");

                KeybindsInstance = Activator.CreateInstance(type);

                Plugin.logger.LogInfo("LTInputUtilsCompat loaded");
            }
        }
    }
}
