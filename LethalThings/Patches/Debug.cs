using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace LethalThings.Patches
{
    public class Debug
    {
        public static void Load()
        {
            On.StartOfRound.Update += StartOfRound_Update;
        }

        private static void StartOfRound_Update(On.StartOfRound.orig_Update orig, StartOfRound self)
        {
            
            if (Keyboard.current.f8Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("RocketLauncher", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Arson", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f10Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Cookie", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f11Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Bilka", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Hamis", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("ArsonDirty", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("ToyHammer", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Maxwell", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("PouchyBelt", self.localPlayerController.gameplayCamera.transform.position);
            }
            else if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                Utilities.LoadPrefab("RemoteRadar", self.localPlayerController.gameplayCamera.transform.position);
            }
            orig(self);
        }
    }
}
