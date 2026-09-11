using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExFillZone.Gameplay.Loot;

namespace ExFillZone.Gameplay.GameFlow
{
    public sealed class MatchTimer : MonoBehaviour
    {
        [Header("Timer")]
        [SerializeField, Min(1f)] private float matchDuration = 900f;
        [SerializeField] private string defeatScene = "Defeat";

        [Header("UI")]
        [SerializeField] private TMP_Text timerText;

        private float remainingTime;
        private bool finished;

        public float RemainingTime => remainingTime;

        private void Start()
        {
            RunLoot.Reset();

            remainingTime = matchDuration;
            UpdateText();
        }

        private void Update()
        {
            if (finished) return;

            remainingTime = Mathf.Max(remainingTime - Time.deltaTime, 0f);
            UpdateText();

            if (remainingTime <= 0f) TimeOut();
        }

        private void UpdateText()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void TimeOut()
        {
            if (finished) return;

            finished = true;
            SceneManager.LoadScene(defeatScene);
        }
    }
}