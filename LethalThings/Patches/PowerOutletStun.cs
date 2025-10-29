using LethalThings.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.Patches
{
    public class PowerOutletStun
    {
        public static void Load()
        {
            // Funny power socket stun

            On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
            On.ItemCharger.Update += ItemCharger_Update;
            On.GameNetworkManager.Start += GameNetworkManager_Start;

        }


        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);

            List<NetworkPrefab> prefabs = self?.GetComponent<NetworkManager>()?.NetworkConfig?.Prefabs?.m_Prefabs;
            if (prefabs == null) return;

            foreach (var prefabContainer in prefabs)
            {
                GameObject prefab = prefabContainer?.Prefab;

                // Using this to search for bugged prefabs:
                /*
                if (prefab?.name == null)
                {
                    Plugin.logger.LogWarning($"Found a potentially bugged prefab! Container [{prefabContainer}] | GameObject: [{prefab}]");
                }
                */

                if (prefab?.GetComponent<GrabbableObject>()?.itemProperties?.isConductiveMetal != true) continue;

                Plugin.logger.LogInfo($"Found conductive scrap [{prefab.name}]");

                var comp = prefab.AddComponent<LethalThings.PowerOutletStun>();
            }
        }


        private static void ItemCharger_Update(On.ItemCharger.orig_Update orig, ItemCharger self)
        {
            orig(self);
            if (self.updateInterval == 0f)
            {
                if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    self.triggerScript.interactable = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null && (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.requiresBattery || GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.isConductiveMetal);
                    return;
                }
            }
        }

        private static void ItemCharger_ChargeItem(On.ItemCharger.orig_ChargeItem orig, ItemCharger self)
        {
            if (NetworkConfig.Instance != null && NetworkConfig.Instance.enableItemChargerElectrocutionNetVar.Value)
            {
                GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
                if (currentlyHeldObjectServer != null && !currentlyHeldObjectServer.itemProperties.requiresBattery)
                {
                    if (currentlyHeldObjectServer.itemProperties.isConductiveMetal)
                    {
                        currentlyHeldObjectServer.GetComponent<LethalThings.PowerOutletStun>().Electrocute(self);
                    }
                }
            }
            orig(self);
        }
    }
}
