using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using GameNetcodeStuff;

namespace LethalThings.MonoBehaviours
{
    public class MaggieSpawner : NetworkBehaviour
    {
        public static GameObject maggieSpawner;

        public static void Init()
        {
            maggieSpawner = Content.MainAssets.LoadAsset<GameObject>("Assets/Custom/LethalThings/Enemies/Maggie/MaggieSpawner.prefab");

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(maggieSpawner);

            On.CentipedeAI.OnPlayerTeleport += CentipedeAI_OnPlayerTeleport;
            On.GameNetcodeStuff.PlayerControllerB.TeleportPlayer += PlayerControllerB_TeleportPlayer;
            On.GameNetcodeStuff.PlayerControllerB.Start += PlayerControllerB_Start;
        }

        private static void PlayerControllerB_Start(On.GameNetcodeStuff.PlayerControllerB.orig_Start orig, PlayerControllerB self)
        {
            orig(self);

            if (NetworkManager.Singleton.IsHost)
            {
                MaggieSpawner maggieSpawner = self.GetComponentInChildren<MaggieSpawner>();
                if(maggieSpawner == null)
                {
                    var mS = Instantiate(MaggieSpawner.maggieSpawner, self.transform);
                    mS.GetComponent<NetworkObject>().Spawn();

                    mS.GetComponent<NetworkObject>().TrySetParent(self.GetComponent<NetworkObject>());

                    mS.GetComponent<MaggieSpawner>().MaggieSpawnerMadeClientRpc();

                    mS.GetComponent<NetworkObject>().ChangeOwnership(self.OwnerClientId);

                    Plugin.logger.LogInfo("MaggieSpawner spawned on " + self.gameObject.name);
                }
            }
        }

        [ClientRpc]
        public void MaggieSpawnerMadeClientRpc()
        {
            Plugin.logger.LogInfo("MaggieSpawner spawned, parent: " + transform.parent);

        }

        private static void PlayerControllerB_TeleportPlayer(On.GameNetcodeStuff.PlayerControllerB.orig_TeleportPlayer orig, GameNetcodeStuff.PlayerControllerB self, UnityEngine.Vector3 pos, bool withRotation, float rot, bool allowInteractTrigger, bool enableController)
        {
            orig(self, pos, withRotation, rot, allowInteractTrigger, enableController);
            Plugin.logger.LogInfo("Teleported wawa");
            MaggieSpawner maggieSpawner = self.GetComponentInChildren<MaggieSpawner>();
            if (maggieSpawner != null && maggieSpawner.isSpawning)
            {
                Plugin.logger.LogInfo("Maggie spawning!!");
                maggieSpawner.isSpawning = false;
                maggieSpawner.SpawnMaggieServerRpc(pos, (int)self.playerClientId);
                // kill player
                self.KillPlayer(Vector3.zero, false, CauseOfDeath.Unknown);
            }
        }

        private static void CentipedeAI_OnPlayerTeleport(On.CentipedeAI.orig_OnPlayerTeleport orig, CentipedeAI self, GameNetcodeStuff.PlayerControllerB playerTeleported)
        {
            // if chance is met, spawn maggie
            if (NetworkConfig.Instance != null && UnityEngine.Random.Range(0f, 100f) <= NetworkConfig.Instance.maggieTeleporterChanceNetVar.Value)
            {
                if (self.clingingToPlayer == playerTeleported && playerTeleported.IsOwner)
                {
                    Plugin.logger.LogInfo("attempt spawn stuff maggie wah");
                    self.KillEnemyServerRpc(true);
                    // get MaggieSpawner on teleporting player
                    MaggieSpawner maggieSpawner = playerTeleported.GetComponentInChildren<MaggieSpawner>();
                    if (maggieSpawner != null)
                    {
                        maggieSpawner.isSpawning = true;
                        Plugin.logger.LogInfo("Allowing maggie spawn!!");
                    }

                    return;
                }
            }

            orig(self, playerTeleported);
        }

        public EnemyType enemyType;
        public bool isSpawning = false;

        [ServerRpc (RequireOwnership = false)]
        public void SpawnMaggieServerRpc(Vector3 pos, int playerKilled)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
            bool flag = pos.y < -80f;
            NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(RoundManager.Instance.GetNavMeshPosition(pos, default(NavMeshHit), 10f), 0, -1, enemyType);
            if (netObjectRef.TryGet(out var networkObject))
            {
                Maggie component = networkObject.GetComponent<Maggie>();
                component.SetSuit(player.currentSuitID);
                component.mimickingPlayer = player;
                component.SetEnemyOutside(!flag);
                player.redirectToEnemy = component;
                if (player.deadBody != null)
                {
                    player.deadBody.DeactivateBody(setActive: false);
                }
            }
            SpawnMaggieClientRpc(netObjectRef, flag, playerKilled);
        }

        [ClientRpc]
        public void SpawnMaggieClientRpc(NetworkObjectReference netObjectRef, bool inFactory, int playerKilled)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
            if (netObjectRef.TryGet(out var networkObject))
            {
                Maggie component = networkObject.GetComponent<Maggie>();
                component.SetSuit(player.currentSuitID);
                component.mimickingPlayer = player;
                component.SetEnemyOutside(!inFactory);
                player.redirectToEnemy = component;
            }
        }

    }
}
