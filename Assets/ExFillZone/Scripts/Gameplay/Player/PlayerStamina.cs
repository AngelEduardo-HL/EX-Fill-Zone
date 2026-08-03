using UnityEngine;

namespace ExFillZone.Gameplay.Player
{
    public sealed class PlayerStamina : MonoBehaviour
    {
        [Header("Stamina")]
        [SerializeField, Min(1f)]
        private float maximumStamina = 100f;

        [SerializeField, Min(0f)]
        private float sprintDrainPerSecond = 20f;

        [SerializeField, Min(0f)]
        private float recoveryPerSecond = 18f;

        [SerializeField, Min(0f)]
        private float recoveryDelay = 1f;

        [Header("Exhaustion")]
        [SerializeField, Range(0.01f, 1f)]
        private float restartExhaustion = 0.20f;

        [Header("Runtime Information")]
        [SerializeField]
        private float currentStamina;

        private float sprintRecovery;
        private bool sprintRestart;

        public float CurrentStamina =>
            currentStamina;

        public float MaximumStamina =>
            maximumStamina;

        public float NormalizedStamina =>
            maximumStamina > 0f
                ? currentStamina / maximumStamina
                : 0f;

        public bool IsExhausted { get; private set; }

        public bool CanSprint =>
            !IsExhausted &&
            currentStamina > 0f;

        private void Awake()
        {
            currentStamina = maximumStamina;
            sprintRestart = true;
        }

        public void Tick(
            bool isSprinting,
            bool sprintInputHeld,
            float deltaTime
        )
        {
            RegisterSprintRelease(sprintInputHeld);

            if (isSprinting && CanSprint)
            {
                ConsumeStamina(deltaTime);
            }
            else
            {
                RecoverStamina(deltaTime);
            }

            UpdateExhaustionState();
        }

        private void ConsumeStamina(float deltaTime)
        {
            currentStamina -=
                sprintDrainPerSecond * deltaTime;

            currentStamina = Mathf.Max(
                currentStamina,
                0f
            );

            sprintRecovery =
                recoveryDelay;

            if (currentStamina <= 0f)
            {
                IsExhausted = true;
                sprintRestart = false;
            }
        }

        private void RecoverStamina(float deltaTime)
        {
            if (sprintRecovery > 0f)
            {
                sprintRecovery -= deltaTime;
                return;
            }

            currentStamina = Mathf.MoveTowards(
                currentStamina,
                maximumStamina,
                recoveryPerSecond * deltaTime
            );
        }

        private void RegisterSprintRelease(
            bool sprintInputHeld
        )
        {
            if (IsExhausted && !sprintInputHeld)
            {
                sprintRestart = true;
            }
        }

        private void UpdateExhaustionState()
        {
            if (!IsExhausted)
            {
                return;
            }

            float requiredStamina =
                maximumStamina *
                restartExhaustion;

            bool recoveredEnough =
                currentStamina >= requiredStamina;

            if (recoveredEnough &&
                sprintRestart)
            {
                IsExhausted = false;
            }
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentStamina = Mathf.Min(
                currentStamina + amount,
                maximumStamina
            );
        }

        public void RefillStamina()
        {
            currentStamina = maximumStamina;
            sprintRecovery = 0f;
            IsExhausted = false;
            sprintRestart = true;
        }
    }
}