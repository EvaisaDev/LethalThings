using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class HandheldRadar : GrabbableObject
    {
        private bool turnedOn = false;
        private Material screenOffMat;
        private Material screenOnMat;

        public AudioSource audioSource;
        public AudioSource audioSourceFar;

        [Space(3f)]

        public AudioClip turnOnSound;
        public AudioClip turnOffSound;
        public AudioClip switchTargetSound;

        [Space(3f)]

        public int noiseRange = 45;


        public static void Load()
        {
            On.ManualCameraRenderer.Update += ManualCameraRenderer_Update;
        }

        private static void ManualCameraRenderer_Update(On.ManualCameraRenderer.orig_Update orig, ManualCameraRenderer self)
        {
            orig(self);
            var anyPlayerHoldingRadar = false;
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player != null && player.currentlyHeldObject is HandheldRadar radar)
                {
                    if(radar.turnedOn)
                    {
                        anyPlayerHoldingRadar = true;
                        break;
                    }
                }
            }
            if (NetworkConfig.remoteRadarEnabled.Value && anyPlayerHoldingRadar) 
            { 
                if (self.mapCamera != null)
                {
                    self.mapCamera.enabled = true;
                }
            }
        }

        public override void Start()
        {
            base.Start();
            mainObjectRenderer = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
            screenOffMat = mainObjectRenderer.materials[1];
            screenOnMat = StartOfRound.Instance.mapScreen.onScreenMat;
        }

        public override void Update()
        {
            isBeingUsed = turnedOn;

            // if battery is dead, turn off
            if (turnedOn && insertedBattery.charge <= 0)
            {
                SwitchScreen(false);
            }

            base.Update();
        }

        [ServerRpc]
        public void SwitchScreenServerRpc(bool on)
        {
            SwitchScreenClientRpc(on);
            SwitchScreen(on);
        }

        [ClientRpc]
        public void SwitchScreenClientRpc(bool on)
        {
            SwitchScreen(on);
        }

        public void SwitchScreen(bool on)
        {
            UnityEngine.Debug.Log("Switching screen: "+on);

            turnedOn = on;

            var sound = on ? turnOnSound : turnOffSound;

            audioSource.PlayOneShot(sound, 1);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(sound, 1);
            }
            WalkieTalkie.TransmitOneShotAudio(audioSource, sound, 1);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

            if (on)
            {
                mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenOnMat });
            }
            else
            {
                mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenOffMat });
            }
        }

        [ServerRpc]
        public void PlayTargetSwitchSoundServerRpc()
        {
            PlayTargetSwitchSoundClientRpc();
        }

        [ClientRpc]
        public void PlayTargetSwitchSoundClientRpc()
        {
            PlayTargetSwitchSound();
        }

        public void PlayTargetSwitchSound()
        {
            UnityEngine.Debug.Log("Playing target switch sound");

            var sound = switchTargetSound;

            audioSource.PlayOneShot(sound, 1);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(sound, 1);
            }
            WalkieTalkie.TransmitOneShotAudio(audioSource, sound, 1);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }


        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!IsOwner || !turnedOn)
            {
                return;
            }

            StartOfRound.Instance.mapScreen.SwitchRadarTargetForward(true);
            if (IsHost)
            {
                PlayTargetSwitchSoundClientRpc();
            }
            else
            {
                PlayTargetSwitchSoundServerRpc();
            }

        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            UnityEngine.Debug.Log("Interacting with handheld radar");

            if(!IsOwner)
            {
                return;
            }

            if (!right)
            {
                turnedOn = !turnedOn;
                if (IsHost)
                {
                    SwitchScreenClientRpc(turnedOn);
                }
                else
                {
                    SwitchScreenServerRpc(turnedOn);
                }

            }

        }
    }
}
