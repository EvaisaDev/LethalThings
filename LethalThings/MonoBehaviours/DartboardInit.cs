using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class DartboardInit : NetworkBehaviour
    {
        public Transform[] dartSpawns;

        public GameObject dartPrefab;

        public List<GameObject> dartInstances = new List<GameObject>();

        public int dartsToSpawn = 3;

        private float dartInitTimer = 0.6f;
        private float dartInitTimerCurrent = 0f;

        public bool initialized = false;
        public bool networkSpawned = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkSpawned = true;
        }




        public void Update()
        {
            if(!IsHost)
            {
                return;
            }

            if (!initialized && networkSpawned)
            {
                //check if time has passed
                if (dartInitTimerCurrent >= dartInitTimer)
                {
                    initialized = true;
         
                    Plugin.logger.LogInfo("Attempting to spawn darts!!");

                    // check if there are darts in the scene already
                    var darts = FindObjectsOfType<Dart>();

                    // add darts to list
                    foreach (Dart dart in darts)
                    {
                        dartInstances.Add(dart.gameObject);
                    }

                    // pick random dart spawns, no repeats
                    List<Transform> dartSpawnsList = new List<Transform>(dartSpawns);
                    List<Transform> dartSpawnPositions = new List<Transform>();
                    for (int i = darts.Length; i < dartsToSpawn; i++)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, dartSpawnsList.Count);
                        dartSpawnPositions.Add(dartSpawnsList[randomIndex]);
                        dartSpawnsList.RemoveAt(randomIndex);
                    }

                    foreach (Transform dartSpawn in dartSpawnPositions)
                    {
                        GameObject dart = Instantiate(dartPrefab, dartSpawn.position, dartSpawn.rotation, transform);

                        //Plugin.logger.LogInfo("Spawned dart!!");

                        var dartScript = dart.GetComponent<Dart>();

                        // offset backwards so the dart tip is at the spawn point
                        dart.transform.position -= dart.transform.forward * Vector3.Distance(dartScript.dartTip.position, dartSpawn.position);

                        dartInstances.Add(dart);

                        dart.GetComponent<NetworkObject>().Spawn();
                    }
                    
                }

                dartInitTimerCurrent += Time.deltaTime;
                return;
            }
            else if(!networkSpawned)
            {
                return;
            }

            

            // if any darts are lost, respawn them
            // do every 2 seconds
            if (Time.frameCount % 120 != 0)
            {
                return;
            }
            List<GameObject> newDartInstances = new List<GameObject>();

            var dartsChanged = false;

            foreach (GameObject dart in dartInstances)
            {
                if (dart == null)
                {
                    // pick random dart spawn

                    dartsChanged = true;

                    int randomIndex = UnityEngine.Random.Range(0, dartSpawns.Length);

                    Transform dartSpawn = dartSpawns[randomIndex];

                    
                    GameObject newDart = Instantiate(dartPrefab, dartSpawn.position, dartSpawn.rotation, transform);

                    var dartScript = newDart.GetComponent<Dart>();

                    // offset backwards so the dart tip is at the spawn point
                    newDart.transform.position -= newDart.transform.forward * Vector3.Distance(dartScript.dartTip.position, dartSpawn.position);


                    newDartInstances.Add(newDart);
                    newDart.GetComponent<NetworkObject>().Spawn();
                }
                else
                {
                    newDartInstances.Add(dart);
                }
            }

            if(dartsChanged)
            {
                dartInstances = newDartInstances;

            }
        }
    }
}
