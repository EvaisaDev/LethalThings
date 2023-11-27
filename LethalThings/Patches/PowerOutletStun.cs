using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalThings.Patches
{
    public class PowerOutletStun
    {
        public static void Load()
        {
            // Funny power socket stun
            if (Config.enableItemChargerElectrocution.Value)
            {
                On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
                On.ItemCharger.Update += ItemCharger_Update;
                On.GameNetworkManager.Start += GameNetworkManager_Start;
            }
        }


        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);

            foreach (var prefab in self.GetComponent<NetworkManager>().NetworkConfig.Prefabs.m_Prefabs)
            {
                if (prefab.Prefab.GetComponent<GrabbableObject>() != null)
                {
                    if (prefab.Prefab.GetComponent<GrabbableObject>().itemProperties.isConductiveMetal)
                    {
                        var comp = prefab.Prefab.AddComponent<LethalThings.PowerOutletStun>();
                    }
                }
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
            GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer == null)
            {
                return;
            }
            if (!currentlyHeldObjectServer.itemProperties.requiresBattery)
            {
                if (currentlyHeldObjectServer.itemProperties.isConductiveMetal)
                {
                    currentlyHeldObjectServer.GetComponent<LethalThings.PowerOutletStun>().Electrocute(self);
                }
                return;
            }
            orig(self);
        }
    }
}
