using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WorldStarterMapGenerator
{
	private const int MapWidth = 200;
	private const int MapHeight = 200;

	[MenuItem("BlazedOdyssey/World/Generate Starter Map (200x200)")]
	public static void Generate()
	{
		// Ensure Grid and layers
		var gridGo = GameObject.Find("WorldGrid") ?? new GameObject("WorldGrid");
		var grid = gridGo.GetComponent<Grid>() ?? gridGo.AddComponent<Grid>();
		grid.cellSize = new Vector3(1, 1, 0); // 1 unit = 1 tile (16px)

		var ground = GetOrCreateLayer(gridGo.transform, "Ground");
		var roads = GetOrCreateLayer(gridGo.transform, "Roads");
		var plaza = GetOrCreateLayer(gridGo.transform, "Plaza");
		var forest = GetOrCreateLayer(gridGo.transform, "Forest");

		// Load tilesets (16 Wang tiles each)
		var tsGrassPath = LoadTileset("Grass_DirtPath");
		var tsGrassCobble = LoadTileset("Grass_Cobble");
		var tsGrassPlaza = LoadTileset("Grass_Plaza");
		var tsGrassForest = LoadTileset("Grass_ForestFloor");
		if (tsGrassPath == null || tsGrassCobble == null || tsGrassPlaza == null || tsGrassForest == null)
		{
			Debug.LogError("❌ Missing tilesets under Resources/Tiles. Run 'Import PixelLAB Tilesets' first.");
			return;
		}

		Undo.RegisterFullObjectHierarchyUndo(gridGo, "Generate Starter Map");

		// Base grass fill (use index 0 from any grass-* set)
		FillBaseGrass(ground, tsGrassPath);

		// Plaza mask (center square ~34x34)
		var plazaMask = NewVertexMask();
		RectInt plazaRect = new RectInt(MapWidth/2 - 20, MapHeight/2 - 20, 40, 40);
		MarkRect(plazaMask, plazaRect);
		PaintWang(plaza, tsGrassPlaza, plazaMask, skipLower: true);

		// Roads (cobblestone) — 4 arms from plaza to edges (width ~6)
		var roadMask = NewVertexMask();
		MarkRoad(roadMask, new Vector2Int(MapWidth/2, MapHeight/2 + plazaRect.height/2), Vector2.up, MapHeight/2 - 2, 6);
		MarkRoad(roadMask, new Vector2Int(MapWidth/2, MapHeight/2 - plazaRect.height/2), Vector2.down, MapHeight/2 - 2, 6);
		MarkRoad(roadMask, new Vector2Int(MapWidth/2 + plazaRect.width/2, MapHeight/2), Vector2.right, MapWidth/2 - 2, 6);
		MarkRoad(roadMask, new Vector2Int(MapWidth/2 - plazaRect.width/2, MapHeight/2), Vector2.left, MapWidth/2 - 2, 6);
		PaintWang(roads, tsGrassCobble, roadMask, skipLower: true);

		// Dirt paths (rings and diagonals), width ~3
		var pathMask = NewVertexMask();
		MarkCircle(pathMask, new Vector2(MapWidth/2f, MapHeight/2f), 35, 3);
		MarkCircle(pathMask, new Vector2(MapWidth/2f, MapHeight/2f), 75, 3);
		MarkRoad(pathMask, new Vector2Int(MapWidth/2, MapHeight/2), new Vector2(1,1).normalized, 180, 3);
		MarkRoad(pathMask, new Vector2Int(MapWidth/2, MapHeight/2), new Vector2(-1,1).normalized, 180, 3);
		PaintWang(roads, tsGrassPath, pathMask, skipLower: true);

		// Forest ring near the perimeter
		var forestMask = NewVertexMask();
		int inner = 95;
		for (int y = 0; y <= MapHeight; y++)
		{
			for (int x = 0; x <= MapWidth; x++)
			{
				float dx = x - MapWidth/2f;
				float dy = y - MapHeight/2f;
				float d = Mathf.Sqrt(dx*dx + dy*dy);
				if (d > inner) forestMask[x, y] = true;
			}
		}
		PaintWang(forest, tsGrassForest, forestMask, skipLower: true);

		Debug.Log("✅ Starter map generated: 200x200 with central plaza, roads, paths, and forest ring.");
	}

	private static Tilemap GetOrCreateLayer(Transform parent, string name)
	{
		var t = (parent.Find(name)?.GetComponent<Tilemap>()) ?? CreateLayer(parent, name);
		return t;
	}

	private static Tilemap CreateLayer(Transform parent, string name)
	{
		var go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.AddComponent<GridLayout>();
		var tm = go.AddComponent<Tilemap>();
		var r = go.AddComponent<TilemapRenderer>();
		r.sortOrder = TilemapRenderer.SortOrder.TopLeft;
		return tm;
	}

	private static Tile[] LoadTileset(string setName)
	{
		Tile[] tiles = new Tile[16];
		for (int i = 0; i < 16; i++)
		{
			var sprite = Resources.Load<Sprite>($"Tiles/{setName}/index_{i:D2}");
			if (sprite == null) return null;
			var tile = ScriptableObject.CreateInstance<Tile>();
			tile.sprite = sprite;
			tiles[i] = tile;
		}
		return tiles;
	}

	private static void FillBaseGrass(Tilemap ground, Tile[] grassTiles)
	{
		var grass = grassTiles[0];
		for (int y = 0; y < MapHeight; y++)
			for (int x = 0; x < MapWidth; x++)
				ground.SetTile(new Vector3Int(x, y, 0), grass);
	}

	private static bool[,] NewVertexMask()
	{
		return new bool[MapWidth + 1, MapHeight + 1];
	}

	private static void MarkRect(bool[,] mask, RectInt rect)
	{
		for (int y = rect.yMin; y <= rect.yMax; y++)
			for (int x = rect.xMin; x <= rect.xMax; x++)
				if (x >= 0 && x <= MapWidth && y >= 0 && y <= MapHeight)
					mask[x, y] = true;
	}

	private static void MarkRoad(bool[,] mask, Vector2Int start, Vector2 dir, int length, int halfWidth)
	{
		dir.Normalize();
		Vector2 p = start;
		for (int i = 0; i < length; i++)
		{
			int cx = Mathf.RoundToInt(p.x);
			int cy = Mathf.RoundToInt(p.y);
			for (int oy = -halfWidth; oy <= halfWidth; oy++)
			{
				for (int ox = -halfWidth; ox <= halfWidth; ox++)
				{
					int vx = cx + ox;
					int vy = cy + oy;
					if (vx >= 0 && vx <= MapWidth && vy >= 0 && vy <= MapHeight)
						mask[vx, vy] = true;
				}
			}
			p += dir * 1.0f; // dense to avoid holes
		}
	}

	private static void MarkCircle(bool[,] mask, Vector2 center, float radius, int thickness)
	{
		float rIn = Mathf.Max(0, radius - thickness);
		float rOut = radius + thickness;
		for (int y = 0; y <= MapHeight; y++)
		{
			for (int x = 0; x <= MapWidth; x++)
			{
				float dx = x - center.x;
				float dy = y - center.y;
				float d = Mathf.Sqrt(dx*dx + dy*dy);
				if (d >= rIn && d <= rOut)
					mask[x, y] = true;
			}
		}
	}

	private static void PaintWang(Tilemap tilemap, Tile[] tileset, bool[,] vertexUpper, bool skipLower)
	{
		for (int y = 0; y < MapHeight; y++)
		{
			for (int x = 0; x < MapWidth; x++)
			{
				bool NW = vertexUpper[x, y+1];
				bool NE = vertexUpper[x+1, y+1];
				bool SW = vertexUpper[x, y];
				bool SE = vertexUpper[x+1, y];
				int idx = (NW?8:0) + (NE?4:0) + (SW?2:0) + (SE?1:0);
				if (skipLower && idx == 0) continue; // let base grass show
				tilemap.SetTile(new Vector3Int(x, y, 0), tileset[idx]);
			}
		}
	}
}


