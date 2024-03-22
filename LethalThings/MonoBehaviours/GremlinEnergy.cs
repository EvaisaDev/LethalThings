using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class GremlinEnergy : GrabbableObject
    {
        public AudioSource audioSource;
        public AudioClip[] drinkClips;
        private float transformChance = 1f;
        private float timeToDeathMin = 5f;
        private float timeToDeathMax = 10f;

        public static void Init()
        {
            On.GameNetworkManager.Start += GameNetworkManager_Start;
        }

        public static GameObject blobPrefab = null;

        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);

            List<NetworkPrefab> prefabs = self?.GetComponent<NetworkManager>()?.NetworkConfig?.Prefabs?.m_Prefabs;
            if (prefabs == null) return;

            foreach (var prefabContainer in prefabs)
            {
                GameObject prefab = prefabContainer?.Prefab;
                if (prefab?.GetComponent<BlobAI>()?.enemyType?.enemyName != "Blob") continue;

                blobPrefab = prefab;

                //Plugin.logger.LogMessage("Found blob prefab!");

                break;
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);


            int num = UnityEngine.Random.Range(0, drinkClips.Length);
            audioSource.PlayOneShot(drinkClips[num]);


            WalkieTalkie.TransmitOneShotAudio(audioSource, drinkClips[num], 1f);
            RoundManager.Instance.PlayAudibleNoise(base.transform.position, 20, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

            /*
            if(StartOfRound.Instance && !StartOfRound.Instance.inShipPhase && UnityEngine.Random.Range(0f, 1f) < transformChance)
            {
                // transform into a slime
                if (IsOwner)
                {
                    DelayedDeathServerRpc(UnityEngine.Random.Range(timeToDeathMin, timeToDeathMax));
                    //HUDManager.Instance.DisplayTip("Anomaly detected in vital signs.", "");
                }
            }*/

            if (base.IsOwner)
            {
                playerHeldBy.activatingItem = buttonDown;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);
            }
            StartCoroutine(undoAnimation());


        }

        public IEnumerator undoAnimation()
        {
            yield return new WaitForSeconds(3);
            if (base.IsOwner)
            {
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
                playerHeldBy.activatingItem = false;

                // damage player 
                playerHeldBy.DamagePlayer(50, causeOfDeath: CauseOfDeath.Unknown, deathAnimation: 1);

            }
        }


        /*

        [ServerRpc]
        public void DelayedDeathServerRpc(float timeToDeath)
        {
            DelayedDeathClientRpc(timeToDeath);
        }

        [ClientRpc]
        public void DelayedDeathClientRpc(float timeToDeath)
        {
            StartCoroutine(DelayedDeath(timeToDeath));
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            lastPlayerUsedBy.activatingItem = false;
            lastPlayerUsedBy.playerBodyAnimator.SetBool("useTZPItem", false);
        }
        */
        /*
        [ServerRpc]
        public void HandleKillSpawnServerRpc()
        {
            var blob = Instantiate(blobPrefab, playerHeldBy.transform.position, Quaternion.identity);
            blob.GetComponent<NetworkObject>().Spawn();
        }

        public IEnumerator DelayedDeath(float timeToDeath)
        {


            yield return new WaitForSeconds(timeToDeath);

            lastPlayerUsedBy.activatingItem = false;
            lastPlayerUsedBy.playerBodyAnimator.SetBool("useTZPItem", false);


            lastPlayerUsedBy.movementAudio.PlayOneShot(StartOfRound.Instance.bloodGoreSFX);
            WalkieTalkie.TransmitOneShotAudio(lastPlayerUsedBy.movementAudio, StartOfRound.Instance.bloodGoreSFX);




            if (IsOwner)
            {
                lastPlayerUsedBy.DiscardHeldObject();

                lastPlayerUsedBy.DropBlood();


                lastPlayerUsedBy.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown, 1);
                lastPlayerUsedBy.inSpecialInteractAnimation = false;
                lastPlayerUsedBy.inAnimationWithEnemy = null;


            }
        }*/

    }
}
