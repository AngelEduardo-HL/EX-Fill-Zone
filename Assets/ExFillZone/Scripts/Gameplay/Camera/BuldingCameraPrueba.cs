using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    public sealed class BuldingCameraPrueba : MonoBehaviour
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

        private void Awake()
        {
            PrepareRoofInitialState();

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

            // Al entrar, desactivar el MeshRenderer (ocultar el tejado)
            SetRenderersEnabled(false);
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

            // Al salir, reactivar el MeshRenderer (mostrar el tejado)
            SetRenderersEnabled(true);
        }

        private void PrepareRoofInitialState()
        {
            // Asegurar estado inicial visible
            SetRenderersEnabled(true);
        }

        private void SetRenderersEnabled(bool enabled)
        {
            foreach (Renderer roof in roofParts)
            {
                if (roof == null)
                {
                    continue;
                }

                // Preferir desactivar el MeshRenderer si existe
                MeshRenderer meshRenderer = roof.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = enabled;
                    continue;
                }

                // Fallback: desactivar el Renderer (por ejemplo SkinnedMeshRenderer u otros)
                roof.enabled = enabled;
            }
        }
    }

}