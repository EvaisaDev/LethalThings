using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LethalThings.MonoBehaviours
{
    public class DecalRandomizer : SaveableNetworkBehaviour
    {
        public List<DecalProjector> decalProjectors = new List<DecalProjector>();
        [HideInInspector]
        private NetworkList<int> decalIndexes = new NetworkList<int>(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public List<Material> decalMaterials = new List<Material>();

        /*
        public override object SaveObjectData()
        {
            // convert networklist to list
            List<int> decalIndexesList = new List<int>();

            for (int i = 0; i < decalIndexes.Count; i++)
            {
                decalIndexesList.Add(decalIndexes[i]);
            }

            return decalIndexesList;
        }

        public override void LoadObjectData(object data)
        {
            if (IsHost)
            {
                decalIndexes.Clear();
                var decalIndexesList = (List<int>)data;

                for (int i = 0; i < decalIndexesList.Count; i++)
                {
                    decalIndexes.Add(decalIndexesList[i]);
                }
            }
           
        }*/

        public override void SaveObjectData()
        {
            List<int> decalIndexesList = new List<int>();

            for (int i = 0; i < decalIndexes.Count; i++)
            {
                decalIndexesList.Add(decalIndexes[i]);
            }

            SaveData.SaveObjectData<List<int>>("decalData", decalIndexesList, uniqueId);
        }

        public void ToggleDecals(bool enabled)
        {
            // loop decalrenderers and toggle
            for (int i = 0; i < decalProjectors.Count; i++)
            {
                decalProjectors[i].enabled = enabled;
            }
        }

        public override void LoadObjectData()
        {
            if (IsHost)
            {
                decalIndexes.Clear();
                var decalIndexesList = SaveData.LoadObjectData<List<int>>("decalData", uniqueId);

                if(decalIndexesList == null)
                {
                    decalIndexesList = new List<int>();
                }

                for (int i = 0; i < decalIndexesList.Count; i++)
                {
                    decalIndexes.Add(decalIndexesList[i]);
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost)
            {
                for (int i = 0; i < decalProjectors.Count; i++)
                {
                    decalIndexes.Add(UnityEngine.Random.Range(0, decalMaterials.Count));
                }
            }
        }

        public void Start()
        {
            if (IsServer)
            {
                for (int i = 0; i < decalProjectors.Count; i++)
                {
                    decalIndexes[i] = UnityEngine.Random.Range(0, decalMaterials.Count);
                }
            }

            for (int i = 0; i < decalProjectors.Count; i++)
            {
                decalProjectors[i].material = decalMaterials[decalIndexes[i]];
            }
        }

        public static void Init()
        {
            On.GrabbableObject.EnableItemMeshes += GrabbableObject_EnableItemMeshes;
        }

        private static void GrabbableObject_EnableItemMeshes(On.GrabbableObject.orig_EnableItemMeshes orig, GrabbableObject self, bool enable)
        {
            orig(self, enable);

            if(self.GetComponentsInChildren<DecalRandomizer>() == null)
            {
                return;
            }

            var decalRandomizers = self.GetComponentsInChildren<DecalRandomizer>();

            
            for (int i = 0; i < decalRandomizers.Length; i++)
            {
                decalRandomizers[i].ToggleDecals(enable);
            }
        }
    }
}
