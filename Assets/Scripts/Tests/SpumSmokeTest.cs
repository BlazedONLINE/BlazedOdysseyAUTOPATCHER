using UnityEngine;

public class SpumSmokeTest : MonoBehaviour
{
	[Header("SPUM Prefab Name (without .prefab)")]
	public string prefabName;

	void Start()
	{
		if (string.IsNullOrWhiteSpace(prefabName))
		{
			Debug.LogError("SpumSmokeTest: prefabName is empty. Set it to a prefab under Assets/SPUM/Resources/Units.");
			return;
		}

		var go = Resources.Load<GameObject>("Units/" + prefabName);
		if (go == null)
		{
			Debug.LogError("SPUM prefab not found under Resources/Units: " + prefabName);
			return;
		}

		Instantiate(go, Vector3.zero, Quaternion.identity);
		Debug.Log("SPUM prefab instantiated: " + prefabName);
	}
}


