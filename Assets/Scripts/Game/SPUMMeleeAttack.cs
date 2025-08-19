using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Melee attack system for SPUM characters that deals damage to mobs in front of the player
/// </summary>
public class SPUMMeleeAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int baseDamage = 5;
    [SerializeField] private float attackRange = 1.0f; // Adjacent tile only
    [SerializeField] private float attackArc = 90f; // degrees in front of player
    [SerializeField] private LayerMask targetLayerMask = -1; // all layers by default
    [SerializeField] private float attackCooldown = 0.5f; // Prevent spam attacks
    [SerializeField] private bool useAdjacentTileOnly = true; // Limit to adjacent tiles
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugVisualization = true;
    [SerializeField] private Color debugColor = Color.red;
    [SerializeField] private float debugDisplayDuration = 0.2f;
    
    [Header("Audio/Effects")]
    [SerializeField] private AudioClip attackSound; // Legacy - use CombatSoundManager instead
    [SerializeField] private GameObject hitEffect; // VFX to spawn on hit
    [SerializeField] private bool useCombatSoundManager = true;
    
    private SPUMCharacterController characterController;
    private AudioSource audioSource;
    private bool isDebugVisualizationActive = false;
    private float lastAttackTime = 0f;
    
    void Start()
    {
        // Get the SPUM character controller
        characterController = GetComponent<SPUMCharacterController>();
        if (characterController == null)
        {
            Debug.LogWarning("SPUMMeleeAttack: No SPUMCharacterController found on this GameObject");
        }
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && attackSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Listen for attack events from character controller
        if (characterController != null)
        {
            // Hook into existing attack system
            StartCoroutine(MonitorAttacks());
        }
    }
    
    /// <summary>
    /// Monitor for spacebar input and immediately trigger attacks (NexusTK style)
    /// </summary>
    IEnumerator MonitorAttacks()
    {
        while (true)
        {
            yield return null; // Wait one frame
            
            // Listen for direct spacebar input for immediate attack response
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Check cooldown to prevent spam attacks
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    lastAttackTime = Time.time;
                    // Immediate damage application like NexusTK
                    PerformMeleeAttack();
                }
            }
        }
    }
    
    /// <summary>
    /// Perform the actual melee attack damage calculation
    /// </summary>
    public void PerformMeleeAttack()
    {
        Vector2 attackDirection = GetAttackDirection();
        Vector2 attackPosition = transform.position;
        
        Debug.Log($"⚔️ Performing melee attack from {attackPosition} in direction {attackDirection}");
        
        List<GameObject> validTargets = new List<GameObject>();
        
        if (useAdjacentTileOnly)
        {
            // Get target in the adjacent tile based on facing direction
            validTargets = GetAdjacentTileTargets(attackPosition, attackDirection);
        }
        else
        {
            // Use original arc-based detection
            List<Collider2D> targetsInRange = FindTargetsInRange(attackPosition);
            
            foreach (var target in targetsInRange)
            {
                if (IsTargetInAttackArc(attackPosition, attackDirection, target.transform.position))
                {
                    validTargets.Add(target.gameObject);
                }
            }
        }
        
        Debug.Log($"⚔️ Found {validTargets.Count} targets in attack range");
        
        // Play swing sound immediately when attacking
        PlaySwingSound();
        
        // Deal damage to all valid targets
        bool hitAnyTarget = false;
        foreach (var target in validTargets)
        {
            DealDamageToTarget(target);
            hitAnyTarget = true;
        }
        
        // Play hit sound if we hit something
        if (hitAnyTarget)
        {
            PlayHitSound();
        }
        
        // Show debug visualization
        if (showDebugVisualization)
        {
            StartCoroutine(ShowDebugVisualization(attackPosition, attackDirection));
        }
    }
    
    /// <summary>
    /// Get targets in adjacent tile based on facing direction
    /// </summary>
    List<GameObject> GetAdjacentTileTargets(Vector2 attackPosition, Vector2 attackDirection)
    {
        List<GameObject> targets = new List<GameObject>();
        
        // Get the adjacent tile position based on direction
        Vector2 adjacentTile = GetAdjacentTilePosition(attackPosition, attackDirection);
        
        Debug.Log($"🎯 Checking adjacent tile at {adjacentTile} for targets");
        
        // Check for targets at this specific tile (precise tile-based detection)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(adjacentTile, 0.3f);
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;
            
            if (HasDamageableComponent(collider.gameObject))
            {
                targets.Add(collider.gameObject);
                Debug.Log($"🎯 Found target in adjacent tile: {collider.gameObject.name}");
            }
        }
        
        return targets;
    }
    
    /// <summary>
    /// Get adjacent tile position based on direction
    /// </summary>
    Vector2 GetAdjacentTilePosition(Vector2 position, Vector2 direction)
    {
        // Round current position to nearest tile
        Vector2Int currentTile = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        
        // Determine which adjacent tile to check based on direction
        Vector2Int targetTile = currentTile;
        
        // Convert direction to cardinal direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement
            targetTile.x += direction.x > 0 ? 1 : -1;
        }
        else
        {
            // Vertical movement
            targetTile.y += direction.y > 0 ? 1 : -1;
        }
        
        return new Vector2(targetTile.x, targetTile.y);
    }
    
    /// <summary>
    /// Get the direction the character is facing for the attack
    /// </summary>
    Vector2 GetAttackDirection()
    {
        if (characterController != null)
        {
            // Use the character's last move direction
            Vector2 moveDir = characterController.GetMoveDirection();
            if (moveDir.magnitude > 0.1f)
            {
                return moveDir.normalized;
            }
        }
        
        // Default to facing down if no movement direction available
        return Vector2.down;
    }
    
    /// <summary>
    /// Find all colliders within attack range (improved targeting)
    /// </summary>
    List<Collider2D> FindTargetsInRange(Vector2 position)
    {
        // Use OverlapCircleAll with no layer mask to catch everything, then filter
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, attackRange);
        List<Collider2D> validTargets = new List<Collider2D>();
        
        foreach (var collider in colliders)
        {
            // Don't hit ourselves
            if (collider.gameObject == gameObject) continue;
            
            // Check for damageable components
            if (HasDamageableComponent(collider.gameObject))
            {
                validTargets.Add(collider);
                Debug.Log($"🎯 Found potential target: {collider.gameObject.name} at {collider.transform.position}");
            }
        }
        
        return validTargets;
    }
    
    /// <summary>
    /// Check if a target is within the attack arc in front of the player
    /// </summary>
    bool IsTargetInAttackArc(Vector2 attackPos, Vector2 attackDir, Vector2 targetPos)
    {
        Vector2 toTarget = (targetPos - attackPos).normalized;
        float angle = Vector2.Angle(attackDir, toTarget);
        
        return angle <= attackArc / 2f;
    }
    
    /// <summary>
    /// Check if a GameObject has a component that can take damage
    /// </summary>
    bool HasDamageableComponent(GameObject target)
    {
        // Check for various mob types
        return target.GetComponent<HorseMob>() != null ||
               target.GetComponent<MobBase>() != null ||
               target.GetComponent<IDamageable>() != null; // For future extensibility
    }
    
    /// <summary>
    /// Deal damage to a specific target (NexusTK style damage calculation)
    /// </summary>
    void DealDamageToTarget(GameObject target)
    {
        // Calculate final damage with some randomization like classic 2D RPGs
        int finalDamage = Mathf.Max(1, baseDamage + Random.Range(-1, 2));
        
        Debug.Log($"💥 Dealing {finalDamage} damage to {target.name}");
        
        // Try different damage interfaces
        var horseMob = target.GetComponent<HorseMob>();
        if (horseMob != null)
        {
            horseMob.Damage(finalDamage, transform); // Pass attacker reference
            SpawnHitEffect(target.transform.position);
            Debug.Log($"✅ Successfully damaged HorseMob {target.name} for {finalDamage} damage");
            return;
        }
        
        var mobBase = target.GetComponent<MobBase>();
        if (mobBase != null)
        {
            mobBase.Damage(finalDamage, transform); // Pass attacker reference
            SpawnHitEffect(target.transform.position);
            Debug.Log($"✅ Successfully damaged MobBase {target.name} for {finalDamage} damage");
            return;
        }
        
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(finalDamage);
            SpawnHitEffect(target.transform.position);
            Debug.Log($"✅ Successfully damaged IDamageable {target.name} for {finalDamage} damage");
            return;
        }
        
        Debug.LogWarning($"⚠️ Could not find damage interface on {target.name}");
    }
    
    /// <summary>
    /// Spawn visual hit effect
    /// </summary>
    void SpawnHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up after 2 seconds
        }
    }
    
    /// <summary>
    /// Show debug visualization of attack range and arc
    /// </summary>
    IEnumerator ShowDebugVisualization(Vector2 attackPos, Vector2 attackDir)
    {
        isDebugVisualizationActive = true;
        yield return new WaitForSeconds(debugDisplayDuration);
        isDebugVisualizationActive = false;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugVisualization) return;
        
        Vector2 attackPos = transform.position;
        Vector2 attackDir = GetAttackDirection();
        
        // Draw attack range circle
        Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.3f);
        Gizmos.DrawSphere(attackPos, attackRange);
        
        // Draw attack arc
        if (isDebugVisualizationActive)
        {
            Gizmos.color = debugColor;
            
            // Draw attack direction
            Gizmos.DrawLine(attackPos, attackPos + attackDir * attackRange);
            
            // Draw attack arc boundaries
            float halfArc = attackArc / 2f;
            Vector2 leftBoundary = RotateVector(attackDir, -halfArc) * attackRange;
            Vector2 rightBoundary = RotateVector(attackDir, halfArc) * attackRange;
            
            Gizmos.DrawLine(attackPos, attackPos + leftBoundary);
            Gizmos.DrawLine(attackPos, attackPos + rightBoundary);
        }
    }
    
    /// <summary>
    /// Rotate a vector by the specified angle in degrees
    /// </summary>
    Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
    
    /// <summary>
    /// Play swing sound using modern sound manager or fallback
    /// </summary>
    void PlaySwingSound()
    {
        if (useCombatSoundManager && CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlaySwingSound();
        }
        else if (attackSound != null && audioSource != null)
        {
            // Fallback to legacy sound system
            audioSource.PlayOneShot(attackSound);
        }
    }
    
    /// <summary>
    /// Play hit sound using modern sound manager
    /// </summary>
    void PlayHitSound()
    {
        if (useCombatSoundManager && CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayHitSound();
        }
    }
    
    // Public API for external scripts
    public void SetDamage(int damage) => baseDamage = damage;
    public void SetAttackRange(float range) => attackRange = range;
    public void SetAttackArc(float arc) => attackArc = arc;
    public int GetDamage() => baseDamage;
    public float GetAttackRange() => attackRange;
    public float GetAttackArc() => attackArc;
}

/// <summary>
/// Interface for objects that can take damage (for future extensibility)
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
    int GetCurrentHealth();
    int GetMaxHealth();
}