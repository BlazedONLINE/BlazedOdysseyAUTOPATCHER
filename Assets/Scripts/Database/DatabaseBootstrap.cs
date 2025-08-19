using UnityEngine;

/// <summary>
/// Ensures database systems are initialized when the game starts
/// Add this to a persistent GameObject in your first scene
/// </summary>
public class DatabaseBootstrap : MonoBehaviour
{
    [Header("Database Settings")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool debugMode = true;
    
    void Awake()
    {
        if (initializeOnAwake)
        {
            InitializeDatabaseSystems();
        }
    }
    
    [ContextMenu("Initialize Database Systems")]
    public void InitializeDatabaseSystems()
    {
        if (debugMode) Debug.Log("🔄 Initializing database systems...");
        
        // Initialize UnityWebDatabase
        var webDb = UnityWebDatabase.Instance;
        if (webDb != null && debugMode) 
        {
            Debug.Log("✅ UnityWebDatabase initialized");
        }
        
        // Initialize AccountManager  
        var accountMgr = AccountManager.Instance;
        if (accountMgr != null && debugMode)
        {
            Debug.Log("✅ AccountManager initialized");
        }
        
        // Initialize MMOCharacterManager
        var charMgr = MMOCharacterManager.Instance;
        if (charMgr != null && debugMode)
        {
            Debug.Log("✅ MMOCharacterManager initialized");
        }
        
        if (debugMode) Debug.Log("🎉 Database systems initialization complete!");
    }
    
    void Start()
    {
        // Test connection on start
        if (UnityWebDatabase.Instance != null)
        {
            UnityWebDatabase.Instance.TestConnectionAsync();
        }
    }
}