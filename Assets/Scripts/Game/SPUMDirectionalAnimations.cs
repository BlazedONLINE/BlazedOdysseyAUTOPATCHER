using UnityEngine;

/// <summary>
/// Adds 8-directional animation support to SPUM characters by combining sprite flipping with rotation/scaling.
/// Since SPUM sprites are primarily horizontal (left/right), this simulates other directions creatively.
/// </summary>
public class SPUMDirectionalAnimations : MonoBehaviour
{
    [Header("Directional Animation Settings")]
    [SerializeField] private bool enableDirectionalAnimations = true;
    [SerializeField] private bool useVerticalFlipping = false;
    [SerializeField] private bool useRotationForDiagonals = false;
    [SerializeField] private float diagonalRotationAngle = 15f;
    
    [Header("Animation Tweaks")]
    [SerializeField] private Vector3 northOffset = Vector3.zero;
    [SerializeField] private Vector3 southOffset = Vector3.zero;
    [SerializeField] private Vector3 eastOffset = Vector3.zero;
    [SerializeField] private Vector3 westOffset = Vector3.zero;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Current direction state
    private SPUMCharacterController.Direction8 currentDirection = SPUMCharacterController.Direction8.South;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    // Component references
    private SPUMCharacterController characterController;
    private SimpleSPUMDirection simpleDirection;
    
    void Start()
    {
        // Store original transform values
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        // Get controller references
        characterController = GetComponent<SPUMCharacterController>();
        if (characterController == null)
            simpleDirection = GetComponent<SimpleSPUMDirection>();
        
        if (showDebugInfo)
            Debug.Log($"🎭 SPUMDirectionalAnimations initialized on {gameObject.name}");
    }
    
    void Update()
    {
        if (!enableDirectionalAnimations) return;
        
        UpdateDirectionalAnimation();
    }
    
