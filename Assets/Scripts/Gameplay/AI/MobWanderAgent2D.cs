using UnityEngine;
using UnityEngine.Tilemaps;

public class MobWanderAgent2D
{
	private Rect leashRect;
	private Tilemap[] blockingTilemaps;
	private float moveSpeed;
	private float smoothLerp;
	private float idleMin;
	private float idleMax;

	private Vector2 targetPos;
	private float idleTimer;
	public Vector2 CurrentDirection { get; private set; }

	public void Initialize(Rect leash, Tilemap[] blocking, float speed, float smooth = 0.12f, float idleMinSeconds = 0.4f, float idleMaxSeconds = 1.2f)
	{
		leashRect = leash;
		blockingTilemaps = blocking;
		moveSpeed = speed;
		smoothLerp = Mathf.Clamp01(smooth);
		idleMin = Mathf.Max(0f, idleMinSeconds);
		idleMax = Mathf.Max(idleMin, idleMaxSeconds);
	}

	public bool Tick(float dt, Transform transform)
	{
		Vector2 pos = transform.position;
		// Handle idle pause at destinations
		if (idleTimer > 0f)
		{
			idleTimer -= dt;
			CurrentDirection = Vector2.zero;
			return false;
		}

		if (Vector2.Distance(pos, targetPos) < 0.08f || IsBlocked(targetPos))
		{
			PickNewTarget(pos);
			return false;
		}

		Vector2 desired = (targetPos - pos).normalized;
		CurrentDirection = Vector2.Lerp(CurrentDirection, desired, smoothLerp);
		if (CurrentDirection.sqrMagnitude < 0.0001f) CurrentDirection = desired;
		Vector2 next = pos + CurrentDirection * moveSpeed * dt;

		// Clamp to leash
		next.x = Mathf.Clamp(next.x, leashRect.xMin + 0.1f, leashRect.xMax - 0.1f);
		next.y = Mathf.Clamp(next.y, leashRect.yMin + 0.1f, leashRect.yMax - 0.1f);

		// If blocked, pick new target and idle briefly
		if (IsBlocked(next))
		{
			PickNewTarget(pos);
			return false;
		}

		transform.position = new Vector3(next.x, next.y, 0f);
		return true;
	}

	public void PickNewTarget(Vector2 from)
	{
		for (int i = 0; i < 30; i++)
		{
			float x = Random.Range(leashRect.xMin + 0.5f, leashRect.xMax - 0.5f);
			float y = Random.Range(leashRect.yMin + 0.5f, leashRect.yMax - 0.5f);
			Vector2 candidate = new Vector2(x, y);
			if (!IsBlocked(candidate))
			{
				targetPos = candidate;
				idleTimer = Random.Range(idleMin, idleMax);
				return;
			}
		}
		// fallback near center
		targetPos = leashRect.center;
		idleTimer = Random.Range(idleMin, idleMax);
	}

	private bool IsBlocked(Vector2 worldPos)
	{
		if (blockingTilemaps == null) return false;
		foreach (var tm in blockingTilemaps)
		{
			if (tm == null) continue;
			Vector3Int cell = tm.WorldToCell(worldPos);
			if (tm.HasTile(cell)) return true;
		}
		return false;
	}
}


