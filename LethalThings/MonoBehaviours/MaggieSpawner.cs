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
                }
            }
        }

        private static void PlayerControllerB_TeleportPlayer(On.GameNetcodeStuff.PlayerControllerB.orig_TeleportPlayer orig, GameNetcodeStuff.PlayerControllerB self, UnityEngine.Vector3 pos, bool withRotation, float rot, bool allowInteractTrigger, bool enableController)
        {
            orig(self, pos, withRotation, rot, allowInteractTrigger, enableController);

            MaggieSpawner maggieSpawner = self.GetComponentInChildren<MaggieSpawner>();
            if (maggieSpawner != null && maggieSpawner.isSpawning)
            {
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
                if (self.clingingToPlayer == playerTeleported && self.IsOwner)
                {
                    self.KillEnemyOnOwnerClient(true);
                    // get MaggieSpawner on teleporting player
                    MaggieSpawner maggieSpawner = playerTeleported.GetComponentInChildren<MaggieSpawner>();
                    if (maggieSpawner != null)
                    {
                        maggieSpawner.isSpawning = true;
                    }

                    return;
                }
            }

            orig(self, playerTeleported);
        }

        public EnemyType enemyType;
        public bool isSpawning = false;

        [ServerRpc]
        public void SpawnMaggieServerRpc(Vector3 pos, int playerKilled)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
            bool flag = pos.y < -80f;
            NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(pos, 0, -1, enemyType);
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
