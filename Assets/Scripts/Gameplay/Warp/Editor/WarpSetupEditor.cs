#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WarpSetupEditor
{
    private const string NewInnScenePath = "Assets/Maps/Accepted/Poppy Inn/PoppyInn.unity";
    private static readonly Vector2 OutsideWarpPos = new Vector2(9.5296f, -9.002f);

    [MenuItem("BlazedOdyssey/Warp/Setup Poppy Inn Warps")] 
    public static void SetupWarps()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode before running setup.", "OK");
            return;
        }

        var outsideScene = EditorSceneManager.GetActiveScene();
        if (!outsideScene.IsValid() || outsideScene.isDirty == false)
        {
            // ok either way; we'll save after creating objects
        }

        // Ensure SceneSpawnResolver
        EnsureComponentInScene<SceneSpawnResolver>();

        // Create TownEntrance spawn at player position (or fallback 0,0)
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 townSpawnPos = player ? player.transform.position : Vector3.zero;
        CreateSpawnPoint("TownEntrance", townSpawnPos);

        // Create outside warp trigger to Inn
        var warpOutside = new GameObject("Warp_To_Inn", typeof(BoxCollider2D), typeof(WarpPoint))
        {
            tag = "Untagged"
        };
        warpOutside.transform.position = new Vector3(OutsideWarpPos.x, OutsideWarpPos.y, 0f);
        var bc = warpOutside.GetComponent<BoxCollider2D>();
        bc.isTrigger = true; bc.size = new Vector2(0.9f, 0.9f);
        var wp = warpOutside.GetComponent<WarpPoint>();
        wp.requireButton = true; wp.triggerKey = KeyCode.E; wp.targetScene = System.IO.Path.GetFileNameWithoutExtension(NewInnScenePath); wp.targetSpawnPoint = "InnEntrance";

        // Save outside scene
        EditorSceneManager.MarkSceneDirty(outsideScene);
        EditorSceneManager.SaveScene(outsideScene);

        // Create (or open) PoppyInn scene
        Scene innScene;
        if (System.IO.File.Exists(NewInnScenePath))
        {
            innScene = EditorSceneManager.OpenScene(NewInnScenePath, OpenSceneMode.Single);
        }
        else
        {
            innScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(innScene, NewInnScenePath);
        }

        // Add resolver
        EnsureComponentInScene<SceneSpawnResolver>();

        // Add inn entrance spawn and warp back
        CreateSpawnPoint("InnEntrance", Vector3.zero);
        var warpInside = new GameObject("Warp_To_Town", typeof(BoxCollider2D), typeof(WarpPoint));
        warpInside.transform.position = new Vector3(0f, -1f, 0f);
        var bci = warpInside.GetComponent<BoxCollider2D>(); bci.isTrigger = true; bci.size = new Vector2(0.9f,0.9f);
        var wpi = warpInside.GetComponent<WarpPoint>(); wpi.requireButton = true; wpi.triggerKey = KeyCode.E; wpi.targetScene = outsideScene.name; wpi.targetSpawnPoint = "TownEntrance";

        // Save inn scene and add to build settings if needed
        EditorSceneManager.MarkSceneDirty(innScene);
        EditorSceneManager.SaveScene(innScene);
        AddSceneToBuildSettings(NewInnScenePath);

        // Reopen outside scene
        EditorSceneManager.OpenScene(outsideScene.path, OpenSceneMode.Single);
        EditorUtility.DisplayDialog("Warp Setup", "Poppy Inn warps and spawn points have been created. Adjust trigger positions as needed.", "OK");
    }

    private static void CreateSpawnPoint(string name, Vector3 position)
    {
        var sp = new GameObject(name, typeof(SpawnPoint)); sp.transform.position = position; sp.GetComponent<SpawnPoint>().spawnName = name;
    }

    private static void EnsureComponentInScene<T>() where T : Component
    {
        if (Object.FindObjectOfType<T>() == null)
        {
            new GameObject(typeof(T).Name, typeof(T));
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list) if (s.path == scenePath) return;
        list.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
#endif


