#nullable enable
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    public class MenuSettingsController : MonoBehaviour
    {
        [Header("Audio")]
        public AudioMixer? mixer; // Expose "MasterVol","MusicVol","SfxVol" (in dB)
        public Slider? masterSlider, musicSlider, sfxSlider;

        [Header("Video")]
        public Toggle? fullscreenToggle, vsyncToggle;
        public Slider? uiScaleSlider;
        public CanvasScaler? canvasScaler;

        private bool _mixerWarningShown = false;
        private bool _canvasScalerWarningShown = false;

        void Awake()
        {
            SettingsStore.Load();
            // init UI values
            if (masterSlider) masterSlider.value = SettingsStore.Master;
            if (musicSlider)  musicSlider.value  = SettingsStore.Music;
            if (sfxSlider)    sfxSlider.value    = SettingsStore.Sfx;
            if (fullscreenToggle) fullscreenToggle.isOn = SettingsStore.Fullscreen;
            if (vsyncToggle)      vsyncToggle.isOn      = SettingsStore.VSync;
            if (uiScaleSlider)    uiScaleSlider.value   = SettingsStore.UiScale;

            ApplyAllRuntime(); // reflect current saved state on open
        }

        // --- Called by UI events (OnValueChanged) ---
        public void OnMaster(float v)
        { 
            SettingsStore.Master = Mathf.Clamp01(v); 
            SetMixer("MasterVol", SettingsStore.Master);
            Debug.Log($"[Settings] Master volume → {SettingsStore.Master:F2}");
        }
        
        public void OnMusic(float v)
        { 
            SettingsStore.Music = Mathf.Clamp01(v); 
            SetMixer("MusicVol", SettingsStore.Music);
            Debug.Log($"[Settings] Music volume → {SettingsStore.Music:F2}");
        }
        
        public void OnSfx(float v)
        { 
            SettingsStore.Sfx = Mathf.Clamp01(v); 
            SetMixer("SfxVol", SettingsStore.Sfx);
            Debug.Log($"[Settings] SFX volume → {SettingsStore.Sfx:F2}");
        }
        
        public void OnFullscreen(bool on)
        { 
            SettingsStore.Fullscreen = on; 
            Screen.fullScreen = on;
            Debug.Log($"[Settings] Fullscreen → {on}");
        }
        
        public void OnVSync(bool on)
        { 
            SettingsStore.VSync = on; 
            QualitySettings.vSyncCount = on ? 1 : 0;
            Debug.Log($"[Settings] VSync → {on}");
        }
        
        public void OnUiScale(float v)
        {
            SettingsStore.UiScale = Mathf.Clamp(v, 0.75f, 1.5f);
            if (canvasScaler) 
            {
                canvasScaler.scaleFactor = SettingsStore.UiScale;
                Debug.Log($"[Settings] UI scale → {SettingsStore.UiScale:F2}");
            }
            else if (!_canvasScalerWarningShown)
            {
                Debug.LogWarning("[Settings] CanvasScaler not assigned, UI scale changes won't take effect");
                _canvasScalerWarningShown = true;
            }
        }

        // --- Buttons ---
        public void OnApply()
        {
            SettingsStore.Save();
            ApplyAllRuntime();
            Debug.Log("[Settings] Applied & Saved");
        }
        
        public void OnClose() 
        { 
            gameObject.SetActive(false);
            Debug.Log("[Settings] Closed");
        }

        void ApplyAllRuntime()
        {
            // Audio
            SetMixer("MasterVol", SettingsStore.Master);
            SetMixer("MusicVol",  SettingsStore.Music);
            SetMixer("SfxVol",    SettingsStore.Sfx);
            
            // Video
            Screen.fullScreen = SettingsStore.Fullscreen;
            QualitySettings.vSyncCount = SettingsStore.VSync ? 1 : 0;
            
            if (canvasScaler) 
            {
                canvasScaler.scaleFactor = Mathf.Clamp(SettingsStore.UiScale, 0.75f, 1.5f);
            }
            else if (!_canvasScalerWarningShown)
            {
                Debug.LogWarning("[Settings] CanvasScaler not assigned, UI scale changes won't take effect");
                _canvasScalerWarningShown = true;
            }
        }

        void SetMixer(string exposedParam, float linear01)
        {
            if (!mixer) 
            {
                if (!_mixerWarningShown)
                {
                    Debug.LogWarning("[Settings] AudioMixer not assigned, volume changes won't take effect");
                    _mixerWarningShown = true;
                }
                return;
            }

            // Convert 0..1 to decibels (-80..0). Clamp to avoid -inf.
            float dB = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(linear01));
            
            if (!mixer.SetFloat(exposedParam, dB))
            {
                Debug.LogWarning($"[Settings] AudioMixer parameter '{exposedParam}' not found or not exposed");
            }
        }
    }
}