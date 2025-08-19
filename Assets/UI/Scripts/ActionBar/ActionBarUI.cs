// NOTE: Serialized field changes: Added fallbackFont field for TMP support
#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlazedOdyssey.UI
{
    public class ActionBarUI : MonoBehaviour
    {
        public RectTransform? root;
        public InventorySlotUI? slotTemplate;
        
        [Header("Font Fallback")]
        [SerializeField] private TMP_FontAsset? fallbackFont;
        [Header("Key Labels")]
        [SerializeField] private bool showKeyLabels = false; // revert: off by default
        
        private InventorySlotUI[]? _slots;

        private KeyCode[] _keys = new KeyCode[11]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
            KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.M // Mount
        };

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (Input.GetKeyDown(_keys[i]))
                {
                    Debug.Log($"Pressed action slot {i+1}");
                }
            }
        }

        private void Build()
        {
            if (!root || !slotTemplate) return;
            slotTemplate.gameObject.SetActive(false);
            _slots = new InventorySlotUI[11];

            var layout = root.GetComponent<HorizontalLayoutGroup>();
            if (!layout) layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            for (int i = 0; i < 11; i++)
            {
                var s = GameObject.Instantiate(slotTemplate, root);
                s.gameObject.SetActive(true);
                s.index = i;

                if (showKeyLabels)
                {
                    // Optional small number label inside slot (disabled by default)
                    var labelGO = new GameObject("KeyLabel", typeof(RectTransform));
                    var r = labelGO.GetComponent<RectTransform>();
                    r.SetParent(s.transform, false);
                    r.anchorMin = new Vector2(1f, 1f);
                    r.anchorMax = new Vector2(1f, 1f);
                    r.pivot = new Vector2(1f, 1f);
                    r.sizeDelta = new Vector2(18, 14);
                    r.anchoredPosition = new Vector2(-2, -2);

                    var keyText = (i < 10) ? ((i+1) % 10).ToString() : "M";
                    if (fallbackFont != null)
                    {
                        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
                        tmp.font = fallbackFont;
                        tmp.fontSize = 12;
                        tmp.color = new Color(1,1,1,0.9f);
                        tmp.text = keyText;
                        tmp.alignment = TextAlignmentOptions.Midline;
                    }
                    else
                    {
                        var txt = labelGO.AddComponent<Text>();
                        txt.alignment = TextAnchor.MiddleCenter;
                        try { txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
                        txt.fontSize = 12;
                        txt.color = new Color(1,1,1,0.9f);
                        txt.text = keyText;
                    }
                }

                _slots[i] = s;
            }
        }
    }
}