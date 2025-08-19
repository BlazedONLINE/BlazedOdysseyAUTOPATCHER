using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class UITestHelper : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🧪 UITestHelper: Testing UI interactivity...");
        
        // Test if we can find UI elements
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log($"🔘 Found {buttons.Length} buttons in scene");
        
        foreach (var button in buttons)
        {
            Debug.Log($"  - Button: {button.name}, Interactable: {button.interactable}");
        }
        
        // Test if we can find input fields
        var inputFields = FindObjectsOfType<TMPro.TMP_InputField>();
        Debug.Log($"📝 Found {inputFields.Length} input fields in scene");
        
        foreach (var input in inputFields)
        {
            Debug.Log($"  - Input: {input.name}, Interactable: {input.interactable}");
        }
        
        // Test if we can find toggles
        Toggle[] toggles = FindObjectsOfType<Toggle>();
        Debug.Log($"🔘 Found {toggles.Length} toggles in scene");
        
        foreach (var toggle in toggles)
        {
            Debug.Log($"  - Toggle: {toggle.name}, Interactable: {toggle.interactable}");
        }
        
        // Test EventSystem
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log("✅ EventSystem found and working");
        }
        else
        {
            Debug.LogWarning("⚠️ No EventSystem found - UI won't be interactive!");
        }
        
        // Test InputSystemUIInputModule
        var inputModule = FindObjectOfType<InputSystemUIInputModule>();
        if (inputModule != null)
        {
            Debug.Log("✅ InputSystemUIInputModule found and working");
        }
        else
        {
            Debug.LogWarning("⚠️ No InputSystemUIInputModule found - UI won't be interactive!");
        }
        
        // Test GraphicRaycaster
        var raycaster = FindObjectOfType<GraphicRaycaster>();
        if (raycaster != null)
        {
            Debug.Log("✅ GraphicRaycaster found and working");
        }
        else
        {
            Debug.LogWarning("⚠️ No GraphicRaycaster found - UI won't be interactive!");
        }
    }
    
    [ContextMenu("Test UI Elements")]
    void TestUIElements()
    {
        Start();
    }
}
