using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using static UnityEngine.GraphicsBuffer;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

// idk wtf i am doing lmao!!!

namespace LethalThings.MonoBehaviours
{
    [ExecuteAlways]
    public class Dart : GrabbableRigidbody
    {


        public LayerMask hitLayerMask;

        private PlayerControllerB playerThrownBy;

        public Transform dartTip;

        public float throwForce = 10f;
        private float t = 0f;
        public bool isThrown = false;

        public override void Start()
        {
            base.Start();
            /*
            if (IsHost && !isThrown)
            {
                rb.isKinematic = false;
                rb.AddForce(dartTip.forward * throwForce, ForceMode.Impulse);
                t = 0f;
            }
            isThrown = true;*/
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            playerThrownBy = playerHeldBy;
            if (IsOwner && !isThrown)
            {
                
                playerHeldBy.DiscardHeldObject();
                rb.isKinematic = false;
                rb.AddForce(dartTip.forward * throwForce, ForceMode.Impulse);
                t = 0f;
            }
            isThrown = true;
        }

        public override void Update()
        {
            

            if (IsHost)
            {
                if (isThrown)
                {
                    if (rb.velocity.magnitude > 0.1f)
                    {
                        transform.LookAt(transform.position + rb.velocity);
                    }
                    /*else
                    {
                        rb.isKinematic = false;
                        rb.AddForce(dartTip.forward * throwForce, ForceMode.Impulse);
                    }*/

                    t += Time.deltaTime;
                    if (t > 5f)
                    {
                        isThrown = false;
                        rb.isKinematic = true;
                    }

                    // set velocity to forward
                    //rb.velocity = transform.forward * throwForce;
                    //rb.velocity += Vector3.down * 0.5f * gravity * t;

                    RaycastHit hit;
                    // do spherecast at dart tip to see if we hit anything
                    
                    if (Physics.SphereCast(dartTip.position, 0.01f, transform.forward, out hit, 0.01f, hitLayerMask))
                    {
                        // if we hit something, parent ourselves to the closest child of the hit object
                        var closestChild = hit.transform;

                        if(closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform))
                        {
                            return;
                        }

                        transform.parent = closestChild;

                        Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                        isThrown = false;
                        rb.isKinematic = true;
                    }
                }
            }

            base.Update();
        }


