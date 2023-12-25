using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace LethalThings.MonoBehaviours
{
    public class Arson : GrabbableObject
    {

        public AudioSource noiseAudio;

        public AudioSource noiseAudioFar;

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

        public bool isCleanable;

        public Item cleanArson;

        public bool arsonBeingShowered = false;

        public float showerTime = 0f;

        public float totalShowerTime = 5f;

        public static List<Arson> allArsonList = new List<Arson>();

        public bool canBeShowered = false;

        public ShowerTrigger currentShower = null;

        public static void Init()
        {
            On.ShowerTrigger.CheckBoundsForPlayers += ShowerTrigger_CheckBoundsForPlayers;
        }


        private static void ShowerTrigger_CheckBoundsForPlayers(On.ShowerTrigger.orig_CheckBoundsForPlayers orig, ShowerTrigger self)
        {

            
            if (Time.realtimeSinceStartup - self.cleanInterval < 1.5f)
            {
                return;
            }

            var collider = self.showerCollider;
            var bounds = collider.bounds;

            for (int i = 0; i < allArsonList.Count; i++)
            {
                var arson = allArsonList[i];

                if (arson.arsonBeingShowered || !arson.isCleanable)
                {
                    continue;
                }

                Plugin.logger.LogMessage("Checking arson " + arson.gameObject.name);

                var arsonCollider = arson.GetComponent<BoxCollider>();

                if (collider.bounds.Intersects(arsonCollider.bounds))
                {
                    arson.arsonBeingShowered = true;
                    arson.currentShower = self;
                    Plugin.logger.LogMessage("Eww!! stinky arson.");
                }
                else
                {
                    arson.arsonBeingShowered = false;
                    arson.showerTime = 0f;
                }
            }

            orig(self);


        }

        public override void Update()
        {
            base.Update();
            if (arsonBeingShowered)
            {
                if(currentShower == null || !currentShower.showerOn)
                {
                    arsonBeingShowered = false;
                    return;
                }

                showerTime += Time.deltaTime;

                if (showerTime >= totalShowerTime)
                {
                    arsonBeingShowered = false;
                    showerTime = 0f;

                    if (IsHost)
                    {
                        var gameObject = UnityEngine.Object.Instantiate(cleanArson.spawnPrefab, transform.position, transform.rotation);
                        gameObject.GetComponent<NetworkObject>().Spawn();

                        gameObject.transform.rotation = transform.rotation;

                        var grabbable = gameObject.GetComponent<GrabbableObject>();

                        if (grabbable && grabbable.itemProperties && grabbable.itemProperties.isScrap && RoundManager.Instance)
                        {
                            int price = (int)(Random.Range(grabbable.itemProperties.minValue, grabbable.itemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier);

                            grabbable.SetScrapValue(price);

                            syncScrapValueClientRpc(grabbable.GetComponent<NetworkObject>(), price, isInElevator, isInShipRoom);

                        }

                        GetComponent<NetworkObject>().Despawn(true);
                    }
                }
            }
        }


        [ClientRpc]
        public void syncScrapValueClientRpc(NetworkObjectReference obj, int scrapValue, bool itemIsInElevator, bool itemIsInShipRoom)
        {
            if (obj.TryGet(out NetworkObject targetObject))
            {
                var grabbable = targetObject.GetComponent<GrabbableObject>();

                if (grabbable)
                {
                    grabbable.SetScrapValue(scrapValue);

                    StartOfRound.Instance.localPlayerController.SetItemInElevator(itemIsInShipRoom, itemIsInElevator, grabbable);
                }
            }
        }

        public void Awake()
        {
            allArsonList.Add(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            allArsonList.Remove(this);
        }

        public override void Start()
        {
            base.Start();
            noisemakerRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!(GameNetworkManager.Instance.localPlayerController == null))
            {
                int num = noisemakerRandom.Next(0, noiseSFX.Length);
                float num2 = (float)noisemakerRandom.Next((int)(minLoudness * 100f), (int)(maxLoudness * 100f)) / 100f;
                float pitch = (float)noisemakerRandom.Next((int)(minPitch * 100f), (int)(maxPitch * 100f)) / 100f;
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
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, noiseRange, num2, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

                if(noiseSFX[num].name.Contains("chomp"))
                {
                    if (IsOwner)
                    {
                        playerHeldBy.DamagePlayer(30, causeOfDeath: CauseOfDeath.Mauling);

                        // drop item
                        playerHeldBy.DiscardHeldObject();
                    }
                }
            }
        }


    }
}
