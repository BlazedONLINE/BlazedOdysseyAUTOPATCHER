using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Comprehensive SPUM character controller that handles movement, direction, and attacks.
/// This integrates with SPUM's animation system to provide smooth character control.
/// </summary>
public class SPUMCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 0.05f; // Very slow movement speed
    [SerializeField] private float runSpeed = 3f; // Reserved for future spell system
    [SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private bool enableDirectionalMovement = true;
    [SerializeField] private bool enableRunning = false; // Disabled - reserved for spell system
    private bool isRunning = false; // Always false for now
    
    [Header("Attack Settings")]
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private bool enableAttack = true;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useMultiDirectionalAnimation = false; // Future feature
    [SerializeField] private float animationBlendSpeed = 5f;
    [SerializeField] private float walkAnimationSpeed = 1.0f; // Normal animation speed for run animation
    [SerializeField] private float runAnimationSpeed = 1.0f; // Normal speed for running
    [SerializeField] private int walkAnimationIndex = 0; // Use index 0 for run animation but at very slow speed
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool enableDebugLogs = false;
    
    // Component references
    private SPUM_Prefabs spumPrefabs;
    private Rigidbody2D rb;
    private Animator animator;
    private SimpleSPUMDirection directionController;
    private SPUMMeleeAttack meleeAttack;
    
    // Movement state
    private Vector2 currentInput;
    private Vector2 lastMoveDirection = Vector2.down; // Default facing down
    private bool isMoving = false;
    private bool isAttacking = false;
    
    // Attack state
    private float lastAttackTime;
    private bool attackQueued = false;
    
    // Animation state
    private PlayerState currentState = PlayerState.IDLE;
    private int currentAnimationIndex = 0;
    
    // Direction mapping for 8-directional movement
    public enum Direction8
    {
        South,      // Down
        SouthWest,  // Down-Left  
        West,       // Left
        NorthWest,  // Up-Left
        North,      // Up
        NorthEast,  // Up-Right
        East,       // Right
        SouthEast   // Down-Right
    }
    
    private Direction8 currentDirection = Direction8.South;
    
    void Start()
    {
        InitializeComponents();
        InitializeSPUM();
        
        if (enableDebugLogs)
            Debug.Log($"🎮 SPUMCharacterController initialized on {gameObject.name}");
    }
    
    void InitializeComponents()
    {
        // Get SPUM components
        spumPrefabs = GetComponent<SPUM_Prefabs>();
        if (spumPrefabs == null)
            spumPrefabs = GetComponentInChildren<SPUM_Prefabs>();
            
        // Get or add physics components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
        
        // Ensure player has proper collider for warp detection
        EnsurePlayerCollider();
        
        // Get animator
        if (spumPrefabs != null)
            animator = spumPrefabs._anim;
        
        // Get or add direction controller and configure it properly
        directionController = GetComponent<SimpleSPUMDirection>();
        if (directionController == null)
            directionController = gameObject.AddComponent<SimpleSPUMDirection>();
            
        // Disable automatic direction flipping to prevent interference with our movement
        if (directionController != null)
        {
            directionController.SetFlippingEnabled(false);
        }
        
        // Get melee attack component
        meleeAttack = GetComponent<SPUMMeleeAttack>();
        if (meleeAttack == null && enableAttack)
        {
            // Add melee attack component if attack is enabled but not present
            meleeAttack = gameObject.AddComponent<SPUMMeleeAttack>();
            if (enableDebugLogs)
                Debug.Log("✅ Added SPUMMeleeAttack component for combat");
        }
            
        if (enableDebugLogs)
        {
            Debug.Log($"🔍 SPUM Prefabs: {spumPrefabs != null}");
            Debug.Log($"🔍 Animator: {animator != null}");
            Debug.Log($"🔍 Direction Controller: {directionController != null}");
            Debug.Log($"🔍 Melee Attack: {meleeAttack != null}");
        }
    }
    
    void InitializeSPUM()
    {
        if (spumPrefabs == null) return;
        
        // Initialize SPUM animation system
        if (!spumPrefabs.allListsHaveItemsExist())
        {
            spumPrefabs.PopulateAnimationLists();
        }
        spumPrefabs.OverrideControllerInit();
        
        // Start with idle animation
        PlaySPUMAnimation(PlayerState.IDLE, 0);
        
        // Debug: Check available MOVE animations
        CheckAvailableAnimations();
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleAnimations();
        
        if (showDebugInfo)
            DisplayDebugInfo();
    }
    
    void HandleInput()
    {
        // Block gameplay inputs while any UI input field is focused (chat or other)
        bool uiTyping = false;
        var es = EventSystem.current;
        if (es != null && es.currentSelectedGameObject != null)
        {
            // If any TMP_InputField is selected, treat as typing
            uiTyping = es.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
        }
        bool blockGameplay = uiTyping;
        if (blockGameplay)
        {
            currentInput = Vector2.zero;
            // if chat engaged, immediately cancel any attack state and return to idle
            if (isAttacking)
            {
                isAttacking = false;
                PlaySPUMAnimation(PlayerState.IDLE, 0);
            }
            return;
        }

        // Movement input
        Vector2 input = Vector2.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
        
        currentInput = input.normalized;
        
        // Running disabled for now (reserved for spell system)
        // isRunning = Input.GetKey(runKey);
        
        // Attack input
        if (enableAttack && Input.GetKeyDown(attackKey))
        {
            TryAttack();
        }
    }
    
    void HandleMovement()
    {
        if (isAttacking) return; // Can't move while attacking
        
        isMoving = currentInput.magnitude > inputThreshold;
        
        if (isMoving)
        {
            // Update last move direction for idle facing
            lastMoveDirection = currentInput;
            
            // Update 8-directional facing
            currentDirection = GetDirection8FromVector(currentInput);
            
            // Handle sprite flipping manually (prevent slowdown issues)
            HandleSpriteFlipping(currentInput);
            
            // Move the character with collision detection
            float currentMoveSpeed = walkSpeed;
            Vector3 movement = new Vector3(currentInput.x, currentInput.y, 0f) * currentMoveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
            
            // Check for collisions before moving
            if (CanMoveTo(newPosition))
            {
                transform.position = newPosition;
            }
            else
            {
                // Try moving in individual axes if diagonal movement is blocked
                TryPartialMovement(movement);
            }
            
            if (enableDebugLogs)
                Debug.Log($"🚶 Moving: {currentDirection} - Input: {currentInput}");
        }
    }
    
    void HandleAnimations()
    {
        if (isAttacking)
        {
            // Attack animation is already playing, let it finish
            if (currentState != PlayerState.ATTACK)
            {
                PlaySPUMAnimation(PlayerState.ATTACK, 0);
            }
        }
        else if (isMoving)
        {
            // Play walk animation (use specific index for walk vs run)
            if (currentState != PlayerState.MOVE)
            {
                PlaySPUMAnimation(PlayerState.MOVE, walkAnimationIndex);
            }
        }
        else
        {
            // Play idle animation
            if (currentState != PlayerState.IDLE)
            {
                PlaySPUMAnimation(PlayerState.IDLE, 0);
            }
        }
    }
    
    void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        StartAttack();
    }
    
    void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (enableDebugLogs)
            Debug.Log($"⚔️ Attack started! Direction: {currentDirection}");
        
        // Play attack animation
        PlaySPUMAnimation(PlayerState.ATTACK, 0);
        
        // Calculate attack duration based on animation length
        float attackDuration = GetAnimationDuration(PlayerState.ATTACK);
        if (attackDuration <= 0) attackDuration = 0.5f; // Fallback
        
        // End attack after animation completes
        StartCoroutine(EndAttackAfterDelay(attackDuration));
    }
    
    IEnumerator EndAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        
        if (enableDebugLogs)
            Debug.Log($"⚔️ Attack finished!");
    }
    
    float GetAnimationDuration(PlayerState state)
    {
        if (spumPrefabs == null || !spumPrefabs.StateAnimationPairs.ContainsKey(state.ToString()))
            return 0f;
            
        var animList = spumPrefabs.StateAnimationPairs[state.ToString()];
        if (animList.Count > currentAnimationIndex && animList[currentAnimationIndex] != null)
        {
            return animList[currentAnimationIndex].length;
        }
        
        return 0f;
    }
    
    void PlaySPUMAnimation(PlayerState state, int index = 0)
    {
        if (spumPrefabs == null) return;
        
        currentState = state;
        currentAnimationIndex = index;
        
        try
        {
            spumPrefabs.PlayAnimation(state, index);
            
            // Adjust animation speed for walking
            if (animator != null && state == PlayerState.MOVE)
            {
                animator.speed = walkAnimationSpeed;
                
                if (enableDebugLogs)
                    Debug.Log($"🎬 Set animation speed to {walkAnimationSpeed} for walking");
            }
            else if (animator != null && state != PlayerState.MOVE)
            {
                // Reset to normal speed for non-movement animations
                animator.speed = 1.0f;
            }
            
            if (enableDebugLogs)
                Debug.Log($"🎬 Playing SPUM animation: {state} (index {index})");
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ Failed to play SPUM animation {state}: {e.Message}");
        }
    }
    
    Direction8 GetDirection8FromVector(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        
        // Map angles to 8 directions (45-degree sectors)
        // 0° = East, 90° = North, 180° = West, 270° = South
        if (angle >= 337.5f || angle < 22.5f) return Direction8.East;        // Right
        else if (angle >= 22.5f && angle < 67.5f) return Direction8.NorthEast;  // Up-Right
        else if (angle >= 67.5f && angle < 112.5f) return Direction8.North;     // Up
        else if (angle >= 112.5f && angle < 157.5f) return Direction8.NorthWest; // Up-Left
        else if (angle >= 157.5f && angle < 202.5f) return Direction8.West;     // Left
        else if (angle >= 202.5f && angle < 247.5f) return Direction8.SouthWest; // Down-Left
        else if (angle >= 247.5f && angle < 292.5f) return Direction8.South;    // Down
        else return Direction8.SouthEast; // Down-Right
    }
    
    void HandleSpriteFlipping(Vector2 input)
    {
        // Only flip on horizontal movement to prevent issues
        if (Mathf.Abs(input.x) > inputThreshold)
        {
            bool shouldFaceRight = input.x > 0;
            
            Vector3 scale = transform.localScale;
            
            // SPUM sprites face LEFT by default, so:
            // - To face RIGHT: use negative scale (flip the sprite)
            // - To face LEFT: use positive scale (normal/default)
            if (shouldFaceRight)
            {
                scale.x = -Mathf.Abs(scale.x); // Make negative (flipped)
            }
            else
            {
                scale.x = Mathf.Abs(scale.x); // Make positive (normal)
            }
            
            transform.localScale = scale;
            
            if (enableDebugLogs)
                Debug.Log($"🔄 Manual sprite flip: facing right = {shouldFaceRight}, scale = {scale}");
        }
    }
    
    void EnsurePlayerCollider()
    {
        // Ensure player has proper tag
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
            if (enableDebugLogs)
                Debug.Log("🏷️ Set player tag to 'Player'");
        }
        
        // Ensure player has collider for warp detection
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            // Add a small trigger collider for warp detection
            BoxCollider2D warpDetector = gameObject.AddComponent<BoxCollider2D>();
            warpDetector.size = new Vector2(0.8f, 0.8f);
            warpDetector.isTrigger = false; // Not a trigger so it can interact with warp triggers
            
            if (enableDebugLogs)
                Debug.Log("✅ Added BoxCollider2D for warp detection");
        }
        else
        {
            // Ensure existing collider is properly configured
            if (playerCollider.isTrigger)
            {
                // If it's a trigger, add another non-trigger collider
                BoxCollider2D warpDetector = gameObject.AddComponent<BoxCollider2D>();
                warpDetector.size = new Vector2(0.8f, 0.8f);
                warpDetector.isTrigger = false;
                
                if (enableDebugLogs)
                    Debug.Log("✅ Added additional BoxCollider2D for warp detection (existing was trigger)");
            }
        }
    }
    
    void CheckAvailableAnimations()
    {
        if (spumPrefabs == null || !enableDebugLogs) return;
        
        try
        {
            if (spumPrefabs.StateAnimationPairs.ContainsKey("MOVE"))
            {
                var moveList = spumPrefabs.StateAnimationPairs["MOVE"];
                Debug.Log($"🎬 Available MOVE animations: {moveList.Count}");
                
                for (int i = 0; i < moveList.Count && i < 5; i++)
                {
                    if (moveList[i] != null)
                    {
                        Debug.Log($"   • Index {i}: {moveList[i].name} (length: {moveList[i].length:F2}s)");
                    }
                }
            }
            else
            {
                Debug.Log("⚠️ No MOVE animations found in StateAnimationPairs");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error checking animations: {e.Message}");
        }
    }
    
    void DisplayDebugInfo()
    {
        if (!showDebugInfo) return;
        
        // This could be expanded to show UI debug info
        // For now, we just do periodic console logging
        if (enableDebugLogs && Time.frameCount % 60 == 0) // Every second at 60 FPS
        {
            Debug.Log($"📊 State: {currentState}, Direction: {currentDirection}, Moving: {isMoving}, Attacking: {isAttacking}");
        }
    }
    
    // Public API for external scripts
    public bool IsMoving() => isMoving;
    public bool IsAttacking() => isAttacking;
    public Direction8 GetCurrentDirection() => currentDirection;
    public PlayerState GetCurrentState() => currentState;
    public Vector2 GetMoveDirection() => lastMoveDirection;
    
    public void SetWalkSpeed(float speed) => walkSpeed = speed;
    public void SetRunSpeed(float speed) => runSpeed = speed;
    public void SetRunning(bool running) => isRunning = running;
    public void SetAttackCooldown(float cooldown) => attackCooldown = cooldown;
    
    /// <summary>
    /// Force play a specific SPUM animation
    /// </summary>
    public void PlayAnimation(PlayerState state, int index = 0)
    {
        PlaySPUMAnimation(state, index);
    }
    
    /// <summary>
    /// Trigger an attack programmatically
    /// </summary>
    public void TriggerAttack()
    {
        TryAttack();
    }
    
    /// <summary>
    /// Test different walk animation indices (for finding walk vs run)
    /// </summary>
    public void SetWalkAnimationIndex(int index)
    {
        walkAnimationIndex = index;
        Debug.Log($"🎬 Walk animation index set to {index}");
    }
    
    /// <summary>
    /// Get the current walk animation index
    /// </summary>
    public int GetWalkAnimationIndex()
    {
        return walkAnimationIndex;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 170, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("SPUM Character Controller", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"State: {currentState}");
        GUILayout.Label($"Direction: {currentDirection}");
        GUILayout.Label($"Moving: {isMoving} (Walking)");
        GUILayout.Label($"Attacking: {isAttacking}");
        GUILayout.Label($"Input: {currentInput}");
        GUILayout.Label($"Speed: {walkSpeed:F1}");
        GUILayout.Space(5);
        
        GUILayout.Label($"Controls:");
        GUILayout.Label($"  WASD/Arrows = Walk");
        GUILayout.Label($"  {attackKey} = Attack");
        GUILayout.Space(5);
        
        GUILayout.Label($"Animation Testing:");
        GUILayout.Label($"Walk Anim Index: {walkAnimationIndex}");
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Prev Anim"))
        {
            SetWalkAnimationIndex(Mathf.Max(0, walkAnimationIndex - 1));
        }
        if (GUILayout.Button("Next Anim"))
        {
            SetWalkAnimationIndex(walkAnimationIndex + 1);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Check if the player can move to a specific position (collision detection)
    /// </summary>
    bool CanMoveTo(Vector3 targetPosition)
    {
        // Get player's collider size for collision checking
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return true; // No collider means no collision
        
        // Use the collider bounds to check for overlaps
        Vector2 colliderSize = Vector2.one * 0.8f; // Default size
        if (playerCollider is BoxCollider2D boxCollider)
        {
            colliderSize = boxCollider.size;
        }
        else if (playerCollider is CapsuleCollider2D capsuleCollider)
        {
            colliderSize = capsuleCollider.size;
        }
        
        // Check for overlap with other colliders at the target position
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(targetPosition, colliderSize * 0.9f, 0f);
        
        foreach (var collider in overlapping)
        {
            // Ignore our own collider
            if (collider == playerCollider) continue;
            
            // Check if this is a blocking object (horses, other mobs, obstacles)
            if (IsBlockingCollider(collider))
            {
                if (enableDebugLogs)
                    Debug.Log($"🚫 Movement blocked by {collider.gameObject.name}");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Check if a collider should block player movement
    /// </summary>
    bool IsBlockingCollider(Collider2D collider)
    {
        // Don't block on trigger colliders (warps, pickups, etc.)
        if (collider.isTrigger) return false;
        
        // Block on mobs (horses, enemies, etc.)
        if (collider.GetComponent<HorseMob>() != null) return true;
        if (collider.GetComponent<MobBase>() != null) return true;
        
        // Block on walls and obstacles (objects on specific layers)
        GameObject obj = collider.gameObject;
        string layerName = LayerMask.LayerToName(obj.layer);
        if (layerName == "Obstacles" || layerName == "Walls" || layerName == "Environment") return true;
        
        // Block on objects tagged as obstacles
        if (obj.CompareTag("Obstacle") || obj.CompareTag("Wall")) return true;
        
        return false;
    }
    
    /// <summary>
    /// Try to move partially if diagonal movement is blocked
    /// </summary>
    void TryPartialMovement(Vector3 movement)
    {
        // Try X-axis movement only
        Vector3 xOnlyMovement = new Vector3(movement.x, 0, 0);
        Vector3 xPosition = transform.position + xOnlyMovement;
        if (CanMoveTo(xPosition))
        {
            transform.position = xPosition;
            if (enableDebugLogs)
                Debug.Log("🔄 Partial movement: X-axis only");
            return;
        }
        
        // Try Y-axis movement only
        Vector3 yOnlyMovement = new Vector3(0, movement.y, 0);
        Vector3 yPosition = transform.position + yOnlyMovement;
        if (CanMoveTo(yPosition))
        {
            transform.position = yPosition;
            if (enableDebugLogs)
                Debug.Log("🔄 Partial movement: Y-axis only");
            return;
        }
        
        // Can't move in any direction
        if (enableDebugLogs)
            Debug.Log("🚫 Movement completely blocked");
    }
}