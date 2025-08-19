#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to help set up SPUM direction controllers on existing SPUM characters.
/// This script provides menu options and automation for adding direction handling to SPUM characters.
/// </summary>
public class SpumDirectionSetup : EditorWindow
{
    private static Vector2 scrollPosition;
    
    [MenuItem("SPUM/Fix Direction Controllers")]
    public static void ShowWindow()
    {
        GetWindow<SpumDirectionSetup>("SPUM Direction Setup");
    }
    
    [MenuItem("SPUM/Quick Fix/Add Full Character Controllers to Scene", false, 1)]
    public static void AddFullCharacterControllersToScene()
    {
        int fixedCount = 0;
        
        // Find all SPUM_Prefabs in the scene
        SPUM_Prefabs[] spumCharacters = FindObjectsOfType<SPUM_Prefabs>();
        
        foreach (var spumPrefab in spumCharacters)
        {
            GameObject spumObject = spumPrefab.gameObject;
            
            // Check if it already has any controller
            if (spumObject.GetComponent<SPUMCharacterController>() == null)
            {
                // Add the comprehensive character controller
                var controller = spumObject.AddComponent<SPUMCharacterController>();
                
                Debug.Log($"✅ Added SPUMCharacterController to {spumObject.name}");
                fixedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(spumObject);
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"🎯 Successfully added full character controllers to {fixedCount} SPUM character(s)!");
        }
        else
        {
            Debug.Log("ℹ️ No SPUM characters found that needed character controllers.");
        }
    }
    
    [MenuItem("SPUM/Quick Fix/Add Simple Direction Controllers to Scene", false, 2)]
    public static void AddSimpleDirectionControllersToScene()
    {
        int fixedCount = 0;
        
        // Find all SPUM_Prefabs in the scene
        SPUM_Prefabs[] spumCharacters = FindObjectsOfType<SPUM_Prefabs>();
        
        foreach (var spumPrefab in spumCharacters)
        {
            GameObject spumObject = spumPrefab.gameObject;
            
            // Check if it already has any direction controller
            if (spumObject.GetComponent<SimpleSPUMDirection>() == null && 
                spumObject.GetComponent<SPUMDirectionController>() == null && 
                spumObject.GetComponent<SPUMMovementSync>() == null &&
                spumObject.GetComponent<SPUMCharacterController>() == null)
            {
                // Add the simple direction controller (most compatible)
                var controller = spumObject.AddComponent<SimpleSPUMDirection>();
                
                Debug.Log($"✅ Added SimpleSPUMDirection to {spumObject.name}");
                fixedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(spumObject);
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"🎯 Successfully added simple direction controllers to {fixedCount} SPUM character(s)!");
        }
        else
        {
            Debug.Log("ℹ️ No SPUM characters found that needed direction controllers.");
        }
    }
    
    [MenuItem("SPUM/Quick Fix/Add Advanced Direction Controllers to Scene", false, 2)]
    public static void AddAdvancedDirectionControllersToScene()
    {
        int fixedCount = 0;
        
        // Find all SPUM_Prefabs in the scene
        SPUM_Prefabs[] spumCharacters = FindObjectsOfType<SPUM_Prefabs>();
        
        foreach (var spumPrefab in spumCharacters)
        {
            GameObject spumObject = spumPrefab.gameObject;
            
            // Check if it already has a direction controller
            if (spumObject.GetComponent<SPUMDirectionController>() == null && 
                spumObject.GetComponent<SPUMMovementSync>() == null &&
                spumObject.GetComponent<SimpleSPUMDirection>() == null)
            {
                // Add the advanced direction controller
                var controller = spumObject.AddComponent<SPUMDirectionController>();
                
                Debug.Log($"✅ Added SPUMDirectionController to {spumObject.name}");
                fixedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(spumObject);
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"🎯 Successfully added advanced direction controllers to {fixedCount} SPUM character(s)!");
        }
        else
        {
            Debug.Log("ℹ️ No SPUM characters found that needed direction controllers.");
        }
    }
    
    [MenuItem("SPUM/Quick Fix/Add Alternative Movement Sync to Scene", false, 3)]
    public static void AddMovementSyncToScene()
    {
        int fixedCount = 0;
        
        // Find all SPUM_Prefabs in the scene
        SPUM_Prefabs[] spumCharacters = FindObjectsOfType<SPUM_Prefabs>();
        
        foreach (var spumPrefab in spumCharacters)
        {
            GameObject spumObject = spumPrefab.gameObject;
            
            // Check if it already has a movement sync controller
            if (spumObject.GetComponent<SPUMMovementSync>() == null)
            {
                // Remove any existing direction controller to avoid conflicts
                var existingController = spumObject.GetComponent<SPUMDirectionController>();
                if (existingController != null)
                {
                    DestroyImmediate(existingController);
                    Debug.Log($"🔄 Replaced SPUMDirectionController with SPUMMovementSync on {spumObject.name}");
                }
                
                var existingSimple = spumObject.GetComponent<SimpleSPUMDirection>();
                if (existingSimple != null)
                {
                    DestroyImmediate(existingSimple);
                    Debug.Log($"🔄 Replaced SimpleSPUMDirection with SPUMMovementSync on {spumObject.name}");
                }
                
                // Add the movement sync controller
                var movementSync = spumObject.AddComponent<SPUMMovementSync>();
                
                // Configure with reasonable defaults
                movementSync.directionSettings.enableHorizontalFlipping = true;
                movementSync.directionSettings.enableVerticalFlipping = false;
                movementSync.directionSettings.movementThreshold = 0.1f;
                movementSync.enableDebugLogs = false;
                
                fixedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(spumObject);
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"🎯 Successfully added movement sync controllers to {fixedCount} SPUM character(s)!");
        }
        else
        {
            Debug.Log("ℹ️ No SPUM characters found that needed movement sync controllers.");
        }
    }
    
