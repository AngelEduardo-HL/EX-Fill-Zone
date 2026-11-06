using DG.Tweening;
using UnityEngine;
using System;

namespace ExFillZone.Gameplay.Loot
{
    public sealed class LootCrate : MonoBehaviour
    {
        [Header("Loot")]
        [SerializeField, Min(0)] private int minMoney = 100;
        [SerializeField, Min(0)] private int maxMoney = 500;

        [Header("Reward Mode")]
        [SerializeField] private bool giveMoneyOnOpen = true;

        [Header("Animation")]
        [SerializeField] private Transform lid;
        [SerializeField] private Vector3 openRotationOffset = new Vector3(-100f, 0f, 0f);
        [SerializeField, Min(0.01f)] private float openDuration = 0.45f;

        private bool opened;
        private Vector3 closedRotation;
        public event Action OpenCompleted;


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
                OpenCompleted?.Invoke();
                return true;
            }

            lid.DOKill();

            lid.DOLocalRotate(closedRotation + openRotationOffset, openDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    GiveLoot();
                    OpenCompleted?.Invoke();
                });

            return true;
        }

        private void GiveLoot()
        {
            if (!giveMoneyOnOpen) return;

            int money = UnityEngine.Random.Range(minMoney, maxMoney + 1);
            RunLoot.AddMoney(money);
        }

        private void OnDestroy()
        {
            if (lid != null) lid.DOKill();
        }
    }
}