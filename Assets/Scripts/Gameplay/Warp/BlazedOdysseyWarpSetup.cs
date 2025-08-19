using UnityEngine;

/// <summary>
/// Specific warp setup for Blazed Odyssey MMO.
/// This script sets up the warps between StarterMapScene and PoppyInn.
/// </summary>
public class BlazedOdysseyWarpSetup : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Manual Setup")]
    [SerializeField] private bool createPlayerPositionUI = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupBlazedOdysseyWarps();
        }
        
        if (createPlayerPositionUI)
        {
            SetupPlayerPositionUI();
        }
    }
    
    [ContextMenu("Setup Blazed Odyssey Warps")]
    public void SetupBlazedOdysseyWarps()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (showDebugLogs)
            Debug.Log($"🗺️ Setting up warps for scene: {currentScene}");
        
        switch (currentScene)
        {
            case "StarterMapScene":
                SetupStarterMapWarps();
                break;
                
            case "PoppyInn":
                SetupPoppyInnWarps();
                break;
                
            default:
                if (showDebugLogs)
                    Debug.LogWarning($"⚠️ No warp setup configured for scene: {currentScene}");
                break;
        }
    }
    
    void SetupStarterMapWarps()
    {
        if (showDebugLogs)
            Debug.Log("🏠 Setting up StarterMapScene warps...");
        
        // Create or find WarpManager
        WarpManager warpManager = FindOrCreateWarpManager();
        
        // Create warp to INN at the specified position
        var warpData = new WarpManager.WarpData
        {
            warpName = "Entrance to Poppy Inn",
            warpPosition = new Vector3(9.791413f, -9.003255f, 0f),
            warpSize = new Vector2(1.5f, 1.5f), // Slightly larger for easier activation
            targetScene = "PoppyInn",
            targetSpawnPoint = "FromStarterMap",
            requireButton = false, // Just step on it
            triggerKey = KeyCode.E,
            showGizmo = true,
            gizmoColor = Color.blue
        };
        
        CreateSingleWarp(warpManager, warpData);
        
        // Create spawn point for return from INN
        warpManager.CreateSpawnPoint("FromPoppyInn", new Vector3(9.791413f, -8.003255f, 0f)); // Slightly above warp
        
        if (showDebugLogs)
            Debug.Log("✅ StarterMapScene warps setup complete");
    }
    
    void SetupPoppyInnWarps()
    {
        if (showDebugLogs)
            Debug.Log("🍺 Setting up PoppyInn warps...");
        
        // Create or find WarpManager
        WarpManager warpManager = FindOrCreateWarpManager();
        
        // Create warp back to starter map (near the entrance)
        var warpData = new WarpManager.WarpData
        {
            warpName = "Exit to Starter Map",
            warpPosition = new Vector3(0f, -3f, 0f), // Adjust this position based on INN layout
            warpSize = new Vector2(1.5f, 1.5f),
            targetScene = "StarterMapScene",
            targetSpawnPoint = "FromPoppyInn",
            requireButton = false, // Just step on it
            triggerKey = KeyCode.E,
            showGizmo = true,
            gizmoColor = Color.green
        };
        
        CreateSingleWarp(warpManager, warpData);
        
        // Create spawn point for arrival from starter map
        warpManager.CreateSpawnPoint("FromStarterMap", new Vector3(0f, -2f, 0f)); // Slightly above exit warp
        
        if (showDebugLogs)
            Debug.Log("✅ PoppyInn warps setup complete");
    }
    
    WarpManager FindOrCreateWarpManager()
    {
        WarpManager warpManager = FindObjectOfType<WarpManager>();
        
        if (warpManager == null)
        {
            GameObject warpManagerObj = new GameObject("WarpManager");
            warpManager = warpManagerObj.AddComponent<WarpManager>();
            
            if (showDebugLogs)
                Debug.Log("📦 Created new WarpManager");
        }
        
        return warpManager;
    }
    
    void CreateSingleWarp(WarpManager warpManager, WarpManager.WarpData warpData)
    {
        // Use reflection to access the private CreateWarp method, or just create manually
        CreateWarpManually(warpData);
    }
    
    void CreateWarpManually(WarpManager.WarpData data)
    {
        // Check if warp already exists
        string warpObjectName = $"Warp_To_{data.targetScene}_{data.targetSpawnPoint}";
        if (GameObject.Find(warpObjectName) != null)
        {
            if (showDebugLogs)
                Debug.Log($"ℹ️ Warp '{warpObjectName}' already exists");
            return;
        }
        
        // Create warp GameObject
        GameObject warpObj = new GameObject(warpObjectName);
        warpObj.transform.position = data.warpPosition;
        
        // Add BoxCollider2D
        BoxCollider2D collider = warpObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = data.warpSize;
        
        // Add Enhanced WarpPoint component
        EnhancedWarpPoint warpPoint = warpObj.AddComponent<EnhancedWarpPoint>();
        warpPoint.targetScene = data.targetScene;
        warpPoint.targetSpawnPoint = data.targetSpawnPoint;
        warpPoint.requireButton = data.requireButton;
        warpPoint.triggerKey = data.triggerKey;
        
        // Add visual feedback
        if (data.showGizmo)
        {
            var renderer = warpObj.AddComponent<SpriteRenderer>();
            renderer.color = new Color(data.gizmoColor.r, data.gizmoColor.g, data.gizmoColor.b, 0.3f);
            
            // Create a simple colored square sprite
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            renderer.sprite = sprite;
            renderer.sortingOrder = -1; // Behind everything
        }
        
        if (showDebugLogs)
            Debug.Log($"✅ Created warp '{data.warpName}' at {data.warpPosition} -> {data.targetScene}:{data.targetSpawnPoint}");
    }
    
    void SetupPlayerPositionUI()
    {
        // Check if PlayerPositionUI already exists
        if (FindObjectOfType<PlayerPositionUI>() != null)
        {
            if (showDebugLogs)
                Debug.Log("ℹ️ PlayerPositionUI already exists");
            return;
        }
        
        // Create PlayerPositionUI
        GameObject positionUIObj = new GameObject("PlayerPositionUI");
        positionUIObj.AddComponent<PlayerPositionUI>();
        
        if (showDebugLogs)
            Debug.Log("✅ Created PlayerPositionUI");
    }
    
    [ContextMenu("Force Recreate All Warps")]
    public void ForceRecreateWarps()
    {
        // Remove existing warps
        WarpPoint[] existingWarps = FindObjectsOfType<WarpPoint>();
        for (int i = existingWarps.Length - 1; i >= 0; i--)
        {
            if (existingWarps[i] != null)
            {
                DestroyImmediate(existingWarps[i].gameObject);
            }
        }
        
        // Remove existing enhanced warps
        EnhancedWarpPoint[] existingEnhancedWarps = FindObjectsOfType<EnhancedWarpPoint>();
        for (int i = existingEnhancedWarps.Length - 1; i >= 0; i--)
        {
            if (existingEnhancedWarps[i] != null)
            {
                DestroyImmediate(existingEnhancedWarps[i].gameObject);
            }
        }
        
        // Remove existing spawn points
        SpawnPoint[] existingSpawns = FindObjectsOfType<SpawnPoint>();
        for (int i = existingSpawns.Length - 1; i >= 0; i--)
        {
            if (existingSpawns[i] != null)
            {
                DestroyImmediate(existingSpawns[i].gameObject);
            }
        }
        
        if (showDebugLogs)
            Debug.Log("🗑️ Removed existing warps and spawn points");
        
        // Recreate warps
        SetupBlazedOdysseyWarps();
    }
}