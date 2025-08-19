using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Redirects SharpUI scene names to our own scenes without modifying SharpUI.
/// Drop this on a bootstrap GameObject in the first scene.
/// Default maps SharpUI "GamePlayground" to our "Scenes/StarterMapScene".
/// </summary>
public class SharpUISceneRedirect : MonoBehaviour
{
	[Tooltip("Map SharpUI scene name → our scene name in Build Settings")] 
	[SerializeField] private List<SceneNameMap> mappings = new List<SceneNameMap>
	{
		new SceneNameMap("GamePlayground", "Scenes/StarterMapScene")
	};

	private Dictionary<string, string> _map;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		_map = new Dictionary<string, string>();
		for (int i = 0; i < mappings.Count; i++)
		{
			var m = mappings[i];
			if (!string.IsNullOrEmpty(m.fromName) && !string.IsNullOrEmpty(m.toName))
				_map[m.fromName] = m.toName;
		}
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (_map == null || _map.Count == 0) return;
		if (_map.TryGetValue(scene.name, out var target))
		{
			// Load target scene and unload redirected one
			LoadTargetScene(target, scene);
		}
	}

	private void LoadTargetScene(string targetSceneName, Scene sceneToUnload)
	{
		if (string.IsNullOrEmpty(targetSceneName)) return;
		// If target is already loaded, just switch active scene
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			var s = SceneManager.GetSceneAt(i);
			if (s.name == targetSceneName)
			{
				SceneManager.SetActiveScene(s);
				if (sceneToUnload.IsValid()) SceneManager.UnloadSceneAsync(sceneToUnload);
				return;
			}
		}
		// Load and then unload redirected scene
		var async = SceneManager.LoadSceneAsync(targetSceneName);
		async.completed += _ =>
		{
			var loaded = SceneManager.GetSceneByName(targetSceneName);
			if (loaded.IsValid()) SceneManager.SetActiveScene(loaded);
			if (sceneToUnload.IsValid()) SceneManager.UnloadSceneAsync(sceneToUnload);
		};
	}

	[System.Serializable]
	public struct SceneNameMap
	{
		public string fromName;
		public string toName;
		public SceneNameMap(string from, string to)
		{
			fromName = from; toName = to;
		}
	}
}


