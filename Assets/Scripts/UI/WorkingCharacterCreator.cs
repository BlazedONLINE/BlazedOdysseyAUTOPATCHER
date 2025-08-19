using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorkingCharacterCreator : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject raceSelectionPanel;
    public GameObject classSelectionPanel;
    public GameObject characterPreviewPanel;
    public GameObject characterNamePanel;
    
    [Header("Character Preview")]
    public Image characterPreviewImage;
    public TextMeshProUGUI characterInfoText;
    public TextMeshProUGUI characterNameText;
    
    [Header("Input")]
    public TMP_InputField characterNameInput;
    public Button createCharacterButton;
    
    private string selectedRace = "";
    private string selectedClass = "";
    private Dictionary<string, Dictionary<string, Sprite>> characterSprites;
    
    void Start()
    {
        InitializeUI();
        LoadCharacterSprites();
        SetupEventListeners();
        ShowRaceSelection();
    }
    
    void InitializeUI()
    {
        // Create race selection panel
        CreateRaceSelectionPanel();
        
        // Create class selection panel
        CreateClassSelectionPanel();
        
        // Create character preview panel
        CreateCharacterPreviewPanel();
        
        // Create character name input panel
        CreateCharacterNamePanel();
        
        // Initially hide all panels except race selection
        classSelectionPanel.SetActive(false);
        characterPreviewPanel.SetActive(false);
        characterNamePanel.SetActive(false);
    }
    
    void CreateRaceSelectionPanel()
    {
        raceSelectionPanel = new GameObject("RaceSelectionPanel");
        raceSelectionPanel.transform.SetParent(transform, false);
        
        var panelImage = raceSelectionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.1f, 0.2f, 0.95f); // Dark blue like login
        
        var panelRect = raceSelectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.6f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(raceSelectionPanel.transform, false);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select Your Race";
        titleText.fontSize = 28;
        titleText.color = new Color(1f, 0.6f, 0.0f, 1f); // Orange like login
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create race buttons
        CreateRaceButton("Human", new Vector2(-300, 0));
        CreateRaceButton("Elf", new Vector2(-100, 0));
        CreateRaceButton("Dwarf", new Vector2(100, 0));
        CreateRaceButton("Orc", new Vector2(300, 0));
    }
    
    void CreateRaceButton(string raceName, Vector2 position)
    {
        var buttonObj = new GameObject($"{raceName}Button").AddComponent<Button>();
        buttonObj.transform.SetParent(raceSelectionPanel.transform, false);
        
        var buttonImage = buttonObj.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(120, 50);
        
        // Add button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = raceName;
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add click listener
        buttonObj.onClick.AddListener(() => SelectRace(raceName));
    }
    
    void CreateClassSelectionPanel()
    {
        classSelectionPanel = new GameObject("ClassSelectionPanel");
        classSelectionPanel.transform.SetParent(transform, false);
        
        var panelImage = classSelectionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);
        
        var panelRect = classSelectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.3f);
        panelRect.anchorMax = new Vector2(0.9f, 0.6f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(classSelectionPanel.transform, false);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select Your Class";
        titleText.fontSize = 28;
        titleText.color = new Color(1f, 0.6f, 0.0f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create class buttons
        CreateClassButton("Warrior", new Vector2(-300, 0));
        CreateClassButton("Mage", new Vector2(-100, 0));
        CreateClassButton("Rogue", new Vector2(100, 0));
        CreateClassButton("Cleric", new Vector2(300, 0));
    }
    
    void CreateClassButton(string className, Vector2 position)
    {
        var buttonObj = new GameObject($"{className}Button").AddComponent<Button>();
        buttonObj.transform.SetParent(classSelectionPanel.transform, false);
        
        var buttonImage = buttonObj.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(120, 50);
        
        // Add button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = className;
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add click listener
        buttonObj.onClick.AddListener(() => SelectClass(className));
    }
    
    void CreateCharacterPreviewPanel()
    {
        characterPreviewPanel = new GameObject("CharacterPreviewPanel");
        characterPreviewPanel.transform.SetParent(transform, false);
        
        var panelImage = characterPreviewPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);
        
        var panelRect = characterPreviewPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.3f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add character preview image
        var previewObj = new GameObject("CharacterPreview");
        previewObj.transform.SetParent(characterPreviewPanel.transform, false);
        characterPreviewImage = previewObj.AddComponent<Image>();
        characterPreviewImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        var previewRect = previewObj.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.1f, 0.2f);
        previewRect.anchorMax = new Vector2(0.4f, 0.8f);
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;
        
        // Add character info text
        var infoObj = new GameObject("CharacterInfo");
        infoObj.transform.SetParent(characterPreviewPanel.transform, false);
        characterInfoText = infoObj.AddComponent<TextMeshProUGUI>();
        characterInfoText.text = "Select Race and Class";
        characterInfoText.fontSize = 20;
        characterInfoText.color = Color.white;
        characterInfoText.alignment = TextAlignmentOptions.Left;
        
        var infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.45f, 0.3f);
        infoRect.anchorMax = new Vector2(0.9f, 0.7f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
    }
    
    void CreateCharacterNamePanel()
    {
        characterNamePanel = new GameObject("CharacterNamePanel");
        characterNamePanel.transform.SetParent(transform, false);
        
        var panelImage = characterNamePanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);
        
        var panelRect = characterNamePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.15f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add character name input
        var inputObj = new GameObject("CharacterNameInput");
        inputObj.transform.SetParent(characterNamePanel.transform, false);
        characterNameInput = inputObj.AddComponent<TMP_InputField>();
        
        var inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.1f, 0.2f);
        inputRect.anchorMax = new Vector2(0.6f, 0.8f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;
        
        // Add input field background
        var inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Add text component
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        // Add placeholder
        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        var placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter character name...";
        placeholder.fontSize = 18;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholder.alignment = TextAlignmentOptions.Left;
        
        var placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        // Setup input field
        characterNameInput.textComponent = text;
        characterNameInput.placeholder = placeholder;
        characterNameInput.targetGraphic = inputImage;
        
        // Add create character button
        var buttonObj = new GameObject("CreateCharacterButton").AddComponent<Button>();
        buttonObj.transform.SetParent(characterNamePanel.transform, false);
        createCharacterButton = buttonObj;
        
        var buttonImage = buttonObj.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.65f, 0.2f);
        buttonRect.anchorMax = new Vector2(0.9f, 0.8f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        var buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Create Character";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        // Add click listener
        createCharacterButton.onClick.AddListener(CreateCharacter);
    }
    
    void LoadCharacterSprites()
    {
        characterSprites = new Dictionary<string, Dictionary<string, Sprite>>();
        
        // Load SPUM character sprites from Resources
        var allSprites = Resources.LoadAll<Sprite>("SPUM/Units");
        Debug.Log($"Found {allSprites.Length} SPUM sprites");
        
        // Try to map SPUM sprites to our race/class system
        if (allSprites.Length > 0)
        {
            MapSPUMSprites(allSprites);
        }
        else
        {
            Debug.LogWarning("No SPUM sprites found! Creating placeholders...");
            // Initialize empty dictionaries and create placeholders
            characterSprites["Human"] = new Dictionary<string, Sprite>();
            characterSprites["Elf"] = new Dictionary<string, Sprite>();
            characterSprites["Dwarf"] = new Dictionary<string, Sprite>();
            characterSprites["Orc"] = new Dictionary<string, Sprite>();
            FillMissingSprites();
        }
    }
    
    void MapSPUMSprites(Sprite[] sprites)
    {
        // Initialize dictionaries for each race
        characterSprites["Human"] = new Dictionary<string, Sprite>();
        characterSprites["Elf"] = new Dictionary<string, Sprite>();
        characterSprites["Dwarf"] = new Dictionary<string, Sprite>();
        characterSprites["Orc"] = new Dictionary<string, Sprite>();
        
        // Try to find and map SPUM sprites
        foreach (var sprite in sprites)
        {
            string spriteName = sprite.name.ToLower();
            Debug.Log($"Processing SPUM sprite: {spriteName}");
            
            // Simple mapping logic - you can improve this based on your SPUM naming convention
            if (spriteName.Contains("human") || spriteName.Contains("warrior"))
            {
                if (!characterSprites["Human"].ContainsKey("Warrior"))
                    characterSprites["Human"]["Warrior"] = sprite;
            }
            else if (spriteName.Contains("elf") || spriteName.Contains("mage"))
            {
                if (!characterSprites["Elf"].ContainsKey("Mage"))
                    characterSprites["Elf"]["Mage"] = sprite;
            }
            else if (spriteName.Contains("dwarf") || spriteName.Contains("rogue"))
            {
                if (!characterSprites["Dwarf"].ContainsKey("Rogue"))
                    characterSprites["Dwarf"]["Rogue"] = sprite;
            }
            else if (spriteName.Contains("orc") || spriteName.Contains("cleric"))
            {
                if (!characterSprites["Orc"].ContainsKey("Cleric"))
                    characterSprites["Orc"]["Cleric"] = sprite;
            }
        }
        
        // Fill any missing combinations with placeholders
        FillMissingSprites();
    }
    
    void FillMissingSprites()
    {
        string[] classes = { "Warrior", "Mage", "Rogue", "Cleric" };
        
        foreach (var race in characterSprites.Keys)
        {
            foreach (var className in classes)
            {
                if (!characterSprites[race].ContainsKey(className))
                {
                    Debug.Log($"Creating placeholder for {race} {className}");
                    characterSprites[race][className] = CreateColoredSprite(GetClassColor(className));
                }
            }
        }
    }
    
    Color GetClassColor(string className)
    {
        switch (className)
        {
            case "Warrior": return Color.red;
            case "Mage": return Color.blue;
            case "Rogue": return Color.green;
            case "Cleric": return Color.yellow;
            default: return Color.gray;
        }
    }
    
    // CreatePlaceholderSprites method removed - now handled by MapSPUMSprites
    
    Sprite CreateColoredSprite(Color color)
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    void SetupEventListeners()
    {
        // Button listeners are set up in the button creation methods
    }
    
    void ShowRaceSelection()
    {
        raceSelectionPanel.SetActive(true);
        classSelectionPanel.SetActive(false);
        characterPreviewPanel.SetActive(false);
        characterNamePanel.SetActive(false);
    }
    
    void ShowClassSelection()
    {
        raceSelectionPanel.SetActive(false);
        classSelectionPanel.SetActive(true);
        characterPreviewPanel.SetActive(false);
        characterNamePanel.SetActive(false);
    }
    
    void ShowCharacterPreview()
    {
        raceSelectionPanel.SetActive(false);
        classSelectionPanel.SetActive(false);
        characterPreviewPanel.SetActive(true);
        characterNamePanel.SetActive(false);
    }
    
    void ShowCharacterNameInput()
    {
        raceSelectionPanel.SetActive(false);
        classSelectionPanel.SetActive(false);
        characterPreviewPanel.SetActive(true);
        characterNamePanel.SetActive(true);
    }
    
    public void SelectRace(string race)
    {
        selectedRace = race;
        Debug.Log($"Selected Race: {race}");
        ShowClassSelection();
    }
    
    public void SelectClass(string className)
    {
        selectedClass = className;
        Debug.Log($"Selected Class: {className}");
        UpdateCharacterPreview();
        ShowCharacterPreview();
        
        // Show character name input after a short delay
        Invoke(nameof(ShowCharacterNameInput), 1f);
    }
    
    void UpdateCharacterPreview()
    {
        if (characterInfoText != null)
        {
            characterInfoText.text = $"{selectedRace} {selectedClass}\n\nA mighty {selectedRace.ToLower()} {selectedClass.ToLower()} ready for adventure!";
        }
        
        // Update character sprite
        if (characterPreviewImage != null && characterSprites.ContainsKey(selectedRace) && characterSprites[selectedRace].ContainsKey(selectedClass))
        {
            characterPreviewImage.sprite = characterSprites[selectedRace][selectedClass];
            characterPreviewImage.color = Color.white;
        }
        else
        {
            characterPreviewImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
    
    void CreateCharacter()
    {
        if (string.IsNullOrEmpty(selectedRace) || string.IsNullOrEmpty(selectedClass))
        {
            Debug.LogWarning("Please select both race and class before creating character!");
            return;
        }
        
        string characterName = characterNameInput != null ? characterNameInput.text : "Adventurer";
        if (string.IsNullOrEmpty(characterName))
        {
            characterName = "Adventurer";
        }
        
        Debug.Log($"Creating character: {characterName} - {selectedRace} {selectedClass}");
        
        // Here you would actually create the character and save it
        // For now, just log the creation
        
        // Transition to character selection or game
        Debug.Log("Character created successfully! Transitioning...");
    }
}
