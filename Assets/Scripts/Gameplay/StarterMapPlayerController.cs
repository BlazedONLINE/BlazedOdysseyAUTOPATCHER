using UnityEngine;
using UnityEngine.InputSystem;

public class StarterMapPlayerController : MonoBehaviour
{
	public float moveSpeed = 2.4f; // world units per second (slower walk)
	public float spriteScale = 1.35f; // make character a bit bigger
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	
	private void Awake()
	{
		spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sortingOrder = 5; // draw above ground tilemaps by default
		LoadIdle();
		SetupAnimator();
		transform.localScale = Vector3.one * spriteScale;
		// Ensure we participate in trigger collisions (warps) without physics forces
		var rb = GetComponent<Rigidbody2D>();
		if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Kinematic;
		rb.simulated = true;
		var col = GetComponent<CapsuleCollider2D>();
		if (col == null) col = gameObject.AddComponent<CapsuleCollider2D>();
		col.direction = CapsuleDirection2D.Vertical;
		col.size = new Vector2(0.4f, 0.8f);
	}

	private void LoadIdle()
	{
		// Try explicit idle path first
		var idle = Resources.Load<Sprite>(SelectedCharacter.GetIdleResourcePath());
		if (idle != null) { spriteRenderer.sprite = idle; return; }
		// Otherwise, use first frame of walk sheet
		var sheet = Resources.Load<Texture2D>(SelectedCharacter.GetWalkFramesFolder());
		if (sheet != null)
		{
			sheet.filterMode = FilterMode.Point;
			var all = Slice(sheet, 64, 64);
			if (all.Count > 0) { spriteRenderer.sprite = all[0]; return; }
		}

		// Last resort: visible placeholder
		var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
		tex.filterMode = FilterMode.Point;
		for (int y = 0; y < tex.height; y++)
			for (int x = 0; x < tex.width; x++)
				tex.SetPixel(x, y, (x + y) % 2 == 0 ? new Color(0.2f, 0.9f, 0.9f, 1f) : new Color(0.1f, 0.5f, 0.8f, 1f));
		tex.Apply();
		spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f);
	}

	private void SetupAnimator()
	{
		animator = gameObject.AddComponent<Animator>();
		// Runtime simple animator replacement using Animation component would be verbose;
		// instead, we animate by swapping sprites manually in Update from Resources/walk frames.
	}

	private enum Dir { South, SouthEast, East, NorthEast, North, NorthWest, West, SouthWest }
	private System.Collections.Generic.Dictionary<Dir, Sprite[]> walkByDir = new System.Collections.Generic.Dictionary<Dir, Sprite[]>();
	private Dir currentDir = Dir.South;
	private Vector2 lastMoveInput;
	private Sprite[] walkFrames; // current direction frames
	private int frameIndex;
	private float frameTimer;
	public float framesPerSecond = 7f;

	private void Start()
	{
		// Load and slice the walk sheet from Resources/Characters/<ClassName>/<ClassName>_walk
		var sheet = Resources.Load<Texture2D>(SelectedCharacter.GetWalkFramesFolder());
		if (sheet != null)
		{
			sheet.filterMode = FilterMode.Point;
			var all = SliceAuto(sheet);
			// Rows order in Franuka pack (top->bottom): South, East, North, West
			AssignRowFromTop(Dir.South, all, 0);
			AssignRowFromTop(Dir.East,  all, 1);
			AssignRowFromTop(Dir.North, all, 2);
			AssignRowFromTop(Dir.West,  all, 3);
		}
		// Default frames
		walkFrames = walkByDir.ContainsKey(currentDir) ? walkByDir[currentDir] : new Sprite[0];
	}

	private void AssignRowFromTop(Dir dir, System.Collections.Generic.List<Sprite> all, int ordinalFromTop)
	{
		var row = new System.Collections.Generic.List<Sprite>();
		// Determine unique row indices (bottom->top)
		var rowIndices = new System.Collections.Generic.HashSet<int>();
		int cellH = Mathf.RoundToInt(all[0].rect.height);
		for (int i = 0; i < all.Count; i++) rowIndices.Add(Mathf.FloorToInt(all[i].rect.y / (float)cellH));
		var sorted = new System.Collections.Generic.List<int>(rowIndices);
		sorted.Sort(); // bottom->top
		int pick = Mathf.Clamp(sorted.Count - 1 - ordinalFromTop, 0, sorted.Count - 1);
		int targetRow = sorted[pick];
		for (int i = 0; i < all.Count; i++) { var s = all[i]; int r = Mathf.FloorToInt(s.rect.y / (float)cellH); if (r == targetRow) row.Add(s); }
		row.Sort((a, b) => a.rect.x.CompareTo(b.rect.x));
		if (row.Count > 0) walkByDir[dir] = row.ToArray();
	}

	private System.Collections.Generic.List<Sprite> Slice(Texture2D tex, int w, int h)
	{
		var list = new System.Collections.Generic.List<Sprite>();
		int cols = Mathf.Max(1, tex.width / w);
		int rows = Mathf.Max(1, tex.height / h);
		for (int ry = 0; ry < rows; ry++)
		{
			for (int cx = 0; cx < cols; cx++)
			{
				int x = cx * w; int y = ry * h;
				if (x + w > tex.width || y + h > tex.height) continue;
				var rect = new Rect(x, y, w, h);
				var sp = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 64f);
				list.Add(sp);
			}
		}
		// Sort bottom-to-top, then left-to-right
		list.Sort((a, b) =>
		{
			int ay = Mathf.FloorToInt(a.rect.y);
			int by = Mathf.FloorToInt(b.rect.y);
			if (ay != by) return ay.CompareTo(by);
			return Mathf.FloorToInt(a.rect.x).CompareTo(Mathf.FloorToInt(b.rect.x));
		});
		return list;
	}

	private System.Collections.Generic.List<Sprite> SliceAuto(Texture2D tex)
	{
		int cell = GuessSquareCellSize(tex.width, tex.height);
		return Slice(tex, cell, cell);
	}

	private int GuessSquareCellSize(int width, int height)
	{
		int[] common = new int[] { 48, 64 };
		for (int i = 0; i < common.Length; i++)
			if (width % common[i] == 0 && height % common[i] == 0) return common[i];
		// Fallback to GCD in sane bounds
		int a = width, b = height; while (b != 0) { int t = a % b; a = b; b = t; }
		int gcd = Mathf.Max(1, a);
		if (gcd >= 16 && gcd <= 256) return gcd;
		return 48; // default for Franuka heroes
	}

	private void LoadDir(Dir d, string folder)
	{
		var frames = Resources.LoadAll<Sprite>(folder);
		if (frames != null && frames.Length > 0)
			walkByDir[d] = frames;
	}

	private void Update()
	{
		Vector2 input = Vector2.zero;
		if (Keyboard.current != null)
		{
			if (Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
			if (Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
			if (Keyboard.current.upArrowKey.isPressed) input.y += 1f;
			if (Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
		}
		Vector3 delta = new Vector3(input.x, input.y, 0f).normalized * moveSpeed * Time.deltaTime;
		Vector3 next = transform.position + delta;
		// Prevent movement through blocking tilemaps (Object/Above)
		if (IsBlocked(next))
		{
			Vector3 tryX = transform.position + new Vector3(delta.x, 0f, 0f);
			Vector3 tryY = transform.position + new Vector3(0f, delta.y, 0f);
			if (!IsBlocked(tryX)) next = tryX;
			else if (!IsBlocked(tryY)) next = tryY;
			else next = transform.position;
		}
		// Clamp to map bounds if available
		if (MapBounds.Instance != null)
		{
			var r = MapBounds.Instance.worldRect;
			next.x = Mathf.Clamp(next.x, r.xMin, r.xMax);
			next.y = Mathf.Clamp(next.y, r.yMin, r.yMax);
		}
		transform.position = next;

		bool isMoving = input.sqrMagnitude > 0.01f;
		if (isMoving && walkFrames.Length > 0)
		{
			// Determine 8-direction sector from input instantly for snappy turning
			lastMoveInput = input;
			Dir nextDir = FromVector(lastMoveInput);
			if (nextDir != currentDir)
			{
				currentDir = nextDir;
				walkFrames = walkByDir.ContainsKey(currentDir) ? walkByDir[currentDir] : walkFrames;
				frameIndex = 0; // restart animation on turn for responsiveness
			}
			frameTimer += Time.deltaTime;
			float frameDuration = 1f / framesPerSecond;
			if (frameTimer >= frameDuration)
			{
				frameTimer -= frameDuration;
				frameIndex = (frameIndex + 1) % walkFrames.Length;
				spriteRenderer.sprite = walkFrames[frameIndex];
			}
		}
		else
		{
			// return to idle
			LoadIdle();
		}
	}

	private Dir FromVector(Vector2 v)
	{
		float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg; // -180..180 (0 = east)
		if (angle < 0) angle += 360f;
		// sector width 45 degrees centered on the 8 directions
		if (In(angle, 337.5f, 360f) || In(angle, 0f, 22.5f)) return Dir.East;
		if (In(angle, 22.5f, 67.5f)) return Dir.NorthEast;
		if (In(angle, 67.5f, 112.5f)) return Dir.North;
		if (In(angle, 112.5f, 157.5f)) return Dir.NorthWest;
		if (In(angle, 157.5f, 202.5f)) return Dir.West;
		if (In(angle, 202.5f, 247.5f)) return Dir.SouthWest;
		if (In(angle, 247.5f, 292.5f)) return Dir.South;
		return Dir.SouthEast; // 292.5..337.5
	}

	private bool In(float a, float min, float max) => a >= min && a < max;

	private bool IsBlocked(Vector3 worldPos)
	{
		if (MapBounds.Instance == null) return false;
		var tms = MapBounds.Instance.GetBlockingTilemaps();
		if (tms == null) return false;
		for (int i = 0; i < tms.Length; i++)
		{
			var tm = tms[i]; if (tm == null) continue;
			var cell = tm.WorldToCell(worldPos);
			if (tm.HasTile(cell)) return true;
		}
		return false;
	}
}


