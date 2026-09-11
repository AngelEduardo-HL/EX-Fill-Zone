using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExFillZone.Gameplay.GameFlow
{
    public sealed class ExtractionZone : MonoBehaviour
    {
        [Header("Extraction")]
        [SerializeField, Min(0.1f)] private float extractionTime = 5f;
        [SerializeField] private string victoryScene = "Victory";

        [Header("UI")]
        [SerializeField] private TMP_Text extractionText;
        [SerializeField] private CanvasGroup extractionHUD;
        [SerializeField] private CanvasGroup screenFade;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float hudFadeDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float screenFadeDuration = 0.75f;

        private float remainingTime;
        private bool playerInside;
        private bool extracting;

        private void Start()
        {
            remainingTime = extractionTime;

            if (extractionHUD != null) extractionHUD.alpha = 0f;
            if (screenFade != null) screenFade.alpha = 0f;

            UpdateText();
        }

        private void Update()
        {
            if (!playerInside || extracting) return;

            remainingTime = Mathf.Max(remainingTime - Time.deltaTime, 0f);
            UpdateText();

            if (remainingTime <= 0f) CompleteExtraction();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || extracting) return;

            playerInside = true;
            remainingTime = extractionTime;

            if (extractionHUD != null)
            {
                extractionHUD.DOKill();
                extractionHUD.DOFade(1f, hudFadeDuration);
            }

            UpdateText();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") || extracting) return;

            playerInside = false;
            remainingTime = extractionTime;

            if (extractionHUD != null)
            {
                extractionHUD.DOKill();
                extractionHUD.DOFade(0f, hudFadeDuration);
            }

            UpdateText();
        }

        private void UpdateText()
        {
            if (extractionText == null) return;
            extractionText.text = $"{remainingTime:0.0}";
        }

        private void CompleteExtraction()
        {
            if (extracting) return;

            extracting = true;
            playerInside = false;

            if (extractionHUD != null)
            {
                extractionHUD.DOKill();
                extractionHUD.DOFade(0f, hudFadeDuration);
            }

            if (screenFade == null)
            {
                SceneManager.LoadScene(victoryScene);
                return;
            }

            screenFade.DOKill();
            screenFade.DOFade(1f, screenFadeDuration).SetEase(Ease.InOutQuad).OnComplete(() => SceneManager.LoadScene(victoryScene));
        }

        private void OnDisable()
        {
            if (extractionHUD != null) extractionHUD.DOKill();
            if (screenFade != null) screenFade.DOKill();
        }
    }
}