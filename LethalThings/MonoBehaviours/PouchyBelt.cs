using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using LethalThings.Extensions;
using UnityEngine.InputSystem;

namespace LethalThings
{
    public class PouchyBelt : GrabbableObject
    {

        public Transform beltCosmetic;
        public Vector3 beltCosmeticPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 beltCosmeticRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public int beltCapacity = 3;
        private PlayerControllerB previousPlayerHeldBy;

        public static void Initialize()
        {
            On.GameNetcodeStuff.PlayerControllerB.SetHoverTipAndCurrentInteractTrigger += PlayerControllerB_SetHoverTipAndCurrentInteractTrigger;
            On.GameNetcodeStuff.PlayerControllerB.BeginGrabObject += PlayerControllerB_BeginGrabObject;
        }

        public void Awake()
        {
            if (InputCompat.Enabled) { 
                InputCompat.LTUtilityBeltQuick1.started += InputReceived;
                InputCompat.LTUtilityBeltQuick2.started += InputReceived;
                InputCompat.LTUtilityBeltQuick3.started += InputReceived;
                InputCompat.LTUtilityBeltQuick4.started += InputReceived;
            }
        }

        private static void PlayerControllerB_BeginGrabObject(On.GameNetcodeStuff.PlayerControllerB.orig_BeginGrabObject orig, PlayerControllerB self)
        {
            self.interactRay = new Ray(self.gameplayCamera.transform.position, self.gameplayCamera.transform.forward);
            if (!Physics.Raycast(self.interactRay, out self.hit, self.grabDistance, self.interactableObjectsMask) || self.hit.collider.gameObject.layer == 8 || !(self.hit.collider.tag == "PhysicsProp") || self.twoHanded || self.sinkingValue > 0.73f)
            {
                return;
            }
            self.currentlyGrabbingObject = self.hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
            if (!GameNetworkManager.Instance.gameHasStarted && !self.currentlyGrabbingObject.itemProperties.canBeGrabbedBeforeGameStart && (StartOfRound.Instance.testRoom == null || !StartOfRound.Instance.testRoom.activeSelf))
            {
                return;
            }
            if (self.currentlyGrabbingObject == null || self.inSpecialInteractAnimation || self.currentlyGrabbingObject.isHeld || self.currentlyGrabbingObject.isPocketed)
            {
                return;
            }
            NetworkObject networkObject = self.currentlyGrabbingObject.NetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return;
            }
            if (self.currentlyGrabbingObject is PouchyBelt)
            {
                var hasBelt = self.ItemSlots.Any(x => x != null && x is PouchyBelt);

                if (hasBelt)
                {
                    self.currentlyGrabbingObject.grabbable = false;
                }
            }

            orig(self);

            if (self.currentlyGrabbingObject is PouchyBelt)
            {
                self.currentlyGrabbingObject.grabbable = true;
            }
        }

        private static void PlayerControllerB_SetHoverTipAndCurrentInteractTrigger(On.GameNetcodeStuff.PlayerControllerB.orig_SetHoverTipAndCurrentInteractTrigger orig, PlayerControllerB self)
        {
            orig(self);
            if (Physics.Raycast(self.interactRay, out self.hit, self.grabDistance, self.interactableObjectsMask) && self.hit.collider.gameObject.layer != 8)
            {
                string text = self.hit.collider.tag;
                if (text == "PhysicsProp")
                {
                    if (self.FirstEmptyItemSlot() == -1)
                    {
                        self.cursorTip.text = "Inventory full!";
                    }
                    else
                    {
                        GrabbableObject component2 = self.hit.collider.gameObject.GetComponent<GrabbableObject>();
                        // if component is PouchyBelt
                        if (component2 is PouchyBelt)
                        {
                            var hasBelt = self.ItemSlots.Any(x => x != null && x is PouchyBelt);

                            if (hasBelt)
                            {
                                self.cursorTip.text = "(Cannot hold more than 1 belt)";
                            }
                            else
                            {
                                self.cursorTip.text = "Pick up belt";
                            }
                        }

                    }
                }
            }

         }

