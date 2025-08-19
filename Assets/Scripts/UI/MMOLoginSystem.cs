using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using System.Threading.Tasks;

public class MMOLoginSystem : MonoBehaviour
{
    [Header("Login UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button registerButton;
    public Button settingsButton;
    public Button exitButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI versionText;
    
    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public TMP_Dropdown resolutionDropdown;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider dialogVolumeSlider;
    public Toggle[] chatToggles;
    public Button applySettingsButton;
    public Button closeSettingsButton;
    
    [Header("Background Art")]
    public Image backgroundImage;
    public Image logoImage;
    
    private Canvas mainCanvas;
    private string savedUsername = "";
    private string savedEmail = "";
    private string savedPassword = "";
    private bool rememberCredentials = false;
    private bool useFallbackUI = false;
    private bool isRegistering = false;
    private AccountManager accountManager;
    private DatabaseManager databaseManager;
    
    // Pixel art MMO theme colors
    private Color pixelDarkBlue = new Color(0.1f, 0.1f, 0.3f, 1f);
    private Color pixelGold = new Color(1f, 0.8f, 0.2f, 1f);
    private Color pixelGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
    private Color pixelRed = new Color(0.8f, 0.2f, 0.2f, 1f);
    private Color pixelPurple = new Color(0.5f, 0.2f, 0.8f, 1f);
    
    void Start()
    {
        Debug.Log("🎮 MMOLoginSystem starting...");
        
        // Initialize managers
        InitializeManagers();
        
        // Check if TextMeshPro is available
        if (typeof(TMPro.TextMeshProUGUI) == null)
        {
            Debug.LogWarning("⚠️ TextMeshPro not found, using fallback UI system");
            useFallbackUI = true;
        }
        
        CreateCamera();
        LoadSavedCredentials();
        CreateLoginUI();
        CreateSettingsUI();
        ShowLoginPanel();
        HideSettingsPanel();
        
        Debug.Log("✅ MMOLoginSystem initialization complete!");
    }
    
    void InitializeManagers()
    {
        // Initialize AccountManager
        accountManager = AccountManager.Instance;
        accountManager.OnAccountLogin += OnAccountLoginSuccess;
        accountManager.OnAccountLogout += OnAccountLogout;
        accountManager.OnAccountError += OnAccountError;
        
        // Initialize DatabaseManager
        databaseManager = DatabaseManager.Instance;
        
        Debug.Log("🔐 Account and Database managers initialized");
    }
    
    void CreateCamera()
    {
        // Check for existing cameras and AudioListeners
        Camera[] existingCameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioListener[] existingAudioListeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        Debug.Log($"📷 Found {existingCameras.Length} existing cameras and {existingAudioListeners.Length} AudioListeners");
        
        // If we have multiple AudioListeners, disable all but the first one
        if (existingAudioListeners.Length > 1)
        {
            Debug.LogWarning("⚠️ Multiple AudioListeners detected! Disabling duplicates...");
            for (int i = 1; i < existingAudioListeners.Length; i++)
            {
                existingAudioListeners[i].enabled = false;
                Debug.Log($"🔇 Disabled AudioListener on: {existingAudioListeners[i].gameObject.name}");
            }
        }
        
        // Create a camera if none exists
        if (Camera.main == null)
        {
            Debug.Log("📷 Creating main camera...");
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = pixelDarkBlue;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.cullingMask = LayerMask.GetMask("Default", "UI");
            
            // Only add AudioListener if none exists
            if (existingAudioListeners.Length == 0)
            {
                cameraObj.AddComponent<AudioListener>();
                Debug.Log("🔊 Added AudioListener to new camera");
            }
            else
            {
                Debug.Log("🔊 AudioListener already exists, skipping...");
            }
            
            Debug.Log("✅ Main camera created successfully");
        }
        else
        {
            Debug.Log("📷 Main camera already exists, updating settings...");
            Camera mainCamera = Camera.main;
            
            // Update existing camera settings for 2D MMO
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.backgroundColor = pixelDarkBlue;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.cullingMask = LayerMask.GetMask("Default", "UI");
            
            // Ensure proper depth for UI rendering
            mainCamera.depth = 0;
            
            // Ensure the main camera has an AudioListener if none exists
            if (existingAudioListeners.Length == 0)
            {
                if (mainCamera.GetComponent<AudioListener>() == null)
                {
                    mainCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("🔊 Added AudioListener to existing main camera");
                }
            }
            
            Debug.Log("✅ Main camera settings updated successfully");
        }
        
        // Final verification
        AudioListener[] finalAudioListeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int enabledListeners = 0;
        foreach (var listener in finalAudioListeners)
        {
            if (listener.enabled) enabledListeners++;
        }
        
        Debug.Log($"✅ Scene now has {enabledListeners} enabled AudioListener(s)");
    }
    
    void LoadSavedCredentials()
    {
        savedUsername = PlayerPrefs.GetString("SavedUsername", "");
        savedEmail = PlayerPrefs.GetString("SavedEmail", "");
        savedPassword = PlayerPrefs.GetString("SavedPassword", "");
        rememberCredentials = PlayerPrefs.GetInt("RememberMe", 0) == 1;
        
        if (rememberCredentials && !string.IsNullOrEmpty(savedUsername))
        {
            Debug.Log("📧 Loaded saved credentials");
        }
    }
    
    void CreateLoginUI()
    {
        try
        {
            // Create main canvas
            GameObject canvasObj = new GameObject("MMO Login Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 1;
            
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster for input handling
            var raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            
            // Add EventSystem if none exists
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
                Debug.Log("✅ EventSystem created with InputSystemUIInputModule for new Input System");
            }
            
            // Create background with pixel art theme
            CreateBackground(canvasObj);
            
            // Create main login panel
            CreateLoginPanel(canvasObj);
            
            // Create logo
            CreateLogo(canvasObj);
            
            // Create version text
            CreateVersionText(canvasObj);
            
            Debug.Log("✅ Login UI created successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creating login UI: {e.Message}");
            CreateFallbackUI();
        }
    }
    
    void CreateFallbackUI()
    {
        Debug.Log("🔄 Creating fallback UI system...");
        
        // Create a simple canvas with basic UI
        GameObject canvasObj = new GameObject("Fallback MMO Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create simple background
        GameObject bgObj = new GameObject("Fallback Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = pixelDarkBlue;
        
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create simple text
        GameObject textObj = new GameObject("Fallback Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        var text = textObj.AddComponent<Text>();
        text.text = "MMO Login System\nTextMeshPro Required\nPlease install TextMeshPro package";
        text.fontSize = 24;
        text.color = pixelGold;
        text.alignment = TextAnchor.MiddleCenter;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 200);
        textRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("✅ Fallback UI created successfully");
    }
    
    void CreateBackground(GameObject canvas)
    {
        GameObject bgObj = new GameObject("Pixel Art Background");
        bgObj.transform.SetParent(canvas.transform, false);
        
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = pixelDarkBlue;
        
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Add pixel art pattern overlay
        GameObject patternObj = new GameObject("Pixel Pattern");
        patternObj.transform.SetParent(bgObj.transform, false);
        
        var patternImage = patternObj.AddComponent<Image>();
        patternImage.color = new Color(1f, 1f, 1f, 0.05f);
        
        var patternRect = patternObj.GetComponent<RectTransform>();
        patternRect.anchorMin = Vector2.zero;
        patternRect.anchorMax = Vector2.one;
        patternRect.offsetMin = Vector2.zero;
        patternRect.offsetMax = Vector2.zero;
    }
    
    void CreateLoginPanel(GameObject canvas)
    {
        GameObject panelObj = new GameObject("Login Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        // Panel background
        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 500);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Panel border
        CreatePanelBorder(panelObj);
        
        // Title
        CreateText(panelObj, "BLAZED ODYSSEY", new Vector2(0, 180), 32, pixelGold);
        
        // Username input (always visible)
        usernameInput = CreateInputField(panelObj, "Username", new Vector2(0, 120));
        if (rememberCredentials) usernameInput.text = savedUsername;
        
        // Email input (visible during registration)
        emailInput = CreateInputField(panelObj, "Email Address", new Vector2(0, 80));
        emailInput.gameObject.SetActive(false); // Hidden by default
        if (rememberCredentials) emailInput.text = savedEmail;
        
        // Password input
        passwordInput = CreateInputField(panelObj, "Password", new Vector2(0, 40));
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        if (rememberCredentials) passwordInput.text = savedPassword;
        
        // Confirm password input (visible during registration)
        confirmPasswordInput = CreateInputField(panelObj, "Confirm Password", new Vector2(0, 0));
        confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        confirmPasswordInput.gameObject.SetActive(false); // Hidden by default
        
        // Remember me toggle
        CreateRememberMeToggle(panelObj, new Vector2(-80, -60));
        
        // Login button
        loginButton = CreateButton(panelObj, "LOGIN", new Vector2(0, -100), pixelGreen);
        loginButton.onClick.AddListener(OnLoginClicked);
        
        // Register button
        registerButton = CreateButton(panelObj, "CREATE ACCOUNT", new Vector2(-100, -160), pixelPurple);
        registerButton.onClick.AddListener(OnRegisterClicked);
        
        // Settings button
        settingsButton = CreateButton(panelObj, "SETTINGS", new Vector2(100, -160), pixelPurple);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        
        // Exit button
        exitButton = CreateButton(panelObj, "EXIT", new Vector2(0, -220), pixelRed);
        exitButton.onClick.AddListener(OnExitClicked);
        
        // Status text
        statusText = CreateText(panelObj, "Enter your credentials to login or create a new account", new Vector2(0, -200), 16, Color.white);
    }
    
    void CreatePanelBorder(GameObject panel)
    {
        // Top border
        CreateBorderLine(panel, new Vector2(0, 240), new Vector2(380, 2));
        // Bottom border
        CreateBorderLine(panel, new Vector2(0, -240), new Vector2(380, 2));
        // Left border
        CreateBorderLine(panel, new Vector2(-190, 0), new Vector2(2, 480));
        // Right border
        CreateBorderLine(panel, new Vector2(190, 0), new Vector2(2, 480));
    }
    
    void CreateBorderLine(GameObject parent, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject("Border Line");
        line.transform.SetParent(parent.transform, false);
        
        var lineImage = line.AddComponent<Image>();
        lineImage.color = pixelGold;
        
        var lineRect = line.GetComponent<RectTransform>();
        lineRect.anchoredPosition = position;
        lineRect.sizeDelta = size;
    }
    
    TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
    {
        GameObject inputObj = new GameObject("Input Field");
        inputObj.transform.SetParent(parent.transform, false);
        
        var input = inputObj.AddComponent<TMP_InputField>();
        
        // Background
        var bgImage = inputObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchoredPosition = position;
        inputRect.sizeDelta = new Vector2(300, 40);
        
        // Text component for input
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        // Placeholder text component
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        
        var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        // Configure input field
        input.textComponent = text;
        input.placeholder = placeholderText;
        input.textViewport = textRect;
        
        // Enable input
        input.interactable = true;
        input.enabled = true;
        
        return input;
    }
    
    void CreateRememberMeToggle(GameObject parent, Vector2 position)
    {
        GameObject toggleObj = new GameObject("Remember Me Toggle");
        toggleObj.transform.SetParent(parent.transform, false);
        
        var toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = rememberCredentials;
        toggle.onValueChanged.AddListener(OnRememberMeChanged);
        
        var toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(20, 20);
        
        // Toggle background
        var bgImage = toggleObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleObj.transform, false);
        
        var checkImage = checkmark.AddComponent<Image>();
        checkImage.color = pixelGold;
        checkImage.sprite = CreateCheckmarkSprite();
        
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        toggle.graphic = checkImage;
        
        // Ensure toggle is interactive
        toggle.interactable = true;
        toggle.enabled = true;
        
        // Add transition
        var colors = toggle.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.3f, 1f);
        colors.selectedColor = new Color(0.2f, 0.2f, 0.4f, 1f);
        colors.fadeDuration = 0.1f;
        toggle.colors = colors;
        
        // Remember me text
        CreateText(parent, "Remember Me", new Vector2(20, position.y), 16, Color.white);
    }
    
    Button CreateButton(GameObject parent, string text, Vector2 position, Color color)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(parent.transform, false);
        
        var button = btnObj.AddComponent<Button>();
        
        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(150, 40); // Wider buttons for longer text
        
        // Button text
        var btnText = CreateText(btnObj, text, Vector2.zero, 18, Color.white);
        btnText.alignment = TextAlignmentOptions.Center;
        
        // Ensure button is interactive
        button.interactable = true;
        button.enabled = true;
        
        // Add navigation
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        
        // Add transition
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, color.a);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
        colors.selectedColor = color;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        
        return button;
    }
    
