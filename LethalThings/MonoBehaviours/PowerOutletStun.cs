﻿using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using System.Security;
using System.Security.Permissions;
using System.Collections;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalThings
{
    public class PowerOutletStun : NetworkBehaviour
    {
        private Coroutine electrocutionCoroutine;
        public void Electrocute(ItemCharger socket)
        {
            Debug.Log("Attempting electrocution");
            if (electrocutionCoroutine != null)
            {
                StopCoroutine(electrocutionCoroutine);
            }
            electrocutionCoroutine = StartCoroutine(electrocutionDelayed(socket));
        }

        public AudioSource strikeAudio;
        public ParticleSystem strikeParticle;

        [HideInInspector]
        private NetworkVariable<int> damage = new NetworkVariable<int>(20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public void Awake()
        {
            var stormyWeather = FindObjectOfType<StormyWeather>(true);
            GameObject audioSource = stormyWeather.targetedStrikeAudio.gameObject;
            // copy gameobject and add to this object as a child
            strikeAudio = Instantiate(audioSource, transform).GetComponent<AudioSource>();
            strikeAudio.transform.localPosition = Vector3.zero;
            strikeAudio.gameObject.SetActive(true);
            strikeParticle = Instantiate(stormyWeather.explosionEffectParticle.gameObject, transform).GetComponent<ParticleSystem>();
            strikeParticle.transform.localPosition = Vector3.zero;
            strikeParticle.gameObject.SetActive(true);


        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost)
            {
                damage.Value = (NetworkConfig.itemChargerElectrocutionDamage.Value);
            }
        }

        private IEnumerator electrocutionDelayed(ItemCharger socket)
        {
            Debug.Log("Electrocution started");
            socket.zapAudio.Play();
            yield return new WaitForSeconds(0.75f);
            socket.chargeStationAnimator.SetTrigger("zap");


            Debug.Log("Electrocution finished");
            if (NetworkObject.IsOwner && !NetworkObject.IsOwnedByServer)
            {
                Debug.Log("Sending stun to server!!");
                ElectrocutedServerRpc(transform.position);
            }
            else if (NetworkObject.IsOwner && NetworkObject.IsOwnedByServer)
            {
                Debug.Log("Sending stun to clients!!");
                ElectrocutedClientRpc(transform.position);
                Electrocuted(transform.position);
            }

            yield break;
        }

        [ClientRpc]
        void ElectrocutedClientRpc(Vector3 position)
        {
            Debug.Log("Stun received!!");
            Electrocuted(position);
        }

        void Electrocuted(Vector3 position)
        {
            var stormyWeather = FindObjectOfType<StormyWeather>(true);

            Utilities.CreateExplosion(position, false, damage.Value, 0f, 5f, 3, CauseOfDeath.Electrocution);

            strikeParticle.Play();
            stormyWeather.PlayThunderEffects(position, strikeAudio);
        }

        [ServerRpc]
        void ElectrocutedServerRpc(Vector3 position)
        {
            ElectrocutedClientRpc(position);
        }
    }
}
