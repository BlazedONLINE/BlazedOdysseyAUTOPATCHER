using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to help setup combat capabilities on SPUM prefabs
/// </summary>
public class SPUMCombatSetupEditor : EditorWindow
{
    private bool setupAllSPUMPrefabs = true;
    private bool addCombatBootstrap = true;
    private bool addMeleeAttack = true;
    private bool addCharacterController = true;
    private int baseDamage = 5;
    private float attackRange = 1.5f;
    private float attackArc = 90f;
    private bool showProgress = false;
    
    [MenuItem("SPUM/Combat Setup")]
    public static void ShowWindow()
    {
        GetWindow<SPUMCombatSetupEditor>("SPUM Combat Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("SPUM Combat Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will add combat capabilities to SPUM character prefabs.", EditorStyles.helpBox);
        GUILayout.Space(10);
        
        setupAllSPUMPrefabs = EditorGUILayout.Toggle("Setup All SPUM Prefabs", setupAllSPUMPrefabs);
        
        GUILayout.Space(10);
        GUILayout.Label("Components to Add:", EditorStyles.boldLabel);
        
        addCombatBootstrap = EditorGUILayout.Toggle("SPUMCombatBootstrap", addCombatBootstrap);
        addMeleeAttack = EditorGUILayout.Toggle("SPUMMeleeAttack", addMeleeAttack);
        addCharacterController = EditorGUILayout.Toggle("SPUMCharacterController", addCharacterController);
        
        GUILayout.Space(10);
        GUILayout.Label("Combat Settings:", EditorStyles.boldLabel);
        
        baseDamage = EditorGUILayout.IntField("Base Damage", baseDamage);
        attackRange = EditorGUILayout.FloatField("Attack Range", attackRange);
        attackArc = EditorGUILayout.FloatField("Attack Arc (degrees)", attackArc);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Setup Combat on Selected Prefabs", GUILayout.Height(30)))
        {
            SetupCombatOnSelected();
        }
        
        GUILayout.Space(10);
        
        if (setupAllSPUMPrefabs && GUILayout.Button("Setup Combat on All SPUM Prefabs", GUILayout.Height(30)))
        {
            SetupCombatOnAllSPUMPrefabs();
        }
        
        if (showProgress)
        {
            GUILayout.Space(10);
            GUILayout.Label("Processing... Please wait.", EditorStyles.helpBox);
        }
    }
    
    void SetupCombatOnSelected()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more SPUM prefabs to setup combat on.", "OK");
            return;
        }
        
        showProgress = true;
        int processed = 0;
        
        foreach (var obj in selectedObjects)
        {
            if (SetupCombatOnPrefab(obj))
            {
                processed++;
            }
        }
        
        showProgress = false;
        EditorUtility.DisplayDialog("Combat Setup Complete", $"Setup combat on {processed} out of {selectedObjects.Length} selected objects.", "OK");
    }
    
    void SetupCombatOnAllSPUMPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/SPUM/Resources/Units" });
        
        if (prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No SPUM Prefabs Found", "No SPUM prefabs found in Assets/SPUM/Resources/Units", "OK");
            return;
        }
        
        showProgress = true;
        int processed = 0;
        
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && HasSPUMComponents(prefab))
            {
                EditorUtility.DisplayProgressBar("Setting up Combat", $"Processing {prefab.name}...", (float)i / prefabGuids.Length);
                
                if (SetupCombatOnPrefab(prefab))
                {
                    processed++;
                }
            }
        }
        
        EditorUtility.ClearProgressBar();
        showProgress = false;
        EditorUtility.DisplayDialog("Combat Setup Complete", $"Setup combat on {processed} SPUM prefabs.", "OK");
    }
    
    bool SetupCombatOnPrefab(GameObject prefab)
    {
        try
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning($"Could not get asset path for {prefab.name}");
                return false;
            }
            
            // Load the prefab for editing
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            bool modified = false;
            
            // Add SPUMCombatBootstrap
            if (addCombatBootstrap && prefabContents.GetComponent<SPUMCombatBootstrap>() == null)
            {
                var bootstrap = prefabContents.AddComponent<SPUMCombatBootstrap>();
                // Configure bootstrap via reflection since fields might be private
                var type = typeof(SPUMCombatBootstrap);
                var damageField = type.GetField("baseDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var rangeField = type.GetField("attackRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var arcField = type.GetField("attackArc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                damageField?.SetValue(bootstrap, baseDamage);
                rangeField?.SetValue(bootstrap, attackRange);
                arcField?.SetValue(bootstrap, attackArc);
                
                modified = true;
                Debug.Log($"Added SPUMCombatBootstrap to {prefab.name}");
            }
            
            // Add SPUMMeleeAttack
            if (addMeleeAttack && prefabContents.GetComponent<SPUMMeleeAttack>() == null)
            {
                prefabContents.AddComponent<SPUMMeleeAttack>();
                modified = true;
                Debug.Log($"Added SPUMMeleeAttack to {prefab.name}");
            }
            
            // Add SPUMCharacterController
            if (addCharacterController && prefabContents.GetComponent<SPUMCharacterController>() == null)
            {
                prefabContents.AddComponent<SPUMCharacterController>();
                modified = true;
                Debug.Log($"Added SPUMCharacterController to {prefab.name}");
            }
            
            // Ensure proper collider
            if (prefabContents.GetComponent<Collider2D>() == null)
            {
                var collider = prefabContents.AddComponent<CapsuleCollider2D>();
                collider.size = new Vector2(0.6f, 0.8f);
                collider.direction = CapsuleDirection2D.Vertical;
                modified = true;
                Debug.Log($"Added CapsuleCollider2D to {prefab.name}");
            }
            
            // Ensure Rigidbody2D
            if (prefabContents.GetComponent<Rigidbody2D>() == null)
            {
                var rb = prefabContents.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                modified = true;
                Debug.Log($"Added Rigidbody2D to {prefab.name}");
            }
            
            if (modified)
            {
                // Save the changes
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                Debug.Log($"✅ Successfully setup combat on {prefab.name}");
            }
            
            // Unload the prefab contents
            PrefabUtility.UnloadPrefabContents(prefabContents);
            
            return modified;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up combat on {prefab.name}: {e.Message}");
            return false;
        }
    }
    
    bool HasSPUMComponents(GameObject prefab)
    {
        return prefab.GetComponent<SPUM_Prefabs>() != null || 
               prefab.GetComponentInChildren<SPUM_Prefabs>() != null ||
               prefab.name.Contains("SPUM");
    }
}