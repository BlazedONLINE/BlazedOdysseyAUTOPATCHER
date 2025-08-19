using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class PixelLabTilesetImporter
{
	private class TilesetRoot { public TilesetData tileset_data; }
	[Serializable] private class TilesetData { public List<TileEntry> tiles; }
	[Serializable] private class TileEntry { public string id; public ImageEntry image; }
	[Serializable] private class ImageEntry { public string base64; }

	private struct TilesetInfo
	{
		public string name;
		public string url;
		public TilesetInfo(string name, string url) { this.name = name; this.url = url; }
	}

	// Update these URLs if regenerated
	private static readonly TilesetInfo[] Tilesets = new TilesetInfo[]
	{
		new TilesetInfo("Grass_DirtPath", "https://api.pixellab.ai/mcp/tileset/496c9c55-ac8e-43cb-82e3-dcd45e6b1db7/download"),
		new TilesetInfo("Grass_Cobble", "https://api.pixellab.ai/mcp/tileset/51d745a9-a423-424d-8dee-7f44d43fc3ef/download"),
		new TilesetInfo("Ocean_Beach", "https://api.pixellab.ai/mcp/tileset/6400ba0c-7e63-46d5-ac22-210d47de2eca/download"),
		new TilesetInfo("Grass_ForestFloor", "https://api.pixellab.ai/mcp/tileset/9e1dbb8c-0129-4009-955e-a6eaafbb4f2f/download"),
		new TilesetInfo("Grass_Plaza", "https://api.pixellab.ai/mcp/tileset/966c0912-dd52-4036-ad35-e0c3bd1009cf/download"),
	};

	[MenuItem("BlazedOdyssey/World/Import PixelLAB Tilesets")] 
	public static void ImportAll()
	{
		try
		{
			string root = "Assets/Resources/Tiles";
			EnsureFolder("Assets/Resources");
			EnsureFolder(root);

			for (int i = 0; i < Tilesets.Length; i++)
			{
				var t = Tilesets[i];
				EditorUtility.DisplayProgressBar("Downloading Tilesets", t.name, (float)i / Tilesets.Length);
				string folder = Path.Combine(root, t.name).Replace("\\", "/");
				EnsureFolder(folder);
				DownloadAndWriteTiles(t.url, folder);
			}

			AssetDatabase.Refresh();
			// Apply import settings to sprites
			ApplySpriteSettingsRecursively("Assets/Resources/Tiles");
			EditorUtility.ClearProgressBar();
			Debug.Log("✅ PixelLAB tilesets imported to Assets/Resources/Tiles");
		}
		catch (Exception ex)
		{
			EditorUtility.ClearProgressBar();
			Debug.LogError($"❌ Tileset import failed: {ex}");
		}
	}

	private static void DownloadAndWriteTiles(string url, string folder)
	{
		using (UnityWebRequest req = UnityWebRequest.Get(url))
		{
			var op = req.SendWebRequest();
			while (!op.isDone) {}
			if (req.result != UnityWebRequest.Result.Success)
				throw new Exception($"Download failed: {url} -> {req.error}");

			var json = req.downloadHandler.text;
			var root = JsonUtility.FromJson<TilesetRoot>(json);
			if (root?.tileset_data?.tiles == null || root.tileset_data.tiles.Count == 0)
				throw new Exception("Invalid tileset JSON: no tiles");

			// Write 16 tiles as index_00..index_15
			for (int i = 0; i < root.tileset_data.tiles.Count; i++)
			{
				var entry = root.tileset_data.tiles[i];
				byte[] pngBytes = Convert.FromBase64String(entry.image.base64);
				string file = Path.Combine(folder, $"index_{i:D2}.png").Replace("\\", "/");
				File.WriteAllBytes(file, pngBytes);
			}
		}
	}

	private static void EnsureFolder(string path)
	{
		if (!AssetDatabase.IsValidFolder(path))
		{
			string parent = Path.GetDirectoryName(path).Replace("\\", "/");
			string leaf = Path.GetFileName(path);
			if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
			AssetDatabase.CreateFolder(parent, leaf);
		}
	}

	private static void ApplySpriteSettingsRecursively(string path)
	{
		string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[]{ path });
		foreach (var guid in guids)
		{
			string p = AssetDatabase.GUIDToAssetPath(guid);
			var importer = AssetImporter.GetAtPath(p) as TextureImporter;
			if (importer == null) continue;
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Single;
			importer.filterMode = FilterMode.Point;
			importer.alphaIsTransparency = true;
			importer.mipmapEnabled = false;
			importer.spritePixelsPerUnit = 16; // 16px per tile → 1 Unity unit per cell
			importer.SaveAndReimport();
		}
	}
}


