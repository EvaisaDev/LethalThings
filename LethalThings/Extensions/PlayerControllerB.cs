using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.Extensions
{
    public static class PlayerControllerExtensions
    {

        public static bool SwitchItemSlots(this PlayerControllerB self, int requestedSlot)
        {
            if (!self.IsItemSwitchPossible() ||
                self.currentItemSlot == requestedSlot)
            {
                return false;
            }

            var distance = self.currentItemSlot - requestedSlot;
            var requestedSlotIsLowerThanCurrent = (distance > 0);

            if (Math.Abs(distance) == self.ItemSlots.Length - 1)
            {
                self.SwitchItemSlotsServerRpc(requestedSlotIsLowerThanCurrent ? true : false);
            }
            else
            {
                do
                {
                    self.SwitchItemSlotsServerRpc(requestedSlotIsLowerThanCurrent ? true : false);
                    distance += requestedSlotIsLowerThanCurrent ? -1 : 1;
                } while (distance != 0);
            }

            ShipBuildModeManager.Instance.CancelBuildMode();
            self.playerBodyAnimator.SetBool("GrabValidated", false);

            self.SwitchToItemSlot(requestedSlot);

            if (self.currentlyHeldObjectServer != null)
            {
                self.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>().PlayOneShot(self.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
            }

            return true;
        }

        public static bool IsItemSwitchPossible(this PlayerControllerB self)
        {
            return !(self.timeSinceSwitchingSlots < 0.01 || self.inTerminalMenu || self.isGrabbingObjectAnimation ||
                     self.inSpecialInteractAnimation || self.throwingObject || self.isTypingChat ||
                     self.twoHanded || self.activatingItem || self.jetpackControls ||
                     self.disablingJetpackControls);
        }

        public static void DropItem(this PlayerControllerB self, GrabbableObject grabbableObject, int slotIndex = 0, bool itemsFall = true)
        {


            if (itemsFall)
            {
                grabbableObject.parentObject = null;
                grabbableObject.heldByPlayerOnServer = false;
                if (self.isInElevator)
                {
                    grabbableObject.transform.SetParent(self.playersManager.elevatorTransform, worldPositionStays: true);
                }
                else
                {
                    grabbableObject.transform.SetParent(self.playersManager.propsContainer, worldPositionStays: true);
                }
                self.SetItemInElevator(self.isInHangarShipRoom, self.isInElevator, grabbableObject);
                grabbableObject.EnablePhysics(enable: true);
                grabbableObject.EnableItemMeshes(enable: true);
                grabbableObject.transform.localScale = grabbableObject.originalScale;
                grabbableObject.isHeld = false;
                grabbableObject.isPocketed = false;

                grabbableObject.transform.position = self.serverItemHolder.position;

                grabbableObject.startFallingPosition = grabbableObject.transform.parent.InverseTransformPoint(self.serverItemHolder.position);

                /*
                // spawn a debug sphere at startFallingPosition
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = grabbableObject.transform.parent;
                sphere.transform.localPosition = grabbableObject.startFallingPosition;
                sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                // spawn another debug sphere at the grabbableObject's position
                GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere2.transform.parent = grabbableObject.transform.parent;
                sphere2.transform.localPosition = grabbableObject.transform.localPosition;
                sphere2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                */

                grabbableObject.FallToGround(true);
                grabbableObject.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
                if (self.IsOwner)
                {
                    grabbableObject.DiscardItemOnClient();
                }
                else if (!grabbableObject.itemProperties.syncDiscardFunction)
                {
                    grabbableObject.playerHeldBy = null;
                }

                self.SetObjectAsNoLongerHeld(self.isInElevator, self.isInHangarShipRoom, grabbableObject.targetFloorPosition, grabbableObject);
            }
            if (self.IsOwner)
            {
                // if player was holding this item in their hands
                if(self.currentlyHeldObject != null && self.currentlyHeldObject == grabbableObject)
                {
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    HUDManager.Instance.ClearControlTips();
                    self.activatingItem = false;
                }

                HUDManager.Instance.itemSlotIcons[slotIndex].enabled = false;


            }
            self.ItemSlots[slotIndex] = null;
        }



    }
}
