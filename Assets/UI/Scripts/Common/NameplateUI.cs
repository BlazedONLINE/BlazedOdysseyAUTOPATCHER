using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    /// <summary>
    /// Simple world-following nameplate that renders a UI label above a target Transform.
    /// Works with Screen Space - Overlay canvas.
    /// </summary>
    public class NameplateUI : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public Vector3 worldOffset = new Vector3(0, 1.2f, 0);
        public bool useRendererTop = true;
        public float headOffset = 0.55f;
        public bool autoFindPlayerByTag = true;
        public string playerTag = "Player";

        [Header("UI")] 
        public Canvas canvas;
        public RectTransform rectTransform;
        public Text label;
        public Outline outline;
        public int fontSize = 18;

        Camera _cam;

        void Awake()
        {
            rectTransform = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
        }

        public void Initialize(Transform targetTransform, string displayName, Canvas parentCanvas)
        {
            target = targetTransform;
            canvas = parentCanvas;
            if (label != null)
            {
                label.text = displayName;
                label.fontSize = fontSize;
                label.alignment = TextAnchor.LowerCenter;
            }
        }

        public void SetText(string displayName)
        {
            if (label != null) label.text = displayName;
        }

        void LateUpdate()
        {
            if (target == null && autoFindPlayerByTag)
            {
                var p = GameObject.FindGameObjectWithTag(playerTag);
                if (p != null) target = p.transform;
            }
            if (target == null || canvas == null || rectTransform == null) return;

            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) return;
            }

            Vector3 worldPos;
            if (useRendererTop)
            {
                var sr = target.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    var b = sr.bounds;
                    float centerX = b.center.x;
                    float topY = b.max.y;
                    worldPos = new Vector3(centerX, topY + headOffset, target.position.z);
                }
                else
                {
                    worldPos = target.position + worldOffset;
                }
            }
            else
            {
                worldPos = target.position + worldOffset;
            }
            Vector2 screenPoint = _cam.WorldToScreenPoint(worldPos);

            // Convert to canvas local point
            RectTransform canvasRT = canvas.transform as RectTransform;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPoint, null, out localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }
    }
}


