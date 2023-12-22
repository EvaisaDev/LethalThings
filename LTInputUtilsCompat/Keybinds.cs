using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace LTInputUtilsCompat
{
    public class Keybinds : LcInputActions
    {
        public Keybinds() : base() { }

        public static Keybinds Instance;

        [InputAction("", Name = "[LT] Utility Belt Quick 1")]
        public InputAction LTUtilityBeltQuick1 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 2")]
        public InputAction LTUtilityBeltQuick2 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 3")]
        public InputAction LTUtilityBeltQuick3 { get; set; }
        [InputAction("", Name = "[LT] Utility Belt Quick 4")]
        public InputAction LTUtilityBeltQuick4 { get; set; }
    }
}
                                                                