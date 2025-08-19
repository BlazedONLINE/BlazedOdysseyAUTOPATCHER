using UnityEngine;

/// <summary>
/// Alternative SPUM direction handler that can be used as a standalone component.
/// This script can be manually added to any SPUM character to enable direction flipping.
/// It monitors for movement in various ways and handles the sprite direction accordingly.
/// </summary>
[System.Serializable]
public class SPUMDirectionSettings
{
    [Header("Flipping Behavior")]
    public bool enableHorizontalFlipping = true;
    public bool enableVerticalFlipping = false;
    public float movementThreshold = 0.1f;
    
    [Header("Direction Detection")]
    public bool useTransformMovement = true;
    public bool useInputSystem = true;
    public bool useVelocity = false;
    
    [Header("Scale Settings")]
    public bool invertHorizontalScale = false; // In case the sprites are backwards
    public bool invertVerticalScale = false;
}

public class SPUMMovementSync : MonoBehaviour
{
    [Header("Settings")]
    public SPUMDirectionSettings directionSettings = new SPUMDirectionSettings();
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool enableDebugLogs = false;
    
    // Internal state
    private Vector3 lastPosition;
    private Vector2 lastMovementDirection = Vector2.zero;
    private Vector2 currentFacingDirection = Vector2.left; // SPUM default is facing left
    
    // Component references
    private SPUM_Prefabs spumPrefab;
    private Rigidbody2D rb;
    private Animator animator;
    
    // Movement detection
    private float positionUpdateTimer = 0f;
    private const float POSITION_UPDATE_INTERVAL = 0.1f; // Check position every 0.1 seconds
    
    void Start()
    {
        InitializeComponents();
        lastPosition = transform.position;
        
        if (enableDebugLogs)
            Debug.Log($"🔄 SPUMMovementSync initialized on {gameObject.name}");
    }
    
