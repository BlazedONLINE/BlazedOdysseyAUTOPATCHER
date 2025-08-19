using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleWorkingUI : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🚀 SimpleWorkingUI starting...");
        CreateSimpleUI();
    }
    
    void CreateSimpleUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Working Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        
        // Add CanvasScaler
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("✅ Canvas created successfully");
        
        // Create Background Panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvasObj.transform, false);
        
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Debug.Log("✅ Background created successfully");
        
        // Create Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BLAZED ODYSSEY MMO";
        titleText.fontSize = 48;
        titleText.color = Color.yellow;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.offsetMin = new Vector2(-200, -50);
        titleRect.offsetMax = new Vector2(200, 50);
        
        Debug.Log("✅ Title created successfully");
        
        // Create Login Button
        GameObject loginBtn = new GameObject("Login Button");
        loginBtn.transform.SetParent(canvasObj.transform, false);
        
        var btnImage = loginBtn.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        // Create text as a child object
        GameObject btnTextObj = new GameObject("Button Text");
        btnTextObj.transform.SetParent(loginBtn.transform, false);
        var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "LOGIN";
        btnText.fontSize = 24;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        
        // Position the text in the center of the button
        var btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        var btnRect = loginBtn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.6f);
        btnRect.offsetMin = new Vector2(-100, -25);
        btnRect.offsetMax = new Vector2(100, 25);
        
        var btn = loginBtn.AddComponent<Button>();
        btn.onClick.AddListener(() => Debug.Log("🎮 Login button clicked!"));
        
        Debug.Log("✅ Login button created successfully");
        
        // Create Status Text
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(canvasObj.transform, false);
        
        var statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "UI System Working! Press F1-F5 for different panels";
        statusText.fontSize = 18;
        statusText.color = Color.cyan;
        statusText.alignment = TextAlignmentOptions.Center;
        
        var statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.3f);
        statusRect.anchorMax = new Vector2(0.5f, 0.4f);
        statusRect.offsetMin = new Vector2(-300, -25);
        statusRect.offsetMax = new Vector2(300, 25);
        
        Debug.Log("✅ Status text created successfully");
        
        Debug.Log("🎉 ALL UI ELEMENTS CREATED SUCCESSFULLY!");
    }
}
