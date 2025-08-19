using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
	public Transform target;
	public Vector3 offset = new Vector3(0f, 0f, -10f);
	public float smoothTime = 0.12f;
	public bool clampToMap = true;

	private Vector3 _velocity = Vector3.zero;

	private Camera _camera;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		if (_camera != null)
		{
			_camera.orthographic = true;
		}
	}

	private void LateUpdate()
	{
		if (target == null || _camera == null)
			return;

		Vector3 desired = target.position + offset;

		if (clampToMap && MapBounds.Instance != null)
		{
			Rect r = MapBounds.Instance.worldRect;
			float halfHeight = _camera.orthographicSize;
			float halfWidth = halfHeight * _camera.aspect;

			float minX = r.xMin + halfWidth;
			float maxX = r.xMax - halfWidth;
			float minY = r.yMin + halfHeight;
			float maxY = r.yMax - halfHeight;

			desired.x = Mathf.Clamp(desired.x, minX, maxX);
			desired.y = Mathf.Clamp(desired.y, minY, maxY);
		}

		Vector3 current = transform.position;
		Vector3 next = Vector3.SmoothDamp(current, desired, ref _velocity, smoothTime);
		transform.position = next;
	}
}


