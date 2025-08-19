using UnityEngine;
using System.Collections;

/// <summary>
/// Simple player health system with minimum health protection for testing
/// Implements IDamageable interface for mob attacks
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int minimumHealth = 1; // Can't go below this for testing
    
    [Header("Damage Feedback")]
    [SerializeField] private bool showDamageFlash = true;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private bool showDamageNumbers = true;
    
    [Header("Health Regeneration")]
    [SerializeField] private bool enableRegen = true;
    [SerializeField] private float regenRate = 1f; // HP per second
    [SerializeField] private float regenDelay = 5f; // Delay after taking damage
    
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private float lastDamageTime = 0f;
    private bool isFlashing = false;
    
    // Events for UI updates
    public System.Action<int, int> OnHealthChanged; // (current, max)
    public System.Action OnPlayerDied;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Get sprite renderers for damage flash
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
        
        // Notify UI of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"🏥 PlayerHealth initialized: {currentHealth}/{maxHealth} (Min: {minimumHealth})");
    }
    
    void Update()
    {
        // Health regeneration
        if (enableRegen && currentHealth < maxHealth && Time.time - lastDamageTime > regenDelay)
        {
            RegenerateHealth();
        }
        
    }
    
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        
        int actualDamage = Mathf.Abs(damage);
        int newHealth = Mathf.Max(minimumHealth, currentHealth - actualDamage);
        
        // Calculate actual damage dealt (considering minimum health)
        int damageDealt = currentHealth - newHealth;
        currentHealth = newHealth;
        
        lastDamageTime = Time.time;
        
        Debug.Log($"💔 Player took {damageDealt} damage! Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback
        if (showDamageFlash && !isFlashing)
        {
            StartCoroutine(DamageFlash());
        }
        
        if (showDamageNumbers)
        {
            ShowDamageNumber(damageDealt);
        }
        
        // Notify UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death (but only if we can go below minimum)
        if (currentHealth <= 0 && minimumHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int actualHealing = currentHealth - oldHealth;
        
        if (actualHealing > 0)
        {
            Debug.Log($"💚 Player healed {actualHealing} HP! Health: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (showDamageNumbers)
            {
                ShowHealNumber(actualHealing);
            }
        }
    }
    
    public void HealToFull()
    {
        int healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            Heal(healAmount);
            Debug.Log("✨ Player fully healed!");
        }
    }
    
    void RegenerateHealth()
    {
        float regenAmount = regenRate * Time.deltaTime;
        if (regenAmount >= 1f)
        {
            Heal(Mathf.FloorToInt(regenAmount));
        }
    }
    
    void Die()
    {
        Debug.Log("💀 Player died!");
        OnPlayerDied?.Invoke();
        
        // For testing, respawn immediately with minimum health
        if (minimumHealth > 0)
        {
            currentHealth = minimumHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"🔄 Player respawned with {minimumHealth} health for testing");
        }
    }
    
    IEnumerator DamageFlash()
    {
        isFlashing = true;
        
        // Flash red
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = damageFlashColor;
            }
        }
        
        yield return new WaitForSeconds(flashDuration);
        
        // Return to original colors
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
        
        isFlashing = false;
    }
    
    void ShowDamageNumber(int damage)
    {
        // Simple damage number display in console for now
        // Could be expanded to show floating text later
        Debug.Log($"💥 -{damage}");
    }
    
    void ShowHealNumber(int healing)
    {
        Debug.Log($"💚 +{healing}");
    }
    
    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / (float)maxHealth;
    public bool IsAlive() => currentHealth > 0;
    public bool IsAtMinimumHealth() => currentHealth <= minimumHealth;
    
    // Public setters for testing
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMinimumHealth(int newMinHealth)
    {
        minimumHealth = Mathf.Max(0, newMinHealth);
        currentHealth = Mathf.Max(minimumHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
}

// Using IDamageable interface from SPUMMeleeAttack.cs