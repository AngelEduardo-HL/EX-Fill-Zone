using ExFillZone.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.UI
{
    public sealed class PlayerStaminaUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerStamina playerStamina;

        [SerializeField]
        private Image staminaFill;

        [Header("Animation")]
        [SerializeField, Min(0.1f)]
        private float fillSpeed = 5f;

        private float currentFill;

        private void Start()
        {
            if (playerStamina == null)
            {
                GameObject player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    playerStamina =
                        player.GetComponent<PlayerStamina>();
                }
            }

            if (playerStamina == null ||
                staminaFill == null)
            {
                Debug.LogError(
                    "PlayerStaminaUI tiene referencias sin asignar.",
                    this
                );

                enabled = false;
                return;
            }

            currentFill =
                playerStamina.NormalizedStamina;

            staminaFill.fillAmount =
                currentFill;
        }

        private void Update()
        {
            float targetFill =
                playerStamina.NormalizedStamina;

            currentFill =
                Mathf.MoveTowards(
                    currentFill,
                    targetFill,
                    fillSpeed * Time.deltaTime
                );

            staminaFill.fillAmount =
                currentFill;
        }
    }
}