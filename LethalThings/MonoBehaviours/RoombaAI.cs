using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LethalThings
{
    public class RoombaAI : EnemyAI
    {
        private float angeredTimer = 0f;
        [Header("Behaviors")]
        public AISearchRoutine searchForPlayers;
        public bool investigating = false;
        public bool hasBegunInvestigating = false;
        public Vector3 investigatePosition;

        [Header("Landmine")]
        private bool mineActivated = true;

        public bool hasExploded;

        //public ParticleSystem explosionParticle;

        //public Animator mineAnimator;

        public AudioSource mineAudio;

        public AudioSource mineFarAudio;

        public AudioSource idleSound;

        public AudioClip mineDetonate;

        public AudioClip mineTrigger;

        public AudioClip mineDetonateFar;

        public AudioClip beepNoise;

        //public AudioClip mineDeactivate;

        public AudioClip minePress;

        private bool sendingExplosionRPC;

        private RaycastHit hit;

        private RoundManager roundManager;

        //private float pressMineDebounceTimer;

        private bool localPlayerOnMine;

        private MeshRenderer meshRenderer;

        public Rigidbody Rigidbody;

        // blinking lights
        private List<Light> lights = new List<Light>();
        public float lightInterval = 1f;
        public float lightTimer = 0f;
        public float lightOnDuration = 0.1f;
        

        public override void Start()
        {
            base.Start();
            var root = transform.Find("BoombaModel/Roomba/Cube");

            meshRenderer = root.GetComponent<MeshRenderer>();
            lights = root.parent.GetComponentsInChildren<Light>().ToList();



            // print out the names of all the lights
            /*foreach (var light in lights)
            {
                Plugin.logger.LogInfo("[Boomba] found light, Light name: " + light.name);
            }*/
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead)
            {
                return;
            }


            var speed = Rigidbody.velocity.magnitude;
            idleSound.pitch = Mathf.Lerp(0.8f, 1.2f, speed / 2f);

            if (TargetClosestPlayer(4f, true, 70f))
            {
                StopSearch(searchForPlayers, true);
                movingTowardsTargetPlayer = true;
                hasBegunInvestigating = false;
                investigating = false;
                return;
            }
            if (investigating)
            {
                if (!hasBegunInvestigating)
                {
                    hasBegunInvestigating = true;
                    StopSearch(currentSearch, clear: false);
                    SetDestinationToPosition(investigatePosition);
                }
                if (Vector3.Distance(base.transform.position, investigatePosition) < 5f)
                {
                    investigating = false;
                    hasBegunInvestigating = false;
                }
                return;
            }

            if (!searchForPlayers.inProgress)
            {
                movingTowardsTargetPlayer = false;
                StartSearch(transform.position, searchForPlayers);
            }
        }

        private void FixedUpdate()
        {
            if (!ventAnimationFinished)
            {
                return;
            }

        }

        public IEnumerator disableLights(float timer)
        {
            yield return new WaitForSeconds(timer);
            foreach (var light in lights)
            {
                light.enabled = false;
            }
            //Plugin.logger.LogInfo("Light off");
        }

        public override void Update()
        {
            base.Update();

            // blinking lights and beeping
            if (lightTimer > 0f)
            {
                lightTimer -= Time.deltaTime;
                if (lightTimer <= 0f)
                {
                    foreach (var light in lights)
                    {
                        light.enabled = true;
                    }
                    //Plugin.logger.LogInfo("Light on");
                    StartCoroutine(disableLights(lightOnDuration));
                    // play audio and walkie
                    mineAudio.PlayOneShot(beepNoise);
                    WalkieTalkie.TransmitOneShotAudio(mineAudio, beepNoise);
                }
            }
            else
            {
                lightTimer = lightInterval;
            }
            

            if (!ventAnimationFinished || !(creatureAnimator != null))
            {
                return;
            }
            creatureAnimator.enabled = false;
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead)
            {
                return;
            }

            Vector3 serverPosition = this.serverPosition;

            /*if (pressMineDebounceTimer > 0f)
            {
                pressMineDebounceTimer -= Time.deltaTime;
            }*/

            if (stunNormalizedTimer > 0f)
            {
                agent.speed = 0f;
                angeredTimer = 7f;
                return;
            }
            else if (angeredTimer > 0f)
            {
                angeredTimer -= Time.deltaTime;
                if (IsOwner)
                {
                    agent.stoppingDistance = 0.1f;
                    agent.speed = 1f;
                    return;
                }
                return;
            }
            else
            {
                if (IsOwner)
                {
                    agent.stoppingDistance = 5f;
                    agent.speed = 0.8f;
                }
            }
        }


        /*public void ToggleMine(bool enabled)
        {
            if (mineActivated != enabled)
            {
                mineActivated = enabled;
                if (!enabled)
                {
                    mineAudio.PlayOneShot(mineDeactivate);
                    WalkieTalkie.TransmitOneShotAudio(mineAudio, mineDeactivate);
                }
                ToggleMineServerRpc(enabled);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleMineServerRpc(bool enable)
        {
            ToggleMineClientRpc(enable);
        }

        [ClientRpc]
        public void ToggleMineClientRpc(bool enable)
        {
            ToggleMineEnabledLocalClient(enable);
        }

        public void ToggleMineEnabledLocalClient(bool enabled)
        {
            if (mineActivated != enabled)
            {
                mineActivated = enabled;
                if (!enabled)
                {
                    mineAudio.PlayOneShot(mineDeactivate);
                    WalkieTalkie.TransmitOneShotAudio(mineAudio, mineDeactivate);
                }
            }
        }*/

        private IEnumerator StartIdleAnimation()
        {
            roundManager = Object.FindObjectOfType<RoundManager>();
            if (!(roundManager == null))
            {
                if (roundManager.BreakerBoxRandom != null)
                {
                    yield return new WaitForSeconds((float)roundManager.BreakerBoxRandom.NextDouble() + 0.5f);
                }
                //mineAnimator.SetTrigger("startIdle");
                mineAudio.pitch = Random.Range(0.9f, 1.1f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasExploded)
            {
                return;
            }
            // log tag and name
            Plugin.logger.LogInfo("[Boomba] Trigger enter, tag: " + other.tag + ", name: " + other.name);
            if (other.CompareTag("Player") || other.transform.parent.CompareTag("Player"))
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();

                if(!other.CompareTag("Player"))
                {
                    component = other.transform.parent.GetComponent<PlayerControllerB>();
                }

                if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
                {
                    localPlayerOnMine = true;
                    //pressMineDebounceTimer = 0.5f;
                    PressMineServerRpc();
                    StartCoroutine(TriggerMine(other));
                }
            }
            else
            {
                if (!other.CompareTag("PlayerRagdoll") /*&& !other.CompareTag("PhysicsProp")*/)
                {
                    return;
                }
                if ((bool)other.GetComponent<DeadBodyInfo>())
                {
                    if (other.GetComponent<DeadBodyInfo>().playerScript != GameNetworkManager.Instance.localPlayerController)
                    {
                        return;
                    }
                }
                else if ((bool)other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
                {
                    return;
                }
                //pressMineDebounceTimer = 0.5f;
                PressMineServerRpc();
                StartCoroutine(TriggerMine(other));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PressMineServerRpc()
        {
            PressMineClientRpc();
        }

        [ClientRpc]
        public void PressMineClientRpc()
        {
            //pressMineDebounceTimer = 0.3f;
            mineAudio.PlayOneShot(minePress);
            WalkieTalkie.TransmitOneShotAudio(mineAudio, minePress);


        }


        public IEnumerator TriggerMine(Collider other)
        {
            Debug.Log("Object entering mine trigger, gameobject name: " + other.gameObject.name);
            yield return new WaitForSeconds(0.5f);
            MineGoesBoom(other);
        }

        public void MineGoesBoom(Collider other)
        {
            if (!hasExploded)
            {
                Debug.Log("Object leaving mine trigger, gameobject name: " + other.gameObject.name);
                if (other.CompareTag("Player") || other.transform.parent.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();

                    if (!other.CompareTag("Player"))
                    {
                        component = other.transform.parent.GetComponent<PlayerControllerB>();
                    }

                    if (component != null && !component.isPlayerDead && !(component != GameNetworkManager.Instance.localPlayerController))
                    {
                        localPlayerOnMine = false;
                        TriggerMineOnLocalClientByExiting();
                    }
                }
                else
                {
                    if (!other.CompareTag("PlayerRagdoll") /*&& !other.CompareTag("PhysicsProp")*/)
                    {
                        return;
                    }
                    if ((bool)other.GetComponent<DeadBodyInfo>())
                    {
                        if (other.GetComponent<DeadBodyInfo>().playerScript != GameNetworkManager.Instance.localPlayerController)
                        {
                            return;
                        }
                    }
                    else if ((bool)other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
                    {
                        return;
                    }
                    TriggerMineOnLocalClientByExiting();
                }
            }
        }

        private void TriggerMineOnLocalClientByExiting()
        {
            if (!hasExploded)
            {
                hasExploded = true;
                SetOffMineAnimation();
                sendingExplosionRPC = true;
                ExplodeMineServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ExplodeMineServerRpc()
        {
            hasExploded = true;
            ExplodeMineClientRpc();
        }

        [ClientRpc]
        public void ExplodeMineClientRpc()
        {
            if (sendingExplosionRPC)
            {
                sendingExplosionRPC = false;
            }
            else
            {
                SetOffMineAnimation();
            }
        }

        public void SetOffMineAnimation()
        {
            hasExploded = true;
            //mineAnimator.SetTrigger("detonate");
            mineAudio.PlayOneShot(mineTrigger, 1f);
            // detonate mine after 0.5 seconds
            StartCoroutine(detonateMineDelayed());
        }

        private IEnumerator detonateMineDelayed ()
        {
            yield return new WaitForSeconds(0.5f);
            Detonate();
            // destroy mine
            Destroy(gameObject);
        }

        public void Detonate()  
        {
            mineAudio.pitch = Random.Range(0.93f, 1.07f);
            mineAudio.PlayOneShot(mineDetonate, 1f);
            Utilities.CreateExplosion(base.transform.position + Vector3.up, spawnExplosionEffect: true, 100, 5.7f, 6.4f);
        }

        public bool MineHasLineOfSight(Vector3 pos)
        {
            return !Physics.Linecast(base.transform.position, pos, out hit, 256);
        }


        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit, false);
            angeredTimer = 18f;
            SetOffMineAnimation();
            sendingExplosionRPC = true;
            ExplodeMineServerRpc();
        }

        public override void DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);

            float num = Vector3.Distance(noisePosition, base.transform.position);
            if (!(num > 15f) && !movingTowardsTargetPlayer)
            {
                investigatePosition = noisePosition;
            }
        }

        public void InvestigatePosition(Vector3 position)
        {
            if (!hasBegunInvestigating)
            {
                investigatePosition = position;
                investigating = true;
            }
        }


    }
}
