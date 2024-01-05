using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalThings.MonoBehaviours
{
    public class Pinger : GrabbableObject
    {
        public RenderTexture renderTexture;

        public Camera renderCamera;

        public Material screenMat;

        public Light backLight;

        public AudioSource audioSource;
        public AudioSource audioSourceFar;

        [Space(3f)]

        private static int renderTextureID = 0;
        private int currentRenderTextureID = 0;

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

        }


    }
}
