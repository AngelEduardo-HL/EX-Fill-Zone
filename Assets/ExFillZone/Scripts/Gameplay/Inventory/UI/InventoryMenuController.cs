using System.Collections.Generic;
using DG.Tweening;
using ExFillZone.Gameplay.CameraSystem;
using ExFillZone.Gameplay.Player;
using ExFillZone.Gameplay.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    [DefaultExecutionOrder(-300)]
    public sealed class InventoryMenuController : MonoBehaviour
    {
        [Header("Interfaz")]
        [SerializeField] private RectTransform inventoryRoot;
        [SerializeField] private CanvasGroup inventoryGroup;
        [SerializeField] private InventoryDragController dragController;
        [SerializeField] private bool startOpen;
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Fondo")]
        [SerializeField, Range(0f, 0.8f)] private float backgroundDarkness = 0.3f;
        [SerializeField] private bool enableDepthOfField = true;
        [SerializeField, Range(0f, 1f)] private float blurStrength = 0.85f;

        private static InventoryMenuController current;
        private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();

        private PlayerCrosshairUI crosshairUI;
        private Image backgroundDim;
        private Volume blurVolume;
        private VolumeProfile runtimeProfile;
        private Tween fadeTween;
        private Tween blurTween;
        private bool isOpen;
        private bool cursorWasVisible;
        private CursorLockMode previousLockMode;

        public static bool IsOpen => current != null && current.isOpen;
        public bool Open => isOpen;

        private void Awake()
        {
            current = this;

            if (inventoryRoot == null || inventoryGroup == null)
            {
                Debug.LogError("[Inventario] Asigna Inventory Root e Inventory Group.", this);
                enabled = false;
                return;
            }

            if (dragController == null) dragController = GetComponent<InventoryDragController>();

            CreateBackgroundDim();
            CreateDepthOfField();

            inventoryGroup.alpha = 0f;
            inventoryGroup.interactable = false;
            inventoryGroup.blocksRaycasts = false;
            if (backgroundDim != null) backgroundDim.color = new Color(0f, 0f, 0f, backgroundDarkness);
            if (blurVolume != null) blurVolume.weight = 0f;
        }

        private void Start()
        {
            crosshairUI = FindFirstObjectByType<PlayerCrosshairUI>();

            if (startOpen) SetOpen(true, true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (dragController == null || !dragController.IsDragging)
                    SetOpen(!isOpen);
            }
            else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                if (dragController == null || !dragController.IsDragging)
                    SetOpen(false);
            }
        }

        private void LateUpdate()
        {
            // El cursor del inventario tiene prioridad sobre la retícula del jugador.
            if (!isOpen) return;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void SetOpen(bool open, bool immediate = false)
        {
            if (inventoryGroup == null || (isOpen == open && !immediate)) return;
            if (!open && dragController != null && dragController.IsDragging) return;

            isOpen = open;

            fadeTween?.Kill();
            blurTween?.Kill();

            if (open)
            {
                cursorWasVisible = Cursor.visible;
                previousLockMode = Cursor.lockState;

                PauseGameplay();
                crosshairUI?.OnInventoryOpened();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                inventoryGroup.interactable = true;
                inventoryGroup.blocksRaycasts = true;
            }
            else
            {
                inventoryGroup.interactable = false;
                inventoryGroup.blocksRaycasts = false;

                ResumeGameplay();
                crosshairUI?.OnInventoryClosed();

                Cursor.lockState = previousLockMode;
                Cursor.visible = cursorWasVisible;
            }

            float alpha = open ? 1f : 0f;
            if (immediate || fadeDuration <= 0f)
                inventoryGroup.alpha = alpha;
            else
                fadeTween = inventoryGroup.DOFade(alpha, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true);

            if (blurVolume != null)
            {
                float weight = open ? blurStrength : 0f;

                if (immediate || fadeDuration <= 0f)
                    blurVolume.weight = weight;
                else
                    blurTween = DOTween.To(() => blurVolume.weight, value => blurVolume.weight = value, weight, fadeDuration)
                        .SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }

        private void PauseGameplay()
        {
            pausedBehaviours.Clear();

            foreach (PlayerMotor motor in FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None))
            {
                motor.StopImmediately();
                Pause(motor);
            }

            foreach (PlayerAimController aim in FindObjectsByType<PlayerAimController>(FindObjectsSortMode.None)) Pause(aim);
            foreach (PlayerShooter shooter in FindObjectsByType<PlayerShooter>(FindObjectsSortMode.None)) Pause(shooter);
            foreach (PlayerCameraController camera in FindObjectsByType<PlayerCameraController>(FindObjectsSortMode.None)) Pause(camera);
            foreach (TopDownCameraController camera in FindObjectsByType<TopDownCameraController>(FindObjectsSortMode.None)) Pause(camera);
        }

        private void Pause(Behaviour behaviour)
        {
            if (behaviour == null || !behaviour.isActiveAndEnabled) return;

            pausedBehaviours.Add(behaviour);
            behaviour.enabled = false;
        }

        private void ResumeGameplay()
        {
            foreach (Behaviour behaviour in pausedBehaviours)
            {
                if (behaviour != null) behaviour.enabled = true;
            }

            pausedBehaviours.Clear();
        }

        private void CreateBackgroundDim()
        {
            GameObject overlay = new GameObject("InventoryBackgroundDim", typeof(RectTransform), typeof(Image));
            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.SetParent(inventoryRoot, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();

            backgroundDim = overlay.GetComponent<Image>();
            backgroundDim.color = new Color(0f, 0f, 0f, backgroundDarkness);
            backgroundDim.raycastTarget = false;
        }

        private void CreateDepthOfField()
        {
            if (!enableDepthOfField || Camera.main == null) return;

            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            DepthOfField depthOfField = runtimeProfile.Add<DepthOfField>(true);
            depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            depthOfField.gaussianStart.Override(0.1f);
            depthOfField.gaussianEnd.Override(2f);
            depthOfField.gaussianMaxRadius.Override(1f);

            blurVolume = Camera.main.gameObject.AddComponent<Volume>();
            blurVolume.isGlobal = true;
            blurVolume.priority = 100f;
            blurVolume.weight = 0f;
            blurVolume.sharedProfile = runtimeProfile;
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();

            if (backgroundDim != null) Destroy(backgroundDim.gameObject);
            if (blurVolume != null) Destroy(blurVolume);
            if (runtimeProfile != null) Destroy(runtimeProfile);
        }

        private void Cleanup()
        {
            fadeTween?.Kill();
            blurTween?.Kill();

            if (isOpen)
            {
                isOpen = false;
                ResumeGameplay();
                crosshairUI?.OnInventoryClosed();
                Cursor.lockState = previousLockMode;
                Cursor.visible = cursorWasVisible;
            }

            if (current == this) current = null;
        }
    }
}
