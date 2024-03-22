using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

// idk wtf i am doing lmao!!!

namespace LethalThings.MonoBehaviours
{
    [ExecuteAlways]
    public class Dart : GrabbableRigidbody
    {

        private PlayerControllerB playerThrownBy;

        public Transform dartTip;

        public float throwForce = 10f;
        private float t = 0f;
        public bool isThrown = false;
        [HideInInspector]
        private NetworkVariable<Vector3> throwDir = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [HideInInspector]
        private NetworkVariable<bool> isKinematic = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public AudioClip dartHitSound;
        public AudioSource audioSource;

        public GameObject trackingPoint;

        public BoxCollider collider;

        public override void Start()
        {
            base.Start();
            
            if (IsHost)
            {
                rb.isKinematic = false;
                rb.AddForce(dartTip.forward * throwForce, ForceMode.Impulse);
                t = 0f;
                isThrown = true;
            }

            collider = GetComponent<BoxCollider>();

        }

        public static void Init()
        {
            On.HUDManager.AssignNewNodes += HUDManager_AssignNewNodes;
            On.HUDManager.NodeIsNotVisible += HUDManager_NodeIsNotVisible;
            On.HUDManager.MeetsScanNodeRequirements += HUDManager_MeetsScanNodeRequirements;
        }

        private static bool HUDManager_MeetsScanNodeRequirements(On.HUDManager.orig_MeetsScanNodeRequirements orig, HUDManager self, ScanNodeProperties node, PlayerControllerB playerScript)
        {
            if (node != null && node.transform != null && node.transform.parent != null && node.transform.parent.GetComponent<Dart>() != null)
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                // check if scan node is in field of view of the player
                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(node.transform.position);

                //UnityEngine.Debug.Log($"View pos: {viewPos}");

                // if node is not in field of view, return false
                if ((viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0) || node.transform.parent.GetComponent<Dart>().isHeld)
                {



                    return false;
                }

            }

            return orig(self, node, playerScript);
        }

