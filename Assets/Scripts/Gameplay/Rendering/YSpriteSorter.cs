using UnityEngine;

public class YSpriteSorter : MonoBehaviour
{
	public int baseOrder = 10000;
	public int multiplier = 100;
	private SpriteRenderer _sr;

	private void Awake()
	{
		_sr = GetComponent<SpriteRenderer>();
		if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		if (_sr == null) return;
		_sr.sortingOrder = baseOrder - Mathf.RoundToInt(transform.position.y * multiplier);
	}
}


