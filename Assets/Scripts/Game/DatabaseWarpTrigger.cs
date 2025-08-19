using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Database warp trigger component.
/// Handles individual warp point triggers created from the database configuration.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DatabaseWarpTrigger : MonoBehaviour
{
    private BlazedOdysseyWarpDatabaseManager.WarpLocation warpConfig;
    private bool playerInRange = false;
    private GameObject detectedPlayer = null;
    private bool showDebugLogs = true;
    private SpriteRenderer visualRenderer;
    
    public void Initialize(BlazedOdysseyWarpDatabaseManager.WarpLocation config, bool enableLogs)
    {
        warpConfig = config;
        showDebugLogs = enableLogs;
        
        // Setup collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = config.triggerSize;
        
        // Get visual renderer if it exists
        visualRenderer = GetComponent<SpriteRenderer>();
        
        if (showDebugLogs)
        {
            Debug.Log($"🚪 Database warp trigger initialized: {config.warpName}");
            Debug.Log($"   Destination: {config.destinationMapName}:{config.destinationSpawnPoint}");
            Debug.Log($"   Position: {transform.position}");
            Debug.Log($"   Requires interaction: {config.requireInteraction}");
        }
    }
    
    void Update()
    {
        // Handle interaction if required
        if (playerInRange && warpConfig.requireInteraction && Input.GetKeyDown(warpConfig.interactionKey))
        {
            if (showDebugLogs)
                Debug.Log($"🎯 Player pressed {warpConfig.interactionKey} at warp: {warpConfig.warpName}");
            
            ExecuteWarp();
        }
        
        // Update visual feedback
        if (visualRenderer != null)
        {
            float alpha = playerInRange ? 0.7f : 0.4f;
            Color currentColor = visualRenderer.color;
            visualRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = true;
            detectedPlayer = other.gameObject;
            
            if (showDebugLogs)
                Debug.Log($"🚶 Player '{other.name}' entered warp: {warpConfig.warpName}");
            
            // Auto-warp if no interaction required
            if (!warpConfig.requireInteraction)
            {
                if (showDebugLogs)
                    Debug.Log($"🚪 Auto-warping (no interaction required)");
                
                ExecuteWarp();
            }
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = true;
            detectedPlayer = other.gameObject;
            
            // Check for interaction key if required
            if (warpConfig.requireInteraction && Input.GetKeyDown(warpConfig.interactionKey))
            {
                if (showDebugLogs)
                    Debug.Log($"🎯 Player pressed {warpConfig.interactionKey} in warp: {warpConfig.warpName}");
                
                ExecuteWarp();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = false;
            detectedPlayer = null;
            
            if (showDebugLogs)
                Debug.Log($"🚶 Player '{other.name}' left warp: {warpConfig.warpName}");
        }
    }
    
    bool IsPlayer(GameObject obj)
    {
        // Multi-method player detection
        
        // Method 1: Check by Player tag
        if (obj.CompareTag("Player"))
        {
            if (showDebugLogs)
                Debug.Log($"✅ Detected player by tag: {obj.name}");
            return true;
        }
        
        // Method 2: Check by name containing "Player"
        if (obj.name.ToLower().Contains("player"))
        {
            if (showDebugLogs)
                Debug.Log($"✅ Detected player by name: {obj.name}");
            return true;
        }
        
        // Method 3: Check for SPUM components
        if (obj.GetComponent<SPUM_Prefabs>() != null || obj.GetComponent<SPUMCharacterController>() != null)
        {
            if (showDebugLogs)
                Debug.Log($"✅ Detected SPUM character: {obj.name}");
            return true;
        }
        
        // Method 4: Check for Unity ARPG player character (only if ARPG present)
        #if UNITY_MULTIPLAYER_ARPG
        if (obj.GetComponent<MultiplayerARPG.BasePlayerCharacterEntity>() != null)
        {
            if (showDebugLogs)
                Debug.Log($"✅ Detected Unity ARPG player: {obj.name}");
            return true;
        }
        #endif
        
        return false;
    }
    
    void ExecuteWarp()
    {
        if (warpConfig.destinationMapName == null || warpConfig.destinationMapName == "")
        {
            Debug.LogError($"❌ Cannot warp: destination map is empty for {warpConfig.warpName}");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🌟 EXECUTING DATABASE WARP: {warpConfig.warpName}");
            Debug.Log($"   From: {SceneManager.GetActiveScene().name}");
            Debug.Log($"   To: {warpConfig.destinationMapName}:{warpConfig.destinationSpawnPoint}");
            Debug.Log($"   Player: {(detectedPlayer ? detectedPlayer.name : "Unknown")}");
        }
        
        // Set spawn point for destination
        GameState.NextSpawnPointName = warpConfig.destinationSpawnPoint;
        
        // Load destination scene
        SceneManager.LoadScene(warpConfig.destinationMapName);
    }
    
    // Visual debug gizmos
    void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = playerInRange ? Color.green : (warpConfig.warpColor != Color.clear ? warpConfig.warpColor : Color.yellow);
            Gizmos.DrawWireCube(transform.position, new Vector3(col.size.x, col.size.y, 0.1f));
            
            #if UNITY_EDITOR
            if (warpConfig.destinationMapName != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, 
                    $"DATABASE WARP\\n{warpConfig.warpName}\\n→ {warpConfig.destinationMapName}\\n{warpConfig.destinationSpawnPoint}");
            }
            #endif
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // Draw line to destination if in same scene (for debugging)
        if (!string.IsNullOrEmpty(warpConfig.destinationSpawnPoint))
        {
            GameObject targetSpawn = GameObject.Find($"DatabaseSpawn_{warpConfig.destinationSpawnPoint}");
            if (targetSpawn != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, targetSpawn.transform.position);
            }
        }
    }
}