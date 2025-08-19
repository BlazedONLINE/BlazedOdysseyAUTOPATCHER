using UnityEngine;

/// <summary>
/// Bootstrap script that automatically adds combat capabilities to SPUM characters
/// This should be attached to any SPUM character that needs combat abilities
/// </summary>
public class SPUMCombatBootstrap : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private bool autoAddCombatComponents = true;
    [SerializeField] private int baseDamage = 5;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackArc = 90f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoAddCombatComponents)
        {
            SetupCombatComponents();
        }
    }
    
    /// <summary>
    /// Add combat components to this SPUM character
    /// </summary>
    public void SetupCombatComponents()
    {
        if (enableDebugLogs)
            Debug.Log($"🛠️ Setting up combat components for {gameObject.name}");
        
        // Ensure we have a character controller
        var charController = GetComponent<SPUMCharacterController>();
        if (charController == null)
        {
            charController = gameObject.AddComponent<SPUMCharacterController>();
            if (enableDebugLogs)
                Debug.Log("✅ Added SPUMCharacterController");
        }
        
        // Ensure we have melee attack
        var meleeAttack = GetComponent<SPUMMeleeAttack>();
        if (meleeAttack == null)
        {
            meleeAttack = gameObject.AddComponent<SPUMMeleeAttack>();
            if (enableDebugLogs)
                Debug.Log("✅ Added SPUMMeleeAttack");
        }
        
        // Configure the melee attack
        if (meleeAttack != null)
        {
            meleeAttack.SetDamage(baseDamage);
            meleeAttack.SetAttackRange(attackRange);
            meleeAttack.SetAttackArc(attackArc);
            
            if (enableDebugLogs)
                Debug.Log($"⚔️ Configured melee attack: {baseDamage} damage, {attackRange} range, {attackArc}° arc");
        }
        
        // Ensure proper collider setup
        EnsureColliderSetup();
        
        // Ensure proper tag
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
            if (enableDebugLogs)
                Debug.Log("🏷️ Set tag to 'Player'");
        }
    }
    
    /// <summary>
    /// Ensure the character has proper colliders for both combat and movement
    /// </summary>
    void EnsureColliderSetup()
    {
        var colliders = GetComponents<Collider2D>();
        bool hasNonTriggerCollider = false;
        
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                hasNonTriggerCollider = true;
                break;
            }
        }
        
        if (!hasNonTriggerCollider)
        {
            var newCollider = gameObject.AddComponent<CapsuleCollider2D>();
            newCollider.size = new Vector2(0.6f, 0.8f);
            newCollider.direction = CapsuleDirection2D.Vertical;
            newCollider.isTrigger = false;
            
            if (enableDebugLogs)
                Debug.Log("✅ Added CapsuleCollider2D for combat interactions");
        }
        
        // Ensure Rigidbody2D
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            
            if (enableDebugLogs)
                Debug.Log("✅ Added Rigidbody2D");
        }
    }
    
    /// <summary>
    /// Test the combat system by performing an attack
    /// </summary>
    [ContextMenu("Test Attack")]
    public void TestAttack()
    {
        var meleeAttack = GetComponent<SPUMMeleeAttack>();
        if (meleeAttack != null)
        {
            meleeAttack.PerformMeleeAttack();
            Debug.Log("🧪 Test attack performed!");
        }
        else
        {
            Debug.LogWarning("⚠️ No SPUMMeleeAttack component found for test");
        }
    }
    
    /// <summary>
    /// Manually trigger combat setup from inspector
    /// </summary>
    [ContextMenu("Setup Combat Components")]
    public void ManualSetupCombat()
    {
        SetupCombatComponents();
    }
}