        private static bool HUDManager_NodeIsNotVisible(On.HUDManager.orig_NodeIsNotVisible orig, HUDManager self, ScanNodeProperties node, int elementIndex)
        {
            if (node != null && node.transform != null && node.transform.parent != null && node.transform.parent.GetComponent<Dart>() != null)
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                // check if scan node is in field of view of the player
                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(node.transform.position);

                //UnityEngine.Debug.Log($"View pos: {viewPos}");

                // if node is not in field of view, return true
                if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0 || node.transform.parent.GetComponent<Dart>().isHeld)
                {
                    self.nodesOnScreen.Remove(node);
                }

            }
            return orig(self, node, elementIndex);
        }

        

        private static void HUDManager_AssignNewNodes(On.HUDManager.orig_AssignNewNodes orig, HUDManager self, PlayerControllerB playerScript)
        {
            orig(self, playerScript);

            if (self.nodesOnScreen.Count < self.scanElements.Length)
            {
                // find all darts in the scene
                var darts = FindObjectsOfType<Dart>();
                // check if darts are in range
                foreach (Dart dart in darts)
                {
                    var scanNode = dart.GetComponentInChildren<ScanNodeProperties>();
                    if(!dart.isHeld && scanNode != null)
                    {

                        var player = GameNetworkManager.Instance.localPlayerController;

                        // check if scan node is in field of view of the player
                        var camera = player.gameplayCamera;

                        var viewPos = camera.WorldToViewportPoint(scanNode.transform.position);

                        //UnityEngine.Debug.Log($"View pos: {viewPos}");

                        // if node is not in field of view, return false
                        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0)
                        {
                            continue;
                        }

                        var maxDistance = scanNode.maxRange;
                        var distance = Vector3.Distance(playerScript.transform.position, dart.transform.position);
                        if (distance < maxDistance)
                        {
                            // add dart to scan elements
                            self.AttemptScanNode(scanNode, 0, playerScript);
                        }
                    }
                }
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            isThrown = false;
            rb.isKinematic = false;
            t = 0f;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            playerThrownBy = playerHeldBy;
            if (IsOwner)
            {
                
                playerHeldBy.DiscardHeldObject();
                rb.isKinematic = false;
                throwDir.Value = dartTip.forward;

                // cast ray forward for 100 units, if it hit something, we take the direction from the dart tip to the hit point
                RaycastHit hit;
                if (Physics.Raycast(playerThrownBy.gameplayCamera.transform.position, playerThrownBy.gameplayCamera.transform.forward, out hit, 100f, Utilities.MaskForLayer(gameObject.layer), QueryTriggerInteraction.Ignore))
                {
                    throwDir.Value = (hit.point - dartTip.position).normalized;
                }

                ThrowDartServerRpc(throwDir.Value);
                
            }


           
        }

        [ServerRpc(RequireOwnership = false)]
        public void ThrowDartServerRpc(Vector3 throwDir)
        {
            t = 0f;
            rb.isKinematic = false;
            isThrown = true;

            Plugin.logger.LogMessage($"Throwing dart with velocity: {throwDir * throwForce}");

            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
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


            if (isKinematic.Value != rb.isKinematic && !IsHost)
            {
                rb.isKinematic = isKinematic.Value;
            }
            else if(IsHost && rb.isKinematic != isKinematic.Value)
            {
                isKinematic.Value = rb.isKinematic;
            }

            if (isKinematic.Value)
            {
                collider.isTrigger = true;
            }
            else
            {
                collider.isTrigger = false;
            }

            if (IsHost)
            {
                if (rb.isKinematic && !isHeld && trackingPoint != null)
                {
                    rb.position = trackingPoint.transform.position;
                    rb.rotation = trackingPoint.transform.rotation;
                }
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
                    
                    if (Physics.SphereCast(dartTip.position, 0.01f, transform.forward, out hit, 0.01f, Utilities.MaskForLayer(gameObject.layer), QueryTriggerInteraction.Ignore))
                    {
                        // if we hit something, parent ourselves to the closest child of the hit object
                        var closestChild = hit.transform;

                        if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)) || closestChild.GetComponent<Dart>() != null || closestChild.GetComponentInParent<Dart>() != null)
                        {
                            return;
                        }

                        TryParent(hit.collider);

                        Plugin.logger.LogMessage($"(1) Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");

                        PlayDartHitSoundServerRpc();

                        isThrown = false;
                        rb.isKinematic = true;
                    }
                }
            }

            base.Update();
        }

        public void TryParent(Collider collider)
        {

            var root = Utilities.TryFindRoot(collider.transform);
            if (root != null)
            {

                if (root.GetComponent<Dart>() != null || root.GetComponentInParent<Dart>() != null || root.name == "DartTrackingPoint")
                {
                    SetParent(collider.transform);
                }

                // find closest child in terms of distance
                var closestChild = root;
                var closestDistance = Vector3.Distance(transform.position, closestChild.position);
                foreach (Transform child in root)
                {
                    if (child.GetComponent<Dart>() != null || child.name == "DartTrackingPoint")
                    {
                        continue;
                    }

                    var distance = Vector3.Distance(transform.position, child.position);
                    if (distance < closestDistance)
                    {
                        closestChild = child;
                        closestDistance = distance;
                    }
                }

                // parent to closest child
                SetParent(closestChild);
            }
            else
            {
                // recurse up the parent chain until we find PlayerControllerB
                var found = false;
                var parent = collider.transform;
                while (!found)
                {
                    // if parent is null, return
                    if (parent == null)
                    {
                        break;
                    }
                    if (parent.GetComponent<PlayerControllerB>())
                    {
                        found = true;
                    }
                    else
                    {
                        parent = parent.parent;
                    }
                }

                if (!found)
                {
                    SetParent(collider.transform);
                }
                else
                {
                    // find closest child in terms of distance
                    var closestChild = parent;
                    var closestDistance = Vector3.Distance(transform.position, closestChild.position);
                    foreach (Transform child in parent)
                    {
                        if (child.GetComponent<Dart>() != null || child.name == "DartTrackingPoint")
                        {
                            continue;
                        }
                        var distance = Vector3.Distance(transform.position, child.position);
                        if (distance < closestDistance)
                        {
                            closestChild = child;
                            closestDistance = distance;
                        }
                    }

                    // parent to closest child
                    SetParent(closestChild);
                }
            }

            isKinematic.Value = true;
            rb.isKinematic = true;

            //Plugin.logger.LogMessage($"Parented to: {transform.parent}");
        }

        public void SetParent(Transform parent)
        {
            //Plugin.logger.LogInfo($"Setting parent to: {parent.name}");

            // if tracking point is null, create a empty gameobject and set it as the tracking point
            if (trackingPoint == null)
            {
                trackingPoint = new GameObject();
                trackingPoint.name = "DartTrackingPoint";
                trackingPoint.transform.SetParent(parent);
                trackingPoint.transform.position = transform.position;
                trackingPoint.transform.rotation = transform.rotation;
            }
            else
            {
                trackingPoint.transform.SetParent(parent);
                trackingPoint.transform.position = transform.position;
                trackingPoint.transform.rotation = transform.rotation;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (trackingPoint != null)
            {
                Destroy(trackingPoint);
            }
        }

        public void OnTriggerEnter(Collider collision)
        {

            if (IsHost && !collision.isTrigger && collision.gameObject.GetComponent<Dart>() == null && collision.gameObject.GetComponentInParent<Dart>() == null)
            {
                if (isThrown)
                {
                    var closestChild = collision.transform;

                    Plugin.logger.LogMessage($"(2) Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");

                    // if hit layer is not in mask, return
                    if ((Utilities.MaskForLayer(gameObject.layer) & 1 << closestChild.gameObject.layer) == 0)
                    {
                        return;
                    }
                    

                    if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)))
                    {
                        return;
                    }


                    TryParent(collision);

                    //Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;
                    PlayDartHitSoundServerRpc();

                }
            }

        }

        
        public void OnCollisionEnter(Collision collision)
        {
            

            if (IsHost && !collision.collider.isTrigger && collision.gameObject.GetComponent<Dart>() == null && collision.gameObject.GetComponentInParent<Dart>() == null)
            {



                if (isThrown)
                {
                    var closestChild = collision.transform;

                    Plugin.logger.LogMessage($"(3) Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");

                    // if hit layer is not in mask, return
                    if ((Utilities.MaskForLayer(gameObject.layer) & 1 << closestChild.gameObject.layer) == 0)
                    {
                        return;
                    }


                    if (playerThrownBy != null && (closestChild == playerThrownBy.transform || closestChild.IsChildOf(playerThrownBy.transform)))
                    {
                        return;
                    }


                    TryParent(collision.collider);

                    //Plugin.logger.LogMessage($"Hit target ({closestChild}) wawa!!! Layer: ({closestChild.gameObject.layer})");


                    isThrown = false;
                    rb.isKinematic = true;
                    PlayDartHitSoundServerRpc();

                }
            }

        }

    }
}
