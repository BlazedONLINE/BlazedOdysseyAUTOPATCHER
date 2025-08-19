using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class MobBase : MonoBehaviour
{
	[Header("Mob Stats")]
	public int maxHealth = 20;
	public float moveSpeed = 0.55f;
	public float framesPerSecond = 6f;

	[Header("Runtime State")] 
	public string mobId = "horse"; // e.g., horse, slime

	protected int currentHealth;
	protected SpriteRenderer spriteRenderer;
	protected Sprite[] idleFrames = new Sprite[0];
	protected Sprite[] walkFrames = new Sprite[0];
	protected int frameIndex;
	protected float frameTimer;
	protected Rect leashRect;
	protected Tilemap[] blockingTilemaps;
	protected MobWanderAgent2D agent = new MobWanderAgent2D();
	private float aliveSince;

	public void Initialize(Rect allowed, Tilemap[] blocking)
	{
		leashRect = allowed;
		blockingTilemaps = blocking;
	}

	protected virtual void Awake()
	{
		currentHealth = maxHealth;
		spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sortingOrder = 20; // render above ground/objects by default
		LoadAnimations();
		agent.Initialize(leashRect, blockingTilemaps, moveSpeed, 0.12f, 0.6f, 1.6f);
		agent.PickNewTarget(transform.position);
		aliveSince = Time.time;
	}

	protected abstract void LoadAnimations();

	protected virtual void Update()
	{
		bool moving = Roam();
		Animate(moving);
	}

	private bool Roam()
	{
		if (MapBounds.Instance == null) return false;
		if (Time.time - aliveSince < 0.15f) return false;
		return agent.Tick(Time.deltaTime, transform);
	}

	protected void Animate(bool moving)
	{
		var frames = moving && walkFrames.Length > 0 ? walkFrames : idleFrames;
		if (frames == null || frames.Length == 0) return;
		frameTimer += Time.deltaTime;
		float frameDuration = 1f / framesPerSecond;
		if (frameTimer >= frameDuration)
		{
			frameTimer -= frameDuration;
			frameIndex = (frameIndex + 1) % frames.Length;
			spriteRenderer.sprite = frames[frameIndex];
		}
	}

	public void Damage(int amount, Transform attacker = null)
	{
		currentHealth -= Mathf.Abs(amount);
		Debug.Log($"[MobBase] {gameObject.name} took {Mathf.Abs(amount)} damage! Health: {currentHealth}/{maxHealth}");
		if (currentHealth <= 0) Die();
	}

	protected virtual void Die()
	{
		Destroy(gameObject);
	}
}


