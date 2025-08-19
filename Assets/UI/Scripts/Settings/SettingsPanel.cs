using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BlazedOdyssey.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        public RectTransform window;
        public GameObject dimOverlay;
        public Slider musicVolume; // music volume slider
        public Toggle musicMute; // mute toggle
        public Slider uiScale;
        public Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Button applyButton;
        public Button closeButton;
        public Button backToCharacterSelectButton; // New button for MMO
        public RectTransform uiRootToScale; // scale the whole UI

        private Resolution[] _resolutions;
        private int _selectedResIndex;

        private void Awake()
        {
            if (window) window.gameObject.SetActive(false);
            if (dimOverlay) dimOverlay.SetActive(false);

            // Music volume (GameAudioManager)
            if (musicVolume)
            {
                musicVolume.minValue = 0f;
                musicVolume.maxValue = 1f;
                float mv = PlayerPrefs.GetFloat("bo_musicVol", 0.6f);
                bool muted = PlayerPrefs.GetInt("bo_musicMute", 0) == 1;
                _lastUnmutedMusic = mv;
                musicVolume.value = muted ? 0f : mv;
                var mgr = FindObjectOfType<GameAudioManager>();
                if (mgr != null) mgr.SetMusicVolume(mv);
                musicVolume.onValueChanged.AddListener(v => {
                    if (musicMute && musicMute.isOn) return; // ignore while muted
                    _lastUnmutedMusic = v;
                    var m = FindObjectOfType<GameAudioManager>(); if (m != null) m.SetMusicVolume(v);
                });
            }

            if (musicMute)
            {
                musicMute.isOn = PlayerPrefs.GetInt("bo_musicMute", 0) == 1;
                musicMute.onValueChanged.AddListener(on => {
                    if (on)
                    {
                        if (musicVolume) { _lastUnmutedMusic = musicVolume.value; musicVolume.value = 0f; }
                        var m = FindObjectOfType<GameAudioManager>(); if (m != null) m.SetMusicVolume(0f);
                    }
                    else
                    {
                        if (musicVolume) musicVolume.value = Mathf.Clamp01(_lastUnmutedMusic);
                        var m = FindObjectOfType<GameAudioManager>(); if (m != null) m.SetMusicVolume(Mathf.Clamp01(_lastUnmutedMusic));
                    }
                });
            }

            // UI Scale (0.75–1.25)
            if (uiScale)
            {
                uiScale.minValue = 0.75f; uiScale.maxValue = 1.25f;
                uiScale.value = PlayerPrefs.GetFloat("bo_uiScale", 1f);
                uiScale.onValueChanged.AddListener(v => { if (uiRootToScale) uiRootToScale.localScale = new Vector3(v, v, 1f); });
                if (uiRootToScale) uiRootToScale.localScale = new Vector3(uiScale.value, uiScale.value, 1f);
            }

            // Display
            _resolutions = Screen.resolutions.Distinct().ToArray();
            if (resolutionDropdown)
            {
                resolutionDropdown.ClearOptions();
                var options = _resolutions.Select(r => $"{r.width}×{r.height} @{r.refreshRateRatio.value:F0}Hz").ToList();
                resolutionDropdown.AddOptions(options);
                _selectedResIndex = System.Array.FindIndex(_resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
                if (_selectedResIndex < 0) _selectedResIndex = options.Count - 1;
                resolutionDropdown.value = _selectedResIndex;
            }
            if (fullscreenToggle)
                fullscreenToggle.isOn = Screen.fullScreen;

            if (applyButton)
                applyButton.onClick.AddListener(ApplySettings);
            if (closeButton)
                closeButton.onClick.AddListener(Hide);
            if (backToCharacterSelectButton)
                backToCharacterSelectButton.onClick.AddListener(BackToCharacterSelect);
        }

        private void ApplySettings()
        {
            if (musicVolume)
            {
                var vol = (musicMute && musicMute.isOn) ? _lastUnmutedMusic : musicVolume.value;
                PlayerPrefs.SetFloat("bo_musicVol", vol);
            }
            if (musicMute) PlayerPrefs.SetInt("bo_musicMute", musicMute.isOn ? 1 : 0);
            if (uiScale) PlayerPrefs.SetFloat("bo_uiScale", uiScale.value);

            if (_resolutions != null && resolutionDropdown)
            {
                _selectedResIndex = resolutionDropdown.value;
                var r = _resolutions[Mathf.Clamp(_selectedResIndex, 0, _resolutions.Length - 1)];
                Screen.SetResolution(r.width, r.height, fullscreenToggle && fullscreenToggle.isOn);
            }

            PlayerPrefs.Save();
        }

        public void Toggle()
        {
            if (!window) return;
            bool next = !window.gameObject.activeSelf;
            window.gameObject.SetActive(next);
            if (dimOverlay) dimOverlay.SetActive(next);
        }

        public void Show()
        {
            if (window)
            {
                window.gameObject.SetActive(true);
                // Ensure it's moved down as requested
                window.anchoredPosition = new Vector2(0, -260f);
            }
            if (dimOverlay) dimOverlay.SetActive(true);
        }

        public void Hide()
        {
            if (window) window.gameObject.SetActive(false);
            if (dimOverlay) dimOverlay.SetActive(false);
        }

        private float _lastUnmutedMusic = 0.6f;
        
        /// <summary>
        /// Return to character selection scene
        /// </summary>
        private void BackToCharacterSelect()
        {
            Debug.Log("🎭 Returning to character selection...");
            
            // Save current character position and data if in game
            var characterManager = MMOCharacterManager.Instance;
            if (characterManager != null && characterManager.HasSelectedCharacter)
            {
                var currentChar = characterManager.CurrentCharacter;
                
                // Update position from current player transform if available
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    currentChar.positionX = player.transform.position.x;
                    currentChar.positionY = player.transform.position.y;
                    currentChar.positionZ = player.transform.position.z;
                    currentChar.sceneName = SceneManager.GetActiveScene().name;
                }
                
                // Save character data asynchronously
                _ = characterManager.UpdateCharacter(currentChar);
            }
            
            // Logout from current character (but keep account logged in)
            if (characterManager != null)
            {
                characterManager.SelectCharacter(null);
            }
            
            // Load character selection scene
            SceneManager.LoadScene("CharacterSelection");
        }
    }
}
