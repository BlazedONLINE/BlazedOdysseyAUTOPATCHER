using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple F7 teleport system between StarterMapScene and PoppyInn
/// Replaces all the complex warp systems with a single key toggle
/// </summary>
public class F7TeleportController : MonoBehaviour
{
    [Header("F7 Teleport Settings")]
    [SerializeField] private KeyCode teleportKey = KeyCode.F7;
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private bool enableTeleport = true;
    
    [Header("Scene Configuration")]
    [SerializeField] private string starterMapScene = "StarterMapScene";
    [SerializeField] private string poppyInnScene = "PoppyInn";
    
    [Header("Player Spawn Positions")]
    [SerializeField] private Vector3 starterMapSpawnPos = new Vector3(9.791413f, -8.003255f, 0f);
    [SerializeField] private Vector3 poppyInnSpawnPos = new Vector3(0f, -2f, 0f);
    
    [Header("Spawn Point Names")]
    [SerializeField] private string starterMapSpawnName = "FromPoppyInn";
    [SerializeField] private string poppyInnSpawnName = "FromStarterMap";
    
    private string currentScene;
    private GameObject player;
    
    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🎮 F7TeleportController initialized in scene: {currentScene}");
        Debug.Log($"🔧 Press F7 to teleport between maps");
        
        // Find player
        FindPlayer();
        
        // Handle spawn positioning if needed
        HandleSceneSpawn();
    }
    
    void Update()
    {
        if (!enableTeleport) return;
        
        if (Input.GetKeyDown(teleportKey))
        {
            Debug.Log("🔥 F7 key pressed! Attempting teleport...");
            TeleportToOtherMap();
        }
        
        // Debug: Simple key detection
        if (Input.GetKeyDown(KeyCode.F7))
        {
            Debug.Log("🔥 F7 detected via direct check!");
            TeleportToOtherMap();
        }
    }
    
    void FindPlayer()
    {
        // Look for player by tag first
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            // Look for SPUM character
            SPUM_Prefabs spum = Object.FindFirstObjectByType<SPUM_Prefabs>();
            if (spum != null)
            {
                player = spum.gameObject;
                if (!player.CompareTag("Player"))
                {
                    player.tag = "Player";
                    Debug.Log($"✅ Tagged SPUM character as Player: {player.name}");
                }
            }
        }
        
        if (player == null)
        {
            Debug.LogWarning("⚠️ No player found! Teleport may not work properly.");
        }
        else
        {
            Debug.Log($"✅ Player found: {player.name} at {player.transform.position}");
        }
    }
    
    void HandleSceneSpawn()
    {
        // Position player based on which scene we're in and the spawn point name
        if (player == null) return;
        
        string expectedSpawnPoint = GameState.NextSpawnPointName;
        
        if (!string.IsNullOrEmpty(expectedSpawnPoint))
        {
            // Try to find the spawn point object first
            SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var spawn in spawnPoints)
            {
                if (spawn.spawnName == expectedSpawnPoint)
                {
                    player.transform.position = spawn.transform.position;
                    Debug.Log($"🎯 Positioned player at spawn point '{expectedSpawnPoint}': {spawn.transform.position}");
                    GameState.NextSpawnPointName = ""; // Clear after use
                    return;
                }
            }
            
            // Fallback to hardcoded positions if spawn points don't exist
            if (currentScene == starterMapScene && expectedSpawnPoint == starterMapSpawnName)
            {
                player.transform.position = starterMapSpawnPos;
                Debug.Log($"🎯 Positioned player at StarterMap fallback spawn: {starterMapSpawnPos}");
            }
            else if (currentScene == poppyInnScene && expectedSpawnPoint == poppyInnSpawnName)
            {
                player.transform.position = poppyInnSpawnPos;
                Debug.Log($"🎯 Positioned player at PoppyInn fallback spawn: {poppyInnSpawnPos}");
            }
            
            GameState.NextSpawnPointName = ""; // Clear after use
        }
    }
    
    [ContextMenu("Teleport to Other Map")]
    public void TeleportToOtherMap()
    {
        string targetScene = GetTargetScene();
        
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("❌ Cannot determine target scene!");
            return;
        }
        
        Debug.Log($"🚪 F7 Teleport: {currentScene} → {targetScene}");
        
        // Set the spawn point for the target scene
        if (targetScene == starterMapScene)
        {
            GameState.NextSpawnPointName = starterMapSpawnName;
        }
        else if (targetScene == poppyInnScene)
        {
            GameState.NextSpawnPointName = poppyInnSpawnName;
        }
        
        // Load target scene
        SceneManager.LoadScene(targetScene);
    }
    
    string GetTargetScene()
    {
        if (currentScene == starterMapScene)
        {
            return poppyInnScene;
        }
        else if (currentScene == poppyInnScene)
        {
            return starterMapScene;
        }
        else
        {
            // Default fallback - if we're in an unknown scene, go to starter map
            Debug.LogWarning($"⚠️ Unknown scene '{currentScene}', defaulting to StarterMap");
            return starterMapScene;
        }
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 120, 350, 110));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🎮 F7 Teleport Controller", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Current Scene: {currentScene}");
        GUILayout.Label($"Target: {GetTargetScene()}");
        GUILayout.Label($"Player: {(player != null ? player.name : "Not Found")}");
        GUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("F7 Teleport"))
        {
            TeleportToOtherMap();
        }
        
        if (GUILayout.Button("Find Player"))
        {
            FindPlayer();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Manual teleport to specific scene (for testing)
    /// </summary>
    public void TeleportToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name is empty!");
            return;
        }
        
        Debug.Log($"🚪 Manual teleport to: {sceneName}");
        
        // Set appropriate spawn point
        if (sceneName == starterMapScene)
        {
            GameState.NextSpawnPointName = starterMapSpawnName;
        }
        else if (sceneName == poppyInnScene)
        {
            GameState.NextSpawnPointName = poppyInnSpawnName;
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    [ContextMenu("Teleport to StarterMap")]
    public void TeleportToStarterMap()
    {
        TeleportToScene(starterMapScene);
    }
    
    [ContextMenu("Teleport to PoppyInn")]
    public void TeleportToPoppyInn()
    {
        TeleportToScene(poppyInnScene);
    }
}

// Using existing GameState class from Scripts/Gameplay/Warp/GameState.cs