using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class RobotAI : EnemyAI
    {
        public string dogName;
        private bool setRandomDogName;
        private int dogNameIndex = -1;
        public GameObject radarDot;
        public bool radarEnabled;
        public string[] dogNames;

        public float maxFuel = 100f;
        public float drainSpeed = 0.02f;
        public float currentFuel = 100;

        private void OnDisable()
        {
            if (radarEnabled)
            {
                RemoveDogFromRadar();
            }
        }
        public override void DoAIInterval()
        {

        }

        public void SetRadarDogNameLocal(string newName)
        {
            dogName = newName;
            base.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = dogName;
            StartOfRound.Instance.mapScreen.ChangeNameOfTargetTransform(base.transform, newName);
        }

        private void RemoveDogFromRadar()
        {
            StartOfRound.Instance.mapScreen.RemoveTargetFromRadar(base.transform);
        }

        private void AddDogToRadar()
        {
            if (!setRandomDogName)
            {
                setRandomDogName = true;
                int num = (dogNameIndex = ((dogNameIndex != -1) ? dogNameIndex : new System.Random(Mathf.Min(StartOfRound.Instance.randomMapSeed + (int)base.NetworkObjectId, 99999999)).Next(0, dogNames.Length)));
                dogName = dogNames[num];
                base.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = dogName;
            }
            string text = StartOfRound.Instance.mapScreen.AddTransformAsTargetToRadar(base.transform, dogName, isNonPlayer: true);
            if (!string.IsNullOrEmpty(text))
            {
                base.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = text;
            }
            StartOfRound.Instance.mapScreen.SyncOrderOfRadarBoostersInList();
        }

        public void EnableRadarBooster(bool enable)
        {
            radarDot.SetActive(enable);
            if (enable)
            {
                AddDogToRadar();
            }
            else
            {
                RemoveDogFromRadar();
            }
            radarEnabled = enable;
        }

    }
}
