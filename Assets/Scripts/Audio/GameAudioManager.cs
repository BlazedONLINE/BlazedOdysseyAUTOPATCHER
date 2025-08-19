using UnityEngine;

/// <summary>
/// Centralized audio manager with master/music/ambience/sfx mixer levels and lazy audio sources.
/// Provides simple methods used from SettingsMenu.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
	public static GameAudioManager Instance { get; private set; }

	[SerializeField] private AudioSource _music;
	[SerializeField] private AudioSource _ambience;
	[SerializeField] private AudioSource _sfx;

	private float _master = 1f;
	private float _musicVol = 0.6f;
	private float _ambienceVol = 0.6f;
	private float _sfxVol = 0.8f;

	[Header("Music Playlist")]
	[SerializeField] private string _musicResourcesFolder = "Music"; // Assets/Resources/Music
	[SerializeField] private bool _autoStartPlaylist = true;
	[SerializeField] private bool _shuffle = true;
	[SerializeField] private float _crossfadeSeconds = 0.75f;
	private AudioClip[] _playlist = new AudioClip[0];
	private int _playlistIndex = -1;
	private bool _playlistActive;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
		EnsureSources();
	}

	private void Start()
	{
		if (_autoStartPlaylist)
		{
			StartDefaultPlaylist();
		}
	}

	private void EnsureSources()
	{
		if (_music == null)
		{
			_music = new GameObject("Music").AddComponent<AudioSource>();
			_music.transform.SetParent(transform, false);
			_music.loop = false; _music.playOnAwake = false;
		}
		if (_ambience == null)
		{
			_ambience = new GameObject("Ambience").AddComponent<AudioSource>();
			_ambience.transform.SetParent(transform, false);
			_ambience.loop = true; _ambience.playOnAwake = true;
		}
		if (_sfx == null)
		{
			_sfx = new GameObject("SFX").AddComponent<AudioSource>();
			_sfx.transform.SetParent(transform, false);
			_sfx.loop = false; _sfx.playOnAwake = false;
		}
		ApplyVolumes();
	}

	private void ApplyVolumes()
	{
		_music.volume = _master * _musicVol;
		_ambience.volume = _master * _ambienceVol;
		_sfx.volume = _master * _sfxVol;
	}

	public void SetMasterVolume(float v){ _master = Mathf.Clamp01(v); ApplyVolumes(); }
	public void SetMusicVolume(float v){ _musicVol = Mathf.Clamp01(v); ApplyVolumes(); }
	public void SetAmbienceVolume(float v){ _ambienceVol = Mathf.Clamp01(v); ApplyVolumes(); }
	public void SetSfxVolume(float v){ _sfxVol = Mathf.Clamp01(v); ApplyVolumes(); }

	// Getter methods for settings UI
	public float GetMasterVolume() => _master;
	public float GetMusicVolume() => _musicVol;
	public float GetAmbienceVolume() => _ambienceVol;
	public float GetSfxVolume() => _sfxVol;

	// Simple helpers to play clips
	public void PlaySfx(AudioClip clip){ if (clip != null) _sfx.PlayOneShot(clip, _sfx.volume); }
	public void SetMusic(AudioClip clip){ if (_music.clip != clip){ _music.clip = clip; if (clip != null) _music.Play(); }}
	public void SetAmbience(AudioClip clip){ if (_ambience.clip != clip){ _ambience.clip = clip; if (clip != null) _ambience.Play(); }}

	// ===== Playlist =====
	public void StartDefaultPlaylist()
	{
		LoadPlaylistFromResources();
		if (_playlist.Length == 0) return;
		_playlistActive = true;
		_playlistIndex = -1;
		NextTrack();
	}

	public void NextTrack()
	{
		if (_playlist.Length == 0) return;
		if (_shuffle)
		{
			int next;
			do { next = Random.Range(0, _playlist.Length); } while (_playlist.Length > 1 && next == _playlistIndex);
			_playlistIndex = next;
		}
		else
		{
			_playlistIndex = (_playlistIndex + 1) % _playlist.Length;
		}
		var clip = _playlist[_playlistIndex];
		CrossfadeTo(clip);
	}

	private void LoadPlaylistFromResources()
	{
		_playlist = Resources.LoadAll<AudioClip>(_musicResourcesFolder);
	}

	private void Update()
	{
		if (_playlistActive && !_music.isPlaying && _music.clip != null)
		{
			// move to next when current ended
			NextTrack();
		}
	}

	private void CrossfadeTo(AudioClip clip)
	{
		if (clip == null) return;
		StopAllCoroutines();
		StartCoroutine(FadeToClip(clip));
	}

	private System.Collections.IEnumerator FadeToClip(AudioClip clip)
	{
		float startVol = _music.volume;
		float t = 0f;
		while (t < _crossfadeSeconds)
		{
			t += Time.unscaledDeltaTime;
			_music.volume = Mathf.Lerp(startVol, 0f, Mathf.Clamp01(t/_crossfadeSeconds));
			yield return null;
		}
		_music.Stop();
		_music.clip = clip;
		_music.volume = 0f;
		_music.Play();
		t = 0f;
		float target = _master * _musicVol;
		while (t < _crossfadeSeconds)
		{
			t += Time.unscaledDeltaTime;
			_music.volume = Mathf.Lerp(0f, target, Mathf.Clamp01(t/_crossfadeSeconds));
			yield return null;
		}
		_music.volume = target;
	}
}


