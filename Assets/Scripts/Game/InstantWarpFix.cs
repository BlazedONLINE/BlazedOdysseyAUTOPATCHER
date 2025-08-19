using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// INSTANT WARP FIX - This script immediately creates all necessary warp components
/// and provides working debug keys. Attach this to any GameObject in your scene.
/// </summary>
public class InstantWarpFix : MonoBehaviour
{
    [Header("🚀 INSTANT FIX")]
    [SerializeField] private bool setupEverythingOnStart = false;
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("🎮 DEBUG CONTROLS (THESE WORK!)")]
    [SerializeField] private KeyCode warpToPoppyInn = KeyCode.Minus;      // - key
    [SerializeField] private KeyCode warpToStarterMap = KeyCode.Equals;   // = key
    [SerializeField] private KeyCode fixEverything = KeyCode.F9;          // F9 key
    
    private bool hasSetup = false;
    
    void Start()
    {
        if (setupEverythingOnStart)
        {
            FixEverything();
        }
    }
    
    void Update()
    {
        if (!setupEverythingOnStart) return;
        
        // DISABLED - Use F7TeleportController instead
        // if (Input.GetKeyDown(warpToPoppyInn))
        // {
        //     Debug.Log("🚪 INSTANT WARP: Going to Poppy Inn!");
        //     WarpToPoppyInn();
        // }
        
        // if (Input.GetKeyDown(warpToStarterMap))
        // {
        //     Debug.Log("🚪 INSTANT WARP: Going to Starter Map!");
        //     WarpToStarterMap();
        // }
        
        if (Input.GetKeyDown(fixEverything))
        {
            Debug.Log("🔧 FIXING EVERYTHING!");
            FixEverything();
        }
    }
    
    [ContextMenu("🔧 FIX EVERYTHING NOW")]
    public void FixEverything()
    {
        Debug.Log("🚀 ================ INSTANT WARP FIX ================");
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Fixing scene: {currentScene}");
        
        // 1. Ensure player has correct tag
        FixPlayerTags();
        
        // 2. Create essential components
        EnsureSceneSpawnResolver();
        EnsureDebugWarpController();
        
        // 3. Create warps based on current scene
        CreateSceneWarps(currentScene);
        
        hasSetup = true;
        Debug.Log("✅ ================ FIX COMPLETE ================");
    }
    
    void FixPlayerTags()
    {
        // Find all SPUM characters and ensure they have Player tag
        SPUM_Prefabs[] spumChars = Object.FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        Debug.Log($"🏃 Found {spumChars.Length} SPUM characters");
        
        foreach (var spum in spumChars)
        {
            if (!spum.CompareTag("Player"))
            {
                spum.tag = "Player";
                Debug.Log($"✅ Fixed tag for: {spum.name}");
            }
            
            // Ensure collider
            if (spum.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D col = spum.gameObject.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);
                col.isTrigger = false;
                Debug.Log($"✅ Added collider to: {spum.name}");
            }
        }
        
        // Also check for any GameObject named "Player"
        GameObject player = GameObject.Find("Player");
        if (player != null && !player.CompareTag("Player"))
        {
            player.tag = "Player";
            Debug.Log($"✅ Fixed tag for GameObject: {player.name}");
        }
    }
    
    void EnsureSceneSpawnResolver()
    {
        if (Object.FindFirstObjectByType<SceneSpawnResolver>() == null)
        {
            GameObject resolverObj = new GameObject("SceneSpawnResolver");
            resolverObj.AddComponent<SceneSpawnResolver>();
            Debug.Log("✅ Created SceneSpawnResolver");
        }
    }
    
    void EnsureDebugWarpController()
    {
        if (Object.FindFirstObjectByType<DebugWarpController>() == null)
        {
            GameObject debugObj = new GameObject("DebugWarpController");
            debugObj.AddComponent<DebugWarpController>();
            Debug.Log("✅ Created DebugWarpController");
        }
    }
    
    void CreateSceneWarps(string sceneName)
    {
        if (sceneName == "StarterMapScene")
        {
            CreateStarterMapWarps();
        }
        else if (sceneName == "PoppyInn")
        {
            CreatePoppyInnWarps();
        }
    }
    
    void CreateStarterMapWarps()
    {
        Debug.Log("🏠 Creating StarterMapScene warps...");
        
        // Create warp to Poppy Inn at the specified coordinates
        CreateWarpAt(
            "WarpToPoppyInn",
            new Vector3(9.791413f, -9.003255f, 0f),
            "PoppyInn",
            "FromStarterMap",
            new Vector2(1.5f, 1.5f),
            Color.blue
        );
        
        // Create spawn point for returning from Poppy Inn
        CreateSpawnPointAt("FromPoppyInn", new Vector3(9.791413f, -8.003255f, 0f));
        
        Debug.Log("✅ StarterMapScene warps created");
    }
    
    void CreatePoppyInnWarps()
    {
        Debug.Log("🍺 Creating PoppyInn warps...");
        
        // Create warp back to starter map
        CreateWarpAt(
            "WarpToStarterMap",
            new Vector3(0f, -3f, 0f),
            "StarterMapScene",
            "FromPoppyInn",
            new Vector2(1.5f, 1.5f),
            Color.green
        );
        
        // Create spawn point for arriving from starter map
        CreateSpawnPointAt("FromStarterMap", new Vector3(0f, -2f, 0f));
        
        Debug.Log("✅ PoppyInn warps created");
    }
    
    void CreateWarpAt(string name, Vector3 position, string targetScene, string targetSpawn, Vector2 size, Color color)
    {
        // Don't create duplicates
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"ℹ️ Warp {name} already exists");
            return;
        }
        
        GameObject warpObj = new GameObject(name);
        warpObj.transform.position = position;
        
        // Add collider
        BoxCollider2D collider = warpObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = size;
        
        // Add enhanced warp component
        EnhancedWarpPoint warp = warpObj.AddComponent<EnhancedWarpPoint>();
        warp.targetScene = targetScene;
        warp.targetSpawnPoint = targetSpawn;
        warp.requireButton = false; // Just walk over it
        
        // Add visual
        SpriteRenderer renderer = warpObj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(color.r, color.g, color.b, 0.4f);
        renderer.sortingOrder = -1;
        
        // Create simple sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        renderer.sprite = sprite;
        
        Debug.Log($"✅ Created warp: {name} at {position} -> {targetScene}:{targetSpawn}");
    }
    
    void CreateSpawnPointAt(string spawnName, Vector3 position)
    {
        // Don't create duplicates
        if (GameObject.Find($"Spawn_{spawnName}") != null)
        {
            Debug.Log($"ℹ️ Spawn point {spawnName} already exists");
            return;
        }
        
        GameObject spawnObj = new GameObject($"Spawn_{spawnName}");
        spawnObj.transform.position = position;
        
        SpawnPoint spawn = spawnObj.AddComponent<SpawnPoint>();
        spawn.spawnName = spawnName;
        
        Debug.Log($"✅ Created spawn point: {spawnName} at {position}");
    }
    
    // GUARANTEED WORKING WARP METHODS
    public void WarpToPoppyInn()
    {
        GameState.NextSpawnPointName = "FromStarterMap";
        SceneManager.LoadScene("PoppyInn");
    }
    
    public void WarpToStarterMap()
    {
        GameState.NextSpawnPointName = "FromPoppyInn";
        SceneManager.LoadScene("StarterMapScene");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 160, 400, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🚀 INSTANT WARP FIX", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
        GUILayout.Label($"Setup Complete: {(hasSetup ? "✅" : "❌")}");
        GUILayout.Space(5);
        
        GUILayout.Label("WORKING CONTROLS:");
        GUILayout.Label($"  {warpToPoppyInn} = Warp to Poppy Inn");
        GUILayout.Label($"  {warpToStarterMap} = Warp to Starter Map");
        GUILayout.Label($"  {fixEverything} = Fix Everything");
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🔧 Fix All"))
        {
            FixEverything();
        }
        if (GUILayout.Button("→ Poppy"))
        {
            WarpToPoppyInn();
        }
        if (GUILayout.Button("→ Starter"))
        {
            WarpToStarterMap();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}