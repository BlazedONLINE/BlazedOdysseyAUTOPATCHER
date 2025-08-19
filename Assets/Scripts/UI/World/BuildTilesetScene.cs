using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BuildTilesetScene : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("BlazedOdyssey/World/Create Demo Tilemap Scene")] 
	public static void CreateScene()
	{
		// Root objects
		var gridGo = new GameObject("WorldGrid");
		var grid = gridGo.AddComponent<Grid>();
		grid.cellSize = new Vector3(1f, 1f, 0f);

		CreateLayer(gridGo.transform, "Ground");
		CreateLayer(gridGo.transform, "Roads");
		CreateLayer(gridGo.transform, "Details");

		Debug.Log("✅ Tilemap scene created. Use the Tile Palette to paint with imported tiles under Assets/Resources/Tiles/*");
	}

	private static void CreateLayer(Transform parent, string name)
	{
		var go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.AddComponent<Tilemap>();
		var r = go.AddComponent<TilemapRenderer>();
		r.sortOrder = TilemapRenderer.SortOrder.TopLeft;
	}
#endif
}


