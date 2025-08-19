using UnityEngine;
using System.Collections;

/// <summary>
/// Test setup script for combat system - spawns test mobs and sets up combat scenario
/// </summary>
public class CombatTestSetup : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoSpawnTestMobs = true;
    [SerializeField] private int numberOfHorses = 3;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Horse Prefab Settings")]
    [SerializeField] private GameObject horseMobPrefab; // Assign manually if available
    
    void Start()
    {
        if (autoSpawnTestMobs)
        {
            StartCoroutine(SpawnTestMobsDelayed());
        }
    }
    
    /// <summary>
    /// Spawn test mobs after a brief delay to ensure scene is ready
    /// </summary>
    IEnumerator SpawnTestMobsDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnTestHorses();
    }
    
    /// <summary>
    /// Spawn test horses for combat testing
    /// </summary>
    public void SpawnTestHorses()
    {
        if (enableDebugLogs)
            Debug.Log($"🐎 Spawning {numberOfHorses} test horses for combat testing");
        
        for (int i = 0; i < numberOfHorses; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            SpawnHorse(spawnPos, i);
        }
    }
    
    /// <summary>
    /// Get a random position around the test setup center
    /// </summary>
    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
        
        // Ensure spawn position is not too close to the center
        float minDistance = 1.5f;
        Vector3 toSpawn = spawnPos - transform.position;
        if (toSpawn.magnitude < minDistance)
        {
            toSpawn = toSpawn.normalized * minDistance;
            spawnPos = transform.position + toSpawn;
        }
        
        return spawnPos;
    }
    
    /// <summary>
    /// Spawn a single horse at the specified position
    /// </summary>
    void SpawnHorse(Vector3 position, int horseIndex)
    {
        GameObject horse = null;
        
        // Try to use the assigned prefab first
        if (horseMobPrefab != null)
        {
            horse = Instantiate(horseMobPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create a horse from scratch
            horse = CreateHorseFromScratch(position);
        }
        
        if (horse != null)
        {
            horse.name = $"TestHorse_{horseIndex}";
            
            // Ensure the horse has proper components
            SetupHorseComponents(horse);
            
            if (enableDebugLogs)
                Debug.Log($"✅ Spawned test horse {horseIndex} at {position}");
        }
    }
    
    /// <summary>
    /// Create a horse mob from scratch if no prefab is available
    /// </summary>
    GameObject CreateHorseFromScratch(Vector3 position)
    {
        GameObject horse = new GameObject("TestHorse");
        horse.transform.position = position;
        
        // Add the horse mob component
        var horseMob = horse.AddComponent<HorseMob>();
        
        // Initialize with basic settings
        if (MapBounds.Instance != null)
        {
            Rect allowedArea = new Rect(position.x - 2f, position.y - 2f, 4f, 4f);
            horseMob.Initialize(null, null, allowedArea);
        }
        
        return horse;
    }
    
    /// <summary>
    /// Ensure the horse has all necessary components for testing
    /// </summary>
    void SetupHorseComponents(GameObject horse)
    {
        // Ensure it has a collider for melee detection
        var collider = horse.GetComponent<Collider2D>();
        if (collider == null)
        {
            var capsuleCollider = horse.AddComponent<CapsuleCollider2D>();
            capsuleCollider.size = new Vector2(0.8f, 1.2f);
            capsuleCollider.direction = CapsuleDirection2D.Vertical;
        }
        
        // Ensure it has a Rigidbody2D
        var rb = horse.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = horse.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
        
        // Add a simple health display for testing
        horse.AddComponent<SimpleHealthDisplay>();
    }
    
    /// <summary>
    /// Clear all test mobs from the scene
    /// </summary>
    [ContextMenu("Clear Test Mobs")]
    public void ClearTestMobs()
    {
        var testHorses = GameObject.FindObjectsOfType<HorseMob>();
        foreach (var horse in testHorses)
        {
            if (horse.name.Contains("Test"))
            {
                DestroyImmediate(horse.gameObject);
            }
        }
        
        if (enableDebugLogs)
            Debug.Log("🧹 Cleared all test mobs");
    }
    
    /// <summary>
    /// Manually spawn test mobs from inspector
    /// </summary>
    [ContextMenu("Spawn Test Mobs")]
    public void ManualSpawnTestMobs()
    {
        SpawnTestHorses();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn radius as a wire sphere (Unity doesn't have DrawWireCircle in 2D)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw minimum spawn distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}

/// <summary>
/// Simple component to display health above mobs for testing
/// </summary>
public class SimpleHealthDisplay : MonoBehaviour
{
    private HorseMob horseMob;
    private MobBase mobBase;
    private int lastHealth = -1;
    private TextMesh healthText;
    private int estimatedHealth = 100;
    
    void Start()
    {
        horseMob = GetComponent<HorseMob>();
        mobBase = GetComponent<MobBase>();
        
        // Create health display text
        GameObject textObj = new GameObject("HealthDisplay");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.up * 0.8f;
        
        healthText = textObj.AddComponent<TextMesh>();
        healthText.text = "HP: ?";
        healthText.fontSize = 24;
        healthText.color = Color.white;
        healthText.anchor = TextAnchor.MiddleCenter;
        healthText.alignment = TextAlignment.Center;
        
        // Scale down the text
        textObj.transform.localScale = Vector3.one * 0.1f;
        
        // Set initial health estimate
        estimatedHealth = GetMaxHealth();
    }
    
    void Update()
    {
        int currentHealth = GetCurrentHealth();
        
        if (currentHealth != lastHealth)
        {
            lastHealth = currentHealth;
            healthText.text = $"HP: {currentHealth}";
            
            // Change color based on health percentage
            float healthPercent = (float)currentHealth / GetMaxHealth();
            if (healthPercent > 0.6f)
                healthText.color = Color.green;
            else if (healthPercent > 0.3f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }
    
    int GetCurrentHealth()
    {
        // Simple estimation based on whether object still exists and max health
        if (horseMob != null) return estimatedHealth;
        if (mobBase != null) return estimatedHealth;
        return 0;
    }
    
    int GetMaxHealth()
    {
        if (horseMob != null) return horseMob.maxHealth;
        if (mobBase != null) return mobBase.maxHealth;
        return 100;
    }
    
    // Public method for combat system to update health
    public void TakeDamage(int damage)
    {
        estimatedHealth = Mathf.Max(0, estimatedHealth - damage);
    }
}