    void InitializeComponents()
    {
        // Find SPUM components
        spumPrefab = GetComponent<SPUM_Prefabs>();
        if (spumPrefab == null)
            spumPrefab = GetComponentInChildren<SPUM_Prefabs>();
        
        // Get other components that might indicate movement
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"🔍 SPUM Prefab found: {spumPrefab != null}");
            Debug.Log($"🔍 Rigidbody2D found: {rb != null}");
            Debug.Log($"🔍 Animator found: {animator != null}");
        }
    }
    
    void Update()
    {
        Vector2 movementDirection = DetectMovementDirection();
        
        if (movementDirection.magnitude > directionSettings.movementThreshold)
        {
            UpdateSpriteDirection(movementDirection);
            lastMovementDirection = movementDirection;
        }
        
        // Update debug display
        if (showDebugInfo)
        {
            DisplayDebugInfo(movementDirection);
        }
    }
    
    Vector2 DetectMovementDirection()
    {
        Vector2 direction = Vector2.zero;
        
        // Method 1: Transform position movement
        if (directionSettings.useTransformMovement)
        {
            positionUpdateTimer += Time.deltaTime;
            if (positionUpdateTimer >= POSITION_UPDATE_INTERVAL)
            {
                Vector3 deltaPosition = transform.position - lastPosition;
                if (deltaPosition.magnitude > 0.01f) // Ignore tiny movements
                {
                    direction = new Vector2(deltaPosition.x, deltaPosition.y).normalized;
                }
                lastPosition = transform.position;
                positionUpdateTimer = 0f;
            }
        }
        
        // Method 2: Input system (fallback)
        if (directionSettings.useInputSystem && direction.magnitude < directionSettings.movementThreshold)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            direction = new Vector2(horizontal, vertical).normalized;
        }
        
        // Method 3: Rigidbody velocity
        if (directionSettings.useVelocity && rb != null && direction.magnitude < directionSettings.movementThreshold)
        {
            Vector2 velocity = rb.linearVelocity;
            if (velocity.magnitude > directionSettings.movementThreshold)
            {
                direction = velocity.normalized;
            }
        }
        
        return direction;
    }
    
    void UpdateSpriteDirection(Vector2 movementDirection)
    {
        Vector3 newScale = transform.localScale;
        bool scaleChanged = false;
        
        // Handle horizontal flipping
        // SPUM sprites face LEFT by default, so RIGHT = negative scale (flipped)
        if (directionSettings.enableHorizontalFlipping && Mathf.Abs(movementDirection.x) > directionSettings.movementThreshold)
        {
            bool shouldFaceRight = movementDirection.x > 0;
            
            // Apply inversion if needed
            if (directionSettings.invertHorizontalScale)
                shouldFaceRight = !shouldFaceRight;
            
            if (shouldFaceRight && newScale.x > 0)
            {
                newScale.x = -Mathf.Abs(newScale.x); // Face right = negative scale (flipped)
                scaleChanged = true;
                currentFacingDirection = Vector2.right;
            }
            else if (!shouldFaceRight && newScale.x < 0)
            {
                newScale.x = Mathf.Abs(newScale.x); // Face left = positive scale (normal)
                scaleChanged = true;
                currentFacingDirection = Vector2.left;
            }
        }
        
        // Handle vertical flipping (less common but available)
        if (directionSettings.enableVerticalFlipping && Mathf.Abs(movementDirection.y) > directionSettings.movementThreshold)
        {
            bool shouldFaceUp = movementDirection.y > 0;
            
            // Apply inversion if needed
            if (directionSettings.invertVerticalScale)
                shouldFaceUp = !shouldFaceUp;
            
            if (shouldFaceUp && newScale.y < 0)
            {
                newScale.y = Mathf.Abs(newScale.y);
                scaleChanged = true;
                currentFacingDirection = Vector2.up;
            }
            else if (!shouldFaceUp && newScale.y > 0)
            {
                newScale.y = -Mathf.Abs(newScale.y);
                scaleChanged = true;
                currentFacingDirection = Vector2.down;
            }
        }
        
        // Apply scale changes
        if (scaleChanged)
        {
            transform.localScale = newScale;
            
            if (enableDebugLogs)
                Debug.Log($"🔄 SPUM direction updated: movement={movementDirection}, facing={currentFacingDirection}, scale={newScale}");
        }
    }
    
    void DisplayDebugInfo(Vector2 movementDirection)
    {
        // This could be expanded to show debug UI, but for now just console logs
        if (enableDebugLogs && movementDirection.magnitude > 0.1f)
        {
            Debug.Log($"🎯 Movement: {movementDirection}, Facing: {currentFacingDirection}, Scale: {transform.localScale}");
        }
    }
    
    /// <summary>
    /// Manually set the character facing direction
    /// </summary>
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            UpdateSpriteDirection(direction);
        }
    }
    
    /// <summary>
    /// Get the current facing direction
    /// </summary>
    public Vector2 GetFacingDirection()
    {
        return currentFacingDirection;
    }
    
    /// <summary>
    /// Reset to default facing direction (left, which is SPUM default)
    /// </summary>
    public void ResetDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x); // Positive scale = facing left (SPUM default)
        scale.y = Mathf.Abs(scale.y);
        transform.localScale = scale;
        currentFacingDirection = Vector2.left;
        
        if (enableDebugLogs)
            Debug.Log($"🔄 SPUM direction reset to default (facing left)");
    }
    
    /// <summary>
    /// Force update the sprite direction based on current movement
    /// </summary>
    public void ForceUpdateDirection()
    {
        Vector2 direction = DetectMovementDirection();
        if (direction.magnitude > directionSettings.movementThreshold)
        {
            UpdateSpriteDirection(direction);
        }
    }
    
    // Unity Editor helpers
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo)
        {
            // Draw facing direction arrow
            Gizmos.color = Color.green;
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)currentFacingDirection * 1f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawCube(end, Vector3.one * 0.1f);
            
            // Draw movement direction arrow
            if (lastMovementDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Vector3 moveEnd = start + (Vector3)lastMovementDirection * 1.5f;
                Gizmos.DrawLine(start, moveEnd);
                Gizmos.DrawWireCube(moveEnd, Vector3.one * 0.15f);
            }
        }
    }
    #endif
}