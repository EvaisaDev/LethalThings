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
        private NetworkVariable<Vector3> throwDir = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public AudioClip dartHitSound;
        public AudioSource audioSource;

        public override void Start()
        {
            base.Start();
            
            if (IsHost && !isThrown)
            {
                rb.isKinematic = false;
                rb.AddForce(dartTip.forward * throwForce, ForceMode.Impulse);
                t = 0f;
            }
            isThrown = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            playerThrownBy = playerHeldBy;
            if (IsOwner && !isThrown)
            {
                
                playerHeldBy.DiscardHeldObject();
                rb.isKinematic = false;
                throwDir.Value = dartTip.forward;

                // cast ray forward for 100 units, if it hit something, we take the direction from the dart tip to the hit point
                RaycastHit hit;
                if (Physics.Raycast(playerThrownBy.gameplayCamera.transform.position, playerThrownBy.gameplayCamera.transform.forward, out hit, 100f, hitLayerMask))
                {
                    throwDir.Value = (hit.point - dartTip.position).normalized;
                }

                rb.AddForce(throwDir.Value * throwForce, ForceMode.Impulse);

                t = 0f;
            }
            isThrown = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayDartHitSoundServerRpc()
        {
            PlayDartHitSoundClientRpc();
        }

        [ClientRpc]
        public void PlayDartHitSoundClientRpc()
        {
            audioSource.PlayOneShot(dartHitSound);
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

                        if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)))
                        {
                            return;
                        }

                        transform.parent = closestChild;

                        //Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");

                        PlayDartHitSoundServerRpc();

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
                    

                    if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)))
                    {
                        return;
                    }


                    transform.parent = closestChild;

                    //Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;
                    PlayDartHitSoundServerRpc();

                }
            }

        }

        
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


                    if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)))
                    {
                        return;
                    }


                    transform.parent = closestChild;

                    //Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;
                    PlayDartHitSoundServerRpc();

                }
            }

        }

    }
}
