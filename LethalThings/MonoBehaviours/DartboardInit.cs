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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost)
            {

                //Plugin.logger.LogInfo("Attempting to spawn darts!!");

                // pick random dart spawns, no repeats
                List<Transform> dartSpawnsList = new List<Transform>(dartSpawns);
                List<Transform> dartSpawnPositions = new List<Transform>();
                for (int i = 0; i < dartsToSpawn; i++)
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
        }

        public void Update()
        {
            // if any darts are lost, respawn them
            foreach (GameObject dart in dartInstances)
            {
                if (dart == null)
                {
                    // pick random dart spawn

                    int randomIndex = UnityEngine.Random.Range(0, dartSpawns.Length);

                    Transform dartSpawn = dartSpawns[randomIndex];

                    dartInstances.Remove(dart);
                    GameObject newDart = Instantiate(dartPrefab, dartSpawn.position, dartSpawn.rotation, transform);

                    var dartScript = newDart.GetComponent<Dart>();

                    // offset backwards so the dart tip is at the spawn point
                    newDart.transform.position -= newDart.transform.forward * Vector3.Distance(dartScript.dartTip.position, dartSpawn.position);


                    dartInstances.Add(newDart);
                    newDart.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
    }
}
