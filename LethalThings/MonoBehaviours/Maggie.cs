using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Animations.Rigging;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;
using LethalLib.Modules;

namespace LethalThings.MonoBehaviours
{
    public class Maggie : EnemyAI
    {
        public SkinnedMeshRenderer renderer;

        private Ray enemyRay;

        private RaycastHit enemyRayHit;

        private int currentFootstepSurfaceIndex;

        private int previousFootstepClip;

        public AudioSource movementAudio;

        private bool sprinting;

        private int previousBehaviourState = -1;

        public float walkCheckInterval;

        private Vector3 positionLastCheck;

        private Coroutine teleportCoroutine;

        public ParticleSystem teleportParticle;

        public AISearchRoutine searchForPlayers;

        private Vector3 agentLocalVelocity;

        private Vector3 previousPosition;

        private float velX;

        private float velZ;

        public Transform animationContainer;

        private Vector3 currentRandomLookDirection;

        private Vector3 focusOnPosition;

        private float verticalLookAngle;

        private float currentLookAngle;

        public Transform headTiltTarget;

        private float lookAtPositionTimer;

        private float randomLookTimer;

        private bool lostPlayerInChase;

        private float lostLOSTimer;

        private bool running;

        private bool crouching;

        [Space(3f)]
        public PlayerControllerB mimickingPlayer;

        public bool allowSpawningWithoutPlayer;

        [Space(3f)]
        public Transform lerpTarget;

        public float turnSpeedMultiplier;

        public MultiRotationConstraint lookRig1;

        public MultiRotationConstraint lookRig2;

        private float stopAndStareTimer;

        public Transform stareAtTransform;

        private bool inKillAnimation;

        public bool startingKillAnimationLocalClient;

        private Coroutine killAnimationCoroutine;

        private Ray playerRay;

        private PlayerControllerB lastPlayerKilled;

        private float timeLookingAtLastNoise;

        private Vector3 shipHidingSpot;

        private float staminaTimer;

        private bool runningRandomly;

        private bool enemyEnabled;

        private float timeAtLastUsingEntrance;

        private float interestInShipCooldown;

        public AudioClip[] footSquelches;

        public ParticleSystem[] killParticles;

