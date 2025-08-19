using UnityEngine;

/// <summary>
/// Test script to verify SPUM direction controllers are working correctly.
/// Attach this to a SPUM character to test direction changes manually.
/// This helps verify that the direction fixes are working properly.
/// </summary>
public class SPUMDirectionTester : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private KeyCode testFaceLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode testFaceRightKey = KeyCode.E;
    [SerializeField] private KeyCode testResetKey = KeyCode.R;
    
    [Header("Information")]
    [SerializeField] private bool showCurrentDirection = true;
    [SerializeField] private bool enableDebugOutput = true;
    
    // Component references
    private SimpleSPUMDirection simpleDirection;
    private SPUMDirectionController directionController;
    private SPUMMovementSync movementSync;
    
    void Start()
    {
        // Find available direction controllers
        simpleDirection = GetComponent<SimpleSPUMDirection>();
        directionController = GetComponent<SPUMDirectionController>();
        movementSync = GetComponent<SPUMMovementSync>();
        
        if (enableDebugOutput)
        {
            Debug.Log("🧪 SPUM Direction Tester initialized");
            Debug.Log($"   • SimpleSPUMDirection: {simpleDirection != null}");
            Debug.Log($"   • SPUMDirectionController: {directionController != null}");
            Debug.Log($"   • SPUMMovementSync: {movementSync != null}");
            Debug.Log($"   • Test Controls: {testFaceLeftKey} = Face Left, {testFaceRightKey} = Face Right, {testResetKey} = Reset");
        }
    }
    
    void Update()
    {
        HandleTestInputs();
        
        if (showCurrentDirection)
        {
            DisplayCurrentDirection();
        }
    }
    
    void HandleTestInputs()
    {
        // Test face left
        if (Input.GetKeyDown(testFaceLeftKey))
        {
            TestFaceLeft();
        }
        
        // Test face right
        if (Input.GetKeyDown(testFaceRightKey))
        {
            TestFaceRight();
        }
        
        // Test reset
        if (Input.GetKeyDown(testResetKey))
        {
            TestReset();
        }
    }
    
    void TestFaceLeft()
    {
        if (enableDebugOutput)
            Debug.Log("🧪 Testing Face LEFT");
            
        if (simpleDirection != null)
        {
            simpleDirection.SetFacingRight(false);
        }
        else if (directionController != null)
        {
            directionController.SetFacingDirection(Vector2.left);
        }
        else if (movementSync != null)
        {
            movementSync.SetFacingDirection(Vector2.left);
        }
        else
        {
            // Manual fallback
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x); // Positive = left for SPUM
            transform.localScale = scale;
        }
    }
    
    void TestFaceRight()
    {
        if (enableDebugOutput)
            Debug.Log("🧪 Testing Face RIGHT");
            
        if (simpleDirection != null)
        {
            simpleDirection.SetFacingRight(true);
        }
        else if (directionController != null)
        {
            directionController.SetFacingDirection(Vector2.right);
        }
        else if (movementSync != null)
        {
            movementSync.SetFacingDirection(Vector2.right);
        }
        else
        {
            // Manual fallback
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x); // Negative = right for SPUM
            transform.localScale = scale;
        }
    }
    
    void TestReset()
    {
        if (enableDebugOutput)
            Debug.Log("🧪 Testing RESET to default");
            
        if (simpleDirection != null)
        {
            simpleDirection.ResetDirection();
        }
        else if (directionController != null)
        {
            directionController.ResetToDefaultDirection();
        }
        else if (movementSync != null)
        {
            movementSync.ResetDirection();
        }
        else
        {
            // Manual fallback - SPUM default is facing left
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x); // Positive = left (SPUM default)
            transform.localScale = scale;
        }
    }
    
    void DisplayCurrentDirection()
    {
        if (transform.localScale.x > 0)
        {
            // Positive scale = facing left (SPUM default)
            if (showCurrentDirection && enableDebugOutput)
                Debug.Log("👈 Currently facing LEFT (SPUM default)");
        }
        else
        {
            // Negative scale = facing right (flipped)
            if (showCurrentDirection && enableDebugOutput)
                Debug.Log("👉 Currently facing RIGHT (flipped)");
        }
    }
    
    void OnGUI()
    {
        if (!showCurrentDirection) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("SPUM Direction Tester", GUI.skin.label);
        GUILayout.Space(5);
        
        // Show current direction
        string currentDir = transform.localScale.x > 0 ? "LEFT (Default)" : "RIGHT (Flipped)";
        GUILayout.Label($"Current Direction: {currentDir}");
        GUILayout.Label($"Scale X: {transform.localScale.x:F2}");
        GUILayout.Space(5);
        
        // Show available controllers
        string controllers = "";
        if (simpleDirection != null) controllers += "SimpleSPUMDirection ";
        if (directionController != null) controllers += "SPUMDirectionController ";
        if (movementSync != null) controllers += "SPUMMovementSync ";
        if (string.IsNullOrEmpty(controllers)) controllers = "None";
        
        GUILayout.Label($"Controllers: {controllers}");
        GUILayout.Space(5);
        
        // Show test instructions
        GUILayout.Label($"Test Controls:");
        GUILayout.Label($"  {testFaceLeftKey} = Face Left");
        GUILayout.Label($"  {testFaceRightKey} = Face Right");
        GUILayout.Label($"  {testResetKey} = Reset");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Quick test all directions
    /// </summary>
    [ContextMenu("Test All Directions")]
    public void TestAllDirections()
    {
        StartCoroutine(TestSequence());
    }
    
    System.Collections.IEnumerator TestSequence()
    {
        Debug.Log("🧪 Starting direction test sequence...");
        
        // Test left
        TestFaceLeft();
        yield return new WaitForSeconds(1f);
        
        // Test right
        TestFaceRight();
        yield return new WaitForSeconds(1f);
        
        // Test reset
        TestReset();
        yield return new WaitForSeconds(1f);
        
        Debug.Log("✅ Direction test sequence complete!");
    }
}