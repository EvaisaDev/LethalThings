using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalThings
{
    public static class InputCompat
    {
        public static bool Enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.LethalCompanyInputUtils");

        public static InputActionAsset Asset;

        public static void Init()
        {
            Keybinds.Instance = new Keybinds();
            Asset = Keybinds.Instance.GetAsset();
            
        }

        public static InputAction LTUtilityBeltQuick1 => Keybinds.Instance.LTUtilityBeltQuick1;

        public static InputAction LTUtilityBeltQuick2 => Keybinds.Instance.LTUtilityBeltQuick2;

        public static InputAction LTUtilityBeltQuick3 => Keybinds.Instance.LTUtilityBeltQuick3;

        public static InputAction LTUtilityBeltQuick4 => Keybinds.Instance.LTUtilityBeltQuick4;

        
    }


    public class Keybinds : LcInputActions
    {
        [InputAction("", Name = "[LT] Utility Belt Quick 1")]
        public InputAction LTUtilityBeltQuick1 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 2")]
        public InputAction LTUtilityBeltQuick2 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 3")]
        public InputAction LTUtilityBeltQuick3 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 4")]
        public InputAction LTUtilityBeltQuick4 { get; set; }

        public static Keybinds Instance;

        public InputActionAsset GetAsset() => Asset;
    }
}
