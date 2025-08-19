using UnityEngine;

/// <summary>
/// Automatically sets up essential scene components for warping to work properly.
/// This ensures every scene has the necessary components for the warp system.
/// </summary>
public class AutoSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Debug Components")]
    [SerializeField] private bool addDebugWarpController = true;
    [SerializeField] private bool addPlayerPositionUI = true;
    [SerializeField] private bool addWarpSetup = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (showDebugLogs)
            Debug.Log($"🔧 Setting up scene: {sceneName}");
        
        // Ensure SceneSpawnResolver exists
        SetupSceneSpawnResolver();
        
        // Setup debug tools
        if (addDebugWarpController)
            SetupDebugWarpController();
            
        if (addPlayerPositionUI)
            SetupPlayerPositionUI();
            
        if (addWarpSetup)
            SetupWarpSetup();
        
        if (showDebugLogs)
            Debug.Log($"✅ Scene setup complete for {sceneName}");
    }
    
    void SetupSceneSpawnResolver()
    {
        SceneSpawnResolver resolver = Object.FindFirstObjectByType<SceneSpawnResolver>();
        
        if (resolver == null)
        {
            GameObject resolverObj = new GameObject("SceneSpawnResolver");
            resolver = resolverObj.AddComponent<SceneSpawnResolver>();
            
            if (showDebugLogs)
                Debug.Log("✅ Created SceneSpawnResolver");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("ℹ️ SceneSpawnResolver already exists");
        }
    }
    
    void SetupDebugWarpController()
    {
        DebugWarpController debugController = Object.FindFirstObjectByType<DebugWarpController>();
        
        if (debugController == null)
        {
            GameObject debugObj = new GameObject("DebugWarpController");
            debugController = debugObj.AddComponent<DebugWarpController>();
            
            if (showDebugLogs)
                Debug.Log("✅ Created DebugWarpController");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("ℹ️ DebugWarpController already exists");
        }
    }
    
    void SetupPlayerPositionUI()
    {
        PlayerPositionUI positionUI = Object.FindFirstObjectByType<PlayerPositionUI>();
        
        if (positionUI == null)
        {
            GameObject uiObj = new GameObject("PlayerPositionUI");
            positionUI = uiObj.AddComponent<PlayerPositionUI>();
            
            if (showDebugLogs)
                Debug.Log("✅ Created PlayerPositionUI");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("ℹ️ PlayerPositionUI already exists");
        }
    }
    
    void SetupWarpSetup()
    {
        BlazedOdysseyWarpSetup warpSetup = Object.FindFirstObjectByType<BlazedOdysseyWarpSetup>();
        
        if (warpSetup == null)
        {
            GameObject warpObj = new GameObject("BlazedOdysseyWarpSetup");
            warpSetup = warpObj.AddComponent<BlazedOdysseyWarpSetup>();
            
            if (showDebugLogs)
                Debug.Log("✅ Created BlazedOdysseyWarpSetup");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("ℹ️ BlazedOdysseyWarpSetup already exists");
        }
    }
    
    [ContextMenu("Validate Scene Setup")]
    public void ValidateSceneSetup()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"🔍 Validating scene setup for: {sceneName}");
        
        // Check essential components
        bool hasResolver = Object.FindFirstObjectByType<SceneSpawnResolver>() != null;
        bool hasDebugController = Object.FindFirstObjectByType<DebugWarpController>() != null;
        bool hasPositionUI = Object.FindFirstObjectByType<PlayerPositionUI>() != null;
        bool hasWarpSetup = Object.FindFirstObjectByType<BlazedOdysseyWarpSetup>() != null;
        
        Debug.Log($"   SceneSpawnResolver: {(hasResolver ? "✅" : "❌")}");
        Debug.Log($"   DebugWarpController: {(hasDebugController ? "✅" : "❌")}");
        Debug.Log($"   PlayerPositionUI: {(hasPositionUI ? "✅" : "❌")}");
        Debug.Log($"   BlazedOdysseyWarpSetup: {(hasWarpSetup ? "✅" : "❌")}");
        
        // Check spawn points
        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"   Spawn points: {spawnPoints.Length}");
        foreach (var spawn in spawnPoints)
        {
            Debug.Log($"      • '{spawn.spawnName}' at {spawn.transform.position}");
        }
        
        // Check warp points
        WarpPoint[] warpPoints = Object.FindObjectsByType<WarpPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EnhancedWarpPoint[] enhancedWarps = Object.FindObjectsByType<EnhancedWarpPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int totalWarps = warpPoints.Length + enhancedWarps.Length;
        
        Debug.Log($"   Warp points: {totalWarps} (Regular: {warpPoints.Length}, Enhanced: {enhancedWarps.Length})");
        
        foreach (var warp in warpPoints)
        {
            Debug.Log($"      • To '{warp.targetScene}:{warp.targetSpawnPoint}' at {warp.transform.position}");
        }
        
        foreach (var warp in enhancedWarps)
        {
            Debug.Log($"      • To '{warp.targetScene}:{warp.targetSpawnPoint}' at {warp.transform.position}");
        }
        
        // Check player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player != null)
        {
            Debug.Log($"   Player: ✅ '{player.name}' at {player.transform.position}");
            
            // Check player collider
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Debug.Log($"      Collider: ✅ {playerCollider.GetType().Name} (IsTrigger: {playerCollider.isTrigger})");
            }
            else
            {
                Debug.LogWarning("      Collider: ❌ Player has no Collider2D");
            }
        }
        else
        {
            Debug.LogWarning("   Player: ❌ No player found");
        }
    }
}