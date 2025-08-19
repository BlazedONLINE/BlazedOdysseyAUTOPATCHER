#nullable enable
using UnityEngine;

namespace BlazedOdyssey.UI
{
    /// Tag any RectTransform you want movable. Give it a unique Id.
    [RequireComponent(typeof(RectTransform))]
    public class HudMovable : MonoBehaviour
    {
        [Tooltip("Unique save key (e.g., HUD_TopBars, HUD_Hotbar)")]
        public string Id = "HUD_Element";
        [Tooltip("Optional: draw a thin outline in edit mode")]
        public bool HighlightInEdit = true;
        [Tooltip("If true, HUD editor will treat this element as resizable (via corner handles)")]
        public bool Resizable = true;

        public RectTransform Rect { get; private set; } = default!;
        
        void Awake() 
        { 
            Rect = GetComponent<RectTransform>(); 
            if (Rect == null)
            {
                Debug.LogError($"[HudMovable] {gameObject.name} is missing RectTransform component!");
            }
        }

        /// <summary>
        /// Ensure Rect is assigned - call this before using Rect in other scripts
        /// </summary>
        public void EnsureRect()
        {
            if (Rect == null)
            {
                Rect = GetComponent<RectTransform>();
            }
        }
    }
}