using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Simple tool to add SPUMCombatBootstrap to the right places
/// </summary>
public class SPUMCombatBootstrapTool : EditorWindow
{
    private Vector2 scrollPosition;
    private bool foundAnyTargets = false;
    
    [MenuItem("Tools/Add SPUM Combat Bootstrap")]
    public static void ShowWindow()
    {
        GetWindow<SPUMCombatBootstrapTool>("SPUM Combat Bootstrap Tool");
    }
    
    void OnGUI()
    {
        GUILayout.Label("SPUM Combat Bootstrap Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("This tool automatically finds and adds SPUMCombatBootstrap to:\n" +
                               "• Player GameObjects\n" +
                               "• Objects with SPUM_Prefabs components\n" +
                               "• Objects with SPUMCharacterController\n" +
                               "• SPUM prefabs in the project", MessageType.Info);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Scan and Add Combat Bootstrap", GUILayout.Height(30)))
        {
            ScanAndAddCombatBootstrap();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add to Selected Objects Only", GUILayout.Height(25)))
        {
            AddToSelectedObjects();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Find All SPUM Objects (Preview)", GUILayout.Height(25)))
        {
            PreviewSPUMObjects();
        }
        
        if (foundAnyTargets)
        {
            GUILayout.Space(10);
            GUILayout.Label("Preview Results:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            // Show current SPUM objects in scene
            var spumObjects = FindAllSPUMObjects();
            foreach (var obj in spumObjects)
            {
                bool hasBootstrap = obj.GetComponent<SPUMCombatBootstrap>() != null;
                GUIStyle style = hasBootstrap ? EditorStyles.label : EditorStyles.boldLabel;
                Color originalColor = GUI.color;
                GUI.color = hasBootstrap ? Color.green : Color.yellow;
                
                EditorGUILayout.LabelField($"{obj.name} {(hasBootstrap ? "[Has Bootstrap]" : "[Needs Bootstrap]")}", style);
                GUI.color = originalColor;
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    void ScanAndAddCombatBootstrap()
    {
        Debug.Log("🔍 Scanning for SPUM objects that need combat bootstrap...");
        
        var spumObjects = FindAllSPUMObjects();
        int added = 0;
        
        foreach (var obj in spumObjects)
        {
            if (AddCombatBootstrapToObject(obj))
            {
                added++;
            }
        }
        
        // Also check prefabs
        int prefabsProcessed = ProcessSPUMPrefabs();
        
        EditorUtility.DisplayDialog("Combat Bootstrap Added", 
            $"Added SPUMCombatBootstrap to:\n" +
            $"• {added} scene objects\n" +
            $"• {prefabsProcessed} prefabs", "OK");
        
        Debug.Log($"✅ Added combat bootstrap to {added} scene objects and {prefabsProcessed} prefabs");
    }
    
    void AddToSelectedObjects()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects to add combat bootstrap to.", "OK");
            return;
        }
        
        int added = 0;
        foreach (var obj in selectedObjects)
        {
            if (AddCombatBootstrapToObject(obj))
            {
                added++;
            }
        }
        
        EditorUtility.DisplayDialog("Combat Bootstrap Added", 
            $"Added SPUMCombatBootstrap to {added} out of {selectedObjects.Length} selected objects.", "OK");
    }
    
    void PreviewSPUMObjects()
    {
        foundAnyTargets = true;
        Repaint();
    }
    
    GameObject[] FindAllSPUMObjects()
    {
        var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        return allObjects.Where(obj => 
            obj.GetComponent<SPUM_Prefabs>() != null ||
            obj.GetComponent<SPUMCharacterController>() != null ||
            obj.CompareTag("Player") ||
            obj.name.ToLower().Contains("spum") ||
            obj.name.ToLower().Contains("player")
        ).ToArray();
    }
    
    bool AddCombatBootstrapToObject(GameObject obj)
    {
        if (obj == null) return false;
        
        // Check if it already has the component
        if (obj.GetComponent<SPUMCombatBootstrap>() != null)
        {
            return false; // Already has it
        }
        
        // Determine if this object should have combat bootstrap
        bool shouldHaveBootstrap = ShouldObjectHaveCombatBootstrap(obj);
        
        if (shouldHaveBootstrap)
        {
            // Add the component
            var bootstrap = obj.AddComponent<SPUMCombatBootstrap>();
            
            // Mark object as dirty for saving
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(obj);
            }
            
            Debug.Log($"✅ Added SPUMCombatBootstrap to {obj.name}");
            return true;
        }
        
        return false;
    }
    
    bool ShouldObjectHaveCombatBootstrap(GameObject obj)
    {
        // Has SPUM components
        if (obj.GetComponent<SPUM_Prefabs>() != null) return true;
        
        // Has SPUM character controller
        if (obj.GetComponent<SPUMCharacterController>() != null) return true;
        
        // Is tagged as Player
        if (obj.CompareTag("Player")) return true;
        
        // Has "player" or "spum" in name
        string name = obj.name.ToLower();
        if (name.Contains("player") || name.Contains("spum")) return true;
        
        // Has any of the combat components already
        if (obj.GetComponent<SPUMMeleeAttack>() != null) return true;
        
        return false;
    }
    
    int ProcessSPUMPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/SPUM" });
        int processed = 0;
        
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && ShouldObjectHaveCombatBootstrap(prefab))
            {
                // Check if prefab already has the component
                if (prefab.GetComponent<SPUMCombatBootstrap>() == null)
                {
                    try
                    {
                        // Load prefab for editing
                        GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
                        
                        // Add component
                        prefabContents.AddComponent<SPUMCombatBootstrap>();
                        
                        // Save changes
                        PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                        PrefabUtility.UnloadPrefabContents(prefabContents);
                        
                        processed++;
                        Debug.Log($"✅ Added SPUMCombatBootstrap to prefab: {prefab.name}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"❌ Failed to add combat bootstrap to prefab {prefab.name}: {e.Message}");
                    }
                }
            }
        }
        
        if (processed > 0)
        {
            AssetDatabase.Refresh();
        }
        
        return processed;
    }
    
    [MenuItem("Tools/Add Combat Bootstrap to Selected")]
    public static void AddToSelected()
    {
        var tool = GetWindow<SPUMCombatBootstrapTool>();
        tool.AddToSelectedObjects();
    }
    
    [MenuItem("GameObject/SPUM/Add Combat Bootstrap", false, 10)]
    public static void AddToSelectedGameObject()
    {
        var selected = Selection.activeGameObject;
        if (selected != null)
        {
            if (selected.GetComponent<SPUMCombatBootstrap>() == null)
            {
                selected.AddComponent<SPUMCombatBootstrap>();
                Debug.Log($"✅ Added SPUMCombatBootstrap to {selected.name}");
            }
            else
            {
                Debug.Log($"ℹ️ {selected.name} already has SPUMCombatBootstrap");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No GameObject selected");
        }
    }
}