using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class SeasonalHandler : NetworkBehaviour
    {
        public GameObject[] crimasObjects;
        public AudioClip[] crimasNoisemakerSounds;
        public AudioClip[] crimasFarNoisemakerSounds;

        public void Start()
        {
            // if it is current christmas, enable all christmas objects
            if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 20)
            {
                foreach (var crimasObject in crimasObjects)
                {
                    crimasObject.SetActive(true);
                }

                var noiseMaker = GetComponent<NoisemakerProp>();

                if (noiseMaker != null)
                {
                    var noiseSfx = noiseMaker.noiseSFX.ToList();
                    foreach (var crimasItemSound in crimasNoisemakerSounds)
                    {
                        noiseSfx.Add(crimasItemSound);
                    }
                    noiseMaker.noiseSFX = noiseSfx.ToArray();

                    var noiseFarSfx = noiseMaker.noiseSFXFar.ToList();
                    foreach (var crimasFarItemSound in crimasFarNoisemakerSounds)
                    {
                        noiseFarSfx.Add(crimasFarItemSound);
                    }
                    noiseMaker.noiseSFXFar = noiseFarSfx.ToArray();
                }
            }


            
        }


    }
}
