using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using MonoMod.Cil;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace LethalThings
{
    public class RoombaAI : EnemyAI
    {
        public override void Start()
        {
            base.Start();
            this.nearPlayerColliders = new Collider[4];
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();
            if (StartOfRound.Instance.livingPlayers == 0)
            {
                return;
            }
            if (this.isEnemyDead)
            {
                return;
            }
            int currentBehaviourStateIndex = this.currentBehaviourStateIndex;
            if (currentBehaviourStateIndex != 0)
            {
                if (currentBehaviourStateIndex != 1)
                {
                    return;
                }
                this.CheckForVeryClosePlayer();
                if (this.lostPlayerInChase)
                {
                    this.movingTowardsTargetPlayer = false;
                    if (!this.searchForPlayers.inProgress)
                    {
                        this.searchForPlayers.searchWidth = 30f;
                        base.StartSearch(this.lastPositionOfSeenPlayer, this.searchForPlayers);
                        Debug.Log("Crawler: Lost player in chase; beginning search where the player was last seen");
                        return;
                    }
                }
                else if (this.searchForPlayers.inProgress)
                {
                    base.StopSearch(this.searchForPlayers, true);
                    this.movingTowardsTargetPlayer = true;
                    Debug.Log("Crawler: Found player during chase; stopping search coroutine and moving after target player");
                }
            }
            else if (!this.searchForPlayers.inProgress)
            {
                base.StartSearch(base.transform.position, this.searchForPlayers);
                Debug.Log(string.Format("Crawler: Started new search; is searching?: {0}", this.searchForPlayers.inProgress));
                return;
            }
        }

        public override void FinishedCurrentSearchRoutine()
        {
            base.FinishedCurrentSearchRoutine();
            this.searchForPlayers.searchWidth = Mathf.Clamp(this.searchForPlayers.searchWidth + 10f, 1f, this.maxSearchAndRoamRadius);
        }




        public override void Update()
        {
            base.Update();
            if (this.isEnemyDead)
            {
                return;
            }
            if (!base.IsOwner)
            {
                this.inSpecialAnimation = false;
            }
            this.CalculateAgentSpeed();
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(base.transform.position + Vector3.up * 0.25f, 80f, 25, 5f))
            {
                if (this.currentBehaviourStateIndex == 1)
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 1f);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 0.5f);
                }
            }
            int currentBehaviourStateIndex = this.currentBehaviourStateIndex;
            if (currentBehaviourStateIndex != 0)
            {
                if (currentBehaviourStateIndex != 1)
                {
                    return;
                }
                if (!this.hasEnteredChaseMode)
                {
                    this.hasEnteredChaseMode = true;
                    this.lostPlayerInChase = false;
                    this.checkLineOfSightInterval = 0f;
                    this.noticePlayerTimer = 0f;
                    this.beginningChasingThisClient = false;
                    this.useSecondaryAudiosOnAnimatedObjects = true;
                    this.openDoorSpeedMultiplier = 1.5f;
                    this.agent.stoppingDistance = 0.5f;
                    this.agent.speed = 0f;
                }
                if (!base.IsOwner)
                {
                    return;
                }
                if (this.stunNormalizedTimer > 0f)
                {
                    return;
                }
                if (this.checkLineOfSightInterval <= 0.075f)
                {
                    this.checkLineOfSightInterval += Time.deltaTime;
                    return;
                }
                this.checkLineOfSightInterval = 0f;
                if (this.inSpecialAnimation)
                {
                    return;
                }
                if (this.lostPlayerInChase)
                {
                    PlayerControllerB playerControllerB = base.CheckLineOfSightForPlayer(55f, 60, -1);
                    if (playerControllerB)
                    {
                        this.noticePlayerTimer = 0f;
                        this.lostPlayerInChase = false;
                        this.MakeScreechNoiseServerRpc();
                        if (playerControllerB != this.targetPlayer)
                        {
                            base.SetMovingTowardsTargetPlayer(playerControllerB);
                            base.ChangeOwnershipOfEnemy(playerControllerB.actualClientId);
                            return;
                        }
                    }
                    else
                    {
                        this.noticePlayerTimer -= 0.075f;
                        if (this.noticePlayerTimer < -15f)
                        {
                            base.SwitchToBehaviourState(0);
                            return;
                        }
                    }
                }
                else
                {
                    PlayerControllerB playerControllerB2 = base.CheckLineOfSightForPlayer(65f, 80, -1);
                    if (playerControllerB2 != null)
                    {
                        this.noticePlayerTimer = 0f;
                        this.lastPositionOfSeenPlayer = playerControllerB2.transform.position;
                        if (playerControllerB2 != this.targetPlayer)
                        {
                            this.targetPlayer = playerControllerB2;
                            base.ChangeOwnershipOfEnemy(this.targetPlayer.actualClientId);
                            return;
                        }
                    }
                    else
                    {
                        this.noticePlayerTimer += 0.075f;
                        if (this.noticePlayerTimer > 1.5f)
                        {
                            this.lostPlayerInChase = true;
                        }
                    }
                }
            }
            else
            {
                if (this.hasEnteredChaseMode)
                {
                    this.hasEnteredChaseMode = false;
                    this.searchForPlayers.searchWidth = 25f;
                    this.beginningChasingThisClient = false;
                    this.noticePlayerTimer = 0f;
                    this.useSecondaryAudiosOnAnimatedObjects = false;
                    this.openDoorSpeedMultiplier = 0.6f;
                    this.agent.stoppingDistance = 0f;
                    this.agent.speed = 7f;
                }
                if (this.checkLineOfSightInterval <= 0.05f)
                {
                    this.checkLineOfSightInterval += Time.deltaTime;
                    return;
                }
                this.checkLineOfSightInterval = 0f;
                PlayerControllerB playerControllerB3;
                if (this.stunnedByPlayer != null)
                {
                    playerControllerB3 = this.stunnedByPlayer;
                    this.noticePlayerTimer = 1f;
                }
                else
                {
                    playerControllerB3 = base.CheckLineOfSightForPlayer(55f, 60, -1);
                }
                if (!(playerControllerB3 == GameNetworkManager.Instance.localPlayerController))
                {
                    this.noticePlayerTimer -= Time.deltaTime;
                    return;
                }
                this.noticePlayerTimer = Mathf.Clamp(this.noticePlayerTimer + 0.05f, 0f, 10f);
                if (this.noticePlayerTimer > 0.2f && !this.beginningChasingThisClient)
                {
                    this.beginningChasingThisClient = true;
                    this.BeginChasingPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                    base.ChangeOwnershipOfEnemy(playerControllerB3.actualClientId);
                    Debug.Log("Begin chasing on local client");
                    return;
                }
            }
        }

        private void CalculateAgentSpeed()
        {
            if (this.stunNormalizedTimer >= 0f)
            {
                this.agent.speed = 0.1f;
                this.agent.acceleration = 200f;
                //this.creatureAnimator.SetBool("stunned", true);
                return;
            }
            //this.creatureAnimator.SetBool("stunned", false);
            //this.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(base.transform.position - this.previousPosition, 1f).sqrMagnitude / (Time.deltaTime / 2.25f));
            float num = (this.previousPosition - base.transform.position).sqrMagnitude / (Time.deltaTime / 1.4f);
            if (base.IsOwner && this.previousVelocity - num > Mathf.Clamp(num * 0.25f, 2f, 100f))
            {
                this.agent.speed = 0f;
                if (this.currentBehaviourStateIndex == 1)
                {
                    if (this.wallCollisionSFXDebounce > 0.5f)
                    {
                        if (base.IsServer)
                        {
                            this.CollideWithWallServerRpc();
                        }
                        else
                        {
                            this.CollideWithWallClientRpc();
                        }
                    }
                    this.wallCollisionSFXDebounce = 0f;
                }
            }
            this.wallCollisionSFXDebounce += Time.deltaTime;
            this.previousVelocity = num;
            this.previousPosition = base.transform.position;
            if (this.currentBehaviourStateIndex == 0)
            {
                this.agent.speed = 8f;
                this.agent.acceleration = 26f;
                return;
            }
            if (this.currentBehaviourStateIndex == 1)
            {
                this.agentSpeedWithNegative += Time.deltaTime * 1.5f;
                this.agent.speed = Mathf.Clamp(this.agentSpeedWithNegative, -3f, 16f);
                this.agent.acceleration = Mathf.Clamp(45f - num * 12f, 4f, 80f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CollideWithWallServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (IsServer || (!networkManager.IsServer && !networkManager.IsHost))
            {
                return;
            }
            this.CollideWithWallClientRpc();
        }

        [ClientRpc]
        public void CollideWithWallClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (IsClient || (!networkManager.IsClient && !networkManager.IsHost))
            {
                return;
            }
            RoundManager.PlayRandomClip(this.creatureSFX, this.hitWallSFX, true, 1f, 0);
            float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, base.transform.position);
            if (num < 15f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                return;
            }
            if (num < 24f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }
        }

        private void CheckForVeryClosePlayer()
        {
            if (Physics.OverlapSphereNonAlloc(base.transform.position, 1.5f, this.nearPlayerColliders, 8, QueryTriggerInteraction.Ignore) <= 0)
            {
                return;
            }
            PlayerControllerB component = this.nearPlayerColliders[0].transform.GetComponent<PlayerControllerB>();
            if (component != null && component != this.targetPlayer && !Physics.Linecast(base.transform.position + Vector3.up * 0.3f, component.transform.position, StartOfRound.Instance.collidersAndRoomMask))
            {
                this.targetPlayer = component;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BeginChasingPlayerServerRpc(int playerObjectId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (this.IsServer || (!networkManager.IsServer && !networkManager.IsHost))
            {
                return;
            }
            this.BeginChasingPlayerClientRpc(playerObjectId);
        }

        [ClientRpc]
        public void BeginChasingPlayerClientRpc(int playerObjectId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (this.IsClient || (!networkManager.IsClient && !networkManager.IsHost))
            {
                return;
            }
            this.MakeScreech();
            base.SwitchToBehaviourStateOnLocalClient(1);
            base.SetMovingTowardsTargetPlayer(StartOfRound.Instance.allPlayerScripts[playerObjectId]);
        }

        [ServerRpc(RequireOwnership = false)]
        public void MakeScreechNoiseServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (IsServer || (!networkManager.IsServer && !networkManager.IsHost))
            {
                return;
            }
            this.MakeScreechNoiseClientRpc();
        }

        [ClientRpc]
        public void MakeScreechNoiseClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if (networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (IsClient || (!networkManager.IsClient && !networkManager.IsHost))
            {
                return;
            }
            this.MakeScreech();
        }

        private void MakeScreech()
        {
            int num = Random.Range(0, this.longRoarSFX.Length);
            this.creatureVoice.PlayOneShot(this.longRoarSFX[num]);
            WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.longRoarSFX[num], 1f);
            if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, base.transform.position) < 15f)
            {
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.75f, true);
            }
        }

        public override void OnCollideWithPlayer(Collider other)
        {
            base.OnCollideWithPlayer(other);

        }



        public AISearchRoutine searchForPlayers;

        private float checkLineOfSightInterval;

        public float maxSearchAndRoamRadius = 100f;

        [Space(5f)]
        public float noticePlayerTimer;

        private bool hasEnteredChaseMode;

        private bool lostPlayerInChase;

        private bool beginningChasingThisClient;

        private Collider[] nearPlayerColliders;

        public AudioClip shortRoar;

        public AudioClip[] hitWallSFX;

        public AudioClip bitePlayerSFX;

        private Vector3 previousPosition;

        private float previousVelocity;

        private float wallCollisionSFXDebounce;

        public AudioClip[] hitCrawlerSFX;

        public AudioClip[] longRoarSFX;

        public DeadBodyInfo currentlyHeldBody;

        private bool pullingSecondLimb;

        private float agentSpeedWithNegative;

        private Vector3 lastPositionOfSeenPlayer;
    }
}
