using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
	public class ReflectedListSection : GameDataListSection
	{
		private readonly string fieldName;
		private readonly string iconPath;
		private Vector2 menuScrollPosition;
		private Vector2 contentScrollPosition;
		private int selectedMenuIndex;
		private UnityEngine.Object selectedUnlistedObject;
		private string searchString = string.Empty;
		private int selectedCategoryIndex = 0;
		private string[] categoryOptions = new string[] { "All" };
		private List<ScriptableObject> filteredList = new List<ScriptableObject>();

		public override string MenuTitle { get; }

		public ReflectedListSection(string fieldName, string menuTitle, string iconPath)
		{
			this.fieldName = fieldName;
			this.iconPath = iconPath;
			this.MenuTitle = menuTitle;
			icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath) ?? AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Database UI PRO/Icons/default.png");
		}

		public override void OnGUI(float width, float height)
		{
			ScriptableObject db = GetWorkingDatabase();
			if (db == null)
				return;

			FieldInfo field = db.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				EditorGUILayout.HelpBox($"Field '{fieldName}' not found on {db.GetType().Name}", MessageType.Warning);
				return;
			}

			// Determine element type for arrays or List<T>
			Type elementType = field.FieldType.IsArray
				? field.FieldType.GetElementType()
				: (field.FieldType.IsGenericType ? field.FieldType.GetGenericArguments()[0] : null);
			// Normalize collection to array for display
			Array arr = field.FieldType.IsArray
				? (field.GetValue(db) as Array)
				: ToArray(field.GetValue(db), elementType);

			PopulateCategoryOptions(arr);

			GUILayout.BeginHorizontal();
			{
				DrawLeftMenu(arr, height);

				GUILayout.BeginVertical();
				{
					DrawSearchAndFilter();
					DrawActionButtons(arr, elementType, db, field);
					DrawSelectedContent();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		private void DrawLeftMenu(Array arr, float height)
		{
			GUIStyle leftMenuButton = new GUIStyle(EditorStyles.label)
			{
				fontSize = 12,
				alignment = TextAnchor.MiddleLeft,
				fixedHeight = 40,
				margin = new RectOffset(5, 5, 5, 5)
			};

			GUIStyle selectedLeftMenuButton = new GUIStyle(leftMenuButton)
			{
				normal = { background = MakeTex(600, 1, new Color(0.24f, 0.49f, 0.90f)), textColor = Color.white }
			};

			FilterData(arr);

			GUILayout.BeginVertical(GUILayout.Width(220), GUILayout.Height(height));
			{
				menuScrollPosition = GUILayout.BeginScrollView(menuScrollPosition);
				{
					if (filteredList != null && filteredList.Count > 0)
					{
						for (int i = 0; i < filteredList.Count; ++i)
						{
							ScriptableObject item = filteredList[i];
							Texture2D itemIcon = GetItemIcon(item) ?? (icon as Texture2D);
							GUIContent content = new GUIContent($"  ID: {GetItemId(item)}\n  {item.name}", itemIcon);
							if (GUILayout.Button(content, i == selectedMenuIndex ? selectedLeftMenuButton : leftMenuButton))
							{
								selectedMenuIndex = i;
								GUI.FocusControl(null);
							}
						}
					}
					else
					{
						GUILayout.Label("No items found.");
					}
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		private void DrawSearchAndFilter()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				GUILayout.Label("Search:", GUILayout.Width(50));
				GUI.SetNextControlName("SearchField");
				searchString = GUILayout.TextField(searchString, "ToolbarSeachTextField", GUILayout.Width(200));
				if (GUILayout.Button("", "ToolbarSeachCancelButton"))
				{
					searchString = string.Empty;
					GUI.FocusControl(null);
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Category:", GUILayout.Width(70));
				selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categoryOptions, EditorStyles.toolbarPopup, GUILayout.Width(150));
			}
			GUILayout.EndHorizontal();
		}

		private void DrawActionButtons(Array arr, Type elementType, ScriptableObject db, FieldInfo field)
		{
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(new GUIContent(" Create", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(30)))
				{
					CreateNewAsset(elementType, db, field, arr);
				}

				if (typeof(ScriptableObject).IsAssignableFrom(elementType))
				{
					selectedUnlistedObject = EditorGUILayout.ObjectField(selectedUnlistedObject, elementType, false, GUILayout.Height(30));
					if (selectedUnlistedObject != null)
					{
						if (GUILayout.Button(new GUIContent(" Add", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(30)))
						{
							AddToCollection(db, field, elementType, selectedUnlistedObject as ScriptableObject);
							selectedUnlistedObject = null;
						}
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawSelectedContent()
		{
			GUILayout.BeginVertical("box");
			{
				contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition);
				if (filteredList.Count > 0)
				{
					if (selectedMenuIndex < 0)
						selectedMenuIndex = 0;
					Editor editor = Editor.CreateEditor(filteredList[selectedMenuIndex]);
					editor.OnInspectorGUI();
				}
				else
				{
					GUILayout.Label("No item selected.");
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		private void FilterData(Array arr)
		{
			filteredList.Clear();
			if (arr == null) return;
			for (int i = 0; i < arr.Length; ++i)
			{
				ScriptableObject so = arr.GetValue(i) as ScriptableObject;
				if (so == null) continue;
				if (!string.IsNullOrEmpty(searchString) && so.name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) < 0)
					continue;
				if (selectedCategoryIndex > 0)
				{
					string selectedCategory = categoryOptions[selectedCategoryIndex];
					if (GetItemCategory(so) != selectedCategory)
						continue;
				}
				filteredList.Add(so);
			}
			if (filteredList != null && selectedMenuIndex >= filteredList.Count)
				selectedMenuIndex = filteredList.Count - 1;
		}

		private string GetItemCategory(ScriptableObject item)
		{
			PropertyInfo prop = item.GetType().GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (prop != null)
			{
				object val = prop.GetValue(item);
				if (val is string s && !string.IsNullOrEmpty(s))
					return s;
			}
			return "Undefined";
		}

		private void PopulateCategoryOptions(Array arr)
		{
			HashSet<string> categories = new HashSet<string>();
			categories.Add("All");
			if (arr != null)
			{
				for (int i = 0; i < arr.Length; ++i)
				{
					ScriptableObject so = arr.GetValue(i) as ScriptableObject;
					if (so == null) continue;
					categories.Add(GetItemCategory(so));
				}
			}
			categoryOptions = new string[categories.Count];
			categories.CopyTo(categoryOptions);
		}

		private Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		private Texture2D GetItemIcon(ScriptableObject item)
		{
			PropertyInfo prop = item.GetType().GetProperty("Icon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (prop != null)
			{
				object val = prop.GetValue(item);
				if (val is Sprite sprite)
					return sprite.texture;
				if (val is Texture2D tex)
					return tex;
			}
			return null;
		}

		private string GetItemId(ScriptableObject item)
		{
			PropertyInfo prop = item.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (prop != null)
			{
				object val = prop.GetValue(item);
				if (val is string s && !string.IsNullOrEmpty(s))
					return s;
			}
			return item.name;
		}

		private static ScriptableObject GetWorkingDatabase()
		{
			var type = System.Type.GetType("MultiplayerARPG.EditorGlobalData, Assembly-CSharp-Editor");
			if (type != null)
			{
				var prop = type.GetProperty("WorkingDatabase", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (prop != null)
					return prop.GetValue(null) as ScriptableObject;
			}
			var helperType = System.Type.GetType("MultiplayerARPG.GameDatabaseManagerEditor, Assembly-CSharp-Editor");
			if (helperType != null)
			{
				var method = helperType.GetMethod("GetCurrentWorkingDatabase", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
					return method.Invoke(null, null) as ScriptableObject;
			}
			return null;
		}

		private void AddToCollection(ScriptableObject db, FieldInfo field, Type elementType, ScriptableObject toAdd)
		{
			if (field.FieldType.IsArray)
			{
				Array existing = field.GetValue(db) as Array;
				List<ScriptableObject> agg = new List<ScriptableObject>();
				if (existing != null)
					for (int i = 0; i < existing.Length; ++i)
						agg.Add(existing.GetValue(i) as ScriptableObject);
				if (!agg.Contains(toAdd)) agg.Add(toAdd);
				Array newArr = Array.CreateInstance(elementType, agg.Count);
				for (int i = 0; i < agg.Count; ++i) newArr.SetValue(agg[i], i);
				field.SetValue(db, newArr);
			}
			else
			{
				var list = field.GetValue(db) as System.Collections.IList;
				if (list != null && !list.Contains(toAdd)) list.Add(toAdd);
			}
			EditorUtility.SetDirty(db);
		}

		private void CreateNewAsset(Type elementType, ScriptableObject db, FieldInfo field, Array arr)
		{
			string rootPath = "Assets/Database UI PRO/Generated/" + fieldName;
			string baseRoot = "Assets/Database UI PRO/Generated";
			if (!AssetDatabase.IsValidFolder("Assets/Database UI PRO"))
				AssetDatabase.CreateFolder("Assets", "Database UI PRO");
			if (!AssetDatabase.IsValidFolder(baseRoot))
				AssetDatabase.CreateFolder("Assets/Database UI PRO", "Generated");
			if (!AssetDatabase.IsValidFolder(rootPath))
				AssetDatabase.CreateFolder(baseRoot, fieldName);

			// If element type is a ScriptableObject, create an asset. Otherwise handle List<T>
			if (typeof(ScriptableObject).IsAssignableFrom(elementType))
			{
				ScriptableObject newAsset = ScriptableObject.CreateInstance(elementType) as ScriptableObject;
				newAsset.name = $"New {elementType.Name}";
				string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{rootPath}/{newAsset.name}.asset");
				AssetDatabase.CreateAsset(newAsset, assetPath);
				AssetDatabase.SaveAssets();
				AddToCollection(db, field, elementType, newAsset);
			}
			else
			{
				var list = field.GetValue(db) as System.Collections.IList;
				if (list != null)
				{
					object instance = Activator.CreateInstance(elementType);
					list.Add(instance);
					EditorUtility.SetDirty(db);
				}
			}
			EditorUtility.DisplayDialog("Created", $"Created and added new {elementType.Name}.", "OK");
		}

		private Array ToArray(object collection, Type elementType)
		{
			if (collection == null || elementType == null) return null;
			var list = collection as System.Collections.IList;
			if (list == null) return null;
			Array arr = Array.CreateInstance(elementType, list.Count);
			for (int i = 0; i < list.Count; ++i)
				arr.SetValue(list[i], i);
			return arr;
		}
	}
}