        public override void Start()
        {
            try
            {
                agent = base.gameObject.GetComponentInChildren<NavMeshAgent>();
                skinnedMeshRenderers = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                meshRenderers = base.gameObject.GetComponentsInChildren<MeshRenderer>();
                if (creatureAnimator == null)
                {
                    creatureAnimator = base.gameObject.GetComponentInChildren<Animator>();
                }
                thisNetworkObject = base.gameObject.GetComponentInChildren<NetworkObject>();
                serverPosition = base.transform.position;
                thisEnemyIndex = RoundManager.Instance.numberOfEnemiesInScene;
                RoundManager.Instance.numberOfEnemiesInScene++;
                isOutside = base.transform.position.y > -80f;
                if (isOutside)
                {
                    if (allAINodes == null || allAINodes.Length == 0)
                    {
                        allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
                    }
                    if (GameNetworkManager.Instance.localPlayerController != null)
                    {
                        EnableEnemyMesh(!StartOfRound.Instance.hangarDoorsClosed || !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
                    }
                }
                else if (allAINodes == null || allAINodes.Length == 0)
                {
                    allAINodes = GameObject.FindGameObjectsWithTag("AINode");
                }
                path1 = new NavMeshPath();
                openDoorSpeedMultiplier = enemyType.doorSpeedMultiplier;
                if (base.IsOwner)
                {
                    SyncPositionToClients();
                }
                else
                {
                    SetClientCalculatingAI(enable: false);
                }
            }
            catch (Exception arg)
            {
                Debug.LogError($"Error when initializing enemy variables for {base.gameObject.name} : {arg}");
            }
            lerpTarget.SetParent(RoundManager.Instance.mapPropsContainer.transform);
            enemyRayHit = default(RaycastHit);
            addPlayerVelocityToDestination = 3f;
            if (base.IsServer && mimickingPlayer == null)
            {
                SetEnemyAsHavingNoPlayerServerRpc();
            }
        }

        [ServerRpc]
        public void SetEnemyAsHavingNoPlayerServerRpc()
        {
            SetEnemyAsHavingNoPlayerClientRpc();
        }


        [ClientRpc]
        public void SetEnemyAsHavingNoPlayerClientRpc()
        {
            allowSpawningWithoutPlayer = true;
        }

        private void Awake()
        {
            SetVisibilityOfMaggie();
        }

        private void LookAndRunRandomly(bool canStartRunning = false, bool onlySetRunning = false)
        {
            randomLookTimer -= AIIntervalTime;
            if (!runningRandomly && !running)
            {
                staminaTimer = Mathf.Min(6f, staminaTimer + AIIntervalTime);
            }
            else
            {
                staminaTimer = Mathf.Max(0f, staminaTimer - AIIntervalTime);
            }
            if (!(randomLookTimer <= 0f))
            {
                return;
            }
            randomLookTimer = UnityEngine.Random.Range(0.7f, 5f);
            if (!runningRandomly)
            {
                int num = ((!isOutside) ? 20 : 35);
                if (onlySetRunning)
                {
                    num /= 3;
                }
                if (staminaTimer >= 5f && UnityEngine.Random.Range(0, 100) < num)
                {
                    running = true;
                    runningRandomly = true;
                    creatureAnimator.SetBool("Running", value: true);
                    SetRunningServerRpc(running: true);
                }
                /*else if (!onlySetRunning)
                {
                    Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                    float y = 0f;
                    if (Physics.Raycast(eye.position, onUnitSphere, 5f, StartOfRound.Instance.collidersRoomMaskDefaultAndPlayers))
                    {
                        y = RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(eye.position, 12f, 5);
                    }
                    onUnitSphere.y = y;
                    LookAtDirectionServerRpc(onUnitSphere, UnityEngine.Random.Range(0.25f, 2f), UnityEngine.Random.Range(-60f, 60f));
                }*/
            }
            else
            {
                int num2 = ((!isOutside) ? 30 : 80);
                if (onlySetRunning)
                {
                    num2 /= 5;
                }
                if (UnityEngine.Random.Range(0, 100) > num2 || staminaTimer <= 0f)
                {
                    running = false;
                    runningRandomly = false;
                    staminaTimer = -6f;
                    creatureAnimator.SetBool("Running", value: false);
                    SetRunningServerRpc(running: false);
                }
            }
        }


        private void TeleportMaggieAndSync(Vector3 pos, bool setOutside)
        {
            if (base.IsOwner)
            {
                TeleportMaggie(pos, setOutside);
                TeleportMaggieServerRpc(pos, setOutside);
            }
        }

        [ServerRpc]
        public void TeleportMaggieServerRpc(Vector3 pos, bool setOutside)
        {
            TeleportMaggieClientRpc(pos, setOutside);
        }

        [ClientRpc]
        public void TeleportMaggieClientRpc(Vector3 pos, bool setOutside)
        {
            TeleportMaggie(pos, setOutside);
        }

        private void TeleportMaggie(Vector3 pos, bool setOutside)
        {
            timeAtLastUsingEntrance = Time.realtimeSinceStartup;
            Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(pos);
            if (base.IsOwner)
            {
                agent.enabled = false;
                base.transform.position = navMeshPosition;
                agent.enabled = true;
            }
            else
            {
                base.transform.position = navMeshPosition;
            }
            serverPosition = navMeshPosition;
            SetEnemyOutside(setOutside);
            EntranceTeleport entranceTeleport = RoundManager.FindMainEntranceScript(setOutside);
            if (entranceTeleport.doorAudios != null && entranceTeleport.doorAudios.Length != 0)
            {
                entranceTeleport.entrancePointAudio.PlayOneShot(entranceTeleport.doorAudios[0]);
                WalkieTalkie.TransmitOneShotAudio(entranceTeleport.entrancePointAudio, entranceTeleport.doorAudios[0]);
            }
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();
            if (isEnemyDead)
            {
                agent.speed = 0f;
                return;
            }
            PlayerControllerB playerControllerB = null;
            switch (currentBehaviourStateIndex)
            {
                case 0:
                    LookAndRunRandomly(canStartRunning: true);
                    if (!searchForPlayers.inProgress)
                    {
                        StartSearch(base.transform.position, searchForPlayers);
                    }
                    playerControllerB = CheckLineOfSightForClosestPlayer();
                    if (playerControllerB != null)
                    {
                        LookAtPlayerServerRpc((int)playerControllerB.playerClientId);
                        SetMovingTowardsTargetPlayer(playerControllerB);
                        SwitchToBehaviourState(1);
                        break;
                    }
                    interestInShipCooldown += AIIntervalTime;
                    if (interestInShipCooldown >= 17f && Vector3.Distance(base.transform.position, StartOfRound.Instance.elevatorTransform.position) < 22f)
                    {
                        SwitchToBehaviourState(2);
                    }
                    break;
                case 1:
                    LookAndRunRandomly(canStartRunning: true, onlySetRunning: true);
                    playerControllerB = CheckLineOfSightForClosestPlayer(70f, 50, 1, 3f);
                    if (playerControllerB != null)
                    {
                        lostPlayerInChase = false;
                        lostLOSTimer = 0f;
                        if (playerControllerB != targetPlayer)
                        {
                            SetMovingTowardsTargetPlayer(playerControllerB);
                            LookAtPlayerServerRpc((int)playerControllerB.playerClientId);
                        }
                        if (mostOptimalDistance > 17f)
                        {
                            if (!running)
                            {
                                running = true;
                                creatureAnimator.SetBool("Running", value: true);
                                Debug.Log(string.Format("Setting running to true 8; {0}", creatureAnimator.GetBool("Running")));
                                SetRunningServerRpc(running: true);
                            }
                        }
                        else if (mostOptimalDistance < 6f)
                        {
  
                        }
                        else if (mostOptimalDistance < 12f)
                        {
                            if (running && !runningRandomly)
                            {
                                running = false;
                                creatureAnimator.SetBool("Running", value: false);
                                Debug.Log(string.Format("Setting running to false 1; {0}", creatureAnimator.GetBool("Running")));
                                SetRunningServerRpc(running: false);
                            }
                        }
                        break;
                    }
                    lostLOSTimer += AIIntervalTime;
                    if (lostLOSTimer > 10f)
                    {
                        SwitchToBehaviourState(0);
                        targetPlayer = null;
                    }
                    else if (lostLOSTimer > 3.5f)
                    {
                        lostPlayerInChase = true;
                        StopLookingAtTransformServerRpc();
                        targetPlayer = null;
                        if (running)
                        {
                            running = false;
                            creatureAnimator.SetBool("Running", value: false);
                            Debug.Log(string.Format("Setting running to false 2; {0}", creatureAnimator.GetBool("Running")));
                            SetRunningServerRpc(running: false);
                        }
                    }
                    break;
                case 2:
                    {
                        if (!isInsidePlayerShip)
                        {
                            interestInShipCooldown -= AIIntervalTime;
                        }
                        if (Vector3.Distance(base.transform.position, StartOfRound.Instance.insideShipPositions[0].position) > 27f || interestInShipCooldown <= 0f)
                        {
                            SwitchToBehaviourState(0);
                            break;
                        }
                        PlayerControllerB closestPlayer = GetClosestPlayer();
                        if (closestPlayer != null)
                        {
                            PlayerControllerB playerControllerB2 = CheckLineOfSightForClosestPlayer(70f, 20, 0);
                            if (playerControllerB2 != null)
                            {
                                if (stareAtTransform != playerControllerB2.gameplayCamera.transform)
                                {
                                    LookAtPlayerServerRpc((int)playerControllerB2.playerClientId);
                                }
                                SetMovingTowardsTargetPlayer(playerControllerB2);
                                SwitchToBehaviourState(1);
                            }
                            else if (isInsidePlayerShip && closestPlayer.HasLineOfSightToPosition(base.transform.position + Vector3.up * 0.7f, 4f, 20))
                            {
                                if (stareAtTransform != closestPlayer.gameplayCamera.transform)
                                {
                                    LookAtPlayerServerRpc((int)closestPlayer.playerClientId);
                                }
                                SetMovingTowardsTargetPlayer(closestPlayer);
                                SwitchToBehaviourState(1);
                            }
                            else if (mostOptimalDistance < 6f)
                            {
                                if (stareAtTransform != closestPlayer.gameplayCamera.transform)
                                {
                                    stareAtTransform = closestPlayer.gameplayCamera.transform;
                                    LookAtPlayerServerRpc((int)closestPlayer.playerClientId);
                                }
                            }
                            else if (mostOptimalDistance > 12f && stareAtTransform != null)
                            {
                                stareAtTransform = null;
                                StopLookingAtTransformServerRpc();
                            }
                        }
                        SetDestinationToPosition(shipHidingSpot);
                        if (!crouching && Vector3.Distance(base.transform.position, shipHidingSpot) < 0.4f)
                        {
                            agent.speed = 0f;
                            crouching = true;
                            SetCrouchingServerRpc(true);
                        }
                        else if (crouching && Vector3.Distance(base.transform.position, shipHidingSpot) > 1f)
                        {
                            crouching = false;
                            SetCrouchingServerRpc(false);
                        }
                        break;
                    }
            }
            if (!(targetPlayer != null) || !PlayerIsTargetable(targetPlayer) || (currentBehaviourStateIndex != 1 && currentBehaviourStateIndex != 2))
            {
                return;
            }
            if (lostPlayerInChase)
            {
                movingTowardsTargetPlayer = false;
                if (!searchForPlayers.inProgress)
                {
                    StartSearch(base.transform.position, searchForPlayers);
                }
            }
            else
            {
                if (searchForPlayers.inProgress)
                {
                    StopSearch(searchForPlayers);
                }
                SetMovingTowardsTargetPlayer(targetPlayer);
            }
        }


        [ServerRpc]
        public void LookAtDirectionServerRpc(Vector3 dir, float time, float vertLookAngle)
        {
            LookAtDirectionClientRpc(dir, time, vertLookAngle);
        }

        [ClientRpc]
        public void LookAtDirectionClientRpc(Vector3 dir, float time, float vertLookAngle)
        {
            LookAtDirection(dir, time, vertLookAngle);
        }

        [ServerRpc]
        public void LookAtPositionServerRpc(Vector3 pos, float time)
        {
            LookAtPositionClientRpc(pos, time);
        }

        [ClientRpc]
        public void LookAtPositionClientRpc(Vector3 pos, float time)
        {
            LookAtPosition(pos, time);
        }

        [ServerRpc]
        public void LookAtPlayerServerRpc(int playerId)
        {
            LookAtPlayerClientRpc(playerId);
        }

        [ClientRpc]
        public void LookAtPlayerClientRpc(int playerId)
        {
            stareAtTransform = StartOfRound.Instance.allPlayerScripts[playerId].gameplayCamera.transform;
        }

        [ServerRpc]
        public void StopLookingAtTransformServerRpc()
        {
            StopLookingAtTransformClientRpc();
        }

        [ClientRpc]
        public void StopLookingAtTransformClientRpc()
        {
            stareAtTransform = null;
        }

        [ServerRpc]
        public void SetCrouchingServerRpc(bool setCrouch)
        {
            SetCrouchingClientRpc(setCrouch);
        }

        [ClientRpc]
        public void SetCrouchingClientRpc(bool setCrouch)
        {
            crouching = setCrouch;
            creatureAnimator.SetBool("Crouching", setCrouch);
        }


        public void LookAtFocusedPosition()
        {
            if (inSpecialAnimation)
            {
                verticalLookAngle = Mathf.Lerp(verticalLookAngle, 0f, 10f * Time.deltaTime);
                currentLookAngle = Mathf.Lerp(currentLookAngle, verticalLookAngle, 7f);
                headTiltTarget.localEulerAngles = new Vector3(currentLookAngle, 0f, 0f);
                return;
            }
            if (lookAtPositionTimer <= 0f)
            {
                if (stareAtTransform != null)
                {
                    if (!(Vector3.Distance(stareAtTransform.position, base.transform.position) > 80f))
                    {
                        agent.angularSpeed = 0f;
                        RoundManager.Instance.tempTransform.position = base.transform.position;
                        RoundManager.Instance.tempTransform.LookAt(stareAtTransform);
                        base.transform.rotation = Quaternion.Lerp(base.transform.rotation, RoundManager.Instance.tempTransform.rotation, turnSpeedMultiplier * Time.deltaTime);
                        base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
                        headTiltTarget.LookAt(stareAtTransform);
                        headTiltTarget.localEulerAngles = new Vector3(headTiltTarget.localEulerAngles.x, 0f, 0f);
                    }
                    return;
                }
                agent.angularSpeed = 450f;
                verticalLookAngle = Mathf.Clamp(verticalLookAngle, -30f, 10f);
            }
            else
            {
                Debug.Log($"Looking at focused position {focusOnPosition}");
                agent.angularSpeed = 0f;
                lookAtPositionTimer -= Time.deltaTime;
                RoundManager.Instance.tempTransform.position = base.transform.position;
                RoundManager.Instance.tempTransform.LookAt(focusOnPosition);
                base.transform.rotation = Quaternion.Lerp(base.transform.rotation, RoundManager.Instance.tempTransform.rotation, turnSpeedMultiplier * Time.deltaTime);
                base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
                verticalLookAngle = Mathf.Clamp(verticalLookAngle + UnityEngine.Random.Range(-3f * Time.deltaTime, 3f * Time.deltaTime), -70f, 70f);
            }
            currentLookAngle = Mathf.Lerp(currentLookAngle, verticalLookAngle, 7f);
            headTiltTarget.localEulerAngles = new Vector3(currentLookAngle, 0f, 0f);
        }

        public void LookAtDirection(Vector3 direction, float lookAtTime = 1f, float vertLookAngle = 0f)
        {
            verticalLookAngle = vertLookAngle;
            direction = Vector3.Normalize(direction * 100f);
            focusOnPosition = base.transform.position + direction * 1000f;
            lookAtPositionTimer = lookAtTime;
        }

        public void LookAtPosition(Vector3 pos, float lookAtTime = 1f)
        {
            Debug.Log($"Look at position {pos} called! lookatpositiontimer setting to {lookAtTime}");
            focusOnPosition = pos;
            lookAtPositionTimer = lookAtTime;
            float num = Vector3.Angle(base.transform.forward, pos - base.transform.position);
            if (pos.y - headTiltTarget.position.y < 0f)
            {
                num *= -1f;
            }
            verticalLookAngle = num;
        }

        [ServerRpc]
        public void SetRunningServerRpc(bool running)
        {
            SetRunningClientRpc(running);
        }

        [ClientRpc]
        public void SetRunningClientRpc(bool setRunning)
        {
            running = setRunning;
            creatureAnimator.SetBool("Running", setRunning);
        }


        private void CalculateAnimationDirection(float maxSpeed = 1f)
        {
            creatureAnimator.SetBool("IsMoving", Vector3.Distance(base.transform.position, previousPosition) > 0f);
            agentLocalVelocity = animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(base.transform.position - previousPosition, 1f) / (Time.deltaTime * 2f));
            velX = Mathf.Lerp(velX, agentLocalVelocity.x, 10f * Time.deltaTime);
            creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(velX, 0f - maxSpeed, maxSpeed));
            velZ = Mathf.Lerp(velZ, agentLocalVelocity.z, 10f * Time.deltaTime);
            creatureAnimator.SetFloat("VelocityZ", Mathf.Clamp(velZ, 0f - maxSpeed, maxSpeed));
            previousPosition = base.transform.position;
        }

