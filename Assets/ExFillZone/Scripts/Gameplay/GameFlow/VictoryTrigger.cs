using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExFillZone.Gameplay.GameFlow
{
    public sealed class VictoryTrigger : MonoBehaviour
    {
        [Header("Victory")]
        [SerializeField]
        private string victoryScene = "Victory";

        private bool victoryActivated;

        private void OnTriggerEnter(Collider other)
        {
            if (victoryActivated)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            victoryActivated = true;

            SceneManager.LoadScene(victoryScene);
        }
    }
}