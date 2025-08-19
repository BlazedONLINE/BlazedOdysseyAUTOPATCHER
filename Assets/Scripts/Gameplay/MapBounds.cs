using UnityEngine;
using UnityEngine.Tilemaps;

public class MapBounds : MonoBehaviour
{
	public static MapBounds Instance { get; private set; }
	public Rect worldRect; // in world units
	[Header("Blocking Tilemaps (Object/Above)")]
	[SerializeField] private Tilemap[] blockingTilemaps;

	private void Awake()
	{
		Instance = this;
		ComputeFromChildTilemaps();
	}

	public void ComputeFromChildTilemaps()
	{
		var tilemaps = GetComponentsInChildren<Tilemap>();
		if (tilemaps.Length == 0)
		{
			worldRect = new Rect(0, 0, 200, 200); // fallback
			return;
		}
		bool first = true;
		Bounds merged = new Bounds();
		foreach (var tm in tilemaps)
		{
			var b = tm.cellBounds;
			// Convert cell bounds to world bounds (assumes 1 unit per cell)
			Vector3 min = tm.CellToWorld(new Vector3Int(b.xMin, b.yMin, 0));
			Vector3 max = tm.CellToWorld(new Vector3Int(b.xMax, b.yMax, 0));
			Bounds wb = new Bounds();
			wb.SetMinMax(min, max);
			if (first) { merged = wb; first = false; }
			else { merged.Encapsulate(wb); }
		}
		worldRect = new Rect(merged.min.x, merged.min.y, merged.size.x, merged.size.y);
	}

	public Tilemap[] GetBlockingTilemaps()
	{
		if (blockingTilemaps != null && blockingTilemaps.Length > 0) return blockingTilemaps;
		// Fallback discovery by typical names
		var tms = GetComponentsInChildren<Tilemap>();
		System.Collections.Generic.List<Tilemap> found = new System.Collections.Generic.List<Tilemap>();
		foreach (var tm in tms)
		{
			string n = tm.gameObject.name.ToLower();
			if (n.Contains("object") || n.Contains("above")) found.Add(tm);
		}
		return found.ToArray();
	}
}


