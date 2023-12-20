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
    public class FlareController : NetworkBehaviour
    {
        public float flareDuration = 20f;
        public float burnoutTime = 5f;
        public NetworkVariable<float> currentBurnTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public bool burntOut = false;
        public float attractRadius = 60f;
        private float startIntensity;
        private float smokeRate;
        private ParticleSystem particleSystem;
        private Light light;
        private float minimumGravity = 1f;
        private float maximumGravity = 6f;
        private Rigidbody rb;
        private ScanNodeProperties scanNodeProperties;
        private HUDManager hudManager;
        public NetworkVariable<float> initialVelocity = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public ParticleSystem popSystem;
        public AudioClip flarePopSound;
        public AudioSource popAudioSource;
        private int timesPlayed = 0;


        public static void Init()
        {
            On.HUDManager.NodeIsNotVisible += HUDManager_NodeIsNotVisible;
        }

        private static bool HUDManager_NodeIsNotVisible(On.HUDManager.orig_NodeIsNotVisible orig, HUDManager self, ScanNodeProperties node, int elementIndex)
        {
            if (node.transform.parent.GetComponent<FlareController>() != null)
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                // check if scan node is in field of view of the player
                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(node.transform.position);

                //UnityEngine.Debug.Log($"View pos: {viewPos}");

                // if node is not in field of view, return false
                if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0)
                {
                    return orig(self, node, elementIndex);
                }

                var flareController = node.transform.parent.GetComponent<FlareController>();
               // Debug.Log($"Burnt out? {flareController.burntOut}");
                if (!flareController.burntOut)
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


            particleSystem = GetComponentInChildren<ParticleSystem>();
            light = GetComponentInChildren<Light>();


            startIntensity = light.intensity;
            smokeRate = particleSystem.emission.rateOverTime.constant;


            rb = GetComponent<Rigidbody>();

            rb.useGravity = false;
            hudManager = HUDManager.Instance;

            scanNodeProperties = GetComponentInChildren<ScanNodeProperties>();

            //hudManager.AttemptScanNode(scanNodeProperties, 0, GameNetworkManager.Instance.localPlayerController);

            if (!hudManager.nodesOnScreen.Contains(scanNodeProperties))
            {
                hudManager.nodesOnScreen.Add(scanNodeProperties);
            }
            hudManager.AssignNodeToUIElement(scanNodeProperties);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                currentBurnTime.Value = flareDuration;
                initialVelocity.Value = rb.velocity.magnitude;
            }


        } 

        public void OnCollisionEnter(Collision other)
        {
            // get owner gameobject from NGO
            var ownerClientID = OwnerClientId;
            var ownerObject = StartOfRound.Instance.allPlayerScripts.First(x => x.OwnerClientId == ownerClientID);

            // damage multiplayer, current velocity vs initial velocity
            var damageMult = rb.velocity.magnitude / initialVelocity.Value;

            if (ownerObject != null)
            {
                if (other.collider.CompareTag("Player"))
                {
                    PlayerControllerB playerControllerB = other.gameObject.GetComponent<PlayerControllerB>();
                    if (!(playerControllerB != GameNetworkManager.Instance.localPlayerController) && playerControllerB != null && !playerControllerB.isPlayerDead)
                    {
                        if (RoundManager.Instance.insideAINodes.Length != 0)
                        {
                            Vector3 position3 = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                            position3 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position3);

                            playerControllerB.DamagePlayer((int)(10f * damageMult), causeOfDeath: CauseOfDeath.Unknown);

                        }
                    }
                }
                else if (other.collider.CompareTag("Enemy"))
                {


                    var enemyAICollision = other.gameObject.GetComponent<EnemyAICollisionDetect>();
                    if (enemyAICollision != null)
                    {
                        var enemyAI = enemyAICollision.mainScript;
                        enemyAI.HitEnemy((int)Mathf.Round(1f * damageMult), ownerObject);
                    }
                }
            }
        }

        public void FixedUpdate()
        {


            float gravity = Mathf.Max(minimumGravity, Mathf.Lerp(minimumGravity, maximumGravity, Mathf.InverseLerp(flareDuration - 2, flareDuration, currentBurnTime.Value)));

            //Debug.Log($"Gravity truly {gravity}");

            rb.AddForce(UnityEngine.Vector3.down * gravity, ForceMode.Acceleration);
        }   

        public void Update()
        {
            if (IsServer)
            {
                if (currentBurnTime.Value > -burnoutTime)
                {
                    currentBurnTime.Value -= Time.deltaTime;
                }
            }

            if (!burntOut && !HUDManager.Instance.nodesOnScreen.Contains(scanNodeProperties))
            {
                var player = GameNetworkManager.Instance.localPlayerController;

                var camera = player.gameplayCamera;

                var viewPos = camera.WorldToViewportPoint(scanNodeProperties.transform.position);

                //Debug.Log($"Wtf truly: {viewPos.x < 1 && viewPos.x > 0 && viewPos.y < 1 && viewPos.y > 0 && viewPos.z >= 0}");

                // if node is not in field of view, return false
                if (viewPos.x < 1 && viewPos.x > 0 && viewPos.y < 1 && viewPos.y > 0 && viewPos.z >= 0)
                {
                    if (!hudManager.nodesOnScreen.Contains(scanNodeProperties))
                    {
                        hudManager.nodesOnScreen.Add(scanNodeProperties);
                    }
                    hudManager.AssignNodeToUIElement(scanNodeProperties);
                }
            }

            if (currentBurnTime.Value <= 0f && !burntOut)
            {
                if (currentBurnTime.Value >= -burnoutTime)
                {
                    // reduce light intensity based on distance from 0 to burnout time
                    float intensity = Mathf.Lerp(startIntensity, 0f, Mathf.InverseLerp(0f, -burnoutTime, currentBurnTime.Value));
                    // reduce particle system emission rate based on distance from 0 to burnout time
                    float emissionRate = Mathf.Lerp(smokeRate, 0f, Mathf.InverseLerp(0f, -burnoutTime, currentBurnTime.Value));
                    light.intensity = intensity;

                    var emission = particleSystem.emission;
                    emission.rateOverTime = emissionRate;


                }
                else
                {
                    burntOut = true;

                    light.enabled = false;

                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    particleSystem.transform.SetParent(null);
                    particleSystem.transform.localScale = Vector3.one;

                    GetComponentInChildren<MeshRenderer>().enabled = false;

                    popSystem.Play();
                    popSystem.transform.SetParent(null);
                    popSystem.transform.localScale = Vector3.one;



                    popAudioSource.PlayOneShot(flarePopSound);

                    Destroy(popSystem.gameObject, popSystem.main.duration + popSystem.main.startLifetime.constant);

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

                    // kill system once all particles are dead
                    Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constant);
                    Plugin.logger.LogInfo("Flare burnt out");
                    StartCoroutine(KillFlare());

                }
                
            }
            else
            {
                light.enabled = true;
                
                if(particleSystem && particleSystem.isStopped)
                {
                    particleSystem.Play();
                }
            }

            if(!burntOut)
            {
                // get gameobjects with EnemyAI component in radius
                Collider[] colliders = Physics.OverlapSphere(transform.position, attractRadius);
                foreach (var collider in colliders)
                {
                    if (collider.gameObject.GetComponent<EnemyAI>() != null)
                    {

                        var enemyAI = collider.gameObject.GetComponent<EnemyAI>();
                        if(AccessTools.Method(enemyAI.GetType(), "DetectNoise").DeclaringType == typeof(EnemyAI))
                        {
                            enemyAI.StopSearch(enemyAI.currentSearch);
                            enemyAI.SetDestinationToPosition(transform.position);
                        }
                        else
                        {
                            enemyAI.DetectNoise(transform.position, 10000, 1, 24234);
                            Plugin.logger.LogInfo($"Playing noise for {enemyAI.name}");
                            timesPlayed++;
                        }

                        enemyAI.SetDestinationToPosition(transform.position);
                    }
                }

            }

        }

        public IEnumerator KillFlare()
        {
            yield return new WaitForSeconds(2);



            Destroy(gameObject);
        }

        // draw radius gizmo in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attractRadius);
        }

    }
}
