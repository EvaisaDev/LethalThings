using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using DigitalRuby.ThunderAndLightning;
using DunGen;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LethalThings
{
    public class Missile : NetworkBehaviour
    {
        public int damage = 50;
        public float maxDistance = 10f;
        public float minDistance = 0f;
        public float gravity = 2.4f;
        public float flightDelay = 0.5f;
        public float flightForce = 150f;
        public float flightTime = 2f;
        public float autoDestroyTime = 3f;
        private float timeAlive = 0f;
        public float LobForce = 100f;
        public ParticleSystem particleSystem;
        void OnCollisionEnter(Collision collision)
        {
            if (IsHost)
            {
                Boom();
                BoomClientRpc();
            }
            else
            {
                BoomServerRpc();
            }
        }

        void Start()
        {
            GetComponent<Rigidbody>().useGravity = false;
            if (IsHost)
            {
                GetComponent<Rigidbody>().AddForce(transform.forward * LobForce, ForceMode.Impulse);
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
            Utilities.CreateExplosion(transform.position, true, damage, minDistance, maxDistance, 10, CauseOfDeath.Blast, player);
        }

        public void Boom()
        {
            if (particleSystem == null)
            {
                Debug.LogError("No particle system set on missile, destruction time!!");
                CreateExplosion();

                if (IsHost)
                {
                    Destroy(gameObject);
                }

                return;
            }
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            particleSystem.transform.SetParent(null);
            particleSystem.transform.localScale = Vector3.one;

            // kill system once all particles are dead
            Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constant);

            CreateExplosion();

            if (IsHost)
            {
                Destroy(gameObject);
            }
        }

        void FixedUpdate()
        {
            if (IsHost)
            {
                GetComponent<Rigidbody>().useGravity = false;
                // apply downwards force, gravity.
                GetComponent<Rigidbody>().AddForce(Vector3.down * gravity);

                // apply forward force, flightForce, calculate using acceleration and stuff

                if (timeAlive <= flightTime && timeAlive >= flightDelay)
                {
                    GetComponent<Rigidbody>().AddForce(transform.forward * flightForce);
                }


                timeAlive += Time.fixedDeltaTime;



                // if time is passt autoDestroyTime, destroy
                if (timeAlive > autoDestroyTime)
                {
                    if (IsHost)
                    {
                        Boom();
                        BoomClientRpc();
                    }
                    else
                    {
                        BoomServerRpc();
                    }
                }
                else
                {
                    Debug.Log("Time alive: " + timeAlive + " / " + autoDestroyTime);
                }
            }

        }

    }

}