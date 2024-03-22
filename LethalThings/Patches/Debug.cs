using LethalThings.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace LethalThings.Patches
{
    public class Debug
    {
        public static void Load()
        {
            On.StartOfRound.Update += StartOfRound_Update;
            On.RoundManager.Update += RoundManager_Update;
            On.StartOfRound.Start += StartOfRound_Start;
           // On.ShipBuildModeManager.Update += ShipBuildModeManager_Update;
        }

        private static void ShipBuildModeManager_Update(On.ShipBuildModeManager.orig_Update orig, ShipBuildModeManager self)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            self.player = GameNetworkManager.Instance.localPlayerController;
            if (!self.PlayerMeetsConditionsToBuild(log: false))
            {
                self.CancelBuildMode();
            }
            if (self.placingObject == null)
            {
                self.CancelBuildMode();
            }
            if (self.InBuildMode)
            {
                if (self.currentCollider == null)
                {
                    self.currentCollider = self.placingObject.placeObjectCollider as BoxCollider;
                }
                if (IngamePlayerSettings.Instance.playerInput.actions.FindAction("ReloadBatteries").IsPressed() || (StartOfRound.Instance.localPlayerUsingController && self.playerActions.Movement.InspectItem.IsPressed()))
                {
                    self.ghostObject.eulerAngles = new Vector3(self.ghostObject.eulerAngles.x, self.ghostObject.eulerAngles.y + Time.deltaTime * 155f, self.ghostObject.eulerAngles.z);
                }
                self.playerCameraRay = new Ray(self.player.gameplayCamera.transform.position, self.player.gameplayCamera.transform.forward);
                if (Physics.Raycast(self.playerCameraRay, out self.rayHit, 4f, self.placementMask, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Angle(self.rayHit.normal, Vector3.up) < 45f)
                    {
                        self.ghostObject.position = self.rayHit.point + Vector3.up * self.placingObject.yOffset;
                    }
                    else if (self.placingObject.AllowPlacementOnWalls)
                    {
                        self.ghostObject.position = self.OffsetObjectFromWallBasedOnDimensions(self.rayHit.point, self.rayHit);
                        if (Physics.Raycast(self.ghostObject.position, Vector3.down, out self.rayHit, self.placingObject.yOffset, self.placementMask, QueryTriggerInteraction.Ignore))
                        {
                            self.ghostObject.position += Vector3.up * self.rayHit.distance;
                        }
                    }
                    else if (Physics.Raycast(self.OffsetObjectFromWallBasedOnDimensions(self.rayHit.point, self.rayHit), Vector3.down, out self.rayHit, 20f, self.placementMask, QueryTriggerInteraction.Ignore))
                    {
                        self.ghostObject.position = self.rayHit.point + Vector3.up * self.placingObject.yOffset;
                    }
                }
                else if (Physics.Raycast(self.playerCameraRay.GetPoint(4f), Vector3.down, out self.rayHit, 20f, self.placementMask, QueryTriggerInteraction.Ignore))
                {
                    self.ghostObject.position = self.rayHit.point + Vector3.up * self.placingObject.yOffset;
                }
                bool flag = Physics.CheckBox(self.ghostObject.position, self.currentCollider.size * 0.5f * 0.57f, Quaternion.Euler(self.ghostObject.eulerAngles), self.placementMaskAndBlockers, QueryTriggerInteraction.Ignore);
                if (!flag && self.placingObject.doCollisionPointCheck)
                {
                    Vector3 vector = self.ghostObject.position + self.ghostObject.forward * self.placingObject.collisionPointCheck.z + self.ghostObject.right * self.placingObject.collisionPointCheck.x + self.ghostObject.up * self.placingObject.collisionPointCheck.y;
                    UnityEngine.Debug.DrawRay(vector, Vector3.up * 2f, Color.blue);
                    if (Physics.CheckSphere(vector, 1f, self.placementMaskAndBlockers, QueryTriggerInteraction.Ignore))
                    {
                        flag = true;
                    }
                }
                self.CanConfirmPosition = !flag && StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(self.ghostObject.position);
                if (flag)
                {
                    self.ghostObjectRenderer.sharedMaterial = self.ghostObjectRed;

                    var colliders = Physics.OverlapBox(self.ghostObject.position, self.currentCollider.size * 0.5f * 0.57f, Quaternion.Euler(self.ghostObject.eulerAngles), self.placementMaskAndBlockers, QueryTriggerInteraction.Ignore);

                    foreach (var collider in colliders)
                    {
                        UnityEngine.Debug.Log($"Collider: {collider.gameObject.name}");
                    }
                }
                else
                {
                    self.ghostObjectRenderer.sharedMaterial = self.ghostObjectGreen;
                }
            }
            else
            {
                self.timeSincePlacingObject += Time.deltaTime;
            }
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


            
            /*if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                var ray = Content.Prefabs["CrystalRay"];

                var gameObject = GameObject.Instantiate(ray, self.playersManager.localPlayerController.gameplayCamera.transform.position, Quaternion.identity);

                gameObject.GetComponent<NetworkObject>().Spawn();

            }*/
        }

        static List<Key> currentCheatCode = new List<Key>();

        static List<Key> devModeCode = new List<Key>()
        {
            Key.UpArrow,
            Key.UpArrow,
            Key.DownArrow,
            Key.DownArrow,
            Key.LeftArrow,
            Key.RightArrow,
            Key.LeftArrow,
            Key.RightArrow,
            Key.B,
            Key.A,
            Key.Enter
        };

        private static void StartOfRound_Update(On.StartOfRound.orig_Update orig, StartOfRound self)
        {
            // cheat code for enabling dev menu

            if (self.IsHost && DevMenu.Instance == null)
            {
                foreach (var key in Keyboard.current.allKeys)
                {
                    if (key.wasReleasedThisFrame)
                    {
                        currentCheatCode.Add(key.keyCode);

                        // print current code
                        /*var codeString = "";
                        foreach (var k in currentCheatCode)
                        {
                            codeString += k.ToString() + " ";
                            
                        }*/

                        //Plugin.logger.LogMessage(codeString);
                    }
                }

                for (int i = 0; i < currentCheatCode.Count; i++)
                {
                    if (currentCheatCode[i] != devModeCode[i])
                    {
                        currentCheatCode.Clear();
                        break;
                    }
                }
      
                if (currentCheatCode.Count == devModeCode.Count)
                {
                    currentCheatCode.Clear();

                    Plugin.logger.LogMessage("Dev mode enabled, press F1 to open dev menu.");

                    var gameObject = GameObject.Instantiate(Content.devMenuPrefab);
                    // spawn network object
                    gameObject.GetComponent<NetworkObject>().Spawn();
                }

            }


            orig(self);
        }
    }
}
