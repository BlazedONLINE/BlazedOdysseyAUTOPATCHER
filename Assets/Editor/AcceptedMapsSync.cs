using System.IO;
using UnityEditor;
using UnityEngine;

public static class AcceptedMapsSync
{
	private static readonly string kAssetsAccepted = "Assets/Maps/Accepted";

	[MenuItem("BlazedOdyssey/World/Sync Accepted Maps → Assets")] 
	public static void SyncAcceptedMaps()
	{
		string projectRoot = Directory.GetParent(Application.dataPath).FullName;
		string externalAccepted = Path.Combine(projectRoot, "Maps/Accepted");
		if (!Directory.Exists(externalAccepted))
		{
			EditorUtility.DisplayDialog("Accepted Maps", $"No folder found at:\n{externalAccepted}\n\nCreate it and place your .tmx/.tsx/.png there.", "OK");
			return;
		}

		// Ensure destination folder inside Assets
		if (!AssetDatabase.IsValidFolder("Assets/Maps")) AssetDatabase.CreateFolder("Assets", "Maps");
		if (!AssetDatabase.IsValidFolder(kAssetsAccepted)) AssetDatabase.CreateFolder("Assets/Maps", "Accepted");

		// Copy files (.tmx, .tsx, .png) preserving relative structure
		var files = Directory.GetFiles(externalAccepted, "*.*", SearchOption.AllDirectories);
		int copied = 0;
		foreach (var path in files)
		{
			string ext = Path.GetExtension(path).ToLowerInvariant();
			if (ext != ".tmx" && ext != ".tsx" && ext != ".png") continue;

			string rel = path.Substring(externalAccepted.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string dest = Path.Combine(kAssetsAccepted, rel).Replace('\\', '/');
			string destDir = Path.GetDirectoryName(dest);
			EnsureAssetsFolder(destDir);
			File.Copy(path, dest, true);
			copied++;
		}

		AssetDatabase.Refresh();
		ApplyTilesetTextureSettings();
		EditorUtility.DisplayDialog("Accepted Maps", $"Synced {copied} file(s) to {kAssetsAccepted}.", "OK");
	}

	[MenuItem("BlazedOdyssey/World/Open Accepted Source Folder")] 
	public static void OpenExternalAcceptedFolder()
	{
		string projectRoot = Directory.GetParent(Application.dataPath).FullName;
		string externalAccepted = Path.Combine(projectRoot, "Maps/Accepted");
		if (!Directory.Exists(externalAccepted)) Directory.CreateDirectory(externalAccepted);
		EditorUtility.RevealInFinder(externalAccepted);
	}

	[MenuItem("BlazedOdyssey/World/Open Accepted Assets Folder")] 
	public static void OpenAssetsAcceptedFolder()
	{
		if (!AssetDatabase.IsValidFolder(kAssetsAccepted))
		{
			if (!AssetDatabase.IsValidFolder("Assets/Maps")) AssetDatabase.CreateFolder("Assets", "Maps");
			AssetDatabase.CreateFolder("Assets/Maps", "Accepted");
		}
		EditorUtility.RevealInFinder(Path.Combine(Directory.GetParent(Application.dataPath).FullName, kAssetsAccepted));
	}

	private static void EnsureAssetsFolder(string assetsFolder)
	{
		if (string.IsNullOrEmpty(assetsFolder)) return;
		assetsFolder = assetsFolder.Replace('\\', '/');
		if (AssetDatabase.IsValidFolder(assetsFolder)) return;
		string[] parts = assetsFolder.Split('/');
		string current = parts[0];
		for (int i = 1; i < parts.Length; i++)
		{
			string next = current + "/" + parts[i];
			if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
			current = next;
		}
	}

	private static void ApplyTilesetTextureSettings()
	{
		// Ensure tiles look crisp and scaled correctly
		string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { kAssetsAccepted });
		foreach (var guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			var importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null) continue;
			bool changed = false;
			if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
			if (importer.spritePixelsPerUnit != 32f) { importer.spritePixelsPerUnit = 32f; changed = true; }
			if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
			if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
			if (changed)
			{
				importer.SaveAndReimport();
			}
		}
	}
}


