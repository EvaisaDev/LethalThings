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
            if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
            {
                return;
            }

            if (base.TargetClosestPlayer(4f, false, 70f))
            {
                base.StopSearch(this.searchForPlayers, true);
                this.movingTowardsTargetPlayer = true;
                return;
            }
            this.movingTowardsTargetPlayer = false;
            base.StartSearch(base.transform.position, this.searchForPlayers);
        }

        private void FixedUpdate()
        {
            if (!this.ventAnimationFinished)
            {
                return;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!this.ventAnimationFinished || !(this.creatureAnimator != null))
            {
                return;
            }
            this.creatureAnimator.enabled = false;
            if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
            {
                return;
            }

            Vector3 serverPosition = this.serverPosition;

            if (this.stunNormalizedTimer > 0f)
            {
                this.agent.speed = 0f;
                this.angeredTimer = 7f;
                return;
            }
            else if (this.angeredTimer > 0f)
            {
                this.angeredTimer -= Time.deltaTime;
                if (base.IsOwner)
                {
                    this.agent.stoppingDistance = 0.1f;
                    this.agent.speed = 1f;
                    return;
                }
                return;
            }
            else
            {
                if (base.IsOwner)
                {
                    this.agent.stoppingDistance = 5f;
                    this.agent.speed = 0.8f;
                }
            }
        }


        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit, false);
            this.angeredTimer = 18f;
        }



    }
}
