using UnityEngine;

/// <summary>
/// Handles direction-based sprite flipping for SPUM characters.
/// Monitors movement input and flips the SPUM character sprites to face the movement direction.
/// This component should be attached to SPUM visual objects to enable proper directional facing.
/// </summary>
public class SPUMDirectionController : MonoBehaviour
{
    [Header("Direction Settings")]
    [SerializeField] private bool enableDirectionFlipping = true;
    [SerializeField] private bool flipOnHorizontalMovement = true;
    [SerializeField] private bool flipOnVerticalMovement = false;
    [SerializeField] private float directionThreshold = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    
    // Reference to the SPUM prefab component
    private SPUM_Prefabs spumPrefab;
    
    // Movement tracking
    private Vector2 lastMovementDirection = Vector2.zero;
    private Vector2 currentFacingDirection = Vector2.left; // Default to facing left (SPUM default)
    
    // Parent player controller references (auto-detected)
    private StarterMapPlayerController starterMapController;
    private MonoBehaviour playerController2D; // Using MonoBehaviour to avoid namespace issues
    
    void Start()
    {
        InitializeComponents();
        
        if (enableDebugLogs)
            Debug.Log($"🧭 SPUMDirectionController initialized on {gameObject.name}");
    }
    
    void InitializeComponents()
    {
        // Get SPUM prefab component
        spumPrefab = GetComponent<SPUM_Prefabs>();
        if (spumPrefab == null)
            spumPrefab = GetComponentInChildren<SPUM_Prefabs>();
        
        // Try to find player controllers in parent hierarchy
        starterMapController = GetComponentInParent<StarterMapPlayerController>();
        
        // Try to find PlayerController2D without hard namespace dependency
        var allControllers = GetComponentsInParent<MonoBehaviour>();
        foreach (var controller in allControllers)
        {
            if (controller.GetType().Name == "PlayerController2D")
            {
                playerController2D = controller;
                break;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"🔍 SPUM Prefab: {spumPrefab != null}");
            Debug.Log($"🔍 StarterMap Controller: {starterMapController != null}");
            Debug.Log($"🔍 Player Controller 2D: {playerController2D != null}");
        }
    }
    
    void Update()
    {
        if (!enableDirectionFlipping) return;
        
        Vector2 movementDirection = GetCurrentMovementDirection();
        
        // Only update direction if there's significant movement
        if (movementDirection.magnitude > directionThreshold)
        {
            UpdateFacingDirection(movementDirection);
            lastMovementDirection = movementDirection;
        }
    }
    
    /// <summary>
    /// Get the current movement direction from available player controllers
    /// </summary>
    Vector2 GetCurrentMovementDirection()
    {
        Vector2 direction = Vector2.zero;
        
        // Try StarterMapPlayerController first
        if (starterMapController != null)
        {
            // Get movement input manually (since StarterMapPlayerController doesn't expose it)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            direction = new Vector2(horizontal, vertical).normalized;
        }
        // Try PlayerController2D using reflection to avoid namespace issues
        else if (playerController2D != null)
        {
            try
            {
                var getMoveDirectionMethod = playerController2D.GetType().GetMethod("GetMoveDirection");
                if (getMoveDirectionMethod != null)
                {
                    var result = getMoveDirectionMethod.Invoke(playerController2D, null);
                    if (result is Vector2 moveDir)
                    {
                        direction = moveDir;
                    }
                }
            }
            catch (System.Exception)
            {
                // Silently fall back to input if reflection fails
            }
        }
        // Fallback: read input directly
        else
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            direction = new Vector2(horizontal, vertical).normalized;
        }
        
        return direction;
    }
    
    /// <summary>
    /// Update the character's facing direction based on movement
    /// </summary>
    void UpdateFacingDirection(Vector2 movementDirection)
    {
        bool shouldFlip = false;
        Vector3 newScale = transform.localScale;
        
        // Handle horizontal flipping (most common for 2D sprites)
        // SPUM sprites face LEFT by default, so we flip them to face RIGHT
        if (flipOnHorizontalMovement && Mathf.Abs(movementDirection.x) > directionThreshold)
        {
            if (movementDirection.x > 0) // Moving right
            {
                // Face right: negative scale (flipped)
                if (newScale.x > 0)
                {
                    newScale.x = -Mathf.Abs(newScale.x);
                    shouldFlip = true;
                }
                currentFacingDirection = Vector2.right;
            }
            else if (movementDirection.x < 0) // Moving left
            {
                // Face left: positive scale (normal/default)
                if (newScale.x < 0)
                {
                    newScale.x = Mathf.Abs(newScale.x);
                    shouldFlip = true;
                }
                currentFacingDirection = Vector2.left;
            }
        }
        
        // Handle vertical flipping (less common, but available if needed)
        if (flipOnVerticalMovement && Mathf.Abs(movementDirection.y) > directionThreshold)
        {
            if (movementDirection.y > 0) // Moving up
            {
                if (newScale.y < 0)
                {
                    newScale.y = Mathf.Abs(newScale.y);
                    shouldFlip = true;
                }
                currentFacingDirection = Vector2.up;
            }
            else if (movementDirection.y < 0) // Moving down
            {
                if (newScale.y > 0)
                {
                    newScale.y = -Mathf.Abs(newScale.y);
                    shouldFlip = true;
                }
                currentFacingDirection = Vector2.down;
            }
        }
        
        // Apply the scale change if needed
        if (shouldFlip)
        {
            transform.localScale = newScale;
            
            if (enableDebugLogs)
                Debug.Log($"🔄 Flipped SPUM character: direction={movementDirection}, scale={newScale}");
        }
    }
    
    /// <summary>
    /// Manually set the character's facing direction
    /// </summary>
    public void SetFacingDirection(Vector2 direction)
    {
        UpdateFacingDirection(direction);
    }
    
    /// <summary>
    /// Get the current facing direction
    /// </summary>
    public Vector2 GetFacingDirection()
    {
        return currentFacingDirection;
    }
    
    /// <summary>
    /// Enable or disable direction flipping
    /// </summary>
    public void SetDirectionFlippingEnabled(bool enabled)
    {
        enableDirectionFlipping = enabled;
    }
    
    /// <summary>
    /// Reset character to face left (SPUM default direction)
    /// </summary>
    public void ResetToDefaultDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x); // Positive scale = facing left (SPUM default)
        scale.y = Mathf.Abs(scale.y);
        transform.localScale = scale;
        currentFacingDirection = Vector2.left;
        
        if (enableDebugLogs)
            Debug.Log($"🔄 Reset SPUM character to default direction (facing left)");
    }
}