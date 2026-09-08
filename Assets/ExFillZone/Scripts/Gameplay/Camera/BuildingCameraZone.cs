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
        private float outsideAlpha = 1f;

        [SerializeField, Range(0f, 1f)]
        private float insideAlpha = 0.1f;

        [SerializeField, Min(0.1f)]
        private float fadeSpeed = 3f;

        private readonly List<Material> roofMaterials =
            new List<Material>();

        private float targetAlpha;

        private void Awake()
        {
            targetAlpha = outsideAlpha;

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

            targetAlpha = insideAlpha;
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

            targetAlpha = outsideAlpha;
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
                if (material == null)
                {
                    continue;
                }

                Color color = GetColor(material);

                color.a = Mathf.MoveTowards(
                    color.a,
                    targetAlpha,
                    fadeSpeed * Time.deltaTime
                );

                SetColor(
                    material,
                    color
                );
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

                SetColor(
                    material,
                    color
                );
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

        private void SetColor(
            Material material,
            Color color
        )
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    color
                );

                return;
            }

            material.color = color;
        }
    }

}