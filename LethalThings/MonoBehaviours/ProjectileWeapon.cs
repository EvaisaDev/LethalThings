using System;
using System.Collections;
using System.Linq;
using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using LethalLib.Modules;
using LethalThings.MonoBehaviours;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings
{
    public class ProjectileWeapon : SaveableObject
    {
        public AudioSource mainAudio;

        public AudioClip[] activateClips;
        public AudioClip[] noAmmoSounds;
        public AudioClip[] reloadSounds;

        public Transform aimDirection;

        public int maxAmmo = 4;
        [HideInInspector]
        private NetworkVariable<int> currentAmmo = new NetworkVariable<int>(4, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public GameObject projectilePrefab;

        public float LobForce = 100f;

        private float timeSinceLastShot;

        private PlayerControllerB previousPlayerHeldBy;

        public Animator Animator;

        public ParticleSystem particleSystem;

        public Item ammoItem;

        public int ammoSlotToUse = -1;


        public override void SaveObjectData()
        {
            SaveData.SaveObjectData<int>("projectileWeaponAmmoData", currentAmmo.Value, uniqueId);
        }

        public override void LoadObjectData()
        {
            if (IsHost)
            {
                currentAmmo.Value = SaveData.LoadObjectData<int>("projectileWeaponAmmoData", uniqueId);
            }
        }

        public static void Init()
        {
            //On.GrabbableObject.UseItemBatteries += GrabbableObject_UseItemBatteries;
        }

        private static bool GrabbableObject_UseItemBatteries(On.GrabbableObject.orig_UseItemBatteries orig, GrabbableObject self, bool isThrowable, bool buttonDown)
        {
            if(self is ProjectileWeapon)
            {
                return true;
            }
                
            return orig(self,isThrowable, buttonDown);
        }

        public override void Start()
        {

            base.Start();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                currentAmmo.Value = maxAmmo;
            }
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
                timeSinceLastShot = 0f;

                if (IsOwner)
                {
                   
                    if (IsHost)
                    {
                        ProjectileSpawner(aimDirection.position, aimDirection.rotation, aimDirection.forward);
                    }
                    else
                    {
                        SpawnProjectileServerRpc(aimDirection.position, aimDirection.rotation, aimDirection.forward);
                    }
                }
            }
            else
            {
                PlayRandomAudio(mainAudio, noAmmoSounds);
            }
        }

        private bool ReloadedGun()
        {
            int num = FindAmmoInInventory();
            if (num == -1)
            {
                return false;
            }
            ammoSlotToUse = num;
            return true;
        }

        private int FindAmmoInInventory()
        {
            for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
            {
                if (!(playerHeldBy.ItemSlots[i] == null))
                {
                    if(playerHeldBy.ItemSlots[i].itemProperties.itemId == ammoItem.itemId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }


        [ServerRpc]
        private void FixWeightServerRpc()
        {
            FixWeightClientRpc();
        }

        [ClientRpc]
        private void FixWeightClientRpc()
        {
            //Plugin.logger.LogInfo("Did RPC run??");
            if (playerHeldBy != null)
            {
                playerHeldBy.carryWeight -= Mathf.Clamp(ammoItem.weight - 1f, 0f, 10f);
                //Plugin.logger.LogInfo("Fixed weightloss for ammo because game is scuffed!!!");
            }
        }



        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
           // Plugin.logger.LogInfo("HELLO???");

            if (!right)
            {
                //Plugin.logger.LogInfo("Player pressed Q on projectile weapon");

                //Plugin.logger.LogInfo("Attempting reload frfr");

                if (IsOwner)
                {
                    if (ReloadedGun())
                    {

                        if (currentAmmo.Value > 0)
                        {
                            HUDManager.Instance.DisplayTip("Item already loaded.", "You can reload once you use up all the ammo.");
                        }
                        else
                        {
                            ReloadAmmoServerRpc();

                            //var lastWeight = playerHeldBy.carryWeight;
                            //playerHeldBy.DestroyItemInSlotAndSync(ammoSlotToUse);
                            DestroyItemInSlotAndSync(ammoSlotToUse);
                            /*if (lastWeight == playerHeldBy.carryWeight)
                            {
                                if (IsHost)
                                {
                                    FixWeightClientRpc();
                                }
                                else
                                {
                                    FixWeightServerRpc();
                                }
                            }*/
                            ammoSlotToUse = -1;

                            /*
                            if (IsHost)
                            {
                                currentAmmo.Value = maxAmmo;
                            }

                            if (IsOwner)
                            {

                            }


                            */
                        }
                    }
                    else
                    {
                        HUDManager.Instance.DisplayTip("No ammo found.", $"Buy {ammoItem.itemName} from the Terminal to reload.");
                    }
                }
            }
        }

        [ServerRpc]
        private void ReloadAmmoServerRpc()
        {
            currentAmmo.Value = maxAmmo;
            //Plugin.logger.LogInfo("Client attempted to reload gun!!");
            ReloadAmmoSoundClientRpc();
        }

        [ClientRpc]
        private void ReloadAmmoSoundClientRpc()
        {
            PlayRandomAudio(mainAudio, reloadSounds);
            Animator.Play("reload");
            //Plugin.logger.LogInfo("Reload successful!!");
        }

        // server rpc for spawning projectile
        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector3 aimPosition, Quaternion aimRotation, Vector3 forward)
        {
            ProjectileSpawner(aimPosition, aimRotation, forward);
        }

        private void ProjectileSpawner(Vector3 aimPosition, Quaternion aimRotation, Vector3 forward)
        {
            // spawn projectile
            GameObject projectile = Instantiate(projectilePrefab, aimPosition, aimRotation);

            // set owner of projectile
            projectile.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);

            ApplyProjectileForceClientRpc(projectile.GetComponent<NetworkObject>(), aimPosition, aimRotation, forward);
        }

        [ClientRpc]
        public void ApplyProjectileForceClientRpc(NetworkObjectReference projectile, Vector3 aimPosition, Quaternion aimRotation, Vector3 forward)
        {
            NetworkObject networkObject;
            if (projectile.TryGet(out networkObject))
            {
                GameObject projectileObject = networkObject.gameObject;
                projectileObject.transform.position = aimPosition;
                projectileObject.transform.rotation = aimRotation;
                projectileObject.GetComponent<Rigidbody>().AddForce(forward * LobForce, ForceMode.Impulse);
            }
        }

        private void PlayRandomAudio(AudioSource audioSource, AudioClip[] audioClips)
        {
            if (audioClips.Length != 0)
            {
                audioSource.PlayOneShot(audioClips[UnityEngine.Random.Range(0, audioClips.Length)]);
            }
        }


        public override void Update()
        {
            base.Update();
            timeSinceLastShot += Time.deltaTime;

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
                //playerHeldBy.activatingItem = false;
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
            playerHeldBy.equippedUsableItemQE = true;
        }


        public void DestroyItemInSlotAndSync(int itemSlot)
        {



            if (IsOwner)
            {
                if (itemSlot >= playerHeldBy.ItemSlots.Length || playerHeldBy.ItemSlots[itemSlot] == null)
                {
                    Debug.LogError($"Destroy item in slot called for a slot (slot {itemSlot}) which is empty or incorrect");
                }
                DestroyItemInSlotServerRpc(itemSlot);
            }
        }

        [ServerRpc]
        public void DestroyItemInSlotServerRpc(int itemSlot)
        {
            DestroyItemInSlotClientRpc(itemSlot);
        }

        [ClientRpc]
        public void DestroyItemInSlotClientRpc(int itemSlot)
        {
            DestroyItemInSlot(itemSlot);
        }

        public void DestroyItemInSlot(int itemSlot)
        {
            if (GameNetworkManager.Instance.localPlayerController == null || NetworkManager.Singleton == null || NetworkManager.Singleton.ShutdownInProgress)
            {
                return;
            }
            GrabbableObject grabbableObject = playerHeldBy.ItemSlots[itemSlot];

            if(grabbableObject == null || grabbableObject.itemProperties == null)
            {
                Plugin.logger.LogError("Item properties are null, cannot destroy item in slot");
                return;
            }

            playerHeldBy.carryWeight -= Mathf.Clamp(grabbableObject.itemProperties.weight - 1f, 0f, 10f);
            if (playerHeldBy.currentItemSlot == itemSlot)
            {

                playerHeldBy.isHoldingObject = false;
                playerHeldBy.twoHanded = false;
                if (playerHeldBy.IsOwner)
                {
                    playerHeldBy.playerBodyAnimator.SetBool("cancelHolding", value: true);
                    playerHeldBy.playerBodyAnimator.SetTrigger("Throw");
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    HUDManager.Instance.ClearControlTips();
                    playerHeldBy.activatingItem = false;
                }
            }
            if (IsOwner)
            {
                HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
            }
            if (playerHeldBy.currentlyHeldObjectServer != null && playerHeldBy.currentlyHeldObjectServer == grabbableObject)
            {
                if (playerHeldBy.IsOwner)
                {
                    playerHeldBy.SetSpecialGrabAnimationBool(setTrue: false, playerHeldBy.currentlyHeldObjectServer);
                    playerHeldBy.currentlyHeldObjectServer.DiscardItemOnClient();
                }
                playerHeldBy.currentlyHeldObjectServer = null;
            }

            playerHeldBy.ItemSlots[itemSlot] = null;
            if (IsServer)
            {
                grabbableObject.NetworkObject.Despawn(true);
            }
        }



    }

}