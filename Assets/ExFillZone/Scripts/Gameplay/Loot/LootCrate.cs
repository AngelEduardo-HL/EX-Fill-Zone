using DG.Tweening;
using UnityEngine;

namespace ExFillZone.Gameplay.Loot
{
    public sealed class LootCrate : MonoBehaviour
    {
        [Header("Loot")]
        [SerializeField, Min(0)] private int minMoney = 100;
        [SerializeField, Min(0)] private int maxMoney = 500;

        [Header("Animation")]
        [SerializeField] private Transform lid;
        [SerializeField] private Vector3 openRotationOffset = new Vector3(-100f, 0f, 0f);
        [SerializeField, Min(0.01f)] private float openDuration = 0.45f;

        private bool opened;
        private Vector3 closedRotation;

        public bool IsOpened => opened;

        private void Awake()
        {
            if (lid != null) closedRotation = lid.localEulerAngles;
        }

        public bool TryOpen()
        {
            if (opened) return false;

            opened = true;

            if (lid == null)
            {
                GiveLoot();
                return true;
            }

            lid.DOKill();
            lid.DOLocalRotate(closedRotation + openRotationOffset, openDuration).SetEase(Ease.OutCubic).OnComplete(GiveLoot);

            return true;
        }

        private void GiveLoot()
        {
            int money = Random.Range(minMoney, maxMoney + 1);
            RunLoot.AddMoney(money);
        }

        private void OnDestroy()
        {
            if (lid != null) lid.DOKill();
        }
    }
}