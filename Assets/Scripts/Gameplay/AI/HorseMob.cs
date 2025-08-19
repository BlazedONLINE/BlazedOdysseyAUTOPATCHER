using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HorseMob : MonoBehaviour
{
	public int maxHealth = 20;
	public float moveSpeed = 0.18f;
	public float framesPerSecond = 6f;
	public bool isAggressive = false; // passive roaming horses
	
	[Header("Combat Settings")]
	public float attackRange = 1.0f;
	public int attackDamage = 5;
	public float attackCooldown = 2f;

	[Header("Rewards")]
	public int experienceReward = 0;

	private int currentHealth;
	private SpriteRenderer spriteRenderer;
	private MobHealthBar healthBar;
	private bool isDead = false;
	private bool isAttackingPlayer = false;
	private Transform playerTarget;
	private float lastAttackTime = 0f;
	private Sprite[] idleFrames = new Sprite[0];
	private Sprite[] walkFrames = new Sprite[0];
	private System.Collections.Generic.Dictionary<string, Sprite[]> walkByDir = new System.Collections.Generic.Dictionary<string, Sprite[]>();
	private int frameIndex;
	private float frameTimer;
	private Vector2 targetPos;
	private HorseSpawner ownerSpawner;
	private Rect leashRect; // allowed roam area
	private Tilemap[] blockingTilemaps;
	private float idleTimer;
	private Vector2 currentDir;
	private float aliveSince;
	private Vector2Int stepDir; // cardinal step
	private Vector3 stepTargetWorld;
	private bool stepping;
	private string currentFacing = "south"; // south by default
	private int pathStepsRemaining = 0;
	private Vector2Int lastDir = Vector2Int.zero;

	public void Initialize(HorseSpawner spawner, Tilemap[] blocking, Rect allowed)
	{
		ownerSpawner = spawner;
		blockingTilemaps = blocking;
		leashRect = allowed;
	}

	private void Awake()
	{
		currentHealth = maxHealth;
		spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		// Respect external scale (set by spawner); do not adjust here
		spriteRenderer.transform.localScale = spriteRenderer.transform.localScale;
		gameObject.AddComponent<YSpriteSorter>();
		var rb = gameObject.AddComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Kinematic;
		rb.simulated = true;
		var col = gameObject.AddComponent<CapsuleCollider2D>();
		col.direction = CapsuleDirection2D.Vertical;
		col.size = new Vector2(0.4f, 0.8f);
		LoadIdleFrames();
		LoadWalkFrames();
		LoadDirectionalWalkFrames();
		CreateHealthBar();
		PickNewTarget();
		aliveSince = Time.time;
	}
	
	private void CreateHealthBar()
	{
		GameObject healthBarGO = new GameObject($"HealthBar_{gameObject.name}");
		healthBar = healthBarGO.AddComponent<MobHealthBar>();
		healthBar.Initialize(transform, isAggressive);
		healthBar.UpdateHealth(currentHealth, maxHealth);
	}

	private void LoadIdleFrames()
	{
		// Try multiple known resource paths (handles subfolder names with spaces)
		string[] candidates = new[]
		{
			"Monsters/Monsters and animals/Horse_unsaddled_idle (64x64)",
			"Monsters/Horse_unsaddled_idle (64x64)"
		};
		foreach (var path in candidates)
		{
			idleFrames = Resources.LoadAll<Sprite>(path);
			if (idleFrames != null && idleFrames.Length > 0)
			{
				// set initial sprite
				spriteRenderer.sprite = idleFrames[0];
				return;
			}
			// Try runtime slicing if sheet not pre-sliced in import settings
			var sheetTex = Resources.Load<Texture2D>(path);
			if (sheetTex != null)
			{
				idleFrames = SliceTextureToSprites(sheetTex, 64, 64, 64f);
				if (idleFrames.Length > 0) { spriteRenderer.sprite = idleFrames[0]; return; }
			}
		}

		// visible placeholder if missing
		var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
		expandPlaceholder(tex);
		spriteRenderer.sprite = Sprite.Create(tex, new Rect(0,0,32,32), new Vector2(0.5f,0.5f), 32f);
	}

	private void LoadWalkFrames()
	{
		string[] candidates = new[]
		{
			"Monsters/Monsters and animals/Horse_unsaddled_walk (64x64)",
			"Monsters/Horse_unsaddled_walk (64x64)"
		};
		foreach (var path in candidates)
		{
			walkFrames = Resources.LoadAll<Sprite>(path);
			if (walkFrames != null && walkFrames.Length > 0) return;
			var sheetTex = Resources.Load<Texture2D>(path);
			if (sheetTex != null)
			{
				walkFrames = SliceTextureToSprites(sheetTex, 64, 64, 64f);
				if (walkFrames.Length > 0) return;
			}
		}
	}

	private void LoadDirectionalWalkFrames()
	{
		// Group walk frames by token if available
		if (walkFrames == null || walkFrames.Length == 0) return;
		var dict = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Sprite>>()
		{
			{"north", new System.Collections.Generic.List<Sprite>()},
			{"south", new System.Collections.Generic.List<Sprite>()},
			{"east", new System.Collections.Generic.List<Sprite>()},
			{"west", new System.Collections.Generic.List<Sprite>()}
		};
		for (int i = 0; i < walkFrames.Length; i++)
		{
			var n = walkFrames[i].name.ToLower();
			bool tagged = false;
			foreach (var k in dict.Keys)
			{
				if (n.Contains(k)) { dict[k].Add(walkFrames[i]); tagged = true; break; }
			}
			if (!tagged) dict["south"].Add(walkFrames[i]); // default to south-facing frames
		}
		foreach (var kv in dict)
			walkByDir[kv.Key] = kv.Value.ToArray();
	}

	private static void expandPlaceholder(Texture2D tex)
	{
		tex.filterMode = FilterMode.Point;
		for (int y = 0; y < tex.height; y++)
			for (int x = 0; x < tex.width; x++)
				tex.SetPixel(x, y, (x + y) % 2 == 0 ? new Color(0.85f, 0.85f, 0.9f, 1f) : new Color(0.7f, 0.7f, 0.85f, 1f));
		tex.Apply();
	}

	private void Update()
	{
		if (isDead) return;
		
		bool moving = false;
		
		if (isAttackingPlayer && playerTarget != null)
		{
			moving = ChaseAndAttackPlayer();
		}
		else
		{
			moving = RoamCardinal();
		}
		
		Animate(moving);
	}
	
	private bool ChaseAndAttackPlayer()
	{
		if (playerTarget == null) 
		{
			isAttackingPlayer = false;
			return false;
		}
		
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
		
		// Attack if in range
		if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
		{
			AttackPlayer();
			return false; // Stop moving to attack
		}
		
		// Move towards player if too far
		if (distanceToPlayer > attackRange)
		{
			Vector2 direction = (playerTarget.position - transform.position).normalized;
			Vector2 newPos = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
			transform.position = newPos;
			
			// Update facing direction
			UpdateFacingDirection(direction);
			return true;
		}
		
		return false;
	}
	
	private void AttackPlayer()
	{
		lastAttackTime = Time.time;
		Debug.Log($"🐎 {gameObject.name} attacks player for {attackDamage} damage!");
		
		// Try to damage player (you'll need to implement player damage interface)
		var player = playerTarget.GetComponent<IDamageable>();
		if (player != null)
		{
			player.TakeDamage(attackDamage);
		}
		
		// Visual feedback - could add attack animation here
		StartCoroutine(FlashAttack());
	}
	
	private System.Collections.IEnumerator FlashAttack()
	{
		Color originalColor = spriteRenderer.color;
		spriteRenderer.color = Color.yellow;
		yield return new WaitForSeconds(0.2f);
		spriteRenderer.color = originalColor;
	}
	
	private void UpdateFacingDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			if (direction.x > 0)
			{
				currentFacing = "east";
				spriteRenderer.flipX = false;
				if ((!walkByDir.ContainsKey("east") || walkByDir["east"].Length == 0) && walkByDir.ContainsKey("west"))
					spriteRenderer.flipX = true;
			}
			else
			{
				currentFacing = "west";
				spriteRenderer.flipX = false;
			}
		}
		else
		{
			currentFacing = direction.y > 0 ? "north" : "south";
		}
	}

	private void Animate(bool moving)
	{
		Sprite[] frames = null;
		if (moving)
		{
			if (!walkByDir.TryGetValue(currentFacing, out frames) || frames == null || frames.Length == 0)
				frames = walkFrames; // fallback
		}
		else
		{
			frames = idleFrames;
		}
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

	private bool RoamCardinal()
	{
		if (idleTimer > 0f) { idleTimer -= Time.deltaTime; return false; }
		if (!stepping)
		{
			ChooseCardinalStep();
			if (!stepping) { idleTimer = Random.Range(0.5f, 1.2f); return false; }
		}
		Vector2 pos = transform.position;
		Vector2 desired = ((Vector2)stepTargetWorld - pos).normalized;
		currentDir = desired;
		Vector2 next = pos + desired * moveSpeed * Time.deltaTime;
		if (Vector2.Distance(next, stepTargetWorld) <= 0.03f || Vector2.Distance(pos, stepTargetWorld) <= 0.03f)
		{
			transform.position = stepTargetWorld;
			stepping = false;
			idleTimer = Random.Range(0.5f, 1.2f);
			return false;
		}
		transform.position = new Vector3(next.x, next.y, 0f);
		return true;
	}

	private void PickNewTarget()
	{
		Rect r = leashRect.width > 0.01f ? leashRect : (MapBounds.Instance != null ? MapBounds.Instance.worldRect : new Rect(transform.position.x-1, transform.position.y-1, 2, 2));
		for (int i = 0; i < 30; i++)
		{
			float x = Random.Range(r.xMin + 0.5f, r.xMax - 0.5f);
			float y = Random.Range(r.yMin + 0.5f, r.yMax - 0.5f);
			Vector2 cand = new Vector2(x, y);
			if (!IsBlocked(cand)) { targetPos = cand; break; }
		}
		idleTimer = Random.Range(0.4f, 1.2f);
	}

	private void ChooseCardinalStep()
	{
		Vector2 pos = transform.position;
		Vector2Int cell = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
		Vector2Int[] dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
		// Follow current path if still valid
		if (pathStepsRemaining > 0 && lastDir != Vector2Int.zero)
		{
			Vector2Int cand = cell + lastDir;
			Vector3 world = new Vector3(cand.x, cand.y, 0f);
			if (!IsOutOfLeash(world) && !IsBlocked(world))
			{
				stepDir = lastDir; stepTargetWorld = world; stepping = true; pathStepsRemaining--; SetFacingFromDir(stepDir); return;
			}
			pathStepsRemaining = 0;
		}
		// Weighted random: prefer continuing, then perpendiculars, then reverse
		System.Collections.Generic.List<Vector2Int> order = new System.Collections.Generic.List<Vector2Int>();
		if (lastDir != Vector2Int.zero) order.Add(lastDir);
		if (lastDir == Vector2Int.up || lastDir == Vector2Int.down) { order.Add(Vector2Int.left); order.Add(Vector2Int.right); }
		else if (lastDir == Vector2Int.left || lastDir == Vector2Int.right) { order.Add(Vector2Int.up); order.Add(Vector2Int.down); }
		foreach (var d in dirs) if (!order.Contains(d)) order.Add(d);
		foreach (var d in order)
		{
			Vector2Int cand = cell + d;
			Vector3 world = new Vector3(cand.x, cand.y, 0f);
			if (IsOutOfLeash(world) || IsBlocked(world)) continue;
			stepDir = d; stepTargetWorld = world; stepping = true; lastDir = d; pathStepsRemaining = Random.Range(2, 5); SetFacingFromDir(stepDir); return;
		}
		stepping = false;
	}

	private bool IsOutOfLeash(Vector3 world)
	{
		return (world.x < leashRect.xMin + 0.1f || world.x > leashRect.xMax - 0.1f || world.y < leashRect.yMin + 0.1f || world.y > leashRect.yMax - 0.1f);
	}

	private void SetFacingFromDir(Vector2Int d)
	{
		if (d == Vector2Int.up) currentFacing = "north";
		else if (d == Vector2Int.down) currentFacing = "south";
		else if (d == Vector2Int.left) { currentFacing = "west"; spriteRenderer.flipX = false; }
		else { currentFacing = "east"; spriteRenderer.flipX = false; if ((!walkByDir.ContainsKey("east") || walkByDir["east"].Length == 0) && walkByDir.ContainsKey("west")) spriteRenderer.flipX = true; }
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

	private Sprite[] SliceTextureToSprites(Texture2D tex, int tileW, int tileH, float ppu)
	{
		if (tex == null || tileW <= 0 || tileH <= 0) return new Sprite[0];
		tex.filterMode = FilterMode.Point;
		int cols = tex.width / tileW;
		int rows = tex.height / tileH;
		var list = new System.Collections.Generic.List<Sprite>();
		for (int y = rows - 1; y >= 0; y--)
		{
			for (int x = 0; x < cols; x++)
			{
				var rect = new Rect(x * tileW, y * tileH, tileW, tileH);
				var sp = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu);
				list.Add(sp);
			}
		}
		return list.ToArray();
	}

	public void Damage(int amount, Transform attacker = null)
	{
		if (isDead) return;
		
		int actualDamage = Mathf.Abs(amount);
		currentHealth -= actualDamage;
		
		// Update health bar
		if (healthBar != null)
		{
			healthBar.UpdateHealth(currentHealth, maxHealth);
		}
		
		// Visual feedback - flash red briefly
		if (spriteRenderer != null)
		{
			StartCoroutine(FlashDamage());
		}
		
		// Become aggressive if not already and attacked by player
		if (!isAggressive && attacker != null && attacker.CompareTag("Player"))
		{
			BecomeAggressive(attacker);
		}
		
		Debug.Log($"[HorseMob] {gameObject.name} took {actualDamage} damage! Health: {currentHealth}/{maxHealth}");
		
		if (currentHealth <= 0)
		{
			Debug.Log($"[HorseMob] {gameObject.name} died!");
			Die();
		}
	}
	
	private void BecomeAggressive(Transform target)
	{
		isAggressive = true;
		isAttackingPlayer = true;
		playerTarget = target;
		
		// Update health bar color to red
		if (healthBar != null)
		{
			healthBar.SetAggressive(true);
		}
		
		Debug.Log($"🐎 {gameObject.name} became aggressive and is now attacking {target.name}!");
	}
	
	private System.Collections.IEnumerator FlashDamage()
	{
		Color originalColor = spriteRenderer.color;
		spriteRenderer.color = Color.red;
		yield return new WaitForSeconds(0.1f);
		spriteRenderer.color = originalColor;
	}

	private void Die()
	{
		isDead = true;
		
		// Notify health bar of death
		if (healthBar != null)
		{
			healthBar.OnMobDeath();
		}
		// Grant experience hook (placeholder): send message to attacker if it has AddExperience(int)
		if (playerTarget != null && experienceReward > 0)
		{
			playerTarget.SendMessage("AddExperience", experienceReward, SendMessageOptions.DontRequireReceiver);
		}
		
		// Start death animation/sequence
		StartCoroutine(DeathSequence());
	}
	
	private System.Collections.IEnumerator DeathSequence()
	{
		// Death animation - fade out and fall
		Color originalColor = spriteRenderer.color;
		Vector3 originalScale = spriteRenderer.transform.localScale;
		
		float deathDuration = 1.0f;
		float elapsed = 0f;
		
		while (elapsed < deathDuration)
		{
			elapsed += Time.deltaTime;
			float progress = elapsed / deathDuration;
			
			// Fade out
			Color newColor = originalColor;
			newColor.a = Mathf.Lerp(1f, 0f, progress);
			spriteRenderer.color = newColor;
			
			// Shrink slightly
			float scaleMultiplier = Mathf.Lerp(1f, 0.8f, progress);
			spriteRenderer.transform.localScale = originalScale * scaleMultiplier;
			
			yield return null;
		}
		
		// Notify spawner and cleanup
		if (ownerSpawner != null) 
		{
			ownerSpawner.NotifyMobDied(this);
		}
		
		Destroy(gameObject);
	}
}


