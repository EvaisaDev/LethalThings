using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

// define unity random
using Random = UnityEngine.Random;

namespace LethalThings.MonoBehaviours
{
    public class ToyGun : GrabbableObject
    {
        public bool isFiring = false;
        public float reloadTime = 2f;
        public bool wasFired = false;
        public AudioClip[] fireSounds;
        public AudioClip[] reloadSounds;
        public AudioSource audioSource;
        public AudioSource audioSourceFar;
        public Animator animator;

        public void Fire(int fireSound, int reloadSound)
        {
            Plugin.logger.LogInfo("Firing gun.");

            if (wasFired)
            {
                return;
            }
            Plugin.logger.LogInfo("Firing gun 2.");

            wasFired = true;

            // play fire animation
            animator.Play("pewpew");

            isFiring = false;

            StartCoroutine(FireSound(fireSound));
            StartCoroutine(Reload(reloadSound));
        }

        public IEnumerator Reload(int soundIndex)
        {
            yield return new WaitForSeconds(reloadTime);
            wasFired = false;

            // play reload animation
            animator.Play("unpew");

            // play reload sound
            PlayReloadSound(soundIndex);
        }

        public IEnumerator FireSound(int soundIndex)
        {
            yield return new WaitForSeconds(0.03f);
            PlayFireSound(soundIndex);
        }

        public void PlayFireSound(int soundIndex)
        {
            audioSource.PlayOneShot(fireSounds[soundIndex]);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(fireSounds[soundIndex]);
            }
        }

        public void PlayReloadSound(int soundIndex)
        {
            audioSource.PlayOneShot(reloadSounds[soundIndex]);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(reloadSounds[soundIndex]);
            }
        }

        [ServerRpc]
        public void FireServerRpc(int fireSound, int reloadSound)
        {
            FireClientRpc(fireSound, reloadSound);
            Plugin.logger.LogInfo("Firing gun server");
        }

        [ClientRpc]
        public void FireClientRpc(int fireSound, int reloadSound)
        {
            Fire(fireSound, reloadSound);
            Plugin.logger.LogInfo("Firing gun client");
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            Plugin.logger.LogInfo("gun item activated");

            if (base.IsOwner)
            {
                //Debug.Log("Player activated item to fire gun.");
                if (!isFiring && !wasFired)
                {
                    isFiring = true;
                    int fireSound = Random.Range(0, fireSounds.Length);
                    int reloadSound = Random.Range(0, reloadSounds.Length);
                    FireServerRpc(fireSound, reloadSound);
                    Plugin.logger.LogInfo("firing goofy ahh!!");
                }
            }

        }

    }
}
