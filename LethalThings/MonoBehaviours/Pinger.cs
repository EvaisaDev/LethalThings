using DunGen.Graph;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using static LethalThings.DynamicBone.DynamicBoneColliderBase;
using static LethalThings.MonoBehaviours.HackingTool;

namespace LethalThings.MonoBehaviours
{
    public class Pinger : GrabbableObject
    {
        public RenderTexture renderTexture;

        public Camera renderCamera;

        public Material screenMat;

        public Light backLight;

        public AudioSource audioSource;
        public AudioSource audioSourceFar;

        [Space(3f)]

        private static int renderTextureID = 0;
        private int currentRenderTextureID = 0;

        [Space(3f)]
        public Transform UISystem;
        public Transform selectionUI;
        public Transform pingUI;
        public Animator pingAnimator;
        public Animator triggerAnimator;
        public TextMeshProUGUI selectionString;

        // how long pings last
        public float pingTime = 5f;
        public float maxPingDistance = 50f;

        public GameObject pingMarkerPrefab;

        [HideInInspector]
        private NetworkVariable<bool> turnedOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public AudioClip turnOnClip;
        public AudioClip pingClip;

        public int noiseRange = 45;

        public LayerMask rayMask;

        public bool isPinging = false;

        private Vector3 pingPosition;
        private int nodeType;
        private string pingText;

        public bool validPing = false;

        public void Awake()
        {
            renderTexture = new RenderTexture(500, 390, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = $"PingerTool({renderTextureID})";
            currentRenderTextureID = renderTextureID;
            renderTextureID++;
            // setup camera to render to texture
            renderCamera.targetTexture = renderTexture;
            screenMat.mainTexture = renderTexture;

            // duplicate material
            screenMat = new Material(screenMat);

            // set material
            mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenMat });

            backLight.intensity = 2f;

        }

