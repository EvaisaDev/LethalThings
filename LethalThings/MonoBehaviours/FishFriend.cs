using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LethalThings.MonoBehaviours
{
    public class FishFriend : EnemyAI
    {
        private Transform localPlayerCamera;

        public Transform turnCompass;

        private Vector3 agentLocalVelocity;
        private float agentLocalSpeed;

        public Collider thisEnemyCollider;

        private Vector3 previousPosition;

        public List<Transform> ignoredNodes = new List<Transform>();

        public float playerEvadeDistance = 10f;

        public float playerEvadeSpeed = 5f;

        private float evadeTime = 0f;
        public float evadeTimeMax = 10f;

        public ParticleSystem loveParticles;
        public ParticleSystem hateParticles;

        private PlayerControllerB favouritePlayer;
        private int tamedLevel = 0;

        public Animator animator;

        public InteractTrigger petTrigger;


        public override void Start()
        {
            base.Start();
            movingTowardsTargetPlayer = true;
            localPlayerCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
        }

        public override void DoAIInterval()
        {
            if (StartOfRound.Instance.livingPlayers == 0)
            {
                base.DoAIInterval();
                return;
            }
            if (TargetClosestPlayer())
            {
                // sneaking up on player
                if (currentBehaviourStateIndex == 0)
                {
                    ChooseClosestNodeToPlayer();
                    evadeTime = 0f;
                }
                // evading player
                else if (currentBehaviourStateIndex == 1)
                {
                    AvoidClosestPlayer();
                    evadeTime += Time.deltaTime;
                }
            }
            base.DoAIInterval();
        }

        public void wasPet(PlayerControllerB player)
        {
            increaseLove(player, 1);
        }

        public void increaseLove(PlayerControllerB player, int value)
        {

            increaseLoveServerRpc(player.playerClientId, value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void increaseLoveServerRpc(ulong playerID, int value)
        {
            increaseLoveClientRpc(playerID, value);
        }

        [ClientRpc]
        public void increaseLoveClientRpc(ulong playerID, int value)
        {
            tamedLevel += value;

            var player = StartOfRound.Instance.allPlayerScripts[playerID];

            favouritePlayer = player;

            loveParticles.Play();

            animator.SetTrigger("nuzzle");
        }


        public void decreaseLove(PlayerControllerB player, int value)
        {

            decreaseLoveServerRpc(player.playerClientId, value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void decreaseLoveServerRpc(ulong playerID, int value)
        {
            decreaseLoveClientRpc(playerID, value);
        }

        [ClientRpc]
        public void decreaseLoveClientRpc(ulong playerID, int value)
        {
            tamedLevel -= value;

            var player = StartOfRound.Instance.allPlayerScripts[playerID];

            if(favouritePlayer == player)
            {
                favouritePlayer = null;
            }

            hateParticles.Play();
        }


        public void AvoidClosestPlayer()
        {
            Transform transform = ChooseFarthestNodeFromPosition(targetPlayer.transform.position, avoidLineOfSight: true, 0, log: true);
            if (transform != null && mostOptimalDistance > 5f && Physics.Linecast(transform.transform.position, targetPlayer.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                targetNode = transform;
                SetDestinationToPosition(targetNode.position);
                return;
            }
            movingTowardsTargetPlayer = false;
            agent.speed = 0f;
        }


        public void ChooseClosestNodeToPlayer()
        {
            if (targetNode == null)
            {
                targetNode = allAINodes[0].transform;
            }
            Transform transform = ChooseClosestNodeToPosition(targetPlayer.transform.position, avoidLineOfSight: true);
            if (transform != null)
            {
                targetNode = transform;
            }
            float num = Vector3.Distance(targetPlayer.transform.position, base.transform.position);
            if (num - mostOptimalDistance < 0.1f && (!PathIsIntersectedByLineOfSight(targetPlayer.transform.position, calculatePathDistance: true) || num < 3f))
            {
                if (pathDistance > 10f && !ignoredNodes.Contains(targetNode) && ignoredNodes.Count < 4)
                {
                    ignoredNodes.Add(targetNode);
                }
                movingTowardsTargetPlayer = true;
            }
            else
            {
                SetDestinationToPosition(targetNode.position);
            }
        }

        public override void Update()
        {
            base.Update();
            // calculate velocity based on position
            agentLocalVelocity = (transform.position - previousPosition) / Time.deltaTime;

            // calculate speed based on velocity
            agentLocalSpeed = agentLocalVelocity.magnitude;

            // check target player velocity, if they are moving too fast and we are close enough, switch to evade mode
            if(targetPlayer != null)
            {
                Plugin.logger.LogInfo("target player velocity: " + targetPlayer.thisController.velocity.magnitude);
                Plugin.logger.LogInfo("target player distance: " + Vector3.Distance(transform.position, targetPlayer.transform.position));
                if(targetPlayer.thisController.velocity.magnitude > playerEvadeSpeed && Vector3.Distance(transform.position, targetPlayer.transform.position) < playerEvadeDistance)
                {
                    SwitchToBehaviourState(1);
                }
            }

            if(currentBehaviourStateIndex == 1)
            {
                if (evadeTime > evadeTimeMax && Vector3.Distance(transform.position, targetPlayer.transform.position) >= playerEvadeDistance)
                {
                    SwitchToBehaviourState(0);
                }
            }

            // set previous position
            previousPosition = transform.position;
        }



        public override void KillEnemy(bool destroy = false)
        {
            animator.SetTrigger("killed");
            petTrigger.enabled = false;
            base.KillEnemy(destroy);
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit);
            if (isEnemyDead)
            {
                return;
            }
            enemyHP -= force;

            // run away
            SwitchToBehaviourState(1);
            evadeTime = 0f;

            decreaseLove(playerWhoHit, 1);

            // play hurt animation
            animator.SetTrigger("hurt");
        }
    }
}
