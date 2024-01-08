using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition.Attributes;
using Vector3 = UnityEngine.Vector3;

namespace LethalThings.MonoBehaviours
{
    public class ForcedPing : NetworkBehaviour
    {
        public float pingDuration = 20f;
        public float currentPingTime = 0;
        [HideInInspector]
        public NetworkVariable<bool> isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public ScanNodeProperties scanNodeProperties;
        private HUDManager hudManager;
        public bool destroyAfterPing = false;

        public static void Init()
        {
            On.HUDManager.NodeIsNotVisible += HUDManager_NodeIsNotVisible;
        }

        private static bool HUDManager_NodeIsNotVisible(On.HUDManager.orig_NodeIsNotVisible orig, HUDManager self, ScanNodeProperties node, int elementIndex)
        {
            if (node != null && node.transform != null && node.transform.parent != null && node.transform.parent.GetComponent<ForcedPing>() != null)
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                // check if scan node is in field of view of the player
                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(node.transform.position);

                //UnityEngine.Debug.Log($"View pos: {viewPos}");

                // if node is not in field of view, return false
                if (Vector3.Distance(node.transform.position, camera.transform.position) > node.maxRange || (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0))
                {
                    Plugin.logger.LogInfo($"[1] Node {node.name} not valid");
                    return orig(self, node, elementIndex);
                }

                var flareController = node.transform.parent.GetComponent<ForcedPing>();
               // Debug.Log($"Burnt out? {flareController.burntOut}");
                if (flareController.isActive.Value)
                {
                    if (!self.nodesOnScreen.Contains(node))
                    {
                        self.nodesOnScreen.Add(node);
                    }
                    return false;
                }
            }
            return orig(self, node, elementIndex);
        }


        public void Awake()
        {
            hudManager = HUDManager.Instance;

            scanNodeProperties = GetComponentInChildren<ScanNodeProperties>();

            //hudManager.AttemptScanNode(scanNodeProperties, 0, GameNetworkManager.Instance.localPlayerController);

            if(Vector3.Distance(scanNodeProperties.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < scanNodeProperties.maxRange)
            {
                if (!hudManager.nodesOnScreen.Contains(scanNodeProperties))
                {
                    hudManager.nodesOnScreen.Add(scanNodeProperties);
                }
                hudManager.AssignNodeToUIElement(scanNodeProperties);
            }
  


        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                isActive.Value = true;
                currentPingTime = 0;
            }


        }

        [ClientRpc]
        public void RemovePingClientRpc()
        {
            Plugin.logger.LogInfo("Removing ping");
            Dictionary<RectTransform, ScanNodeProperties> scanNodes = hudManager.scanNodes;
            RectTransform rectTransform = null;
            foreach (var scanNode in scanNodes)
            {
                if (scanNode.Value == scanNodeProperties)
                {
                    rectTransform = scanNode.Key;
                }
            }

            if (rectTransform != null)
            {
                scanNodes.Remove(rectTransform);
                rectTransform.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
            if (IsServer)
            {
                if (currentPingTime <= pingDuration)
                {
                    currentPingTime += Time.deltaTime;
                }
                else
                {
                    // no longer active
                    isActive.Value = false;
                    RemovePingClientRpc();

                    if (destroyAfterPing)
                    {
                        GetComponent<NetworkObject>().Despawn(true);
                    }
                }
            }

            if (isActive.Value && !HUDManager.Instance.nodesOnScreen.Contains(scanNodeProperties))
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(scanNodeProperties.transform.position);

                if (Vector3.Distance(scanNodeProperties.transform.position, camera.transform.position) < scanNodeProperties.maxRange && (viewPos.x < 1 && viewPos.x > 0 && viewPos.y < 1 && viewPos.y > 0 && viewPos.z >= 0))
                {
                    Plugin.logger.LogInfo($"[2] Node {scanNodeProperties.name} not valid");
                    if (!hudManager.nodesOnScreen.Contains(scanNodeProperties))
                    {
                        hudManager.nodesOnScreen.Add(scanNodeProperties);
                    }
                    hudManager.AssignNodeToUIElement(scanNodeProperties);
                }
            }


        }

    }
}
