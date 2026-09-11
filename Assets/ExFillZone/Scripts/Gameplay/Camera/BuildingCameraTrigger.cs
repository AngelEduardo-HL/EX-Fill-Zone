using Unity.Cinemachine;
using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    [RequireComponent(typeof(Collider))]
    public sealed class BuildingCameraTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildingCameraController controller;
        [SerializeField] private CinemachineCamera buildingCamera;

        [Header("Fade")]
        [SerializeField] private BuildingFadeGroup[] fadeGroups;

        private bool playerInside;

        public CinemachineCamera BuildingCamera => buildingCamera;

        private void Awake()
        {
            if (controller == null) controller = GetComponentInParent<BuildingCameraController>();

            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;

            controller?.Register(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (playerInside || !other.CompareTag("Player")) return;

            playerInside = true;

            foreach (BuildingFadeGroup group in fadeGroups)
            {
                if (group != null) group.Enter();
            }

            controller?.EnterZone(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!playerInside || !other.CompareTag("Player")) return;

            playerInside = false;

            foreach (BuildingFadeGroup group in fadeGroups)
            {
                if (group != null) group.Exit();
            }

            controller?.ExitZone(this);
        }

        private void OnDisable()
        {
            if (!playerInside) return;

            playerInside = false;

            foreach (BuildingFadeGroup group in fadeGroups)
            {
                if (group != null) group.Exit();
            }

            controller?.ExitZone(this);
        }

        public void SetCameraPriority(int priority)
        {
            if (buildingCamera != null) buildingCamera.Priority.Value = priority;
        }
    }
}