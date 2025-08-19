using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleMMOUISystem : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject gameWorldPanel;
    
    private Canvas mainCanvas;
    private Camera uiCamera;
    
    void Awake()
    {
        Debug.Log("🎮 SimpleMMOUISystem initializing...");
        SetupCamera();
        SetupCanvas();
        CreateAllUI();
        ShowLoginPanel();
    }
    
    void SetupCamera()
    {
        // Create proper 2D camera for top-down MMO view
        GameObject cameraObj = new GameObject("MMO Camera");
        uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.tag = "MainCamera";
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 5f;
        uiCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        uiCamera.clearFlags = CameraClearFlags.SolidColor;
        uiCamera.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("📷 MMO Camera created successfully");
    }
    
    void SetupCanvas()
    {
        // Create main canvas for all UI
        GameObject canvasObj = new GameObject("MMO Canvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 1;
        
        // Add canvas scaler for different resolutions
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add graphic raycaster for UI interactions
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("🎨 MMO Canvas created successfully");
    }
    
    void CreateAllUI()
    {
        CreateLoginPanel();
        CreateCharacterSelectPanel();
        CreateCharacterCreatePanel();
        CreateGameWorldPanel();
    }
    
    void CreateLoginPanel()
    {
        loginPanel = new GameObject("Login Panel");
        loginPanel.transform.SetParent(mainCanvas.transform, false);
        
        // Create background panel
        var background = CreatePanel(loginPanel, new Vector2(400, 500), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateText(loginPanel, "BLAZED ODYSSEY", new Vector2(0, 180), 32, new Color(1f, 0.6f, 0.0f, 1f));
        
        // Create username input
        var usernameInput = CreateInputField(loginPanel, "Username", new Vector2(0, 80));
        
        // Create password input
        var passwordInput = CreateInputField(loginPanel, "Password", new Vector2(0, 20));
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        
        // Create login button
        var loginButton = CreateButton(loginPanel, "LOGIN", new Vector2(0, -80), new Color(0.2f, 0.6f, 0.2f));
        loginButton.onClick.AddListener(() => {
            Debug.Log("🔐 Login button clicked!");
            ShowCharacterSelectPanel();
        });
        
        // Create register button
        var registerButton = CreateButton(loginPanel, "REGISTER", new Vector2(0, -120), new Color(0.2f, 0.4f, 0.8f));
        registerButton.onClick.AddListener(() => {
            Debug.Log("📝 Register button clicked!");
        });
        
        // Create exit button
        var exitButton = CreateButton(loginPanel, "EXIT", new Vector2(0, -160), new Color(0.6f, 0.2f, 0.2f));
        exitButton.onClick.AddListener(() => {
            Debug.Log("🚪 Exit button clicked!");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
        
        Debug.Log("🔐 Login panel created successfully");
    }
    
    void CreateCharacterSelectPanel()
    {
        characterSelectPanel = new GameObject("Character Select Panel");
        characterSelectPanel.transform.SetParent(mainCanvas.transform, false);
        characterSelectPanel.SetActive(false);
        
        // Create background panel
        var background = CreatePanel(characterSelectPanel, new Vector2(600, 700), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateText(characterSelectPanel, "SELECT CHARACTER", new Vector2(0, 280), 28, new Color(1f, 0.6f, 0.0f, 1f));
        
        // Create character slots
        for (int i = 0; i < 3; i++)
        {
            CreateCharacterSlot(characterSelectPanel, i);
        }
        
        // Create buttons
        var createButton = CreateButton(characterSelectPanel, "CREATE NEW", new Vector2(0, -100), new Color(0.2f, 0.6f, 0.2f));
        createButton.onClick.AddListener(() => {
            Debug.Log("🎨 Create character button clicked!");
            ShowCharacterCreatePanel();
        });
        
        var playButton = CreateButton(characterSelectPanel, "PLAY", new Vector2(0, -140), new Color(0.6f, 0.4f, 0.2f));
        playButton.onClick.AddListener(() => {
            Debug.Log("🎮 Play button clicked!");
            ShowGameWorldPanel();
        });
        
        var logoutButton = CreateButton(characterSelectPanel, "LOGOUT", new Vector2(0, -180), new Color(0.4f, 0.4f, 0.4f));
        logoutButton.onClick.AddListener(() => {
            Debug.Log("🚪 Logout button clicked!");
            ShowLoginPanel();
        });
        
        Debug.Log("👤 Character select panel created successfully");
    }
    
    void CreateCharacterSlot(GameObject parent, int slotIndex)
    {
        var slotObj = new GameObject($"Character Slot {slotIndex + 1}");
        slotObj.transform.SetParent(parent.transform, false);
        
        // Create slot background
        var slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        
        var slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = new Vector2(0, 100 - (slotIndex * 80));
        slotRect.sizeDelta = new Vector2(450, 70);
        
        // Add character info text
        CreateText(slotObj, $"Empty Slot {slotIndex + 1}", Vector2.zero, 18, Color.gray);
        
        // Add click handler
        var button = slotObj.AddComponent<Button>();
        button.onClick.AddListener(() => {
            Debug.Log($"Character slot {slotIndex + 1} clicked!");
        });
    }
    
    void CreateCharacterCreatePanel()
    {
        characterCreatePanel = new GameObject("Character Create Panel");
        characterCreatePanel.transform.SetParent(mainCanvas.transform, false);
        characterCreatePanel.SetActive(false);
        
        // Create background panel
        var background = CreatePanel(characterCreatePanel, new Vector2(500, 600), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateText(characterCreatePanel, "CREATE CHARACTER", new Vector2(0, 240), 28, new Color(1f, 0.6f, 0.0f, 1f));
        
        // Create character name input
        var nameInput = CreateInputField(characterCreatePanel, "Character Name", new Vector2(0, 140));
        
        // Create race selection
        CreateText(characterCreatePanel, "RACE:", new Vector2(-150, 80), 20, Color.white);
        string[] races = { "Human", "Elf", "Dwarf", "Orc" };
        for (int i = 0; i < 4; i++)
        {
            var raceButton = CreateButton(characterCreatePanel, races[i], new Vector2(-150 + (i * 80), 40), new Color(0.3f, 0.3f, 0.5f));
            raceButton.onClick.AddListener(() => Debug.Log($"Race {races[i]} selected!"));
        }
        
        // Create class selection
        CreateText(characterCreatePanel, "CLASS:", new Vector2(150, 80), 20, Color.white);
        string[] classes = { "Warrior", "Mage", "Archer", "Priest" };
        for (int i = 0; i < 4; i++)
        {
            var classButton = CreateButton(characterCreatePanel, classes[i], new Vector2(150, 40 - (i * 40)), new Color(0.3f, 0.5f, 0.3f));
            classButton.onClick.AddListener(() => Debug.Log($"Class {classes[i]} selected!"));
        }
        
        // Create buttons
        var createButton = CreateButton(characterCreatePanel, "CREATE", new Vector2(0, -120), new Color(0.2f, 0.6f, 0.2f));
        createButton.onClick.AddListener(() => {
            Debug.Log("🎨 Character created!");
            ShowCharacterSelectPanel();
        });
        
        var backButton = CreateButton(characterCreatePanel, "BACK", new Vector2(0, -160), new Color(0.4f, 0.4f, 0.4f));
        backButton.onClick.AddListener(() => {
            Debug.Log("⬅️ Back button clicked!");
            ShowCharacterSelectPanel();
        });
        
        Debug.Log("🎨 Character create panel created successfully");
    }
    
    void CreateGameWorldPanel()
    {
        gameWorldPanel = new GameObject("Game World Panel");
        gameWorldPanel.transform.SetParent(mainCanvas.transform, false);
        gameWorldPanel.SetActive(false);
        
        // Create background panel
        var background = CreatePanel(gameWorldPanel, new Vector2(800, 600), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateText(gameWorldPanel, "GAME WORLD", new Vector2(0, 250), 28, new Color(1f, 0.6f, 0.0f, 1f));
        
        // Create game world panels
        CreateSubPanel(gameWorldPanel, "Chat", new Vector2(300, 200), new Vector2(-400, -100), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        CreateSubPanel(gameWorldPanel, "Inventory", new Vector2(250, 300), new Vector2(400, 50), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        CreateSubPanel(gameWorldPanel, "Character", new Vector2(250, 300), new Vector2(-400, 100), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        CreateSubPanel(gameWorldPanel, "Map", new Vector2(300, 250), new Vector2(400, -150), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        // Create back button
        var backButton = CreateButton(gameWorldPanel, "BACK TO CHARACTER SELECT", new Vector2(0, -250), new Color(0.4f, 0.4f, 0.4f));
        backButton.onClick.AddListener(() => {
            Debug.Log("⬅️ Back to character select!");
            ShowCharacterSelectPanel();
        });
        
        Debug.Log("🌍 Game world panel created successfully");
    }
    
    // Helper methods
    GameObject CreatePanel(GameObject parent, Vector2 size, Color color)
    {
        var panel = new GameObject("Panel");
        panel.transform.SetParent(parent.transform, false);
        
        var image = panel.AddComponent<Image>();
        image.color = color;
        
        var rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        
        return panel;
    }
    
    GameObject CreateSubPanel(GameObject parent, string name, Vector2 size, Vector2 position, Color color)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        
        var image = panel.AddComponent<Image>();
        image.color = color;
        
        var rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        // Add title
        CreateText(panel, name, new Vector2(0, size.y/2 - 20), 16, Color.white);
        
        return panel;
    }
    
    TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
    {
        var inputObj = new GameObject("Input Field");
        inputObj.transform.SetParent(parent.transform, false);
        
        var input = inputObj.AddComponent<TMP_InputField>();
        
        // Create background
        var background = new GameObject("Background");
        background.transform.SetParent(inputObj.transform, false);
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(300, 40);
        
        // Create text area
        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(background.transform, false);
        var textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchoredPosition = Vector2.zero;
        textAreaRect.sizeDelta = new Vector2(280, 30);
        
        // Create text component
        var textComponent = new GameObject("Text");
        textComponent.transform.SetParent(textArea.transform, false);
        var text = textComponent.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.color = Color.white;
        text.fontSize = 18;
        
        var textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(280, 30);
        
        // Create placeholder
        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.fontSize = 18;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchoredPosition = Vector2.zero;
        placeholderRect.sizeDelta = new Vector2(280, 30);
        
        // Setup input field
        input.textComponent = text;
        input.placeholder = placeholderText;
        input.textViewport = textAreaRect;
        
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchoredPosition = position;
        inputRect.sizeDelta = new Vector2(300, 40);
        
        return input;
    }
    
    Button CreateButton(GameObject parent, string text, Vector2 position, Color color)
    {
        var buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(parent.transform, false);
        
        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = color;
        
        // Create text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.fontSize = 18;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(200, 40);
        
        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(200, 40);
        
        return button;
    }
    
    TextMeshProUGUI CreateText(GameObject parent, string text, Vector2 position, int fontSize, Color color)
    {
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = FontStyles.Bold;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(400, 50);
        
        return textComponent;
    }
    
    // Panel management
    void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(false);
        gameWorldPanel.SetActive(false);
        Debug.Log("🔐 Showing login panel");
    }
    
    void ShowCharacterSelectPanel()
    {
        loginPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        characterCreatePanel.SetActive(false);
        gameWorldPanel.SetActive(false);
        Debug.Log("👤 Showing character select panel");
    }
    
    void ShowCharacterCreatePanel()
    {
        loginPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(true);
        gameWorldPanel.SetActive(false);
        Debug.Log("🎨 Showing character create panel");
    }
    
    void ShowGameWorldPanel()
    {
        loginPanel.SetActive(false);
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(false);
        gameWorldPanel.SetActive(true);
        Debug.Log("🌍 Showing game world panel");
    }
    
    void Update()
    {
        // Disable debug function keys
    }
}