        public override void Update()
        {
            UISystem.position = new Vector3(-1000, -1000 * (currentRenderTextureID + 1), -1000);

            if (IsOwner && turnedOn.Value && insertedBattery.empty)
            {
                turnedOn.Value = false;
            }

            if (turnedOn.Value)
            {
                backLight.enabled = true;

                // if not pinging, cast ray forward and check if hit something
                // run every 20 frames
                if (!isPinging && Time.frameCount % 20 == 0)
                {
                    selectionUI.gameObject.SetActive(true);
                    pingUI.gameObject.SetActive(false);

                    RaycastHit raycastHit;

                    if (Physics.Raycast(StartOfRound.Instance.activeCamera.transform.position + (StartOfRound.Instance.activeCamera.transform.forward * 1), StartOfRound.Instance.activeCamera.transform.forward, out raycastHit, maxPingDistance, rayMask, QueryTriggerInteraction.Ignore))
                    {
                        /*
                        if (raycastHit.collider.transform.GetComponentInChildren<ScanNodeProperties>() != null)
                        {
                            var scanNode = raycastHit.collider.transform.GetComponentInChildren<ScanNodeProperties>();

                            selectionString.text = $"[{scanNode.headerText}]";
                        }
                        else
                        {
                            selectionString.text = "[???]";
                        }*/


                        selectionString.text = $"[Unknown Surface]";

                        nodeType = 0;
                        pingText = "Received Ping";

                        var root = Utilities.TryFindRoot(raycastHit.collider.transform);
                        if (root != null)
                        {
                    
                            if (root.GetComponentInChildren<ScanNodeProperties>(true) != null)
                            {
                                var scanNode = root.GetComponentInChildren<ScanNodeProperties>(true);

                                var scanNodeRoot = Utilities.TryFindRoot(scanNode.transform);

                                if (scanNodeRoot == root)
                                {
                                    selectionString.text = $"[{scanNode.headerText}]";
                                    nodeType = scanNode.nodeType;
                                    pingText = scanNode.headerText;

                                    //Plugin.logger.LogInfo($"target: {pingText}");
                                }
                                else
                                {
                                    selectionString.text = $"[Unknown Surface]";
                                }

                            }
                            else
                            {
                                if (root.GetComponentInChildren<PlayerControllerB>(true) != null)
                                {
                                    var playerController = root.GetComponentInChildren<PlayerControllerB>(true);

                                    var childRoot = Utilities.TryFindRoot(playerController.transform);
                                    if (childRoot == root)
                                    {

                                        selectionString.text = $"[{playerController.playerUsername}]";
                                        pingText = playerController.playerUsername;
                                    }
                                    else
                                    {
                                        selectionString.text = $"[Unknown Surface]";
                                    }
                                }
                                else if (root.GetComponentInChildren<DeadBodyInfo>(true) != null)
                                {
                                    var child = root.GetComponentInChildren<DeadBodyInfo>(true);
                                    var childRoot = Utilities.TryFindRoot(child.transform);
                                    if (childRoot == root)
                                    {
                                        selectionString.text = $"[{child.playerScript.playerUsername} (Dead)]";
                                        pingText = $"{child.playerScript.playerUsername} (Dead)";
                                    }
                                    else
                                    {
                                        selectionString.text = $"[Unknown Surface]";
                                    }
                                }
                                else if (root.GetComponentInChildren<GrabbableObject>(true))
                                { 
                                    var item = root.GetComponentInChildren<GrabbableObject>(true);
                                    var childRoot = Utilities.TryFindRoot(item.transform);
                                    if (childRoot == root && item.itemProperties != null)
                                    {
                                        selectionString.text = $"[{item.itemProperties.itemName}]";
                                        pingText = item.itemProperties.itemName;
                                    }
                                    else
                                    {
                                        selectionString.text = $"[Unknown Surface]";
                                    }
                                }
                                else if (root.GetComponentInChildren<PlaceableShipObject>(true))
                                {
                                    var item = root.GetComponentInChildren<PlaceableShipObject>(true);

                                    var childRoot = Utilities.TryFindRoot(item.transform);
                                    if (childRoot == root)
                                    {

                                        selectionString.text = $"[{StartOfRound.Instance.unlockablesList.unlockables[item.unlockableID].unlockableName}]";
                                        pingText = StartOfRound.Instance.unlockablesList.unlockables[item.unlockableID].unlockableName;

                                        var material = root.GetComponentInChildren<MeshRenderer>(true).material;

                                        // print hdrp/lit shader material type property
                                        // [Enum(Subsurface Scattering, 0, Standard, 1, Anisotropy, 2, Iridescence, 3, Specular Color, 4, Translucent, 5)] _MaterialID("MaterialId", Int) = 1 


                                        // Find the property ID for "_MaterialID"
                                        //int materialIDPropertyID = Shader.PropertyToID("_MaterialID");

                                        // Get the value of the "_MaterialID" property
                                        //int materialIDValue = material.GetInt(materialIDPropertyID);

                                        //Plugin.logger.LogInfo($"Material ID: {materialIDValue}");
                                    }
                                    else
                                    {
                                        selectionString.text = $"[Unknown Surface]";
                                    }

                                }
                                else {
                                    selectionString.text = $"[Unknown Surface]";
                                }
                                
                            }


                        }

                        // scale text to string size

                        validPing = true;

                        // set ping target
                        pingPosition = raycastHit.point;
                    }
                    else
                    {
                        // if not hit anything, hide selection UI
                        selectionString.text = "[No Target]";
                        // set ping target
                        validPing = false;

                    }
                }
                else if(isPinging)
                {
                    // if pinging, show ping UI
                    selectionUI.gameObject.SetActive(false);
                    pingUI.gameObject.SetActive(true);
                }


                
            }
            else
            {
                backLight.enabled = false;
                selectionUI.gameObject.SetActive(false);
                pingUI.gameObject.SetActive(false);
            }
            base.Update();
        }

        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
                playerHeldBy.activatingItem = false;
            }
            //backLight.enabled = false;
            isBeingUsed = false;

            /*
            if (IsOwner)
            {
                turnedOn.Value = false;
            }*/
            base.DiscardItem();
        }

        public override void PocketItem()
        {
            base.PocketItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
            playerHeldBy.activatingItem = false;
        }

        [ServerRpc]
        public void PingServerRpc(Vector3 position, string text, int nodeType, int senderID)
        {
            // spawn ping marker
            var pingMarker = Instantiate(pingMarkerPrefab, position, Quaternion.identity);
            pingMarker.GetComponent<ForcedPing>().pingDuration = pingTime;

            // spawn on clients
            pingMarker.GetComponent<NetworkObject>().Spawn();

            // set ping marker properties
            PingClientRpc(pingMarker.GetComponent<NetworkObject>(), text, nodeType, senderID);
        }

        [ClientRpc]
        public void PingClientRpc(NetworkObjectReference pingNetworkObject, string text, int nodeType, int senderID)
        {
            NetworkObject pingObject;
            if(pingNetworkObject.TryGet(out pingObject))
            {

                var pingMarker = pingObject.GetComponent<ForcedPing>();

                if (pingMarker != null)
                {
                    pingMarker.pingDuration = pingTime;
                    pingMarker.scanNodeProperties.headerText = text;
                    pingMarker.scanNodeProperties.nodeType = nodeType;

                    var sender = StartOfRound.Instance.allPlayerScripts[senderID].playerUsername;

                    pingMarker.scanNodeProperties.subText = $"Received from {sender}";
                }
            }
            
        }


        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            triggerAnimator.Play("fire");

            if (!IsOwner || turnedOn.Value == false || validPing == false)
            {
                return;
            }

            if (!isPinging)
            {
                isPinging = true;

                PlaySoundServerRpc(true);

                // ping
                PingServerRpc(pingPosition, pingText, nodeType, (int)playerHeldBy.playerClientId);

                

                selectionUI.gameObject.SetActive(false);
                pingUI.gameObject.SetActive(true);
                StartCoroutine(WaitForPing());
                isBeingUsed = true;
            }


        }

        // coroutine to wait for ping to finish
        public IEnumerator WaitForPing()
        {
            yield return new WaitForSeconds(2);

            isBeingUsed = false;

            isPinging = false;
        }

        [ServerRpc]
        public void PlaySoundServerRpc(bool isPing)
        {
            PlaySoundClientRpc(isPing);
        }

        [ClientRpc]
        public void PlaySoundClientRpc(bool isPing)
        {
            if (isPing)
            {
                pingAnimator.Play("ping");
                audioSource.PlayOneShot(pingClip);
                audioSourceFar.PlayOneShot(pingClip);
                WalkieTalkie.TransmitOneShotAudio(audioSource, pingClip, 1);
                RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            }
            else
            {
                audioSource.PlayOneShot(turnOnClip);
                audioSourceFar.PlayOneShot(turnOnClip);
                WalkieTalkie.TransmitOneShotAudio(audioSource, turnOnClip, 1);
                RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (!IsOwner)
            {
                return;
            }

            if (!right)
            {
                // turn on
                turnedOn.Value = !turnedOn.Value;

                // if was turned on, play turn on sound
                if (turnedOn.Value)
                {
                    PlaySoundServerRpc(false);
                }
            }

        }


    }
}
