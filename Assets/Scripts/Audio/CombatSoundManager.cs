using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages combat sound effects with selectable options
/// Supports swing sounds and hit sounds with multiple variants
/// </summary>
public class CombatSoundManager : MonoBehaviour
{
    [Header("Swing Sounds")]
    [SerializeField] private AudioClip[] swingSounds;
    [SerializeField] private int selectedSwingSoundIndex = 0;
    
    [Header("Hit Sounds")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private int selectedHitSoundIndex = 0;
    
    [Header("Audio Settings")]
    [SerializeField] private float swingVolume = 0.7f;
    [SerializeField] private float hitVolume = 0.8f;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug & Testing")]
    [SerializeField] private bool showSoundSelector = true;
    [SerializeField] private KeyCode testSwingKey = KeyCode.F5;
    [SerializeField] private KeyCode testHitKey = KeyCode.F6;
    
    private static CombatSoundManager instance;
    public static CombatSoundManager Instance => instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get or create audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        
        LoadDefaultSounds();
    }
    
    void Update()
    {
        // Test sounds in editor/game
        if (Input.GetKeyDown(testSwingKey))
        {
            PlaySwingSound();
        }
        
        if (Input.GetKeyDown(testHitKey))
        {
            PlayHitSound();
        }
    }
    
    void LoadDefaultSounds()
    {
        // Load default swing sounds (will be replaced with downloaded sounds)
        var defaultSwingSounds = new List<AudioClip>();
        
        // Try to load from Resources folder
        AudioClip[] loadedSwingSounds = Resources.LoadAll<AudioClip>("Audio/Combat/Swings");
        if (loadedSwingSounds.Length > 0)
        {
            defaultSwingSounds.AddRange(loadedSwingSounds);
        }
        
        // Load default hit sounds
        var defaultHitSounds = new List<AudioClip>();
        AudioClip[] loadedHitSounds = Resources.LoadAll<AudioClip>("Audio/Combat/Hits");
        if (loadedHitSounds.Length > 0)
        {
            defaultHitSounds.AddRange(loadedHitSounds);
        }
        
        swingSounds = defaultSwingSounds.ToArray();
        hitSounds = defaultHitSounds.ToArray();
        
        Debug.Log($"🔊 Loaded {swingSounds.Length} swing sounds and {hitSounds.Length} hit sounds");
    }
    
    public void PlaySwingSound()
    {
        if (swingSounds.Length == 0) 
        {
            Debug.LogWarning("No swing sounds available!");
            return;
        }
        
        AudioClip clipToPlay = swingSounds[selectedSwingSoundIndex % swingSounds.Length];
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, swingVolume);
            Debug.Log($"🔊 Playing swing sound: {clipToPlay.name}");
        }
    }
    
    public void PlayHitSound()
    {
        if (hitSounds.Length == 0)
        {
            Debug.LogWarning("No hit sounds available!");
            return;
        }
        
        AudioClip clipToPlay = hitSounds[selectedHitSoundIndex % hitSounds.Length];
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, hitVolume);
            Debug.Log($"🔊 Playing hit sound: {clipToPlay.name}");
        }
    }
    
    public void PlayRandomSwingSound()
    {
        if (swingSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, swingSounds.Length);
        AudioClip clipToPlay = swingSounds[randomIndex];
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, swingVolume);
        }
    }
    
    public void PlayRandomHitSound()
    {
        if (hitSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, hitSounds.Length);
        AudioClip clipToPlay = hitSounds[randomIndex];
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, hitVolume);
        }
    }
    
    public void SetSwingSoundIndex(int index)
    {
        selectedSwingSoundIndex = Mathf.Clamp(index, 0, swingSounds.Length - 1);
        Debug.Log($"🔊 Selected swing sound {selectedSwingSoundIndex}: {(swingSounds.Length > 0 ? swingSounds[selectedSwingSoundIndex].name : "None")}");
    }
    
    public void SetHitSoundIndex(int index)
    {
        selectedHitSoundIndex = Mathf.Clamp(index, 0, hitSounds.Length - 1);
        Debug.Log($"🔊 Selected hit sound {selectedHitSoundIndex}: {(hitSounds.Length > 0 ? hitSounds[selectedHitSoundIndex].name : "None")}");
    }
    
    public int GetSwingSoundCount() => swingSounds.Length;
    public int GetHitSoundCount() => hitSounds.Length;
    public int GetSelectedSwingSoundIndex() => selectedSwingSoundIndex;
    public int GetSelectedHitSoundIndex() => selectedHitSoundIndex;
    
    public string GetCurrentSwingSoundName()
    {
        if (swingSounds.Length == 0 || selectedSwingSoundIndex >= swingSounds.Length)
            return "None";
        return swingSounds[selectedSwingSoundIndex].name;
    }
    
    public string GetCurrentHitSoundName()
    {
        if (hitSounds.Length == 0 || selectedHitSoundIndex >= hitSounds.Length)
            return "None";
        return hitSounds[selectedHitSoundIndex].name;
    }
    
    void OnGUI()
    {
        if (!showSoundSelector) return;
        
        GUILayout.BeginArea(new Rect(10, 400, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Combat Sound Selector", GUI.skin.label);
        GUILayout.Space(5);
        
        // Swing sounds
        GUILayout.Label($"Swing Sounds: {GetCurrentSwingSoundName()} ({selectedSwingSoundIndex + 1}/{swingSounds.Length})");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◄ Prev Swing") && swingSounds.Length > 0)
        {
            SetSwingSoundIndex((selectedSwingSoundIndex - 1 + swingSounds.Length) % swingSounds.Length);
        }
        if (GUILayout.Button("Next Swing ►") && swingSounds.Length > 0)
        {
            SetSwingSoundIndex((selectedSwingSoundIndex + 1) % swingSounds.Length);
        }
        if (GUILayout.Button("Test"))
        {
            PlaySwingSound();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Hit sounds
        GUILayout.Label($"Hit Sounds: {GetCurrentHitSoundName()} ({selectedHitSoundIndex + 1}/{hitSounds.Length})");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◄ Prev Hit") && hitSounds.Length > 0)
        {
            SetHitSoundIndex((selectedHitSoundIndex - 1 + hitSounds.Length) % hitSounds.Length);
        }
        if (GUILayout.Button("Next Hit ►") && hitSounds.Length > 0)
        {
            SetHitSoundIndex((selectedHitSoundIndex + 1) % hitSounds.Length);
        }
        if (GUILayout.Button("Test"))
        {
            PlayHitSound();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        GUILayout.Label($"Test Keys: {testSwingKey} (Swing), {testHitKey} (Hit)");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}