    [MenuItem("SPUM/Debug/List All SPUM Characters", false, 10)]
    public static void ListAllSpumCharacters()
    {
        SPUM_Prefabs[] spumCharacters = FindObjectsOfType<SPUM_Prefabs>();
        
        Debug.Log($"🔍 Found {spumCharacters.Length} SPUM character(s) in scene:");
        
        foreach (var spumPrefab in spumCharacters)
        {
            GameObject spumObject = spumPrefab.gameObject;
            
            bool hasCharacterController = spumObject.GetComponent<SPUMCharacterController>() != null;
            bool hasSimpleDirection = spumObject.GetComponent<SimpleSPUMDirection>() != null;
            bool hasDirectionController = spumObject.GetComponent<SPUMDirectionController>() != null;
            bool hasMovementSync = spumObject.GetComponent<SPUMMovementSync>() != null;
            bool hasPlayerObj = spumObject.GetComponent<PlayerObj>() != null;
            
            string status = "";
            if (hasCharacterController) status += "[SPUMCharacterController] ";
            if (hasSimpleDirection) status += "[SimpleSPUMDirection] ";
            if (hasDirectionController) status += "[SPUMDirectionController] ";
            if (hasMovementSync) status += "[SPUMMovementSync] ";
            if (hasPlayerObj) status += "[PlayerObj] ";
            if (string.IsNullOrEmpty(status)) status = "[No Control]";
            
            Debug.Log($"  • {spumObject.name} {status}");
        }
    }
    
    void OnGUI()
    {
        GUILayout.Label("SPUM Direction Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool helps fix SPUM characters that don't turn when walking. " +
            "The issue occurs when SPUM sprites don't flip direction based on movement input.",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Quick fixes section
        GUILayout.Label("Quick Fixes", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Full Character Controllers (Recommended)"))
        {
            AddFullCharacterControllersToScene();
        }
        
        EditorGUILayout.HelpBox(
            "This adds SPUMCharacterController to all SPUM characters in the scene. " +
            "This provides complete movement, direction, and attack control with spacebar attacks and WASD movement.",
            MessageType.None);
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Add Simple Direction Controllers"))
        {
            AddSimpleDirectionControllersToScene();
        }
        
        EditorGUILayout.HelpBox(
            "This adds SimpleSPUMDirection to all SPUM characters in the scene. " +
            "Use this if you only need direction flipping without attacks or custom movement.",
            MessageType.None);
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Add Advanced Direction Controllers"))
        {
            AddAdvancedDirectionControllersToScene();
        }
        
        EditorGUILayout.HelpBox(
            "This adds SPUMDirectionController with advanced features. " +
            "Use this if you need more control or integration with specific controllers.",
            MessageType.None);
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Add Alternative Movement Sync Controllers"))
        {
            AddMovementSyncToScene();
        }
        
        EditorGUILayout.HelpBox(
            "This adds SPUMMovementSync controllers instead. Use this if the regular " +
            "direction controllers don't work with your movement system.",
            MessageType.None);
        
        GUILayout.Space(10);
        
        // Debug section
        GUILayout.Label("Debug & Information", EditorStyles.boldLabel);
        
        if (GUILayout.Button("List All SPUM Characters in Scene"))
        {
            ListAllSpumCharacters();
        }
        
        EditorGUILayout.HelpBox(
            "Shows all SPUM characters found in the scene and their current direction control status.",
            MessageType.None);
        
        GUILayout.Space(10);
        
        // Manual instructions
        GUILayout.Label("Manual Setup Instructions", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "If the automatic fixes don't work:\n\n" +
            "1. Select your SPUM character in the scene\n" +
            "2. In the Inspector, click 'Add Component'\n" +
            "3. Search for one of these:\n" +
            "   • 'SimpleSPUMDirection' (recommended - most compatible)\n" +
            "   • 'SPUMDirectionController' (advanced features)\n" +
            "   • 'SPUMMovementSync' (alternative approach)\n" +
            "4. Add the component\n" +
            "5. Test movement - the character should now turn when walking\n\n" +
            "For characters spawned at runtime, the SpawnPlayerFromSelection script " +
            "has been updated to automatically add SimpleSPUMDirection controllers.",
            MessageType.None);
        
        EditorGUILayout.EndScrollView();
    }
}
#endif