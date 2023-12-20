using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Cursor = UnityEngine.Cursor;

namespace LethalThings.MonoBehaviours
{
    public class DevMenu : NetworkBehaviour
    {
        public List<Button> mainButtons;
        public List<GameObject> mainViewports;

        public GameObject Root;

        public TextMeshProUGUI MoneyText;
        public TMP_InputField MoneyInputField;
        public Button MoneyApply;

        private Terminal terminal;

        public Button ItemListButtonTemplate;
        public Button ClearItemsInShipButton;

        public Button EnemyListButtonTemplate;

        public bool itemListGenerated = false;

        public static DevMenu Instance;

        public List<GameObject> itemPrefabList = new List<GameObject>();
        public List<EnemyType> enemyTypes = new List<EnemyType>();

        public void Awake()
        {
            // set up buttons so they toggle the viewports
            for (int i = 0; i < mainButtons.Count; i++)
            {
                int index = i;
                mainButtons[i].onClick.AddListener(() => ToggleViewport(index));
            }

            // set up money input field, so that when we click apply, it sets the money
            MoneyApply.onClick.AddListener(() => SetMoney());

            Instance = this;

            // hide self
            Root.SetActive(false);
        }


        public void Update()
        {
            if (terminal == null)
            {
                terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                MoneyInputField.text = $"{terminal.groupCredits}";
            }

            if(terminal != null)
            {
                MoneyText.text = $"Credits: {terminal.groupCredits}";
            }

            if(StartOfRound.Instance != null && !itemListGenerated)
            {
                itemListGenerated = true;
                GenerateItemList();
                GenerateEnemyList();
                // setup clear items in ship button
                ClearItemsInShipButton.onClick.AddListener(() =>
                {
                    if (IsHost)
                    {
                        ClearItemsInShip();
                    }
                    else
                    {
                        ClearItemsInShipServerRpc();
                    }
                });
            }

            // check if F1 was pressed
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                // toggle root
                Root.SetActive(!Root.activeSelf);

                // toggle cursor
                Cursor.visible = !Cursor.visible;
                Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
                if (Root.activeSelf)
                {
                    StartOfRound.Instance.localPlayerController.playerActions.Disable();
                    IngamePlayerSettings.Instance.playerInput.actions.Disable();
                }
                else
                {
                    StartOfRound.Instance.localPlayerController.playerActions.Enable();
                    IngamePlayerSettings.Instance.playerInput.actions.Enable();
                }
                //StartOfRound.Instance.localPlayerController.inTerminalMenu = Root.activeSelf;
            }

