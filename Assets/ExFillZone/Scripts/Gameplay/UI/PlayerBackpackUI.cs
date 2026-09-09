using DG.Tweening;
using ExFillZone.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerBackpackUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAmmo playerAmmo;
        [SerializeField] private Image backpackFill;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float fillDuration = 0.20f;

        private int lastAmmo = -1;
        private Tween fillTween;

        private void Start()
        {
            if (playerAmmo == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerAmmo = player.GetComponent<PlayerAmmo>();
            }

            if (playerAmmo == null || backpackFill == null)
            {
                Debug.LogError("PlayerBackpackUI tiene referencias sin asignar.", this);
                enabled = false;
                return;
            }

            lastAmmo = playerAmmo.CurrentAmmo;
            backpackFill.fillAmount = GetFillAmount();
        }

        private void Update()
        {
            if (playerAmmo.CurrentAmmo == lastAmmo) return;

            lastAmmo = playerAmmo.CurrentAmmo;
            UpdateBackpack();
        }

        private float GetFillAmount()
        {
            if (playerAmmo.MaxAmmo <= 0) return 0f;
            return (float)playerAmmo.CurrentAmmo / playerAmmo.MaxAmmo;
        }

        private void UpdateBackpack()
        {
            fillTween?.Kill();
            fillTween = backpackFill.DOFillAmount(GetFillAmount(), fillDuration).SetEase(Ease.OutQuad);
        }

        private void OnDisable()
        {
            fillTween?.Kill();
        }
    }
}