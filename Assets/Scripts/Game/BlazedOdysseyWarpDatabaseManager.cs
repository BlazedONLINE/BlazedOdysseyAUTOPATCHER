using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Only include MultiplayerARPG reference if it exists
#if UNITY_MULTIPLAYER_ARPG
using MultiplayerARPG;
#endif

/// <summary>
/// Database-driven warp system for Blazed Odyssey MMO.
/// This integrates with Unity ARPG's built-in WarpPortalDatabase system,
/// inspired by Nexus TK's map transition approach.
/// </summary>
[CreateAssetMenu(fileName = "BlazedOdysseyWarpDatabase", menuName = "BlazedOdyssey/Warp Database Manager")]
public class BlazedOdysseyWarpDatabaseManager : ScriptableObject
{
    [Header("🗺️ Map Configurations")]
    [SerializeField] private WarpMapConfig[] mapConfigs;
    
    [Header("🚪 Debug Controls")]
    [SerializeField] private bool enableDebugWarps = true;
    [SerializeField] private KeyCode debugWarpToPoppyInn = KeyCode.Minus;
    [SerializeField] private KeyCode debugWarpToStarterMap = KeyCode.Equals;
    
    [System.Serializable]
    public class WarpMapConfig
    {
        [Header("Map Info")]
        public string mapName;
#if UNITY_MULTIPLAYER_ARPG
        public BaseMapInfo mapInfo;
#else
        // Placeholder to avoid ARPG dependency. Assign a relevant scene/map object as needed.
        public Object mapInfo;
#endif
        
        [Header("Warp Points (Nexus TK Style)")]
        public WarpLocation[] warpLocations;
        
        [Header("Spawn Points")]
        public SpawnLocation[] spawnLocations;
    }
    
    [System.Serializable]
    public class WarpLocation
    {
        [Header("Location Info")]
        public string warpName;
        public Vector3 position;
        public Vector2 triggerSize = new Vector2(1.5f, 1.5f);
        
        [Header("Destination (RTK Style)")]
        public string destinationMapName;
        public string destinationSpawnPoint;
        public Vector3 destinationPosition;
        
        [Header("Trigger Settings")]
        public bool requireInteraction = false;
        public KeyCode interactionKey = KeyCode.E;
        
        [Header("Visual")]
        public Color warpColor = Color.blue;
        public bool showVisual = true;
    }
    
    [System.Serializable]
    public class SpawnLocation
    {
        public string spawnName;
        public Vector3 position;
        public Vector3 rotation = Vector3.zero;
    }
    
    /// <summary>
    /// Get map configuration by name
    /// </summary>
    public WarpMapConfig GetMapConfig(string mapName)
    {
        foreach (var config in mapConfigs)
        {
            if (config.mapName == mapName)
                return config;
        }
        return null;
    }
    
    /// <summary>
    /// Get warp location by name in a specific map
    /// </summary>
    public WarpLocation GetWarpLocation(string mapName, string warpName)
    {
        var config = GetMapConfig(mapName);
        if (config != null)
        {
            foreach (var warp in config.warpLocations)
            {
                if (warp.warpName == warpName)
                    return warp;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get spawn location by name in a specific map
    /// </summary>
    public SpawnLocation GetSpawnLocation(string mapName, string spawnName)
    {
        var config = GetMapConfig(mapName);
        if (config != null)
        {
            foreach (var spawn in config.spawnLocations)
            {
                if (spawn.spawnName == spawnName)
                    return spawn;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Create default configurations for Blazed Odyssey maps
    /// </summary>
    [ContextMenu("Setup Blazed Odyssey Default Warps")]
    public void SetupDefaultWarps()
    {
        mapConfigs = new WarpMapConfig[]
        {
            // StarterMapScene Configuration
            new WarpMapConfig
            {
                mapName = "StarterMapScene",
                warpLocations = new WarpLocation[]
                {
                    new WarpLocation
                    {
                        warpName = "PoppyInnEntrance",
                        position = new Vector3(9.791413f, -9.003255f, 0f),
                        triggerSize = new Vector2(1.5f, 1.5f),
                        destinationMapName = "PoppyInn",
                        destinationSpawnPoint = "FromStarterMap",
                        destinationPosition = new Vector3(0f, -2f, 0f),
                        requireInteraction = false,
                        warpColor = Color.blue,
                        showVisual = true
                    }
                },
                spawnLocations = new SpawnLocation[]
                {
                    new SpawnLocation
                    {
                        spawnName = "FromPoppyInn",
                        position = new Vector3(9.791413f, -8.003255f, 0f),
                        rotation = Vector3.zero
                    }
                }
            },
            
            // PoppyInn Configuration
            new WarpMapConfig
            {
                mapName = "PoppyInn",
                warpLocations = new WarpLocation[]
                {
                    new WarpLocation
                    {
                        warpName = "ExitToStarterMap",
                        position = new Vector3(0f, -3f, 0f),
                        triggerSize = new Vector2(1.5f, 1.5f),
                        destinationMapName = "StarterMapScene",
                        destinationSpawnPoint = "FromPoppyInn",
                        destinationPosition = new Vector3(9.791413f, -8.003255f, 0f),
                        requireInteraction = false,
                        warpColor = Color.green,
                        showVisual = true
                    }
                },
                spawnLocations = new SpawnLocation[]
                {
                    new SpawnLocation
                    {
                        spawnName = "FromStarterMap",
                        position = new Vector3(0f, -2f, 0f),
                        rotation = Vector3.zero
                    }
                }
            }
        };
        
        Debug.Log("✅ Setup default Blazed Odyssey warp configurations");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}