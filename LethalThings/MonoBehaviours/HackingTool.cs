using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class HackingTool : GrabbableObject
    {
        private bool turnedOn = false;

        private RenderTexture renderTexture;

        public Camera renderCamera;

        public Material screenOffMat;
        public Material screenOnMat;

        public AudioSource audioSource;
        public AudioSource audioSourceFar;

        [Space(3f)]

        public AudioClip turnOnSound;
        public AudioClip turnOffSound;
        public AudioClip hackingSound;

        [Space(3f)]

        public int noiseRange = 45;


        public static void Load()
        {

        }

        public override void Start()
        {
            base.Start();
            mainObjectRenderer = transform.Find("Tool/Cube").GetComponent<MeshRenderer>();

            renderTexture = new RenderTexture(500, 390, 16, RenderTextureFormat.ARGB32);
            // setup camera to render to texture
            renderCamera.targetTexture = renderTexture;

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
            UnityEngine.Debug.Log("Switching screen: " + on);

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

            if (!IsOwner)
            {
                return;
            }


        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (!IsOwner)
            {
                return;
            }



        }
    }
}
