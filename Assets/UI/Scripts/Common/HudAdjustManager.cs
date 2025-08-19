// NOTE: Serialized field changes: Added fallbackFont field for TMP support
#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlazedOdyssey.UI
{
    public class HudAdjustManager : MonoBehaviour
    {
        public static HudAdjustManager? Instance { get; private set; }
        public static bool IsEditing => Instance != null && Instance._isEditing;

        [Header("Keys")]
        public KeyCode toggleKey = KeyCode.F;

        [Header("Overlay")]
        public RectTransform? overlayBanner;
        public Color bannerColor = new Color(0, 0, 0, 0.5f);
        
        [Header("Font Fallback")]
        [SerializeField] private TMP_FontAsset? fallbackFont;

        private bool _isEditing;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildBanner();
            SetEditing(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                SetEditing(!_isEditing);

            if (_isEditing && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))
                SaveAll();
        }

        private void BuildBanner()
        {
            if (overlayBanner != null) return;

            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            var bannerGO = new GameObject("HUD_Edit_Banner", typeof(RectTransform), typeof(Image));
            overlayBanner = bannerGO.GetComponent<RectTransform>();
            overlayBanner.SetParent(canvas.transform, false);
            overlayBanner.anchorMin = new Vector2(0.5f, 1f);
            overlayBanner.anchorMax = new Vector2(0.5f, 1f);
            overlayBanner.pivot = new Vector2(0.5f, 1f);
            overlayBanner.sizeDelta = new Vector2(720, 36);
            overlayBanner.anchoredPosition = new Vector2(0, -6);

            var img = bannerGO.GetComponent<Image>();
            img.color = bannerColor;

            var txtGO = new GameObject("Text", typeof(RectTransform));
            var rt = txtGO.GetComponent<RectTransform>();
            rt.SetParent(overlayBanner, false);
            rt.anchorMin = Vector2.zero; 
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;  
            rt.offsetMax = Vector2.zero;

            // Try to use TMP first, fallback to legacy Text
            if (fallbackFont != null)
            {
                var tmp = txtGO.AddComponent<TextMeshProUGUI>();
                tmp.font = fallbackFont;
                tmp.text = "HUD EDIT MODE — Drag panels. Right-click resets a panel. Ctrl+S saves. Press F to finish.";
                tmp.color = Color.white;
                tmp.fontSize = 16;
                tmp.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                var t = txtGO.AddComponent<Text>();
                
                // Try to get LegacyRuntime.ttf, fallback gracefully if not available
                try
                {
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[HudAdjustManager] Could not load LegacyRuntime.ttf: {ex.Message}. Text may not display properly.");
                    // Don't assign font, let Unity use default
                }
                
                t.text = "HUD EDIT MODE — Drag panels. Right-click resets a panel. Ctrl+S saves. Press F to finish.";
                t.color = Color.white;
                t.fontSize = 16;
                t.alignment = TextAnchor.MiddleCenter;
            }
        }

        public void SetEditing(bool editing)
        {
            _isEditing = editing;
            if (overlayBanner) overlayBanner.gameObject.SetActive(editing);

            // Note: DraggableHud.All is not available in this codebase, so this is commented out
            // If you have draggable HUD components, uncomment and adapt this:
            // foreach (var d in DraggableHud.All)
            //     if (d != null) d.SetEditVisuals(editing);
        }

        public void SaveAll()
        {
            // Note: HudLayoutPersistence is not available in this codebase, so this is commented out
            // If you have layout persistence, uncomment and adapt this:
            // foreach (var d in DraggableHud.All)
            //     if (d != null) HudLayoutPersistence.Save(d.elementId, d.GetComponent<RectTransform>());
            
            Debug.Log("[HudAdjustManager] Save functionality not implemented - use HudEditMode instead");
        }

        public void ResetAllToDefault()
        {
            // Note: DraggableHud.All is not available in this codebase, so this is commented out
            // If you have draggable HUD components, uncomment and adapt this:
            // foreach (var d in DraggableHud.All)
            //     if (d != null) d.ResetToDefault();
            
            Debug.Log("[HudAdjustManager] Reset functionality not implemented - use HudEditMode instead");
        }
    }
}