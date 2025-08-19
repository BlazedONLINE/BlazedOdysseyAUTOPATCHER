using UnityEngine;

public class Mob_Horse : MobBase
{
	protected override void LoadAnimations()
	{
		// idle
		string[] idleCandidates = new[]
		{
			"Monsters/Monsters and animals/Horse_unsaddled_idle (64x64)",
			"Monsters/Horse_unsaddled_idle (64x64)"
		};
		foreach (var p in idleCandidates)
		{
			idleFrames = Resources.LoadAll<Sprite>(p);
			if (idleFrames != null && idleFrames.Length > 0) { spriteRenderer.sprite = idleFrames[0]; break; }
			var tex = Resources.Load<Texture2D>(p);
			if (tex != null)
			{
				idleFrames = Slice(tex, 64, 64);
				if (idleFrames.Length > 0) { spriteRenderer.sprite = idleFrames[0]; break; }
			}
		}

		// walk
		string[] walkCandidates = new[]
		{
			"Monsters/Monsters and animals/Horse_unsaddled_walk (64x64)",
			"Monsters/Horse_unsaddled_walk (64x64)"
		};
		foreach (var p in walkCandidates)
		{
			walkFrames = Resources.LoadAll<Sprite>(p);
			if (walkFrames != null && walkFrames.Length > 0) break;
			var tex = Resources.Load<Texture2D>(p);
			if (tex != null)
			{
				walkFrames = Slice(tex, 64, 64);
				if (walkFrames.Length > 0) break;
			}
		}
	}

	private Sprite[] Slice(Texture2D tex, int w, int h)
	{
		if (tex == null) return new Sprite[0];
		tex.filterMode = FilterMode.Point;
		int cols = tex.width / w;
		int rows = tex.height / h;
		var list = new System.Collections.Generic.List<Sprite>();
		for (int y = rows - 1; y >= 0; y--)
			for (int x = 0; x < cols; x++)
				list.Add(Sprite.Create(tex, new Rect(x * w, y * h, w, h), new Vector2(0.5f, 0.5f), 64f));
		return list.ToArray();
	}
}


