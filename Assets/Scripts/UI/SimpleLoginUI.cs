using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class SimpleLoginUI : MonoBehaviour
{
    private AuthenticationManager authManager;
    private Canvas canvas;
    
    // UI Elements that will be created
    private GameObject loginPanel;
    private TMP_InputField emailInput;
    private TMP_InputField passwordInput;
    private Toggle rememberMeToggle;
    private Button loginButton;
    private Button registerButton;
    private Button exitButton;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI titleText;
    
    private bool isProcessing = false;

    void Awake()
    {
        Debug.Log("🎮 SimpleLoginUI starting...");
        
        // Find or create authentication manager
        authManager = FindFirstObjectByType<AuthenticationManager>();
        if (authManager == null)
        {
            GameObject authObj = new GameObject("AuthenticationManager");
            authManager = authObj.AddComponent<AuthenticationManager>();
        }
        
        // Get the canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        // Build the UI
        StartCoroutine(BuildUIAfterFrame());
        
        // Create animated background
        CreateAnimatedCityBackground();
    }
    
    IEnumerator BuildUIAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        CreateCompleteLoginUI();
        Debug.Log("🎨 SimpleLoginUI created successfully!");
    }
    
    void CreateCompleteLoginUI()
    {
        // Create main login panel
        loginPanel = new GameObject("Login Panel", typeof(RectTransform));
        loginPanel.transform.SetParent(canvas.transform, false);
        
        var panelImage = loginPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        
        var panelRect = loginPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(450, 600);
        
        // Create title
        CreateTitle();
        
        // Create form elements
        CreateFormElements();
        
        // Create status text
        CreateStatusText();
        
        // Setup button events
        SetupEvents();
        
        LoadRememberedCredentials();
    }
    
    void CreateTitle()
    {
        GameObject title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(loginPanel.transform, false);
        
        titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "BLAZED ODYSSEY";
        titleText.fontSize = 36;
        titleText.color = new Color(1f, 0.6f, 0.0f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 240);
        titleRect.sizeDelta = new Vector2(420, 50);
        
        // Create subtitle
        GameObject subtitle = new GameObject("Subtitle", typeof(RectTransform));
        subtitle.transform.SetParent(loginPanel.transform, false);
        
        var subtitleText = subtitle.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "MMO Alpha - Welcome, Adventurer!";
        subtitleText.fontSize = 16;
        subtitleText.color = new Color(0.8f, 0.8f, 0.9f, 1f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontStyle = FontStyles.Italic;
        
        var subtitleRect = subtitle.GetComponent<RectTransform>();
        subtitleRect.anchoredPosition = new Vector2(0, 200);
        subtitleRect.sizeDelta = new Vector2(420, 25);
    }
    
    void CreateFormElements()
    {
        float yPos = 140f;
        
        // Email input
        emailInput = CreateInputField("Email", "Enter your email...", new Vector2(0, yPos), TMP_InputField.ContentType.EmailAddress);
        yPos -= 80f;
        
        // Password input
        passwordInput = CreateInputField("Password", "Enter your password...", new Vector2(0, yPos), TMP_InputField.ContentType.Password);
        yPos -= 80f;
        
        // Remember Me toggle
        CreateRememberMeToggle(new Vector2(-30, yPos + 15));
        yPos -= 50f;
        
        // Buttons
        loginButton = CreateStyledButton("Login", new Vector2(-80, yPos), new Vector2(130, 40), new Color(0.2f, 0.6f, 1f, 1f));
        registerButton = CreateStyledButton("Register", new Vector2(80, yPos), new Vector2(130, 40), new Color(0.6f, 0.2f, 1f, 1f));
        yPos -= 60f;
        
        exitButton = CreateStyledButton("Exit Game", new Vector2(0, yPos), new Vector2(220, 35), new Color(0.8f, 0.3f, 0.3f, 1f));
    }
    
    TMP_InputField CreateInputField(string labelText, string placeholder, Vector2 position, TMP_InputField.ContentType contentType)
    {
        // Create container
        GameObject container = new GameObject(labelText + " Container", typeof(RectTransform));
        container.transform.SetParent(loginPanel.transform, false);
        
        var containerRect = container.GetComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(380, 65);
        
        // Create label
        GameObject label = new GameObject("Label", typeof(RectTransform));
        label.transform.SetParent(container.transform, false);
        
        var labelComponent = label.AddComponent<TextMeshProUGUI>();
        labelComponent.text = labelText + ":";
        labelComponent.fontSize = 14;
        labelComponent.color = new Color(0.9f, 0.9f, 1f, 1f);
        labelComponent.fontStyle = FontStyles.Bold;
        labelComponent.alignment = TextAlignmentOptions.Left;
        
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0, 20);
        labelRect.sizeDelta = new Vector2(300, 20);
        
        // Create input field
        GameObject inputObj = new GameObject("Input", typeof(RectTransform));
        inputObj.transform.SetParent(container.transform, false);
        
        var inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        
        var inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.contentType = contentType;
        
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchoredPosition = new Vector2(0, -10);
        inputRect.sizeDelta = new Vector2(300, 35);
        
        // Create text component for input
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(inputObj.transform, false);
        
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 16;
        text.color = Color.white;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        // Create placeholder
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(inputObj.transform, false);
        
        var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.6f, 1f);
        placeholderText.fontStyle = FontStyles.Italic;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    void CreateRememberMeToggle(Vector2 position)
    {
        GameObject toggleObj = new GameObject("Remember Me", typeof(RectTransform));
        toggleObj.transform.SetParent(loginPanel.transform, false);
        
        var toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(200, 25);
        
        rememberMeToggle = toggleObj.AddComponent<Toggle>();
        
        // Create background
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(toggleObj.transform, false);
        
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(-50, 0);
        bgRect.sizeDelta = new Vector2(18, 18);
        
        // Create checkmark
        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
        checkmark.transform.SetParent(background.transform, false);
        
        var checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        // Create label
        GameObject label = new GameObject("Label", typeof(RectTransform));
        label.transform.SetParent(toggleObj.transform, false);
        
        var labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "Remember Me";
        labelText.fontSize = 13;
        labelText.color = new Color(0.9f, 0.9f, 1f, 1f);
        labelText.alignment = TextAlignmentOptions.Left;
        
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(25, 0);
        labelRect.sizeDelta = new Vector2(120, 20);
        
        rememberMeToggle.targetGraphic = bgImage;
        rememberMeToggle.graphic = checkImage;
    }
    
    Button CreateStyledButton(string text, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObj = new GameObject(text + " Button", typeof(RectTransform));
        buttonObj.transform.SetParent(loginPanel.transform, false);
        
        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;
        
        var buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        var button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Create button text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    void CreateStatusText()
    {
        GameObject statusObj = new GameObject("Status Text", typeof(RectTransform));
        statusObj.transform.SetParent(loginPanel.transform, false);
        
        statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Press F1 for quick test login • F2 for dev login\nOr enter your credentials above";
        statusText.fontSize = 11;
        statusText.color = new Color(0.6f, 0.8f, 0.6f, 1f);
        statusText.alignment = TextAlignmentOptions.Center;
        
        var statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -240);
        statusRect.sizeDelta = new Vector2(420, 40);
    }
    
    void SetupEvents()
    {
        loginButton.onClick.AddListener(() => StartCoroutine(HandleLogin()));
        registerButton.onClick.AddListener(() => StartCoroutine(HandleRegister()));
        exitButton.onClick.AddListener(ExitGame);
        
        emailInput.onSubmit.AddListener((_) => StartCoroutine(HandleLogin()));
        passwordInput.onSubmit.AddListener((_) => StartCoroutine(HandleLogin()));
    }
    
    void Update()
    {
        if (!isProcessing && Keyboard.current != null)
        {
            // Quick login shortcuts
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                QuickLogin("test@blazed.com", "testpass123");
            }
            
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                QuickLogin("dev@blazed.com", "devpass123");
            }
            
            // Exit with Escape
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitGame();
            }
        }
    }
    
    void QuickLogin(string email, string password)
    {
        emailInput.text = email;
        passwordInput.text = password;
        StartCoroutine(HandleLogin());
    }
    
    IEnumerator HandleLogin()
    {
        if (isProcessing) yield break;
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter both email and password.", true);
            yield break;
        }
        
        isProcessing = true;
        ShowStatus("Authenticating...", false);
        
        yield return StartCoroutine(authManager.LoginUser(email, password));
        
        if (authManager.IsAuthenticated())
        {
            if (rememberMeToggle.isOn)
            {
                SaveRememberedCredentials(email);
            }
            
            ShowStatus("Login successful! Welcome back!", false);
            yield return new WaitForSeconds(1f);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLoginSuccess();
            }
        }
        else
        {
            ShowStatus("Login failed. Please check your credentials.", true);
        }
        
        isProcessing = false;
    }
    
    IEnumerator HandleRegister()
    {
        if (isProcessing) yield break;
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter both email and password.", true);
            yield break;
        }
        
        if (password.Length < 8)
        {
            ShowStatus("Password must be at least 8 characters long.", true);
            yield break;
        }
        
        isProcessing = true;
        ShowStatus("Creating account...", false);
        
        string username = email.Split('@')[0]; // Use email prefix as username
        yield return StartCoroutine(authManager.RegisterUser(email, username, password));
        
        ShowStatus("Account created! You can now log in.", false);
        isProcessing = false;
    }
    
    void ShowStatus(string message, bool isError)
    {
        if (statusText != null)
        {
            statusText.color = isError ? new Color(1f, 0.4f, 0.4f, 1f) : new Color(0.4f, 1f, 0.4f, 1f);
            statusText.text = message;
            Debug.Log($"💬 {(isError ? "Error" : "Info")}: {message}");
            
            // Auto-clear after 5 seconds
            StartCoroutine(ClearStatusAfterDelay(5f));
        }
    }
    
    IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (statusText != null)
        {
            statusText.text = "Press F1 for quick test login • F2 for dev login\nOr enter your credentials above";
            statusText.color = new Color(0.6f, 0.8f, 0.6f, 1f);
        }
    }
    
    void SaveRememberedCredentials(string email)
    {
        PlayerPrefs.SetString("RememberedEmail", email);
        PlayerPrefs.SetInt("RememberMe", 1);
        PlayerPrefs.Save();
    }
    
    void LoadRememberedCredentials()
    {
        if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            string rememberedEmail = PlayerPrefs.GetString("RememberedEmail", "");
            if (!string.IsNullOrEmpty(rememberedEmail))
            {
                emailInput.text = rememberedEmail;
                rememberMeToggle.isOn = true;
            }
        }
    }
    
    void ExitGame()
    {
        Debug.Log("🚪 Exiting Blazed Odyssey MMO Alpha...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    void CreateAnimatedCityBackground()
    {
        // Create background layer behind everything
        GameObject backgroundLayer = new GameObject("CityBackground", typeof(RectTransform));
        backgroundLayer.transform.SetParent(canvas.transform, false);
        backgroundLayer.transform.SetAsFirstSibling(); // Put it behind everything
        
        var backgroundRect = backgroundLayer.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        // Create gradient background
        var backgroundImage = backgroundLayer.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.2f, 0.35f, 1f); // Dark blue base
        
        // Create multiple city silhouette layers for depth
        CreateCityLayer(backgroundLayer, "FarCityLayer", -300f, 0.3f, new Color(0.1f, 0.15f, 0.25f, 0.8f), 150f);
        CreateCityLayer(backgroundLayer, "MidCityLayer", -200f, 0.5f, new Color(0.15f, 0.2f, 0.3f, 0.9f), 200f);
        CreateCityLayer(backgroundLayer, "NearCityLayer", -100f, 0.7f, new Color(0.2f, 0.25f, 0.35f, 1f), 250f);
        
        // Add some floating particles/stars
        CreateFloatingParticles(backgroundLayer);
        
        // Add subtle moving clouds
        CreateMovingClouds(backgroundLayer);
    }
    
    void CreateCityLayer(GameObject parent, string layerName, float yPosition, float alpha, Color baseColor, float height)
    {
        GameObject cityLayer = new GameObject(layerName, typeof(RectTransform));
        cityLayer.transform.SetParent(parent.transform, false);
        
        var layerRect = cityLayer.GetComponent<RectTransform>();
        layerRect.anchorMin = new Vector2(0, 0);
        layerRect.anchorMax = new Vector2(1, 0);
        layerRect.anchoredPosition = new Vector2(0, yPosition);
        layerRect.sizeDelta = new Vector2(0, height);
        
        // Create building silhouettes
        for (int i = 0; i < 12; i++)
        {
            CreateBuilding(cityLayer, i, baseColor);
        }
        
        // Add some animated windows
        CreateBuildingWindows(cityLayer, baseColor);
    }
    
    void CreateBuilding(GameObject parent, int index, Color baseColor)
    {
        GameObject building = new GameObject($"Building_{index}", typeof(RectTransform));
        building.transform.SetParent(parent.transform, false);
        
        var buildingRect = building.GetComponent<RectTransform>();
        float width = Random.Range(60f, 120f);
        float height = Random.Range(0.5f, 1.0f);
        float xPos = (index * 150f) - 800f + Random.Range(-20f, 20f);
        
        buildingRect.anchoredPosition = new Vector2(xPos, 0);
        buildingRect.sizeDelta = new Vector2(width, parent.GetComponent<RectTransform>().sizeDelta.y * height);
        buildingRect.anchorMin = new Vector2(0, 0);
        buildingRect.anchorMax = new Vector2(0, 0);
        
        var buildingImage = building.AddComponent<Image>();
        buildingImage.color = new Color(baseColor.r * 0.8f, baseColor.g * 0.8f, baseColor.b * 0.8f, baseColor.a);
    }
    
    void CreateBuildingWindows(GameObject parent, Color baseColor)
    {
        // Add some lit windows for atmosphere
        for (int i = 0; i < 15; i++)
        {
            GameObject window = new GameObject($"Window_{i}", typeof(RectTransform));
            window.transform.SetParent(parent.transform, false);
            
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.anchoredPosition = new Vector2(Random.Range(-800f, 800f), Random.Range(10f, 150f));
            windowRect.sizeDelta = new Vector2(Random.Range(3f, 8f), Random.Range(3f, 8f));
            
            var windowImage = window.AddComponent<Image>();
            windowImage.color = new Color(1f, 0.9f, 0.5f, Random.Range(0.3f, 0.8f)); // Warm light
            
            // Add subtle animation
            var animator = window.AddComponent<WindowAnimator>();
            animator.StartAnimation();
        }
    }
    
    void CreateFloatingParticles(GameObject parent)
    {
        // Create floating light particles/stars
        for (int i = 0; i < 20; i++)
        {
            GameObject particle = new GameObject($"Particle_{i}", typeof(RectTransform));
            particle.transform.SetParent(parent.transform, false);
            
            var particleRect = particle.GetComponent<RectTransform>();
            particleRect.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));
            particleRect.sizeDelta = new Vector2(Random.Range(1f, 3f), Random.Range(1f, 3f));
            
            var particleImage = particle.AddComponent<Image>();
            particleImage.color = new Color(1f, 1f, 0.8f, Random.Range(0.2f, 0.6f));
            
            // Add floating animation
            var floater = particle.AddComponent<ParticleFloater>();
            floater.StartFloating();
        }
    }
    
    void CreateMovingClouds(GameObject parent)
    {
        // Create subtle moving cloud shapes
        for (int i = 0; i < 5; i++)
        {
            GameObject cloud = new GameObject($"Cloud_{i}", typeof(RectTransform));
            cloud.transform.SetParent(parent.transform, false);
            
            var cloudRect = cloud.GetComponent<RectTransform>();
            cloudRect.anchoredPosition = new Vector2(Random.Range(-1200f, 1200f), Random.Range(200f, 400f));
            cloudRect.sizeDelta = new Vector2(Random.Range(150f, 300f), Random.Range(50f, 100f));
            
            var cloudImage = cloud.AddComponent<Image>();
            cloudImage.color = new Color(0.3f, 0.35f, 0.5f, Random.Range(0.1f, 0.3f));
            
            // Add slow drifting animation
            var cloudMover = cloud.AddComponent<CloudMover>();
            cloudMover.StartDrifting();
        }
    }
}
