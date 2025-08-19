using UnityEngine;
using UnityEditor;

public class SceneAudioCleanup : MonoBehaviour
{
    [Header("Audio Cleanup")]
    public bool autoCleanupOnStart = true;
    public bool logCleanupDetails = true;
    
    void Start()
    {
        if (autoCleanupOnStart)
        {
            CleanupAudioListeners();
        }
    }
    
    [ContextMenu("Cleanup Audio Listeners")]
    public void CleanupAudioListeners()
    {
        Debug.Log("🧹 Starting Audio Listener cleanup...");
        
        // Find all AudioListeners in the scene
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        
        if (audioListeners.Length == 0)
        {
            Debug.Log("✅ No AudioListeners found in scene");
            return;
        }
        
        Debug.Log($"📻 Found {audioListeners.Length} AudioListener(s) in scene");
        
        // If only one AudioListener, we're good
        if (audioListeners.Length == 1)
        {
            Debug.Log($"✅ Scene has exactly one AudioListener on: {audioListeners[0].gameObject.name}");
            return;
        }
        
        // Multiple AudioListeners detected - need to clean up
        Debug.LogWarning($"⚠️ Multiple AudioListeners detected! Cleaning up...");
        
        // Find the main camera's AudioListener (preferred)
        AudioListener mainCameraListener = null;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraListener = mainCamera.GetComponent<AudioListener>();
        }
        
        // If no main camera AudioListener, use the first one
        if (mainCameraListener == null && audioListeners.Length > 0)
        {
            mainCameraListener = audioListeners[0];
            Debug.Log($"🎯 Using first AudioListener as primary: {mainCameraListener.gameObject.name}");
        }
        
        // Disable all other AudioListeners
        int disabledCount = 0;
        foreach (var listener in audioListeners)
        {
            if (listener != mainCameraListener)
            {
                listener.enabled = false;
                disabledCount++;
                
                if (logCleanupDetails)
                {
                    Debug.Log($"🔇 Disabled AudioListener on: {listener.gameObject.name}");
                }
            }
        }
        
        Debug.Log($"✅ Cleanup complete! Disabled {disabledCount} duplicate AudioListener(s)");
        Debug.Log($"🎯 Primary AudioListener: {mainCameraListener?.gameObject.name ?? "None"}");
        
        // Verify cleanup
        AudioListener[] remainingListeners = FindObjectsOfType<AudioListener>();
        int enabledCount = 0;
        foreach (var listener in remainingListeners)
        {
            if (listener.enabled) enabledCount++;
        }
        
        Debug.Log($"📊 Final state: {enabledCount} enabled AudioListener(s) out of {remainingListeners.Length} total");
    }
    
    [ContextMenu("Force Single Audio Listener")]
    public void ForceSingleAudioListener()
    {
        Debug.Log("🔧 Force cleanup - ensuring only one AudioListener...");
        
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        
        if (audioListeners.Length == 0)
        {
            Debug.Log("⚠️ No AudioListeners found - creating one on main camera");
            if (Camera.main != null)
            {
                Camera.main.gameObject.AddComponent<AudioListener>();
                Debug.Log("✅ Created AudioListener on main camera");
            }
            return;
        }
        
        // Keep only the first one, disable all others
        for (int i = 1; i < audioListeners.Length; i++)
        {
            DestroyImmediate(audioListeners[i]);
        }
        
        Debug.Log($"✅ Forced cleanup: Kept 1 AudioListener, removed {audioListeners.Length - 1}");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneAudioCleanup))]
public class SceneAudioCleanupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SceneAudioCleanup cleanup = (SceneAudioCleanup)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Cleanup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Cleanup Audio Listeners"))
        {
            cleanup.CleanupAudioListeners();
        }
        
        if (GUILayout.Button("Force Single Audio Listener"))
        {
            cleanup.ForceSingleAudioListener();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This script helps resolve Audio Listener conflicts in your scenes. " +
            "Unity only allows one active Audio Listener per scene.",
            MessageType.Info
        );
    }
}
#endif
