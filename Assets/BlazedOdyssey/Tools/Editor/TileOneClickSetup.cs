using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileOneClickSetup
{
    [MenuItem("BlazedOdyssey/Tiles/One-Click: Slice + Create Tiles (16px)")]
    public static void OneClick16() => Run(16, folderOnly: true);

    [MenuItem("BlazedOdyssey/Tiles/One-Click: Slice + Create Tiles (32px)")]
    public static void OneClick32() => Run(32, folderOnly: true);

    [MenuItem("BlazedOdyssey/Tiles/Selection-Only: Slice + Create Tiles (16px)")]
    public static void SelectionOnly16() => Run(16, folderOnly: false);

    [MenuItem("BlazedOdyssey/Tiles/Selection-Only: Slice + Create Tiles (32px)")]
    public static void SelectionOnly32() => Run(32, folderOnly: false);

    private static void Run(int tileSize, bool folderOnly)
    {
        string srcPath = GetSelectedFolder();
        if (string.IsNullOrEmpty(srcPath))
        {
            EditorUtility.DisplayDialog("Tile Setup", "Select a folder under Assets that contains your tilesheets (PNG).", "OK");
            return;
        }

        string dstPath = srcPath.TrimEnd('/') + "/Tiles";
        if (!AssetDatabase.IsValidFolder(dstPath))
        {
            string parent = Path.GetDirectoryName(srcPath).Replace("\\", "/");
            string newFolderName = Path.GetFileName(dstPath);
            AssetDatabase.CreateFolder(parent, newFolderName);
        }

        // Collect targets
        string[] pngGuids;
        if (folderOnly)
        {
            pngGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { srcPath })
                .Where(g => AssetDatabase.GUIDToAssetPath(g).EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        else
        {
            var selected = Selection.objects
                .Select(o => AssetDatabase.GetAssetPath(o))
                .Where(p => p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Tile Setup", "No PNGs selected. Select one or more tilesheets and run Selection-Only again.", "OK");
                return;
            }
            pngGuids = selected.Select(p => AssetDatabase.AssetPathToGUID(p)).ToArray();
        }

        int total = pngGuids.Length;
        int totalSprites = 0, totalTiles = 0;

        try
        {
            for (int i = 0; i < total; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Tile Setup", $"Processing {i + 1}/{total}", (float)(i + 1) / total))
                {
                    break; // allow cancel
                }

                string guid = pngGuids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;

                // Configure importer and slice grid
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = tileSize;

                // Load texture to get dimensions
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                if (tex == null) continue;

                int w = tex.width, h = tex.height;
                var metas = new List<SpriteMetaData>();
                int idx = 0;
                for (int y = h - tileSize; y >= 0; y -= tileSize)
                {
                    for (int x = 0; x <= w - tileSize; x += tileSize)
                    {
                        var meta = new SpriteMetaData
                        {
                            alignment = (int)SpriteAlignment.Center,
                            border = Vector4.zero,
                            name = $"{Path.GetFileNameWithoutExtension(path)}_{idx:D3}",
                            pivot = new Vector2(0.5f, 0.5f),
                            rect = new Rect(x, y, tileSize, tileSize)
                        };
                        metas.Add(meta);
                        idx++;
                    }
                }
                importer.spritesheet = metas.ToArray();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                totalSprites += metas.Count;

                // Create Tile assets for each sprite
                var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToArray();
                foreach (var sprite in sprites)
                {
                    string safeName = MakeSafe(sprite.name);
                    string tilePath = $"{dstPath}/{safeName}.asset";
                    if (File.Exists(tilePath)) continue;

                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.None;
                    AssetDatabase.CreateAsset(tile, tilePath);
                    totalTiles++;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Tile Setup", $"Done! Sliced {totalSprites} sprites and created {totalTiles} tiles in\n{dstPath}", "OK");
        EditorUtility.RevealInFinder(dstPath);
    }

    private static string GetSelectedFolder()
    {
        var obj = Selection.activeObject;
        if (obj == null) return null;
        string path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) return null;
        if (AssetDatabase.IsValidFolder(path)) return path.Replace("\\", "/");
        return Path.GetDirectoryName(path).Replace("\\", "/");
    }

    private static string MakeSafe(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name;
    }
}


