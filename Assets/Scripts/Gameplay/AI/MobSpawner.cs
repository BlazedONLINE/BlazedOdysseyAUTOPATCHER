using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MobSpawner : MonoBehaviour
{
	[System.Serializable]
	public class MobEntry
	{
		public string mobId = "horse"; // maps to specific Mob_* behaviour
		public int maxAlive = 8;
		public float respawnSeconds = 120f;
	}

	public MobEntry entry = new MobEntry();

	private readonly HashSet<MobBase> alive = new HashSet<MobBase>();
	private readonly Queue<float> pendingRespawns = new Queue<float>();
	private Tilemap[] blockingTilemaps;
	private Rect spawnRect;

	private void Start()
	{
		// collect blocking tilemaps
		var tms = new List<Tilemap>();
		foreach (var tm in FindObjectsOfType<Tilemap>())
		{
			string n = tm.gameObject.name.ToLower();
			if (n.Contains("object") || n.Contains("above")) tms.Add(tm);
		}
		blockingTilemaps = tms.ToArray();

		// spawn area: BoxCollider2D on this object if present, else map quadrant
		var bc = GetComponent<BoxCollider2D>();
		if (bc != null)
		{
			Bounds b = bc.bounds; spawnRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
		}
		else if (MapBounds.Instance != null)
		{
			Rect r = MapBounds.Instance.worldRect; Vector2 c = r.center; spawnRect = new Rect(c.x, r.yMin, r.xMax - c.x, c.y - r.yMin);
		}
		else { spawnRect = new Rect(80, 0, 100, 100); }

		StartCoroutine(Warmup());
	}

	private IEnumerator Warmup()
	{
		yield return new WaitForEndOfFrame();
		for (int i = alive.Count; i < entry.maxAlive; i++) SpawnOne();
	}

	private void Update()
	{
		while (pendingRespawns.Count > 0 && Time.time >= pendingRespawns.Peek() && alive.Count < entry.maxAlive)
		{
			pendingRespawns.Dequeue(); SpawnOne();
		}
	}

	private void SpawnOne()
	{
		Vector3 pos = RandomValidPoint();
		MobBase mob = CreateMob(entry.mobId, pos);
		if (mob == null) return;
		mob.Initialize(spawnRect, blockingTilemaps);
		alive.Add(mob);
		// Safety: pin Y to front sorting layer
		mob.GetComponent<SpriteRenderer>().sortingOrder = 20;
	}

	private MobBase CreateMob(string id, Vector3 pos)
	{
		GameObject go = new GameObject("mob_" + id); go.transform.position = pos;
		switch (id.ToLower())
		{
			case "horse": return go.AddComponent<Mob_Horse>();
			default: return go.AddComponent<Mob_Horse>();
		}
	}

	private Vector3 RandomValidPoint()
	{
		Rect r = spawnRect;
		for (int i = 0; i < 64; i++)
		{
			var p = new Vector3(Random.Range(r.xMin + 0.5f, r.xMax - 0.5f), Random.Range(r.yMin + 0.5f, r.yMax - 0.5f), 0f);
			if (!IsBlocked(p)) return p;
		}
		return new Vector3(r.center.x, r.center.y, 0f);
	}

	private bool IsBlocked(Vector3 worldPos)
	{
		foreach (var tm in blockingTilemaps)
		{
			if (tm == null) continue;
			if (tm.HasTile(tm.WorldToCell(worldPos))) return true;
		}
		return false;
	}
}


