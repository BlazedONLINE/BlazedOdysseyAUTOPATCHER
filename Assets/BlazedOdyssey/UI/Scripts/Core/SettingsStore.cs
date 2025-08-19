#nullable enable
using UnityEngine;

namespace BlazedOdyssey.UI
{
    public static class SettingsStore
    {
        public static float Master = 1f, Music = 0.8f, Sfx = 0.9f;
        public static bool Fullscreen = true, VSync = true;
        public static float UiScale = 1f;

        const string K = "BD_Settings_";
        
        public static void Load()
        {
            Master = Mathf.Clamp01(PlayerPrefs.GetFloat(K+"Master", Master));
            Music  = Mathf.Clamp01(PlayerPrefs.GetFloat(K+"Music",  Music));
            Sfx    = Mathf.Clamp01(PlayerPrefs.GetFloat(K+"Sfx",    Sfx));
            Fullscreen = PlayerPrefs.GetInt(K+"Fullscreen", Fullscreen?1:0)==1;
            VSync      = PlayerPrefs.GetInt(K+"VSync",      VSync?1:0)==1;
            UiScale    = Mathf.Clamp(PlayerPrefs.GetFloat(K+"UiScale", UiScale), 0.75f, 1.5f);
            
#if UNITY_EDITOR
            Debug.Log($"[Settings] Loaded: Master={Master:F2}, Music={Music:F2}, Sfx={Sfx:F2}, Fullscreen={Fullscreen}, VSync={VSync}, UiScale={UiScale:F2}");
#endif
        }
        
        public static void Save()
        {
            // Clamp values before saving
            Master = Mathf.Clamp01(Master);
            Music  = Mathf.Clamp01(Music);
            Sfx    = Mathf.Clamp01(Sfx);
            UiScale = Mathf.Clamp(UiScale, 0.75f, 1.5f);
            
            PlayerPrefs.SetFloat(K+"Master", Master);
            PlayerPrefs.SetFloat(K+"Music",  Music);
            PlayerPrefs.SetFloat(K+"Sfx",    Sfx);
            PlayerPrefs.SetInt(K+"Fullscreen", Fullscreen?1:0);
            PlayerPrefs.SetInt(K+"VSync",      VSync?1:0);
            PlayerPrefs.SetFloat(K+"UiScale",  UiScale);
            PlayerPrefs.Save();
            
#if UNITY_EDITOR
            Debug.Log($"[Settings] Saved: Master={Master:F2}, Music={Music:F2}, Sfx={Sfx:F2}, Fullscreen={Fullscreen}, VSync={VSync}, UiScale={UiScale:F2}");
#endif
        }
    }
}