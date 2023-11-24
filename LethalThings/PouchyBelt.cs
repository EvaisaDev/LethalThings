using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings
{
    public class PouchyBelt : GrabbableObject
    {

        public Transform beltCosmetic;
        public Vector3 beltCosmeticPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 beltCosmeticRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);

        public override void LateUpdate()
        {
            base.LateUpdate();
            if (playerHeldBy != null)
            {
                // enable the belt
                beltCosmetic.gameObject.SetActive(true);
                var root = playerHeldBy.lowerSpine.parent;
                beltCosmetic.position = root.position;
                beltCosmetic.rotation = root.rotation;
                base.transform.Rotate(beltCosmeticRotationOffset);
                beltCosmetic.position = root.position;
                Vector3 vector = beltCosmeticPositionOffset;
                vector = root.rotation * vector;
                beltCosmetic.position += vector;
                mainObjectRenderer.enabled = false;
            }
            else
            {
                // disable the belt
                beltCosmetic.gameObject.SetActive(false);
                mainObjectRenderer.enabled = true;
            }
        }
    }
}
