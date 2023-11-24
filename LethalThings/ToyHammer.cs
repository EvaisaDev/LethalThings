using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings
{
    internal class ToyHammer : GrabbableObject
    {

        public int hammerHitForce = 1;
        public float hammerHitPercentage = 1;

        public bool reelingUp;

        public bool isHoldingButton;

        private RaycastHit rayHit;

        private Coroutine reelingUpCoroutine;

        private RaycastHit[] objectsHitByHammer;

        private List<RaycastHit> objectsHitByHammerList = new List<RaycastHit>();

        public AudioClip reelUp;

        public AudioClip swing;

        public AudioClip[] hitSFX;

        public AudioSource hammerAudio;

        private PlayerControllerB previousPlayerHeldBy;

        private int hammerMask = 11012424;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (playerHeldBy == null)
            {
                return;
            }
            //Debug.Log($"Is player pressing down button?: {buttonDown}");
            isHoldingButton = buttonDown;
            //Debug.Log("PLAYER ACTIVATED ITEM TO HIT WITH SHOVEL. Who sent this log: " + GameNetworkManager.Instance.localPlayerController.gameObject.name);
            if (!reelingUp && buttonDown)
            {
                reelingUp = true;
                previousPlayerHeldBy = playerHeldBy;
                //Debug.Log($"Set previousPlayerHeldBy: {previousPlayerHeldBy}");
                if (reelingUpCoroutine != null)
                {
                    StopCoroutine(reelingUpCoroutine);
                }
                reelingUpCoroutine = StartCoroutine(reelUpHammer());
            }
        }

        private IEnumerator reelUpHammer()
        {
            playerHeldBy.activatingItem = true;
            playerHeldBy.twoHanded = true;
            playerHeldBy.playerBodyAnimator.ResetTrigger("hammerHit");
            playerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: true);
            hammerAudio.PlayOneShot(reelUp);
            ReelUpSFXServerRpc();
            yield return new WaitForSeconds(0.35f);
            yield return new WaitUntil(() => !isHoldingButton || !isHeld);
            SwingHammer(!isHeld);
            yield return new WaitForSeconds(0.13f);
            HitHammer(!isHeld);
            yield return new WaitForSeconds(0.3f);
            reelingUp = false;
            reelingUpCoroutine = null;
        }

        [ServerRpc]
        public void ReelUpSFXServerRpc()
        {
            ReelUpSFXClientRpc();
        }

        [ClientRpc]
        public void ReelUpSFXClientRpc()
        {
            hammerAudio.PlayOneShot(reelUp);
        }

        public override void DiscardItem()
        {
            playerHeldBy.activatingItem = false;
            base.DiscardItem();
        }

        public void SwingHammer(bool cancel = false)
        {
            previousPlayerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: false);
            if (!cancel)
            {
                hammerAudio.PlayOneShot(swing);
                previousPlayerHeldBy.UpdateSpecialAnimationValue(specialAnimation: true, (short)previousPlayerHeldBy.transform.localEulerAngles.y, 0.4f);
            }
        }

        public void HitHammer(bool cancel = false)
        {
            if (previousPlayerHeldBy == null)
            {
                return;
            }
            previousPlayerHeldBy.activatingItem = false;
            bool flag = false;
            int hitSurfaceID = -1;
            if (!cancel)
            {
                previousPlayerHeldBy.twoHanded = false;
                Debug.DrawRay(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, previousPlayerHeldBy.gameplayCamera.transform.forward * 1.85f, Color.blue, 5f);
                objectsHitByHammer = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.75f, previousPlayerHeldBy.gameplayCamera.transform.forward, 1.85f, hammerMask, QueryTriggerInteraction.Collide);
                objectsHitByHammerList = objectsHitByHammer.OrderBy((RaycastHit x) => x.distance).ToList();
                Vector3 start = previousPlayerHeldBy.gameplayCamera.transform.position;
                for (int i = 0; i < objectsHitByHammerList.Count; i++)
                {
                    IHittable component;
                    RaycastHit hitInfo;
                    if (objectsHitByHammerList[i].transform.gameObject.layer == 8 || objectsHitByHammerList[i].transform.gameObject.layer == 11)
                    {
                        start = objectsHitByHammerList[i].point + objectsHitByHammerList[i].normal * 0.01f;
                        flag = true;
                        string text = objectsHitByHammerList[i].collider.gameObject.tag;
                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (StartOfRound.Instance.footstepSurfaces[j].surfaceTag == text)
                            {
                                hammerAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[j].hitSurfaceSFX);
                                WalkieTalkie.TransmitOneShotAudio(hammerAudio, StartOfRound.Instance.footstepSurfaces[j].hitSurfaceSFX);
                                hitSurfaceID = j;
                                break;
                            }
                        }
                    }
                    else if (objectsHitByHammerList[i].transform.TryGetComponent<IHittable>(out component) && !(objectsHitByHammerList[i].transform == previousPlayerHeldBy.transform) && (objectsHitByHammerList[i].point == Vector3.zero || !Physics.Linecast(start, objectsHitByHammerList[i].point, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault)))
                    {
                        flag = true;
                        Vector3 forward = previousPlayerHeldBy.gameplayCamera.transform.forward;

                        // 1% chance to hit player
                        if (Random.Range(0f, 100f) < hammerHitPercentage)
                        {
                            component.Hit(hammerHitForce, forward, previousPlayerHeldBy, playHitSFX: true);
                        }
                    }
                }
            }
            if (flag)
            {
                var soundID = RoundManager.PlayRandomClip(hammerAudio, hitSFX);
                Object.FindObjectOfType<RoundManager>().PlayAudibleNoise(base.transform.position, 17f, 0.8f);
                playerHeldBy.playerBodyAnimator.SetTrigger("hammerHit");
                HitHammerServerRpc(soundID);
            }
        }

        [ServerRpc]
        public void HitHammerServerRpc(int soundID)
        {
            HitHammerClientRpc(soundID);
        }

        [ClientRpc]
        public void HitHammerClientRpc(int soundID)
        {
            HitSurfaceWithHammer(soundID);
        }

        private void HitSurfaceWithHammer(int soundID)
        {
            if (!IsOwner) { 
                hammerAudio.PlayOneShot(hitSFX[soundID]);
            }
            WalkieTalkie.TransmitOneShotAudio(hammerAudio, hitSFX[soundID]);
        }

    }

}
