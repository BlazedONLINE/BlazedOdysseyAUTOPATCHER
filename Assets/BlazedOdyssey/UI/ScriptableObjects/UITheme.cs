using UnityEngine;
using TMPro;

namespace BlazedOdyssey.UI
{
    /// <summary>
    /// Centralized UI theme configuration for BlazedOdyssey MMO.
    /// Provides consistent pixel-art styling across all UI elements.
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "BlazedOdyssey/UI/Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Colors (Pixel-Art Palette)")]
        [Tooltip("Primary UI background color")]
        public Color primaryBackground = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        
        [Tooltip("Secondary panel background")]
        public Color secondaryBackground = new Color(0.2f, 0.25f, 0.35f, 0.9f);
        
        [Tooltip("Primary accent color for highlights")]
        public Color primaryAccent = new Color(0.3f, 0.7f, 1f, 1f);
        
        [Tooltip("Secondary accent color")]
        public Color secondaryAccent = new Color(1f, 0.8f, 0.2f, 1f);
        
        [Tooltip("Text color for primary content")]
        public Color primaryText = Color.white;
        
        [Tooltip("Text color for secondary content")]
        public Color secondaryText = new Color(0.8f, 0.8f, 0.9f, 1f);
        
        [Tooltip("Success/positive action color")]
        public Color successColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        
        [Tooltip("Warning color")]
        public Color warningColor = new Color(1f, 0.6f, 0.1f, 1f);
        
        [Tooltip("Error/danger color")]
        public Color errorColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        
        [Tooltip("Disabled element color")]
        public Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        [Header("Typography")]
        [Tooltip("Primary font for UI text")]
        public TMP_FontAsset primaryFont;
        
        [Tooltip("Secondary font for decorative text")]
        public TMP_FontAsset secondaryFont;
        
        [Tooltip("Base font size for normal text")]
        public float baseFontSize = 16f;
        
        [Tooltip("Large font size for headers")]
        public float largeFontSize = 24f;
        
        [Tooltip("Extra large font size for titles")]
        public float titleFontSize = 32f;

        [Header("Sprites & 9-Slice")]
        [Tooltip("9-slice sprite for panels")]
        public Sprite panelSprite;
        
        [Tooltip("9-slice sprite for buttons")]
        public Sprite buttonSprite;
        
        [Tooltip("Sprite for checkboxes")]
        public Sprite checkboxSprite;
        
        [Tooltip("Sprite for checkbox checkmark")]
        public Sprite checkmarkSprite;
        
        [Tooltip("Sprite for dropdown arrow")]
        public Sprite dropdownArrowSprite;
        
        [Tooltip("Icon for settings gear")]
        public Sprite settingsGearSprite;
        
        [Tooltip("Loading spinner sprite")]
        public Sprite loadingSpinnerSprite;

        [Header("Spacing & Layout")]
        [Tooltip("Standard padding for UI elements")]
        public float standardPadding = 8f;
        
        [Tooltip("Large padding for major sections")]
        public float largePadding = 16f;
        
        [Tooltip("Button height")]
        public float buttonHeight = 40f;
        
        [Tooltip("Input field height")]
        public float inputFieldHeight = 35f;
        
        [Tooltip("Standard UI element spacing")]
        public float elementSpacing = 10f;

        [Header("Animation")]
        [Tooltip("Standard UI animation duration")]
        public float animationDuration = 0.3f;
        
        [Tooltip("Fast animation duration")]
        public float fastAnimationDuration = 0.15f;
        
        [Tooltip("Loading spinner rotation speed")]
        public float spinnerSpeed = 360f;

        [Header("Button States")]
        [Tooltip("Button color multiplier for normal state")]
        public float buttonNormalMultiplier = 1f;
        
        [Tooltip("Button color multiplier for highlighted state")]
        public float buttonHighlightMultiplier = 1.1f;
        
        [Tooltip("Button color multiplier for pressed state")]
        public float buttonPressedMultiplier = 0.9f;
        
        [Tooltip("Button color multiplier for disabled state")]
        public float buttonDisabledMultiplier = 0.5f;

        public Color GetColorWithAlpha(Color baseColor, float alpha)
        {
            return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        public Color GetButtonColor(Color baseColor, ButtonState state)
        {
            float multiplier = state switch
            {
                ButtonState.Normal => buttonNormalMultiplier,
                ButtonState.Highlighted => buttonHighlightMultiplier,
                ButtonState.Pressed => buttonPressedMultiplier,
                ButtonState.Disabled => buttonDisabledMultiplier,
                _ => buttonNormalMultiplier
            };
            
            return baseColor * multiplier;
        }
    }

    public enum ButtonState
    {
        Normal,
        Highlighted,
        Pressed,
        Disabled
    }
}


