using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Creates the proper folder structure for Blazed Odyssey character sprites
/// Following the No-SPUM guidelines from the user
/// </summary>
public static class CreateCharacterFolders
{
    private static readonly string Root = "Assets/Resources/Characters";
    private static readonly string[] Paths = new[] {
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
        "Skeleton/Grave Knight/Female",
    };

    [MenuItem("BlazedOdyssey/Setup/Create Character Folders")]
    public static void CreateAll()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
            
        // Create Characters folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(Root))
            AssetDatabase.CreateFolder("Assets/Resources", "Characters");

        foreach (var p in Paths)
        {
            var full = Path.Combine(Root, p).Replace("\\", "/");
            CreatePath(full);
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Character folder tree created/refreshed under {Root}");
        Debug.Log($"📁 Created {Paths.Length} character folders");
    }

    private static void CreatePath(string fullPath)
    {
        string[] parts = fullPath.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
