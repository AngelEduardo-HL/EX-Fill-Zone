using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    public sealed class BuildingCameraController : MonoBehaviour
    {
        [Header("Player Camera")]
        [SerializeField] private CinemachineCamera playerCamera;

        [Header("Priority")]
        [SerializeField] private int playerPriority = 20;
        [SerializeField] private int buildingPriority = 30;

        private readonly List<BuildingCameraTrigger> activeZones = new List<BuildingCameraTrigger>();
        private CinemachineCamera currentCamera;

        private void Awake()
        {
            if (playerCamera != null) playerCamera.Priority.Value = playerPriority;
        }

        public void Register(BuildingCameraTrigger zone)
        {
            if (zone == null) return;
            zone.SetCameraPriority(playerPriority - 10);
        }

        public void EnterZone(BuildingCameraTrigger zone)
        {
            if (zone == null) return;

            if (!activeZones.Contains(zone)) activeZones.Add(zone);

            ActivateCamera(zone.BuildingCamera);
        }

        public void ExitZone(BuildingCameraTrigger zone)
        {
            if (zone == null) return;

            activeZones.Remove(zone);
            zone.SetCameraPriority(playerPriority - 10);

            if (activeZones.Count > 0)
            {
                ActivateCamera(activeZones[activeZones.Count - 1].BuildingCamera);
                return;
            }

            currentCamera = null;

            if (playerCamera != null) playerCamera.Priority.Value = playerPriority;
        }

        private void ActivateCamera(CinemachineCamera camera)
        {
            if (camera == null) return;

            if (currentCamera != null && currentCamera != camera) currentCamera.Priority.Value = playerPriority - 10;

            currentCamera = camera;
            currentCamera.Priority.Value = buildingPriority;
        }
    }
}