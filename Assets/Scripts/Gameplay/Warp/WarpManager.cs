using UnityEngine;

/// <summary>
/// Helper script to create and manage warps easily.
/// Use this to set up warps between scenes programmatically.
/// </summary>
public class WarpManager : MonoBehaviour
{
    [Header("Auto-Setup Warps")]
    [SerializeField] private bool createWarpsOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Warp Configuration")]
    [SerializeField] private WarpData[] warpsToCreate;
    
    [System.Serializable]
    public class WarpData
    {
        [Header("Warp Settings")]
        public string warpName = "New Warp";
        public Vector3 warpPosition = Vector3.zero;
        public Vector2 warpSize = new Vector2(1f, 1f);
        
        [Header("Destination")]
        public string targetScene = "";
        public string targetSpawnPoint = "";
        
        [Header("Trigger")]
        public bool requireButton = false;
        public KeyCode triggerKey = KeyCode.E;
        
        [Header("Visual")]
        public bool showGizmo = true;
        public Color gizmoColor = Color.yellow;
    }
    
    void Start()
    {
        if (createWarpsOnStart)
        {
            CreateAllWarps();
        }
    }
    
    [ContextMenu("Create All Warps")]
    public void CreateAllWarps()
    {
        if (warpsToCreate == null || warpsToCreate.Length == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("⚠️ No warps configured to create");
            return;
        }
        
        int created = 0;
        
        foreach (var warpData in warpsToCreate)
        {
            if (CreateWarp(warpData))
                created++;
        }
        
        if (showDebugLogs)
            Debug.Log($"✅ WarpManager: Created {created}/{warpsToCreate.Length} warps");
    }
    
    bool CreateWarp(WarpData data)
    {
        if (string.IsNullOrEmpty(data.targetScene))
        {
            if (showDebugLogs)
                Debug.LogWarning($"⚠️ Skipping warp '{data.warpName}' - no target scene specified");
            return false;
        }
        
        // Check if warp already exists
        string warpObjectName = $"Warp_To_{data.targetScene}_{data.targetSpawnPoint}";
        if (GameObject.Find(warpObjectName) != null)
        {
            if (showDebugLogs)
                Debug.Log($"ℹ️ Warp '{warpObjectName}' already exists");
            return false;
        }
        
        // Create warp GameObject
        GameObject warpObj = new GameObject(warpObjectName);
        warpObj.transform.position = data.warpPosition;
        warpObj.transform.SetParent(transform);
        
        // Add BoxCollider2D
        BoxCollider2D collider = warpObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = data.warpSize;
        
        // Add WarpPoint component
        WarpPoint warpPoint = warpObj.AddComponent<WarpPoint>();
        warpPoint.targetScene = data.targetScene;
        warpPoint.targetSpawnPoint = data.targetSpawnPoint;
        warpPoint.requireButton = data.requireButton;
        warpPoint.triggerKey = data.triggerKey;
        
        // Add visual feedback (optional)
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
        
        return true;
    }
    
    [ContextMenu("Create Spawn Point Here")]
    public void CreateSpawnPointAtPosition()
    {
        CreateSpawnPoint("DefaultSpawn", transform.position);
    }
    
    public GameObject CreateSpawnPoint(string spawnName, Vector3 position)
    {
        string spawnObjectName = $"SpawnPoint_{spawnName}";
        
        // Check if spawn point already exists
        if (GameObject.Find(spawnObjectName) != null)
        {
            if (showDebugLogs)
                Debug.Log($"ℹ️ Spawn point '{spawnObjectName}' already exists");
            return null;
        }
        
        // Create spawn point GameObject
        GameObject spawnObj = new GameObject(spawnObjectName);
        spawnObj.transform.position = position;
        spawnObj.transform.SetParent(transform);
        
        // Add SpawnPoint component
        SpawnPoint spawnPoint = spawnObj.AddComponent<SpawnPoint>();
        spawnPoint.spawnName = spawnName;
        
        // Add visual indicator (optional)
        var renderer = spawnObj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0, 1, 0, 0.5f); // Green with transparency
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        renderer.sprite = sprite;
        renderer.sortingOrder = 10; // In front of everything
        
        if (showDebugLogs)
            Debug.Log($"✅ Created spawn point '{spawnName}' at {position}");
        
        return spawnObj;
    }
    
    void OnDrawGizmos()
    {
        if (warpsToCreate == null) return;
        
        foreach (var warpData in warpsToCreate)
        {
            if (warpData.showGizmo)
            {
                Gizmos.color = warpData.gizmoColor;
                Gizmos.DrawWireCube(warpData.warpPosition, new Vector3(warpData.warpSize.x, warpData.warpSize.y, 0.1f));
                
                // Draw label
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(warpData.warpPosition + Vector3.up * 0.5f, 
                    $"{warpData.warpName}\n→ {warpData.targetScene}");
                #endif
            }
        }
    }
}