        public void OnTriggerEnter(Collider collision)
        {

            if (IsHost)
            {
                if (isThrown)
                {
                    var closestChild = collision.transform;

                    // if hit layer is not in mask, return
                    if ((hitLayerMask.value & 1 << closestChild.gameObject.layer) == 0)
                    {
                        return;
                    }
                    

                    if (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform))
                    {
                        return;
                    }


                    transform.parent = closestChild;

                    Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;


                }
            }

        }

        /*
        public void OnCollisionEnter(Collision collision)
        {

            if (IsHost)
            {
                if (isThrown)
                {
                    var closestChild = collision.transform;

                    // if hit layer is not in mask, return
                    if ((hitLayerMask.value & 1 << closestChild.gameObject.layer) == 0)
                    {
                        return;
                    }


                    if (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform))
                    {
                        return;
                    }


                    transform.parent = closestChild;

                    Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;


                }
            }

        }*/

        /*
        public float gravity = 9.8f; // Standard gravity on Earth
        public float throwVelocity = 10f;
        public float maxThrowDistance = 20f; // Adjust as needed
        public float t = 0f;
        public float simulatedTime = 0f;


        private NetworkVariable<Vector3> throwDirection = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private Vector3 throwPosition;
        private NetworkVariable<Vector3> startPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<Vector3> currentVelocity = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkVariable<bool> isThrown = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> isBeingThrown = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (IsOwner)
            {
                playerThrownBy = playerHeldBy;
                playerHeldBy.DiscardHeldObject(placeObject: true, null, FindThrowTargetPosition(), false);

                startPosition.Value = transform.position;
                throwDirection.Value = playerThrownBy.gameplayCamera.transform.forward;
                currentVelocity.Value = throwDirection.Value * throwVelocity;
            }

            if (IsHost)
            {
                isBeingThrown.Value = true;
            }
        }

        public override void EquipItem()
        {
            EnableItemMeshes(enable: true);

            if (IsServer) { 
                isThrown.Value = false;
                isBeingThrown.Value = false;
            }
            isPocketed = false;
        }

        public override void Start()
        {
            base.Start();

            if (base.IsOwner)
            {
                //playerHeldBy.DiscardHeldObject(placeObject: true, null, FindThrowTargetPosition());
                //StartOfRound.Instance.localPlayerController.PlaceGrabbableObject(null, FindThrowTargetPosition(), false, this);
            }
        }

        public void ThrowUpdate()
        {
            if(!isThrown.Value)
            {


                throwPosition = startPosition.Value;

                t = 0f;
                isThrown.Value = true;
            }

            throwPosition += currentVelocity.Value * Time.deltaTime;
            throwPosition.y -= 0.5f * gravity * t * t * Time.deltaTime;

            // cast ray from last position to current position, if it hits something, return the between position
            RaycastHit hit;
            if (Physics.Linecast(transform.position, throwPosition, out hit, hitLayerMask))
            {
                // if it hits something, return the between position
                throwPosition = hit.point;

                // if it hits something, parent ourselves to the closest child of the hit object
                var closestChild = hit.transform;


                Debug.Log("Closest child: " + closestChild.name);

                transform.parent = closestChild;

                isThrown.Value = false;
            }
            // rotate the dart to face the direction it is moving, so the direction from transform.position to throwPosition
            transform.LookAt(throwPosition);

            // update the position
            transform.position = throwPosition;

            // offset backwards so that the dart tip is at the throw position
            transform.position -= transform.forward * Vector3.Distance(transform.position, dartTip.position);


            // check if the throw position is within the max throw distance
            if (Vector3.Distance(startPosition.Value, transform.position) > maxThrowDistance)
            {
                isThrown.Value = false;
            }

            // Update time parameter
            t += Time.deltaTime;
        }

        public Vector3 FindThrowTargetPosition()
        {
            // find the position the dart will land at, using a point intersect

            Vector3 simThrowPosition = playerThrownBy.gameplayCamera.transform.position;
            var simThrowDirection = playerThrownBy.gameplayCamera.transform.forward;
            var simCurrentVelocity = simThrowDirection * throwVelocity;


            // run a loop to find the point of intersection

            simulatedTime = 0f;

            for (int i = 0; i < 100; i++)
            {
                Vector3 lastThrowPosition = simThrowPosition;

                // Update the position using the projectile motion equations
                simThrowPosition += simCurrentVelocity * Time.deltaTime;
                simThrowPosition.y -= 0.5f * gravity * simulatedTime * simulatedTime * Time.deltaTime;


                // cast ray from last position to current position, if it hits something, return the between position
                RaycastHit hit;
                if (Physics.Linecast(lastThrowPosition, simThrowPosition, out hit, hitLayerMask))
                {

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lastThrowPosition, hit.point);


                    // if it hits something, return the between position
                    return hit.point;
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(lastThrowPosition, simThrowPosition);

                // Update time parameter
                simulatedTime += Time.deltaTime;

                // check if the throw position is within the max throw distance
                if (Vector3.Distance(simThrowPosition, transform.position) > maxThrowDistance)
                {
                    // if not, return the last throw position
                    return simThrowPosition;
                }


            }



            return simThrowPosition;


        }

        void OnDrawGizmosSelected()
        {
            FindThrowTargetPosition();
        }

        public override void Update()
        {
            base.Update();

            if (IsHost)
            {
                if (isBeingThrown.Value)
                {
                    ThrowUpdate();
                }
                else if (!isThrown.Value)
                {
                    isBeingThrown.Value = false;
                }
            }


        }
        */

    }
}