        public void InputReceived(InputAction.CallbackContext context)
        {
            if (IsOwner && playerHeldBy != null)
            {
                if (context.started)
                {
                    
                    var index = -1;

                    if (context.action == InputCompat.LTUtilityBeltQuick1)
                    {
                        index = 0;
                    }
                    else if (context.action == InputCompat.LTUtilityBeltQuick2)
                    {
                        index = 1;
                    }
                    else if (context.action == InputCompat.LTUtilityBeltQuick3)
                    {
                        index = 2;
                    }
                    else if (context.action == InputCompat.LTUtilityBeltQuick4)
                    {
                        index = 3;
                    }

                    if(index != -1)
                    {
                        if(index >= slotIndexes.Count)
                        {
                            return;
                        }

                        playerHeldBy.SwitchItemSlots(slotIndexes[index]);

                        Plugin.logger.LogInfo($"Switched to slot: {slotIndexes[index]}");

                    }
                }
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            if (previousPlayerHeldBy != null)
            {
                // enable the belt
                beltCosmetic.gameObject.SetActive(true);
                // remove from parent
                beltCosmetic.SetParent(null);
                beltCosmetic.GetComponent<MeshRenderer>().enabled = true;

                if (IsOwner)
                {
                    // prevent belt from showing up for owner.
                    beltCosmetic.GetComponent<MeshRenderer>().enabled = false;
                }

                var root = previousPlayerHeldBy.lowerSpine.parent;

                // Set position and rotation
                beltCosmetic.position = root.position + beltCosmeticPositionOffset;

                // Convert the Vector3 offset to a Quaternion
                Quaternion rotation = Quaternion.Euler(root.rotation.eulerAngles + beltCosmeticRotationOffset);

                // Apply the rotation offset
                beltCosmetic.rotation = rotation;

                mainObjectRenderer.enabled = false;
                gameObject.SetActive(true);
                //Debug.Log("Showing belt!");
            }
            else
            {
                // disable the belt
                beltCosmetic.gameObject.SetActive(false);
                mainObjectRenderer.enabled = true;
                // add back to parent
                beltCosmetic.SetParent(transform);
                //Debug.Log("Hiding belt!");
            }
        }

        public List<int> slotIndexes = new List<int>();

        public void UpdateHUD(bool add)
        {
            slotIndexes.Clear();
            var hud = HUDManager.Instance;
            if (add)
            {
                var beltCount = 0;
                foreach(GrabbableObject slot in GameNetworkManager.Instance.localPlayerController.ItemSlots)
                {
                    // check if slot is type of PouchyBelt
                    if (slot is PouchyBelt)
                    {
                        beltCount++;
                    }
                }

                var referenceFrame = hud.itemSlotIconFrames[0];
                var referenceIcon = hud.itemSlotIcons[0];

                var lastInventorySize = hud.itemSlotIconFrames.Length;

                var canvasScaler = referenceFrame.GetComponentInParent<CanvasScaler>();
                var aspectRatioFitter = referenceFrame.GetComponentInParent<AspectRatioFitter>();
                var slotSizeX = referenceFrame.rectTransform.sizeDelta.x;
                var slotSizeY = referenceFrame.rectTransform.sizeDelta.y;
                var yPosition = referenceFrame.rectTransform.anchoredPosition.y + 1.125f * slotSizeY * beltCount;
                var frameAngles = referenceFrame.rectTransform.localEulerAngles;
                var iconAngles = referenceIcon.rectTransform.localEulerAngles;

                var iconFrames = hud.itemSlotIconFrames.ToList();
                var icons = hud.itemSlotIcons.ToList();

                // find index of last slot named `slot\d` regex
                var index = iconFrames.FindLastIndex(x => {
                    var match = System.Text.RegularExpressions.Regex.IsMatch(x.gameObject.name.ToLowerInvariant(), @"\bslot\d\b");
                    /*
                    if (match)
                    {
                        Debug.Log($"Found match: {x.gameObject.name}");
                    }
                    else
                    {
                        Debug.Log($"No match: {x.gameObject.name}");
                    }*/
                    
                    return match;
                 });


                var totalWidth = (beltCapacity * slotSizeX) + ((beltCapacity - 1) * 15f);


                Debug.Log($"Adding {beltCapacity} item slots! Surely this will go well..");
                Debug.Log($"Adding after index: {index}");

                for (int i = 0; i < beltCapacity; i++)
                {
                    // calculate xPosition to center belt using TotalWidth
                    var anchor = -(referenceFrame.rectTransform.parent.GetComponent<RectTransform>().sizeDelta.x / 2) - (15 / 4);

                    var xPosition = anchor + (i * slotSizeX) + (i * 15f);


                    var prefab = iconFrames[0];

                    var frame = Instantiate(prefab, referenceFrame.transform.parent);
                    frame.name = $"Slot{lastInventorySize + i}[LethalThingsBelt]";
                    frame.rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
                    frame.rectTransform.eulerAngles = frameAngles;

                    var icon = frame.transform.GetChild(0).GetComponent<Image>();
                    icon.name = "icon";
                    icon.enabled = false;
                    icon.rectTransform.eulerAngles = iconAngles;
                    // rotate 90 degrees because unity is goofy
                    icon.rectTransform.Rotate(new Vector3(0.0f, 0.0f, -90.0f));

                    var slotIndex = index + i + 1;

                    // insert at index
                    iconFrames.Insert(slotIndex, frame);
                    icons.Insert(slotIndex, icon);

                    slotIndexes.Add(slotIndex);

                    // move up in parent to match index
                    frame.transform.SetSiblingIndex(slotIndex);

                    
                }

                hud.itemSlotIconFrames = iconFrames.ToArray();
                hud.itemSlotIcons = icons.ToArray();

                Debug.Log($"Added {beltCapacity} item slots!");

            }
            else
            {
                // remove slots marked with [LethalThingsBelt]
                var iconFrames = hud.itemSlotIconFrames.ToList();
                var icons = hud.itemSlotIcons.ToList();

                var lastInventorySize = iconFrames.Count;

                var slotsRemoved = 0;
                for (int i = lastInventorySize - 1; i >= 0; i--)
                {
                    if (iconFrames[i].name.Contains("[LethalThingsBelt]"))
                    {
                        slotsRemoved++;
                        var frame = iconFrames[i];
                        iconFrames.RemoveAt(i);
                        icons.RemoveAt(i);
                        Destroy(frame.gameObject);
                        if(slotsRemoved >= beltCapacity)
                        {
                            break;
                        }
                    }
                }

                hud.itemSlotIconFrames = iconFrames.ToArray();

                hud.itemSlotIcons = icons.ToArray();

                Debug.Log($"Removed {beltCapacity} item slots!");

            }
        }

        public void AddItemSlots()
        {
            if (playerHeldBy != null)
            {
                var itemSlots = playerHeldBy.ItemSlots.ToList();

                playerHeldBy.ItemSlots = new GrabbableObject[itemSlots.Count + beltCapacity];

                // add items back to player
                for (int i = 0; i < itemSlots.Count; i++)
                {
                    playerHeldBy.ItemSlots[i] = itemSlots[i];
                }

                if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                {
                    UpdateHUD(true);
                }
            
            }
        }

        public void RemoveItemSlots()
        {

            if (playerHeldBy != null)
            {

                var slotsAvailable = playerHeldBy.ItemSlots.Length - beltCapacity;

                var originalSlot = playerHeldBy.currentItemSlot;

                // drop items in belt
                for (int i = 0; i < beltCapacity; i++)
                {
                    var slot = playerHeldBy.ItemSlots[slotsAvailable + i];
                    if (slot != null)
                    {
                        //playerHeldBy.DiscardItem(slot);
                        
                        playerHeldBy.DropItem(slot, slotsAvailable + i, true);
                    }
                }

                var newSlot = playerHeldBy.currentItemSlot;

                if (originalSlot >= playerHeldBy.ItemSlots.Length)
                {
                    newSlot = 0;
                }
                else
                {
                    newSlot = originalSlot;
                }

                var itemSlots = playerHeldBy.ItemSlots.ToList();

                playerHeldBy.ItemSlots = new GrabbableObject[itemSlots.Count - beltCapacity];

                // add items back to player
                for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
                {
                    playerHeldBy.ItemSlots[i] = itemSlots[i];
                }

                if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                {
                    UpdateHUD(false);
                }


                playerHeldBy.SwitchItemSlots(newSlot);
            }
        }




        public override void DiscardItem()
        {


            RemoveItemSlots();

            previousPlayerHeldBy = null;

            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            RemoveItemSlots();

            previousPlayerHeldBy = null;

            base.OnNetworkDespawn();

        }

        new public void GrabItemOnClient()
        {

            base.GrabItemOnClient();
        }

        // unregister inputs
        public override void OnDestroy()
        {
            if (InputCompat.Enabled)
            {
                InputCompat.LTUtilityBeltQuick1.started -= InputReceived;
                InputCompat.LTUtilityBeltQuick2.started -= InputReceived;
                InputCompat.LTUtilityBeltQuick3.started -= InputReceived;
                InputCompat.LTUtilityBeltQuick4.started -= InputReceived;
            }

            base.OnDestroy();
        }

        public override void EquipItem()
        {
            base.EquipItem();



            if (playerHeldBy != null)
            {
                if (playerHeldBy != previousPlayerHeldBy)
                {
                    AddItemSlots();
                }

                previousPlayerHeldBy = playerHeldBy;
            }
        }
    }
}
