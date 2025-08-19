using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Debug warp controller for easy testing of scene transitions.
/// Use - key to warp to Poppy Inn, = key to warp back to starting map.
/// </summary>
public class DebugWarpController : MonoBehaviour
{
    [Header("Debug Warp Settings")]
    [SerializeField] private bool enableDebugWarps = false;
    [SerializeField] private bool showDebugUI = false;
    [SerializeField] private KeyCode warpToPoppyInnKey = KeyCode.Minus;
    [SerializeField] private KeyCode warpToStarterMapKey = KeyCode.Equals;
    
    [Header("Scene Names")]
    [SerializeField] private string starterMapScene = "StarterMapScene";
    [SerializeField] private string poppyInnScene = "PoppyInn";
    
    [Header("Spawn Points")]
    [SerializeField] private string starterMapSpawnPoint = "FromPoppyInn";
    [SerializeField] private string poppyInnSpawnPoint = "FromStarterMap";
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugLogs = true;
    
    private string currentScene;
    
    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        
        if (showDebugLogs)
        {
            Debug.Log($"🎮 DebugWarpController initialized in scene: {currentScene}");
            Debug.Log($"🔧 Press '{warpToPoppyInnKey}' to warp to Poppy Inn");
            Debug.Log($"🔧 Press '{warpToStarterMapKey}' to warp to Starter Map");
        }
    }
    
    void Update()
    {
        if (!enableDebugWarps) return;
        
        HandleDebugWarpInput();
    }
    
    void HandleDebugWarpInput()
    {
        // Warp to Poppy Inn
        if (Input.GetKeyDown(warpToPoppyInnKey))
        {
            WarpToPoppyInn();
        }
        
        // Warp to Starter Map
        if (Input.GetKeyDown(warpToStarterMapKey))
        {
            WarpToStarterMap();
        }
    }
    
    [ContextMenu("Warp to Poppy Inn")]
    public void WarpToPoppyInn()
    {
        if (showDebugLogs)
            Debug.Log($"🚪 Debug warp to Poppy Inn from {currentScene}");
            
        GameState.NextSpawnPointName = poppyInnSpawnPoint;
        SceneManager.LoadScene(poppyInnScene);
    }
    
    [ContextMenu("Warp to Starter Map")]
    public void WarpToStarterMap()
    {
        if (showDebugLogs)
            Debug.Log($"🚪 Debug warp to Starter Map from {currentScene}");
            
        GameState.NextSpawnPointName = starterMapSpawnPoint;
        SceneManager.LoadScene(starterMapScene);
    }
    
    /// <summary>
    /// Force warp to specific scene and spawn point
    /// </summary>
    public void WarpToScene(string sceneName, string spawnPointName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Cannot warp: scene name is empty");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log($"🚪 Debug warp to {sceneName}:{spawnPointName} from {currentScene}");
            
        GameState.NextSpawnPointName = spawnPointName;
        SceneManager.LoadScene(sceneName);
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Debug Warp Controller", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Current Scene: {currentScene}");
        GUILayout.Space(5);
        
        GUILayout.Label("Debug Warps:");
        GUILayout.Label($"  {warpToPoppyInnKey} = Warp to Poppy Inn");
        GUILayout.Label($"  {warpToStarterMapKey} = Warp to Starter Map");
        GUILayout.Space(5);
        
        // Manual warp buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("To Poppy Inn"))
        {
            WarpToPoppyInn();
        }
        if (GUILayout.Button("To Starter Map"))
        {
            WarpToStarterMap();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Check if scenes exist and are properly named
    /// </summary>
    [ContextMenu("Validate Scene Setup")]
    public void ValidateSceneSetup()
    {
        Debug.Log("🔍 Validating scene setup...");
        
        // Check current scene
        Debug.Log($"Current scene: {currentScene}");
        
        // Check GameState
        Debug.Log($"GameState.NextSpawnPointName: '{GameState.NextSpawnPointName}'");
        
        // Check for spawn points in current scene
        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {spawnPoints.Length} spawn points in current scene:");
        
        foreach (var spawn in spawnPoints)
        {
            Debug.Log($"   • Spawn point: '{spawn.spawnName}' at {spawn.transform.position}");
        }
        
        // Check for warp points in current scene
        WarpPoint[] warpPoints = Object.FindObjectsByType<WarpPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {warpPoints.Length} warp points in current scene:");
        
        foreach (var warp in warpPoints)
        {
            Debug.Log($"   • Warp to: '{warp.targetScene}:{warp.targetSpawnPoint}' at {warp.transform.position}");
        }
        
        // Check for player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"✅ Player found: {player.name} at {player.transform.position}");
        }
        else
        {
            Debug.LogWarning("⚠️ No player found with 'Player' tag");
        }
    }
}