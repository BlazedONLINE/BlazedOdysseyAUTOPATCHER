using UnityEngine;

/// <summary>
/// Simple, robust SPUM direction controller that works with any movement system.
/// This version has no dependencies and just monitors input directly.
/// Use this if the other direction controllers have compatibility issues.
/// </summary>
public class SimpleSPUMDirection : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableFlipping = true;
    [SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private bool usePositionChange = true;
    [SerializeField] private bool useInputSystem = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // State tracking
    private Vector3 lastPosition;
    private bool facingRight = false; // SPUM default is facing left
    
    void Start()
    {
        lastPosition = transform.position;
        
        if (showDebugLogs)
            Debug.Log($"🎯 SimpleSPUMDirection initialized on {gameObject.name}");
    }
    
    void Update()
    {
        if (!enableFlipping) return;
        
        Vector2 movement = GetMovementDirection();
        
        // Handle horizontal movement (primary direction control for SPUM)
        if (Mathf.Abs(movement.x) > inputThreshold)
        {
            bool shouldFaceRight = movement.x > 0;
            
            if (shouldFaceRight != facingRight)
            {
                FlipCharacter(shouldFaceRight);
            }
        }
        // For purely vertical movement (up/down), we could maintain current facing
        // but SPUM sprites don't have separate up/down sprites, so we keep horizontal facing
    }
    
    Vector2 GetMovementDirection()
    {
        Vector2 direction = Vector2.zero;
        
        // Method 1: Check position change
        if (usePositionChange)
        {
            Vector3 deltaPos = transform.position - lastPosition;
            if (deltaPos.magnitude > 0.01f) // Only register significant movement
            {
                direction = new Vector2(deltaPos.x, deltaPos.y).normalized;
                lastPosition = transform.position;
                return direction;
            }
        }
        
        // Method 2: Direct input reading (fallback)
        if (useInputSystem)
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            // Try multiple input methods
            try
            {
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
            }
            catch
            {
                // Input system not available, try key codes
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
            }
            
            direction = new Vector2(horizontal, vertical).normalized;
        }
        
        return direction;
    }
    
    void FlipCharacter(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        
        // SPUM sprites face LEFT by default, so:
        // - To face RIGHT: use negative scale (flip the sprite)
        // - To face LEFT: use positive scale (normal/default)
        if (faceRight)
        {
            scale.x = -Mathf.Abs(scale.x); // Make negative (flipped)
        }
        else
        {
            scale.x = Mathf.Abs(scale.x); // Make positive (normal)
        }
        
        transform.localScale = scale;
        facingRight = faceRight;
        
        if (showDebugLogs)
            Debug.Log($"🔄 Flipped SPUM character: facing right = {faceRight}, scale = {scale}");
    }
    
    /// <summary>
    /// Manually set character direction
    /// </summary>
    public void SetFacingRight(bool faceRight)
    {
        if (faceRight != facingRight)
        {
            FlipCharacter(faceRight);
        }
    }
    
    /// <summary>
    /// Get current facing direction
    /// </summary>
    public bool IsFacingRight()
    {
        return facingRight;
    }
    
    /// <summary>
    /// Reset to default direction (facing left, which is SPUM default)
    /// </summary>
    public void ResetDirection()
    {
        FlipCharacter(false); // SPUM default is facing left
    }
    
    /// <summary>
    /// Enable or disable direction flipping
    /// </summary>
    public void SetFlippingEnabled(bool enabled)
    {
        enableFlipping = enabled;
    }
}