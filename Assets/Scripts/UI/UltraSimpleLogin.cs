using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltraSimpleLogin : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public Button registerButton;
    public Button exitButton;
    public TextMeshProUGUI statusText;
    
    void Start()
    {
        Debug.Log("🚀 UltraSimpleLogin starting...");
        CreateCamera();
        CreateProperLoginUI();
        Debug.Log("✅ UltraSimpleLogin ready!");
    }
    
    void CreateCamera()
    {
        // Check if there's already a camera
        if (Camera.main == null)
        {
            Debug.Log("📷 Creating main camera...");
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.tag = "MainCamera";
            
            // Add AudioListener
            cameraObj.AddComponent<AudioListener>();
            
            // Position camera for 2D view
            cameraObj.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }
        else
        {
            Debug.Log("📷 Main camera already exists");
        }
    }
    
    void CreateProperLoginUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("MMO Login Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        
        // Add CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background
        CreateBackground(canvasObj);
        
        // Create login panel
        CreateLoginPanel(canvasObj);
        
        // Create EventSystem if none exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("✅ EventSystem created");
        }
    }
    
    void CreateBackground(GameObject canvas)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f);
        
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }
    
    void CreateLoginPanel(GameObject canvas)
    {
        // Main panel
        GameObject panel = new GameObject("Login Panel");
        panel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 500);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Title
        CreateText(panel, "BLAZED ODYSSEY", new Vector2(0, 180), 32, Color.yellow);
        
        // Email input
        emailField = CreateInputField(panel, "Email", new Vector2(0, 100));
        
        // Password input
        passwordField = CreateInputField(panel, "Password", new Vector2(0, 20));
        passwordField.contentType = TMP_InputField.ContentType.Password;
        
        // Login button
        loginButton = CreateButton(panel, "LOGIN", new Vector2(0, -80), Color.green);
        loginButton.onClick.AddListener(OnLoginClicked);
        
        // Register button
        registerButton = CreateButton(panel, "REGISTER", new Vector2(-100, -140), Color.magenta);
        registerButton.onClick.AddListener(OnRegisterClicked);
        
        // Exit button
        exitButton = CreateButton(panel, "EXIT", new Vector2(100, -140), Color.red);
        exitButton.onClick.AddListener(OnExitClicked);
        
        // Status text
        statusText = CreateText(panel, "", new Vector2(0, -180), 16, Color.white);
    }
    
    TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
    {
        GameObject inputObj = new GameObject("Input Field");
        inputObj.transform.SetParent(parent.transform, false);
        
        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
        
        // Background
        Image bgImage = inputObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchoredPosition = position;
        inputRect.sizeDelta = new Vector2(300, 40);
        
        // Text component
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        // Configure input field
        input.textComponent = text;
        input.placeholder = placeholderText;
        
        return input;
    }
    
    Button CreateButton(GameObject parent, string text, Vector2 position, Color color)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(parent.transform, false);
        
        Button button = btnObj.AddComponent<Button>();
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(120, 40);
        
        // Button text
        CreateText(btnObj, text, Vector2.zero, 18, Color.white);
        
        return button;
    }
    
    TextMeshProUGUI CreateText(GameObject parent, string text, Vector2 position, int fontSize, Color color)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(300, 50);
        
        return textComponent;
    }
    
    // Button event handlers
    void OnLoginClicked()
    {
        Debug.Log("🎮 Login clicked!");
        if (string.IsNullOrEmpty(emailField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            statusText.text = "Please enter both email and password";
            statusText.color = Color.red;
        }
        else
        {
            statusText.text = "Login successful!";
            statusText.color = Color.green;
        }
    }
    
    void OnRegisterClicked()
    {
        Debug.Log("👤 Register clicked!");
        statusText.text = "Registration coming soon...";
        statusText.color = Color.magenta;
    }
    
    void OnExitClicked()
    {
        Debug.Log("🚪 Exit clicked!");
        Application.Quit();
    }
}
