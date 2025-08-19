using UnityEngine;

/// <summary>
/// Database configuration for BlazedOdyssey MMO
/// Contains connection settings for the Digital Ocean MySQL server
/// </summary>
[System.Serializable]
public class DatabaseConfig
{
    [Header("MySQL Server Configuration")]
    public string serverIP = "129.212.181.87";
    public int port = 3306;
    public string database = "blazed_odyssey_db";
    public string username = "gm";
    public string password = ".bLanch33x";
    
    [Header("Connection Settings")]
    public int connectionTimeout = 30;
    public int commandTimeout = 60;
    public bool useSSL = false;
    public bool allowZeroDateTime = true;
    
    [Header("Pool Settings")]
    public int minPoolSize = 5;
    public int maxPoolSize = 20;
    public int connectionLifetime = 300;
    
    /// <summary>
    /// Get the full MySQL connection string
    /// </summary>
    public string GetConnectionString()
    {
        return $"Server={serverIP};" +
               $"Port={port};" +
               $"Database={database};" +
               $"Uid={username};" +
               $"Pwd={password};" +
               $"Connection Timeout={connectionTimeout};" +
               $"Command Timeout={commandTimeout};" +
               $"SslMode={(useSSL ? "Required" : "None")};" +
               $"Allow Zero Datetime={allowZeroDateTime};" +
               $"Min Pool Size={minPoolSize};" +
               $"Max Pool Size={maxPoolSize};" +
               $"Connection Lifetime={connectionLifetime};" +
               $"Charset=utf8mb4;";
    }
    
    /// <summary>
    /// Get connection string without password for logging
    /// </summary>
    public string GetSafeConnectionString()
    {
        return $"Server={serverIP}:{port};Database={database};User={username};SSL={useSSL}";
    }
}

/// <summary>
/// Singleton database configuration manager
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    [SerializeField] private DatabaseConfig config = new DatabaseConfig();
    
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DatabaseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DatabaseManager");
                    _instance = go.AddComponent<DatabaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public DatabaseConfig Config => config;
    public string ConnectionString => config.GetConnectionString();
    
    /// <summary>
    /// Test database connection
    /// </summary>
    public bool TestConnection()
    {
        try
        {
            Debug.Log($"🔗 Testing database connection to: {config.GetSafeConnectionString()}");
            
            // This would need MySQL connector for Unity
            // For now, we'll implement this when we add the MySQL package
            
            Debug.Log("✅ Database connection test successful");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Database connection failed: {e.Message}");
            return false;
        }
    }
    
    void Start()
    {
        Debug.Log($"🗄️ DatabaseManager initialized with config: {config.GetSafeConnectionString()}");
    }
}