﻿using LethalThings.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalThings.Patches
{
    public class Debug
    {
        public static void Load()
        {
            //On.StartOfRound.Update += StartOfRound_Update;
            //On.RoundManager.Update += RoundManager_Update;
            On.StartOfRound.Start += StartOfRound_Start;
        }

        private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
        {
            orig(self);

            if(Plugin.devMode && DevMenu.Instance == null)
            {
                var gameObject = GameObject.Instantiate(Content.devMenuPrefab);
                // spawn network object
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }

        private static void RoundManager_Update(On.RoundManager.orig_Update orig, RoundManager self)
        {
            orig(self);
            
            /*if (Keyboard.current.f8Key.wasPressedThisFrame)
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
    
            }*/
        }

        private static void StartOfRound_Update(On.StartOfRound.orig_Update orig, StartOfRound self)
        {
            
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Flaregun", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F2].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("FlaregunAmmo", self.localPlayerController.gameplayCamera.transform.position);
            }   
            if (Keyboard.current[Key.F3].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("RocketLauncher", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F4].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("Maxwell", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.F5].wasPressedThisFrame)
            {
                Utilities.LoadPrefab("PouchyBelt", self.localPlayerController.gameplayCamera.transform.position);
            }
            if (Keyboard.current[Key.Numpad0].wasPressedThisFrame)
            {
                // goofy ahh player kill
                HUDManager.Instance.localPlayer.KillPlayer(Vector3.zero, spawnBody: false, CauseOfDeath.Crushing);
            }
            orig(self);
        }
    }
}
