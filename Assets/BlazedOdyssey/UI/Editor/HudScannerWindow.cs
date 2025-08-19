using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;

namespace BlazedOdyssey.UI.Editor
{
    public class HudScannerWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _scanOpenScenes = true;
        private bool _scanPrefabs = false;
        private string _prefabRoot = "Assets";

        private readonly string[] _primaryHudTypes = { "BlazedUIBootstrap" };
        private readonly string[] _legacyHudTypes = { "MMOUISystem", "SimpleMMOUISystem", "GameUIBootstrap", "GameUIBootstrap_DISABLED" };
        private readonly string[] _hudElementTypes = { "HudMovable", "PlayerHUD", "InventoryUI", "ActionBarUI", "SettingsPanel" };

        private List<SceneEntry> _sceneResults = new();
        private List<PrefabEntry> _prefabResults = new();

        [MenuItem("BlazedOdyssey/UI/HUD Scanner")] 
        public static void ShowWindow()
        {
            var w = GetWindow<HudScannerWindow>(true, "HUD Scanner");
            w.minSize = new Vector2(720, 520);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("HUD Scanner", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);
            _scanOpenScenes = EditorGUILayout.ToggleLeft("Scan Open Scenes", _scanOpenScenes);
            using (new EditorGUILayout.HorizontalScope())
            {
                _scanPrefabs = EditorGUILayout.ToggleLeft("Scan Prefabs In Project (root)", _scanPrefabs);
                using (new EditorGUI.DisabledScope(!_scanPrefabs))
                {
                    _prefabRoot = EditorGUILayout.TextField(_prefabRoot);
                    if (GUILayout.Button("Pick", GUILayout.Width(60)))
                    {
                        var picked = EditorUtility.OpenFolderPanel("Pick prefab scan root", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(picked))
                        {
                            if (picked.StartsWith(Application.dataPath))
                                _prefabRoot = "Assets" + picked.Substring(Application.dataPath.Length);
                            else
                                EditorUtility.DisplayDialog("Invalid Folder", "Please pick a folder under Assets/", "OK");
                        }
                    }
                }
            }
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan", GUILayout.Height(26))) Scan();
                if (GUILayout.Button("Disable Legacy HUD In Open Scenes", GUILayout.Height(26))) DisableLegacyInOpenScenes();
                if (GUILayout.Button("Set Selected As Active HUD", GUILayout.Height(26))) SetSelectedActiveHud();
                if (GUILayout.Button("Save Active HUD → Prefab Variant", GUILayout.Height(26))) SaveActiveHudAsPrefab();
                if (GUILayout.Button("Cleanup & Reinstall HUD (One-Click)", GUILayout.Height(26))) CleanupAndReinstallHud();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Selected HUD (Play‑Safe)", GUILayout.Height(24))) SaveSelectedHudPlaySafe();
                if (GUILayout.Button("Remove HUD From Scene + Save", GUILayout.Height(24))) RemoveHudFromSceneAndSave();
            }

