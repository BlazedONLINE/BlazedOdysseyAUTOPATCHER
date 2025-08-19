using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles Tab key navigation through input fields for better UX
/// Add this to any GameObject with input fields you want to navigate through
/// </summary>
public class TabNavigation : MonoBehaviour
{
    [Header("Input Fields (in tab order)")]
    [SerializeField] private List<Selectable> selectables = new List<Selectable>();
    
    [Header("Settings")]
    [SerializeField] private bool autoFindInputFields = true;
    [SerializeField] private bool debugMode = false;
    
    private int currentIndex = 0;
    
    void Start()
    {
        if (autoFindInputFields && selectables.Count == 0)
        {
            AutoFindInputFields();
        }
        
        if (debugMode)
        {
            Debug.Log($"📋 TabNavigation found {selectables.Count} input fields");
        }
    }
    
    void Update()
    {
        HandleTabNavigation();
    }
    
    private void HandleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (selectables.Count == 0) return;
            
            // Check if Shift is held for reverse navigation
            bool reverse = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            if (reverse)
            {
                // Navigate backwards
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = selectables.Count - 1;
            }
            else
            {
                // Navigate forwards
                currentIndex++;
                if (currentIndex >= selectables.Count)
                    currentIndex = 0;
            }
            
            // Select the input field
            var selectable = selectables[currentIndex];
            if (selectable != null && selectable.gameObject.activeInHierarchy)
            {
                selectable.Select();
                
                // For input fields, also focus them
                if (selectable is TMP_InputField inputField)
                {
                    inputField.ActivateInputField();
                }
                else if (selectable is InputField legacyInput)
                {
                    legacyInput.ActivateInputField();
                }
                
                if (debugMode)
                {
                    Debug.Log($"🔍 Tab navigation: Selected {selectable.name} (index {currentIndex})");
                }
            }
        }
        
        // Handle Enter key to move to next field or submit
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Move to next field, or trigger submit on last field
            if (currentIndex < selectables.Count - 1)
            {
                currentIndex++;
                var selectable = selectables[currentIndex];
                if (selectable != null && selectable.gameObject.activeInHierarchy)
                {
                    selectable.Select();
                    if (selectable is TMP_InputField inputField)
                    {
                        inputField.ActivateInputField();
                    }
                }
            }
            else
            {
                // On last field, try to find and click a submit button
                var submitButton = GameObject.FindGameObjectWithTag("SubmitButton");
                if (submitButton == null)
                {
                    // Try common button names
                    submitButton = GameObject.Find("CreateButton") ?? 
                                 GameObject.Find("LoginButton") ?? 
                                 GameObject.Find("SubmitButton");
                }
                
                if (submitButton != null)
                {
                    var button = submitButton.GetComponent<Button>();
                    if (button != null && button.interactable)
                    {
                        button.onClick.Invoke();
                        if (debugMode) Debug.Log("🚀 Enter key triggered submit button");
                    }
                }
            }
        }
    }
    
    private void AutoFindInputFields()
    {
        selectables.Clear();
        
        // Find all TMP_InputField components in children
        var tmpInputs = GetComponentsInChildren<TMP_InputField>();
        foreach (var input in tmpInputs)
        {
            if (input.interactable && input.gameObject.activeInHierarchy)
            {
                selectables.Add(input);
            }
        }
        
        // Find all legacy InputField components in children
        var legacyInputs = GetComponentsInChildren<InputField>();
        foreach (var input in legacyInputs)
        {
            if (input.interactable && input.gameObject.activeInHierarchy)
            {
                selectables.Add(input);
            }
        }
        
        // Sort by Y position (top to bottom) then X position (left to right)
        selectables.Sort((a, b) =>
        {
            var aRect = a.GetComponent<RectTransform>();
            var bRect = b.GetComponent<RectTransform>();
            
            if (aRect == null || bRect == null) return 0;
            
            // Compare Y positions first (higher Y = higher on screen = earlier in tab order)
            float yDiff = bRect.anchoredPosition.y - aRect.anchoredPosition.y;
            if (Mathf.Abs(yDiff) > 50f) // If significant Y difference
            {
                return yDiff > 0 ? -1 : 1;
            }
            
            // If similar Y positions, compare X positions (left to right)
            float xDiff = aRect.anchoredPosition.x - bRect.anchoredPosition.x;
            return xDiff < 0 ? -1 : 1;
        });
    }
    
    [ContextMenu("Refresh Input Fields")]
    public void RefreshInputFields()
    {
        AutoFindInputFields();
        Debug.Log($"🔄 Refreshed tab navigation: {selectables.Count} input fields found");
    }
    
    public void AddInputField(Selectable selectable)
    {
        if (!selectables.Contains(selectable))
        {
            selectables.Add(selectable);
        }
    }
    
    public void RemoveInputField(Selectable selectable)
    {
        selectables.Remove(selectable);
    }
}