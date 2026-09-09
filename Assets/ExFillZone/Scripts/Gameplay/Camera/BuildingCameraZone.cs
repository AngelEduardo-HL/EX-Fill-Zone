using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    public sealed class BuildingCameraZone : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField]
        private CinemachineCamera playerCamera;

        [SerializeField]
        private CinemachineCamera buildingCamera;

        [SerializeField]
        private int playerPriority = 20;

        [SerializeField]
        private int buildingPriority = 30;

        [Header("Roof")]
        [SerializeField]
        private Renderer[] roofParts;

        [SerializeField, Range(0f, 1f)]
        private float outsideAlpha = 0f;

        [SerializeField, Range(0f, 1f)]
        private float insideAlpha = 0.9f;

        [SerializeField, Min(0.1f)]
        private float fadeSpeed = 3f;

        private readonly List<Material> roofMaterials = new List<Material>();

        private float targetFade;
        private static readonly int FadeID = Shader.PropertyToID("_Fade");

        private void Awake()
        {
            targetFade = outsideAlpha;

            PrepareRoofMaterials();

            SetAlpha(outsideAlpha);

            if (playerCamera != null)
            {
                playerCamera.Priority.Value =
                    playerPriority;
            }

            if (buildingCamera != null)
            {
                buildingCamera.Priority.Value =
                    playerPriority - 10;
            }
        }

        private void Update()
        {
            FadeRoof();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            EnterBuilding();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            ExitBuilding();
        }

        private void EnterBuilding()
        {
            if (buildingCamera != null)
            {
                buildingCamera.Priority.Value =
                    buildingPriority;
            }

            targetFade = insideAlpha;
        }

        private void ExitBuilding()
        {
            if (buildingCamera != null)
            {
                buildingCamera.Priority.Value =
                    playerPriority - 10;
            }

            if (playerCamera != null)
            {
                playerCamera.Priority.Value =
                    playerPriority;
            }

            targetFade = outsideAlpha;
        }

        private void PrepareRoofMaterials()
        {
            roofMaterials.Clear();

            foreach (Renderer roof in roofParts)
            {
                if (roof == null)
                {
                    continue;
                }

                foreach (Material material in roof.materials)
                {
                    roofMaterials.Add(material);
                }
            }
        }

        private void FadeRoof()
        {
            foreach (Material material in roofMaterials)
            {
                if (material == null || !material.HasProperty(FadeID))
                {
                    continue;
                }

                float currentFade = material.GetFloat(FadeID);

                currentFade = Mathf.MoveTowards(currentFade,targetFade, fadeSpeed * Time.deltaTime);

                material.SetFloat(FadeID, currentFade);
            }
        }

        private void SetAlpha(float alpha)
        {
            foreach (Material material in roofMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                Color color = GetColor(material);
                color.a = alpha;

                SetColor(material, color);
            }
        }

        private Color GetColor(Material material)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.color;
        }

        private void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                return;
            }
            material.color = color;
        }
    }

}