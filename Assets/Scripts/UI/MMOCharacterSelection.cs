using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MMOCharacterSelection : MonoBehaviour
{
    [Header("Character Selection UI")]
    public GameObject characterListPanel;
    public GameObject characterPreviewPanel;
    public GameObject characterStatsPanel;
    public Button createCharacterButton;
    public Button enterWorldButton;
    public Button deleteCharacterButton;
    public Button logoutButton;
    
    [Header("Login Fields")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("Character List")]
    public Transform characterListContainer;
    public GameObject characterSlotPrefab;
    public List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    
    [Header("Character Preview")]
    public Image characterPreviewImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI characterClassText;
    public TextMeshProUGUI characterRaceText;
    
    [Header("Character Stats")]
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI equipmentText;
    public TextMeshProUGUI descriptionText;
    
    private Canvas mainCanvas;
    private int selectedCharacterIndex = -1;
    private List<CharacterSelectionData> characters = new List<CharacterSelectionData>();
    
    // Pixel art MMO theme colors (matching login system)
    private Color pixelDarkBlue = new Color(0.1f, 0.1f, 0.3f, 1f);
    private Color pixelGold = new Color(1f, 0.8f, 0.2f, 1f);
    private Color pixelGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
    private Color pixelRed = new Color(0.8f, 0.2f, 0.2f, 1f);
    private Color pixelPurple = new Color(0.5f, 0.2f, 0.8f, 1f);
    private Color pixelBrown = new Color(0.4f, 0.2f, 0.1f, 1f);
    
    void Start()
    {
        Debug.Log("🎭 MMOCharacterSelection starting...");
        
        // IMMEDIATE cleanup of ALL audio listeners and event systems
        ImmediateCleanup();
        
        // Wait a frame to ensure cleanup is complete
        StartCoroutine(StartAfterCleanup());
    }
    
    void ImmediateCleanup()
    {
        Debug.Log("🚨 IMMEDIATE CLEANUP STARTING...");
        
        // Find and destroy ALL EventSystems
        var allEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        Debug.Log($"🗑️ Found {allEventSystems.Length} EventSystems - DESTROYING ALL");
        foreach (var es in allEventSystems)
        {
            Debug.Log($"🗑️ Destroying EventSystem: {es.name}");
            DestroyImmediate(es.gameObject);
        }
        
        // Find and destroy ALL AudioListeners
        var allAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🔇 Found {allAudioListeners.Length} AudioListeners - DESTROYING ALL");
        foreach (var al in allAudioListeners)
        {
            Debug.Log($"🔇 Destroying AudioListener: {al.name}");
            DestroyImmediate(al.gameObject);
        }
        
        // Force garbage collection to ensure cleanup
        System.GC.Collect();
        
        Debug.Log("✅ IMMEDIATE CLEANUP COMPLETE");
    }
    
    IEnumerator StartAfterCleanup()
    {
        // Wait for end of frame to ensure all objects are destroyed
        yield return new WaitForEndOfFrame();
        
        // Verify cleanup
        var remainingEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        var remainingAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🎯 After cleanup - EventSystems: {remainingEventSystems.Length}, AudioListeners: {remainingAudioListeners.Length}");
        
        // Now create our single EventSystem with AudioListener
        CreateSingleEventSystem();
        
        // Wait another frame to ensure EventSystem is ready
        yield return new WaitForEndOfFrame();
        
        // Start UI creation
        StartCoroutine(InitializeUI());
    }
    
    void CreateSingleEventSystem()
    {
        Debug.Log("🎯 Creating SINGLE EventSystem with AudioListener...");
        
        // Create EventSystem GameObject
        GameObject eventSystem = new GameObject("EventSystem");
        
        // Add EventSystem component
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        
        // Add StandaloneInputModule
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Add EXACTLY ONE AudioListener
        eventSystem.AddComponent<AudioListener>();
        
        // Mark as DontDestroyOnLoad to prevent accidental destruction
        DontDestroyOnLoad(eventSystem);
        
        // Add a script to monitor and prevent new AudioListeners
        eventSystem.AddComponent<AudioListenerGuard>();
        
        Debug.Log("✅ Single EventSystem with AudioListener created and marked persistent");
        
        // Verify final state
        var finalEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        var finalAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🎯 Final state - EventSystems: {finalEventSystems.Length}, AudioListeners: {finalAudioListeners.Length}");
    }
    
    IEnumerator InitializeUI()
    {
        // Wait for end of frame to ensure EventSystem is ready
        yield return new WaitForEndOfFrame();
        
        Debug.Log("🎯 EventSystem ready, creating UI...");
        
        CreateCharacterSelectionUI();
        LoadSampleCharacters();
        CreateCharacterSlots();
        SelectCharacter(0); // Select first character by default
        
        // Load saved credentials if remember me was enabled
        LoadSavedCredentials();
        
        Debug.Log("🎯 UI interaction system ready!");
        
        // Test UI interaction after setup
        yield return new WaitForSeconds(0.5f);
        TestUIInteraction();
        
        // Force focus on email input field
        if (emailInputField != null)
        {
            emailInputField.Select();
            Debug.Log("🎯 Forced focus on email input field");
        }
    }
    
    void LoadSavedCredentials()
    {
        if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail", "");
            if (!string.IsNullOrEmpty(savedEmail))
            {
                emailInputField.text = savedEmail;
                rememberMeToggle.isOn = true;
                Debug.Log("🔐 Loaded saved email from PlayerPrefs");
            }
        }
    }
    
    void EnsureEventSystem()
    {
        Debug.Log("🎯 Starting EventSystem cleanup...");
        
        // Remove any existing EventSystems to avoid conflicts
        var existingEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        Debug.Log($"🗑️ Found {existingEventSystems.Length} existing EventSystems");
        foreach (var existing in existingEventSystems)
        {
            Debug.Log("🗑️ Removing EventSystem: " + existing.name);
            DestroyImmediate(existing.gameObject);
        }
        
        // Remove any existing AudioListeners to avoid spam
        var existingAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🔇 Found {existingAudioListeners.Length} existing AudioListeners");
        foreach (var existing in existingAudioListeners)
        {
            Debug.Log("🔇 Removing AudioListener: " + existing.name);
            DestroyImmediate(existing.gameObject);
        }
        
        // Wait a frame to ensure cleanup is complete
        StartCoroutine(CreateEventSystemAfterCleanup());
    }
    
    IEnumerator CreateEventSystemAfterCleanup()
    {
        // Wait for end of frame to ensure all objects are destroyed
        yield return new WaitForEndOfFrame();
        
        // Double-check that everything is cleaned up
        var remainingEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        var remainingAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        Debug.Log($"🎯 After cleanup - EventSystems: {remainingEventSystems.Length}, AudioListeners: {remainingAudioListeners.Length}");
        
        // Create fresh EventSystem
        Debug.Log("🎯 Creating fresh EventSystem for UI interactions...");
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Add single AudioListener to EventSystem
        eventSystem.AddComponent<AudioListener>();
        
        Debug.Log("✅ EventSystem and AudioListener created successfully");
        
        // Verify final state
        var finalEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        var finalAudioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🎯 Final state - EventSystems: {finalEventSystems.Length}, AudioListeners: {finalAudioListeners.Length}");
    }
    
    void CreateCharacterSelectionUI()
    {
        // Create main canvas
        GameObject canvasObj = new GameObject("Character Selection Canvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 1;
        
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster for UI interactions
        var raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        
        // Ensure the canvas is properly set up for events
        canvasObj.layer = LayerMask.NameToLayer("UI");
        
        // Force canvas to be active and enabled
        canvasObj.SetActive(true);
        mainCanvas.enabled = true;
        raycaster.enabled = true;
        
        Debug.Log("🎯 Setting up UI interaction system...");
        
        // Create background with pixel art theme
        CreateBackground(canvasObj);
        
        // Create title
        CreateTitle(canvasObj);
        
        // Create main panels
        CreateCharacterListPanel(canvasObj);
        CreateCharacterPreviewPanel(canvasObj);
        CreateCharacterStatsPanel(canvasObj);
        
        // Create login panel (for testing/development)
        CreateLoginPanel(canvasObj);
        
        // Create action buttons
        CreateActionButtons(canvasObj);
        
        Debug.Log("✅ Character Selection UI created successfully");
        
        // Verify UI setup
        Debug.Log($"🎯 Canvas active: {mainCanvas.gameObject.activeInHierarchy}");
        Debug.Log($"🎯 Canvas enabled: {mainCanvas.enabled}");
        Debug.Log($"🎯 GraphicRaycaster enabled: {mainCanvas.GetComponent<GraphicRaycaster>().enabled}");
        Debug.Log($"🎯 EventSystem exists: {FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null}");
        Debug.Log($"🎯 AudioListener count: {FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length}");
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
        patternImage.color = new Color(1f, 1f, 1f, 0.03f);
        
        var patternRect = patternObj.GetComponent<RectTransform>();
        patternRect.anchorMin = Vector2.zero;
        patternRect.anchorMax = Vector2.one;
        patternRect.offsetMin = Vector2.zero;
        patternRect.offsetMax = Vector2.zero;
    }
    
    void CreateTitle(GameObject canvas)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvas.transform, false);
        
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "CHARACTER SELECTION";
        titleText.fontSize = 48;
        titleText.color = pixelGold;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.offsetMin = new Vector2(-300, -50);
        titleRect.offsetMax = new Vector2(300, 50);
    }
    
    void CreateCharacterListPanel(GameObject canvas)
    {
        GameObject panelObj = new GameObject("Character List Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        // Panel background
        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.1f);
        panelRect.anchorMax = new Vector2(0.35f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Panel border
        CreatePanelBorder(panelObj, pixelGold);
        
        // Panel title
        CreateText(panelObj, "CHARACTERS", new Vector2(0, 300), 24, pixelGold);
        
        // Character list container
        GameObject containerObj = new GameObject("Character List Container");
        containerObj.transform.SetParent(panelObj.transform, false);
        
        var containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.15f);
        containerRect.anchorMax = new Vector2(0.9f, 0.85f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Add scroll view for character list
        var scrollView = containerObj.AddComponent<ScrollRect>();
        var viewport = CreateViewport(containerObj);
        var content = CreateContent(viewport);
        
        scrollView.viewport = viewport.GetComponent<RectTransform>();
        scrollView.content = content.GetComponent<RectTransform>();
        scrollView.horizontal = false;
        scrollView.vertical = true;
        
        characterListContainer = content.transform;
        
        characterListPanel = panelObj;
    }
    
    GameObject CreateViewport(GameObject parent)
    {
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(parent.transform, false);
        
        var viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        
        var viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        var mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        return viewportObj;
    }
    
    GameObject CreateContent(GameObject viewport)
    {
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewport.transform, false);
        
        var contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        
        return contentObj;
    }
    
    void CreateCharacterPreviewPanel(GameObject canvas)
    {
        GameObject panelObj = new GameObject("Character Preview Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        // Panel background
        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.4f, 0.1f);
        panelRect.anchorMax = new Vector2(0.7f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Panel border
        CreatePanelBorder(panelObj, pixelGold);
        
        // Panel title
        CreateText(panelObj, "CHARACTER PREVIEW", new Vector2(0, 300), 24, pixelGold);
        
        // Character preview image (placeholder)
        CreateCharacterPreviewImage(panelObj);
        
        // Character info
        characterNameText = CreateText(panelObj, "Character Name", new Vector2(0, 200), 20, pixelGold);
        characterLevelText = CreateText(panelObj, "Level: 1", new Vector2(0, 150), 18, Color.white);
        characterClassText = CreateText(panelObj, "Class: Warrior", new Vector2(0, 100), 18, Color.white);
        characterRaceText = CreateText(panelObj, "Race: Human", new Vector2(0, 50), 18, Color.white);
        
        characterPreviewPanel = panelObj;
    }
    
    void CreateCharacterPreviewImage(GameObject parent)
    {
        GameObject imageObj = new GameObject("Character Preview Image");
        imageObj.transform.SetParent(parent.transform, false);
        
        characterPreviewImage = imageObj.AddComponent<Image>();
        characterPreviewImage.color = new Color(0.2f, 0.2f, 0.4f, 1f);
        
        var imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.sizeDelta = new Vector2(200, 200);
        imageRect.anchoredPosition = new Vector2(0, 0);
        
        // Add placeholder text
        var placeholderText = CreateText(imageObj, "CHARACTER\nPREVIEW", Vector2.zero, 16, Color.white);
        placeholderText.alignment = TextAlignmentOptions.Center;
    }
    
    void CreateCharacterStatsPanel(GameObject canvas)
    {
        GameObject panelObj = new GameObject("Character Stats Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        // Panel background
        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.75f, 0.1f);
        panelRect.anchorMax = new Vector2(0.95f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Panel border
        CreatePanelBorder(panelObj, pixelGold);
        
        // Panel title
        CreateText(panelObj, "CHARACTER STATS", new Vector2(0, 300), 24, pixelGold);
        
        // Stats text
        statsText = CreateText(panelObj, "Strength: 10\nDexterity: 8\nIntelligence: 6\nVitality: 12", new Vector2(0, 200), 16, Color.white);
        statsText.alignment = TextAlignmentOptions.Left;
        
        // Equipment text
        equipmentText = CreateText(panelObj, "EQUIPMENT:\nHead: None\nChest: Cloth Shirt\nHands: None\nFeet: Leather Boots", new Vector2(0, 50), 16, Color.white);
        equipmentText.alignment = TextAlignmentOptions.Left;
        
        // Description text
        descriptionText = CreateText(panelObj, "A brave warrior from the lands of Blazed Odyssey, ready to embark on epic adventures!", new Vector2(0, -100), 14, Color.cyan);
        descriptionText.alignment = TextAlignmentOptions.Center;
        
        characterStatsPanel = panelObj;
    }
    
    void CreateLoginPanel(GameObject canvas)
    {
        Debug.Log("🔐 Creating login panel...");
        
        // Create login panel background
        GameObject loginPanel = new GameObject("Login Panel");
        loginPanel.transform.SetParent(canvas.transform, false);
        
        var panelImage = loginPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        
        var panelRect = loginPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.6f);
        panelRect.anchorMax = new Vector2(0.7f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add border
        CreatePanelBorder(loginPanel, pixelGold);
        
        // Panel title
        CreateText(loginPanel, "LOGIN", new Vector2(0, 120), 32, pixelGold);
        
        // Email input field
        emailInputField = CreateInputField(loginPanel, "Email", new Vector2(0, 60), 200);
        
        // Password input field
        passwordInputField = CreateInputField(loginPanel, "Password", new Vector2(0, 0), 200);
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        
        // Remember me toggle
        rememberMeToggle = CreateToggle(loginPanel, "Remember Me", new Vector2(-80, -60));
        
        // Login button
        loginButton = CreateButton(loginPanel, "LOGIN", new Vector2(0, -120), pixelGreen);
        loginButton.onClick.AddListener(OnLoginClicked);
        
        // Settings button
        settingsButton = CreateButton(loginPanel, "SETTINGS", new Vector2(-100, -180), pixelPurple);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        
        // Exit button
        exitButton = CreateButton(loginPanel, "EXIT", new Vector2(100, -180), pixelRed);
        exitButton.onClick.AddListener(OnExitClicked);
        
        Debug.Log("✅ Login panel created successfully!");
    }
    
    TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position, float width)
    {
        GameObject inputObj = new GameObject("Input Field");
        inputObj.transform.SetParent(parent.transform, false);
        
        // Add RectTransform
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(width, 40);
        inputRect.anchoredPosition = position;
        
        // Add background image
        var bgImage = inputObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        
        // Add InputField component
        var inputField = inputObj.AddComponent<TMP_InputField>();
        
        // Create text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        var textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);
        
        // Create placeholder text
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.fontSize = 16;
        placeholderText.alignment = TextAlignmentOptions.Left;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        // Create input text
        GameObject inputTextObj = new GameObject("Input Text");
        inputTextObj.transform.SetParent(textArea.transform, false);
        var inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.color = Color.white;
        inputText.fontSize = 16;
        inputText.alignment = TextAlignmentOptions.Left;
        
        var inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;
        
        // Set up input field references
        inputField.textViewport = textArea.GetComponent<RectTransform>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        // Add visual feedback for input field states
        var colors = inputField.colors;
        colors.normalColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        colors.pressedColor = new Color(0.25f, 0.25f, 0.35f, 1f);
        inputField.colors = colors;
        
        // Add input field events for better UX
        inputField.onSelect.AddListener((string value) => {
            Debug.Log($"🎯 Input field selected: {placeholder}");
            bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        });
        
        inputField.onDeselect.AddListener((string value) => {
            Debug.Log($"🎯 Input field deselected: {placeholder}");
            bgImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        });
        
        // Ensure input field is on UI layer
        inputObj.layer = LayerMask.NameToLayer("UI");
        
        Debug.Log($"🎯 Created interactive input field: {placeholder}");
        
        return inputField;
    }
    
    Toggle CreateToggle(GameObject parent, string label, Vector2 position)
    {
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent.transform, false);
        
        // Add RectTransform
        var toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRect.sizeDelta = new Vector2(200, 30);
        toggleRect.anchoredPosition = position;
        
        // Add Toggle component
        var toggle = toggleObj.AddComponent<Toggle>();
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        var checkImage = checkmark.AddComponent<Image>();
        checkImage.color = pixelGreen;
        
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        // Create label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.color = Color.white;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Left;
        
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(35, 0);
        labelRect.offsetMax = Vector2.zero;
        
        // Set up toggle references
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        
        // Add toggle event for visual feedback
        toggle.onValueChanged.AddListener((bool isOn) => {
            Debug.Log($"🎯 Toggle '{label}' changed to: {isOn}");
            if (isOn)
            {
                checkImage.color = pixelGreen;
                bgImage.color = new Color(0.15f, 0.15f, 0.25f, 1f);
            }
            else
            {
                checkImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                bgImage.color = new Color(0.1f, 0.1f, 0.2f, 1f);
            }
        });
        
        // Ensure toggle is on UI layer
        toggleObj.layer = LayerMask.NameToLayer("UI");
        
        Debug.Log($"🎯 Created interactive toggle: {label}");
        
        return toggle;
    }
    
    void CreateActionButtons(GameObject canvas)
    {
        Debug.Log("🎯 Creating action buttons...");
        
        // Create Character Button
        createCharacterButton = CreateButton(canvas, "CREATE CHARACTER", new Vector2(-400, -200), pixelGreen);
        createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        Debug.Log("✅ CREATE CHARACTER button created and listener added");
        
        // Enter World Button
        enterWorldButton = CreateButton(canvas, "ENTER WORLD", new Vector2(-200, -200), pixelGold);
        enterWorldButton.onClick.AddListener(OnEnterWorldClicked);
        Debug.Log("✅ ENTER WORLD button created and listener added");
        
        // Delete Character Button
        deleteCharacterButton = CreateButton(canvas, "DELETE CHARACTER", new Vector2(0, -200), pixelRed);
        deleteCharacterButton.onClick.AddListener(OnDeleteCharacterClicked);
        Debug.Log("✅ DELETE CHARACTER button created and listener added");
        
        // Logout Button
        logoutButton = CreateButton(canvas, "LOGOUT", new Vector2(200, -200), pixelPurple);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        Debug.Log("✅ LOGOUT button created and listener added");
        
        Debug.Log("🎯 All action buttons created successfully!");
    }
    
    void CreatePanelBorder(GameObject panel, Color borderColor)
    {
        // Top border
        CreateBorderLine(panel, new Vector2(0, 320), new Vector2(380, 2), borderColor);
        // Bottom border
        CreateBorderLine(panel, new Vector2(0, -320), new Vector2(380, 2), borderColor);
        // Left border
        CreateBorderLine(panel, new Vector2(-190, 0), new Vector2(2, 640), borderColor);
        // Right border
        CreateBorderLine(panel, new Vector2(190, 0), new Vector2(2, 640), borderColor);
    }
    
    void CreateBorderLine(GameObject parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject line = new GameObject("Border Line");
        line.transform.SetParent(parent.transform, false);
        
        var lineImage = line.AddComponent<Image>();
        lineImage.color = color;
        
        var lineRect = line.GetComponent<RectTransform>();
        lineRect.anchoredPosition = position;
        lineRect.sizeDelta = size;
    }
    
    Button CreateButton(GameObject parent, string text, Vector2 position, Color color)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(parent.transform, false);
        
        // Add RectTransform first
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(150, 40);
        btnRect.anchoredPosition = position;
        
        // Add Image component for visual
        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        // Add Button component
        var button = btnObj.AddComponent<Button>();
        
        // Set up button navigation
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        
        // Set up button colors
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, color.a);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
        button.colors = colors;
        
        // Button text
        var btnText = CreateText(btnObj, text, Vector2.zero, 16, Color.white);
        btnText.alignment = TextAlignmentOptions.Center;
        
        // Ensure button is on UI layer
        btnObj.layer = LayerMask.NameToLayer("UI");
        
        Debug.Log($"🎯 Created interactive button: {text}");
        
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
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(300, 50);
        textRect.anchoredPosition = position;
        
        return textComponent;
    }
    
    void LoadSampleCharacters()
    {
        // Create sample characters for testing using simple data structure
        characters.Add(new CharacterSelectionData("Thunderfist", 15, "Warrior", "Human", 15, 12, 8, 18));
        characters.Add(new CharacterSelectionData("Shadowweave", 12, "Mage", "Elf", 6, 10, 18, 10));
        characters.Add(new CharacterSelectionData("Swiftarrow", 18, "Archer", "Human", 10, 18, 8, 12));
        characters.Add(new CharacterSelectionData("Ironheart", 22, "Paladin", "Human", 16, 8, 12, 16));
        
        Debug.Log($"✅ Loaded {characters.Count} sample characters");
    }
    
    void CreateCharacterSlots()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CreateCharacterSlot(i);
        }
    }
    
    void CreateCharacterSlot(int index)
    {
        GameObject slotObj = new GameObject($"Character Slot {index}");
        slotObj.transform.SetParent(characterListContainer, false);
        
        // Set up RectTransform first
        var slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(0, 80);
        
        // Add Image component for visual
        var slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        
        // Character info
        var character = characters[index];
        var nameText = CreateText(slotObj, character.name, new Vector2(-100, 20), 18, pixelGold);
        nameText.alignment = TextAlignmentOptions.Left;
        
        var levelText = CreateText(slotObj, $"Level {character.level}", new Vector2(-100, -10), 14, Color.white);
        levelText.alignment = TextAlignmentOptions.Left;
        
        var classText = CreateText(slotObj, character.characterClass, new Vector2(100, 20), 16, Color.white);
        classText.alignment = TextAlignmentOptions.Right;
        
        var raceText = CreateText(slotObj, character.race, new Vector2(100, -10), 14, Color.cyan);
        raceText.alignment = TextAlignmentOptions.Right;
        
        // Add Button component for selection
        var button = slotObj.AddComponent<Button>();
        
        // Set up button colors for better interaction feedback
        var colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f, 0.9f);
        colors.pressedColor = new Color(0.3f, 0.3f, 0.5f, 0.9f);
        button.colors = colors;
        
        // Set up button navigation
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        
        // Add click listener
        int slotIndex = index; // Capture index for lambda
        button.onClick.AddListener(() => SelectCharacter(slotIndex));
        
        // Ensure slot is on UI layer
        slotObj.layer = LayerMask.NameToLayer("UI");
        
        // Create CharacterSlot component
        var characterSlot = slotObj.AddComponent<CharacterSlot>();
        characterSlot.Initialize(index, character, button);
        characterSlots.Add(characterSlot);
        
        Debug.Log($"🎯 Created interactive character slot: {character.name}");
    }
    
    void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        selectedCharacterIndex = index;
        var character = characters[index];
        
        // Update preview
        characterNameText.text = character.name;
        characterLevelText.text = $"Level: {character.level}";
        characterClassText.text = $"Class: {character.characterClass}";
        characterRaceText.text = $"Race: {character.race}";
        
        // Update stats
        statsText.text = $"Strength: {character.strength}\nDexterity: {character.dexterity}\nIntelligence: {character.intelligence}\nVitality: {character.vitality}";
        
        // Update equipment based on class
        UpdateEquipmentDisplay(character);
        
        // Update description
        descriptionText.text = GetCharacterDescription(character);
        
        // Update slot selection visuals
        UpdateSlotSelection();
        
        Debug.Log($"🎭 Selected character: {character.name}");
    }
    
    void UpdateEquipmentDisplay(CharacterSelectionData character)
    {
        string equipment = "EQUIPMENT:\n";
        
        switch (character.characterClass.ToLower())
        {
            case "warrior":
                equipment += "Head: Iron Helmet\nChest: Steel Breastplate\nHands: Gauntlets\nFeet: Steel Boots";
                break;
            case "mage":
                equipment += "Head: Wizard Hat\nChest: Robe of the Archmage\nHands: Silk Gloves\nFeet: Enchanted Slippers";
                break;
            case "archer":
                equipment += "Head: Leather Cap\nChest: Studded Leather\nHands: Archer's Gloves\nFeet: Leather Boots";
                break;
            case "paladin":
                equipment += "Head: Holy Crown\nChest: Divine Armor\nHands: Blessed Gauntlets\nFeet: Sacred Greaves";
                break;
            default:
                equipment += "Head: None\nChest: Cloth Shirt\nHands: None\nFeet: Leather Boots";
                break;
        }
        
        equipmentText.text = equipment;
    }
    
    string GetCharacterDescription(CharacterSelectionData character)
    {
        switch (character.characterClass.ToLower())
        {
            case "warrior":
                return "A mighty warrior with unbreakable spirit, ready to charge into battle and protect allies with steel and courage.";
            case "mage":
                return "A wise spellcaster who wields the arcane arts, capable of devastating magical attacks and protective enchantments.";
            case "archer":
                return "A skilled hunter with deadly precision, using speed and agility to strike from afar with lethal accuracy.";
            case "paladin":
                return "A holy warrior blessed by divine power, combining martial prowess with sacred magic to smite evil.";
            default:
                return "A brave adventurer from the lands of Blazed Odyssey, ready to embark on epic quests and legendary battles.";
        }
    }
    
    void UpdateSlotSelection()
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            var slot = characterSlots[i];
            var slotImage = slot.GetComponent<Image>();
            
            if (i == selectedCharacterIndex)
            {
                slotImage.color = new Color(0.3f, 0.3f, 0.5f, 0.9f); // Selected color
            }
            else
            {
                slotImage.color = new Color(0.15f, 0.15f, 0.25f, 0.9f); // Normal color
            }
        }
    }
    
    // Event handlers
    void OnLoginClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        bool rememberMe = rememberMeToggle.isOn;
        
        Debug.Log($"🔐 Login attempt - Email: {email}, Remember Me: {rememberMe}");
        
        // TODO: Implement actual login logic
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            // Save credentials if remember me is checked
            if (rememberMe)
            {
                PlayerPrefs.SetString("SavedEmail", email);
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.Save();
                Debug.Log("💾 Credentials saved to PlayerPrefs");
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedEmail");
                PlayerPrefs.DeleteKey("RememberMe");
                PlayerPrefs.Save();
                Debug.Log("🗑️ Credentials cleared from PlayerPrefs");
            }
            
            Debug.Log("✅ Login successful! Loading character selection...");
            // Hide login panel and show character selection
            if (emailInputField != null) emailInputField.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("⚠️ Please enter both email and password");
        }
    }
    
    void OnSettingsClicked()
    {
        Debug.Log("⚙️ Settings button clicked!");
        // TODO: Load settings scene
    }
    
    void OnExitClicked()
    {
        Debug.Log("🚪 Exit button clicked!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnCreateCharacterClicked()
    {
        Debug.Log("🎨 Create Character button clicked!");
        // TODO: Load character creation scene
        StartCoroutine(LoadCharacterCreation());
    }
    
    void OnEnterWorldClicked()
    {
        if (selectedCharacterIndex >= 0)
        {
            Debug.Log($"🎮 Entering world with character: {characters[selectedCharacterIndex].name}");
            // TODO: Load game world scene
            StartCoroutine(LoadGameWorld());
        }
        else
        {
            Debug.Log("⚠️ No character selected!");
        }
    }
    
    void OnDeleteCharacterClicked()
    {
        if (selectedCharacterIndex >= 0)
        {
            Debug.Log($"🗑️ Delete character: {characters[selectedCharacterIndex].name}");
            // TODO: Show delete confirmation dialog
        }
        else
        {
            Debug.Log("⚠️ No character selected!");
        }
    }
    
    void OnLogoutClicked()
    {
        Debug.Log("🚪 Logout button clicked!");
        // TODO: Return to login scene
        StartCoroutine(ReturnToLogin());
    }
    
    IEnumerator LoadCharacterCreation()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("🎨 Loading character creation scene...");
        // TODO: SceneManager.LoadScene("CharacterCreation");
    }
    
    IEnumerator LoadGameWorld()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("🌍 Loading game world scene...");
        // TODO: SceneManager.LoadScene("GameWorld");
    }
    
    IEnumerator ReturnToLogin()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("🔙 Returning to login scene...");
        // TODO: SceneManager.LoadScene("Login");
    }
    
    void Update()
    {
        // Handle keyboard input for testing
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (emailInputField != null && emailInputField.isFocused)
            {
                Debug.Log("🎯 Enter key pressed in email field - focusing password");
                passwordInputField.Select();
            }
            else if (passwordInputField != null && passwordInputField.isFocused)
            {
                Debug.Log("🎯 Enter key pressed in password field - attempting login");
                OnLoginClicked();
            }
        }
        
        // Tab key navigation between input fields
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (emailInputField != null && emailInputField.isFocused)
            {
                Debug.Log("🎯 Tab key pressed - moving to password field");
                passwordInputField.Select();
            }
            else if (passwordInputField != null && passwordInputField.isFocused)
            {
                Debug.Log("🎯 Tab key pressed - moving to remember me toggle");
                rememberMeToggle.Select();
            }
        }
        
        // Test UI interaction with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🎯 T key pressed - testing UI interaction...");
            TestUIInteraction();
        }
        
        // Monitor for duplicate audio listeners (press A to check)
        if (Input.GetKeyDown(KeyCode.A))
        {
            CheckAndCleanAudioListeners();
        }
        
        // Press D to debug what's creating audio listeners
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugAudioListenerSources();
        }
        
        // AGGRESSIVE audio listener monitoring - check every frame
        if (Time.frameCount % 30 == 0) // Every 30 frames (about every 0.5 seconds)
        {
            AggressiveAudioListenerCleanup();
        }
    }
    
    void AggressiveAudioListenerCleanup()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (audioListeners.Length > 1)
        {
            Debug.Log($"🚨 AGGRESSIVE CLEANUP: Found {audioListeners.Length} AudioListeners!");
            
            // Find our EventSystem AudioListener (should be the first one)
            AudioListener keepListener = null;
            for (int i = 0; i < audioListeners.Length; i++)
            {
                if (audioListeners[i].GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                {
                    keepListener = audioListeners[i];
                    Debug.Log($"🎯 Keeping EventSystem AudioListener: {keepListener.name}");
                    break;
                }
            }
            
            // Destroy ALL other AudioListeners
            for (int i = 0; i < audioListeners.Length; i++)
            {
                if (audioListeners[i] != keepListener)
                {
                    Debug.Log($"🔇 DESTROYING duplicate AudioListener: {audioListeners[i].name} on {audioListeners[i].gameObject.name}");
                    DestroyImmediate(audioListeners[i].gameObject);
                }
            }
            
            // Verify cleanup
            var remainingListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            Debug.Log($"✅ Aggressive cleanup complete. Remaining AudioListeners: {remainingListeners.Length}");
        }
    }
    
    void CheckAndCleanAudioListeners()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🔇 Found {audioListeners.Length} AudioListeners in scene");
        
        if (audioListeners.Length > 1)
        {
            Debug.Log("⚠️ Multiple AudioListeners detected! Cleaning up...");
            
            // Keep the first one (our EventSystem one), remove the rest
            for (int i = 1; i < audioListeners.Length; i++)
            {
                Debug.Log($"🔇 Removing duplicate AudioListener: {audioListeners[i].name}");
                DestroyImmediate(audioListeners[i].gameObject);
            }
            
            // Verify cleanup
            var remainingListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            Debug.Log($"✅ Cleanup complete. Remaining AudioListeners: {remainingListeners.Length}");
        }
        else
        {
            Debug.Log("✅ AudioListener count is correct (1)");
        }
    }
    
    void DebugAudioListenerSources()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"🔍 DEBUG: Found {audioListeners.Length} AudioListeners in scene");
        
        for (int i = 0; i < audioListeners.Length; i++)
        {
            var listener = audioListeners[i];
            var gameObject = listener.gameObject;
            
            Debug.Log($"🔍 AudioListener {i}: {listener.name} on GameObject: {gameObject.name}");
            Debug.Log($"🔍 GameObject active: {gameObject.activeInHierarchy}");
            Debug.Log($"🔍 GameObject layer: {LayerMask.LayerToName(gameObject.layer)}");
            
            // Check what components are on this GameObject
            var components = gameObject.GetComponents<Component>();
            Debug.Log($"🔍 Components on {gameObject.name}:");
            foreach (var component in components)
            {
                if (component != null)
                {
                    Debug.Log($"  - {component.GetType().Name}");
                }
            }
            
            // Check parent hierarchy
            var parent = gameObject.transform.parent;
            if (parent != null)
            {
                Debug.Log($"🔍 Parent: {parent.name}");
            }
            else
            {
                Debug.Log($"🔍 No parent (root object)");
            }
            
            Debug.Log("---");
        }
    }
    
    void TestUIInteraction()
    {
        // Test if buttons are properly set up
        if (loginButton != null)
        {
            Debug.Log($"🎯 Login button exists: {loginButton.name}");
            Debug.Log($"🎯 Login button active: {loginButton.gameObject.activeInHierarchy}");
            Debug.Log($"🎯 Login button enabled: {loginButton.enabled}");
            Debug.Log($"🎯 Login button has onClick: {loginButton.onClick.GetPersistentEventCount() > 0}");
            
            // Try to programmatically click the button
            loginButton.onClick.Invoke();
        }
        else
        {
            Debug.Log("⚠️ Login button is null!");
        }
        
        // Test if input fields are properly set up
        if (emailInputField != null)
        {
            Debug.Log($"🎯 Email input field exists: {emailInputField.name}");
            Debug.Log($"🎯 Email input field active: {emailInputField.gameObject.activeInHierarchy}");
            Debug.Log($"🎯 Email input field enabled: {emailInputField.enabled}");
            
            // Try to focus the input field
            emailInputField.Select();
            Debug.Log("🎯 Attempted to focus email input field");
        }
        else
        {
            Debug.Log("⚠️ Email input field is null!");
        }
    }
    
    // Public method to test button clicks from console
    [ContextMenu("Test Login Button Click")]
    public void TestLoginButtonClick()
    {
        Debug.Log("🎯 Testing login button click from context menu...");
        if (loginButton != null)
        {
            loginButton.onClick.Invoke();
        }
        else
        {
            Debug.Log("⚠️ Login button is null!");
        }
    }
    
    // Public method to test input field focus from console
    [ContextMenu("Test Input Field Focus")]
    public void TestInputFieldFocus()
    {
        Debug.Log("🎯 Testing input field focus from context menu...");
        if (emailInputField != null)
        {
            emailInputField.Select();
            Debug.Log("🎯 Email input field focused!");
        }
        else
        {
            Debug.Log("⚠️ Email input field is null!");
        }
    }
}



