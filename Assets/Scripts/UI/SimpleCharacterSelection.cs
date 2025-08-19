using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleCharacterSelection : MonoBehaviour
{
    [Header("UI References")]
    public GameObject characterListPanel;
    public Button createNewCharacterButton;
    public Button backToLoginButton;
    
    [Header("Character Slots")]
    public Transform characterSlotContainer;
    public GameObject characterSlotPrefab;
    
    [Header("Character Info")]
    public TextMeshProUGUI selectedCharacterName;
    public TextMeshProUGUI selectedCharacterRace;
    public TextMeshProUGUI selectedCharacterClass;
    public Image selectedCharacterImage;
    
    private List<CharacterData> characters = new List<CharacterData>();
    private CharacterData selectedCharacter;
    
    [System.Serializable]
    public class CharacterData
    {
        public string name;
        public string race;
        public string characterClass;
        public Sprite sprite;
        public int level;
    }
    
    void Start()
    {
        InitializeUI();
        LoadSampleCharacters();
        CreateCharacterSlots();
    }
    
    void InitializeUI()
    {
        // Create main panel if it doesn't exist
        if (characterListPanel == null) CreateCharacterListPanel();
        if (createNewCharacterButton == null) CreateNewCharacterButton();
        if (backToLoginButton == null) CreateBackToLoginButton();
        if (characterSlotContainer == null) CreateCharacterSlotContainer();
        
        // Set up button listeners
        if (createNewCharacterButton != null)
        {
            var button = createNewCharacterButton.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(CreateNewCharacter);
        }
        
        if (backToLoginButton != null)
        {
            var button = backToLoginButton.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(BackToLogin);
        }
    }
    
    void CreateCharacterListPanel()
    {
        characterListPanel = new GameObject("CharacterListPanel");
        characterListPanel.transform.SetParent(transform, false);
        
        var panelImage = characterListPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.1f, 0.2f, 0.95f); // Dark blue like login
        
        var panelRect = characterListPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(characterListPanel.transform, false);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select Your Character";
        titleText.fontSize = 32;
        titleText.color = new Color(1f, 0.6f, 0.0f, 1f); // Orange like login
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    void CreateNewCharacterButton()
    {
        var buttonObj = new GameObject("CreateNewCharacterButton");
        createNewCharacterButton = buttonObj.AddComponent<Button>();
        buttonObj.transform.SetParent(characterListPanel.transform, false);
        
        var buttonImage = createNewCharacterButton.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        var buttonRect = createNewCharacterButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.7f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.9f, 0.12f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(createNewCharacterButton.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Create New";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void CreateBackToLoginButton()
    {
        var buttonObj = new GameObject("BackToLoginButton");
        backToLoginButton = buttonObj.AddComponent<Button>();
        buttonObj.transform.SetParent(characterListPanel.transform, false);
        
        var buttonImage = backToLoginButton.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.6f, 0.2f, 0.2f, 1f);
        
        var buttonRect = backToLoginButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.1f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.3f, 0.12f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(backToLoginButton.transform, false);
        var buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Back to Login";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void CreateCharacterSlotContainer()
    {
        var containerObj = new GameObject("CharacterSlotContainer", typeof(RectTransform));
        containerObj.transform.SetParent(characterListPanel.transform, false);
        characterSlotContainer = containerObj.transform;
        
        var containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.15f);
        containerRect.anchorMax = new Vector2(0.9f, 0.8f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
    }
    
    void LoadSampleCharacters()
    {
        // Add some sample characters for testing
        characters.Add(new CharacterData { name = "Thorin", race = "Dwarf", characterClass = "Warrior", level = 5 });
        characters.Add(new CharacterData { name = "Legolas", race = "Elf", characterClass = "Rogue", level = 3 });
        characters.Add(new CharacterData { name = "Gandalf", race = "Human", characterClass = "Mage", level = 8 });
    }
    
    void CreateCharacterSlots()
    {
        if (characterSlotContainer == null) return;
        
        for (int i = 0; i < characters.Count; i++)
        {
            CreateCharacterSlot(characters[i], i);
        }
    }
    
    void CreateCharacterSlot(CharacterData character, int index)
    {
        var slotObj = new GameObject($"CharacterSlot_{index}");
        slotObj.transform.SetParent(characterSlotContainer, false);
        
        var slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        var slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0, 0.8f - (index * 0.2f));
        slotRect.anchorMax = new Vector2(1, 1f - (index * 0.2f));
        slotRect.offsetMin = new Vector2(10, 5);
        slotRect.offsetMax = new Vector2(-10, -5);
        
        // Add character info
        var infoObj = new GameObject("CharacterInfo");
        infoObj.transform.SetParent(slotObj.transform, false);
        
        var infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = $"{character.name} - Level {character.level} {character.race} {character.characterClass}";
        infoText.fontSize = 16;
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Left;
        
        var infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.1f, 0.2f);
        infoRect.anchorMax = new Vector2(0.9f, 0.8f);
        infoRect.offsetMin = Vector2.zero;
        infoRect.offsetMax = Vector2.zero;
        
        // Add button component for selection
        var button = slotObj.AddComponent<Button>();
        button.onClick.AddListener(() => SelectCharacter(character));
        
        // Add hover effect
        var colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        button.colors = colors;
    }
    
    void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        Debug.Log($"Selected character: {character.name}");
        
        // Update character info display
        if (selectedCharacterName != null) selectedCharacterName.text = character.name;
        if (selectedCharacterRace != null) selectedCharacterRace.text = character.race;
        if (selectedCharacterClass != null) selectedCharacterClass.text = character.characterClass;
        
        // Here you would load the character sprite
        if (selectedCharacterImage != null)
        {
            selectedCharacterImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
    
    void CreateNewCharacter()
    {
        Debug.Log("Creating new character...");
        // Here you would transition to character creation scene
        // For now, just log it
    }
    
    void BackToLogin()
    {
        Debug.Log("Going back to login...");
        // Here you would transition back to login scene
        // For now, just log it
    }
}
