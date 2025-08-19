using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSpriteManager : MonoBehaviour
{
    // Simple but effective character representation system
    // Can easily be upgraded to real sprites later
    
    public static void CreateCharacterDisplay(GameObject container, string className, bool isMale, Color classColor)
    {
        // Clear existing display
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Create character silhouette background
        GameObject silhouette = new GameObject("CharacterSilhouette", typeof(RectTransform));
        silhouette.transform.SetParent(container.transform, false);
        
        var silhouetteRect = silhouette.GetComponent<RectTransform>();
        silhouetteRect.anchorMin = Vector2.zero;
        silhouetteRect.anchorMax = Vector2.one;
        silhouetteRect.offsetMin = new Vector2(20, 20);
        silhouetteRect.offsetMax = new Vector2(-20, -20);
        
        var silhouetteImage = silhouette.AddComponent<Image>();
        silhouetteImage.color = new Color(classColor.r, classColor.g, classColor.b, 0.3f);
        
        // Create character figure (stylized representation)
        CreateCharacterFigure(silhouette, className, isMale, classColor);
        
        // Add class symbol overlay
        CreateClassSymbol(silhouette, className, classColor);
        
        // Add gender indicator
        CreateGenderIndicator(silhouette, isMale);
    }
    
    static void CreateCharacterFigure(GameObject parent, string className, bool isMale, Color classColor)
    {
        // Head
        GameObject head = CreateBodyPart(parent, "Head", new Vector2(0, 60), new Vector2(40, 40), classColor);
        
        // Body shape varies by class and gender
        Vector2 bodySize = GetBodySize(className, isMale);
        GameObject body = CreateBodyPart(parent, "Body", new Vector2(0, 10), bodySize, classColor);
        
        // Arms
        CreateBodyPart(parent, "LeftArm", new Vector2(-30, 20), new Vector2(15, 50), classColor);
        CreateBodyPart(parent, "RightArm", new Vector2(30, 20), new Vector2(15, 50), classColor);
        
        // Legs
        CreateBodyPart(parent, "LeftLeg", new Vector2(-15, -40), new Vector2(18, 60), classColor);
        CreateBodyPart(parent, "RightLeg", new Vector2(15, -40), new Vector2(18, 60), classColor);
        
        // Class-specific equipment
        CreateClassEquipment(parent, className, classColor);
    }
    
    static GameObject CreateBodyPart(GameObject parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject part = new GameObject(name, typeof(RectTransform));
        part.transform.SetParent(parent.transform, false);
        
        var partRect = part.GetComponent<RectTransform>();
        partRect.anchoredPosition = position;
        partRect.sizeDelta = size;
        
        var partImage = part.AddComponent<Image>();
        partImage.color = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 0.9f);
        
        return part;
    }
    
    static Vector2 GetBodySize(string className, bool isMale)
    {
        // Different body proportions for classes and genders
        float baseWidth = isMale ? 35f : 30f;
        float baseHeight = isMale ? 70f : 65f;
        
        switch (className)
        {
            case "Warrior":
                return new Vector2(baseWidth + 10f, baseHeight + 5f); // Bulkier
            case "Mage":
                return new Vector2(baseWidth - 5f, baseHeight + 10f); // Tall and lean
            case "Priest":
                return new Vector2(baseWidth, baseHeight + 8f); // Robed figure
            case "Archer":
                return new Vector2(baseWidth - 2f, baseHeight); // Athletic
            case "Thief":
                return new Vector2(baseWidth - 5f, baseHeight - 5f); // Compact and agile
            default:
                return new Vector2(baseWidth, baseHeight);
        }
    }
    
    static void CreateClassEquipment(GameObject parent, string className, Color classColor)
    {
        switch (className)
        {
            case "Warrior":
                // Shield on left arm
                CreateEquipment(parent, "Shield", new Vector2(-45, 20), new Vector2(25, 35), classColor, "🛡️");
                // Sword on right side
                CreateEquipment(parent, "Sword", new Vector2(45, 0), new Vector2(12, 60), classColor, "⚔️");
                break;
                
            case "Mage":
                // Staff
                CreateEquipment(parent, "Staff", new Vector2(40, 40), new Vector2(8, 80), classColor, "🔮");
                // Spell book
                CreateEquipment(parent, "Book", new Vector2(-35, 0), new Vector2(20, 25), classColor, "📖");
                break;
                
            case "Archer":
                // Bow
                CreateEquipment(parent, "Bow", new Vector2(-40, 30), new Vector2(15, 50), classColor, "🏹");
                // Quiver
                CreateEquipment(parent, "Quiver", new Vector2(35, -10), new Vector2(15, 40), classColor, "🎯");
                break;
                
            case "Thief":
                // Dual daggers
                CreateEquipment(parent, "Dagger1", new Vector2(-35, 10), new Vector2(8, 30), classColor, "🗡️");
                CreateEquipment(parent, "Dagger2", new Vector2(35, 10), new Vector2(8, 30), classColor, "🗡️");
                break;
                
            case "Priest":
                // Holy symbol
                CreateEquipment(parent, "HolySymbol", new Vector2(0, 40), new Vector2(20, 20), classColor, "✨");
                // Prayer beads
                CreateEquipment(parent, "Beads", new Vector2(-30, -10), new Vector2(15, 15), classColor, "📿");
                break;
        }
    }
    
    static void CreateEquipment(GameObject parent, string name, Vector2 position, Vector2 size, Color color, string symbol)
    {
        GameObject equipment = new GameObject(name, typeof(RectTransform));
        equipment.transform.SetParent(parent.transform, false);
        
        var equipRect = equipment.GetComponent<RectTransform>();
        equipRect.anchoredPosition = position;
        equipRect.sizeDelta = size;
        
        var equipImage = equipment.AddComponent<Image>();
        equipImage.color = new Color(1f, 1f, 1f, 0.8f);
        
        // Add symbol
        GameObject symbolObj = new GameObject("Symbol", typeof(RectTransform));
        symbolObj.transform.SetParent(equipment.transform, false);
        
        var symbolText = symbolObj.AddComponent<TextMeshProUGUI>();
        symbolText.text = symbol;
        symbolText.fontSize = Mathf.Min(size.x, size.y) * 0.6f;
        symbolText.alignment = TextAlignmentOptions.Center;
        symbolText.color = color;
        
        var symbolRect = symbolObj.GetComponent<RectTransform>();
        symbolRect.anchorMin = Vector2.zero;
        symbolRect.anchorMax = Vector2.one;
        symbolRect.offsetMin = Vector2.zero;
        symbolRect.offsetMax = Vector2.zero;
    }
    
    static void CreateClassSymbol(GameObject parent, string className, Color classColor)
    {
        GameObject symbol = new GameObject("ClassSymbol", typeof(RectTransform));
        symbol.transform.SetParent(parent.transform, false);
        
        var symbolRect = symbol.GetComponent<RectTransform>();
        symbolRect.anchoredPosition = new Vector2(60, 60);
        symbolRect.sizeDelta = new Vector2(30, 30);
        
        var symbolImage = symbol.AddComponent<Image>();
        symbolImage.color = new Color(0, 0, 0, 0.7f);
        
        // Class icon
        GameObject icon = new GameObject("Icon", typeof(RectTransform));
        icon.transform.SetParent(symbol.transform, false);
        
        var iconText = icon.AddComponent<TextMeshProUGUI>();
        iconText.text = GetClassSymbol(className);
        iconText.fontSize = 20;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = classColor;
        iconText.fontStyle = FontStyles.Bold;
        
        var iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
    }
    
    static void CreateGenderIndicator(GameObject parent, bool isMale)
    {
        GameObject indicator = new GameObject("GenderIndicator", typeof(RectTransform));
        indicator.transform.SetParent(parent.transform, false);
        
        var indicatorRect = indicator.GetComponent<RectTransform>();
        indicatorRect.anchoredPosition = new Vector2(-60, 60);
        indicatorRect.sizeDelta = new Vector2(25, 25);
        
        var indicatorImage = indicator.AddComponent<Image>();
        indicatorImage.color = isMale ? new Color(0.3f, 0.6f, 1f, 0.8f) : new Color(1f, 0.4f, 0.7f, 0.8f);
        
        // Gender symbol
        GameObject symbol = new GameObject("Symbol", typeof(RectTransform));
        symbol.transform.SetParent(indicator.transform, false);
        
        var symbolText = symbol.AddComponent<TextMeshProUGUI>();
        symbolText.text = isMale ? "♂" : "♀";
        symbolText.fontSize = 18;
        symbolText.alignment = TextAlignmentOptions.Center;
        symbolText.color = Color.white;
        symbolText.fontStyle = FontStyles.Bold;
        
        var symbolRect = symbol.GetComponent<RectTransform>();
        symbolRect.anchorMin = Vector2.zero;
        symbolRect.anchorMax = Vector2.one;
        symbolRect.offsetMin = Vector2.zero;
        symbolRect.offsetMax = Vector2.zero;
    }
    
    static string GetClassSymbol(string className)
    {
        switch (className)
        {
            case "Warrior": return "🛡️";
            case "Mage": return "🔮";
            case "Archer": return "🏹";
            case "Thief": return "🗡️";
            case "Priest": return "✨";
            default: return "⚔️";
        }
    }
}
