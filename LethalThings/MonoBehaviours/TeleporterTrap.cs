using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalThings.MonoBehaviours
{
    public class TeleporterTrap : NetworkBehaviour
    {
        private RaycastHit hit;
        public float teleportCooldownTime = 5f;
        public float teleporterChargeUp = 2f;
        private NetworkVariable<float> teleportCooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public AudioSource teleporterAudio;
        public AudioClip teleporterBeamUpSFX;
        public AudioClip startTeleportingSFX;
        public AudioClip teleporterPrimeSFX;

        public void Update()
        {
            // if we are host, and teleport cooldown is over 0, decrease it
            if (IsHost && teleportCooldown.Value > 0f)
            {
                teleportCooldown.Value -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (teleportCooldown.Value > 0)
            {
                return;
            }
            if (other.CompareTag("Player"))
            {
                PlayerControllerB playerControllerB = other.gameObject.GetComponent<PlayerControllerB>();
                if (!(playerControllerB != GameNetworkManager.Instance.localPlayerController) && playerControllerB != null && !playerControllerB.isPlayerDead)
                {
                    if (RoundManager.Instance.insideAINodes.Length != 0)
                    {
                        Vector3 position3 = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                        position3 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position3);
                        // call teleport coroutine

                        teleporterAudio.PlayOneShot(teleporterPrimeSFX);
                        playerControllerB.movementAudio.PlayOneShot(teleporterPrimeSFX);
                        //StartCoroutine(PlayTeleportAudio());
                        if (IsHost)
                        {
                            teleportCooldown.Value = teleportCooldownTime;
                        }

                        if (playerControllerB.deadBody != null)
                        {
                            StartCoroutine(TeleportPlayerBodyCoroutine((int)playerControllerB.playerClientId, position3));
                            return;
                        }
                        StartCoroutine(TeleportPlayerCoroutine((int)playerControllerB.playerClientId, position3));
                    }
                }
            }
            else if (other.CompareTag("Enemy"))
            {


                var enemyAICollision = other.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (enemyAICollision != null)
                {
                    var enemyAI = enemyAICollision.mainScript;
                    if (RoundManager.Instance.insideAINodes.Length != 0)
                    {
                        Vector3 position3 = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                        position3 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position3);
                        // call teleport coroutine
                        StartCoroutine(TeleportEnemyCoroutine(enemyAI, position3));
                        teleporterAudio.PlayOneShot(teleporterPrimeSFX);
                        //StartCoroutine(PlayTeleportAudio());
                        if (IsHost)
                        {
                            teleportCooldown.Value = teleportCooldownTime;
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator PlayTeleportAudio()
        {
            yield return new WaitForSeconds(0.2f);
            teleporterAudio.PlayOneShot(startTeleportingSFX);
        }

        // coroutine for teleporting
        private System.Collections.IEnumerator TeleportPlayerCoroutine(int playerObj, Vector3 teleportPos)
        {
            yield return new WaitForSeconds(teleporterChargeUp);
            Utilities.TeleportPlayer(playerObj, teleportPos);
            TeleportPlayerServerRpc(playerObj, teleportPos);
        }

        private System.Collections.IEnumerator TeleportEnemyCoroutine(EnemyAI enemy, Vector3 teleportPos)
        {
            yield return new WaitForSeconds(teleporterChargeUp);
            Utilities.TeleportEnemy(enemy, teleportPos);
            // call server rpc with enemy object reference
            TeleportEnemyServerRpc(enemy.NetworkObject, teleportPos);
        }

        private System.Collections.IEnumerator TeleportPlayerBodyCoroutine(int playerObj, Vector3 teleportPos)
        {
            yield return new WaitForSeconds(teleporterChargeUp);
            TeleportPlayerBodyServerRpc(playerObj, teleportPos);
            StartCoroutine(Utilities.TeleportPlayerBody(playerObj, teleportPos));
        }


        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(int playerObj, Vector3 teleportPos)
        {
            TeleportPlayerClientRpc(playerObj, teleportPos);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportEnemyServerRpc(NetworkObjectReference enemy, Vector3 teleportPos)
        {
            TeleportEnemyClientRpc(enemy, teleportPos);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerBodyServerRpc(int playerObj, Vector3 teleportPos)
        {
            TeleportPlayerBodyClientRpc(playerObj, teleportPos);
        }


        [ClientRpc]
        public void TeleportPlayerClientRpc(int playerObj, Vector3 teleportPos)
        {
            teleporterAudio.PlayOneShot(teleporterBeamUpSFX);
            StartOfRound.Instance.allPlayerScripts[playerObj].movementAudio.PlayOneShot(teleporterBeamUpSFX);
            Utilities.TeleportPlayer(playerObj, teleportPos);
        }

        [ClientRpc]
        public void TeleportEnemyClientRpc(NetworkObjectReference enemy, Vector3 teleportPos)
        {
            var enemyObj = NetworkObjectReference.Resolve(enemy);
            teleporterAudio.PlayOneShot(teleporterBeamUpSFX);
            Utilities.TeleportEnemy(enemyObj.GetComponent<EnemyAI>(), teleportPos);
        }

        [ClientRpc]
        public void TeleportPlayerBodyClientRpc(int playerObj, Vector3 teleportPos)
        {
            teleporterAudio.PlayOneShot(teleporterBeamUpSFX);
            StartOfRound.Instance.allPlayerScripts[playerObj].movementAudio.PlayOneShot(teleporterBeamUpSFX);
            StartCoroutine(Utilities.TeleportPlayerBody(playerObj, teleportPos));
        }


    }
}
