using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class GameDataListSection
    {
        public abstract string MenuTitle { get; }
        public abstract void OnGUI(float width, float height);
        public Texture2D icon;
    }

    public class GameDataListSection<T> : GameDataListSection
        where T : ScriptableObject
    {
        protected Vector2 menuScrollPosition;
        protected Vector2 contentScrollPosition;
        protected int selectedMenuIndex;
        protected T selectedUnlistedObject;
        protected string fieldName;
        public string FieldName { get { return fieldName; } }
        protected string menuTitle;
        protected T editorData;
        protected Editor editor;
        public override string MenuTitle { get { return menuTitle; } }

        // Variables for search and filter
        protected string searchString = string.Empty;
        protected int selectedCategoryIndex = 0;
        protected string[] categoryOptions = new string[] { "All" };
        protected List<T> filteredList = new List<T>();

        // Icons
        protected Texture2D defaultIcon;
        protected Texture2D folderIcon;

        public GameDataListSection(string fieldName, string menuTitle, string iconPath)
        {
            this.fieldName = fieldName;
            this.menuTitle = menuTitle;

            // Load default icon
            defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Database UI PRO/Icons/default.png");
            // Load folder icon
            folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            icon = folderIcon ?? defaultIcon;
        }

        public override void OnGUI(float width, float height)
        {
            var db = GetWorkingDatabase();
            if (db == null)
                return;

            T[] arr = (T[])db.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(db);

            // Populate category options
            PopulateCategoryOptions(arr);

            // Main area
            GUILayout.BeginHorizontal();
            {
                // Left menu
                DrawLeftMenu(arr, height);

                // Content area
                GUILayout.BeginVertical();
                {
                    // Search bar and filter
                    DrawSearchAndFilter();

                    // Action buttons
                    DrawActionButtons(arr);

                    // Selected content
                    DrawSelectedContent();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawLeftMenu(T[] arr, float height)
        {
            // Prepare GUI styles
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

            // Filter data
            FilterData(arr);

            // Menu
            GUILayout.BeginVertical(GUILayout.Width(220), GUILayout.Height(height));
            {
                menuScrollPosition = GUILayout.BeginScrollView(menuScrollPosition);
                {
                    if (filteredList != null && filteredList.Count > 0)
                    {
                        for (int i = 0; i < filteredList.Count; ++i)
                        {
                            T item = filteredList[i];
                            Texture2D icon = GetItemIcon(item) ?? defaultIcon;

                            GUIContent content = new GUIContent($"  ID: {GetItemId(item)}\n  {item.name}", icon);

                            if (GUILayout.Button(content, i == selectedMenuIndex ? selectedLeftMenuButton : leftMenuButton))
                            {
                                selectedMenuIndex = i;
                                GUI.FocusControl(null); // Remove focus from text fields
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
                // Search bar
                GUILayout.Label("Search:", GUILayout.Width(50));
                GUI.SetNextControlName("SearchField");
                searchString = GUILayout.TextField(searchString, "ToolbarSeachTextField", GUILayout.Width(200));
                if (GUILayout.Button("", "ToolbarSeachCancelButton"))
                {
                    searchString = string.Empty;
                    GUI.FocusControl(null);
                }

                // Flexible space
                GUILayout.FlexibleSpace();

                // Category filter
                GUILayout.Label("Category:", GUILayout.Width(70));
                selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, categoryOptions, EditorStyles.toolbarPopup, GUILayout.Width(150));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawActionButtons(T[] arr)
        {
            GUILayout.BeginHorizontal();
            {
                // Create button
                if (GUILayout.Button(new GUIContent(" Create", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(30)))
                {
                    Create(arr);
                }

                // Add button
                selectedUnlistedObject = EditorGUILayout.ObjectField(selectedUnlistedObject, typeof(T), false, GUILayout.Height(30)) as T;
                if (selectedUnlistedObject != null)
                {
                    if (GUILayout.Button(new GUIContent(" Add", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(30)))
                    {
                        Add(arr);
                        selectedUnlistedObject = null;
                    }
                }

                // Duplicate button
                GUI.enabled = filteredList.Count > 0;
                if (GUILayout.Button(new GUIContent(" Duplicate", EditorGUIUtility.IconContent("d_TreeEditor.Duplicate").image), GUILayout.Height(30)))
                {
                    Duplicate(arr);
                }

                // Delete button
                if (GUILayout.Button(new GUIContent(" Delete", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image), GUILayout.Height(30)))
                {
                    Delete(arr);
                }
                GUI.enabled = true;
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
                    if (editorData != filteredList[selectedMenuIndex])
                    {
                        editor = Editor.CreateEditor(filteredList[selectedMenuIndex]);
                        editorData = filteredList[selectedMenuIndex];
                    }
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

        private void FilterData(T[] arr)
        {
            filteredList = new List<T>(arr);

            if (!string.IsNullOrEmpty(searchString))
            {
                filteredList = filteredList.FindAll(item => item.name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (selectedCategoryIndex > 0)
            {
                string selectedCategory = categoryOptions[selectedCategoryIndex];
                filteredList = filteredList.FindAll(item => GetItemCategory(item) == selectedCategory);
            }

            if (filteredList != null && selectedMenuIndex >= filteredList.Count)
                selectedMenuIndex = filteredList.Count - 1;
        }

        protected virtual string GetItemCategory(T item)
        {
            var prop = item.GetType().GetProperty("Category", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                object val = prop.GetValue(item);
                if (val is string s && !string.IsNullOrEmpty(s))
                    return s;
            }
            return "Undefined";
        }

        private void PopulateCategoryOptions(T[] arr)
        {
            HashSet<string> categories = new HashSet<string>();
            categories.Add("All"); // Add "All" option

            foreach (var item in arr)
            {
                categories.Add(GetItemCategory(item));
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

        protected virtual Texture2D GetItemIcon(T item)
        {
            var prop = item.GetType().GetProperty("Icon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

        protected virtual string GetItemId(T item)
        {
            var prop = item.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                object val = prop.GetValue(item);
                if (val is string s && !string.IsNullOrEmpty(s))
                    return s;
            }
            return item.name;
        }


        protected virtual void Add(T[] arr)
        {
            if (selectedUnlistedObject == null)
                return;
            List<T> appending = new List<T>(arr);
            if (appending.Contains(selectedUnlistedObject))
            {
                EditorUtility.DisplayDialog("Warning", "The item is already in the list.", "OK");
                return;
            }
            appending.Add(selectedUnlistedObject);
            T[] newArr = appending.ToArray();
            var db = GetWorkingDatabase();
            db.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SetValue(db, newArr);
            EditorUtility.SetDirty(db);
            selectedUnlistedObject = null;
            EditorUtility.DisplayDialog("Success", "Item added successfully.", "OK");
        }

        protected virtual void Create(T[] arr)
        {
            var db = GetWorkingDatabase();
            GameDataCreatorEditor.CreateNewEditor(db, typeof(T), fieldName, arr);
        }

        protected virtual void Duplicate(T[] arr)
        {
            if (filteredList.Count == 0)
                return;

            T itemToDuplicate = filteredList[selectedMenuIndex];
            string path = AssetDatabase.GetAssetPath(itemToDuplicate);
            List<string> splitedPath = new List<string>(path.Split('.'));
            string extension = splitedPath[splitedPath.Count - 1];
            splitedPath.RemoveAt(splitedPath.Count - 1);
            string savePath = string.Join(".", splitedPath);
            int retries = 20;
            string savedPath = string.Empty;
            for (var i = 0; i < retries; i++)
            {
                savedPath = $"{savePath}{i}.{extension}";
                if (AssetDatabase.CopyAsset(path, savedPath))
                    break;
                if (i == retries - 1)
                {
                    Debug.LogError($"Cannot duplicate {path} ({retries} retries)");
                    return;
                }
            }
            List<T> appending = new List<T>(arr);
            appending.Add(AssetDatabase.LoadAssetAtPath<T>(savedPath));
            T[] newArr = appending.ToArray();
            var db = GetWorkingDatabase();
            db.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SetValue(db, newArr);
            EditorUtility.SetDirty(db);
            EditorUtility.DisplayDialog("Success", "Item duplicated successfully.", "OK");
        }

        protected virtual void Delete(T[] arr)
        {
            if (filteredList.Count == 0)
                return;

            T itemToDelete = filteredList[selectedMenuIndex];

            if (EditorUtility.DisplayDialog("Warning", "The selected data file will be deleted, do you want to proceed?", "Yes", "No"))
            {
                if (AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(itemToDelete)))
                {
                    List<T> list = new List<T>(arr);
                    list.Remove(itemToDelete);
                    T[] newArr = list.ToArray();
                    var db = GetWorkingDatabase();
                    db.GetType()
                        .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .SetValue(db, newArr);
                    EditorUtility.SetDirty(db);
                    EditorUtility.DisplayDialog("Success", "Item deleted successfully.", "OK");
                }
            }
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
    }
}
