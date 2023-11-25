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

        public static void DiscardItem(this PlayerControllerB self, GrabbableObject item, bool placeObject = false, NetworkObject parentObjectTo = null, Vector3 placePosition = default(Vector3), bool matchRotationOfParent = true)
        {
            self.SetSpecialGrabAnimationBool(setTrue: false, item);
            self.playerBodyAnimator.SetBool("cancelHolding", value: true);
            self.playerBodyAnimator.SetTrigger("Throw");
            if (placeObject)
            {
                if (parentObjectTo == null)
                {
                    self.throwingObject = true;
                    placePosition = ((!self.isInElevator) ? StartOfRound.Instance.propsContainer.InverseTransformPoint(placePosition) : StartOfRound.Instance.elevatorTransform.InverseTransformPoint(placePosition));
                    int floorYRot = (int)self.transform.localEulerAngles.y;
                    self.SetObjectAsNoLongerHeld(self.isInElevator, self.isInHangarShipRoom, placePosition, item, floorYRot);
                    item.DiscardItemOnClient();
                    self.ThrowObjectServerRpc(item.gameObject.GetComponent<NetworkObject>(), self.isInElevator, self.isInHangarShipRoom, placePosition, floorYRot);
                }
                else
                {
                    self.PlaceGrabbableObject(parentObjectTo.transform, placePosition, matchRotationOfParent, item);
                    item.DiscardItemOnClient();
                    self.PlaceObjectServerRpc(item.gameObject.GetComponent<NetworkObject>(), parentObjectTo, placePosition, matchRotationOfParent);
                }
                return;
            }
            self.throwingObject = true;
            bool droppedInElevator = self.isInElevator;
            Vector3 targetFloorPosition;
            if (!self.isInElevator)
            {
                Vector3 vector = ((!item.itemProperties.allowDroppingAheadOfPlayer) ? item.GetItemFloorPosition() : self.DropItemAheadOfPlayer());
                if (!self.playersManager.shipBounds.bounds.Contains(vector))
                {
                    targetFloorPosition = self.playersManager.propsContainer.InverseTransformPoint(vector);
                }
                else
                {
                    droppedInElevator = true;
                    targetFloorPosition = self.playersManager.elevatorTransform.InverseTransformPoint(vector);
                }
            }
            else
            {
                Vector3 vector = item.GetItemFloorPosition();
                if (!self.playersManager.shipBounds.bounds.Contains(vector))
                {
                    droppedInElevator = false;
                    targetFloorPosition = self.playersManager.propsContainer.InverseTransformPoint(vector);
                }
                else
                {
                    targetFloorPosition = self.playersManager.elevatorTransform.InverseTransformPoint(vector);
                }
            }
            int floorYRot2 = (int)self.transform.localEulerAngles.y;
            self.SetObjectAsNoLongerHeld(droppedInElevator, self.isInHangarShipRoom, targetFloorPosition, item, floorYRot2);
            item.DiscardItemOnClient();
            self.ThrowObjectServerRpc(item.NetworkObject, droppedInElevator, self.isInHangarShipRoom, targetFloorPosition, floorYRot2);
        }
    }
}
