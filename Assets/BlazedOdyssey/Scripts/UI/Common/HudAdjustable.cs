using UnityEngine;

namespace BlazedOdyssey.UI.Common
{
    /// Lightweight tag for HUD mover/editor compatibility
    [DisallowMultipleComponent]
    public class HudAdjustable : MonoBehaviour
    {
        public string WidgetId = "Widget";

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}


