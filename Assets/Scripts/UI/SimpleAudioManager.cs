using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    void Start()
    {
        FixAudioListeners();
    }
    
    void FixAudioListeners()
    {
        Debug.Log("🔇 SimpleAudioManager: Checking for Audio Listener conflicts...");
        
        // Find all AudioListeners
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            Debug.Log("✅ No AudioListeners found - creating one on main camera");
            if (Camera.main != null)
            {
                Camera.main.gameObject.AddComponent<AudioListener>();
            }
            return;
        }
        
        if (listeners.Length == 1)
        {
            Debug.Log($"✅ Scene has exactly one AudioListener on: {listeners[0].gameObject.name}");
            return;
        }
        
        // Multiple AudioListeners - fix this
        Debug.LogWarning($"⚠️ Found {listeners.Length} AudioListeners! Fixing...");
        
        // Keep the first one, disable the rest
        for (int i = 1; i < listeners.Length; i++)
        {
            listeners[i].enabled = false;
            Debug.Log($"🔇 Disabled AudioListener on: {listeners[i].gameObject.name}");
        }
        
        Debug.Log($"✅ Fixed! Now have 1 enabled AudioListener on: {listeners[0].gameObject.name}");
    }
    
    [ContextMenu("Fix Audio Listeners")]
    void FixAudioListenersFromMenu()
    {
        FixAudioListeners();
    }
}
