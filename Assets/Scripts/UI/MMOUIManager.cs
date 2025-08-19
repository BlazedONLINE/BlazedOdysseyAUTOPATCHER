using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class MMOUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;
    public TMP_InputField usernameField;
    public Button loginButton;
    public Button registerButton;
    public Button exitButton;
    public Button settingsButton;
    public Button characterSelectButton;
    public Button createCharacterButton;
    public Button backButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI titleText;
    
    [Header("Screen Management")]
    public GameObject loginScreen;
    public GameObject registerScreen;
    public GameObject characterSelectScreen;
    public GameObject characterCreationScreen;
    public GameObject settingsScreen;
    public GameObject mainMenuScreen;
    
    [Header("Character Creation")]
    public TMP_InputField characterNameField;
    public TMP_Dropdown characterClassDropdown;
    public Button generateCharacterButton;
    public Button saveCharacterButton;
    public Image characterPreviewImage;
    
    [Header("Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;
    public Button applySettingsButton;
    
    private GameObject currentScreen;
    private List<GameObject> allScreens = new List<GameObject>();
    
    void Start()
    {
        Debug.Log("🚀 MMOUIManager starting...");
        CreateCamera();
        CreateAllScreens();
        ShowScreen(loginScreen);
        Debug.Log("✅ MMOUIManager ready!");
    }
    
    void CreateCamera()
    {
        if (Camera.main == null)
        {
            Debug.Log("📷 Creating main camera...");
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();
            cameraObj.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }
    }
    
    void CreateAllScreens()
    {
        CreateCanvas();
        CreateLoginScreen();
        CreateRegisterScreen();
        CreateCharacterSelectScreen();
        CreateCharacterCreationScreen();
        CreateSettingsScreen();
        CreateMainMenuScreen();
        CreateEventSystem();
    }
    
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("MMO UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f);
        
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }
    
    void CreateLoginScreen()
    {
        loginScreen = CreateScreen("Login Screen");
        
        // Title
        titleText = CreateText(loginScreen, "BLAZED ODYSSEY", new Vector2(0, 180), 32, Color.yellow);
        
        // Email input
        emailField = CreateInputField(loginScreen, "Email", new Vector2(0, 100));
        
        // Password input
        passwordField = CreateInputField(loginScreen, "Password", new Vector2(0, 20));
        passwordField.contentType = TMP_InputField.ContentType.Password;
        
        // Login button
        loginButton = CreateButton(loginScreen, "LOGIN", new Vector2(0, -80), Color.green);
        loginButton.onClick.AddListener(OnLoginClicked);
        
        // Register button
        registerButton = CreateButton(loginScreen, "REGISTER", new Vector2(-100, -140), Color.magenta);
        registerButton.onClick.AddListener(OnRegisterClicked);
        
        // Settings button
        settingsButton = CreateButton(loginScreen, "SETTINGS", new Vector2(100, -140), Color.blue);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        
        // Exit button
        exitButton = CreateButton(loginScreen, "EXIT", new Vector2(0, -200), Color.red);
        exitButton.onClick.AddListener(OnExitClicked);
        
        // Status text
        statusText = CreateText(loginScreen, "", new Vector2(0, -240), 16, Color.white);
    }
    
    void CreateRegisterScreen()
    {
        registerScreen = CreateScreen("Register Screen");
        
        // Title
        CreateText(registerScreen, "CREATE ACCOUNT", new Vector2(0, 180), 32, Color.yellow);
        
        // Username input
        usernameField = CreateInputField(registerScreen, "Username", new Vector2(0, 120));
        
        // Email input
        TMP_InputField regEmailField = CreateInputField(registerScreen, "Email", new Vector2(0, 60));
        
        // Password input
        TMP_InputField regPasswordField = CreateInputField(registerScreen, "Password", new Vector2(0, 0));
        regPasswordField.contentType = TMP_InputField.ContentType.Password;
        
        // Confirm password input
        confirmPasswordField = CreateInputField(registerScreen, "Confirm Password", new Vector2(0, -60));
        confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
        
        // Create account button
        Button createAccountButton = CreateButton(registerScreen, "CREATE ACCOUNT", new Vector2(0, -120), Color.green);
        createAccountButton.onClick.AddListener(OnCreateAccountClicked);
        
        // Back button
        backButton = CreateButton(registerScreen, "BACK TO LOGIN", new Vector2(0, -180), Color.gray);
        backButton.onClick.AddListener(OnBackToLoginClicked);
    }
    
    void CreateCharacterSelectScreen()
    {
        characterSelectScreen = CreateScreen("Character Select Screen");
        
        // Title
        CreateText(characterSelectScreen, "SELECT CHARACTER", new Vector2(0, 180), 32, Color.yellow);
        
        // Character list placeholder
        CreateText(characterSelectScreen, "No characters found", new Vector2(0, 100), 18, Color.white);
        
        // Status text for character count
        statusText = CreateText(characterSelectScreen, "Press F2 to create Dev Wizard", new Vector2(0, 60), 16, Color.yellow);
        
        // Create new character button
        createCharacterButton = CreateButton(characterSelectScreen, "CREATE NEW CHARACTER", new Vector2(0, 0), Color.green);
        createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        
        // Back button
        Button backToMenuButton = CreateButton(characterSelectScreen, "BACK TO MENU", new Vector2(0, -80), Color.gray);
        backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
    }
    
    void CreateCharacterCreationScreen()
    {
        characterCreationScreen = CreateScreen("Character Creation Screen");
        
        // Title
        CreateText(characterCreationScreen, "CREATE CHARACTER", new Vector2(0, 180), 32, Color.yellow);
        
        // Character name input
        characterNameField = CreateInputField(characterCreationScreen, "Character Name", new Vector2(0, 120));
        
        // Character class dropdown
        characterClassDropdown = CreateDropdown(characterCreationScreen, new string[] { "Warrior", "Mage", "Archer", "Rogue" }, new Vector2(0, 60));
        
        // Generate character button (PixelLAB AI integration)
        generateCharacterButton = CreateButton(characterCreationScreen, "🎨 GENERATE WITH AI", new Vector2(0, 0), Color.magenta);
        generateCharacterButton.onClick.AddListener(OnGenerateCharacterClicked);
        
        // Character preview placeholder
        characterPreviewImage = CreateImage(characterCreationScreen, new Vector2(0, -80), new Vector2(200, 200));
        
        // Save character button
        saveCharacterButton = CreateButton(characterCreationScreen, "SAVE CHARACTER", new Vector2(0, -160), Color.green);
        saveCharacterButton.onClick.AddListener(OnSaveCharacterClicked);
        
        // Back button
        Button backToSelectButton = CreateButton(characterCreationScreen, "BACK TO SELECT", new Vector2(0, -220), Color.gray);
        backToSelectButton.onClick.AddListener(OnBackToSelectClicked);
    }
    
    void CreateSettingsScreen()
    {
        settingsScreen = CreateScreen("Settings Screen");
        
        // Title
        CreateText(settingsScreen, "SETTINGS", new Vector2(0, 180), 32, Color.yellow);
        
        // Music volume
        CreateText(settingsScreen, "Music Volume", new Vector2(-150, 120), 18, Color.white);
        musicVolumeSlider = CreateSlider(settingsScreen, new Vector2(150, 120));
        
        // SFX volume
        CreateText(settingsScreen, "SFX Volume", new Vector2(-150, 60), 18, Color.white);
        sfxVolumeSlider = CreateSlider(settingsScreen, new Vector2(150, 60));
        
        // Fullscreen toggle
        CreateText(settingsScreen, "Fullscreen", new Vector2(-150, 0), 18, Color.white);
        fullscreenToggle = CreateToggle(settingsScreen, new Vector2(150, 0));
        
        // Quality dropdown
        CreateText(settingsScreen, "Quality", new Vector2(-150, -60), 18, Color.white);
        qualityDropdown = CreateDropdown(settingsScreen, new string[] { "Low", "Medium", "High", "Ultra" }, new Vector2(150, -60));
        
        // Apply settings button
        applySettingsButton = CreateButton(settingsScreen, "APPLY SETTINGS", new Vector2(0, -120), Color.green);
        applySettingsButton.onClick.AddListener(OnApplySettingsClicked);
        
        // Back button
        Button backToMenuButton = CreateButton(settingsScreen, "BACK TO MENU", new Vector2(0, -180), Color.gray);
        backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
    }
    
    void CreateMainMenuScreen()
    {
        mainMenuScreen = CreateScreen("Main Menu Screen");
        
        // Title
        CreateText(mainMenuScreen, "BLAZED ODYSSEY", new Vector2(0, 180), 32, Color.yellow);
        
        // Character select button
        characterSelectButton = CreateButton(mainMenuScreen, "SELECT CHARACTER", new Vector2(0, 100), Color.green);
        characterSelectButton.onClick.AddListener(OnCharacterSelectClicked);
        
        // Settings button
        Button settingsMenuButton = CreateButton(mainMenuScreen, "SETTINGS", new Vector2(0, 20), Color.blue);
        settingsMenuButton.onClick.AddListener(OnSettingsClicked);
        
        // Logout button
        Button logoutButton = CreateButton(mainMenuScreen, "LOGOUT", new Vector2(0, -60), Color.red);
        logoutButton.onClick.AddListener(OnLogoutClicked);
    }
    
    GameObject CreateScreen(string name)
    {
        GameObject screen = new GameObject(name);
        screen.transform.SetParent(GameObject.Find("MMO UI Canvas").transform, false);
        
        CanvasGroup canvasGroup = screen.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        allScreens.Add(screen);
        return screen;
    }
    
    private void ShowScreen(GameObject screen)
    {
        // Hide all screens
        foreach (GameObject s in allScreens)
        {
            if (s != null)
            {
                s.SetActive(false);
                CanvasGroup group = s.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 0;
                    group.interactable = false;
                    group.blocksRaycasts = false;
                }
            }
        }
        
        // Show the target screen
        if (screen != null)
        {
            screen.SetActive(true);
            currentScreen = screen;
            Debug.Log($"🖥️ Showing screen: {screen.name}");
            
            // Make the screen visible with CanvasGroup
            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
            
            // Special handling for character select screen
            if (screen == characterSelectScreen)
            {
                DisplayGeneratedCharactersInSelect();
            }
        }
    }
    
    private void DisplayGeneratedCharactersInSelect()
    {
        Debug.Log("🎭 Character Select Screen shown - checking for generated characters...");
        
        PixelLABMCPIntegration pixelLAB = FindFirstObjectByType<PixelLABMCPIntegration>();
        if (pixelLAB != null)
        {
            pixelLAB.DisplayGeneratedCharacters();
            
            // Update the character count display
            if (statusText != null)
            {
                int characterCount = pixelLAB.GetCharacterCount();
                if (characterCount > 0)
                {
                    statusText.text = $"🎭 Found {characterCount} generated character(s)";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = "🎭 No characters found - Press F2 to create Dev Wizard";
                    statusText.color = Color.yellow;
                }
            }
        }
        else
        {
            Debug.LogError("❌ PixelLAB MCP Integration not found in character select!");
        }
    }
    
    // UI Creation Helper Methods
    TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
    {
        GameObject inputObj = new GameObject("Input Field");
        inputObj.transform.SetParent(parent.transform, false);
        
        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
        
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
        btnRect.sizeDelta = new Vector2(200, 40);
        
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
    
    TMP_Dropdown CreateDropdown(GameObject parent, string[] options, Vector2 position)
    {
        GameObject dropdownObj = new GameObject("Dropdown");
        dropdownObj.transform.SetParent(parent.transform, false);
        
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        Image bgImage = dropdownObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        
        RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
        dropdownRect.anchoredPosition = position;
        dropdownRect.sizeDelta = new Vector2(200, 40);
        
        // Simple dropdown setup - just add options
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        
        return dropdown;
    }
    
    Slider CreateSlider(GameObject parent, Vector2 position)
    {
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(parent.transform, false);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(200, 20);
        
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        
        return slider;
    }
    
    Toggle CreateToggle(GameObject parent, Vector2 position)
    {
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent.transform, false);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(20, 20);
        
        return toggle;
    }
    
    Image CreateImage(GameObject parent, Vector2 position, Vector2 size)
    {
        GameObject imageObj = new GameObject("Image");
        imageObj.transform.SetParent(parent.transform, false);
        
        Image image = imageObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        RectTransform imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchoredPosition = position;
        imageRect.sizeDelta = size;
        
        return image;
    }
    
    void CreateEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("✅ EventSystem created");
        }
    }
    
    // Event Handlers
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
            ShowScreen(mainMenuScreen);
        }
    }
    
    void OnRegisterClicked()
    {
        Debug.Log("👤 Register clicked!");
        ShowScreen(registerScreen);
    }
    
    void OnCreateAccountClicked()
    {
        Debug.Log("📝 Create account clicked!");
        // Add account creation logic here
        ShowScreen(loginScreen);
    }
    
    void OnBackToLoginClicked()
    {
        ShowScreen(loginScreen);
    }
    
    void OnCharacterSelectClicked()
    {
        ShowScreen(characterSelectScreen);
    }
    
    void OnCreateCharacterClicked()
    {
        ShowScreen(characterCreationScreen);
    }
    
    private void OnGenerateCharacterClicked()
    {
        Debug.Log("🎨 Generate Character with AI clicked!");
        
        // Get the PixelLAB MCP integration component
        PixelLABMCPIntegration pixelLAB = FindFirstObjectByType<PixelLABMCPIntegration>();
        
        if (pixelLAB != null)
        {
            // Find the character name input and class dropdown in the character creation screen
            TMP_InputField nameInput = characterCreationScreen.GetComponentInChildren<TMP_InputField>();
            TMP_Dropdown classDropdown = characterCreationScreen.GetComponentInChildren<TMP_Dropdown>();
            
            if (nameInput != null && classDropdown != null)
            {
                // Set the references in the PixelLAB component
                pixelLAB.characterNameInput = nameInput;
                pixelLAB.characterClassDropdown = classDropdown;
                
                // Find status text and character preview
                TextMeshProUGUI statusText = characterCreationScreen.GetComponentInChildren<TextMeshProUGUI>();
                Image characterPreview = characterCreationScreen.GetComponentInChildren<Image>();
                
                if (statusText != null) pixelLAB.statusText = statusText;
                if (characterPreview != null) pixelLAB.characterPreview = characterPreview;
                
                // Generate the character using PixelLAB MCP
                pixelLAB.GenerateCharacter();
                
                // Show status
                if (statusText != null)
                {
                    statusText.text = "🎨 Generating character with PixelLAB AI...";
                    statusText.color = Color.magenta;
                }
            }
            else
            {
                Debug.LogError("❌ Could not find character name input or class dropdown!");
            }
        }
        else
        {
            Debug.LogError("❌ PixelLAB MCP Integration not found! Please add PixelLABMCPIntegration script to a GameObject in the scene.");
            
            // Show error to user
            TextMeshProUGUI statusText = characterCreationScreen.GetComponentInChildren<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = "❌ PixelLAB MCP Integration not found!";
                statusText.color = Color.red;
            }
        }
    }
    
    void OnSaveCharacterClicked()
    {
        Debug.Log("💾 Saving character...");
        ShowScreen(characterSelectScreen);
    }
    
    void OnBackToSelectClicked()
    {
        ShowScreen(characterSelectScreen);
    }
    
    void OnBackToMenuClicked()
    {
        ShowScreen(mainMenuScreen);
    }
    
    void OnSettingsClicked()
    {
        ShowScreen(settingsScreen);
    }
    
    void OnApplySettingsClicked()
    {
        Debug.Log("⚙️ Applying settings...");
        // Add settings application logic here
    }
    
    void OnLogoutClicked()
    {
        ShowScreen(loginScreen);
    }
    
    void OnExitClicked()
    {
        Debug.Log("🚪 Exit clicked!");
        Application.Quit();
    }
    
    private void Update()
    {
        // Dev character creation with F2 key
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            CreateDevCharacter();
        }
    }
    
    private void CreateDevCharacter()
    {
        Debug.Log("🎭 Creating Dev Character with F2...");
        
        // Auto-login as dev character
        string devCharacterName = "Dev_Wizard";
        string devCharacterClass = "Wizard";
        
        // Generate the character using PixelLAB MCP
        PixelLABMCPIntegration pixelLAB = FindFirstObjectByType<PixelLABMCPIntegration>();
        if (pixelLAB != null)
        {
            // Safely set dev character data (only if UI elements exist)
            if (pixelLAB.characterNameInput != null)
            {
                pixelLAB.characterNameInput.text = devCharacterName;
            }
            
            if (pixelLAB.characterClassDropdown != null)
            {
                pixelLAB.characterClassDropdown.value = GetClassIndex(devCharacterClass);
            }
            
            // Generate the ghost wizard
            pixelLAB.GenerateDevWizard();
            
            // Auto-login
            StartCoroutine(AutoLoginAsDev(devCharacterName, devCharacterClass));
        }
        else
        {
            Debug.LogError("❌ PixelLAB MCP Integration not found!");
        }
    }
    
    private int GetClassIndex(string className)
    {
        string[] classes = {"Warrior", "Mage", "Archer", "Rogue", "Paladin", "Druid", "Monk", "Warlock"};
        for (int i = 0; i < classes.Length; i++)
        {
            if (classes[i] == className) return i;
        }
        return 0;
    }
    
    private IEnumerator AutoLoginAsDev(string characterName, string characterClass)
    {
        yield return new WaitForSeconds(3f); // Wait for character generation
        
        Debug.Log($"🎭 Auto-logging in as {characterName} the {characterClass}");
        
        // Show main menu
        ShowScreen(mainMenuScreen);
        
        // Update status
        if (statusText != null)
        {
            statusText.text = $"🎭 Logged in as {characterName} the {characterClass} (Dev Mode)";
            statusText.color = Color.cyan;
        }
    }
}