    TextMeshProUGUI CreateText(GameObject parent, string text, Vector2 position, int fontSize, Color color)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(300, 50);
        
        return textComponent;
    }
    
    void CreateLogo(GameObject canvas)
    {
        GameObject logoObj = new GameObject("MMO Logo");
        logoObj.transform.SetParent(canvas.transform, false);
        
        var logoImage = logoObj.AddComponent<Image>();
        logoImage.color = pixelGold;
        
        var logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.8f);
        logoRect.anchorMax = new Vector2(0.5f, 0.9f);
        logoRect.offsetMin = new Vector2(-100, -50);
        logoRect.offsetMax = new Vector2(100, 50);
        
        // Logo text
        var logoText = CreateText(logoObj, "MMO", Vector2.zero, 48, Color.black);
        logoText.alignment = TextAlignmentOptions.Center;
    }
    
    void CreateVersionText(GameObject canvas)
    {
        GameObject versionObj = new GameObject("Version Text");
        versionObj.transform.SetParent(canvas.transform, false);
        
        versionText = versionObj.AddComponent<TextMeshProUGUI>();
        versionText.text = "Version 1.0.0 Alpha";
        versionText.fontSize = 14;
        versionText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        versionText.alignment = TextAlignmentOptions.Center;
        
        var versionRect = versionObj.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(0.5f, 0.05f);
        versionRect.anchorMax = new Vector2(0.5f, 0.1f);
        versionRect.offsetMin = new Vector2(-100, -25);
        versionRect.offsetMax = new Vector2(100, 25);
    }
    
    void CreateSettingsUI()
    {
        // Settings panel will be created here
        // This is a placeholder for now
        Debug.Log("⚙️ Settings UI placeholder created");
    }
    
    void ShowLoginPanel()
    {
        // Login panel is always visible in this implementation
    }
    
    void HideSettingsPanel()
    {
        // Settings panel hiding logic
    }
    
    // Event handlers
    async void OnLoginClicked()
    {
        if (isRegistering)
        {
            await HandleRegistration();
        }
        else
        {
            await HandleLogin();
        }
    }
    
    async Task HandleLogin()
    {
        Debug.Log("🎮 Login attempt started!");
        
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            statusText.text = "Please enter both username and password";
            statusText.color = pixelRed;
            return;
        }
        
        statusText.text = "Connecting to server...";
        statusText.color = Color.yellow;
        
        var loginRequest = new MMOLoginRequest
        {
            username = usernameInput.text,
            password = passwordInput.text
        };
        
        bool success = await accountManager.Login(loginRequest);
        
        if (success)
        {
            // Save credentials if remember me is checked
            if (rememberCredentials)
            {
                PlayerPrefs.SetString("SavedUsername", usernameInput.text);
                PlayerPrefs.SetString("SavedPassword", passwordInput.text);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedUsername");
                PlayerPrefs.DeleteKey("SavedPassword");
                PlayerPrefs.SetInt("RememberMe", 0);
                PlayerPrefs.Save();
            }
        }
    }
    
    async Task HandleRegistration()
    {
        Debug.Log("👤 Account creation attempt started!");
        
        if (string.IsNullOrEmpty(usernameInput.text) || 
            string.IsNullOrEmpty(emailInput.text) || 
            string.IsNullOrEmpty(passwordInput.text) ||
            string.IsNullOrEmpty(confirmPasswordInput.text))
        {
            statusText.text = "Please fill in all fields";
            statusText.color = pixelRed;
            return;
        }
        
        if (passwordInput.text != confirmPasswordInput.text)
        {
            statusText.text = "Passwords do not match";
            statusText.color = pixelRed;
            return;
        }
        
        if (passwordInput.text.Length < 6)
        {
            statusText.text = "Password must be at least 6 characters";
            statusText.color = pixelRed;
            return;
        }
        
        statusText.text = "Creating account...";
        statusText.color = Color.yellow;
        
        var createRequest = new MMOCreateAccountRequest
        {
            username = usernameInput.text,
            email = emailInput.text,
            password = passwordInput.text
        };
        
        bool success = await accountManager.CreateAccount(createRequest);
        
        if (success)
        {
            statusText.text = "Account created! You can now login.";
            statusText.color = pixelGreen;
            
            // Switch back to login mode
            ToggleRegistrationMode(false);
        }
    }
    
    void OnRegisterClicked()
    {
        Debug.Log("👤 Register button clicked!");
        ToggleRegistrationMode(!isRegistering);
    }
    
    void ToggleRegistrationMode(bool registering)
    {
        isRegistering = registering;
        
        if (isRegistering)
        {
            // Show registration fields
            emailInput.gameObject.SetActive(true);
            confirmPasswordInput.gameObject.SetActive(true);
            
            // Update button text and positioning
            loginButton.GetComponentInChildren<TextMeshProUGUI>().text = "CREATE ACCOUNT";
            registerButton.GetComponentInChildren<TextMeshProUGUI>().text = "BACK TO LOGIN";
            
            statusText.text = "Fill in all fields to create your account";
            statusText.color = Color.white;
            
            // Update title
            var titleText = GameObject.Find("Login Panel").transform.Find("Text").GetComponent<TextMeshProUGUI>();
            if (titleText != null) titleText.text = "CREATE ACCOUNT";
        }
        else
        {
            // Hide registration fields
            emailInput.gameObject.SetActive(false);
            confirmPasswordInput.gameObject.SetActive(false);
            
            // Reset button text
            loginButton.GetComponentInChildren<TextMeshProUGUI>().text = "LOGIN";
            registerButton.GetComponentInChildren<TextMeshProUGUI>().text = "CREATE ACCOUNT";
            
            statusText.text = "Enter your credentials to login";
            statusText.color = Color.white;
            
            // Reset title
            var titleText = GameObject.Find("Login Panel").transform.Find("Text").GetComponent<TextMeshProUGUI>();
            if (titleText != null) titleText.text = "BLAZED ODYSSEY";
        }
    }
    
    void OnSettingsClicked()
    {
        Debug.Log("⚙️ Settings button clicked!");
        statusText.text = "Settings panel coming soon...";
        statusText.color = pixelPurple;
    }
    
    void OnExitClicked()
    {
        Debug.Log("🚪 Exit button clicked!");
        Application.Quit();
    }
    
    void OnRememberMeChanged(bool isOn)
    {
        rememberCredentials = isOn;
        Debug.Log($"📧 Remember me changed to: {isOn}");
    }
    
    void OnAccountLoginSuccess(MMOAccountData account)
    {
        statusText.text = $"Welcome {account.username}! Loading character selection...";
        statusText.color = pixelGreen;
        
        StartCoroutine(LoadCharacterSelection());
    }
    
    void OnAccountLogout()
    {
        statusText.text = "Logged out successfully";
        statusText.color = Color.white;
    }
    
    void OnAccountError(string error)
    {
        statusText.text = error;
        statusText.color = pixelRed;
        Debug.LogWarning($"❌ Account error: {error}");
    }
    
    IEnumerator LoadCharacterSelection()
    {
        yield return new WaitForSeconds(2f);
        
        // Load character selection scene
        SceneManager.LoadScene("CharacterSelection");
        Debug.Log("🎭 Loading character selection scene...");
    }
    
    // Helper method to create a simple checkmark sprite
    Sprite CreateCheckmarkSprite()
    {
        // This is a placeholder - in a real implementation you'd load an actual sprite
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[256];
        for (int i = 0; i < 256; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }
}
