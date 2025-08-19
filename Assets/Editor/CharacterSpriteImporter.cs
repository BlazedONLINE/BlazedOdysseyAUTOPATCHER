using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Automatically sets the correct import settings for all character sprites
/// Following the user's PixelLAB guidelines
/// </summary>
public class CharacterSpriteImporter : AssetPostprocessor
{
    // This runs automatically when any asset is imported
    void OnPreprocessTexture()
    {
        // Check if this is a character sprite in our Characters folder
        if (assetPath.Contains("Assets/Resources/Characters/"))
        {
            Debug.Log($"🎨 Setting pixel art import settings for: {assetPath}");
            
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            
            // Set the perfect settings for 64x64 pixel art
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePixelsPerUnit = 64f;
            textureImporter.filterMode = FilterMode.Point; // No filtering for crisp pixels
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            
            // Set max texture size to ensure quality
            textureImporter.maxTextureSize = 512;
            
            Debug.Log($"✅ Applied pixel art settings to: {Path.GetFileName(assetPath)}");
        }
    }
    
    /// <summary>
    /// Menu item to manually fix all existing character sprites
    /// </summary>
    [MenuItem("BlazedOdyssey/Setup/Fix All Character Sprite Settings")]
    public static void FixAllCharacterSprites()
    {
        string charactersPath = "Assets/Resources/Characters";
        
        if (!AssetDatabase.IsValidFolder(charactersPath))
        {
            Debug.LogError("❌ Characters folder not found! Please create it first.");
            return;
        }
        
        // Find all PNG files in the Characters folder
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { charactersPath });
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("⚠️ No sprites found in Characters folder!");
            return;
        }
        
        Debug.Log($"🔧 Found {guids.Length} sprites to fix...");
        
        int fixedCount = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Only process PNG files (our character sprites)
            if (assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                
                if (textureImporter != null)
                {
                    bool needsReimport = false;
                    
                    // Check and fix settings
                    if (textureImporter.textureType != TextureImporterType.Sprite)
                    {
                        textureImporter.textureType = TextureImporterType.Sprite;
                        needsReimport = true;
                    }
                    
                    if (textureImporter.spritePixelsPerUnit != 64f)
                    {
                        textureImporter.spritePixelsPerUnit = 64f;
                        needsReimport = true;
                    }
                    
                    if (textureImporter.filterMode != FilterMode.Point)
                    {
                        textureImporter.filterMode = FilterMode.Point;
                        needsReimport = true;
                    }
                    
                    if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                        needsReimport = true;
                    }
                    
                    if (!textureImporter.alphaIsTransparency)
                    {
                        textureImporter.alphaIsTransparency = true;
                        needsReimport = true;
                    }
                    
                    if (textureImporter.mipmapEnabled)
                    {
                        textureImporter.mipmapEnabled = false;
                        needsReimport = true;
                    }
                    
                    if (needsReimport)
                    {
                        AssetDatabase.ImportAsset(assetPath);
                        fixedCount++;
                        Debug.Log($"✅ Fixed: {Path.GetFileName(assetPath)}");
                    }
                }
            }
        }
        
        Debug.Log($"🎉 Fixed {fixedCount} character sprites!");
        Debug.Log($"📊 All sprites now have perfect 64x64 pixel art settings:");
        Debug.Log($"   • Texture Type: Sprite (2D and UI)");
        Debug.Log($"   • Pixels Per Unit: 64");
        Debug.Log($"   • Filter Mode: Point (no filter)");
        Debug.Log($"   • Compression: None");
        Debug.Log($"   • Alpha is Transparency: ON");
        Debug.Log($"   • Mipmaps: OFF");
    }
    
    /// <summary>
    /// Menu item to show current character sprite status
    /// </summary>
    [MenuItem("BlazedOdyssey/Setup/Check Character Sprite Status")]
    public static void CheckCharacterSpriteStatus()
    {
        string charactersPath = "Assets/Resources/Characters";
        
        if (!AssetDatabase.IsValidFolder(charactersPath))
        {
            Debug.LogError("❌ Characters folder not found!");
            return;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { charactersPath });
        
        Debug.Log($"📊 CHARACTER SPRITE STATUS REPORT");
        Debug.Log($"=================================");
        Debug.Log($"📁 Scanning: {charactersPath}");
        Debug.Log($"🖼️ Found {guids.Length} sprites");
        Debug.Log("");
        
        int correctSettings = 0;
        int needsFix = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            if (assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                
                if (textureImporter != null)
                {
                    bool isCorrect = textureImporter.textureType == TextureImporterType.Sprite &&
                                   textureImporter.spritePixelsPerUnit == 64f &&
                                   textureImporter.filterMode == FilterMode.Point &&
                                   textureImporter.textureCompression == TextureImporterCompression.Uncompressed &&
                                   textureImporter.alphaIsTransparency &&
                                   !textureImporter.mipmapEnabled;
                    
                    if (isCorrect)
                    {
                        Debug.Log($"✅ {Path.GetFileName(assetPath)} - Perfect settings!");
                        correctSettings++;
                    }
                    else
                    {
                        Debug.Log($"❌ {Path.GetFileName(assetPath)} - Needs fixing");
                        needsFix++;
                    }
                }
            }
        }
        
        Debug.Log("");
        Debug.Log($"📈 SUMMARY:");
        Debug.Log($"   ✅ Correct: {correctSettings}");
        Debug.Log($"   ❌ Need Fix: {needsFix}");
        
        if (needsFix > 0)
        {
            Debug.Log("");
            Debug.Log($"💡 Run 'BlazedOdyssey > Setup > Fix All Character Sprite Settings' to fix them!");
        }
    }
}
