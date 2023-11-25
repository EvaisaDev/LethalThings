using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LethalThings
{
    public class Dingus : GrabbableObject
    {

        public AudioSource noiseAudio;

        public AudioSource noiseAudioFar;

        public AudioSource musicAudio;

        public AudioSource musicAudioFar;

        [Space(3f)]
        public AudioClip[] noiseSFX;

        public AudioClip[] noiseSFXFar;

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

        public override void Start()
        {
            base.Start();
            roundManager = FindObjectOfType<RoundManager>();
            noisemakerRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
            danceAnimator.Play("dingusDance");
            Debug.Log("Making the dingus dance");
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
            if (musicAudio.isPlaying)
            {
                danceAnimator.Play("dingusDance");
                Debug.Log("Making the dingus dance");
            }
            else
            {
                danceAnimator.Play("dingusIdle");
                Debug.Log("Making the dingus idle");
            }
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
            danceAnimator.Play("dingusIdle");
            Debug.Log("Making the dingus idle");
            if (IsOwner)
            {
                HUDManager.Instance.DisplayTip("Maxwell acquired", "Press E to toggle music.", isWarning: false, useSave: true, "LCTip_UseManual");
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            // toggle music
            if (right)
            {
                if (musicAudio.isPlaying)
                {
                    musicAudio.Pause();
                    musicAudioFar.Pause();
                }
                else
                {
                    musicAudio.Play();
                    musicAudioFar.Play();
                }
            }

        }

        public override void Update()
        {
            base.Update();
            if (musicAudio.isPlaying)
            {
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
            }

        }
    }

}
