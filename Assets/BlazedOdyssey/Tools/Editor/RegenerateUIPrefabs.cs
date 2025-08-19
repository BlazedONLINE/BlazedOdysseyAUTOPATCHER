using UnityEditor;
using UnityEngine;

namespace BlazedOdyssey.Tools.Editor
{
    public static class RegenerateUIPrefabs
    {
        [MenuItem("BlazedOdyssey/UI/Regenerate All UI Prefabs")]
        public static void RegenerateAllUIPrefabs()
        {
            Debug.Log("🔄 Regenerating all UI prefabs with latest changes...");
            
            // Clean up duplicates first
            CleanupAllDuplicateUI();
            
            // Regenerate Login Canvas with Tab Navigation
            Debug.Log("📝 Regenerating LoginCanvas...");
            LoginUiBuilder.BuildLogin();
            
            // Regenerate Signup Canvas with fixed back button and Tab Navigation  
            Debug.Log("📝 Regenerating SignupCanvas...");
            SignupUiBuilder.BuildSignup();
            
            Debug.Log("✅ All UI prefabs regenerated successfully!");
            Debug.Log("🎯 Changes include:");
            Debug.Log("   • Automatic duplicate UI cleanup");
            Debug.Log("   • Fixed canvas positioning below background");
            Debug.Log("   • Fixed back button logic in SignupController");
            Debug.Log("   • Added Tab navigation to both login and signup forms");
            Debug.Log("   • Updated button binding and validation");
        }
        
        [MenuItem("BlazedOdyssey/UI/Clean Up Duplicate UI")]
        public static void CleanupAllDuplicateUI()
        {
            Debug.Log("🧹 Cleaning up all duplicate UI canvases...");
            
            var allCanvases = Object.FindObjectsOfType<Canvas>();
            int totalRemoved = 0;
            
            var duplicateGroups = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Canvas>>();
            
            foreach (var canvas in allCanvases)
            {
                string baseName = canvas.gameObject.name;
                // Remove (Clone) suffix and numbers
                baseName = baseName.Replace("(Clone)", "").Trim();
                if (baseName.EndsWith(")"))
                {
                    int lastParen = baseName.LastIndexOf("(");
                    if (lastParen > 0)
                    {
                        baseName = baseName.Substring(0, lastParen).Trim();
                    }
                }
                
                if (!duplicateGroups.ContainsKey(baseName))
                {
                    duplicateGroups[baseName] = new System.Collections.Generic.List<Canvas>();
                }
                duplicateGroups[baseName].Add(canvas);
            }
            
            foreach (var group in duplicateGroups)
            {
                if (group.Value.Count > 1)
                {
                    Debug.Log($"🔍 Found {group.Value.Count} instances of '{group.Key}'");
                    
                    // Keep the first one, remove the rest
                    for (int i = 1; i < group.Value.Count; i++)
                    {
                        Debug.Log($"🗑️ Removing duplicate: {group.Value[i].gameObject.name}");
                        Object.DestroyImmediate(group.Value[i].gameObject);
                        totalRemoved++;
                    }
                }
            }
            
            if (totalRemoved > 0)
            {
                Debug.Log($"✅ Cleaned up {totalRemoved} duplicate UI canvases");
            }
            else
            {
                Debug.Log("ℹ️ No duplicate UI canvases found");
            }
        }
        
        [MenuItem("BlazedOdyssey/UI/Regenerate Login Canvas")]
        public static void RegenerateLoginCanvas()
        {
            Debug.Log("🔄 Regenerating LoginCanvas prefab...");
            LoginUiBuilder.BuildLogin();
            Debug.Log("✅ LoginCanvas regenerated with Tab navigation!");
        }
        
        [MenuItem("BlazedOdyssey/UI/Regenerate Signup Canvas")]  
        public static void RegenerateSignupCanvas()
        {
            Debug.Log("🔄 Regenerating SignupCanvas prefab...");
            SignupUiBuilder.BuildSignup();
            Debug.Log("✅ SignupCanvas regenerated with fixed back button and Tab navigation!");
        }
    }
}