using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class Cookie : ThrowableNoisemaker
    {
        public AudioClip[] cookieSpecialAudio;
        private float explodePercentage = 0.04f;
        private float oooPennyPercentage = 0.2f;
        public bool wasThrown = false;
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            Plugin.logger.LogInfo($"Attempted to throw cookie!!");

            if (!IsOwner)
            {
                return;
            }

            Plugin.logger.LogInfo($"Cookie thrown with right: {right} - {throwWithRight}");

            if ((throwWithRight && right) || !right)
            {
                wasThrown = true;

                PlayCookieAudioServerRpc(0);

            }
        }

        [ServerRpc]
        public void PlayCookieAudioServerRpc(int index)
        {
            PlayCookieAudioClientRpc(index);
        }

        [ClientRpc]
        public void PlayCookieAudioClientRpc(int index)
        {
            noiseAudio.PlayOneShot(cookieSpecialAudio[index]);
            if (noiseAudioFar != null)
            {
                noiseAudioFar.PlayOneShot(cookieSpecialAudio[index]);
            }

            WalkieTalkie.TransmitOneShotAudio(noiseAudio, cookieSpecialAudio[index], 0.5f);
            RoundManager.Instance.PlayAudibleNoise(base.transform.position, noiseRange, 0.5f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }

        [ServerRpc]
        public void StopPlayingCookieAudioServerRpc()
        {
            StopPlayingCookieAudioClientRpc();
        }

        [ClientRpc]
        public void StopPlayingCookieAudioClientRpc()
        {
            noiseAudio.Stop();
            if (noiseAudioFar != null)
            {
                noiseAudioFar.Stop();
            }
        }


        [ClientRpc]
        public void BoomClientRpc()
        {
            Boom();
        }

        [ServerRpc]
        public void BoomServerRpc()
        {
            Boom();
            BoomClientRpc();
        }


        public void CreateExplosion()
        {
            var player = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(x => x.OwnerClientId == OwnerClientId);
            Utilities.CreateExplosion(transform.position, true, 20, 0, 4, 10, CauseOfDeath.Blast, player);
        }

        public void Boom()
        {
            CreateExplosion();

            if (IsHost)
            {
                Destroy(gameObject);
            }
        }

        public override void OnHitGround()
        {
            if (wasThrown)
            {
                wasThrown = false;

                if (IsOwner)
                {
                    if ((UnityEngine.Random.Range(0f, 1000f) / 1000f) <= explodePercentage)
                    {
                        Boom();
                        BoomServerRpc();
                    }
                    else
                    {
                        
                        // if random chance
                        if ((UnityEngine.Random.Range(0f, 1000f) / 1000f) <= oooPennyPercentage)
                        {
                            StopPlayingCookieAudioServerRpc();
                            PlayCookieAudioServerRpc(1);
                        }
                    }
                }

            }
        }
    }
}
