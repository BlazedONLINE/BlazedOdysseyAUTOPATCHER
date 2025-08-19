#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BlazedOdyssey.Database;
using BlazedOdyssey.Database.Data;

public class DatabaseWindow : EditorWindow
{
    private DatabaseAsset db;
    private Vector2 scroll;

    [MenuItem("BlazedOdyssey/Database/Open Manager")] 
    public static void Open()
    {
        GetWindow<DatabaseWindow>("BO Database");
    }

    private void OnEnable()
    {
        if (db == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:DatabaseAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                db = AssetDatabase.LoadAssetAtPath<DatabaseAsset>(path);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        db = (DatabaseAsset)EditorGUILayout.ObjectField("Database Asset", db, typeof(DatabaseAsset), false);
        if (db == null)
        {
            if (GUILayout.Button("Create New Database"))
            {
                db = ScriptableObject.CreateInstance<DatabaseAsset>();
                AssetDatabase.CreateAsset(db, "Assets/BlazedOdyssey/Database/GameDatabase.asset");
                AssetDatabase.SaveAssets();
            }
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawItems();
        EditorGUILayout.Space(8);
        DrawCharacters();
        EditorGUILayout.EndScrollView();
    }

    private void DrawItems()
    {
        EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Item"))
        {
            Undo.RecordObject(db, "Add Item");
            db.items.Add(new ItemData{ id = NewId("ITEM_", db.items.Count+1), displayName = "New Item"});
            EditorUtility.SetDirty(db);
        }
        for (int i=0;i<db.items.Count;i++)
        {
            var it = db.items[i];
            EditorGUILayout.BeginVertical("box");
            it.id = EditorGUILayout.TextField("Id", it.id);
            it.displayName = EditorGUILayout.TextField("Name", it.displayName);
            it.description = EditorGUILayout.TextArea(it.description);
            it.icon = (Sprite)EditorGUILayout.ObjectField("Icon", it.icon, typeof(Sprite), false);
            it.maxStack = EditorGUILayout.IntField("Max Stack", it.maxStack);
            it.rarity = (Rarity)EditorGUILayout.EnumPopup("Rarity", it.rarity);
            it.baseValue = EditorGUILayout.IntField("Base Value", it.baseValue);
            db.items[i] = it;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Duplicate")) { Undo.RecordObject(db, "Dup Item"); db.items.Insert(i+1, it); EditorUtility.SetDirty(db); }
            if (GUILayout.Button("Delete")) { Undo.RecordObject(db, "Del Item"); db.items.RemoveAt(i); EditorUtility.SetDirty(db); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawCharacters()
    {
        EditorGUILayout.LabelField("Characters", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Character"))
        {
            Undo.RecordObject(db, "Add Character");
            var ch = ScriptableObject.CreateInstance<PlayerCharacterDB>();
            ch.id = NewId("CHAR_", db.characters.Count+1);
            ch.displayName = "New Character";
            string folder = "Assets/BlazedOdyssey/Database/Characters";
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Database")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Database");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Database", "Characters");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{ch.id}.asset");
            AssetDatabase.CreateAsset(ch, path);
            AssetDatabase.SaveAssets();
            db.characters.Add(ch);
            EditorUtility.SetDirty(db);
        }
        for (int i=0;i<db.characters.Count;i++)
        {
            var c = db.characters[i];
            EditorGUILayout.BeginVertical("box");
            c.id = EditorGUILayout.TextField("Id", c.id);
            c.displayName = EditorGUILayout.TextField("Name", c.displayName);
            c.className = EditorGUILayout.TextField("Class", c.className);
            c.portrait = (Sprite)EditorGUILayout.ObjectField("Portrait", c.portrait, typeof(Sprite), false);
            c.spumUnitId = EditorGUILayout.TextField("SPUM Unit Id", c.spumUnitId);
            db.characters[i] = c;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Duplicate")) { Undo.RecordObject(db, "Dup Char"); var dup=Object.Instantiate(c); string p=AssetDatabase.GetAssetPath(c); string np=AssetDatabase.GenerateUniqueAssetPath(p.Replace(".asset","_copy.asset")); AssetDatabase.CreateAsset(dup, np); AssetDatabase.SaveAssets(); db.characters.Insert(i+1, dup); EditorUtility.SetDirty(db); }
            if (GUILayout.Button("Delete")) { Undo.RecordObject(db, "Del Char"); var p=AssetDatabase.GetAssetPath(c); db.characters.RemoveAt(i); if(!string.IsNullOrEmpty(p)) AssetDatabase.DeleteAsset(p); EditorUtility.SetDirty(db); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    private string NewId(string prefix, int index) => prefix + index.ToString("D4");
}
#endif


