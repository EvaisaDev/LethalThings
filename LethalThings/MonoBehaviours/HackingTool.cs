using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LethalThings.MonoBehaviours
{
    public class HackingTool : GrabbableObject
    {
        public RenderTexture renderTexture;

        public Camera renderCamera;

        public Material screenMat;

        public Light backLight;

        public AudioSource audioSource;
        public AudioSource audioSourceFar;

        [Space(3f)]

        public AudioClip turnOnSound;
        public AudioClip turnOffSound;
        public AudioClip connecting;
        public AudioClip buttonAccept;
        public AudioClip hackingSuccess;
        public AudioClip hackingCorrect;
        public AudioClip hackingFailed;

        public int noiseRange = 45;

        [Space(3f)]

        public Transform UISystem;
        public TextMeshProUGUI hackString;
        public TextMeshProUGUI hackInput;
        public TextMeshProUGUI targetString;
        public TextMeshProUGUI progressString;
        public Slider connectionBar;
        /*
         * This shit ain't workin chief.
         * 
        [System.Serializable]
        public class HackStateEntry
        {
            public HackState state;
            public Transform guiElement;
        }

        public List<HackStateEntry> entries;
        */

        public List<Transform> hackGuiElements;

        [Space(3f)]

        public float connectionTime = 5f;
        private float connectionTimer = 0f;
        public float maxHackDistance = 15f;
        public float hackUpdateTime = 1f;
        private float hackUpdateTimer = 0f;
        public float resetTime = 3f;
        public int hackCount = 8;
        public int currentHack = 1;
        public int currentHackAnswer = 0;

        private static int renderTextureID = 0;
        private int currentRenderTextureID = 0;


        public enum HackState
        {
            Off,
            Selection,
            Connecting,
            Hacking,
            Failed,
            Success
        }


        public NetworkVariable<HackState> hackState = new NetworkVariable<HackState>(HackState.Off, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private HackState lastHackState = HackState.Off;
        private TerminalAccessibleObject selectedTarget;
        private string answerString = "";

        public void PlaySoundByID(string soundID)
        {
            if (IsHost)
            {
                PlaySoundClientRpc(soundID);
            }
            else
            {
                PlaySoundServerRpc(soundID);
            }
        }

        [ServerRpc]
        public void PlaySoundServerRpc(string sound)
        {
            PlaySoundClientRpc(sound);
        }

        [ClientRpc]
        public void PlaySoundClientRpc(string sound)
        {
            PlaySound(sound);
        }

        public void PlaySound(string soundID)
        {
            UnityEngine.Debug.Log("Playing target switch sound");

            AudioClip sound = null;

            switch (soundID)
            {
                case "turnOn":
                {
                    sound = turnOnSound;
                    break;
                }
                case "turnOff":
                {
                    sound = turnOffSound;
                    break;
                }
                case "connecting":
                {
                    sound = connecting;
                    break;
                }
                case "buttonAccept":
                {
                    sound = buttonAccept;
                    break;
                }
                case "hackingSuccess":
                {
                    sound = hackingSuccess;
                    break;
                }
                case "hackingCorrect":
                {
                    sound = hackingCorrect;
                    break;
                }
                case "hackingFailed":
                {
                    sound = hackingFailed;
                    break;
                }
            }

            if(sound == null)
            {
                return;
            }

            audioSource.PlayOneShot(sound, 1);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(sound, 1);
            }
            WalkieTalkie.TransmitOneShotAudio(audioSource, sound, 1);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }

        public bool turnedOn
        {
            get
            {
                return hackState.Value != HackState.Off;
            }
        }

        public bool isHacking
        {
            get
            {
                return hackState.Value == HackState.Hacking;
            }
        }

        public void SwitchHackState(HackState state)
        {
            if (IsOwner)
            {
                hackState.Set(state);
            }
            var index = 0;
            foreach (var entry in hackGuiElements)
            {
                index++;
                if (entry == null)
                {
                    continue;
                }
                entry.gameObject.SetActive((HackState)(index) == state);
            }

            if(state != HackState.Connecting)
            {
                connectionTimer = 0f;
            }

            switch (state)
            {
                case HackState.Selection:
                    {
                        targetString.text = "No target";
                        answerString = "";
                        break;
                    }
                case HackState.Hacking:
                    {
                        currentHack = 1;
                        GenerateHack();
                        break;
                    }
                case HackState.Connecting:
                    {
                        PlaySoundByID("connecting");
                        break;
                    }
            }
        }

        public IEnumerator Reset()
        {
            yield return new WaitForSeconds(resetTime);
            if (IsOwner && hackState.Value != HackState.Off)
            {
                hackState.Set(HackState.Selection);
            }
        }

        public static void Load()
        {
            On.GameNetcodeStuff.PlayerControllerB.PerformEmote += PlayerControllerB_PerformEmote;
        }

        private static void PlayerControllerB_PerformEmote(On.GameNetcodeStuff.PlayerControllerB.orig_PerformEmote orig, GameNetcodeStuff.PlayerControllerB self, InputAction.CallbackContext context, int emoteID)
        {
            // if hacking tool is equipped, don't emote
            if (self.currentlyHeldObjectServer != null && self.currentlyHeldObjectServer is HackingTool)
            {
                var hackingToolScript = (HackingTool)self.currentlyHeldObjectServer;
                if (hackingToolScript.isHacking)
                {
                    Plugin.logger.LogInfo("Hacking tool equipped, not emoting");
                    return;
                }
            }

            orig(self, context, emoteID);
        }

        public void GenerateHack()
        {

            var input1 = UnityEngine.Random.Range(1, 10);
            var input2 = UnityEngine.Random.Range(1, 10);

            // select whether plus or minus
            var plus = UnityEngine.Random.Range(0, 2) == 0;

            // calculate answer
            currentHackAnswer = Math.Abs(plus ? input1 + input2 : input1 - input2) % 10;

            var symbol = plus ? "+" : "-";
            // set text
            hackString.text = $"MOD(|{input1}{symbol}{input2}|)";
        }

        public void Awake()
        {
            mainObjectRenderer = transform.Find("Tool/Cube").GetComponent<MeshRenderer>();

            renderTexture = new RenderTexture(500, 390, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = $"HackingToolRenderTexture({renderTextureID})";
            currentRenderTextureID = 0;
            renderTextureID++;
            // setup camera to render to texture
            renderCamera.targetTexture = renderTexture;
            screenMat.mainTexture = renderTexture;

            // duplicate material
            screenMat = new Material(screenMat);

            

            // set material
            mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenMat });

            backLight.intensity = 2f;


            UnityEngine.Debug.Log("init waaaa");

        }

        public override void Update()
        {
            UISystem.position = new Vector3(-1000, -1000 * (currentRenderTextureID + 1), -1000);

            isBeingUsed = turnedOn;

            // if battery is dead, turn off
            if (turnedOn && insertedBattery.charge <= 0)
            {
                if (IsOwner)
                {
                    hackState.Set(HackState.Off);
                }
            }

            backLight.enabled = turnedOn;


            if (selectedTarget != null && Vector3.Distance(selectedTarget.transform.position, transform.position) > maxHackDistance)
            {

                selectedTarget = null;
                targetString.text = "No target";
                if (hackState.Value != HackState.Off)
                {
                    if (IsOwner)
                    {
                        hackState.Set(HackState.Selection);
                    }
                }

            }

            if (hackState.Value != lastHackState)
            {
                lastHackState = hackState.Value;
                SwitchHackState(hackState.Value);
            }

            switch (hackState.Value)
            {
                case HackState.Selection:
                    {
                        

                        hackUpdateTimer += Time.deltaTime;

                        var doUpdate = false;
                        if (hackUpdateTimer > hackUpdateTime)
                        {
                            doUpdate = true;
                            hackUpdateTimer = 0f;
                        }

                        currentHack = 1;

                        progressString.text = $"________";
                        answerString = "";

                        if (doUpdate)
                        {
                            // find closest hackable object
                            TerminalAccessibleObject closest = null;
                            float closestDist = maxHackDistance;
                            var hackableObjects = FindObjectsOfType<TerminalAccessibleObject>();

                            foreach (var obj in hackableObjects)
                            {
                                if (obj == null || obj.inCooldown || !obj.isPoweredOn)
                                {
                                    continue;
                                }
                                float dist = Vector3.Distance(obj.transform.position, transform.position);

                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    closest = obj;
                                }
                            }

                            if (closest != null)
                            {
                                selectedTarget = closest;
                                targetString.text = closest.objectCode;
                            }
                            else
                            {
                                selectedTarget = null;
                                targetString.text = "No target";
                            }

                            
                        }
                       
                        break;
                    }
                case HackState.Connecting:
                    {
                        if (selectedTarget == null)
                        {
                            if (IsOwner)
                            {
                                hackState.Set(HackState.Selection);
                            }
                            break;
                        }

                        if (connectionTimer > connectionTime)
                        {
                            if (IsOwner)
                            {
                                hackState.Set(HackState.Hacking);
                                PlaySoundByID("buttonAccept");
                            }
                            break;
                        }

                        connectionTimer += Time.deltaTime;
                        connectionBar.value = connectionTimer / connectionTime;

                        break;
                    }
                case HackState.Hacking:
                    {
                        // check for input key press
                        var input = -1;

                        if(Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame)
                        {
                            input = 0;
                        }
                        else if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                        {
                            input = 1;
                        }
                        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                        {
                            input = 2;
                        }
                        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                        {
                            input = 3;
                        }
                        else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
                        {
                            input = 4;
                        }
                        else if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame)
                        {
                            input = 5;
                        }
                        else if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame)
                        {
                            input = 6;
                        }
                        else if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
                        {
                            input = 7;
                        }
                        else if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame)
                        {
                            input = 8;
                        }
                        else if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame)
                        {
                            input = 9;
                        }

                        if(input != -1)
                        {
                            if(input == currentHackAnswer)
                            {
                                answerString += input.ToString();
                                // correct
                                progressString.text = $"{answerString}{string.Join("", Enumerable.Repeat("_", hackCount - (currentHack)))}";
                                if (currentHack >= hackCount)
                                {
                                    // success
                                    if (IsOwner)
                                    {
                                        hackState.Set(HackState.Success);
                                        selectedTarget.CallFunctionFromTerminal();
                                        PlaySoundByID("hackingSuccess");
                                    }

                                    StartCoroutine(Reset());
                                }
                                else
                                {
                                    GenerateHack();
                                    PlaySoundByID("hackingCorrect");
                                }
                                currentHack++;
                            }
                            else
                            {
                                // fail
                                if (IsOwner)
                                {
                                    hackState.Set(HackState.Failed);
                                    PlaySoundByID("hackingFailed");
                                }
                                StartCoroutine(Reset());
                            }
                        }


                        break;
                    }
            }



 

            base.Update();
        }

        /*
        [ServerRpc]
        public void SwitchScreenServerRpc(bool on)
        {
            SwitchScreenClientRpc(on);
            SwitchScreen(on);
        }

        [ClientRpc]
        public void SwitchScreenClientRpc(bool on)
        {
            SwitchScreen(on);
        }

        public void SwitchScreen(bool on)
        {
            UnityEngine.Debug.Log("Switching screen: " + on);

            turnedOn = on;

            var sound = on ? turnOnSound : turnOffSound;

            audioSource.PlayOneShot(sound, 1);
            if (audioSourceFar != null)
            {
                audioSourceFar.PlayOneShot(sound, 1);
            }
            WalkieTalkie.TransmitOneShotAudio(audioSource, sound, 1);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

            if (on)
            {
                mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenOnMat });
            }
            else
            {
                mainObjectRenderer.SetMaterials(new List<Material>() { mainObjectRenderer.materials[0], screenOffMat });
            }
        }*/

        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
            hackState.Value = HackState.Off;
            selectedTarget = null;
            currentHack = 1;
            currentHackAnswer = 0;
            answerString = "";
            backLight.enabled = false;
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!IsOwner)
            {
                return;
            }


            if (selectedTarget != null)
            {
                // switch to connecting state
                if (hackState.Value == HackState.Selection)
                {
                    hackState.Set(HackState.Connecting);
                    PlaySoundByID("buttonAccept");
                }
            }

        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (!IsOwner)
            {
                return;
            }

            if (!right)
            {
                if(hackState.Value != HackState.Off)
                {
                    hackState.Set(HackState.Off);
                    selectedTarget = null;
                    PlaySoundByID("turnOff");
                    backLight.enabled = false;
                }
                else
                {
                    hackState.Set(HackState.Selection);
                    UnityEngine.Debug.Log("Switching to selection");
                    PlaySoundByID("turnOn");
                    backLight.enabled = true;
                }
            }

        }
    }
}
