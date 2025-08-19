using UnityEditor;
using UnityEngine;
using System.IO;
using BlazedOdyssey.Database;
using BlazedOdyssey.Database.Data;

namespace BlazedOdyssey.Database.Editor
{
	public static class MonsterDatabaseTools
	{
		[MenuItem("BlazedOdyssey/Database/Ensure Monster 'mob_horse'")]
		public static void EnsureMobHorse()
		{
			// 1) Find or create DatabaseAsset
			var db = FindOrCreateDatabaseAsset();
			if (db == null)
			{
				EditorUtility.DisplayDialog("Monster DB", "Failed to find or create DatabaseAsset.", "OK");
				return;
			}

			// 2) Find or create MonsterCharacterDB asset
			var monster = FindOrCreateMonsterAsset("mob_horse", "Horse");
			if (monster == null)
			{
				EditorUtility.DisplayDialog("Monster DB", "Failed to create 'mob_horse' asset.", "OK");
				return;
			}

			// 3) Add to database if missing
			if (!db.monsterCharacters.Contains(monster))
			{
				Undo.RecordObject(db, "Add Monster to Database");
				db.monsterCharacters.Add(monster);
				EditorUtility.SetDirty(db);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Monster DB", "Ensured monster 'mob_horse' is present in DatabaseAsset.", "OK");
		}

		private static DatabaseAsset FindOrCreateDatabaseAsset()
		{
			var guids = AssetDatabase.FindAssets("t:BlazedOdyssey.Database.DatabaseAsset");
			if (guids != null && guids.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				return AssetDatabase.LoadAssetAtPath<DatabaseAsset>(path);
			}
			// Create default at Assets/BlazedOdyssey/Database/GameDatabase.asset
			var dir = "Assets/BlazedOdyssey/Database";
			if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
			if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Database");
			var asset = ScriptableObject.CreateInstance<DatabaseAsset>();
			var assetPath = Path.Combine(dir, "GameDatabase.asset").Replace('\\', '/');
			AssetDatabase.CreateAsset(asset, assetPath);
			return asset;
		}

		private static MonsterCharacterDB FindOrCreateMonsterAsset(string id, string displayName)
		{
			// Look for existing MonsterCharacterDB with matching id in project
			var guids = AssetDatabase.FindAssets("t:BlazedOdyssey.Database.Data.MonsterCharacterDB");
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				var mc = AssetDatabase.LoadAssetAtPath<MonsterCharacterDB>(path);
				if (mc != null && mc.id == id) return mc;
			}
			// Create new one under Assets/BlazedOdyssey/Database/Data/Monsters/
			var dir = "Assets/BlazedOdyssey/Database/Data/Monsters";
			if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Database/Data")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Database", "Data");
			if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Database/Data", "Monsters");
			var asset = ScriptableObject.CreateInstance<MonsterCharacterDB>();
			asset.id = id;
			asset.displayName = displayName;
			// Try to assign a portrait sprite from Resources (horse idle sheet)
			var sprites = Resources.LoadAll<Sprite>("Monsters/Monsters and animals/Horse_unsaddled_idle (64x64)");
			if (sprites != null && sprites.Length > 0) asset.portrait = sprites[0];
			var assetPath = Path.Combine(dir, id + ".asset").Replace('\\', '/');
			AssetDatabase.CreateAsset(asset, assetPath);
			return asset;
		}
	}
}
