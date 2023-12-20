using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    [ExecuteAlways]
    public class DecorPlacementDebug : MonoBehaviour
    {
        private int placementMaskAndBlockers = 134220033;

        public PlaceableShipObject placeableShipObject;


        void OnDrawGizmos()
        {
            if (placeableShipObject == null)
            {
                return;
            }

            var currentCollider = placeableShipObject.placeObjectCollider as BoxCollider;
            var ghostObject = placeableShipObject.transform;

            //bool flag = Physics.CheckBox(ghostObject.position, currentCollider.size * 0.5f * 0.57f, Quaternion.Euler(ghostObject.eulerAngles), placementMaskAndBlockers, QueryTriggerInteraction.Ignore);

            Gizmos.color = Color.red;
            //Gizmos.DrawWireCube(placeableShipObject.transform.position, placeableShipObject.transform.localScale);

            // draw gizmo that matches check box
            Gizmos.DrawWireCube(transform.position, currentCollider.size * 0.5f * 0.57f);

            var layerString = "";
            // print out all the layer numbers from placementMaskAndBlockers
            for (int i = 0; i < 32; i++)
            {
                if ((placementMaskAndBlockers & (1 << i)) != 0)
                {
                    layerString += LayerMask.LayerToName(i) + ", ";
                }
            }

            Debug.Log(layerString);

        }

    }
}
