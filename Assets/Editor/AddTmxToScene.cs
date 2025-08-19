using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class AddTmxToScene
{
	[MenuItem("BlazedOdyssey/World/Add Selected TMX To Scene")] 
	public static void AddSelectedTmx()
	{
		var obj = Selection.activeObject;
		if (obj == null)
		{
			EditorUtility.DisplayDialog("Add TMX", "Select a .tmx asset in the Project window first.", "OK");
			return;
		}
		string path = AssetDatabase.GetAssetPath(obj);
		if (string.IsNullOrEmpty(path) || !path.EndsWith(".tmx", System.StringComparison.OrdinalIgnoreCase))
		{
			EditorUtility.DisplayDialog("Add TMX", "Selected asset is not a .tmx map.", "OK");
			return;
		}

		var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		if (prefab == null)
		{
			EditorUtility.DisplayDialog("Add TMX", "Could not load the TMX prefab. Make sure SuperTiled2Unity imported it correctly.", "OK");
			return;
		}

		var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
		Undo.RegisterCreatedObjectUndo(instance, "Add TMX To Scene");
		instance.name = obj.name;
		instance.transform.position = Vector3.zero;

		// Set recommended sorting orders
		var tilemaps = instance.GetComponentsInChildren<TilemapRenderer>(true);
		foreach (var r in tilemaps)
		{
			string n = r.gameObject.name.ToLower();
			if (n.Contains("above")) r.sortingOrder = 10;
			else if (n.Contains("object")) r.sortingOrder = 2;
			else r.sortingOrder = 0; // ground/default
		}

		Selection.activeObject = instance;
		EditorGUIUtility.PingObject(instance);
	}
}