        public override void DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
            if (!base.IsOwner || isEnemyDead || inSpecialAnimation)
            {
                return;
            }
            if (Vector3.Distance(noisePosition, base.transform.position + Vector3.up * 0.4f) < 0.75f)
            {
                Debug.Log("Can't hear noise reason A");
                return;
            }
            if ((stareAtTransform != null && Vector3.Distance(noisePosition, stareAtTransform.position) < 2f))
            {
                Debug.Log("Can't hear noise reason B");
                return;
            }
            float num = Vector3.Distance(noisePosition, base.transform.position);
            float num2 = noiseLoudness / num;
            Debug.Log($"Noise heard relative loudness: {num2}");
            if (!(num2 < 0.12f) && !(Time.realtimeSinceStartup - timeLookingAtLastNoise < 3f))
            {
                timeLookingAtLastNoise = Time.realtimeSinceStartup;
                LookAtPositionServerRpc(noisePosition, Mathf.Min(num2 * 6f, 2f));
            }
        }

        public void LateUpdate()
        {
            if (!(stunNormalizedTimer >= 0f) && !isEnemyDead)
            {
                LookAtFocusedPosition();
            }
        }

        public void SetVisibilityOfMaggie()
        {
            if (allowSpawningWithoutPlayer)
            {
                if (mimickingPlayer != null && mimickingPlayer.deadBody != null && !mimickingPlayer.deadBody.deactivated)
                {
                    if (enemyEnabled)
                    {
                        enemyEnabled = false;
                        EnableEnemyMesh(enable: false);
                    }
                }
                else if (!enemyEnabled)
                {
                    enemyEnabled = true;
                    EnableEnemyMesh(enable: true);
                }
            }
            else if (mimickingPlayer == null || (mimickingPlayer.deadBody != null && !mimickingPlayer.deadBody.deactivated))
            {
                if (enemyEnabled)
                {
                    enemyEnabled = false;
                    EnableEnemyMesh(enable: false);
                }
            }
            else if (!enemyEnabled)
            {
                enemyEnabled = true;
                EnableEnemyMesh(enable: true);
            }
        }

        public override void Update()
        {
            base.Update();
            CalculateAnimationDirection();
            SetVisibilityOfMaggie();
            if (isEnemyDead)
            {
                agent.speed = 0f;
                if (inSpecialAnimation)
                {
                    FinishKillAnimation();
                }
                return;
            }
            if (lastPlayerKilled != null && lastPlayerKilled.deadBody != null && !lastPlayerKilled.deadBody.deactivated)
            {
                Debug.Log($"Deactivating body of killed player! {lastPlayerKilled.playerClientId}; {isEnemyDead}");
                lastPlayerKilled.deadBody.DeactivateBody(setActive: false);
                lastPlayerKilled = null;
            }
            if (!enemyEnabled)
            {
                return;
            }
            if (ventAnimationFinished)
            {
                lookRig1.weight = 0.452f;
                lookRig2.weight = 1f;
                creatureAnimator.SetBool("Stunned", stunNormalizedTimer >= 0f);
                if (stunNormalizedTimer >= 0f)
                {
                    agent.speed = 0f;
                    if (base.IsOwner && searchForPlayers.inProgress)
                    {
                        StopSearch(searchForPlayers);
                    }
                    if (inSpecialAnimation)
                    {
                        FinishKillAnimation();
                    }
                }
                if (inSpecialAnimation)
                {
                    return;
                }
                if (walkCheckInterval <= 0f)
                {
                    walkCheckInterval = 0.1f;
                    positionLastCheck = base.transform.position;
                }
                else
                {
                    walkCheckInterval -= Time.deltaTime;
                }
                switch (currentBehaviourStateIndex)
                {
                    case 0:
                        if (previousBehaviourState != currentBehaviourStateIndex)
                        {
                            stareAtTransform = null;
                            running = false;
                            runningRandomly = false;
                            creatureAnimator.SetBool("Running", value: false);
                            creatureAnimator.SetBool("HandsOut", value: false);
                            crouching = false;
                            creatureAnimator.SetBool("Crouching", value: false);
                            previousBehaviourState = currentBehaviourStateIndex;
                        }
                        if (running || runningRandomly)
                        {
                            agent.speed = 7f;
                        }
                        else
                        {
                            agent.speed = 3.8f;
                        }
                        break;
                    case 1:
                        if (previousBehaviourState != currentBehaviourStateIndex)
                        {
                            lookAtPositionTimer = 0f;
                            if (previousBehaviourState == 0)
                            {
                                stopAndStareTimer = UnityEngine.Random.Range(2f, 5f);
                            }
                            runningRandomly = false;
                            running = false;
                            creatureAnimator.SetBool("Running", value: false);
                            crouching = false;
                            creatureAnimator.SetBool("Crouching", value: false);
                            previousBehaviourState = currentBehaviourStateIndex;
                        }
                        if (!base.IsOwner)
                        {
                            break;
                        }
                        stopAndStareTimer -= Time.deltaTime;
                        if (stopAndStareTimer >= 0f)
                        {
                            agent.speed = 0f;
                            break;
                        }
                        if (stopAndStareTimer <= -5f)
                        {
                            stopAndStareTimer = UnityEngine.Random.Range(0f, 3f);
                        }
                        if (running || runningRandomly)
                        {
                            agent.speed = 8f;
                        }
                        else
                        {
                            agent.speed = 3.8f;
                        }
                        break;
                    case 2:
                        if (previousBehaviourState != currentBehaviourStateIndex)
                        {
                            movingTowardsTargetPlayer = false;
                            interestInShipCooldown = 17f;
                            agent.speed = 5f;
                            runningRandomly = false;
                            running = false;
                            creatureAnimator.SetBool("Running", value: false);
                            creatureAnimator.SetBool("HandsOut", value: false);
                            if (base.IsOwner)
                            {
                                ChooseShipHidingSpot();
                            }
                            previousBehaviourState = currentBehaviourStateIndex;
                        }
                        break;
                }
            }
            else
            {
                lookRig1.weight = 0f;
                lookRig2.weight = 0f;
            }
        }

        private void ChooseShipHidingSpot()
        {
            bool flag = false;
            for (int i = 0; i < StartOfRound.Instance.insideShipPositions.Length; i++)
            {
                if (Physics.Linecast(StartOfRound.Instance.shipDoorAudioSource.transform.position, StartOfRound.Instance.insideShipPositions[i].position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && SetDestinationToPosition(StartOfRound.Instance.insideShipPositions[i].position, checkForPath: true))
                {
                    flag = true;
                    shipHidingSpot = destination;
                    break;
                }
            }
            if (!flag)
            {
                shipHidingSpot = StartOfRound.Instance.insideShipPositions[UnityEngine.Random.Range(0, StartOfRound.Instance.insideShipPositions.Length)].position;
            }
        }

        public override void ShipTeleportEnemy()
        {
            base.ShipTeleportEnemy();
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
            }
            StartCoroutine(teleportMaggie());
        }

        private IEnumerator teleportMaggie()
        {
            teleportParticle.Play();
            movementAudio.PlayOneShot(UnityEngine.Object.FindObjectOfType<ShipTeleporter>().beamUpPlayerBodySFX);
            yield return new WaitForSeconds(3f);
            if (StartOfRound.Instance.shipIsLeaving)
            {
                yield break;
            }
            SetEnemyOutside(outside: true);
            isInsidePlayerShip = true;
            ShipTeleporter[] array = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();
            ShipTeleporter shipTeleporter = null;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].isInverseTeleporter)
                    {
                        shipTeleporter = array[i];
                    }
                }
            }
            if (shipTeleporter != null)
            {
                if (base.IsOwner)
                {
                    agent.enabled = false;
                    base.transform.position = shipTeleporter.teleporterPosition.position;
                    agent.enabled = true;
                    isInsidePlayerShip = true;
                }
                serverPosition = shipTeleporter.teleporterPosition.position;
            }
        }

        public void SetEnemyOutside(bool outside = false)
        {
            isOutside = outside;
            if (outside)
            {
                allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
            }
            else
            {
                allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            }
            if (searchForPlayers.inProgress)
            {
                StopSearch(searchForPlayers);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void KillEnemy(bool destroy = false)
        {
            base.KillEnemy(destroy);
            creatureAnimator.SetBool("Stunned", value: false);
            creatureAnimator.SetBool("Dead", value: true);
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            base.HitEnemy(force, playerWhoHit, playHitSFX);
            enemyHP -= force;
            stunNormalizedTimer = 0.5f;
            creatureAnimator.SetTrigger("HitEnemy");
            stopAndStareTimer = 0f;
            if (((float)UnityEngine.Random.Range(0, 100) < 40f || enemyHP == 1) && !running)
            {
                running = true;
                runningRandomly = true;
                creatureAnimator.SetBool("Running", value: true);
                SetRunningServerRpc(running: true);
                staminaTimer = 5f;
            }
            if (enemyHP <= 0)
            {
                KillEnemyOnOwnerClient();
            }
        }

        public override void OnCollideWithPlayer(Collider other)
        {
            base.OnCollideWithPlayer(other);
            if (!(stunNormalizedTimer >= 0f) && !isEnemyDead && !(Time.realtimeSinceStartup - timeAtLastUsingEntrance < 1.75f))
            {
                PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(other, inKillAnimation || startingKillAnimationLocalClient || !enemyEnabled);
                if (playerControllerB != null)
                {
                    KillPlayerAnimationServerRpc((int)playerControllerB.playerClientId);
                    startingKillAnimationLocalClient = true;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerAnimationServerRpc(int playerObjectId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(3192502457U, serverRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
                __endSendServerRpc(ref bufferWriter, 3192502457U, serverRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                if (!inKillAnimation)
                {
                    inKillAnimation = true;
                    inSpecialAnimation = true;
                    isClientCalculatingAI = false;
                    inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
                    inSpecialAnimationWithPlayer.inAnimationWithEnemy = this;
                    KillPlayerAnimationClientRpc(playerObjectId);
                }
                else
                {
                    CancelKillAnimationClientRpc(playerObjectId);
                }
            }
        }

        [ClientRpc]
        public void CancelKillAnimationClientRpc(int playerObjectId)
        {
            startingKillAnimationLocalClient = false;
        }

        [ClientRpc]
        public void KillPlayerAnimationClientRpc(int playerObjectId)
        {
            if (searchForPlayers.inProgress)
            {
                StopSearch(searchForPlayers);
            }
            inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
            if (inSpecialAnimationWithPlayer == GameNetworkManager.Instance.localPlayerController)
            {
                startingKillAnimationLocalClient = false;
            }
            if (inSpecialAnimationWithPlayer == null || inSpecialAnimationWithPlayer.isPlayerDead || !inSpecialAnimationWithPlayer.isInsideFactory)
            {
                FinishKillAnimation();
            }
            inSpecialAnimationWithPlayer.inAnimationWithEnemy = this;
            inSpecialAnimationWithPlayer.CancelSpecialTriggerAnimations();
            inKillAnimation = true;
            inSpecialAnimation = true;
            creatureAnimator.SetBool("killing", value: true);
            agent.enabled = false;
            inSpecialAnimationWithPlayer.inSpecialInteractAnimation = true;
            inSpecialAnimationWithPlayer.snapToServerPosition = true;
            Vector3 origin = ((!inSpecialAnimationWithPlayer.IsOwner) ? inSpecialAnimationWithPlayer.transform.parent.TransformPoint(inSpecialAnimationWithPlayer.serverPlayerPosition) : inSpecialAnimationWithPlayer.transform.position);
            Vector3 vector = base.transform.position - base.transform.forward * 2f;
            vector.y = origin.y;
            playerRay = new Ray(origin, vector - inSpecialAnimationWithPlayer.transform.position);
            if (killAnimationCoroutine != null)
            {
                StopCoroutine(killAnimationCoroutine);
            }
            killAnimationCoroutine = StartCoroutine(killAnimation());
        }

        private IEnumerator killAnimation()
        {
            WalkieTalkie.TransmitOneShotAudio(creatureSFX, enemyType.audioClips[0]);
            creatureSFX.PlayOneShot(enemyType.audioClips[0]);
            Vector3 endPosition = playerRay.GetPoint(0.7f);
            if (isOutside && endPosition.y < -80f)
            {
                SetEnemyOutside();
            }
            else if (!isOutside && endPosition.y > -80f)
            {
                SetEnemyOutside(outside: true);
            }
            inSpecialAnimationWithPlayer.disableSyncInAnimation = true;
            inSpecialAnimationWithPlayer.disableLookInput = true;
            RoundManager.Instance.tempTransform.position = inSpecialAnimationWithPlayer.transform.position;
            RoundManager.Instance.tempTransform.LookAt(endPosition);
            Quaternion startingPlayerRot = inSpecialAnimationWithPlayer.transform.rotation;
            Quaternion targetRot = RoundManager.Instance.tempTransform.rotation;
            Vector3 startingPosition = base.transform.position;
            foreach(var particle in killParticles)
            {
                particle.Play();
            }
            for (int i = 0; i < 8; i++)
            {
                if (i > 0)
                {
                    base.transform.LookAt(inSpecialAnimationWithPlayer.transform.position);
                    base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
                }
                base.transform.position = Vector3.Lerp(startingPosition, endPosition, (float)i / 8f);
                inSpecialAnimationWithPlayer.transform.rotation = Quaternion.Lerp(startingPlayerRot, targetRot, (float)i / 8f);
                inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0f, inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0f);
                yield return null;
            }
            base.transform.position = endPosition;
            inSpecialAnimationWithPlayer.transform.rotation = targetRot;
            inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0f, inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0f);
            if (inSpecialAnimationWithPlayer == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: true);
            }
            yield return new WaitForSeconds(1.5f);
            lastPlayerKilled = inSpecialAnimationWithPlayer;
            if (inSpecialAnimationWithPlayer != null)
            {
                bool flag = inSpecialAnimationWithPlayer.transform.position.y < -80f;

                //Plugin.logger.LogInfo($"Killing player {inSpecialAnimationWithPlayer.playerClientId} with ragdoll index {Player.GetRagdollIndex("LTGoopRagdoll")}");

                inSpecialAnimationWithPlayer.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Bludgeoning, Player.GetRagdollIndex("LTGoopRagdoll"));
                inSpecialAnimationWithPlayer.snapToServerPosition = false;
                /*if (base.IsServer)
                {
                    NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(GetGroundPosition(playerRay.origin), inSpecialAnimationWithPlayer.transform.eulerAngles.y, -1, enemyType);
                    if (netObjectRef.TryGet(out var networkObject))
                    {
                        Maggie component = networkObject.GetComponent<Maggie>();
                        component.SetSuit(inSpecialAnimationWithPlayer.currentSuitID);
                        component.mimickingPlayer = inSpecialAnimationWithPlayer;
                        component.SetEnemyOutside(!flag);
                        inSpecialAnimationWithPlayer.redirectToEnemy = component;
                        if (inSpecialAnimationWithPlayer.deadBody != null)
                        {
                            inSpecialAnimationWithPlayer.deadBody.DeactivateBody(setActive: false);
                        }
                    }
                    CreateMimicClientRpc(netObjectRef, flag, (int)inSpecialAnimationWithPlayer.playerClientId);
                }*/
                FinishKillAnimation(killedPlayer: true);
            }
            else
            {
                FinishKillAnimation();
            }
        }
        /*
        [ClientRpc]
        public void CreateMimicClientRpc(NetworkObjectReference netObjectRef, bool inFactory, int playerKilled)
        {
            StartCoroutine(waitForMimicEnemySpawn(netObjectRef, inFactory, playerKilled));
        }
        */
        /*
        private IEnumerator waitForMimicEnemySpawn(NetworkObjectReference netObjectRef, bool inFactory, int playerKilled)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
            NetworkObject netObject = null;
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || netObjectRef.TryGet(out netObject));
            if (player.deadBody == null)
            {
                startTime = Time.realtimeSinceStartup;
                yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime > 20f || player.deadBody != null);
            }
            if (!(player.deadBody == null))
            {
                player.deadBody.DeactivateBody(setActive: false);
                if (netObject != null)
                {
                    Maggie component = netObject.GetComponent<Maggie>();
                    component.mimickingPlayer = player;
                    component.SetSuit(player.currentSuitID);
                    component.SetEnemyOutside(!inFactory);
                    player.redirectToEnemy = component;
                }
            }
        }
        */


        public override void CancelSpecialAnimationWithPlayer()
        {
            base.CancelSpecialAnimationWithPlayer();
            FinishKillAnimation();
        }

        public void FinishKillAnimation(bool killedPlayer = false)
        {
            if (!killedPlayer)
            {
                creatureSFX.Stop();
            }
            if (killAnimationCoroutine != null)
            {
                StopCoroutine(killAnimationCoroutine);
            }
            inSpecialAnimation = false;
            inKillAnimation = false;
            creatureAnimator.SetBool("killing", value: false);
            startingKillAnimationLocalClient = false;
            if (inSpecialAnimationWithPlayer != null)
            {
                inSpecialAnimationWithPlayer.disableSyncInAnimation = false;
                inSpecialAnimationWithPlayer.disableLookInput = false;
                inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
                inSpecialAnimationWithPlayer.snapToServerPosition = false;
                inSpecialAnimationWithPlayer.inAnimationWithEnemy = null;
            }
            stopAndStareTimer = 3f;
            movingTowardsTargetPlayer = false;
            if (base.IsOwner)
            {
                base.transform.position = GetGroundPosition(base.transform.position);
                agent.enabled = true;
                isClientCalculatingAI = true;
            }
            if (base.NetworkObject.IsSpawned)
            {
                SwitchToBehaviourStateOnLocalClient(0);
                if (base.IsServer)
                {
                    SwitchToBehaviourState(0);
                }
            }
        }

        private Vector3 GetGroundPosition(Vector3 startingPos)
        {
            Vector3 pos = startingPos;
            pos = RoundManager.Instance.GetNavMeshPosition(pos, default(NavMeshHit), 3f);
            if (!RoundManager.Instance.GotNavMeshPositionResult)
            {
                if (Physics.Raycast(startingPos + Vector3.up * 0.15f, -Vector3.up, out var hitInfo, 50f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    pos = RoundManager.Instance.GetNavMeshPosition(hitInfo.point, default(NavMeshHit), 10f);
                }
                else
                {
                    int num = UnityEngine.Random.Range(0, allAINodes.Length);
                    if (allAINodes != null && allAINodes[num] != null)
                    {
                        pos = allAINodes[num].transform.position;
                    }
                }
            }
            return pos;
        }

        public void SetSuit(int suitId)
        {
            Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[suitId].suitMaterial;
            renderer.materials[1] = suitMaterial;
        }


        public void GetMaterialStandingOn()
        {
            enemyRay = new Ray(base.transform.position + Vector3.up, -Vector3.up);
            if (Physics.Raycast(enemyRay, out enemyRayHit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
            {
                if (enemyRayHit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].surfaceTag))
                {
                    return;
                }
                for (int i = 0; i < StartOfRound.Instance.footstepSurfaces.Length; i++)
                {
                    if (enemyRayHit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[i].surfaceTag))
                    {
                        currentFootstepSurfaceIndex = i;
                        break;
                    }
                }
            }
            else
            {
                Debug.DrawRay(enemyRay.origin, enemyRay.direction, Color.white, 0.3f);
            }
        }

        public void PlayFootstepSound()
        {
            GetMaterialStandingOn();
            int num = UnityEngine.Random.Range(0, StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips.Length);
            if (num == previousFootstepClip)
            {
                num = (num + 1) % StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips.Length;
            }
            movementAudio.pitch = UnityEngine.Random.Range(0.93f, 1.07f);
            float num2 = 0.95f;
            if (!sprinting)
            {
                num2 = 0.75f;
            }
            var randomfootSquelch = UnityEngine.Random.Range(0, footSquelches.Length);

            movementAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips[num], num2);
            movementAudio.PlayOneShot(footSquelches[randomfootSquelch], num2);
            previousFootstepClip = num;
            WalkieTalkie.TransmitOneShotAudio(movementAudio, StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].clips[num], num2);
            WalkieTalkie.TransmitOneShotAudio(movementAudio, footSquelches[randomfootSquelch], num2);
        }

        public override void AnimationEventA()
        {
            base.AnimationEventA();
            PlayFootstepSound();
        }


    }
}
