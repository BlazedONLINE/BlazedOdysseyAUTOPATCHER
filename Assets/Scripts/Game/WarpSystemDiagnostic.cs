using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Comprehensive diagnostic and fix tool for the warp system.
/// This will identify and fix all warp-related issues.
/// </summary>
public class WarpSystemDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runDiagnosticsOnStart = true;
    [SerializeField] private bool autoFixIssues = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Manual Testing")]
    [SerializeField] private KeyCode diagnosticKey = KeyCode.None;
    [SerializeField] private KeyCode forceWarpToPoppyKey = KeyCode.F2;
    [SerializeField] private KeyCode forceWarpToStarterKey = KeyCode.F3;
    
    void Start()
    {
        if (runDiagnosticsOnStart)
        {
            Invoke("RunCompleteDiagnostic", 1f); // Wait a frame for everything to initialize
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunCompleteDiagnostic();
        }
        
        if (Input.GetKeyDown(forceWarpToPoppyKey))
        {
            ForceWarpToPoppyInn();
        }
        
        if (Input.GetKeyDown(forceWarpToStarterKey))
        {
            ForceWarpToStarter();
        }
    }
    
    [ContextMenu("Run Complete Diagnostic")]
    public void RunCompleteDiagnostic()
    {
        Debug.Log("🔧 =================== WARP SYSTEM DIAGNOSTIC ===================");
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Current Scene: {currentScene}");
        
        // 1. Check critical components
        CheckDebugWarpController();
        CheckSpawnResolver();
        CheckPlayer();
        CheckWarps();
        CheckSpawnPoints();
        
        // 2. Auto-fix if enabled
        if (autoFixIssues)
        {
            Debug.Log("🔧 Auto-fixing detected issues...");
            AutoFixIssues();
        }
        
        Debug.Log("🔧 =================== DIAGNOSTIC COMPLETE ===================");
    }
    
    void CheckDebugWarpController()
    {
        DebugWarpController debugController = Object.FindFirstObjectByType<DebugWarpController>();
        
        if (debugController == null)
        {
            Debug.LogWarning("❌ ISSUE: No DebugWarpController found in scene");
            
            if (autoFixIssues)
            {
                CreateDebugWarpController();
            }
        }
        else
        {
            Debug.Log($"✅ DebugWarpController found: {debugController.name}");
        }
    }
    
    void CheckSpawnResolver()
    {
        SceneSpawnResolver resolver = Object.FindFirstObjectByType<SceneSpawnResolver>();
        
        if (resolver == null)
        {
            Debug.LogWarning("❌ ISSUE: No SceneSpawnResolver found in scene");
            
            if (autoFixIssues)
            {
                CreateSceneSpawnResolver();
            }
        }
        else
        {
            Debug.Log($"✅ SceneSpawnResolver found: {resolver.name}");
        }
    }
    
    void CheckPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogWarning("❌ ISSUE: No player found with 'Player' tag");
            
            // Try to find SPUM characters
            SPUM_Prefabs[] spumChars = Object.FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (spumChars.Length > 0)
            {
                Debug.Log($"🔍 Found {spumChars.Length} SPUM characters, checking tags...");
                
                foreach (var spum in spumChars)
                {
                    Debug.Log($"   • SPUM character: {spum.name} (tag: {spum.tag})");
                    
                    if (autoFixIssues && !spum.CompareTag("Player"))
                    {
                        spum.tag = "Player";
                        Debug.Log($"✅ Fixed: Set {spum.name} tag to 'Player'");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"✅ Player found: {player.name} at {player.transform.position}");
            
            // Check player collider
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider == null)
            {
                Debug.LogWarning("❌ ISSUE: Player has no Collider2D");
                
                if (autoFixIssues)
                {
                    BoxCollider2D newCollider = player.AddComponent<BoxCollider2D>();
                    newCollider.size = new Vector2(0.8f, 0.8f);
                    newCollider.isTrigger = false;
                    Debug.Log("✅ Fixed: Added BoxCollider2D to player");
                }
            }
            else
            {
                Debug.Log($"✅ Player collider: {playerCollider.GetType().Name} (IsTrigger: {playerCollider.isTrigger})");
            }
        }
    }
    
    void CheckWarps()
    {
        WarpPoint[] basicWarps = Object.FindObjectsByType<WarpPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EnhancedWarpPoint[] enhancedWarps = Object.FindObjectsByType<EnhancedWarpPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        Debug.Log($"📍 Warp Points Found: {basicWarps.Length} basic, {enhancedWarps.Length} enhanced");
        
        foreach (var warp in basicWarps)
        {
            Debug.Log($"   • Basic Warp: {warp.name} -> {warp.targetScene}:{warp.targetSpawnPoint}");
            Debug.Log($"     Position: {warp.transform.position}, Require Button: {warp.requireButton}");
        }
        
        foreach (var warp in enhancedWarps)
        {
            Debug.Log($"   • Enhanced Warp: {warp.name} -> {warp.targetScene}:{warp.targetSpawnPoint}");
            Debug.Log($"     Position: {warp.transform.position}, Require Button: {warp.requireButton}");
        }
        
        if (basicWarps.Length == 0 && enhancedWarps.Length == 0)
        {
            Debug.LogWarning("❌ ISSUE: No warp points found in scene");
        }
    }
    
    void CheckSpawnPoints()
    {
        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        Debug.Log($"📍 Spawn Points Found: {spawnPoints.Length}");
        
        foreach (var spawn in spawnPoints)
        {
            Debug.Log($"   • Spawn Point: '{spawn.spawnName}' at {spawn.transform.position}");
        }
        
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("❌ ISSUE: No spawn points found in scene");
        }
    }
    
    void AutoFixIssues()
    {
        // Ensure essential warp setup exists
        BlazedOdysseyWarpSetup warpSetup = Object.FindFirstObjectByType<BlazedOdysseyWarpSetup>();
        if (warpSetup == null)
        {
            CreateBlazedOdysseyWarpSetup();
        }
        
        // Force setup warps
        if (warpSetup != null)
        {
            warpSetup.SetupBlazedOdysseyWarps();
        }
    }
    
    void CreateDebugWarpController()
    {
        GameObject debugObj = new GameObject("DebugWarpController");
        debugObj.AddComponent<DebugWarpController>();
        Debug.Log("✅ Created DebugWarpController");
    }
    
    void CreateSceneSpawnResolver()
    {
        GameObject resolverObj = new GameObject("SceneSpawnResolver");
        resolverObj.AddComponent<SceneSpawnResolver>();
        Debug.Log("✅ Created SceneSpawnResolver");
    }
    
    void CreateBlazedOdysseyWarpSetup()
    {
        GameObject warpSetupObj = new GameObject("BlazedOdysseyWarpSetup");
        warpSetupObj.AddComponent<BlazedOdysseyWarpSetup>();
        Debug.Log("✅ Created BlazedOdysseyWarpSetup");
    }
    
    [ContextMenu("Force Warp to Poppy Inn")]
    public void ForceWarpToPoppyInn()
    {
        Debug.Log("🚪 FORCE WARP: Going to Poppy Inn");
        GameState.NextSpawnPointName = "FromStarterMap";
        SceneManager.LoadScene("PoppyInn");
    }
    
    [ContextMenu("Force Warp to Starter Map")]
    public void ForceWarpToStarter()
    {
        Debug.Log("🚪 FORCE WARP: Going to Starter Map");
        GameState.NextSpawnPointName = "FromPoppyInn";
        SceneManager.LoadScene("StarterMapScene");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🔧 Warp System Diagnostic", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
        GUILayout.Space(5);
        
        GUILayout.Label("Manual Controls:");
        GUILayout.Label($"  {diagnosticKey} = Run Diagnostic");
        GUILayout.Label($"  {forceWarpToPoppyKey} = Force Warp to Poppy Inn");
        GUILayout.Label($"  {forceWarpToStarterKey} = Force Warp to Starter");
        GUILayout.Space(5);
        
        if (GUILayout.Button("🔧 Run Diagnostic"))
        {
            RunCompleteDiagnostic();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("→ Poppy Inn"))
        {
            ForceWarpToPoppyInn();
        }
        if (GUILayout.Button("→ Starter Map"))
        {
            ForceWarpToStarter();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}