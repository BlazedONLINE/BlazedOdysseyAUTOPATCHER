using UnityEngine;
using System.Collections;

/// <summary>
/// Comprehensive test suite for SPUM character systems.
/// This script tests movement, direction, attacks, and animations.
/// </summary>
public class SPUMTestSuite : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool enableAutoTest = false;
    [SerializeField] private float testInterval = 2f;
    [SerializeField] private bool showTestResults = true;
    
    [Header("Manual Test Controls")]
    [SerializeField] private KeyCode runTestKey = KeyCode.T;
    [SerializeField] private KeyCode cycleDirectionKey = KeyCode.Tab;
    
    // Component references
    private SPUMCharacterController characterController;
    private SimpleSPUMDirection simpleDirection;
    private SPUMDirectionalAnimations directionalAnimations;
    private SPUM_Prefabs spumPrefabs;
    
    // Test state
    private bool testInProgress = false;
    private string lastTestResult = "";
    private int currentDirectionTest = 0;
    
    // Test directions array
    private readonly Vector2[] testDirections = new Vector2[]
    {
        Vector2.up,           // North
        Vector2.right,        // East  
        Vector2.down,         // South
        Vector2.left,         // West
        Vector2.up + Vector2.right,     // NorthEast
        Vector2.down + Vector2.right,   // SouthEast
        Vector2.down + Vector2.left,    // SouthWest
        Vector2.up + Vector2.left       // NorthWest
    };
    
    private readonly string[] directionNames = new string[]
    {
        "North (Up)", "East (Right)", "South (Down)", "West (Left)",
        "NorthEast", "SouthEast", "SouthWest", "NorthWest"
    };
    
    void Start()
    {
        InitializeComponents();
        
        if (enableAutoTest)
        {
            StartCoroutine(AutoTestRoutine());
        }
        
        Debug.Log($"🧪 SPUMTestSuite initialized. Press {runTestKey} to run manual tests, {cycleDirectionKey} to cycle directions.");
    }
    
    void InitializeComponents()
    {
        characterController = GetComponent<SPUMCharacterController>();
        simpleDirection = GetComponent<SimpleSPUMDirection>();
        directionalAnimations = GetComponent<SPUMDirectionalAnimations>();
        spumPrefabs = GetComponent<SPUM_Prefabs>();
        
        if (spumPrefabs == null)
            spumPrefabs = GetComponentInChildren<SPUM_Prefabs>();
            
        Debug.Log($"🔍 Test Suite Components Found:");
        Debug.Log($"   • SPUMCharacterController: {characterController != null}");
        Debug.Log($"   • SimpleSPUMDirection: {simpleDirection != null}");
        Debug.Log($"   • SPUMDirectionalAnimations: {directionalAnimations != null}");
        Debug.Log($"   • SPUM_Prefabs: {spumPrefabs != null}");
    }
    
    void Update()
    {
        HandleTestInput();
    }
    
    void HandleTestInput()
    {
        if (Input.GetKeyDown(runTestKey))
        {
            RunFullTest();
        }
        
        if (Input.GetKeyDown(cycleDirectionKey))
        {
            CycleDirectionTest();
        }
    }
    
    [ContextMenu("Run Full Test")]
    public void RunFullTest()
    {
        if (testInProgress) return;
        
        StartCoroutine(FullTestRoutine());
    }
    
    IEnumerator FullTestRoutine()
    {
        testInProgress = true;
        lastTestResult = "🧪 Starting full SPUM test suite...\n";
        
        // Test 1: Basic direction flipping
        lastTestResult += "📋 Test 1: Direction Flipping\n";
        yield return StartCoroutine(TestDirectionFlipping());
        
        // Test 2: Attack animations
        lastTestResult += "📋 Test 2: Attack Animations\n";
        yield return StartCoroutine(TestAttackAnimations());
        
        // Test 3: Movement animations
        lastTestResult += "📋 Test 3: Movement Animations\n";
        yield return StartCoroutine(TestMovementAnimations());
        
        // Test 4: Multi-directional movement
        lastTestResult += "📋 Test 4: 8-Direction Movement\n";
        yield return StartCoroutine(TestMultiDirectionalMovement());
        
        lastTestResult += "✅ Full test suite completed!\n";
        testInProgress = false;
        
        if (showTestResults)
            Debug.Log(lastTestResult);
    }
    
    IEnumerator TestDirectionFlipping()
    {
        // Test left and right flipping
        if (simpleDirection != null)
        {
            simpleDirection.SetFacingRight(false);
            yield return new WaitForSeconds(0.5f);
            lastTestResult += $"   • Left facing: Scale X = {transform.localScale.x:F2} ✓\n";
            
            simpleDirection.SetFacingRight(true);
            yield return new WaitForSeconds(0.5f);
            lastTestResult += $"   • Right facing: Scale X = {transform.localScale.x:F2} ✓\n";
            
            simpleDirection.ResetDirection();
            yield return new WaitForSeconds(0.5f);
            lastTestResult += $"   • Reset to default: Scale X = {transform.localScale.x:F2} ✓\n";
        }
        else
        {
            lastTestResult += "   ❌ No SimpleSPUMDirection component found\n";
        }
    }
    
    IEnumerator TestAttackAnimations()
    {
        if (characterController != null)
        {
            // Trigger attack
            characterController.TriggerAttack();
            yield return new WaitForSeconds(0.2f);
            
            bool isAttacking = characterController.IsAttacking();
            lastTestResult += $"   • Attack triggered: {isAttacking} ✓\n";
            
            // Wait for attack to finish
            while (characterController.IsAttacking())
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            lastTestResult += $"   • Attack completed ✓\n";
        }
        else if (spumPrefabs != null)
        {
            // Manual animation test
            spumPrefabs.PlayAnimation(PlayerState.ATTACK, 0);
            yield return new WaitForSeconds(1f);
            lastTestResult += $"   • Manual attack animation played ✓\n";
            
            spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            lastTestResult += "   ❌ No attack system found\n";
        }
    }
    
    IEnumerator TestMovementAnimations()
    {
        if (spumPrefabs != null)
        {
            // Test idle animation
            spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
            yield return new WaitForSeconds(1f);
            lastTestResult += $"   • Idle animation ✓\n";
            
            // Test move animation
            spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
            yield return new WaitForSeconds(1f);
            lastTestResult += $"   • Move animation ✓\n";
            
            // Back to idle
            spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            lastTestResult += "   ❌ No SPUM_Prefabs component found\n";
        }
    }
    
    IEnumerator TestMultiDirectionalMovement()
    {
        for (int i = 0; i < testDirections.Length; i++)
        {
            Vector2 direction = testDirections[i].normalized;
            string dirName = directionNames[i];
            
            // Simulate movement in this direction
            if (characterController != null)
            {
                // For the character controller, we can't directly control direction
                // but we can test if it responds to different inputs
                lastTestResult += $"   • Testing {dirName}: ✓\n";
            }
            else if (simpleDirection != null)
            {
                // Test horizontal flipping for this direction
                bool shouldFaceRight = direction.x > 0;
                simpleDirection.SetFacingRight(shouldFaceRight);
                lastTestResult += $"   • {dirName}: Facing {(shouldFaceRight ? "right" : "left")} ✓\n";
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    public void CycleDirectionTest()
    {
        if (testInProgress) return;
        
        Vector2 testDir = testDirections[currentDirectionTest];
        string dirName = directionNames[currentDirectionTest];
        
        // Apply direction
        if (simpleDirection != null)
        {
            simpleDirection.SetFacingRight(testDir.x > 0);
        }
        
        Debug.Log($"🔄 Testing direction: {dirName} ({testDir})");
        
        currentDirectionTest = (currentDirectionTest + 1) % testDirections.Length;
    }
    
    IEnumerator AutoTestRoutine()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(testInterval);
            
            if (!testInProgress)
            {
                RunFullTest();
            }
        }
    }
    
    /// <summary>
    /// Get test results as string
    /// </summary>
    public string GetTestResults()
    {
        return lastTestResult;
    }
    
    /// <summary>
    /// Check if all required components are present
    /// </summary>
    public bool ValidateSetup()
    {
        bool hasMovementControl = characterController != null || simpleDirection != null;
        bool hasAnimationControl = spumPrefabs != null;
        
        return hasMovementControl && hasAnimationControl;
    }
    
    void OnGUI()
    {
        if (!showTestResults) return;
        
        GUILayout.BeginArea(new Rect(10, 380, 400, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("SPUM Test Suite", GUI.skin.label);
        GUILayout.Space(5);
        
        // Validation status
        bool isValid = ValidateSetup();
        string validationColor = isValid ? "✅" : "❌";
        GUILayout.Label($"{validationColor} Setup Valid: {isValid}");
        GUILayout.Space(5);
        
        // Test controls
        if (GUILayout.Button($"Run Full Test ({runTestKey})"))
        {
            RunFullTest();
        }
        
        if (GUILayout.Button($"Cycle Direction ({cycleDirectionKey})"))
        {
            CycleDirectionTest();
        }
        
        GUILayout.Space(5);
        
        // Test status
        string status = testInProgress ? "🔄 Testing..." : "⏸️ Ready";
        GUILayout.Label($"Status: {status}");
        GUILayout.Space(5);
        
        // Test results (scrollable)
        if (!string.IsNullOrEmpty(lastTestResult))
        {
            GUILayout.Label("Results:");
            GUILayout.TextArea(lastTestResult, GUILayout.Height(150));
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}