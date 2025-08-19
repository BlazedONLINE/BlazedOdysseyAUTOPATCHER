using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap script to ensure each scene has the necessary components
/// Automatically adds F7TeleportController and other essentials to scenes
/// </summary>
public class SceneSetupBootstrap : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool addF7Teleport = true;
    [SerializeField] private bool addCombatSoundManager = true;
    [SerializeField] private bool addPlayerHealth = true;
    [SerializeField] private bool fixPlayerTags = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (showDebugLogs)
            Debug.Log($"🚀 SceneSetupBootstrap: Setting up scene '{sceneName}'");
        
        if (addF7Teleport)
        {
            EnsureF7TeleportController();
        }
        
        if (addCombatSoundManager)
        {
            EnsureCombatSoundManager();
        }
        
        if (addPlayerHealth)
        {
            EnsurePlayerHealth();
        }
        
        if (fixPlayerTags)
        {
            FixPlayerTags();
        }
        
        // Scene-specific setup
        if (sceneName == "PoppyInn")
        {
            SetupPoppyInn();
        }
        else if (sceneName == "StarterMapScene")
        {
            SetupStarterMap();
        }
        
        if (showDebugLogs)
            Debug.Log($"✅ SceneSetupBootstrap: Scene '{sceneName}' setup complete");
    }
    
    void EnsureF7TeleportController()
    {
        if (Object.FindFirstObjectByType<F7TeleportController>() == null)
        {
            GameObject teleportObj = new GameObject("F7TeleportController");
            teleportObj.AddComponent<F7TeleportController>();
            
            if (showDebugLogs)
                Debug.Log("✅ Added F7TeleportController");
        }
    }
    
    void EnsureCombatSoundManager()
    {
        if (Object.FindFirstObjectByType<CombatSoundManager>() == null)
        {
            GameObject soundObj = new GameObject("CombatSoundManager");
            soundObj.AddComponent<CombatSoundManager>();
            
            if (showDebugLogs)
                Debug.Log("✅ Added CombatSoundManager");
        }
    }
    
    void EnsurePlayerHealth()
    {
        // Find player first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
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
                }
            }
        }
        
        if (player != null)
        {
            // Add PlayerHealth if missing
            if (player.GetComponent<PlayerHealth>() == null)
            {
                player.AddComponent<PlayerHealth>();
                if (showDebugLogs)
                    Debug.Log("✅ Added PlayerHealth to player");
            }
            
            // Add PlayerHealthUI if missing
            if (Object.FindFirstObjectByType<PlayerHealthUI>() == null)
            {
                GameObject healthUIObj = new GameObject("PlayerHealthUI");
                healthUIObj.AddComponent<PlayerHealthUI>();
                if (showDebugLogs)
                    Debug.Log("✅ Added PlayerHealthUI");
            }
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("⚠️ No player found for health system setup");
        }
    }
    
    void FixPlayerTags()
    {
        // Find SPUM characters and ensure they're tagged as Player
        SPUM_Prefabs[] spumChars = Object.FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var spum in spumChars)
        {
            if (!spum.CompareTag("Player"))
            {
                spum.tag = "Player";
                
                if (showDebugLogs)
                    Debug.Log($"✅ Tagged {spum.name} as Player");
            }
            
            // Ensure collider exists
            if (spum.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D col = spum.gameObject.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);
                col.isTrigger = false;
                
                if (showDebugLogs)
                    Debug.Log($"✅ Added collider to {spum.name}");
            }
        }
    }
    
    void SetupPoppyInn()
    {
        if (showDebugLogs)
            Debug.Log("🍺 Setting up Poppy Inn specific components...");
        
        // Ensure proper lighting for interior
        EnsureInteriorLighting();
        
        // Add any Poppy Inn specific components here
    }
    
    void SetupStarterMap()
    {
        if (showDebugLogs)
            Debug.Log("🏞️ Setting up Starter Map specific components...");
        
        // Ensure proper outdoor lighting
        EnsureOutdoorLighting();
        
        // Add any Starter Map specific components here
    }
    
    void EnsureInteriorLighting()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        if (lights.Length == 0)
        {
            GameObject lightObj = new GameObject("Interior Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f; // Slightly dimmer for interior
            light.color = new Color(1f, 0.95f, 0.8f); // Warm interior lighting
            lightObj.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            
            if (showDebugLogs)
                Debug.Log("💡 Added interior lighting");
        }
    }
    
    void EnsureOutdoorLighting()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        if (lights.Length == 0)
        {
            GameObject lightObj = new GameObject("Outdoor Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f; // Full brightness for outdoor
            light.color = Color.white; // Natural white light
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            if (showDebugLogs)
                Debug.Log("☀️ Added outdoor lighting");
        }
    }
    
    // Manual setup methods for testing
    [ContextMenu("Add F7 Teleport")]
    public void AddF7Teleport()
    {
        EnsureF7TeleportController();
    }
    
    [ContextMenu("Add Combat Sound Manager")]
    public void AddCombatSoundManager()
    {
        EnsureCombatSoundManager();
    }
    
    [ContextMenu("Fix Player Tags")]
    public void FixPlayerTagsManual()
    {
        FixPlayerTags();
    }
}