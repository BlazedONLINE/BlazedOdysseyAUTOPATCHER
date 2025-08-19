using TMPro;
using UnityEngine;
using BlazedOdyssey.UI;

namespace BlazedOdyssey.UI.Common
{
    public static class UIThemeUtil
    {
        public static UITheme LoadTheme()
        {
            return Resources.Load<UITheme>("UITheme");
        }

        public static void ApplyFontIfAvailable(TextMeshProUGUI text, bool isTitle = false)
        {
            if (text == null) return;
            var theme = LoadTheme();
            if (theme == null) return;
            if (theme.primaryFont != null)
            {
                text.font = theme.primaryFont;
            }
            if (isTitle && theme.titleFontSize > 0f)
            {
                text.fontSize = theme.titleFontSize;
            }
            else if (!isTitle && theme.baseFontSize > 0f)
            {
                // Keep current font size if already set explicitly by builder; otherwise use theme
                if (Mathf.Approximately(text.fontSize, 0f))
                    text.fontSize = theme.baseFontSize;
            }
            if (theme.primaryText.a > 0f)
            {
                text.color = theme.primaryText;
            }
        }
    }
}


