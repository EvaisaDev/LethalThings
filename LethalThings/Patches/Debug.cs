using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalThings.Patches
{
    public class Debug
    {
        public static void Load()
        {
            On.StartOfRound.Update += StartOfRound_Update;
            On.RoundManager.Update += RoundManager_Update;
        }

        private static void RoundManager_Update(On.RoundManager.orig_Update orig, RoundManager self)
        {
            orig(self);
            
            if (Keyboard.current.f8Key.wasPressedThisFrame)
            {
                UnityEngine.Debug.Log("Attempting to spawn enemy from vent.");
                var vents = UnityEngine.Object.FindObjectsOfType<EnemyVent>();

                var position = StartOfRound.Instance.localPlayerController.gameplayCamera.transform.position;
                // closest vent
                EnemyVent vent = null;

                foreach (var v in vents)
                {
                    if (vent == null)
                    {
                        vent = v;
                        continue;
                    }

                    if (Vector3.Distance(position, v.transform.position) < Vector3.Distance(position, vent.transform.position))
                    {
                        vent = v;
                    }
                }

                vent.enemyType = Content.Prefabs["Boomba"].GetComponent<RoombaAI>().enemyType;
                vent.enemyTypeIndex = self.currentLevel.Enemies.FindIndex(x => x.enemyType == vent.enemyType);

                UnityEngine.Debug.Log($"Spawning enemy from vent {vent.name}.");


                self.SpawnEnemyFromVent(vent);
    
            }
        }

        private static void StartOfRound_Update(On.StartOfRound.orig_Update orig, StartOfRound self)
        {
            
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Arson", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F2].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Cookie", self.localPlayerController.gameplayCamera.transform.position);
            }   
            if (Keyboard.current[Key.F3].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Bilka", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F4].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Hamis", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F5].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("ArsonDirty", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F6].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Maxwell", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F8].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Glizzy", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F9].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("RocketLauncher", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F10].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("ToyHammer", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F11].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("RemoteRadar", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F12].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("PouchyBelt", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.Numpad0].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("HackingTool", self.localPlayerController.gameplayCamera.transform.position);
            }
            orig(self);
        }
    }
}
