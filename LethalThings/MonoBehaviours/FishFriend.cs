﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace LethalThings.MonoBehaviours
{
    public class FishFriend : EnemyAI
    {
        private Transform localPlayerCamera;

        public Transform turnCompass;

        private Vector3 agentLocalVelocity;
        private float agentLocalSpeed;

        public Collider thisEnemyCollider;

        private Vector3 previousPosition;

        public float playerEvadeDistance = 10f;
        public float playerSpookDistance = 4f;

        public float playerEvadeSpeed = 5f;

        public float playerClosestDistance = 2f;
        public float PlayerFarthestDistance = 3f;

        private float evadeTime = 0f;
        public float evadeTimeMax = 10f;

        private float lastPlayerInvestigateTime = 0f;
        public float playerInvestigateInterval = 20f;

        public float maxInvestigateTime = 5f;
        private float investigateTime = 0f;

        public float talkIntervalMax = 1f;
        public float talkIntervalMin = 0.3f;

        private float talkInterval = 0f;
        private float nextTalkTime = 0f;


        public ParticleSystem loveParticles;
        public ParticleSystem hateParticles;

        private PlayerControllerB favouritePlayer;
        private int tamedLevel = 0;

        public Animator animator;

        public InteractTrigger petTrigger;

        public AudioClip[] voices;
        public AudioClip petHappy;

        public Transform gemTransform;
        public Item gemItem;

        public override void Start()
        {
            base.Start();
            movingTowardsTargetPlayer = true;
            localPlayerCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
        }

        public override void DoAIInterval()
        {
            if (StartOfRound.Instance.livingPlayers == 0)
            {
                base.DoAIInterval();
                return;
            }
            if (TargetClosestPlayer())
            {

                // sneaking up on player
                if (currentBehaviourStateIndex == 0)
                {
                    if(Vector3.Distance(transform.position, targetPlayer.transform.position) > PlayerFarthestDistance)
                    {
                        ChooseRandomNodeAroundPlayer();
                    }
                    
                    Plugin.logger.LogInfo($"(Fibsh fren) We are sneaking up on {targetPlayer.playerUsername}!!");
                    lastPlayerInvestigateTime = 0f;

                    if(investigateTime > maxInvestigateTime)
                    {
                        SwitchToBehaviourState(2);
                    }
                    evadeTime = 0f;
                    agent.speed = 0.9f;
                    petTrigger.interactable = true;
                }
                // evading player
                else if (currentBehaviourStateIndex == 1)
                {
                    AvoidClosestPlayer();
                    Plugin.logger.LogInfo($"(Fibsh fren) We are evading {targetPlayer.playerUsername}!!");
                    lastPlayerInvestigateTime = 0f;
                    investigateTime = 0f;

                    if (evadeTime > evadeTimeMax || Vector3.Distance(transform.position, targetPlayer.transform.position) >= playerEvadeDistance)
                    {
                        Plugin.logger.LogInfo($"(Fibsh fren) We are done evading {targetPlayer.playerUsername}!!");
                        SwitchToBehaviourState(2);
                    }
                    agent.speed = 5f;

                    evadeTime += Time.deltaTime;
                    petTrigger.interactable = false;
                }
                // roaming
                else if (currentBehaviourStateIndex == 2)
                {
                    investigateTime = 0f;
                    //Plugin.logger.LogInfo($"(Fibsh fren) We are roaming!!");
                    // if we are close enough to the target node, choose a new one
                    if (targetNode == null || Vector3.Distance(transform.position, targetNode.position) < 2f)
                    {
                        ChooseRandomNodeAroundPlayer();
                    }
                    petTrigger.interactable = false;

                    Plugin.logger.LogInfo($"(Fibsh fren) {lastPlayerInvestigateTime} / {playerInvestigateInterval}");

                    agent.speed = 1f;

                    if (lastPlayerInvestigateTime >= playerInvestigateInterval)
                    {
                        if (Random.Range(0, 100) <= 30)
                        {
                            SwitchToBehaviourState(0);
                            Plugin.logger.LogInfo($"(Fibsh fren) We are investigating {targetPlayer.playerUsername}!!");
                        }
                    }
                }

                // check target player velocity, if they are moving too fast and we are close enough, switch to evade mode
                if (currentBehaviourStateIndex != 1)
                {
                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (player == null)
                        {
                            continue;
                        }

                        var tamedValue = !(Random.Range(1, 100) <= (tamedLevel * 10));

                        if (tamedValue && ((targetPlayer.thisController.velocity.magnitude > playerEvadeSpeed && Vector3.Distance(transform.position, targetPlayer.transform.position) < playerEvadeDistance) || (Vector3.Distance(transform.position, targetPlayer.transform.position) < playerSpookDistance && currentBehaviourStateIndex != 0)))
                        {
                            SwitchToBehaviourState(1);
                            Plugin.logger.LogInfo($"(Fibsh fren) Player {player.playerUsername} is spooking us!!");
                            break;
                        }


                    }
                }


            }


            base.DoAIInterval();
        }

        public void wasPet(PlayerControllerB player)
        {
            // log player
            if (currentBehaviourStateIndex == 0)  
            {
                Plugin.logger.LogInfo($"Player {player.playerUsername} petted {gameObject.name}");
                increaseLove(player, 1);
            }

        }

        public void increaseLove(PlayerControllerB player, int value)
        {

            increaseLoveServerRpc(player.playerClientId, value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void increaseLoveServerRpc(ulong playerID, int value)
        {
            increaseLoveClientRpc(playerID, value);
        }

        [ClientRpc]
        public void increaseLoveClientRpc(ulong playerID, int value)
        {
            tamedLevel += value;

            animator.SetTrigger("nuzzle");

            var player = StartOfRound.Instance.allPlayerScripts[playerID];

            favouritePlayer = player;

            creatureSFX.PlayOneShot(petHappy);
            WalkieTalkie.TransmitOneShotAudio(creatureVoice, petHappy);
            RoundManager.Instance.PlayAudibleNoise(base.transform.position, 12f, 0.6f, 0, noiseIsInsideClosedShip: false, 911);

            loveParticles.Play();

        }


        public void decreaseLove(PlayerControllerB player, int value)
        {

            decreaseLoveServerRpc(player.playerClientId, value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void decreaseLoveServerRpc(ulong playerID, int value)
        {
            decreaseLoveClientRpc(playerID, value);
        }

        [ClientRpc]
        public void decreaseLoveClientRpc(ulong playerID, int value)
        {
            tamedLevel -= value;

            var player = StartOfRound.Instance.allPlayerScripts[playerID];

            if(favouritePlayer == player)
            {
                favouritePlayer = null;
            }

            hateParticles.Play();
        }


        public void AvoidClosestPlayer()
        {
           // get point from current position to target position in opposite direction, at specified distance
            Vector3 point = this.transform.position + (this.transform.position - targetPlayer.transform.position).normalized * 1000;

            // get closest node to point
            Transform transform = ChooseClosestNodeToPosition(point);

            if (transform != null)
            {
                targetNode = transform;
                SetDestinationToPosition(targetNode.position);
                return;
            }

            movingTowardsTargetPlayer = false;
            
        }



        public void ChooseRandomNodeAroundPlayer()
        {
            Transform transform = ChooseRandomNodeAroundPosition(targetPlayer.transform.position, playerClosestDistance, PlayerFarthestDistance);

            if (transform != null)
            {
                targetNode = transform;
                SetDestinationToPosition(targetNode.position);
                return;
            }
           
            movingTowardsTargetPlayer = true;
        }

        public Transform ChooseRandomNodeAroundPosition(Vector3 pos, float minDist, float maxDist, bool avoidLineOfSight = false, int offset = 0)
        {
            // Filter nodes based on distance
            var filteredNodes = allAINodes
                .Where(node =>
                    Vector3.Distance(pos, node.transform.position) >= minDist &&
                    Vector3.Distance(pos, node.transform.position) <= maxDist)
                .ToArray();

            // If there are no nodes within the specified distance range, return null
            if (filteredNodes.Length == 0)
            {
                return null;
            }

            // Shuffle the filtered nodes array to get a random order
            for (int i = 0; i < filteredNodes.Length - 1; i++)
            {
                int randomIndex = Random.Range(i, filteredNodes.Length);
                var temp = filteredNodes[i];
                filteredNodes[i] = filteredNodes[randomIndex];
                filteredNodes[randomIndex] = temp;
            }

            // Iterate through the shuffled array and return the first node that satisfies line of sight conditions
            foreach (var node in filteredNodes)
            {
                if (!PathIsIntersectedByLineOfSight(node.transform.position, calculatePathDistance: false, avoidLineOfSight))
                {
                    if (offset == 0)
                    {
                        return node.transform;
                    }
                    offset--;
                }
            }

            return null;
        }

        public Transform ChooseRandomNode()
        {
            var randomNode = allAINodes[Random.Range(0, allAINodes.Length)];

            return randomNode.transform;
        }



        public override void Update()
        {
            base.Update();
            if (isEnemyDead)
            {
                return;
            }


            openDoorSpeedMultiplier = 1f;

            // overlapCircle to check for doors
            if (IsHost)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, 1 << 9);

                if (colliders.Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        DoorLock doorLock;
                        if (collider.gameObject.TryGetComponent<DoorLock>(out doorLock))
                        {
                            if (!doorLock.isLocked && !doorLock.isDoorOpened)
                            {
                                doorLock.OpenDoorAsEnemyServerRpc();
                            }

                            break;
                        }
                    }
                }
            }
            // handle talking
            if (talkInterval > nextTalkTime)
            {
                var index = RoundManager.PlayRandomClip(creatureVoice, voices);
                WalkieTalkie.TransmitOneShotAudio(creatureVoice, voices[index]);
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, 12f, 0.6f, 0, noiseIsInsideClosedShip: false, 911);

                nextTalkTime = Random.Range(talkIntervalMin, talkIntervalMax);
                talkInterval = 0f;
            }

            talkInterval += Time.deltaTime;
            lastPlayerInvestigateTime += Time.deltaTime;

            if (currentBehaviourStateIndex == 0)
            {
                // if we are close enough, increase investigate time
                if (Vector3.Distance(transform.position, targetPlayer.transform.position) < PlayerFarthestDistance)
                {
                    investigateTime += Time.deltaTime;
                }
            }

            // calculate velocity based on position
            agentLocalVelocity = (transform.position - previousPosition) / Time.deltaTime;

            // calculate speed based on velocity
            agentLocalSpeed = agentLocalVelocity.magnitude;



            // set previous position
            previousPosition = transform.position;
        }

        public bool gemDropped = false;

        public override void KillEnemy(bool destroy = false)
        {
            animator.SetTrigger("killed");
            petTrigger.enabled = false;


            StartCoroutine(dropGem());
        

            base.KillEnemy(destroy);
        }

        public IEnumerator dropGem()
        {
            gemTransform.gameObject.SetActive(false);
            GetComponentInChildren<Light>().enabled = false;

            if (IsOwner)
            {
                dropGemServerRpc();
            }

            // wait for animation to finish
            yield return new WaitForSeconds(3f);
        }

        [ServerRpc]
        public void dropGemServerRpc()
        {
            if (!gemDropped)
            {
                GameObject obj = UnityEngine.Object.Instantiate(gemItem.spawnPrefab, transform.position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                var price = (int)(RoundManager.Instance.AnomalyRandom.Next(gemItem.minValue, gemItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
                component.scrapValue = price;
                NetworkObject component2 = obj.GetComponent<NetworkObject>();
                component2.Spawn();

                StartCoroutine(waitForGemToSpawnToSync(component2, price));
                gemDropped = true;
            }
        }

        private IEnumerator waitForGemToSpawnToSync(NetworkObjectReference spawnedScrap, int scrapValues)
        {
            yield return new WaitForSeconds(0.7f);

            syncGemValueClientRpc(spawnedScrap, scrapValues);
            
        }

        [ClientRpc]
        public void syncGemValueClientRpc(NetworkObjectReference obj, int price)
        {
            Plugin.logger.LogInfo($"Syncing gem value to {price}");
            RoundManager.Instance.totalScrapValueInLevel += price;
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
                if (component != null)
                {

                    component.SetScrapValue(price);
                    if (component.itemProperties.meshVariants.Length != 0)
                    {
                        component.gameObject.GetComponent<MeshFilter>().mesh = component.itemProperties.meshVariants[RoundManager.Instance.ScrapValuesRandom.Next(0, component.itemProperties.meshVariants.Length)];
                    }
                    try
                    {
                        if (component.itemProperties.materialVariants.Length != 0)
                        {
                            component.gameObject.GetComponent<MeshRenderer>().sharedMaterial = component.itemProperties.materialVariants[RoundManager.Instance.ScrapValuesRandom.Next(0, component.itemProperties.materialVariants.Length)];
                        }
                    }
                    catch (Exception arg)
                    {
                        Debug.Log($"Item name: {component.gameObject.name}; {arg}");
                    }
                }
                else
                {
                    Debug.LogError("Scrap networkobject object did not contain grabbable object!: " + networkObject.gameObject.name);
                }
            }
            else
            {
                Debug.LogError($"Failed to get networkobject reference for scrap. id: {obj.NetworkObjectId}");
            }
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit);
            if (isEnemyDead)
            {
                return;
            }


            enemyHP -= force;

            if (enemyHP <= 0 && base.IsOwner)
            {
                KillEnemyOnOwnerClient();
            }
            else
            {

                // run away
                SwitchToBehaviourState(1);
                evadeTime = 0f;

                decreaseLove(playerWhoHit, 1);

                // play hurt animation
                animator.SetTrigger("hurt");
            }
        }
    }
}
