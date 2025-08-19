using UnityEngine;
using UnityEngine.SceneManagement;
using MultiplayerARPG;

/// <summary>
/// Database-driven warp system runtime component.
/// This creates warps from the BlazedOdysseyWarpDatabaseManager configuration.
/// Inspired by Nexus TK's database approach but integrated with Unity ARPG.
/// </summary>
public class DatabaseDrivenWarpSystem : MonoBehaviour
{
    [Header("🗂️ Database Configuration")]
    [SerializeField] private BlazedOdysseyWarpDatabaseManager warpDatabase;
    
    [Header("🔧 Runtime Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("🎮 Debug Controls")]
    [SerializeField] private KeyCode setupWarpsKey = KeyCode.F8;
    [SerializeField] private KeyCode debugWarpToPoppyKey = KeyCode.Minus;
    [SerializeField] private KeyCode debugWarpToStarterKey = KeyCode.Equals;
    
    private string currentMapName;
    private bool warpsSetup = false;
    
    void Start()
    {
        currentMapName = SceneManager.GetActiveScene().name;
        
        if (autoSetupOnStart)
        {
            SetupCurrentMapWarps();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🗺️ DatabaseDrivenWarpSystem initialized for map: {currentMapName}");
            Debug.Log($"🎮 Debug controls: {setupWarpsKey}=Setup, {debugWarpToPoppyKey}=Poppy, {debugWarpToStarterKey}=Starter");
        }
    }
    
    void Update()
    {
        if (!enableDebugKeys) return;
        
        if (Input.GetKeyDown(setupWarpsKey))
        {
            SetupCurrentMapWarps();
        }
        
        if (Input.GetKeyDown(debugWarpToPoppyKey))
        {
            DebugWarpToPoppyInn();
        }
        
        if (Input.GetKeyDown(debugWarpToStarterKey))
        {
            DebugWarpToStarterMap();
        }
    }
    
    [ContextMenu("Setup Current Map Warps")]
    public void SetupCurrentMapWarps()
    {
        if (warpDatabase == null)
        {
            Debug.LogError("❌ No warp database assigned!");
            return;
        }
        
        currentMapName = SceneManager.GetActiveScene().name;
        var mapConfig = warpDatabase.GetMapConfig(currentMapName);
        
        if (mapConfig == null)
        {
            Debug.LogWarning($"⚠️ No warp configuration found for map: {currentMapName}");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log($"🗺️ Setting up warps for map: {currentMapName}");
        
        // Clear existing warps
        ClearExistingWarps();
        
        // Create spawn points
        CreateSpawnPoints(mapConfig);
        
        // Create warp points
        CreateWarpPoints(mapConfig);
        
        // Ensure scene resolver exists
        EnsureSceneSpawnResolver();
        
        warpsSetup = true;
        
        if (showDebugLogs)
            Debug.Log($"✅ Warps setup complete for {currentMapName}");
    }
    
    void ClearExistingWarps()
    {
        // Remove existing database-created warps
        GameObject[] existingWarps = GameObject.FindGameObjectsWithTag("DatabaseWarp");
        foreach (var warp in existingWarps)
        {
            DestroyImmediate(warp);
        }
        
        // Remove existing database-created spawns
        GameObject[] existingSpawns = GameObject.FindGameObjectsWithTag("DatabaseSpawn");
        foreach (var spawn in existingSpawns)
        {
            DestroyImmediate(spawn);
        }
        
        if (showDebugLogs)
            Debug.Log("🗑️ Cleared existing database warps and spawns");
    }
    
    void CreateSpawnPoints(BlazedOdysseyWarpDatabaseManager.WarpMapConfig mapConfig)
    {
        foreach (var spawnConfig in mapConfig.spawnLocations)
        {
            GameObject spawnObj = new GameObject($"DatabaseSpawn_{spawnConfig.spawnName}");
            spawnObj.tag = "DatabaseSpawn";
            spawnObj.transform.position = spawnConfig.position;
            spawnObj.transform.rotation = Quaternion.Euler(spawnConfig.rotation);
            
            SpawnPoint spawnPoint = spawnObj.AddComponent<SpawnPoint>();
            spawnPoint.spawnName = spawnConfig.spawnName;
            
            if (showDebugLogs)
                Debug.Log($"✅ Created spawn point: {spawnConfig.spawnName} at {spawnConfig.position}");
        }
    }
    
    void CreateWarpPoints(BlazedOdysseyWarpDatabaseManager.WarpMapConfig mapConfig)
    {
        foreach (var warpConfig in mapConfig.warpLocations)
        {
            GameObject warpObj = new GameObject($"DatabaseWarp_{warpConfig.warpName}");
            warpObj.tag = "DatabaseWarp";
            warpObj.transform.position = warpConfig.position;
            
            // Add collider for trigger detection
            BoxCollider2D collider = warpObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = warpConfig.triggerSize;
            
            // Add database warp component
            DatabaseWarpTrigger warpTrigger = warpObj.AddComponent<DatabaseWarpTrigger>();
            warpTrigger.Initialize(warpConfig, showDebugLogs);
            
            // Add visual feedback if enabled
            if (warpConfig.showVisual)
            {
                CreateWarpVisual(warpObj, warpConfig);
            }
            
            if (showDebugLogs)
                Debug.Log($"✅ Created warp: {warpConfig.warpName} at {warpConfig.position} → {warpConfig.destinationMapName}");
        }
    }
    
    void CreateWarpVisual(GameObject warpObj, BlazedOdysseyWarpDatabaseManager.WarpLocation warpConfig)
    {
        SpriteRenderer renderer = warpObj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(warpConfig.warpColor.r, warpConfig.warpColor.g, warpConfig.warpColor.b, 0.4f);
        renderer.sortingOrder = -1;
        
        // Create simple sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        renderer.sprite = sprite;
    }
    
    void EnsureSceneSpawnResolver()
    {
        if (Object.FindFirstObjectByType<SceneSpawnResolver>() == null)
        {
            GameObject resolverObj = new GameObject("SceneSpawnResolver");
            resolverObj.AddComponent<SceneSpawnResolver>();
            
            if (showDebugLogs)
                Debug.Log("✅ Created SceneSpawnResolver");
        }
    }
    
    [ContextMenu("Debug Warp to Poppy Inn")]
    public void DebugWarpToPoppyInn()
    {
        if (showDebugLogs)
            Debug.Log("🚪 DEBUG WARP: Going to Poppy Inn");
        
        GameState.NextSpawnPointName = "FromStarterMap";
        SceneManager.LoadScene("PoppyInn");
    }
    
    [ContextMenu("Debug Warp to Starter Map")]
    public void DebugWarpToStarterMap()
    {
        if (showDebugLogs)
            Debug.Log("🚪 DEBUG WARP: Going to Starter Map");
        
        GameState.NextSpawnPointName = "FromPoppyInn";
        SceneManager.LoadScene("StarterMapScene");
    }
    
    void OnGUI()
    {
        if (!enableDebugKeys) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 350, Screen.height - 180, 340, 170));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🗂️ Database Warp System", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Map: {currentMapName}");
        GUILayout.Label($"Database: {(warpDatabase != null ? "✅" : "❌")}");
        GUILayout.Label($"Warps Setup: {(warpsSetup ? "✅" : "❌")}");
        GUILayout.Space(5);
        
        GUILayout.Label("Controls:");
        GUILayout.Label($"  {setupWarpsKey} = Setup Warps");
        GUILayout.Label($"  {debugWarpToPoppyKey} = Warp to Poppy Inn");
        GUILayout.Label($"  {debugWarpToStarterKey} = Warp to Starter Map");
        GUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🔧 Setup"))
        {
            SetupCurrentMapWarps();
        }
        if (GUILayout.Button("→ Poppy"))
        {
            DebugWarpToPoppyInn();
        }
        if (GUILayout.Button("→ Starter"))
        {
            DebugWarpToStarterMap();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}