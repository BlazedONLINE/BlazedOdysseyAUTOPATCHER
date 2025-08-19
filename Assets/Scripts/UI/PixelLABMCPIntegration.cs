using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// PixelLAB MCP Integration for Unity
/// Uses MCP functions to generate AI characters and assets
/// </summary>
public class PixelLABMCPIntegration : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI statusText;
    public Image characterPreview;
    public TMP_InputField characterNameInput;
    public TMP_Dropdown characterClassDropdown;
    
    [Header("Character Generation Settings")]
    public int characterSize = 48;
    public string characterStyle = "pixel art";
    public string characterProportions = "default";
    
    [Header("Generated Characters")]
    public List<GeneratedCharacter> generatedCharacters = new List<GeneratedCharacter>();
    
    [System.Serializable]
    public class GeneratedCharacter
    {
        public string characterId;
        public string name;
        public string characterClass;
        public string imageUrl;
        public Sprite characterSprite;
        public System.DateTime generatedAt;
        
        // New fields for sprite persistence
        public string localSpritePath;
        public bool isSpriteLoaded;
        public string pixellabCharacterId; // The actual PixelLAB character ID
    }
    
    [Header("Sprite Management")]
    public string spriteSavePath = "GeneratedSprites";
    public bool autoSaveSprites = true;
    public bool autoLoadSprites = true;
    
    private void Start()
    {
        SetupCharacterClassDropdown();
        LoadAllSavedSprites();
    }
    
    private void SetupCharacterClassDropdown()
    {
        if (characterClassDropdown != null)
        {
            characterClassDropdown.ClearOptions();
            characterClassDropdown.AddOptions(new List<string>
            {
                "Warrior",
                "Mage", 
                "Archer",
                "Rogue",
                "Paladin",
                "Druid",
                "Monk",
                "Warlock"
            });
        }
    }
    
    /// <summary>
    /// Generate a character using PixelLAB MCP
    /// </summary>
    public void GenerateCharacter()
    {
        if (string.IsNullOrEmpty(characterNameInput.text))
        {
            ShowStatus("Please enter a character name!", Color.red);
            return;
        }
        
        string characterClass = characterClassDropdown.options[characterClassDropdown.value].text;
        StartCoroutine(GenerateCharacterCoroutine(characterNameInput.text, characterClass));
    }
    
    private IEnumerator GenerateCharacterCoroutine(string characterName, string characterClass)
    {
        ShowStatus("🎨 Generating character with AI...", Color.magenta);
        
        // Create character description based on class
        string description = GenerateCharacterDescription(characterName, characterClass);
        
        Debug.Log($"🎨 Generating {characterClass} character: {characterName}");
        Debug.Log($"📝 Description: {description}");
        
        // Note: In a real implementation, you would call the MCP function here
        // For now, we'll simulate the process and show how it would work
        
        yield return new WaitForSeconds(2f); // Simulate API call
        
        // Simulate successful generation
        string mockCharacterId = System.Guid.NewGuid().ToString();
        string mockImageUrl = $"https://pixellab.ai/characters/{mockCharacterId}.png";
        
        // Create generated character entry
        GeneratedCharacter newCharacter = new GeneratedCharacter
        {
            characterId = mockCharacterId,
            name = characterName,
            characterClass = characterClass,
            imageUrl = mockImageUrl,
            generatedAt = System.DateTime.Now
        };
        
        generatedCharacters.Add(newCharacter);
        
        ShowStatus($"✅ {characterName} the {characterClass} generated!", Color.green);
        
        // In real implementation, you would:
        // 1. Call mcp_pixellab_create_character with the description
        // 2. Get the character ID and image URLs
        // 3. Download and display the generated sprites
        
        Debug.Log($"🎯 MCP Function Call Example:");
        Debug.Log($"mcp_pixellab_create_character(");
        Debug.Log($"  description: \"{description}\",");
        Debug.Log($"  size: {characterSize},");
        Debug.Log($"  proportions: \"{characterProportions}\"");
        Debug.Log($")");
    }
    
    private string GenerateCharacterDescription(string name, string characterClass)
    {
        Dictionary<string, string> classDescriptions = new Dictionary<string, string>
        {
            {"Warrior", $"brave warrior {name} with heavy armor, sword and shield, battle-scarred, determined expression"},
            {"Mage", $"wise mage {name} with flowing robes, magical staff, glowing eyes, mystical aura"},
            {"Archer", $"skilled archer {name} with leather armor, longbow, quiver of arrows, focused gaze"},
            {"Rogue", $"stealthy rogue {name} with dark leather armor, daggers, hooded cloak, sharp eyes"},
            {"Paladin", $"noble paladin {name} with shining plate armor, holy sword, divine aura, righteous stance"},
            {"Druid", $"nature druid {name} with leaf-covered robes, wooden staff, animal companion, earthy presence"},
            {"Monk", $"disciplined monk {name} with simple robes, martial stance, focused energy, peaceful expression"},
            {"Warlock", $"dark warlock {name} with shadowy robes, demonic staff, glowing runes, mysterious aura"}
        };
        
        string baseDescription = classDescriptions.ContainsKey(characterClass) 
            ? classDescriptions[characterClass] 
            : $"adventurer {name} with {characterClass.ToLower()} equipment";
            
        return $"{baseDescription}, {characterStyle}, {characterSize}x{characterSize} pixels";
    }
    
    /// <summary>
    /// Animate an existing character
    /// </summary>
    public void AnimateCharacter(string characterId, string animationType)
    {
        Debug.Log($"🎬 Animating character {characterId} with {animationType}");
        
        // In real implementation, you would call:
        // mcp_pixellab_animate_character(characterId, animationType)
        
        ShowStatus($"🎬 Animating character...", Color.blue);
    }
    
    /// <summary>
    /// Create isometric tiles for the game world
    /// </summary>
    public void CreateWorldTiles()
    {
        Debug.Log("🏗️ Creating world tiles with PixelLAB MCP");
        
        // In real implementation, you would call:
        // mcp_pixellab_create_isometric_tile("grass terrain with flowers")
        // mcp_pixellab_create_isometric_tile("stone path with moss")
        // mcp_pixellab_create_isometric_tile("water tile with ripples")
        
        ShowStatus("🏗️ Creating world tiles...", Color.cyan);
    }
    
    /// <summary>
    /// Create terrain tilesets for seamless world generation
    /// </summary>
    public void CreateTerrainTilesets()
    {
        Debug.Log("🌍 Creating terrain tilesets with PixelLAB MCP");
        
        // In real implementation, you would call:
        // mcp_pixellab_create_tileset("ocean", "beach", 0.25, "wet sand with foam")
        // mcp_pixellab_create_tileset("beach", "grass", 0.5, "sandy grass transition")
        // mcp_pixellab_create_tileset("grass", "forest", 0.75, "dense tree coverage")
        
        ShowStatus("🌍 Creating terrain tilesets...", Color.cyan);
    }
    
    /// <summary>
    /// Generate a Dev Wizard character - ghost-like seasoned wizard
    /// </summary>
    public void GenerateDevWizard()
    {
        Debug.Log("🎭 Generating Dev Wizard - Ghost Seasoned Wizard...");
        
        string devWizardDescription = "seasoned ancient wizard with ghostly transparent appearance, flowing spectral robes, glowing ethereal eyes, wisps of magical energy, ethereal staff, translucent form, mystical aura, aged wise expression, pixel art style, 48x48 pixels";
        
        StartCoroutine(GenerateDevWizardCoroutine(devWizardDescription));
    }
    
    private IEnumerator GenerateDevWizardCoroutine(string description)
    {
        ShowStatus("🎭 Generating Dev Wizard with PixelLAB AI...", Color.cyan);
        
        Debug.Log($"🎭 Dev Wizard Description: {description}");
        
        // In real implementation, you would call:
        // mcp_pixellab_create_character(
        //   description: "seasoned ancient wizard with ghostly transparent appearance...",
        //   size: 48,
        //   proportions: "default",
        //   style: "pixel art"
        // )
        
        yield return new WaitForSeconds(2f); // Simulate API call
        
        // Create dev wizard entry
        GeneratedCharacter devWizard = new GeneratedCharacter
        {
            characterId = "DEV_WIZARD_001",
            name = "Dev_Wizard",
            characterClass = "Wizard",
            imageUrl = "https://pixellab.ai/dev_wizard_ghost.png",
            generatedAt = System.DateTime.Now
        };
        
        generatedCharacters.Add(devWizard);
        
        Debug.Log($"🎭 Dev Wizard created and added to list. Total characters: {generatedCharacters.Count}");
        Debug.Log($"🎭 Character ID: {devWizard.characterId}, Name: {devWizard.name}, Class: {devWizard.characterClass}");
        
        // Save the sprite locally if auto-save is enabled
        if (autoSaveSprites && devWizard.characterSprite != null)
        {
            SaveSpriteToFile(devWizard.characterSprite, devWizard.characterId, devWizard.name);
        }
        
        ShowStatus("🎭 Dev Wizard generated successfully!", Color.green);
        
        Debug.Log($"🎯 MCP Function Call for Dev Wizard:");
        Debug.Log($"mcp_pixellab_create_character(");
        Debug.Log($"  description: \"{description}\",");
        Debug.Log($"  size: {characterSize},");
        Debug.Log($"  proportions: \"{characterProportions}\"");
        Debug.Log($")");
    }
    
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        
        Debug.Log($"[PixelLAB MCP] {message}");
    }
    
    /// <summary>
    /// Clear all generated characters
    /// </summary>
    public void ClearGeneratedCharacters()
    {
        generatedCharacters.Clear();
        ShowStatus("🗑️ Cleared all generated characters", Color.yellow);
    }
    
    /// <summary>
    /// Display all generated characters in the character select screen
    /// </summary>
    public void DisplayGeneratedCharacters()
    {
        Debug.Log($"🎭 Displaying {generatedCharacters.Count} generated characters");
        
        if (generatedCharacters.Count == 0)
        {
            Debug.Log("🎭 No characters to display");
            return;
        }
        
        foreach (var character in generatedCharacters)
        {
            Debug.Log($"🎭 Character: {character.name} ({character.characterClass}) - ID: {character.characterId}");
            Debug.Log($"🎭 Has Sprite: {character.characterSprite != null}");
            Debug.Log($"🎭 Local Path: {character.localSpritePath}");
            Debug.Log($"🎭 Image URL: {character.imageUrl}");
        }
    }
    
    /// <summary>
    /// Get the count of generated characters
    /// </summary>
    public int GetCharacterCount()
    {
        return generatedCharacters.Count;
    }
    
    /// <summary>
    /// Get a specific character by ID
    /// </summary>
    public GeneratedCharacter GetCharacter(string characterId)
    {
        return generatedCharacters.Find(c => c.characterId == characterId);
    }
    
    /// <summary>
    /// Export character data for saving
    /// </summary>
    public string ExportCharacterData()
    {
        return JsonUtility.ToJson(generatedCharacters, true);
    }

    /// <summary>
    /// Save a sprite to local storage
    /// </summary>
    public void SaveSpriteToFile(Sprite sprite, string characterId, string characterName)
    {
        if (sprite == null) return;
        
        try
        {
            // Create directory if it doesn't exist
            string fullPath = Path.Combine(Application.persistentDataPath, spriteSavePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            // Generate filename
            string fileName = $"{characterId}_{characterName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(fullPath, fileName);
            
            // Convert sprite to texture and save as PNG
            Texture2D texture = sprite.texture;
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            
            Debug.Log($"💾 Sprite saved to: {filePath}");
            
            // Update character data
            var character = generatedCharacters.Find(c => c.characterId == characterId);
            if (character != null)
            {
                character.localSpritePath = filePath;
                character.isSpriteLoaded = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to save sprite: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load a sprite from local storage
    /// </summary>
    public Sprite LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        
        try
        {
            // Load PNG data
            byte[] pngData = File.ReadAllBytes(filePath);
            
            // Create texture
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(pngData);
            
            // Create sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            Debug.Log($"📁 Sprite loaded from: {filePath}");
            return sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to load sprite: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Check if a character's sprite exists locally
    /// </summary>
    public bool HasLocalSprite(string characterId)
    {
        var character = generatedCharacters.Find(c => c.characterId == characterId);
        return character != null && !string.IsNullOrEmpty(character.localSpritePath) && File.Exists(character.localSpritePath);
    }
    
    /// <summary>
    /// Load all saved sprites on startup
    /// </summary>
    public void LoadAllSavedSprites()
    {
        if (!autoLoadSprites) return;
        
        string fullPath = Path.Combine(Application.persistentDataPath, spriteSavePath);
        if (!Directory.Exists(fullPath)) return;
        
        Debug.Log("📁 Loading all saved sprites...");
        
        foreach (var character in generatedCharacters)
        {
            if (!string.IsNullOrEmpty(character.localSpritePath) && File.Exists(character.localSpritePath))
            {
                character.characterSprite = LoadSpriteFromFile(character.localSpritePath);
                character.isSpriteLoaded = true;
            }
        }
    }
}
