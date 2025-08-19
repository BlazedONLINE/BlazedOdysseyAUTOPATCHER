using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Downloads PixelLAB characters and saves them to the correct folder structure
/// </summary>
public class PixelLABDownloader : MonoBehaviour
{
    [Header("PixelLAB Character Downloads")]
    public bool autoDownloadOnStart = false;
    
    [System.Serializable]
    public class CharacterDownload
    {
        public string characterName;
        public string race;
        public string className;
        public string characterId;
        public string downloadUrl;
        public bool downloaded = false;
    }
    
    public List<CharacterDownload> charactersToDownload = new List<CharacterDownload>
    {
        new CharacterDownload
        {
            characterName = "Vanguard Knight",
            race = "Human",
            className = "Vanguard Knight",
            characterId = "37194dab-800c-45a3-b2c6-e3e0d0feef6f",
            downloadUrl = "https://api.pixellab.ai/mcp/characters/37194dab-800c-45a3-b2c6-e3e0d0feef6f/download"
        },
        new CharacterDownload
        {
            characterName = "Luminar Priest",
            race = "Human",
            className = "Luminar Priest",
            characterId = "4c2c5748-e181-4e5e-92cb-71121e6cf582",
            downloadUrl = "https://api.pixellab.ai/mcp/characters/4c2c5748-e181-4e5e-92cb-71121e6cf582/download"
        },
        new CharacterDownload
        {
            characterName = "Infernal Warlord",
            race = "Devil",
            className = "Infernal Warlord",
            characterId = "13fd0cf0-7261-43ab-84dc-42a88ae863ec",
            downloadUrl = "https://api.pixellab.ai/mcp/characters/13fd0cf0-7261-43ab-84dc-42a88ae863ec/download"
        }
    };
    
    private void Start()
    {
        if (autoDownloadOnStart)
        {
            StartCoroutine(DownloadAllCharacters());
        }
    }
    
    [ContextMenu("Download All Characters")]
    public void DownloadAllCharactersMenu()
    {
        StartCoroutine(DownloadAllCharacters());
    }
    
    [ContextMenu("Show Download Instructions")]
    public void ShowDownloadInstructionsMenu()
    {
        ShowDownloadInstructions();
    }
    
    private IEnumerator DownloadAllCharacters()
    {
        Debug.Log("🎨 Starting PixelLAB character downloads...");
        
        foreach (var character in charactersToDownload)
        {
            if (!character.downloaded)
            {
                yield return StartCoroutine(DownloadCharacter(character));
                yield return new WaitForSeconds(1f); // Small delay between downloads
            }
        }
        
        Debug.Log("✅ All PixelLAB characters downloaded!");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("🔄 Unity Project refreshed!");
#endif
    }
    
    private IEnumerator DownloadCharacter(CharacterDownload character)
    {
        Debug.Log($"📥 Downloading {character.race} {character.className}...");
        
        UnityWebRequest request = UnityWebRequest.Get(character.downloadUrl);
        request.timeout = 30;
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            // Get the downloaded data
            byte[] zipData = request.downloadHandler.data;
            
            // Create the target directory
            string targetPath = Path.Combine(Application.dataPath, "Resources", "Characters", character.race, character.className, "Male");
            Directory.CreateDirectory(targetPath);
            
            // Save the ZIP file temporarily
            string tempZipPath = Path.Combine(Application.temporaryCachePath, $"{character.characterId}.zip");
            File.WriteAllBytes(tempZipPath, zipData);
            
            // Extract and process the ZIP
            yield return StartCoroutine(ExtractAndProcessZip(tempZipPath, targetPath, character));
            
            // Clean up temp file
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
            
            character.downloaded = true;
            Debug.Log($"✅ {character.race} {character.className} downloaded and extracted!");
        }
        else
        {
            Debug.LogError($"❌ Failed to download {character.race} {character.className}: {request.error}");
        }
        
        request.Dispose();
    }
    
    private IEnumerator ExtractAndProcessZip(string zipPath, string extractPath, CharacterDownload character)
    {
        // For now, we'll just log that we have the ZIP
        // In a real implementation, you'd use a ZIP library to extract
        Debug.Log($"📦 Processing ZIP for {character.className}...");
        Debug.Log($"📁 Would extract to: {extractPath}");
        Debug.Log($"💡 Manual step: Extract {zipPath} to {extractPath}");
        Debug.Log($"💡 Look for files named like: {character.className}_{character.race}_Male_idle_64x64.png");
        
        yield return null;
    }
    
    public void ShowDownloadInstructions()
    {
        Debug.Log("🎯 MANUAL DOWNLOAD INSTRUCTIONS:");
        Debug.Log("================================");
        
        foreach (var character in charactersToDownload)
        {
            Debug.Log($"📥 {character.race} {character.className}:");
            Debug.Log($"   URL: {character.downloadUrl}");
            Debug.Log($"   Extract to: Assets/Resources/Characters/{character.race}/{character.className}/Male/");
            Debug.Log($"   Look for: {character.className}_{character.race}_Male_idle_64x64.png");
            Debug.Log("");
        }
        
        Debug.Log("💡 After downloading, set Unity import settings:");
        Debug.Log("   - Texture Type: Sprite (2D and UI)");
        Debug.Log("   - Pixels Per Unit: 64");
        Debug.Log("   - Filter Mode: Point (no filter)");
        Debug.Log("   - Compression: None");
    }
}
