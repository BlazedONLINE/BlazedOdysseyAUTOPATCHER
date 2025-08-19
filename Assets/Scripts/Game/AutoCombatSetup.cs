using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically finds and adds combat components to player objects at runtime
/// This ensures players always have combat capabilities without manual setup
/// </summary>
public class AutoCombatSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    [SerializeField] private bool enableAutoSetup = true;
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private float delayBeforeSetup = 1f;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Detection Settings")]
    [SerializeField] private bool setupPlayerTaggedObjects = true;
    [SerializeField] private bool setupSPUMObjects = true;
    [SerializeField] private bool setupCharacterControllers = true;
    
    void Start()
    {
        if (enableAutoSetup && setupOnStart)
        {
            StartCoroutine(AutoSetupDelayed());
        }
    }
    
    IEnumerator AutoSetupDelayed()
    {
        yield return new WaitForSeconds(delayBeforeSetup);
        PerformAutoSetup();
    }
    
    /// <summary>
    /// Perform automatic combat setup on relevant objects
    /// </summary>
    [ContextMenu("Perform Auto Setup")]
    public void PerformAutoSetup()
    {
        if (!enableAutoSetup) return;
        
        if (enableDebugLogs)
            Debug.Log("🔧 Starting automatic combat setup...");
        
        int setupCount = 0;
        
        // Setup player-tagged objects
        if (setupPlayerTaggedObjects)
        {
            setupCount += SetupPlayerTaggedObjects();
        }
        
        // Setup SPUM objects
        if (setupSPUMObjects)
        {
            setupCount += SetupSPUMObjects();
        }
        
        // Setup objects with character controllers
        if (setupCharacterControllers)
        {
            setupCount += SetupCharacterControllerObjects();
        }
        
        if (enableDebugLogs)
            Debug.Log($"🔧 Auto combat setup complete: {setupCount} objects configured");
    }
    
    /// <summary>
    /// Setup combat on all Player-tagged objects
    /// </summary>
    int SetupPlayerTaggedObjects()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        int setupCount = 0;
        
        foreach (var player in players)
        {
            if (SetupCombatOnObject(player))
            {
                setupCount++;
                if (enableDebugLogs)
                    Debug.Log($"✅ Setup combat on Player: {player.name}");
            }
        }
        
        return setupCount;
    }
    
    /// <summary>
    /// Setup combat on all objects with SPUM components
    /// </summary>
    int SetupSPUMObjects()
    {
        var spumObjects = FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int setupCount = 0;
        
        foreach (var spumObj in spumObjects)
        {
            if (SetupCombatOnObject(spumObj.gameObject))
            {
                setupCount++;
                if (enableDebugLogs)
                    Debug.Log($"✅ Setup combat on SPUM object: {spumObj.name}");
            }
        }
        
        return setupCount;
    }
    
    /// <summary>
    /// Setup combat on all objects with SPUM character controllers
    /// </summary>
    int SetupCharacterControllerObjects()
    {
        var controllers = FindObjectsByType<SPUMCharacterController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int setupCount = 0;
        
        foreach (var controller in controllers)
        {
            if (SetupCombatOnObject(controller.gameObject))
            {
                setupCount++;
                if (enableDebugLogs)
                    Debug.Log($"✅ Setup combat on character controller: {controller.name}");
            }
        }
        
        return setupCount;
    }
    
    /// <summary>
    /// Setup combat components on a specific object
    /// </summary>
    bool SetupCombatOnObject(GameObject obj)
    {
        if (obj == null) return false;
        
        bool wasSetup = false;
        
        // Add SPUMCombatBootstrap if missing
        if (obj.GetComponent<SPUMCombatBootstrap>() == null)
        {
            obj.AddComponent<SPUMCombatBootstrap>();
            wasSetup = true;
        }
        
        // Ensure it has a character controller
        if (obj.GetComponent<SPUMCharacterController>() == null)
        {
            // Only add if it has SPUM components
            if (obj.GetComponent<SPUM_Prefabs>() != null)
            {
                obj.AddComponent<SPUMCharacterController>();
                wasSetup = true;
            }
        }
        
        // Ensure it has proper tag
        if (!obj.CompareTag("Player") && ShouldBePlayer(obj))
        {
            obj.tag = "Player";
            wasSetup = true;
        }
        
        return wasSetup;
    }
    
    /// <summary>
    /// Determine if an object should be tagged as Player
    /// </summary>
    bool ShouldBePlayer(GameObject obj)
    {
        // Has SPUM components and character controller
        return obj.GetComponent<SPUM_Prefabs>() != null && 
               obj.GetComponent<SPUMCharacterController>() != null;
    }
    
    /// <summary>
    /// Manual trigger for scene setup
    /// </summary>
    [ContextMenu("Setup All Players in Scene")]
    public void SetupAllPlayersInScene()
    {
        PerformAutoSetup();
    }
    
    /// <summary>
    /// Check what objects would be affected by auto setup
    /// </summary>
    [ContextMenu("Preview Auto Setup")]
    public void PreviewAutoSetup()
    {
        Debug.Log("🔍 Preview of Auto Combat Setup:");
        
        var players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"   Player-tagged objects: {players.Length}");
        foreach (var player in players)
        {
            bool hasBootstrap = player.GetComponent<SPUMCombatBootstrap>() != null;
            Debug.Log($"     • {player.name} {(hasBootstrap ? "[Has Combat]" : "[Needs Combat]")}");
        }
        
        var spumObjects = FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"   SPUM objects: {spumObjects.Length}");
        foreach (var spum in spumObjects)
        {
            bool hasBootstrap = spum.GetComponent<SPUMCombatBootstrap>() != null;
            Debug.Log($"     • {spum.name} {(hasBootstrap ? "[Has Combat]" : "[Needs Combat]")}");
        }
        
        var controllers = FindObjectsByType<SPUMCharacterController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"   Character controllers: {controllers.Length}");
        foreach (var controller in controllers)
        {
            bool hasBootstrap = controller.GetComponent<SPUMCombatBootstrap>() != null;
            Debug.Log($"     • {controller.name} {(hasBootstrap ? "[Has Combat]" : "[Needs Combat]")}");
        }
    }
    
    void OnValidate()
    {
        // Automatically perform setup when component is added or settings change in editor
        if (Application.isPlaying && enableAutoSetup)
        {
            StartCoroutine(AutoSetupDelayed());
        }
    }
}