using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BarUI : MonoBehaviour
    {
        [Header("Wires")]
        public Image fill;
        public Text label;

        [Header("Appearance")]
        public Color highColor = new Color(0.20f, 0.85f, 0.30f, 1f);
        public Color lowColor = new Color(0.90f, 0.20f, 0.20f, 1f);

        private int _max = 100;
        private int _current = 100;

        public void Configure(int current, int max)
        {
            _max = Mathf.Max(1, max);
            _current = Mathf.Clamp(current, 0, _max);
            Refresh();
        }

        public void SetValue(int current)
        {
            _current = Mathf.Clamp(current, 0, _max);
            Refresh();
        }

        private void Refresh()
        {
            float t = Mathf.InverseLerp(0, _max, _current);
            if (fill)
            {
                fill.fillAmount = t;
                fill.color = Color.Lerp(lowColor, highColor, t);
            }
            if (label)
                label.text = $"{_current}/{_max}";
        }
    }
}
