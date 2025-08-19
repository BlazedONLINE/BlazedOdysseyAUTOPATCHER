using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleCharacterCreator : MonoBehaviour
{
    [Header("UI References")]
    public GameObject raceSelectionPanel;
    public GameObject classSelectionPanel;
    public GameObject characterPreviewPanel;
    public Button createCharacterButton;
    
    [Header("Race Buttons")]
    public Button humanButton;
    public Button elfButton;
    public Button dwarfButton;
    public Button orcButton;
    
    [Header("Class Buttons")]
    public Button warriorButton;
    public Button mageButton;
    public Button rogueButton;
    public Button clericButton;
    
    [Header("Character Preview")]
    public Image characterPreviewImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI raceClassText;
    
    [Header("Input Fields")]
    public TMP_InputField characterNameInput;
    
    private string selectedRace = "";
    private string selectedClass = "";
    private Sprite currentCharacterSprite;
    
    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        UpdateUI();
    }
    
    void InitializeUI()
    {
        // Ensure all panels exist
        if (raceSelectionPanel == null) CreateRaceSelectionPanel();
        if (classSelectionPanel == null) CreateClassSelectionPanel();
        if (characterPreviewPanel == null) CreateCharacterPreviewPanel();
        if (createCharacterButton == null) CreateCharacterButton();
        
        // Initially hide class selection and character preview
        classSelectionPanel.SetActive(false);
        characterPreviewPanel.SetActive(false);
        createCharacterButton.gameObject.SetActive(false);
    }
    
    void CreateRaceSelectionPanel()
    {
        raceSelectionPanel = new GameObject("RaceSelectionPanel");
        raceSelectionPanel.transform.SetParent(transform, false);
        
        var panelImage = raceSelectionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
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
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    void CreateClassSelectionPanel()
    {
        classSelectionPanel = new GameObject("ClassSelectionPanel");
        classSelectionPanel.transform.SetParent(transform, false);
        
        var panelImage = classSelectionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
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
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    void CreateCharacterPreviewPanel()
    {
        characterPreviewPanel = new GameObject("CharacterPreviewPanel");
        characterPreviewPanel.transform.SetParent(transform, false);
        
        var panelImage = characterPreviewPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
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
        raceClassText = infoObj.AddComponent<TextMeshProUGUI>();
        raceClassText.text = "Select Race and Class";
        raceClassText.fontSize = 18;
        raceClassText.color = Color.white;
        
        var infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.45f, 0.3f);
        infoRect.anchorMax = new Vector2(0.9f, 0.7f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
    }
    
    void CreateCharacterButton()
    {
        createCharacterButton = new GameObject("CreateCharacterButton").AddComponent<Button>();
        createCharacterButton.transform.SetParent(transform, false);
        
        var buttonImage = createCharacterButton.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var buttonRect = createCharacterButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.4f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.6f, 0.15f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(createCharacterButton.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Create Character";
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void SetupButtonListeners()
    {
        // Race buttons
        if (humanButton != null) humanButton.onClick.AddListener(() => SelectRace("Human"));
        if (elfButton != null) elfButton.onClick.AddListener(() => SelectRace("Elf"));
        if (dwarfButton != null) dwarfButton.onClick.AddListener(() => SelectRace("Dwarf"));
        if (orcButton != null) orcButton.onClick.AddListener(() => SelectRace("Orc"));
        
        // Class buttons
        if (warriorButton != null) warriorButton.onClick.AddListener(() => SelectClass("Warrior"));
        if (mageButton != null) mageButton.onClick.AddListener(() => SelectClass("Mage"));
        if (rogueButton != null) rogueButton.onClick.AddListener(() => SelectClass("Rogue"));
        if (clericButton != null) clericButton.onClick.AddListener(() => SelectClass("Cleric"));
        
        // Create character button
        if (createCharacterButton != null) createCharacterButton.onClick.AddListener(CreateCharacter);
    }
    
    public void SelectRace(string race)
    {
        selectedRace = race;
        Debug.Log($"Selected Race: {race}");
        
        // Show class selection
        classSelectionPanel.SetActive(true);
        
        // Update UI
        UpdateUI();
    }
    
    public void SelectClass(string characterClass)
    {
        selectedClass = characterClass;
        Debug.Log($"Selected Class: {characterClass}");
        
        // Show character preview
        characterPreviewPanel.SetActive(true);
        createCharacterButton.gameObject.SetActive(true);
        
        // Update character preview
        UpdateCharacterPreview();
        
        // Update UI
        UpdateUI();
    }
    
    void UpdateCharacterPreview()
    {
        if (raceClassText != null)
        {
            raceClassText.text = $"{selectedRace} {selectedClass}";
        }
        
        // Here you would load the appropriate SPUM sprite based on race and class
        // For now, we'll just show a placeholder
        if (characterPreviewImage != null)
        {
            characterPreviewImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
    
    void UpdateUI()
    {
        // Update button states based on selections
        if (raceSelectionPanel != null)
        {
            raceSelectionPanel.SetActive(selectedRace == "");
        }
        
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(selectedRace != "" && selectedClass == "");
        }
        
        if (characterPreviewPanel != null)
        {
            characterPreviewPanel.SetActive(selectedRace != "" && selectedClass != "");
        }
        
        if (createCharacterButton != null)
        {
            createCharacterButton.gameObject.SetActive(selectedRace != "" && selectedClass != "");
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
