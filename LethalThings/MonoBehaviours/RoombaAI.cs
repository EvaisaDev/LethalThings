using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;

namespace LethalThings
{
    public class RoombaAI : EnemyAI
    {
        private float angeredTimer = 0f;
        [Header("Behaviors")]
        public AISearchRoutine searchForPlayers;

        public override void Start()
        {
            base.Start();
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead)
            {
                return;
            }

            if (TargetClosestPlayer(4f, false, 70f))
            {
                StopSearch(searchForPlayers, true);
                movingTowardsTargetPlayer = true;
                return;
            }
            movingTowardsTargetPlayer = false;
            StartSearch(transform.position, searchForPlayers);
        }

        private void FixedUpdate()
        {
            if (!ventAnimationFinished)
            {
                return;
            }
        }

        public override void Update()
        {
            base.Update();
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


        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit, false);
            angeredTimer = 18f;
        }



    }
}
