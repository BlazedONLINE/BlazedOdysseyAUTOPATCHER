using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlazedOdysseyMMO.UI;

namespace BlazedOdysseyMMO.UI
{
    /// <summary>
    /// Simple Settings UI that provides basic game settings functionality
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Button backButton;
        [SerializeField] private Button applyButton;
        
        [Header("Settings")]
        [SerializeField] private float defaultVolume = 1f;
        [SerializeField] private bool defaultFullscreen = true;
        [SerializeField] private int defaultQualityLevel = 2;
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            LoadCurrentSettings();
        }
        
        private void InitializeUI()
        {
            // Setup volume slider
            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.value = defaultVolume;
            }
            
            // Setup resolution dropdown
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                Resolution[] resolutions = Screen.resolutions;
                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = $"{resolutions[i].width} x {resolutions[i].height}";
                    resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
                }
                resolutionDropdown.value = resolutions.Length - 1; // Default to highest resolution
            }
            
            // Setup quality dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                string[] qualityNames = QualitySettings.names;
                for (int i = 0; i < qualityNames.Length; i++)
                {
                    qualityDropdown.options.Add(new TMP_Dropdown.OptionData(qualityNames[i]));
                }
                qualityDropdown.value = defaultQualityLevel;
            }
            
            // Setup fullscreen toggle
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = defaultFullscreen;
            }
        }
        
        private void SetupEventListeners()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);
                
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyButtonClicked);
        }
        
        private void LoadCurrentSettings()
        {
            // Load current volume
            if (volumeSlider != null)
                volumeSlider.value = AudioListener.volume;
                
            // Load current fullscreen state
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = Screen.fullScreen;
                
            // Load current quality level
            if (qualityDropdown != null)
                qualityDropdown.value = QualitySettings.GetQualityLevel();
        }
        
        public void OnVolumeChanged(float volume)
        {
            AudioListener.volume = volume;
            Debug.Log($"Volume changed to: {volume}");
        }
        
        public void OnResolutionChanged(int index)
        {
            if (resolutionDropdown != null && index >= 0 && index < Screen.resolutions.Length)
            {
                Resolution resolution = Screen.resolutions[index];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height}");
            }
        }
        
        public void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            Debug.Log($"Fullscreen toggled: {isFullscreen}");
        }
        
        public void OnQualityChanged(int level)
        {
            if (level >= 0 && level < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(level);
                Debug.Log($"Quality changed to: {QualitySettings.names[level]}");
            }
        }
        
        private void OnBackButtonClicked()
        {
            Debug.Log("Back button clicked - returning to previous scene");
            // You can implement scene transition logic here
            // For now, just log the action
        }
        
        private void OnApplyButtonClicked()
        {
            Debug.Log("Settings applied!");
            // Save settings to PlayerPrefs or other persistent storage
            SaveSettings();
        }
        
        private void SaveSettings()
        {
            // Save volume
            PlayerPrefs.SetFloat("GameVolume", AudioListener.volume);
            
            // Save fullscreen state
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
            
            // Save quality level
            PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
            
            // Save resolution index
            if (resolutionDropdown != null)
                PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
                
            PlayerPrefs.Save();
            Debug.Log("Settings saved to PlayerPrefs");
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackButtonClicked);
                
            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyButtonClicked);
        }
    }
}