// Character selection data structure (simple wrapper for UI display)
[System.Serializable]
public class CharacterSelectionData
{
    public string name;
    public int level;
    public string characterClass;
    public string race;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int vitality;
    
    public CharacterSelectionData(string name, int level, string characterClass, string race, int strength, int dexterity, int intelligence, int vitality)
    {
        this.name = name;
        this.level = level;
        this.characterClass = characterClass;
        this.race = race;
        this.strength = strength;
        this.dexterity = dexterity;
        this.intelligence = intelligence;
        this.vitality = vitality;
    }
}

// Character slot component
public class CharacterSlot : MonoBehaviour
{
    public int index;
    public CharacterSelectionData character;
    public Button button;
    
    public void Initialize(int index, CharacterSelectionData character, Button button)
    {
        this.index = index;
        this.character = character;
        this.button = button;
    }
}

// AudioListener Guard - Prevents new AudioListeners from being created
public class AudioListenerGuard : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("🛡️ AudioListenerGuard activated - monitoring for new AudioListeners");
    }
    
    void Update()
    {
        // Check every frame for new AudioListeners
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (audioListeners.Length > 1)
        {
            Debug.Log($"🛡️ GUARD: Detected {audioListeners.Length} AudioListeners! Cleaning up...");
            
            // Find our EventSystem AudioListener
            AudioListener keepListener = null;
            for (int i = 0; i < audioListeners.Length; i++)
            {
                if (audioListeners[i].GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                {
                    keepListener = audioListeners[i];
                    break;
                }
            }
            
            // Destroy ALL other AudioListeners immediately
            for (int i = 0; i < audioListeners.Length; i++)
            {
                if (audioListeners[i] != keepListener)
                {
                    Debug.Log($"🛡️ GUARD: DESTROYING rogue AudioListener on {audioListeners[i].gameObject.name}");
                    DestroyImmediate(audioListeners[i].gameObject);
                }
            }
        }
    }
}
