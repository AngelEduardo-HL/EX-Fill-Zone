using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    public sealed class BuildingFadeTarget : MonoBehaviour
    {
        [Header("Parts")]
        [SerializeField] private Renderer[] parts;

        [Header("Dither")]
        [SerializeField, Range(0f, 1f)] private float outsideFade = 0f;
        [SerializeField, Range(0f, 1f)] private float insideFade = 0.85f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 3f;

        private static readonly int FadeID = Shader.PropertyToID("_Fade");

        private MaterialPropertyBlock propertyBlock;
        private float currentFade;
        private float targetFade;
        private int activeUsers;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            currentFade = outsideFade;
            targetFade = outsideFade;
            SetFade(currentFade);
        }

        private void Update()
        {
            if (Mathf.Approximately(currentFade, targetFade)) return;

            currentFade = Mathf.MoveTowards(currentFade, targetFade, fadeSpeed * Time.deltaTime);
            SetFade(currentFade);
        }

        public void AddUser()
        {
            activeUsers++;
            targetFade = insideFade;
        }

        public void RemoveUser()
        {
            activeUsers = Mathf.Max(0, activeUsers - 1);
            targetFade = activeUsers > 0 ? insideFade : outsideFade;
        }

        private void SetFade(float fade)
        {
            foreach (Renderer part in parts)
            {
                if (part == null) continue;

                part.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(FadeID, fade);
                part.SetPropertyBlock(propertyBlock);
                propertyBlock.Clear();
            }
        }
    }
}