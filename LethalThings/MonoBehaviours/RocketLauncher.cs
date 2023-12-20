using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using LethalLib.Modules;
using LethalThings.MonoBehaviours;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings
{
    public class RocketLauncher : SaveableObject
    {
        public Light laserPointer;
        public Transform lightSource;

        public AudioSource mainAudio;

        public AudioClip[] activateClips;
        public AudioClip[] noAmmoSounds;

        public Transform aimDirection;

        public int maxAmmo = 4;

        private NetworkVariable<int> currentAmmo = new NetworkVariable<int>(4, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> isLaserOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public GameObject missilePrefab;

        //public float LobForce = 100f;

        private float timeSinceLastShot;

        private PlayerControllerB previousPlayerHeldBy;

        private Material[] ammoLampMaterials;

        public Animator Animator;

        public ParticleSystem particleSystem;

        public LineRenderer laserLine;

        private Transform laserRoot;

        public override void SaveObjectData()
        {
            SaveData.SaveObjectData<int>("rocketLauncherAmmoData", currentAmmo.Value, uniqueId);
        }

        public override void LoadObjectData()
        {
            if (IsHost)
            {
                currentAmmo.Value = SaveData.LoadObjectData<int>("rocketLauncherAmmoData", uniqueId);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Plugin.logger.LogInfo("OnNetworkSpawn");
            if (IsServer)
            {
                currentAmmo.Value = maxAmmo;
            }
        }

        public override void Awake()
        {
            base.Awake();

            // get materials from mesh renderer
            var renderer = GetComponentInChildren<MeshRenderer>();
            List<Material> materials = new List<Material>();
            for (int i = 1; i < renderer.materials.Length; i++)
            {
                materials.Add(renderer.materials[i]);
            }

            ammoLampMaterials = materials.ToArray();
        }

        public override void Start()
        {


            laserRoot = laserLine.transform.parent;

            var renderer = GetComponentInChildren<MeshRenderer>();

   

            for (int i = 0; i < ammoLampMaterials.Length; i++)
            {
                if (i >= currentAmmo.Value)
                {
                    ammoLampMaterials[i].SetColor("_BaseColor", Color.red);
                    ammoLampMaterials[i].SetColor("_EmissiveColor", Color.red);
                }
                else
                {
                    ammoLampMaterials[i].SetColor("_BaseColor", Color.green);
                    ammoLampMaterials[i].SetColor("_EmissiveColor", Color.green);
                }
            }

            base.Start();
        }

        public override void Update()
        {
            base.Update();

            // update every 30 calls
            if (Time.frameCount % 30 == 0)
            {
                for (int i = 0; i < ammoLampMaterials.Length; i++)
                {
                    if (i >= currentAmmo.Value)
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.red);
                        ammoLampMaterials[i].SetColor("_EmissiveColor", Color.red);
                    }
                    else
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.green);
                        ammoLampMaterials[i].SetColor("_EmissiveColor", Color.green);
                    }
                }
            }


        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);


            if (currentAmmo.Value > 0)
            {
                if (IsHost)
                {
                    currentAmmo.Value--;
                }

                PlayRandomAudio(mainAudio, activateClips);
                Animator.Play("fire");
                particleSystem.Play();

                for (int i = 0; i < ammoLampMaterials.Length; i++)
                {
                    if (i >= currentAmmo.Value)
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.red);
                        ammoLampMaterials[i].SetColor("_EmissiveColor", Color.red);
                    }
                    else
                    {
                        ammoLampMaterials[i].SetColor("_BaseColor", Color.green);
                        ammoLampMaterials[i].SetColor("_EmissiveColor", Color.green);
                    }
                }

                if (IsOwner)
                {
                    if (IsHost)
                    {
                        MissileSpawner(aimDirection.position, aimDirection.rotation);
                    }
                    else
                    {
                        SpawnMissileServerRpc(aimDirection.position, aimDirection.rotation);
                    }
                }
            }
            else
            {
                PlayRandomAudio(mainAudio, noAmmoSounds);
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (!right)
            {
                if(IsOwner)
                {
                    isLaserOn.Value = !isLaserOn.Value;
                }
            }
        }

        // server rpc for spawning missile
        [ServerRpc]
        private void SpawnMissileServerRpc(Vector3 aimPosition, Quaternion aimRotation)
        {
            MissileSpawner(aimPosition, aimRotation);
        }

        private void MissileSpawner(Vector3 aimPosition, Quaternion aimRotation)
        {
            // spawn missile
            GameObject missile = Instantiate(missilePrefab, aimPosition, aimRotation);

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
            if (Physics.Raycast(laserRoot.position, aimDirection.forward, out hit, 10f, 605030721))
            {
                laserPointer.transform.position = hit.point;
            }
            else
            {
               var pos = laserRoot.position + (aimDirection.forward * 10f);
               laserPointer.transform.position = pos;
            }
            laserLine.SetPosition(0, laserRoot.position);
            laserLine.SetPosition(1, laserPointer.transform.position);

            if (isLaserOn.Value)
            {
                laserPointer.enabled = true;
                laserLine.enabled = true;
            }
            else
            {
                laserPointer.enabled = false;
                laserLine.enabled = false;
            }
        }



        private void OnEnable()
        {

        }



        private void OnDisable()
        {

        }

        public override void PocketItem()
        {
            base.PocketItem();
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            if (playerHeldBy != null)
            {
                //playerHeldBy.activatingItem = false;
                playerHeldBy.equippedUsableItemQE = false;
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (playerHeldBy != null)
            {
                previousPlayerHeldBy = playerHeldBy;
                playerHeldBy.equippedUsableItemQE = true;
            }
        }

    }

}