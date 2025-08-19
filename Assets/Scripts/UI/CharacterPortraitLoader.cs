using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads character portraits from the Resources/Characters folder structure
/// Following the No-SPUM guidelines
/// </summary>
public class CharacterPortraitLoader : MonoBehaviour
{
    [SerializeField] private Image portraitImage;

    private void Awake()
    {
        if (portraitImage == null)
        {
            portraitImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Sets the portrait for a character
    /// Example: SetPortrait("Human", "Vanguard Knight", true);
    /// </summary>
    /// <param name="race">Character race (Human, Devil, Skeleton)</param>
    /// <param name="className">Character class name</param>
    /// <param name="isMale">True for male, false for female</param>
    public void SetPortrait(string race, string className, bool isMale)
    {
        string gender = isMale ? "Male" : "Female";
        string baseFolder = $"Characters/{race}/{className}/{gender}";
        string idlePath = $"{baseFolder}/{className}_{race}_{gender}_idle_64x64";
        string sheetPath = $"{baseFolder}/{className}_{race}_{gender}_sheet_8dir_64x64";

        Debug.Log($"🎭 Loading portrait: {idlePath} (fallbacks enabled)");

        // 1) Try explicit idle file
        var sprite = Resources.Load<Sprite>(idlePath);
        // 2) Fallback: try the sheet if idle is missing
        if (sprite == null)
            sprite = Resources.Load<Sprite>(sheetPath);
        // 3) Fallback: load any sprite from the folder (e.g., south.png)
        if (sprite == null)
        {
            var anySprites = Resources.LoadAll<Sprite>(baseFolder);
            if (anySprites != null && anySprites.Length > 0)
            {
                // Prefer a sprite containing common hints
                Sprite pick = null;
                foreach (var s in anySprites)
                {
                    var n = s.name.ToLower();
                    if (n.Contains("idle") || n.Contains("south") || n.Contains("sheet"))
                    {
                        pick = s; break;
                    }
                }
                sprite = pick ?? anySprites[0];
            }
        }
        if (sprite != null)
        {
            portraitImage.sprite = sprite;
            Debug.Log($"✅ Portrait loaded from Resources: {sprite.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Portrait not found under Resources/{baseFolder}. Tried idle/sheet/any.");
            // Create a placeholder colored sprite
            CreatePlaceholderSprite(race, className);
        }
    }
    
    /// <summary>
    /// Creates a placeholder sprite when the actual sprite is not found
    /// </summary>
    private void CreatePlaceholderSprite(string race, string className)
    {
        if (portraitImage == null) return;
        
        // Create a simple colored placeholder
        Texture2D texture = new Texture2D(64, 64);
        Color placeholderColor = GetClassColor(className);
        
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = placeholderColor;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite placeholder = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        portraitImage.sprite = placeholder;
        
        Debug.Log($"🎨 Created placeholder for {race} {className}");
    }
    
    /// <summary>
    /// Gets a representative color for each class
    /// </summary>
    private Color GetClassColor(string className)
    {
        switch (className)
        {
            case "Vanguard Knight": return new Color(0.4f, 0.5f, 0.8f, 1f); // Blue
            case "Luminar Priest": return new Color(1f, 0.9f, 0.4f, 1f); // Gold
            case "Falcon Archer": return new Color(0.4f, 0.8f, 0.4f, 1f); // Green
            case "Shadowblade Rogue": return new Color(0.6f, 0.4f, 0.8f, 1f); // Purple
            case "Infernal Warlord": return new Color(0.8f, 0.2f, 0.2f, 1f); // Red
            case "Nightfang Stalker": return new Color(0.8f, 0.4f, 0.2f, 1f); // Orange
            case "Abyssal Oracle": return new Color(0.6f, 0.2f, 0.8f, 1f); // Dark Purple
            case "Bonecaster": return new Color(0.4f, 0.8f, 0.6f, 1f); // Green
            case "Grave Knight": return new Color(0.6f, 0.6f, 0.6f, 1f); // Gray
            default: return new Color(0.7f, 0.7f, 0.7f, 1f); // Default gray
        }
    }
}
