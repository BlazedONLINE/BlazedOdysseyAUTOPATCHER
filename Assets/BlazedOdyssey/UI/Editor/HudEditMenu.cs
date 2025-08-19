#nullable enable
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BlazedOdyssey.UI.Editor
{
    public static class HudEditMenu
    {
        [MenuItem("Tools/Blazed Odyssey/HUD/Toggle Edit Mode", false, 1)]
        static void ToggleEditMode()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("HUD Edit Mode", "This feature only works in Play Mode!", "OK");
                return;
            }

            var hudEditMode = Object.FindObjectOfType<HudEditMode>();
            if (hudEditMode == null)
            {
                Debug.LogWarning("[HUD] No HudEditMode component found in the scene. Add one to a Canvas to enable HUD editing.");
                return;
            }

            // Use reflection to toggle the private _editing field
            var editingField = typeof(HudEditMode).GetField("_editing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (editingField != null)
            {
                bool currentState = (bool)editingField.GetValue(hudEditMode);
                editingField.SetValue(hudEditMode, !currentState);
                Debug.Log($"[HUD] Edit Mode toggled to: {(!currentState ? "ON" : "OFF")}");
            }
            else
            {
                Debug.LogError("[HUD] Could not access HudEditMode._editing field");
            }
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Toggle Edit Mode", true)]
        static bool ToggleEditModeValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Clear Saved Layout", false, 2)]
        static void ClearSavedLayout()
        {
            if (EditorUtility.DisplayDialog("Clear HUD Layout", 
                "This will permanently delete the saved HUD layout. Continue?", 
                "Yes, Clear", "Cancel"))
            {
                try
                {
                    var filePath = System.IO.Path.Combine(Application.persistentDataPath, "hud_layout.json");
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        Debug.Log($"[HUD] Cleared saved layout file: {filePath}");
                        EditorUtility.DisplayDialog("Success", "HUD layout cleared successfully!", "OK");
                    }
                    else
                    {
                        Debug.Log("[HUD] No saved layout file found to clear");
                        EditorUtility.DisplayDialog("Info", "No saved layout file found.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[HUD] Failed to clear layout: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to clear layout: {ex.Message}", "OK");
                }
            }
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Apply Saved Layout", false, 3)]
        static void ApplySavedLayout()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Apply HUD Layout", "This feature only works in Play Mode!", "OK");
                return;
            }

            try
            {
                HudEditMode.ForceApplySavedLayout();
                Debug.Log("[HUD] Applied saved layout to all HudMovable components");
                EditorUtility.DisplayDialog("Success", "HUD layout applied successfully!", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HUD] Failed to apply layout: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to apply layout: {ex.Message}", "OK");
            }
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Apply Saved Layout", true)]
        static bool ApplySavedLayoutValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Create Layout Wrapper", false, 10)]
        static void CreateLayoutWrapper()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Create Layout Wrapper", "Please select a GameObject first!", "OK");
                return;
            }

            var rectTransform = selected.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                EditorUtility.DisplayDialog("Create Layout Wrapper", "Selected GameObject must have a RectTransform!", "OK");
                return;
            }

            var hudMovable = selected.GetComponent<HudMovable>();
            if (hudMovable == null)
            {
                EditorUtility.DisplayDialog("Create Layout Wrapper", "Selected GameObject should have a HudMovable component for this to be useful!", "OK");
                return;
            }

            try
            {
                Undo.RegisterCompleteObjectUndo(selected.transform.parent, "Create Layout Wrapper");
                var wrapper = LayoutWrapper.CreateNeutralWrapper(selected.transform, $"{selected.name}_Wrapper");
                Undo.RegisterCreatedObjectUndo(wrapper, "Create Layout Wrapper");
                
                Selection.activeGameObject = wrapper;
                EditorGUIUtility.PingObject(wrapper);
                
                EditorUtility.DisplayDialog("Success", $"Created layout wrapper '{wrapper.name}' successfully!", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HUD] Failed to create wrapper: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create wrapper: {ex.Message}", "OK");
            }
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Create Layout Wrapper", true)]
        static bool CreateLayoutWrapperValidation()
        {
            var selected = Selection.activeGameObject;
            return selected != null && selected.GetComponent<RectTransform>() != null;
        }

        [MenuItem("Tools/Blazed Odyssey/HUD/Show Layout Info", false, 20)]
        static void ShowLayoutInfo()
        {
            var hudMovables = Object.FindObjectsOfType<HudMovable>();
            if (hudMovables.Length == 0)
            {
                EditorUtility.DisplayDialog("HUD Layout Info", "No HudMovable components found in the scene.", "OK");
                return;
            }

            var info = "HudMovable Components Found:\n\n";
            foreach (var hm in hudMovables.OrderBy(h => h.Id))
            {
                var pos = hm.Rect.anchoredPosition;
                var parent = hm.transform.parent;
                var parentName = parent ? parent.name : "None";
                var hasLayoutGroup = parent && parent.GetComponent<UnityEngine.UI.LayoutGroup>() != null;
                
                info += $"• {hm.Id}\n";
                info += $"  Position: ({pos.x:F1}, {pos.y:F1})\n";
                info += $"  Parent: {parentName}";
                if (hasLayoutGroup) info += " ⚠️ Has LayoutGroup";
                info += "\n\n";
            }

            var filePath = System.IO.Path.Combine(Application.persistentDataPath, "hud_layout.json");
            var fileExists = System.IO.File.Exists(filePath);
            info += $"Saved Layout File: {(fileExists ? "Found" : "Not Found")}\n";
            if (fileExists)
            {
                info += $"Path: {filePath}";
            }

            EditorUtility.DisplayDialog("HUD Layout Info", info, "OK");
        }
    }
}