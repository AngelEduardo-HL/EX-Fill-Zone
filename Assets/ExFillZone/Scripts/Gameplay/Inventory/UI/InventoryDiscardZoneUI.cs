using UnityEngine;
using UnityEngine.UI;

namespace ExFillZone.Gameplay.Inventory
{
    [RequireComponent(typeof(Image), typeof(CanvasGroup))]
    public sealed class InventoryDiscardZoneUI : MonoBehaviour
    {
        private Image background;
        private CanvasGroup canvasGroup;
        private Texture2D gradientTexture;
        private Sprite gradientSprite;

        private void Awake()
        {
            background = GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();

            CreateGradient();

            background.raycastTarget = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            SetVisible(false);
        }

        private void CreateGradient()
        {
            const int height = 64;

            gradientTexture = new Texture2D(2, height, TextureFormat.RGBA32, false);
            gradientTexture.wrapMode = TextureWrapMode.Clamp;
            gradientTexture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                float strength = 1f - y / (height - 1f);
                float alpha = Mathf.Pow(strength, 1.5f) * 0.9f;

                Color color = new Color(0.75f, 0.06f, 0.08f, alpha);

                gradientTexture.SetPixel(0, y, color);
                gradientTexture.SetPixel(1, y, color);
            }

            gradientTexture.Apply();

            gradientSprite = Sprite.Create(
                gradientTexture,
                new Rect(0, 0, 2, height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            background.sprite = gradientSprite;
            background.color = Color.white;
            background.type = Image.Type.Simple;
        }

        public void SetVisible(bool visible)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = visible ? 1f : 0f;
        }

        public bool Contains(Vector2 screenPosition, Camera camera)
        {
            return isActiveAndEnabled && canvasGroup != null &&
                   canvasGroup.alpha > 0f &&
                   RectTransformUtility.RectangleContainsScreenPoint(
                       (RectTransform)transform, screenPosition, camera);
        }

        private void OnDestroy()
        {
            if (gradientSprite != null) Destroy(gradientSprite);
            if (gradientTexture != null) Destroy(gradientTexture);
        }
    }
}