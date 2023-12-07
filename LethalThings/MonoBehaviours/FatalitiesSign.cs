using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class FatalitiesSign : NetworkBehaviour
    {
        public static int DaysSinceLastFatality = 0;
        public static int lastDeathCount = 0;
        public static int daysSpent = 0;

        public TextMeshProUGUI textMesh;
        public TextMeshProUGUI textMeshBack;

        public NetworkVariable<int> daysSinceLastFatality = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public static void Init()
        {
            On.GameNetworkManager.SaveGameValues += GameNetworkManager_SaveGameValues;
            On.GameNetworkManager.ResetSavedGameValues += GameNetworkManager_ResetSavedGameValues;
            On.StartOfRound.SetTimeAndPlanetToSavedSettings += StartOfRound_SetTimeAndPlanetToSavedSettings;
        }

        private static void StartOfRound_SetTimeAndPlanetToSavedSettings(On.StartOfRound.orig_SetTimeAndPlanetToSavedSettings orig, StartOfRound self)
        {
            orig(self);
            string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;
            DaysSinceLastFatality = ES3.Load("LethalThings_DaysSinceLastFatality", currentSaveFileName, 0);
        }

        private static void GameNetworkManager_ResetSavedGameValues(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
        {
            orig(self);
            DaysSinceLastFatality = 0;
            ES3.Save("LethalThings_DaysSinceLastFatality", DaysSinceLastFatality, self.currentSaveFileName);
        }

        private static void GameNetworkManager_SaveGameValues(On.GameNetworkManager.orig_SaveGameValues orig, GameNetworkManager self)
        {
            orig(self);
            ES3.Save("LethalThings_DaysSinceLastFatality", DaysSinceLastFatality, self.currentSaveFileName);
        }



        public void Awake()
        {
            if (lastDeathCount == 0)
            {
                lastDeathCount = StartOfRound.Instance.gameStats.deaths;
                daysSpent = StartOfRound.Instance.gameStats.daysSpent;
            }
        }

        public void Update()
        {
            if (StartOfRound.Instance.gameStats.deaths > lastDeathCount)
            {
                DaysSinceLastFatality = 0;
                lastDeathCount = StartOfRound.Instance.gameStats.deaths;
            }
            else if (StartOfRound.Instance.gameStats.daysSpent != daysSpent)
            {
                DaysSinceLastFatality++;
                daysSpent = StartOfRound.Instance.gameStats.daysSpent;
            }

            // if host update the network variable
            if (IsServer && daysSinceLastFatality.Value != DaysSinceLastFatality)
            {
                daysSinceLastFatality.Value = DaysSinceLastFatality;
            }

            if (textMesh.text != daysSinceLastFatality.Value.ToString())
            {
                textMesh.text = daysSinceLastFatality.Value.ToString();
                textMeshBack.text = daysSinceLastFatality.Value.ToString();
            }

        }
    }
}
