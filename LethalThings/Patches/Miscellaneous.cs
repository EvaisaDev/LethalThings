using BepInEx.Logging;
using LethalThings.MonoBehaviours;
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
            if (Config.disableOverlappingModContent.Value)
            {
                On.StartOfRound.Start += StartOfRound_Start;
            }
        }

        private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
        {
            orig(self);

            // remove other dingi, so people stop complaining about non eba dingusses

            foreach (SelectableLevel level in self.levels)
            {
                level.spawnableScrap.RemoveAll((scrap) => scrap.spawnableItem.name == "dingus");
            }

            self.allItemsList.itemsList.RemoveAll((item) => item.name == "dingus");
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
