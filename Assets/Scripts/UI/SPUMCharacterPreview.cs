using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders a 2D/Spine/SPUM character prefab to a RenderTexture and displays it in a RawImage.
/// Keeps everything self-contained so we can preview characters in UI scenes.
/// </summary>
public class SPUMCharacterPreview : MonoBehaviour
{
	[SerializeField] private RawImage targetImage;
	[SerializeField] private int textureSize = 512;
	[SerializeField] private Color backgroundColor = new Color(0,0,0,0);

	private Camera _camera;
	private RenderTexture _rt;
	private GameObject _currentInstance;
	private GameObject _previewRoot;

	private void Ensure()
	{
		if (_previewRoot == null)
		{
			_previewRoot = new GameObject("SPUM_PreviewRoot");
			_previewRoot.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(_previewRoot);
		}
		if (_camera == null)
		{
			var camGo = new GameObject("SPUM_Preview_Camera");
			camGo.transform.SetParent(_previewRoot.transform);
			_camera = camGo.AddComponent<Camera>();
			_camera.orthographic = true;
			_camera.clearFlags = CameraClearFlags.SolidColor;
			_camera.backgroundColor = backgroundColor;
			_camera.cullingMask = ~0; // everything
		}
		if (_rt == null)
		{
			_rt = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
			_rt.Create();
			_camera.targetTexture = _rt;
			if (targetImage != null) targetImage.texture = _rt;
		}
	}

	public void SetTarget(RawImage rawImage)
	{
		targetImage = rawImage;
		if (_rt != null && targetImage != null)
			targetImage.texture = _rt;
	}

	public void ShowPrefab(GameObject prefab)
	{
		if (prefab == null) return;
		Ensure();
		if (_currentInstance != null)
		{
			DestroyImmediate(_currentInstance);
			_currentInstance = null;
		}
		_currentInstance = Instantiate(prefab, _previewRoot.transform);
		_currentInstance.transform.position = Vector3.zero;
		_currentInstance.transform.rotation = Quaternion.identity;
		_currentInstance.transform.localScale = Vector3.one;

		FitCameraToObject(_currentInstance);
		RenderPreviewOnce();
	}

	private void FitCameraToObject(GameObject go)
	{
		var bounds = CalculateRendererBounds(go);
		if (bounds.size == Vector3.zero) bounds = new Bounds(Vector3.zero, Vector3.one);
		_camera.transform.position = bounds.center + new Vector3(0, 0, -10f);
		float extent = Mathf.Max(bounds.extents.x, bounds.extents.y);
		_camera.orthographicSize = Mathf.Max(0.1f, extent * 1.2f);
	}

	private static Bounds CalculateRendererBounds(GameObject go)
	{
		var renderers = go.GetComponentsInChildren<Renderer>();
		if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
		var b = renderers[0].bounds;
		for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
		return b;
	}

	private void RenderPreviewOnce()
	{
		var prev = RenderTexture.active;
		RenderTexture.active = _rt;
		_camera.Render();
		RenderTexture.active = prev;
	}

	private void OnDestroy()
	{
		if (_rt != null)
		{
			_camera.targetTexture = null;
			_rt.Release();
		}
		if (_currentInstance != null) DestroyImmediate(_currentInstance);
		if (_previewRoot != null) DestroyImmediate(_previewRoot);
	}
}


