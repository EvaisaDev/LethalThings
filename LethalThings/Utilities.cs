using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalThings
{
    public class Utilities
    {
        private static Dictionary<int, int> _masksByLayer;
        public static void Init()
        {
            GenerateLayerMap();
        }

        public static void GenerateLayerMap()
        {
            _masksByLayer = new Dictionary<int, int>();
            for (int i = 0; i < 32; i++)
            {
                int mask = 0;
                for (int j = 0; j < 32; j++)
                {
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        mask |= 1 << j;
                    }
                }
                _masksByLayer.Add(i, mask);
            }
        }

        public static Transform TryFindRoot(Transform child)
        {
            // iterate upwards until we find a NetworkObject
            Transform current = child;
            while (current != null)
            {
                if (current.GetComponent<NetworkObject>() != null)
                {
                    return current;
                }
                current = current.transform.parent;
            }
            return null;
        }
        public static int MaskForLayer(int layer)
        {
            return _masksByLayer[layer];
        }

        public static void LoadPrefab(string name, Vector3 position)
        {
            if (Content.Prefabs.ContainsKey(name))
            {
                Plugin.logger.LogInfo($"Loading prefab {name}");
                var item = UnityEngine.Object.Instantiate(Content.Prefabs[name], position, Quaternion.identity);
                item.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                Plugin.logger.LogWarning($"Prefab {name} not found!");
            }
        }



        public static void TeleportPlayer(int playerObj, Vector3 teleportPos)
        {
            PlayerControllerB playerControllerB = StartOfRound.Instance.allPlayerScripts[playerObj];
            // no item dropping woooo
            //playerControllerB.DropAllHeldItems();
            if ((bool)UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
            {
                UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(playerControllerB);
            }
            playerControllerB.isInElevator = false;
            playerControllerB.isInHangarShipRoom = false;
            playerControllerB.isInsideFactory = true;
            playerControllerB.averageVelocity = 0f;
            playerControllerB.velocityLastFrame = Vector3.zero;
            StartOfRound.Instance.allPlayerScripts[playerObj].TeleportPlayer(teleportPos);
            StartOfRound.Instance.allPlayerScripts[playerObj].beamOutParticle.Play();
            if (playerControllerB == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }

        public static IEnumerator TeleportPlayerBody(int playerObj, Vector3 teleportPosition)
        {
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => StartOfRound.Instance.allPlayerScripts[playerObj].deadBody != null || Time.realtimeSinceStartup - startTime > 2f);
            if (StartOfRound.Instance.inShipPhase || SceneManager.sceneCount <= 1)
            {
                yield break;
            }
            DeadBodyInfo deadBody = StartOfRound.Instance.allPlayerScripts[playerObj].deadBody;
            if (deadBody != null)
            {
                deadBody.attachedTo = null;
                deadBody.attachedLimb = null;
                deadBody.secondaryAttachedLimb = null;
                deadBody.secondaryAttachedTo = null;
                if (deadBody.grabBodyObject != null && deadBody.grabBodyObject.isHeld && deadBody.grabBodyObject.playerHeldBy != null)
                {
                    deadBody.grabBodyObject.playerHeldBy.DropAllHeldItems();
                }
                deadBody.isInShip = false;
                deadBody.parentedToShip = false;
                deadBody.transform.SetParent(null, worldPositionStays: true);
                deadBody.SetRagdollPositionSafely(teleportPosition, disableSpecialEffects: true);
            }
        }

        public static void TeleportEnemy(EnemyAI enemy, Vector3 teleportPos)
        {
            enemy.serverPosition = teleportPos;
            enemy.transform.position = teleportPos;
            enemy.agent.Warp(teleportPos);
            enemy.SyncPositionToClients();
        }

        public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, int damage = 20, float minDamageRange = 0f, float maxDamageRange = 1f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
        {
            Debug.Log("Spawning explosion at pos: {explosionPosition}");

            Transform holder = null;

            if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null && RoundManager.Instance.mapPropsContainer.transform != null)
            {
                holder = RoundManager.Instance.mapPropsContainer.transform;
            }

            if (spawnExplosionEffect)
            {
                UnityEngine.Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), holder).SetActive(value: true);
            }

            float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
            if (num < 14f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            else if (num < 25f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }

            bool flag = false;
            Collider[] array = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);
            PlayerControllerB playerControllerB = null;
            RaycastHit hitInfo;
            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
                if (Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, out hitInfo, 1073742080, QueryTriggerInteraction.Ignore) && num2 > 4f)
                {
                    continue;
                }

                if (array[i].gameObject.layer == 3 && !flag)
                {
                    playerControllerB = array[i].gameObject.GetComponent<PlayerControllerB>();
                    if (playerControllerB != null && playerControllerB.IsOwner)
                    {
                        flag = true;
                        // calculate damage based on distance, so if player is minDamageRange or closer, they take full damage
                        // if player is maxDamageRange or further, they take no damage
                        // distance is num2
                        float damageMultiplier = 1f - Mathf.Clamp01((num2 - minDamageRange) / (maxDamageRange - minDamageRange));

                        playerControllerB.DamagePlayer((int)(damage * damageMultiplier), causeOfDeath: causeOfDeath);
                    }
                }
                else if (array[i].gameObject.layer == 21)
                {
                    Landmine componentInChildren = array[i].gameObject.GetComponentInChildren<Landmine>();
                    if (componentInChildren != null && !componentInChildren.hasExploded && num2 < 6f)
                    {
                        Debug.Log("Setting off other mine");
                        componentInChildren.StartCoroutine(componentInChildren.TriggerOtherMineDelayed(componentInChildren));
                    }
                }
                else if (array[i].gameObject.layer == 19)
                {
                    EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                    {
                        componentInChildren2.mainScript.HitEnemyOnLocalClient(enemyHitForce, playerWhoHit: attacker);
                    }
                }
            }

            int num3 = ~LayerMask.GetMask("Room");
            num3 = ~LayerMask.GetMask("Colliders");
            array = Physics.OverlapSphere(explosionPosition, 10f, num3);
            for (int j = 0; j < array.Length; j++)
            {
                Rigidbody component = array[j].GetComponent<Rigidbody>();
                if (component != null)
                {
                    component.AddExplosionForce(70f, explosionPosition, 10f);
                }
            }
        }
    }
}
