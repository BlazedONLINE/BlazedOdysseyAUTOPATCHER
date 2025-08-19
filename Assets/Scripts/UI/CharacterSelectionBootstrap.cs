using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Boots our previous SimpleCharacterSelection screen in an empty scene.
/// Ensures Canvas and EventSystem exist, then adds SimpleCharacterSelection.
/// </summary>
public class CharacterSelectionBootstrap : MonoBehaviour
{
	private void Awake()
	{
		// Disabled to prevent character selection UI from appearing in gameplay scenes
		// EnsureCanvas();
		// EnsureEventSystem();
		// EnsureSelection();
		Destroy(this);
	}

	private void EnsureCanvas()
	{
		var canvas = FindAnyObjectByType<Canvas>();
		if (canvas != null) return;
		var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		canvas = go.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		var scaler = go.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
	}

	private void EnsureEventSystem()
	{
		if (FindAnyObjectByType<EventSystem>() != null) return;
		var es = new GameObject("EventSystem", typeof(EventSystem));
		es.AddComponent<StandaloneInputModule>();
	}

	private void EnsureSelection()
	{
		if (FindAnyObjectByType<SimpleCharacterSelection>() != null) return;
		var go = new GameObject("SimpleCharacterSelection", typeof(SimpleCharacterSelection));
		go.transform.SetParent(FindAnyObjectByType<Canvas>().transform, false);
	}
}


