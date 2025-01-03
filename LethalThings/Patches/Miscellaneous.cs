﻿using BepInEx.Logging;
using LethalThings.MonoBehaviours;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.Patches
{
    public class Miscellaneous
    {
        public static void Load()
        {
            // Allow item to have no grab animation because the game is dumb
            On.GameNetcodeStuff.PlayerControllerB.SetSpecialGrabAnimationBool += PlayerControllerB_SetSpecialGrabAnimationBool;

            On.StartOfRound.Start += StartOfRound_Start;

            /*
            On.GameNetworkManager.Start += GameNetworkManager_Start;

            Hook hook3 = new Hook(typeof(UnityEngine.Object).GetMethod("Instantiate", new Type[]
                      {
                typeof(UnityEngine.Object),
                typeof(Transform),
                typeof(bool)
                      }), typeof(Miscellaneous).GetMethod("InstantiateOPI"));
            Hook hook4 = new Hook(typeof(UnityEngine.Object).GetMethod("Instantiate", new Type[]
            {
                typeof(UnityEngine.Object),
                typeof(Transform)
            }), typeof(Miscellaneous).GetMethod("InstantiateOP"));
            Hook hook5 = new Hook(typeof(UnityEngine.Object).GetMethod("Instantiate", new Type[]
            {
                typeof(UnityEngine.Object)
            }), typeof(Miscellaneous).GetMethod("InstantiateO"));
            Hook hook6 = new Hook(typeof(UnityEngine.Object).GetMethod("Instantiate", new Type[]
            {
                typeof(UnityEngine.Object),
                typeof(Vector3),
                typeof(Quaternion)
            }), typeof(Miscellaneous).GetMethod("InstantiateOPR"));
            Hook hook7 = new Hook(typeof(UnityEngine.Object).GetMethod("Instantiate", new Type[]
            {
                typeof(UnityEngine.Object),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Transform)
            }), typeof(Miscellaneous).GetMethod("InstantiateOPRP"));

            // hook public void NetworkManager.AddNetworkPrefab(GameObject prefab)
            Hook hook = new Hook(typeof(NetworkManager).GetMethod("AddNetworkPrefab", new Type[]
            {
                typeof(GameObject)
            }), typeof(Miscellaneous).GetMethod("AddNetworkPrefab"));*/
        }
        /*
        public static void AddNetworkPrefab(Action<NetworkManager, GameObject> orig, NetworkManager self, GameObject prefab)
        {
            orig(self, prefab);
            foreach (Collider collider in prefab.GetComponentsInChildren<Collider>())
            {
                // if has root marker, skip
                if (collider.gameObject.GetComponent<RootMarker>() != null)
                {
                    continue;
                }
                RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                marker.root = prefab.transform;
            }
        }

        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);
           
            foreach(var obj in NetworkManager.Singleton.NetworkConfig.Prefabs.m_Prefabs)
            {
                var go = obj.Prefab.gameObject;
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            
        }

        public static UnityEngine.Object InstantiateOPI(Func<UnityEngine.Object, Transform, bool, UnityEngine.Object> orig, UnityEngine.Object original, Transform parent, bool instantiateInWorldSpace)
        {
            var obj = orig(original, parent, instantiateInWorldSpace);
            if (obj != null && obj is GameObject)
            {   
                var go = (GameObject)obj;
                // find all colliders, and add root markers to them
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            return obj;
        }

        public static UnityEngine.Object InstantiateOP(Func<UnityEngine.Object, Transform, UnityEngine.Object> orig, UnityEngine.Object original, Transform parent)
        {
            var obj = orig(original, parent);
            if (obj != null && obj is GameObject)
            {
                var go = (GameObject)obj;
                // find all colliders, and add root markers to them
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            return obj;
            
        }

        public static UnityEngine.Object InstantiateO(Func<UnityEngine.Object, UnityEngine.Object> orig, UnityEngine.Object original)
        {
            var obj = orig(original);
            if (obj != null && obj is GameObject)
            {
                var go = (GameObject)obj;
                // find all colliders, and add root markers to them
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            return obj;
            
        }

        public static UnityEngine.Object InstantiateOPR(Func<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object> orig, UnityEngine.Object original, Vector3 position, Quaternion rotation)
        {
            var obj = orig(original, position, rotation);
            if (obj != null && obj is GameObject)
            {
                var go = (GameObject)obj;
                // find all colliders, and add root markers to them
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            return obj;
            
        }

        public static UnityEngine.Object InstantiateOPRP(Func<UnityEngine.Object, Vector3, Quaternion, Transform, UnityEngine.Object> orig, UnityEngine.Object original, Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = orig(original, position, rotation, parent);
            if (obj != null && obj is GameObject)
            {
                var go = (GameObject)obj;
                // find all colliders, and add root markers to them
                foreach (Collider collider in go.GetComponentsInChildren<Collider>())
                {
                    // if has root marker, skip
                    if (collider.gameObject.GetComponent<RootMarker>() != null)
                    {
                        continue;
                    }
                    RootMarker marker = collider.gameObject.AddComponent<RootMarker>();
                    marker.root = go.transform;
                }
            }
            return obj;
            
        }

        */

        private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
        {
            orig(self);

            // remove other dingi, so people stop complaining about non eba dingusses

            if(NetworkConfig.Instance != null && NetworkConfig.Instance.disableOverlappingModContentNetVar.Value)
            {
                foreach (SelectableLevel level in self.levels)
                {
                    level.spawnableScrap.RemoveAll((scrap) => scrap.spawnableItem.name == "dingus");
                }

                self.allItemsList.itemsList.RemoveAll((item) => item.name == "dingus");
            }
        }

        private static void PlayerControllerB_SetSpecialGrabAnimationBool(On.GameNetcodeStuff.PlayerControllerB.orig_SetSpecialGrabAnimationBool orig, GameNetcodeStuff.PlayerControllerB self, bool setTrue, GrabbableObject currentItem)
        {
            if (currentItem == null)
            {
                currentItem = self.currentlyGrabbingObject;
            }
            if (currentItem != null && currentItem.itemProperties.grabAnim == "none")
            {
                //no animation!!!
                return;
            }
            orig(self, setTrue, currentItem);
        }
    }
}
