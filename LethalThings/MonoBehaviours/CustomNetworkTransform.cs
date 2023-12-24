using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class CustomNetworkTransform : NetworkBehaviour
    {
        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool syncScale = true;
        // required magnitude difference to send an update
        public float positionDiffLimit = 0.10f;

        // required angle difference to send an update
        public float rotationDiffLimit = 0.10f;

        // required scale difference to send an update
        public float scaleDiffLimit = 0.10f;

        // interpolation settings
        public bool lerpPosition = true;
        public bool lerpRotation = true;
        public bool lerpScale = true;
        public float positionLerpSpeed = 10f;
        public float rotationLerpSpeed = 10f;
        public float scaleLerpSpeed = 10f;

        // last sent position
        private Vector3 _lastPosition;
        private Vector3 _lastRotation;
        private Vector3 _lastScale;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetScale;

        public void FixedUpdate()
        {
            if (IsServer)
            {
                if (syncPosition)
                {
                    if (Vector3.Distance(transform.position, _lastPosition) > positionDiffLimit)
                    {
                        _lastPosition = transform.position;
                        UpdatePositionClientRpc(transform.position);
                    }
                }

                if (syncRotation)
                {
                    if (Quaternion.Angle(Quaternion.Euler(transform.eulerAngles), Quaternion.Euler(_lastRotation)) > rotationDiffLimit)
                    {
                        _lastRotation = transform.eulerAngles;
                        UpdateRotationClientRpc(transform.eulerAngles);
                    }
                }

                if (syncScale)
                {
                    if (Vector3.Distance(transform.localScale, _lastScale) > scaleDiffLimit)
                    {
                        _lastScale = transform.localScale;
                        UpdateScaleClientRpc(transform.localScale);
                    }
                }
            }
            else
            {
                if (lerpPosition)
                {
                    transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.fixedDeltaTime * positionLerpSpeed);
                }

                if (lerpRotation)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);
                }

                if (lerpScale)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.fixedDeltaTime * scaleLerpSpeed);
                }
            }
        }


        [ClientRpc]
        public void UpdatePositionClientRpc(Vector3 position)
        {
            if (!IsServer && syncPosition)
            {
                if(lerpPosition) { 
                    _targetPosition = position;
                }
                else
                {
                    transform.position = position;

                }
            }
        }


        [ClientRpc]
        public void UpdateRotationClientRpc(Vector3 rotation)
        {
            if (!IsServer && syncRotation)
            {
                if (lerpRotation)
                {
                    _targetRotation = Quaternion.Euler(rotation);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(rotation);
                }
            }
        }


        [ClientRpc]
        public void UpdateScaleClientRpc(Vector3 scale)
        {
            if (!IsServer && syncScale)
            {
                if (lerpScale)
                {
                    _targetScale = scale;
                }
                else
                {
                    transform.localScale = scale;
                }
            }
        }
    }
}