    void UpdateDirectionalAnimation()
    {
        SPUMCharacterController.Direction8 newDirection = GetCurrentDirection();
        
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            ApplyDirectionalTransform(currentDirection);
            
            if (showDebugInfo)
                Debug.Log($"🎭 Applied directional animation: {currentDirection}");
        }
    }
    
    SPUMCharacterController.Direction8 GetCurrentDirection()
    {
        if (characterController != null)
        {
            return characterController.GetCurrentDirection();
        }
        else if (simpleDirection != null)
        {
            // For simple direction controller, we need to manually determine direction from input
            Vector2 input = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
            
            return GetDirection8FromVector(input.normalized);
        }
        
        return SPUMCharacterController.Direction8.South; // Default
    }
    
    SPUMCharacterController.Direction8 GetDirection8FromVector(Vector2 input)
    {
        if (input.magnitude < 0.1f) return currentDirection; // Keep current if no input
        
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        
        if (angle >= 337.5f || angle < 22.5f) return SPUMCharacterController.Direction8.East;
        else if (angle >= 22.5f && angle < 67.5f) return SPUMCharacterController.Direction8.NorthEast;
        else if (angle >= 67.5f && angle < 112.5f) return SPUMCharacterController.Direction8.North;
        else if (angle >= 112.5f && angle < 157.5f) return SPUMCharacterController.Direction8.NorthWest;
        else if (angle >= 157.5f && angle < 202.5f) return SPUMCharacterController.Direction8.West;
        else if (angle >= 202.5f && angle < 247.5f) return SPUMCharacterController.Direction8.SouthWest;
        else if (angle >= 247.5f && angle < 292.5f) return SPUMCharacterController.Direction8.South;
        else return SPUMCharacterController.Direction8.SouthEast;
    }
    
    void ApplyDirectionalTransform(SPUMCharacterController.Direction8 direction)
    {
        Vector3 newScale = originalScale;
        Vector3 newPosition = originalPosition;
        Quaternion newRotation = originalRotation;
        
        switch (direction)
        {
            case SPUMCharacterController.Direction8.North: // Up
                newScale.x = Mathf.Abs(originalScale.x); // Face left (normal)
                if (useVerticalFlipping) newScale.y = -Mathf.Abs(originalScale.y); // Flip vertically
                newPosition += northOffset;
                break;
                
            case SPUMCharacterController.Direction8.South: // Down  
                newScale.x = Mathf.Abs(originalScale.x); // Face left (normal)
                newScale.y = Mathf.Abs(originalScale.y); // Normal vertical
                newPosition += southOffset;
                break;
                
            case SPUMCharacterController.Direction8.East: // Right
                newScale.x = -Mathf.Abs(originalScale.x); // Face right (flipped)
                newScale.y = Mathf.Abs(originalScale.y); // Normal vertical
                newPosition += eastOffset;
                break;
                
            case SPUMCharacterController.Direction8.West: // Left
                newScale.x = Mathf.Abs(originalScale.x); // Face left (normal)
                newScale.y = Mathf.Abs(originalScale.y); // Normal vertical
                newPosition += westOffset;
                break;
                
            case SPUMCharacterController.Direction8.NorthEast: // Up-Right
                newScale.x = -Mathf.Abs(originalScale.x); // Face right (flipped)
                if (useVerticalFlipping) newScale.y = -Mathf.Abs(originalScale.y); // Flip vertically for up
                if (useRotationForDiagonals) newRotation = Quaternion.Euler(0, 0, -diagonalRotationAngle);
                newPosition += (northOffset + eastOffset) * 0.5f;
                break;
                
            case SPUMCharacterController.Direction8.NorthWest: // Up-Left
                newScale.x = Mathf.Abs(originalScale.x); // Face left (normal)
                if (useVerticalFlipping) newScale.y = -Mathf.Abs(originalScale.y); // Flip vertically for up
                if (useRotationForDiagonals) newRotation = Quaternion.Euler(0, 0, diagonalRotationAngle);
                newPosition += (northOffset + westOffset) * 0.5f;
                break;
                
            case SPUMCharacterController.Direction8.SouthEast: // Down-Right
                newScale.x = -Mathf.Abs(originalScale.x); // Face right (flipped)
                newScale.y = Mathf.Abs(originalScale.y); // Normal vertical
                if (useRotationForDiagonals) newRotation = Quaternion.Euler(0, 0, diagonalRotationAngle);
                newPosition += (southOffset + eastOffset) * 0.5f;
                break;
                
            case SPUMCharacterController.Direction8.SouthWest: // Down-Left
                newScale.x = Mathf.Abs(originalScale.x); // Face left (normal)
                newScale.y = Mathf.Abs(originalScale.y); // Normal vertical
                if (useRotationForDiagonals) newRotation = Quaternion.Euler(0, 0, -diagonalRotationAngle);
                newPosition += (southOffset + westOffset) * 0.5f;
                break;
        }
        
        // Apply the transforms
        transform.localScale = newScale;
        transform.localPosition = newPosition;
        transform.localRotation = newRotation;
    }
    
    /// <summary>
    /// Reset to original transform state
    /// </summary>
    public void ResetTransform()
    {
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
    
    /// <summary>
    /// Enable or disable directional animations
    /// </summary>
    public void SetDirectionalAnimationsEnabled(bool enabled)
    {
        enableDirectionalAnimations = enabled;
        if (!enabled)
            ResetTransform();
    }
    
    /// <summary>
    /// Get the current direction
    /// </summary>
    public SPUMCharacterController.Direction8 GetDirection()
    {
        return currentDirection;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(320, 10, 250, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("SPUM Directional Animations", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"Current Direction: {currentDirection}");
        GUILayout.Label($"Scale: {transform.localScale}");
        GUILayout.Label($"Rotation: {transform.localRotation.eulerAngles.z:F1}°");
        GUILayout.Space(5);
        
        GUILayout.Label("Features:");
        GUILayout.Label($"  Vertical Flip: {useVerticalFlipping}");
        GUILayout.Label($"  Diagonal Rotation: {useRotationForDiagonals}");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}