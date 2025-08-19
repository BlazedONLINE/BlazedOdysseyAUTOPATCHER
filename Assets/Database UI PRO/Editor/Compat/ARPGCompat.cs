using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Editor-only portion of the compatibility layer.
// Runtime types are defined in Assets/Database UI PRO/Compat/ARPGCompatRuntime.cs

namespace MultiplayerARPG
{
	public static class EditorMenuConsts
	{
		public const string GAME_DATABASE_MENU = "Window/BlazedOdyssey/Game Database";
		public const int GAME_DATABASE_ORDER = 2000;
	}

	public static class GameDataCreatorEditor
	{
		public static void CreateNewEditor(object database, Type type, string fieldName, object existingArray)
		{
			string rootPath = "Assets/Database UI PRO/Generated";
			if (!AssetDatabase.IsValidFolder("Assets/Database UI PRO"))
				AssetDatabase.CreateFolder("Assets", "Database UI PRO");
			if (!AssetDatabase.IsValidFolder(rootPath))
				AssetDatabase.CreateFolder("Assets/Database UI PRO", "Generated");

			ScriptableObject newAsset = ScriptableObject.CreateInstance(type);
			newAsset.name = $"New {type.Name}";
			string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{rootPath}/{newAsset.name}.asset");
			AssetDatabase.CreateAsset(newAsset, assetPath);
			AssetDatabase.SaveAssets();

			// Append to array field on the database via reflection
			FieldInfo field = database.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.LogError($"Field '{fieldName}' not found on database type {database.GetType().Name}");
				return;
			}
			Array current = field.GetValue(database) as Array;
			int newLen = (current?.Length ?? 0) + 1;
			Array resized = Array.CreateInstance(type, newLen);
			if (current != null && current.Length > 0)
				Array.Copy(current, resized, current.Length);
			resized.SetValue(newAsset, newLen - 1);
			field.SetValue(database, resized);
			EditorUtility.SetDirty((UnityEngine.Object)database);
			EditorUtility.DisplayDialog("Created", $"Created and added new {type.Name}.", "OK");
		}
	}
}



