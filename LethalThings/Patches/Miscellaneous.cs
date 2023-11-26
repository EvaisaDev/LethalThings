using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalThings.Patches
{
    public class Miscellaneous
    {
        public static void Load()
        {
            // Allow item to have no grab animation because the game is dumb
            On.GameNetcodeStuff.PlayerControllerB.SetSpecialGrabAnimationBool += PlayerControllerB_SetSpecialGrabAnimationBool;
        }

        private static void PlayerControllerB_SetSpecialGrabAnimationBool(On.GameNetcodeStuff.PlayerControllerB.orig_SetSpecialGrabAnimationBool orig, GameNetcodeStuff.PlayerControllerB self, bool setTrue, GrabbableObject currentItem)
        {
            if (currentItem == null)
            {
                currentItem = self.currentlyGrabbingObject;
            }
            if (currentItem != null && currentItem.itemProperties.grabAnim == "none")
            {
                Plugin.logger.LogInfo("Skipping grab animation because the item has no grab animation");
                return;
            }
            orig(self, setTrue, currentItem);
        }
    }
}
