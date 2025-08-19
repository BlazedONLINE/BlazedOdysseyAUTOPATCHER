using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildStarterMapScene
{
	[MenuItem("BlazedOdyssey/World/Build StarterMapScene (auto)")]
	public static void BuildScene()
	{
		EditorUtility.DisplayProgressBar("Starter Map", "Creating scene", 0f);

		// Ensure Scenes folder
		if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
		{
			AssetDatabase.CreateFolder("Assets", "Scenes");
		}

		// New empty scene
		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EditorSceneManager.SetActiveScene(scene);
		scene.name = "StarterMapScene";

		// Camera
		var camGo = new GameObject("Main Camera");
		var cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = 4.5f; // closer default zoom
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f, 1f);
		camGo.tag = "MainCamera";
		camGo.transform.position = new Vector3(100f, 100f, -10f);

		EditorUtility.DisplayProgressBar("Starter Map", "Creating Grid and Tilemaps", 0.33f);
		// Create grid + layers
		BuildTilesetScene.CreateScene();

		EditorUtility.DisplayProgressBar("Starter Map", "Painting starter layout", 0.66f);
		// Paint 200x200 map
		WorldStarterMapGenerator.Generate();

		// Bootstrap player spawn
		var bootstrap = new GameObject("Bootstrap");
		bootstrap.AddComponent<StarterMapBootstrap>();

		// Save scene asset
		string path = "Assets/Scenes/StarterMapScene.unity";
		EditorSceneManager.SaveScene(scene, path, true);

		// Ensure included in build settings
		var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		bool exists = list.Exists(s => s.path == path);
		if (!exists)
		{
			list.Add(new EditorBuildSettingsScene(path, true));
			EditorBuildSettings.scenes = list.ToArray();
		}

		EditorUtility.ClearProgressBar();
		Debug.Log("✅ StarterMapScene created and saved to Assets/Scenes/StarterMapScene.unity");
	}
}


