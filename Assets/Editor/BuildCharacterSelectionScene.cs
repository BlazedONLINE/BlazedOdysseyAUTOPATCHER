using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class BuildCharacterSelectionScene
{
	[MenuItem("BlazedOdyssey/UI/Build CharacterSelectionScene (auto)")]
	public static void BuildScene()
	{
		// Ensure Scenes folder
		if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
		{
			AssetDatabase.CreateFolder("Assets", "Scenes");
		}

		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EditorSceneManager.SetActiveScene(scene);
		scene.name = "CharacterSelectionScene";

		// Camera
		var camGo = new GameObject("Main Camera");
		var cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 10f;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f, 1f);
		camGo.tag = "MainCamera";

		// EventSystem (Input System UI)
		var es = new GameObject("EventSystem");
		es.AddComponent<EventSystem>();
		es.AddComponent<InputSystemUIInputModule>();

		// Character Selector root
		var selector = new GameObject("CharacterSelector");
		var comp = selector.AddComponent<BlazedCharacterSelector>();
		comp.showGenerateControls = false;
		comp.showGrid = false;

		// Save scene
		string path = "Assets/Scenes/CharacterSelectionScene.unity";
		EditorSceneManager.SaveScene(scene, path, true);

		// Add both scenes to build settings and make character selection first
		var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
		list.Add(new EditorBuildSettingsScene("Assets/Scenes/CharacterSelectionScene.unity", true));
		list.Add(new EditorBuildSettingsScene("Assets/Scenes/StarterMapScene.unity", true));
		EditorBuildSettings.scenes = list.ToArray();

		Debug.Log("✅ CharacterSelectionScene created and set as first scene in Build Settings.");
	}

	[MenuItem("BlazedOdyssey/UI/Fix EventSystem (to Input System)")]
	public static void FixEventSystemInOpenScene()
	{
		foreach (var ev in Object.FindObjectsOfType<EventSystem>())
		{
			var legacy = ev.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
			if (legacy != null) Undo.DestroyObjectImmediate(legacy);
			if (ev.GetComponent<InputSystemUIInputModule>() == null)
			{
				ev.gameObject.AddComponent<InputSystemUIInputModule>();
			}
		}
		Debug.Log("✅ EventSystem updated to Input System UI module.");
	}
}


