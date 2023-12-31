using System;
using System.Collections.Generic;
using System.Text;

namespace LethalThings.MonoBehaviours
{
    public abstract class SaveableObject : GrabbableObject
    {
        public int uniqueId = 0;

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);

            Plugin.logger.LogInfo($"Loading save data for {name} with id {saveData}");

            uniqueId = saveData;
        }

        public override int GetItemDataToSave()
        {
            Plugin.logger.LogInfo($"Saving save data for {name} with id {uniqueId}");
            return uniqueId;
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost)
            {
                uniqueId = UnityEngine.Random.Range(0, 100000000);

                var SaveableNetworkBehaviours = transform.GetComponentsInChildren<SaveableNetworkBehaviour>();

                foreach (var item in SaveableNetworkBehaviours)
                {
                    item.uniqueId = uniqueId;
                }
            }
        }


        public abstract void SaveObjectData();

        public abstract void LoadObjectData();
    }
}
