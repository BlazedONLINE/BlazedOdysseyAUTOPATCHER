using UnityEngine;

public class DevHorseSimpleAgent : MonoBehaviour
{
	public float moveSpeed = 0.35f;
	public float idleMin = 0.5f;
	public float idleMax = 1.5f;
	public Rect leashRect;
	public Sprite[] idleFrames = new Sprite[0];
	public Sprite[] walkFrames = new Sprite[0];

	private SpriteRenderer sr;
	private Vector2 target;
	private float idleTimer;
	private float frameTimer;
	private int frameIndex;

	private void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		PickNewTarget();
	}

	private void Update()
	{
		bool moving = TickMove(Time.deltaTime);
		Animate(moving, Time.deltaTime);
	}

	private bool TickMove(float dt)
	{
		if (idleTimer > 0f)
		{
			idleTimer -= dt; return false;
		}
		Vector2 pos = transform.position;
		if (Vector2.Distance(pos, target) < 0.08f)
		{
			PickNewTarget();
			return false;
		}
		Vector2 dir = (target - pos).normalized;
		Vector2 next = pos + dir * moveSpeed * dt;
		next.x = Mathf.Clamp(next.x, leashRect.xMin + 0.1f, leashRect.xMax - 0.1f);
		next.y = Mathf.Clamp(next.y, leashRect.yMin + 0.1f, leashRect.yMax - 0.1f);
		transform.position = new Vector3(next.x, next.y, 0f);
		return true;
	}

	private void Animate(bool moving, float dt)
	{
		var frames = moving && walkFrames.Length > 0 ? walkFrames : idleFrames;
		if (frames == null || frames.Length == 0) return;
		frameTimer += dt;
		float fps = 6f;
		float frameDuration = 1f / fps;
		if (frameTimer >= frameDuration)
		{
			frameTimer -= frameDuration;
			frameIndex = (frameIndex + 1) % frames.Length;
			sr.sprite = frames[frameIndex];
		}
	}

	private void PickNewTarget()
	{
		for (int i = 0; i < 32; i++)
		{
			float x = Random.Range(leashRect.xMin + 0.5f, leashRect.xMax - 0.5f);
			float y = Random.Range(leashRect.yMin + 0.5f, leashRect.yMax - 0.5f);
			Vector2 cand = new Vector2(x, y);
			if (cand != (Vector2)transform.position) { target = cand; break; }
		}
		idleTimer = Random.Range(idleMin, idleMax);
	}
}