            EditorGUILayout.Space(8);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSceneResults();
            EditorGUILayout.Space(6);
            DrawPrefabResults();
            EditorGUILayout.EndScrollView();
        }

        private void Scan()
        {
            _sceneResults.Clear();
            _prefabResults.Clear();
            if (_scanOpenScenes) ScanOpenScenes();
            if (_scanPrefabs) ScanPrefabs();
        }

        private void ScanOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                var roots = scene.GetRootGameObjects();
                foreach (var go in roots)
                    CollectSceneEntriesRecursive(scene, go, go.name);
            }
        }

        private void CollectSceneEntriesRecursive(Scene scene, GameObject go, string path)
        {
            var comps = go.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue; // missing script
                var tn = c.GetType().Name;
                if (IsHudType(tn))
                {
                    _sceneResults.Add(new SceneEntry
                    {
                        sceneName = scene.name,
                        path = path,
                        typeName = tn,
                        active = go.activeInHierarchy,
                        go = go
                    });
                }
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                CollectSceneEntriesRecursive(scene, child, path + "/" + child.name);
            }
        }

        private void ScanPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { _prefabRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                bool hit = false;
                try
                {
                    CollectPrefabEntriesRecursive(root, path, root.name, ref hit);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private void CollectPrefabEntriesRecursive(GameObject go, string assetPath, string path, ref bool hit)
        {
            var comps = go.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var tn = c.GetType().Name;
                if (IsHudType(tn))
                {
                    _prefabResults.Add(new PrefabEntry
                    {
                        assetPath = assetPath,
                        path = path,
                        typeName = tn
                    });
                    hit = true;
                }
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                CollectPrefabEntriesRecursive(child, assetPath, path + "/" + child.name, ref hit);
            }
        }

        private bool IsHudType(string typeName)
        {
            return _primaryHudTypes.Contains(typeName) || _legacyHudTypes.Contains(typeName) || _hudElementTypes.Contains(typeName);
        }

        private void DrawSceneResults()
        {
            EditorGUILayout.LabelField("Open Scenes", EditorStyles.boldLabel);
            if (_sceneResults.Count == 0)
            {
                EditorGUILayout.HelpBox("No HUD components found in open scenes.", MessageType.Info);
                return;
            }
            foreach (var e in _sceneResults.OrderBy(e=>e.sceneName).ThenBy(e=>e.typeName))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var tag = _primaryHudTypes.Contains(e.typeName) ? "[PRIMARY]" : (_legacyHudTypes.Contains(e.typeName) ? "[LEGACY]" : "[ELEM]");
                    EditorGUILayout.LabelField($"{e.sceneName}  {tag}  {e.typeName}  —  {e.path}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = e.go;
                        EditorGUIUtility.PingObject(e.go);
                    }
                    using (new EditorGUI.DisabledScope(e.go == null))
                    {
                        if (GUILayout.Button(e.active ? "Disable" : "Enable", GUILayout.Width(64)))
                        {
                            e.go.SetActive(!e.active);
                            e.active = e.go.activeInHierarchy;
                            if (!EditorApplication.isPlaying)
                                EditorSceneManager.MarkSceneDirty(e.go.scene);
                        }
                    }
                }
            }
        }

        private void DrawPrefabResults()
        {
            EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
            if (_prefabResults.Count == 0)
            {
                EditorGUILayout.HelpBox("No HUD components found in scanned prefabs.", MessageType.Info);
                return;
            }
            foreach (var e in _prefabResults.OrderBy(e=>e.assetPath).ThenBy(e=>e.typeName))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var tag = _primaryHudTypes.Contains(e.typeName) ? "[PRIMARY]" : (_legacyHudTypes.Contains(e.typeName) ? "[LEGACY]" : "[ELEM]");
                    EditorGUILayout.LabelField($"{tag}  {e.typeName}  —  {e.assetPath} :: {e.path}");
                    if (GUILayout.Button("Open", GUILayout.Width(60)))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(e.assetPath);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                        AssetDatabase.OpenAsset(obj);
                    }
                }
            }
        }

        private void DisableLegacyInOpenScenes()
        {
            var legacy = new HashSet<string>(_legacyHudTypes);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (var go in scene.GetRootGameObjects())
                    DisableLegacyRecursive(go, legacy);
            }
        }

        private void SetSelectedActiveHud()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Active HUD", "Select a HUD root (e.g., BlazedUI_Canvas) in the Scene Hierarchy first.", "OK");
                return;
            }
            // Enable selected root, disable other HUD roots in the same scenes
            var root = go.scene.IsValid() ? go.scene : SceneManager.GetActiveScene();
            foreach (var sceneGo in root.GetRootGameObjects())
            {
                if (sceneGo == null) continue;
                if (sceneGo == go)
                {
                    sceneGo.SetActive(true);
                    continue;
                }
                // Disable other BlazedUI_Canvas and legacy HUD roots
                if (sceneGo.name == "BlazedUI_Canvas" || ContainsHudComponent(sceneGo))
                {
                    sceneGo.SetActive(false);
                }
            }
            if (!EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();
            else
                Debug.Log("[HUD Scanner] SetActive ran in Play Mode. Scene save is disabled. Use 'Save Selected HUD (Play‑Safe)' to persist as prefab.");
        }

        private bool ContainsHudComponent(GameObject go)
        {
            var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in comps)
            {
                if (c == null) continue;
                var tn = c.GetType().Name;
                if (_primaryHudTypes.Contains(tn) || _legacyHudTypes.Contains(tn)) return true;
            }
            return false;
        }

        private void SaveActiveHudAsPrefab()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Save HUD", "Select the HUD root (BlazedUI_Canvas) in the Scene Hierarchy.", "OK");
                return;
            }
            var path = EditorUtility.SaveFilePanelInProject("Save HUD Prefab Variant", go.name + "_HUD", "prefab", "Pick save location under Assets.");
            if (string.IsNullOrEmpty(path)) return;
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.UserAction);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Save HUD", "Saved HUD prefab: \n" + path, "OK");
        }

        private void SaveSelectedHudPlaySafe()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Save HUD", "Select the HUD root you want to save.", "OK");
                return;
            }
            var defaultDir = "Assets/BlazedOdyssey/UI/Prefabs";
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/UI")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "UI");
            if (!AssetDatabase.IsValidFolder(defaultDir)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/UI", "Prefabs");
            var file = go.name + "_HUD.prefab";
            var path = Path.Combine(defaultDir, file).Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(go, path, out var success);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Save HUD", success ? ("Saved:\n" + path) : "Save failed", "OK");
        }

        private void RemoveHudFromSceneAndSave()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Remove & Save", "Select the HUD root to remove and save.", "OK");
                return;
            }
            SaveSelectedHudPlaySafe();
            if (EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(go);
            else
            {
                Undo.DestroyObjectImmediate(go);
                EditorSceneManager.MarkAllScenesDirty();
            }
        }

        private void CleanupAndReinstallHud()
        {
            // 1) Disable legacy HUDs first
            DisableLegacyInOpenScenes();

            // 2) Find all Blazed UI roots and keep only one
            var blazedRoots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root == null) continue;
                    if (HasComponentByName(root, "BlazedUIBootstrap"))
                        blazedRoots.Add(root);
                }
            }

            GameObject keep = null;
            if (blazedRoots.Count > 0)
            {
                keep = blazedRoots.FirstOrDefault(r => r.name == "BlazedUI_Canvas") ?? blazedRoots[0];
                foreach (var r in blazedRoots)
                {
                    if (r == keep) { r.SetActive(true); continue; }
                    r.SetActive(false);
                }
            }

            // 3) If none, create a fresh one and add BlazedUIBootstrap
            if (keep == null)
            {
                keep = new GameObject("BlazedUI_Canvas");
                var bubType = FindTypeByName("BlazedUIBootstrap");
                if (bubType != null)
                    keep.AddComponent(bubType);
                else
                    Debug.LogWarning("[HUD Scanner] Could not find type BlazedUIBootstrap. Ensure it compiles without errors.");
            }

            // 4) Ensure single EventSystem exists
            EnsureSingleEventSystem();

            // 5) Ensure single AudioListener (prefer MainCamera)
            EnsureSingleAudioListener();

            // 6) Ensure chat adapter exists (optional)
            EnsureComponentByName(keep, "UltimateChatAdapter");

            if (!EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();
            else
                SaveSelectedHudPlaySafe();
            Selection.activeObject = keep;
            EditorGUIUtility.PingObject(keep);
            Debug.Log("[HUD Scanner] Cleanup & Reinstall complete. Active HUD: " + keep.name);
        }

        private static void EnsureSingleEventSystem()
        {
            var existing = GameObject.FindObjectsByType<EventSystem>(FindObjectsSortMode.InstanceID);
            if (existing != null && existing.Length > 0) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureSingleAudioListener()
        {
            var listeners = GameObject.FindObjectsByType<AudioListener>(FindObjectsSortMode.InstanceID);
            if (listeners.Length == 0)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    var go = new GameObject("Main Camera");
                    var c = go.AddComponent<Camera>();
                    c.tag = "MainCamera";
                    go.AddComponent<AudioListener>();
                }
                return;
            }
            if (listeners.Length <= 1) return;
            var mainCam = Camera.main;
            AudioListener keep = null;
            if (mainCam != null) keep = mainCam.GetComponent<AudioListener>();
            if (keep == null) keep = listeners[0];
            foreach (var l in listeners)
            {
                if (l == keep) continue;
                l.enabled = false;
            }
        }

        private static void EnsureComponentByName(GameObject go, string typeName)
        {
            if (go == null) return;
            if (HasComponentByName(go, typeName)) return;
            var t = FindTypeByName(typeName);
            if (t != null) go.AddComponent(t);
        }

        private static bool HasComponentByName(GameObject go, string typeName)
        {
            var comps = go.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == typeName) return true;
            }
            return false;
        }

        private static Type FindTypeByName(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName); }
                catch { }
                if (t != null) return t;
            }
            return null;
        }

        private void DisableLegacyRecursive(GameObject go, HashSet<string> legacy)
        {
            var comps = go.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var tn = c.GetType().Name;
                if (legacy.Contains(tn) && go.activeInHierarchy)
                {
                    go.SetActive(false);
                    if (!EditorApplication.isPlaying)
                        EditorSceneManager.MarkSceneDirty(go.scene);
                    Debug.Log($"[HUD Scanner] Disabled legacy HUD: {tn} on {go.name}");
                    break;
                }
            }
            for (int i = 0; i < go.transform.childCount; i++)
                DisableLegacyRecursive(go.transform.GetChild(i).gameObject, legacy);
        }

        private class SceneEntry
        {
            public string sceneName;
            public string path;
            public string typeName;
            public bool active;
            public GameObject go;
        }
        private class PrefabEntry
        {
            public string assetPath;
            public string path;
            public string typeName;
        }
    }
}


