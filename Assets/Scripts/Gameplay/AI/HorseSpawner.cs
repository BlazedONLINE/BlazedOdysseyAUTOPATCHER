using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HorseSpawner : MonoBehaviour
{
	public int maxHorses = 15;
	public float respawnSeconds = 120f;
	public string monsterResourceFolder = "Monsters";
	public float minSeparation = 1.8f; // minimum distance between spawned horses
	[Header("Horse Settings")]
	public float horseScale = 1.0f; // original size
	public int experiencePerKill = 5;

	private readonly HashSet<HorseMob> alive = new HashSet<HorseMob>();
	private readonly Queue<float> pendingRespawns = new Queue<float>();
	private Tilemap[] blockingTilemaps;
	private Rect spawnRect; // bottom-right pasture area
	private bool _initialized;
	private bool _startedWarmup;

	private void OnEnable()
	{
		InitializeAndStart();
	}

	private void Start()
	{
		InitializeAndStart();
	}

	private void InitializeAndStart()
	{
		if (_initialized && _startedWarmup) return;
		Debug.Log("[HorseSpawner] Start()");
		// Gather blocking tilemaps from names commonly used: "Objects", "Above"
		var tms = new List<Tilemap>();
		foreach (var tm in FindObjectsOfType<Tilemap>())
		{
			string n = tm.gameObject.name.ToLower();
			if (n.Contains("object") || n.Contains("above")) tms.Add(tm);
		}
		blockingTilemaps = tms.ToArray();

		// Prefer BoxCollider2D on this object to define spawn/leash area
		var bc = GetComponent<BoxCollider2D>();
		if (bc != null)
		{
			Bounds b = bc.bounds;
			spawnRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
			Debug.Log($"[HorseSpawner] Using BoxCollider2D rect {spawnRect}");
		}
		else if (MapBounds.Instance != null)
		{
			// Bottom-right quadrant by default
			Rect r = MapBounds.Instance.worldRect;
			Vector2 c = r.center;
			spawnRect = new Rect(c.x, r.yMin, r.xMax - c.x, c.y - r.yMin);
			Debug.Log($"[HorseSpawner] LeashRect set {spawnRect}");
		}
		else
		{
			spawnRect = new Rect(0, 0, 10, 10); // minimal safe fallback near origin
			Debug.Log("[HorseSpawner] Using fallback leashRect");
		}

		// Delay initial fill slightly to ensure MapBounds and tilemaps are fully ready
		if (!_startedWarmup)
		{
			_startedWarmup = true;
			StartCoroutine(SpawnWarmup());
		}
		_initialized = true;
	}

	private System.Collections.IEnumerator SpawnWarmup()
	{
		yield return new WaitForEndOfFrame();
		for (int i = alive.Count; i < maxHorses; i++) SpawnOne();
		Debug.Log($"[HorseSpawner] Spawned initial {alive.Count} horses");
	}

	private void Update()
	{
		// Process pending respawns while respecting the max cap
		while (pendingRespawns.Count > 0 && Time.time >= pendingRespawns.Peek() && alive.Count < maxHorses)
		{
			pendingRespawns.Dequeue();
			SpawnOne();
		}
	}

	private void SpawnOne()
	{
		if (MapBounds.Instance == null) return;
		if (alive.Count >= maxHorses) return;

		var go = new GameObject("Horse");
		go.transform.position = RandomValidPointSeparated();
		go.transform.localScale = Vector3.one * Mathf.Max(0.1f, horseScale);
		var mob = go.AddComponent<HorseMob>();
		mob.Initialize(this, blockingTilemaps, spawnRect);
		mob.experienceReward = Mathf.Max(0, experiencePerKill);
		alive.Add(mob);
		Debug.Log($"[HorseSpawner] Spawned Horse at {go.transform.position}");
	}

	private Vector3 RandomValidPointSeparated()
	{
		Rect r = spawnRect.width > 0.01f ? spawnRect : MapBounds.Instance.worldRect;
		for (int tries = 0; tries < 120; tries++)
		{
			var p = new Vector3(
				Random.Range(r.xMin + 0.5f, r.xMax - 0.5f),
				Random.Range(r.yMin + 0.5f, r.yMax - 0.5f),
				0f
			);
			if (IsBlocked(p)) continue;
			bool farEnough = true;
			foreach (var h in alive)
			{
				if (h == null) continue;
				if (Vector2.Distance(h.transform.position, p) < minSeparation) { farEnough = false; break; }
			}
			if (farEnough) return p;
		}
		// Fallback
		return new Vector3(r.center.x, r.center.y, 0f);
	}

	private Vector3 RandomValidPoint()
	{
		Rect r = spawnRect.width > 0.01f ? spawnRect : MapBounds.Instance.worldRect;
		for (int i = 0; i < 100; i++)
		{
			var p = new Vector3(
				Random.Range(r.xMin + 0.5f, r.xMax - 0.5f),
				Random.Range(r.yMin + 0.5f, r.yMax - 0.5f),
				0f
			);
			if (!IsBlocked(p)) return p;
		}
		// fallback to center
		return new Vector3(r.center.x, r.center.y, 0f);
	}

	public void NotifyMobDied(HorseMob mob)
	{
		alive.Remove(mob);
		pendingRespawns.Enqueue(Time.time + respawnSeconds);
	}

	private bool IsBlocked(Vector3 worldPos)
	{
		foreach (var tm in blockingTilemaps)
		{
			if (tm == null) continue;
			Vector3Int cell = tm.WorldToCell(worldPos);
			if (tm.HasTile(cell)) return true;
		}
		return false;
	}

	public Vector2 RequestValidPointInsideSpawn()
	{
		Rect r = spawnRect.width > 0.01f ? spawnRect : MapBounds.Instance.worldRect;
		for (int i = 0; i < 50; i++)
		{
			var p = new Vector2(
				Random.Range(r.xMin + 0.5f, r.xMax - 0.5f),
				Random.Range(r.yMin + 0.5f, r.yMax - 0.5f)
			);
			if (!IsBlocked(p)) return p;
		}
		return r.center;
	}
}


