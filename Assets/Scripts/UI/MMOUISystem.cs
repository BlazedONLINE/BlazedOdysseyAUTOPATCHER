using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MMOUISystem : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject gameWorldPanel;
    public GameObject settingsPanel;
    
    [Header("Login UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button registerButton;
    public Button exitButton;
    public TextMeshProUGUI statusText;
    
    [Header("Character Select UI")]
    public Transform characterSlotContainer;
    public GameObject characterSlotPrefab;
    public Button createNewCharacterButton;
    public Button playButton;
    public Button deleteCharacterButton;
    public Button logoutButton;
    
    [Header("Character Create UI")]
    public TMP_InputField characterNameInput;
    public Button[] raceButtons;
    public Button[] classButtons;
    public Button createCharacterButton;
    public Button backToSelectButton;
    public Image characterPreviewImage;
    
    [Header("Game World UI")]
    public GameObject chatPanel;
    public GameObject inventoryPanel;
    public GameObject characterPanel;
    public GameObject mapPanel;
    public GameObject partyPanel;
    
    private Canvas mainCanvas;
    private Camera uiCamera;
    private string currentUsername;
    private bool isLoggedIn = false;
    
    void Awake()
    {
        Debug.Log("🎮 MMOUISystem initializing...");
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
        uiCamera.cullingMask = LayerMask.GetMask("Default", "UI");
        
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
        CreateSettingsPanel();
    }
    
    void CreateLoginPanel()
    {
        loginPanel = new GameObject("Login Panel");
        loginPanel.transform.SetParent(mainCanvas.transform, false);
        
        // Create background
        var background = CreatePanelBackground(loginPanel, new Vector2(400, 500), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateTitle(loginPanel, "BLAZED ODYSSEY", new Vector2(0, 180));
        
        // Create input fields
        usernameInput = CreateInputField(loginPanel, "Username", new Vector2(0, 80));
        passwordInput = CreateInputField(loginPanel, "Password", new Vector2(0, 20));
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        
        // Create remember me toggle
        rememberMeToggle = CreateToggle(loginPanel, "Remember Me", new Vector2(0, -20));
        
        // Create buttons
        loginButton = CreateButton(loginPanel, "LOGIN", new Vector2(0, -80), new Color(0.2f, 0.6f, 0.2f));
        registerButton = CreateButton(loginPanel, "REGISTER", new Vector2(0, -120), new Color(0.2f, 0.4f, 0.8f));
        exitButton = CreateButton(loginPanel, "EXIT", new Vector2(0, -160), new Color(0.6f, 0.2f, 0.2f));
        
        // Create status text
        statusText = CreateText(loginPanel, "", new Vector2(0, -200), 16, Color.white);
        
        // Setup button events
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        
        Debug.Log("🔐 Login panel created successfully");
    }
    
    void CreateCharacterSelectPanel()
    {
        characterSelectPanel = new GameObject("Character Select Panel");
        characterSelectPanel.transform.SetParent(mainCanvas.transform, false);
        characterSelectPanel.SetActive(false);
        
        // Create background
        var background = CreatePanelBackground(characterSelectPanel, new Vector2(600, 700), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateTitle(characterSelectPanel, "SELECT CHARACTER", new Vector2(0, 280));
        
        // Create character slots container
        var slotContainerObj = new GameObject("Character Slots Container");
        slotContainerObj.transform.SetParent(characterSelectPanel.transform, false);
        characterSlotContainer = slotContainerObj.transform;
        
        var slotRect = slotContainerObj.AddComponent<RectTransform>();
        slotRect.anchoredPosition = new Vector2(0, 100);
        slotRect.sizeDelta = new Vector2(500, 300);
        
        // Create character slots (3 slots for MMO)
        for (int i = 0; i < 3; i++)
        {
            CreateCharacterSlot(i);
        }
        
        // Create buttons
        createNewCharacterButton = CreateButton(characterSelectPanel, "CREATE NEW", new Vector2(0, -100), new Color(0.2f, 0.6f, 0.2f));
        playButton = CreateButton(characterSelectPanel, "PLAY", new Vector2(0, -140), new Color(0.6f, 0.4f, 0.2f));
        deleteCharacterButton = CreateButton(characterSelectPanel, "DELETE", new Vector2(0, -180), new Color(0.6f, 0.2f, 0.2f));
        logoutButton = CreateButton(characterSelectPanel, "LOGOUT", new Vector2(0, -220), new Color(0.4f, 0.4f, 0.4f));
        
        // Setup button events
        createNewCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        playButton.onClick.AddListener(OnPlayClicked);
        deleteCharacterButton.onClick.AddListener(OnDeleteCharacterClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        
        Debug.Log("👤 Character select panel created successfully");
    }
    
    void CreateCharacterSlot(int slotIndex)
    {
        var slotObj = new GameObject($"Character Slot {slotIndex + 1}");
        slotObj.transform.SetParent(characterSlotContainer, false);
        
        var slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        
        var slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchoredPosition = new Vector2(0, 100 - (slotIndex * 80));
        slotRect.sizeDelta = new Vector2(450, 70);
        
        // Add character info text
        var infoText = CreateText(slotObj, $"Empty Slot {slotIndex + 1}", Vector2.zero, 18, Color.gray);
        
        // Add click handler
        var button = slotObj.AddComponent<Button>();
        button.onClick.AddListener(() => OnCharacterSlotClicked(slotIndex));
    }
    
    void CreateCharacterCreatePanel()
    {
        characterCreatePanel = new GameObject("Character Create Panel");
        characterCreatePanel.transform.SetParent(mainCanvas.transform, false);
        characterCreatePanel.SetActive(false);
        
        // Create background
        var background = CreatePanelBackground(characterCreatePanel, new Vector2(500, 600), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateTitle(characterCreatePanel, "CREATE CHARACTER", new Vector2(0, 240));
        
        // Create character name input
        characterNameInput = CreateInputField(characterCreatePanel, "Character Name", new Vector2(0, 140));
        
        // Create race selection
        CreateText(characterCreatePanel, "RACE:", new Vector2(-150, 80), 20, Color.white);
        raceButtons = new Button[4];
        string[] races = { "Human", "Elf", "Dwarf", "Orc" };
        for (int i = 0; i < 4; i++)
        {
            raceButtons[i] = CreateButton(characterCreatePanel, races[i], new Vector2(-150 + (i * 80), 40), new Color(0.3f, 0.3f, 0.5f));
        }
        
        // Create class selection
        CreateText(characterCreatePanel, "CLASS:", new Vector2(150, 80), 20, Color.white);
        classButtons = new Button[4];
        string[] classes = { "Warrior", "Mage", "Archer", "Priest" };
        for (int i = 0; i < 4; i++)
        {
            classButtons[i] = CreateButton(characterCreatePanel, classes[i], new Vector2(150, 40 - (i * 40)), new Color(0.3f, 0.5f, 0.3f));
        }
        
        // Create character preview
        var previewObj = new GameObject("Character Preview");
        previewObj.transform.SetParent(characterCreatePanel.transform, false);
        characterPreviewImage = previewObj.AddComponent<Image>();
        characterPreviewImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        var previewRect = previewObj.GetComponent<RectTransform>();
        previewRect.anchoredPosition = new Vector2(0, -20);
        previewRect.sizeDelta = new Vector2(100, 100);
        
        // Create buttons
        createCharacterButton = CreateButton(characterCreatePanel, "CREATE", new Vector2(0, -120), new Color(0.2f, 0.6f, 0.2f));
        backToSelectButton = CreateButton(characterCreatePanel, "BACK", new Vector2(0, -160), new Color(0.4f, 0.4f, 0.4f));
        
        // Setup button events
        createCharacterButton.onClick.AddListener(OnCreateCharacterFinalClicked);
        backToSelectButton.onClick.AddListener(OnBackToSelectClicked);
        
        Debug.Log("🎨 Character create panel created successfully");
    }
    
    void CreateGameWorldPanel()
    {
        gameWorldPanel = new GameObject("Game World Panel");
        gameWorldPanel.transform.SetParent(mainCanvas.transform, false);
        gameWorldPanel.SetActive(false);
        
        // Create chat panel (bottom left)
        chatPanel = CreateSubPanel(gameWorldPanel, "Chat Panel", new Vector2(300, 200), new Vector2(-800, -400), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        // Create inventory panel (right side)
        inventoryPanel = CreateSubPanel(gameWorldPanel, "Inventory Panel", new Vector2(250, 400), new Vector2(800, 0), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        // Create character panel (top left)
        characterPanel = CreateSubPanel(gameWorldPanel, "Character Panel", new Vector2(250, 300), new Vector2(-800, 300), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        // Create map panel (top right)
        mapPanel = CreateSubPanel(gameWorldPanel, "Map Panel", new Vector2(300, 250), new Vector2(800, 300), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        // Create party panel (bottom right)
        partyPanel = CreateSubPanel(gameWorldPanel, "Party Panel", new Vector2(250, 200), new Vector2(800, -400), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        
        Debug.Log("🌍 Game world panel created successfully");
    }
    
    void CreateSettingsPanel()
    {
        settingsPanel = new GameObject("Settings Panel");
        settingsPanel.transform.SetParent(mainCanvas.transform, false);
        settingsPanel.SetActive(false);
        
        // Create background
        var background = CreatePanelBackground(settingsPanel, new Vector2(400, 500), new Color(0.05f, 0.05f, 0.1f, 0.95f));
        
        // Create title
        CreateTitle(settingsPanel, "SETTINGS", new Vector2(0, 200));
        
        // Create settings options
        CreateText(settingsPanel, "Graphics Quality:", new Vector2(-100, 120), 18, Color.white);
        CreateText(settingsPanel, "Sound Volume:", new Vector2(-100, 80), 18, Color.white);
        CreateText(settingsPanel, "Music Volume:", new Vector2(-100, 40), 18, Color.white);
        
        // Create close button
        var closeButton = CreateButton(settingsPanel, "CLOSE", new Vector2(0, -200), new Color(0.4f, 0.4f, 0.4f));
        closeButton.onClick.AddListener(() => settingsPanel.SetActive(false));
        
        Debug.Log("⚙️ Settings panel created successfully");
    }
    
    // Helper methods for creating UI elements
    GameObject CreatePanelBackground(GameObject parent, Vector2 size, Color color)
    {
        var background = new GameObject("Background");
        background.transform.SetParent(parent.transform, false);
        
        var image = background.AddComponent<Image>();
        image.color = color;
        
        var rect = background.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        
        return background;
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
    
    Toggle CreateToggle(GameObject parent, string label, Vector2 position)
    {
        var toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent.transform, false);
        
        var toggle = toggleObj.AddComponent<Toggle>();
        
        // Create background
        var background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(20, 20);
        
        // Create checkmark
        var checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        var checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchoredPosition = Vector2.zero;
        checkRect.sizeDelta = new Vector2(16, 16);
        
        // Create label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.color = Color.white;
        labelText.fontSize = 16;
        
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(30, 0);
        labelRect.sizeDelta = new Vector2(200, 20);
        
        // Setup toggle
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        
        var toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(250, 20);
        
        return toggle;
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
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(400, 50);
        
        return textComponent;
    }
    
    void CreateTitle(GameObject parent, string title, Vector2 position)
    {
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 32;
        titleText.color = new Color(1f, 0.6f, 0.0f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = position;
        titleRect.sizeDelta = new Vector2(400, 50);
    }
    
    // UI Event Handlers
    void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter username and password";
            statusText.color = Color.red;
            return;
        }
        
        // Simulate login success
        currentUsername = username;
        isLoggedIn = true;
        statusText.text = "Login successful!";
        statusText.color = Color.green;
        
        StartCoroutine(DelayedShowCharacterSelect());
    }
    
    void OnRegisterClicked()
    {
        statusText.text = "Registration feature coming soon!";
        statusText.color = Color.yellow;
    }
    
    void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnCreateCharacterClicked()
    {
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(true);
    }
    
    void OnPlayClicked()
    {
        characterSelectPanel.SetActive(false);
        gameWorldPanel.SetActive(true);
        Debug.Log("🎮 Entering game world...");
    }
    
    void OnDeleteCharacterClicked()
    {
        statusText.text = "Delete character feature coming soon!";
        statusText.color = Color.yellow;
    }
    
    void OnLogoutClicked()
    {
        isLoggedIn = false;
        currentUsername = "";
        ShowLoginPanel();
    }
    
    void OnCreateCharacterFinalClicked()
    {
        string characterName = characterNameInput.text;
        
        if (string.IsNullOrEmpty(characterName))
        {
            statusText.text = "Please enter a character name";
            statusText.color = Color.red;
            return;
        }
        
        // Simulate character creation
        statusText.text = $"Character {characterName} created successfully!";
        statusText.color = Color.green;
        
        StartCoroutine(DelayedShowCharacterSelect());
    }
    
    void OnBackToSelectClicked()
    {
        characterCreatePanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }
    
    void OnCharacterSlotClicked(int slotIndex)
    {
        Debug.Log($"Character slot {slotIndex + 1} clicked");
        // TODO: Implement character selection logic
    }
    
    IEnumerator DelayedShowCharacterSelect()
    {
        yield return new WaitForSeconds(1f);
        ShowCharacterSelectPanel();
    }
    
    void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(false);
        gameWorldPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    
    void ShowCharacterSelectPanel()
    {
        loginPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
        characterCreatePanel.SetActive(false);
        gameWorldPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    
    void Update()
    {
        // Quick test keys
        // Disable debug toggles: F1-F4
        // if (Input.GetKeyDown(KeyCode.F1)) { ShowLoginPanel(); }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowCharacterSelectPanel();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            gameWorldPanel.SetActive(!gameWorldPanel.activeInHierarchy);
        }
        // if (Input.GetKeyDown(KeyCode.F4)) { settingsPanel.SetActive(!settingsPanel.activeInHierarchy); }
    }
}
