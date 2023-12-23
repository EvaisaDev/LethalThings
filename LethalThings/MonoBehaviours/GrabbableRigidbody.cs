using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode.Components;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    // grabbable object which uses unity rigidbody physics
    [RequireComponent(typeof(Rigidbody))]
    public class GrabbableRigidbody : GrabbableObject
    {
        public float gravity = 9.8f;
        internal Rigidbody rb;
        public override void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            // force some properties which might be missconfigured
            itemProperties.itemSpawnsOnGround = false;
            base.Start();
            EnablePhysics(true);
        }

        public new void EnablePhysics(bool enable)
        {
            for (int i = 0; i < propColliders.Length; i++)
            {
                if (!(propColliders[i] == null) && !propColliders[i].gameObject.CompareTag("InteractTrigger") && !propColliders[i].gameObject.CompareTag("DoNotSet"))
                {
                    propColliders[i].enabled = enable;
                }
            }

            // enable rigidbody
            rb.isKinematic = !enable;
        }

        public override void Update()
        {
            // hax
            fallTime = 1.0f;
            reachedFloorTarget = true;
            var wasHeld = isHeld;
            // hella hax
            isHeld = true;
            base.Update();
            isHeld = wasHeld;
        }


        public void FixedUpdate()
        {
            // handle gravity if rigidbody is enabled
            if (IsHost) { 
                if (!rb.isKinematic && !isHeld)
                {
                    rb.useGravity = false;

                    rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

                    Plugin.logger.LogMessage("Velocity: " + rb.velocity.ToString());
                }
                else
                {
                    rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
                }
            }
        }

        public override void LateUpdate()
        {
            if (parentObject != null && isHeld)
            {
                base.transform.rotation = parentObject.rotation;
                base.transform.Rotate(itemProperties.rotationOffset);
                base.transform.position = parentObject.position;
                Vector3 positionOffset = itemProperties.positionOffset;
                positionOffset = parentObject.rotation * positionOffset;
                base.transform.position += positionOffset;
            }
            if (radarIcon != null)
            {
                radarIcon.position = base.transform.position;
            }
        }

        public override void FallWithCurve()
        {
            // stub, we do not need this.
        }

        public new void FallToGround(bool randomizePosition = false)
        {
            // stub, we do not need this.
        }

        public override void EquipItem()
        {
            // remove parent object
            base.EquipItem();
            transform.SetParent(null, true);
        }
    }
}
