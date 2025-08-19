using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhanced warp point with better debugging and multiple detection methods.
/// This version is more robust and can detect players even if they don't have the exact tag.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class EnhancedWarpPoint : MonoBehaviour
{
    [Header("Destination")]
    public string targetScene;
    public string targetSpawnPoint;
    
    [Header("Trigger")]
    public bool requireButton = false;
    public KeyCode triggerKey = KeyCode.E;
    
    [Header("Detection")]
    [SerializeField] private bool detectByTag = true;
    [SerializeField] private bool detectByName = true;
    [SerializeField] private bool detectSPUMCharacters = true;
    [SerializeField] private string[] playerTags = { "Player" };
    [SerializeField] private string[] playerNames = { "Player" };
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showVisualFeedback = true;
    [SerializeField] private Color warpColor = Color.yellow;
    
    private bool playerInRange = false;
    private GameObject detectedPlayer = null;
    private SpriteRenderer visualRenderer;
    
    private void Start()
    {
        SetupCollider();
        SetupVisualFeedback();
        
        if (showDebugLogs)
        {
            Debug.Log($"🚪 Enhanced Warp Point '{gameObject.name}' ready");
            Debug.Log($"   Target: {targetScene}:{targetSpawnPoint}");
            Debug.Log($"   Position: {transform.position}");
            Debug.Log($"   Require button: {requireButton} ({triggerKey})");
        }
    }
    
    private void SetupCollider()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        
        // Ensure collider is large enough
        if (col.size.magnitude < 0.5f)
        {
            col.size = Vector2.one;
            if (showDebugLogs)
                Debug.Log($"⚠️ Increased warp collider size to {col.size}");
        }
    }
    
    private void SetupVisualFeedback()
    {
        if (!showVisualFeedback) return;
        
        visualRenderer = GetComponent<SpriteRenderer>();
        if (visualRenderer == null)
        {
            visualRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // Create a simple sprite
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            visualRenderer.sprite = sprite;
        }
        
        visualRenderer.color = new Color(warpColor.r, warpColor.g, warpColor.b, 0.3f);
        visualRenderer.sortingOrder = -1;
    }
    
    private void Update()
    {
        if (playerInRange && requireButton && Input.GetKeyDown(triggerKey))
        {
            if (showDebugLogs)
                Debug.Log($"🎯 Player pressed {triggerKey} in warp zone");
            Warp();
        }
        
        // Visual feedback
        if (visualRenderer != null)
        {
            float alpha = playerInRange ? 0.6f : 0.3f;
            visualRenderer.color = new Color(warpColor.r, warpColor.g, warpColor.b, alpha);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = true;
            detectedPlayer = other.gameObject;
            
            if (showDebugLogs)
                Debug.Log($"🚶 Player '{other.name}' entered warp zone at {transform.position}");
            
            if (!requireButton)
            {
                if (showDebugLogs)
                    Debug.Log($"🚪 Auto-warping (no button required)");
                Warp();
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = true;
            detectedPlayer = other.gameObject;
            
            // Check for button press if required
            if (requireButton && Input.GetKeyDown(triggerKey))
            {
                if (showDebugLogs)
                    Debug.Log($"🎯 Player pressed {triggerKey} while in warp zone");
                Warp();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            playerInRange = false;
            detectedPlayer = null;
            
            if (showDebugLogs)
                Debug.Log($"🚶 Player '{other.name}' left warp zone");
        }
    }
    
    private bool IsPlayer(GameObject obj)
    {
        // Method 1: Check by tag
        if (detectByTag)
        {
            foreach (string tag in playerTags)
            {
                if (obj.CompareTag(tag))
                {
                    if (showDebugLogs)
                        Debug.Log($"✅ Detected player by tag: {tag}");
                    return true;
                }
            }
        }
        
        // Method 2: Check by name
        if (detectByName)
        {
            foreach (string name in playerNames)
            {
                if (obj.name.Contains(name))
                {
                    if (showDebugLogs)
                        Debug.Log($"✅ Detected player by name: {obj.name}");
                    return true;
                }
            }
        }
        
        // Method 3: Check for SPUM characters
        if (detectSPUMCharacters)
        {
            if (obj.GetComponent<SPUM_Prefabs>() != null || 
                obj.GetComponent<SPUMCharacterController>() != null)
            {
                if (showDebugLogs)
                    Debug.Log($"✅ Detected SPUM character: {obj.name}");
                return true;
            }
        }
        
        return false;
    }
    
    public void Warp()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError($"❌ Cannot warp: target scene is empty on {gameObject.name}");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🌟 WARPING from {SceneManager.GetActiveScene().name} to {targetScene}");
            Debug.Log($"   Spawn point: {targetSpawnPoint}");
            Debug.Log($"   Player: {(detectedPlayer ? detectedPlayer.name : "Unknown")}");
        }
        
        GameState.NextSpawnPointName = targetSpawnPoint;
        SceneManager.LoadScene(targetScene);
    }
    
    // Manual warp for testing
    [ContextMenu("Test Warp")]
    public void TestWarp()
    {
        Debug.Log($"🧪 Testing warp from {gameObject.name}");
        Warp();
    }
    
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = playerInRange ? Color.green : warpColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(col.size.x, col.size.y, 0.1f));
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                $"WARP\n→ {targetScene}\n{targetSpawnPoint}");
            #endif
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}