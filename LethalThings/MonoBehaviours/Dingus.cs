using LethalLib.Modules;
using LethalThings.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LethalThings
{
    public class Dingus : SaveableObject
    {

        public AudioSource noiseAudio;

        public AudioSource noiseAudioFar;

        public AudioSource musicAudio;

        public AudioSource musicAudioFar;

        [Space(3f)]
        public AudioClip[] noiseSFX;

        public AudioClip[] noiseSFXFar;

        public AudioClip evilNoise;

        [Space(3f)]
        public float noiseRange;

        public float maxLoudness;

        public float minLoudness;

        public float minPitch;

        public float maxPitch;

        private System.Random noisemakerRandom;

        public Animator triggerAnimator;

        int timesPlayedWithoutTurningOff = 0;

        private RoundManager roundManager;

        private float noiseInterval = 1f;

        public Animator danceAnimator;

        public bool wasLoadedFromSave = false;

        public bool exploding = false;

        private NetworkVariable<bool> isEvil = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public GameObject evilObject;


        public NetworkVariable<bool> isPlayingMusic = new NetworkVariable<bool>(NetworkConfig.maxwellPlayMusicDefault.Value, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<bool> isPipebomb = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /*
        public override object SaveObjectData()
        {
            Plugin.logger.LogInfo("Saving object save data");
            return isEvil.Value;
        }

        public override void LoadObjectData(object saveData)
        {
            Plugin.logger.LogInfo("Loading object save data");
            wasLoadedFromSave = true;

            if (IsHost)
            {
                isEvil.Value = (bool)saveData;
            }
        }
        */

        public override void SaveObjectData()
        {
            SaveData.SaveObjectData<bool>("dingusBeEvil", isEvil.Value, uniqueId);
        }

        public override void LoadObjectData()
        {
            if (IsHost)
            {
                var dingusEvilStat = SaveData.LoadObjectData<bool>("dingusBeEvil", uniqueId);

                Plugin.logger.LogInfo($"Loading object[{uniqueId}] save data, evil? {dingusEvilStat}");

                if (dingusEvilStat) { 
                    isEvil.Value = dingusEvilStat;
                }
            }
        }


        public override void Start()
        {
            base.Start();
            roundManager = FindObjectOfType<RoundManager>();
            noisemakerRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 85);

            Debug.Log("Making the dingus dance");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //Plugin.logger.LogInfo("Networkspawn");

            //Plugin.logger.LogInfo($"{isPlayingMusic}, {isPlayingMusic.Value}, {Config.maxwellPlayMusicDefault}, {Config.maxwellPlayMusicDefault.Value}");


            if (IsOwner)
            {
                isPlayingMusic.Value = NetworkConfig.Instance.maxwellPlayMusicDefaultNetVar.Value;
            }

            if (IsHost)
            {
                isEvil.Value = (UnityEngine.Random.Range(0f, 100f) <= NetworkConfig.evilMaxwellChance.Value);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!(GameNetworkManager.Instance.localPlayerController == null))
            {
                int num = noisemakerRandom.Next(0, noiseSFX.Length);
                float num2 = noisemakerRandom.Next((int)(minLoudness * 100f), (int)(maxLoudness * 100f)) / 100f;
                float pitch = noisemakerRandom.Next((int)(minPitch * 100f), (int)(maxPitch * 100f)) / 100f;
                noiseAudio.pitch = pitch;
                noiseAudio.PlayOneShot(noiseSFX[num], num2);
                if (noiseAudioFar != null)
                {
                    noiseAudioFar.pitch = pitch;
                    noiseAudioFar.PlayOneShot(noiseSFXFar[num], num2);
                }
                if (triggerAnimator != null)
                {
                    triggerAnimator.SetTrigger("playAnim");
                }
                WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[num], num2);
                RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, num2, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            }
        }


        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
            isBeingUsed = false;
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
            danceAnimator.Play("dingusIdle");
            Debug.Log("Making the dingus idle");
            /*if (IsOwner)
            {
                HUDManager.Instance.DisplayTip("Maxwell acquired", "Press Q to toggle music.");
            }*/
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            // toggle music
            if (!right)
            {
                if (IsOwner)
                {
                    isPlayingMusic.Value = (!isPlayingMusic.Value);
                }
            }

        }

        public override void InteractItem()
        {
            base.InteractItem();

            // disable music and animation

            //Plugin.logger.LogInfo($"Interacting with maxwell, evil? {isEvil.Value} && exploding? {exploding}");

            if (isEvil.Value && !exploding && !isPipebomb.Value) 
            {
                EvilMaxwellServerRpc();
            }


        }

        public void EvilMaxwellTruly()
        {
            danceAnimator.Play("dingusIdle");
            if (musicAudio.isPlaying)
            {
                musicAudio.Pause();
                musicAudioFar.Pause();
            }

            // evil maxwell moment
            StartCoroutine(evilMaxwellMoment());
        }

        [ServerRpc(RequireOwnership = false)]
        public void EvilMaxwellServerRpc()
        {
            //EvilMaxwellTruly();

            isPipebomb.Value = true;

            EvilMaxwellClientRpc();

            //Plugin.logger.LogInfo("Evil maxwell moment Server");
        }

        [ClientRpc]
        public void EvilMaxwellClientRpc()
        {
            EvilMaxwellTruly();
            exploding = true;
            if (IsOwner)
            {
                isPlayingMusic.Value = (false);
            }
            timesPlayedWithoutTurningOff = 0;
            danceAnimator.Play("dingusIdle");
            if (musicAudio.isPlaying)
            {
                musicAudio.Pause();
                musicAudioFar.Pause();
            }

            Plugin.logger.LogInfo("Evil maxwell moment");
        }

        public IEnumerator evilMaxwellMoment()
        {



            yield return new WaitForSeconds(1f);
            noiseAudio.PlayOneShot(evilNoise, 1);

            evilObject.SetActive(true);
            mainObjectRenderer.enabled = false;

            if (noiseAudioFar != null)
            {
                noiseAudioFar.PlayOneShot(evilNoise, 1);
            }
            if (triggerAnimator != null)
            {
                triggerAnimator.SetTrigger("playAnim");
            }
            WalkieTalkie.TransmitOneShotAudio(noiseAudio, evilNoise, 1);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

            yield return new WaitForSeconds(1.5f);


            // explode
            Utilities.CreateExplosion(transform.position, true, 100, 0f, 6.4f);

            // set rigidbodies to non kinematic
            foreach (var rb in evilObject.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
                // apply force outwards from center
                rb.AddExplosionForce(1000f, evilObject.transform.position, 100f);
            }


            yield return new WaitForSeconds(2f);
            // destroy
            if(IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }

        public override void Update()
        {
            base.Update();

            if (isEvil.Value)
            {
                grabbable = false;
                grabbableToEnemies = false;
            }

            if (isPlayingMusic.Value && !exploding)
            {
                if (!musicAudio.isPlaying)
                {
                    musicAudio.Play();
                    musicAudioFar.Play();
                }

                if (!isHeld)
                {
                    danceAnimator.Play("dingusDance");
                }
                else
                {
                    danceAnimator.Play("dingusIdle");
                }
                if (noiseInterval <= 0f)
                {
                    noiseInterval = 1f;
                    timesPlayedWithoutTurningOff++;
                    roundManager.PlayAudibleNoise(transform.position, 16f, 0.9f, timesPlayedWithoutTurningOff, noiseIsInsideClosedShip: false, 5);
                }
                else
                {
                    noiseInterval -= Time.deltaTime;
                }
            }
            else
            {
                timesPlayedWithoutTurningOff = 0;
                danceAnimator.Play("dingusIdle");
                if (musicAudio.isPlaying)
                {
                    musicAudio.Pause();
                    musicAudioFar.Pause();
                }
            }

        }
    }

}
