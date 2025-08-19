using UnityEngine;

namespace BlazedOdyssey.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SettingsPanelScaler : MonoBehaviour
    {
        [Tooltip("Target reference resolution used for designing the panel")] public Vector2 referenceResolution = new Vector2(1920, 1080);
        [Tooltip("Minimum scale multiplier")] public float minScale = 0.75f;
        [Tooltip("Maximum scale multiplier")] public float maxScale = 1.25f;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            ApplyScale();
        }

        private void OnEnable()
        {
            ApplyScale();
        }

        private void Update()
        {
            // Adjust in play mode as the window size changes
            ApplyScale();
        }

        private void ApplyScale()
        {
            if (_rect == null) return;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var size = canvas.pixelRect.size;
            if (referenceResolution.x <= 0 || referenceResolution.y <= 0) return;
            float scaleX = size.x / referenceResolution.x;
            float scaleY = size.y / referenceResolution.y;
            float scale = Mathf.Clamp(Mathf.Min(scaleX, scaleY), minScale, maxScale);
            _rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}