            if (Root.activeSelf)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                StartOfRound.Instance.localPlayerController.playerActions.Disable();
                IngamePlayerSettings.Instance.playerInput.actions.Disable();
                //StartOfRound.Instance.localPlayerController.inTerminalMenu = true;
            }
        }

        [ServerRpc (RequireOwnership = false)]
        public void ClearItemsInShipServerRpc()
        {
            ClearItemsInShip();
        }

        public void ClearItemsInShip()
        {
            if (StartOfRound.Instance != null)
            {
                GrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();

                // loop
                for (int i = 0; i < array.Length; i++)
                {
                    var grabbableObject = array[i];
                    if (grabbableObject != null && grabbableObject.playerHeldBy == null && grabbableObject.isInShipRoom)
                    {
                        grabbableObject.gameObject.GetComponent<NetworkObject>().Despawn();
                    }
                }
            }

            var allBread = FindObjectsOfType<GrabbableObject>().Where(x => x.itemProperties.itemName == "Stale bread");
        }

        public void GenerateItemList()
        {
            if (StartOfRound.Instance != null)
            {
                // generate item list
                var allItems = StartOfRound.Instance.allItemsList.itemsList;

                for (int i = 0; i < allItems.Count; i++)
                {
                    var item = allItems[i];
                    var button = Instantiate(ItemListButtonTemplate, ItemListButtonTemplate.transform.parent);
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
                    button.onClick.AddListener(() => {
                        if (IsHost)
                        {
                            spawnItem(item.spawnPrefab, StartOfRound.Instance.localPlayerController.gameplayCamera.transform.position);
                        }
                        else
                        {
                            spawnItemServerRpc(itemPrefabList.Count, StartOfRound.Instance.localPlayerController.gameplayCamera.transform.position);
                        }
                    });

                    itemPrefabList.Add(item.spawnPrefab);

                    var rectTransform = button.GetComponent<RectTransform>();
                    var contentRectTransform = ItemListButtonTemplate.transform.parent.GetComponent<RectTransform>();

                    float cumulativeHeight = ((rectTransform.rect.height / 2) + 5) + i * (rectTransform.rect.height + 5);

                    // set position, and update content size to contain the new button
                    contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, cumulativeHeight + ((rectTransform.rect.height / 2) + 5));

                    // calculate the cumulative height of buttons above the current one


                    // set position so that we start from the top of the scroll container
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -cumulativeHeight);
                }
            }
        }

        public void GenerateEnemyList()
        {
            if (StartOfRound.Instance != null)
            {
                // generate item list

                List<EnemyType> allEnemies = new List<EnemyType>();

                foreach (var level in StartOfRound.Instance.levels)
                {
                    foreach(var enemy in level.Enemies)
                    {
                        if (!allEnemies.Contains(enemy.enemyType))
                        {
                            allEnemies.Add(enemy.enemyType);
                        }
                    }

                    foreach (var enemy in level.DaytimeEnemies)
                    {
                        if (!allEnemies.Contains(enemy.enemyType))
                        {
                            allEnemies.Add(enemy.enemyType);
                        }
                    }

                    foreach (var enemy in level.OutsideEnemies)
                    {
                        if (!allEnemies.Contains(enemy.enemyType))
                        {
                            allEnemies.Add(enemy.enemyType);
                        }
                    }
                }

                // print enemy count
                Plugin.logger.LogInfo($"Found {allEnemies.Count} enemies.");

                for (int i = 0; i < allEnemies.Count; i++)
                {
                    var enemy = allEnemies[i];
                    var button = Instantiate(EnemyListButtonTemplate, EnemyListButtonTemplate.transform.parent);
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = enemy.enemyName;
                    button.onClick.AddListener(() => {
                        if (IsHost)
                        {
                            spawnEnemy(enemy, StartOfRound.Instance.localPlayerController.gameplayCamera.transform.position);
                        }
                        else
                        {
                            spawnEnemyServerRpc(enemyTypes.Count, StartOfRound.Instance.localPlayerController.gameplayCamera.transform.position);
                        }
                    });

                    var rectTransform = button.GetComponent<RectTransform>();
                    var contentRectTransform = EnemyListButtonTemplate.transform.parent.GetComponent<RectTransform>();

                    enemyTypes.Add(enemy);

                    float cumulativeHeight = ((rectTransform.rect.height / 2) + 5) + i * (rectTransform.rect.height + 5);

                    // set position, and update content size to contain the new button
                    contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, cumulativeHeight + ((rectTransform.rect.height / 2) + 5));

                    // calculate the cumulative height of buttons above the current one


                    // set position so that we start from the top of the scroll container
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -cumulativeHeight);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void spawnEnemyServerRpc(int enemyIndex, Vector3 position)
        {
            var enemy = enemyTypes[enemyIndex];
            spawnEnemy(enemy, position);
        }

        private enum EnemyTypes
        {
            inside,
            outside,
            daytime
        }

        public void spawnEnemy(EnemyType enemy, Vector3 position)
        {
            UnityEngine.Debug.Log("Attempting to spawn enemy from vent.");

            var enemyTypeIndex = RoundManager.Instance.currentLevel.Enemies.FindIndex(x => x.enemyType == enemy);

            var enemyType = EnemyTypes.inside;


            // if enemyTypeIndex is -1, then the enemy type is not in the current level, check outside enemies list

            if (enemyTypeIndex == -1)
            {
                enemyTypeIndex = RoundManager.Instance.currentLevel.OutsideEnemies.FindIndex(x => x.enemyType == enemy);
                enemyType = EnemyTypes.outside;
            }


            // if enemyTypeIndex is -1, then the enemy type is not in the current level, check daytime enemies list

            if (enemyTypeIndex == -1)
            {
                enemyTypeIndex = RoundManager.Instance.currentLevel.DaytimeEnemies.FindIndex(x => x.enemyType == enemy);
                enemyType = EnemyTypes.daytime;
            }


            if (enemyType == EnemyTypes.inside)
            {
                var vents = UnityEngine.Object.FindObjectsOfType<EnemyVent>();

                var pos = position;
                // closest vent
                EnemyVent vent = null;

                foreach (var v in vents)
                {
                    if (vent == null)
                    {
                        vent = v;
                        continue;
                    }

                    if (Vector3.Distance(pos, v.transform.position) < Vector3.Distance(pos, vent.transform.position))
                    {
                        vent = v;
                    }
                }

                vent.enemyType = enemy;

                vent.enemyTypeIndex = enemyTypeIndex;



                UnityEngine.Debug.Log($"Spawning enemy from vent {vent.name}.");



                RoundManager.Instance.SpawnEnemyFromVent(vent);
            }
            else if (enemyType == EnemyTypes.outside || enemyType == EnemyTypes.daytime)
            {
                GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

                GameObject spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

                var pos = RoundManager.Instance.GetRandomNavMeshPositionInRadius(spawnPoint.transform.position, 4f);

                GameObject enemyObject = UnityEngine.Object.Instantiate(enemy.enemyPrefab, pos, Quaternion.identity);

                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            }
            else
            {
                var pos = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 15f);

                GameObject enemyObject = UnityEngine.Object.Instantiate(enemy.enemyPrefab, pos, Quaternion.identity);

                enemyObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            }
        }



        [ServerRpc(RequireOwnership = false)]
        public void spawnItemServerRpc(int prefabIndex, Vector3 position)
        {
            var prefab = itemPrefabList[prefabIndex];
            spawnItem(prefab, position);
        }


        public void spawnItem(GameObject prefab, Vector3 position)
        {
            
            var gameObject = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            gameObject.GetComponent<NetworkObject>().Spawn();
        }

        public void ToggleViewport(int index)
        {
            for (int i = 0; i < mainViewports.Count; i++)
            {
                if (i == index)
                {

                    mainViewports[i].SetActive(!mainViewports[i].activeSelf);
                }
                else
                {
                    mainViewports[i].SetActive(false);
                }
            }
        }

        public void SetMoney()
        {
            if (terminal != null && MoneyInputField.text != "")
            {
                // check if valid number
                if (int.TryParse(MoneyInputField.text, out int money))
                {
                    // set money
                    MoneyText.text = money.ToString();
                    if (IsHost)
                    {
                        if (money < 0)
                        {
                            money = terminal.groupCredits;
                        }
                        else
                        {
                            terminal.groupCredits = money;
                        }
                        terminal.SyncGroupCreditsClientRpc(money, terminal.numberOfItemsInDropship);
                    }
                    else
                    {
                        terminal.SyncGroupCreditsServerRpc(money, terminal.numberOfItemsInDropship);
                    }
                }
            }
        }
    }
}
