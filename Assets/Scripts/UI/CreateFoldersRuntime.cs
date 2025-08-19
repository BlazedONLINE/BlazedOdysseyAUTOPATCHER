using UnityEngine;
using System.IO;

/// <summary>
/// Runtime script to create character folders - just attach to any GameObject and it will run
/// </summary>
public class CreateFoldersRuntime : MonoBehaviour
{
    [Header("Auto-create folders on Start")]
    public bool createFoldersOnStart = true;
    
    private void Start()
    {
        if (createFoldersOnStart)
        {
            CreateCharacterFolders();
        }
    }
    
    [ContextMenu("Create Character Folders")]
    public void CreateCharacterFolders()
    {
        string basePath = Path.Combine(Application.dataPath, "Resources", "Characters");
        
        string[] folderPaths = new string[]
        {
            "Human/Vanguard Knight/Male",
            "Human/Vanguard Knight/Female",
            "Human/Luminar Priest/Male",
            "Human/Luminar Priest/Female",
            "Human/Falcon Archer/Male",
            "Human/Falcon Archer/Female",
            "Human/Shadowblade Rogue/Male",
            "Human/Shadowblade Rogue/Female",
            "Devil/Infernal Warlord/Male",
            "Devil/Infernal Warlord/Female",
            "Devil/Nightfang Stalker/Male",
            "Devil/Nightfang Stalker/Female",
            "Devil/Abyssal Oracle/Male",
            "Devil/Abyssal Oracle/Female",
            "Skeleton/Bonecaster/Male",
            "Skeleton/Bonecaster/Female",
            "Skeleton/Grave Knight/Male",
            "Skeleton/Grave Knight/Female"
        };
        
        Debug.Log("🏗️ Creating character folder structure...");
        
        foreach (string folderPath in folderPaths)
        {
            string fullPath = Path.Combine(basePath, folderPath);
            
            try
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"✅ Created: {folderPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to create {folderPath}: {e.Message}");
            }
        }
        
        Debug.Log($"🎉 Character folder structure created successfully!");
        Debug.Log($"📁 Base path: {basePath}");
        Debug.Log($"🔄 Refresh Unity's Project window to see the new folders");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("🔄 Unity Project refreshed automatically!");
#endif
    }
}
