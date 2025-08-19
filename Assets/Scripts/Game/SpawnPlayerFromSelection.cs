using UnityEngine;

public class SpawnPlayerFromSelection : MonoBehaviour
{
	[Tooltip("Optional transform to use for player spawn")] public Transform spawnPoint;
	[Tooltip("Fallback position if no spawnPoint set or found")] public Vector3 spawnPosition = Vector3.zero;
	[Tooltip("When true, will try to find a Transform tagged 'PlayerSpawn' or named 'PlayerSpawn' if spawnPoint is null")] public bool autoFindSpawnPoint = true;

	private void Start()
	{
		Vector3 pos = ResolveSpawnPosition();
		
		// Debug character selection data
		Debug.Log($"🎭 Spawning character: {SelectedCharacter.ClassName}");
		Debug.Log($"🚻 Gender: {(SelectedCharacter.IsMale ? "Male" : "Female")}");
		Debug.Log($"🏰 Race: {SelectedCharacter.Race}");
		Debug.Log($"🎯 SPUM Prefab: {SelectedCharacter.SpumPrefabName}");
		
		// Try SPUM first
		if (!string.IsNullOrEmpty(SelectedCharacter.SpumPrefabName))
		{
			string loadPath = "Units/" + SelectedCharacter.SpumPrefabName;
			var spum = Resources.Load<GameObject>(loadPath);
			if (spum != null)
			{
				var go = Instantiate(spum, pos, Quaternion.identity);
				go.tag = "Player"; // Mark as player for easy identification
				Debug.Log($"✅ Spawned SPUM player: {SelectedCharacter.SpumPrefabName} at {pos}");

				// If a scene root named "Player" already exists (with controller/camera follow),
				// make this SPUM visual a child so movement/camera affect it.
				var controllerRoot = GameObject.Find("Player");
				if (controllerRoot != null && controllerRoot != go)
				{
					// Disable any placeholder sprite on the controller root to avoid the blue box
					var rootSr = controllerRoot.GetComponent<SpriteRenderer>();
					if (rootSr != null) rootSr.enabled = false;
					// Parent the visual under the controller so it moves with input and camera
					go.transform.SetParent(controllerRoot.transform, false);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					go.name = "SPUM_Visual";
					Debug.Log("📎 Attached SPUM visual under existing Player object for camera/input control");
				}
				
				// Ensure SPUM_Prefabs component is properly initialized
				var spumPrefabs = go.GetComponent<SPUM_Prefabs>();
				if (spumPrefabs != null)
				{
					Debug.Log($"✅ SPUM_Prefabs component found, ensuring proper setup");
					// The SPUM prefab should already be complete - just verify it has its data
					if (spumPrefabs.spumPackages.Count == 0)
					{
						Debug.LogWarning("⚠️ SPUM prefab has no packages - this might cause missing parts");
					}
				}
				
				// Add SPUMCharacterInitializer for fallback initialization only
				var spumInitializer = go.GetComponent<SPUMCharacterInitializer>();
				if (spumInitializer == null)
					spumInitializer = go.AddComponent<SPUMCharacterInitializer>();
				
				// Add comprehensive SPUM character controller for movement, direction, and attacks
				var spumController = go.GetComponent<SPUMCharacterController>();
				if (spumController == null)
					spumController = go.AddComponent<SPUMCharacterController>();
				
				// Initialize with character class for proper sprite loading
				spumInitializer.InitializeCharacter(SelectedCharacter.ClassName);
				
				// Diagnostics
				var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
				Debug.Log($"SPUM SpriteRenderers: {renderers.Length}");
				
				int spritesLoaded = 0;
				foreach (var sr in renderers)
				{
					if (sr.sprite != null)
					{
						spritesLoaded++;
						Debug.Log($"Loaded SpriteRenderer: {sr.sprite.name} order={sr.sortingOrder}");
					}
				}
				Debug.Log($"✅ SPUM character loaded with {spritesLoaded}/{renderers.Length} sprites");
				// Ensure an initial animation frame is applied
				var animator = go.GetComponentInChildren<Animator>();
				if (animator != null && animator.runtimeAnimatorController != null)
				{
					var clips = animator.runtimeAnimatorController.animationClips;
					AnimationClip chosen = null;
					for (int i = 0; i < clips.Length; i++)
					{
						var n = clips[i].name.ToLowerInvariant();
						if (n.Contains("idle") || n.Contains("move") || n.Contains("walk") || n.Contains("run")) { chosen = clips[i]; break; }
					}
					if (chosen == null && clips.Length > 0) chosen = clips[0];
					animator.Rebind();
					animator.Update(0f);
					if (chosen != null) animator.Play(chosen.name, 0, 0f);
				}
				return;
			}
			else
			{
				Debug.LogError($"❌ Failed to load SPUM prefab at Resources/{loadPath}");
				Debug.LogError($"🔍 Verify the prefab exists under Assets/SPUM/Resources/Units");
				Debug.LogError($"🎭 Selected character: {SelectedCharacter.ClassName}, Gender: {(SelectedCharacter.IsMale ? "Male" : "Female")}, Race: {SelectedCharacter.Race}");
			}
		}
		// Fallback: old sprite-based flow (no-op placeholder)
		Debug.LogWarning($"SPUM prefab not set/found. Would spawn legacy avatar at {pos}.");
	}

	private Vector3 ResolveSpawnPosition()
	{
		if (spawnPoint != null) return spawnPoint.position;
		if (autoFindSpawnPoint)
		{
			var tagged = GameObject.FindGameObjectWithTag("PlayerSpawn");
			if (tagged != null) return tagged.transform.position;
			var named = GameObject.Find("PlayerSpawn");
			if (named != null) return named.transform.position;
		}
		return spawnPosition;
	}
}

