using System;
using System.Collections;
using System.Linq;
using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings
{
    public class RocketLauncher : GrabbableObject
    {
        public Light laserPointer;
        public Transform lightSource;

        public AudioSource mainAudio;

        public AudioClip[] activateClips;
        public AudioClip[] noAmmoSounds;

        public Transform aimDirection;

        public int maxAmmo = 4;

        private int currentAmmo;

        public GameObject missilePrefab;

        //public float LobForce = 100f;

        private float timeSinceLastShot;

        private PlayerControllerB previousPlayerHeldBy;

        public Material[] ammoLampMaterials;

        public Animator Animator;

        public ParticleSystem particleSystem;

        public override void Start()
        {
            currentAmmo = maxAmmo;

            for (int i = 0; i < ammoLampMaterials.Length; i++)
            {
                if (i >= currentAmmo)
                {
                    ammoLampMaterials[i].SetColor("_BaseColor", Color.red);
                    ammoLampMaterials[i].SetColor("_EmissiveColorMap", Color.red);
                }
                else
                {
                    ammoLampMaterials[i].SetColor("_BaseColor", Color.green);
                    ammoLampMaterials[i].SetColor("_EmissiveColorMap", Color.green);
                }
            }

            base.Start();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (currentAmmo > 0)
            {
                currentAmmo--;
                PlayRandomAudio(mainAudio, activateClips);
                Animator.Play("fire");
                particleSystem.Play();

                for (int i = 0; i < ammoLampMaterials.Length; i++)
                {
                    if (i >= currentAmmo)
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.red);
                    }
                    else
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.green);
                    }
                }

                if (IsOwner)
                {
                    if (IsHost)
                    {
                        MissileSpawner();
                    }
                    else
                    {
                        SpawnMissileServerRpc();
                    }
                }
            }
            else
            {
                PlayRandomAudio(mainAudio, noAmmoSounds);
            }
        }

        // server rpc for spawning missile
        [ServerRpc]
        private void SpawnMissileServerRpc()
        {
            MissileSpawner();
        }

        private void MissileSpawner()
        {
            // spawn missile
            GameObject missile = Instantiate(missilePrefab, aimDirection.position, aimDirection.rotation);

            // add force to missile
            // missile.GetComponent<Rigidbody>().AddForce(aimDirection.forward * LobForce);

            // set owner of missile
            missile.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }

        private void PlayRandomAudio(AudioSource audioSource, AudioClip[] audioClips)
        {
            if (audioClips.Length != 0)
            {
                audioSource.PlayOneShot(audioClips[UnityEngine.Random.Range(0, audioClips.Length)]);
            }
        }


        public override void LateUpdate()
        {
            base.LateUpdate();

            // raytace laser pointer
            RaycastHit hit;
            if (Physics.Raycast(lightSource.position, lightSource.forward, out hit, 100f, 1 << 3))
            {
                laserPointer.transform.position = hit.point;
                laserPointer.enabled = true;
            }
            else
            {
                laserPointer.enabled = false;
            }
        }



        private void OnEnable()
        {

        }



        private void OnDisable()
        {

        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (playerHeldBy != null)
            {
                previousPlayerHeldBy = playerHeldBy;
            }
        }


    }

}