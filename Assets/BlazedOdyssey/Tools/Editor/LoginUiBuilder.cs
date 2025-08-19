using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;
using BlazedOdyssey.UI.Common;
using BlazedOdyssey.UI.Login;
using BlazedOdyssey.UI;

namespace BlazedOdyssey.Tools.Editor
{
    public static class LoginUiBuilder
    {
        private const string PrefabFolder = "Assets/BlazedOdyssey/Prefabs/UI";
        private const string LoginPrefabPath = PrefabFolder + "/LoginCanvas.prefab";
        private const string SettingsPrefabPath = PrefabFolder + "/SettingsPanel.prefab";
        // Explicit path provided by user for the login background image
        private const string BgPath = "Assets/BlazedOdyssey/UI/Art/login_bg_blazed_odyssey_1920x1080.png";

        [MenuItem("BlazedOdyssey/UI/Build Login Canvas (Scalable)")]
        public static void BuildLogin()
        {
            // Ensure folder
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Prefabs")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Prefabs", "UI");

            // Clean up old/duplicate instances in scene
            CleanupDuplicateCanvases("LoginCanvas");
            
            // Clean corrupt prefab if any
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LoginPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(LoginPrefabPath);
            }

            var root = new GameObject("LoginCanvas");
            var canvas = root.AddComponent<Canvas>(); 
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Above background (0) but below overlays (20+)
            root.AddComponent<GraphicRaycaster>();
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<InputShortcuts>();

            // Background image (robust lookup across Art/ART folders)
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.SetParent(canvas.transform, false);
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.GetComponent<Image>();
            // Prefer using the exact PNG as a Texture2D via RawImage (no sprite import required)
            AssetDatabase.Refresh();
            bool assigned = false;
            System.Func<Texture2D, bool> assignRaw = (tex) => {
                if (tex == null) return false;
                var existingRaw = bgGO.GetComponent<UnityEngine.UI.RawImage>();
                if (bgImg != null) Object.DestroyImmediate(bgImg, true);
                if (existingRaw == null) existingRaw = bgGO.AddComponent<UnityEngine.UI.RawImage>();
                existingRaw.texture = tex; existingRaw.color = Color.white;
                return true;
            };
            var exactTex = AssetDatabase.LoadAssetAtPath<Texture2D>(BgPath);
            if (assignRaw(exactTex)) { Debug.Log($"[LoginUiBuilder] Assigned background RawImage from exact path: {BgPath}"); assigned = true; }

            if (!assigned)
            {
                // Fallback: GUID lookup by name
                var guid = AssetDatabase.FindAssets("login_bg_blazed_odyssey_1920x1080 t:Texture2D");
                if (guid != null && guid.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid[0]);
                    var tex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (assignRaw(tex2)) { Debug.Log($"[LoginUiBuilder] Assigned background RawImage via GUID at: {path}"); assigned = true; }
                }
            }

            if (!assigned)
            {
                var tex = TryFindLoginBackgroundTexture();
                if (assignRaw(tex)) { Debug.Log($"[LoginUiBuilder] Assigned background RawImage from: {AssetDatabase.GetAssetPath(tex)}"); assigned = true; }
            }

            if (!assigned)
            {
                // Try sprite path
                var bgSprite = TryFindLoginBackgroundSprite();
                if (bgSprite != null)
                {
                    if (bgImg == null) bgImg = bgGO.AddComponent<Image>();
                    bgImg.sprite = bgSprite; bgImg.preserveAspect = false; bgImg.color = Color.white; bgImg.type = Image.Type.Simple;
                    Debug.Log($"[LoginUiBuilder] Assigned background sprite: {AssetDatabase.GetAssetPath(bgSprite)}");
                    assigned = true;
                }
            }

            if (!assigned)
            {
                Debug.LogError("[LoginUiBuilder] Could not find login background. Click OK to select the file manually.");
                var selected = EditorUtility.OpenFilePanel("Select Login Background", Application.dataPath, "png");
                if (!string.IsNullOrEmpty(selected))
                {
                    // Normalize slashes
                    var sel = selected.Replace('\\','/');
                    var dataPath = Application.dataPath.Replace('\\','/');
                    string projPath = null;
                    if (sel.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        projPath = "Assets" + sel.Substring(dataPath.Length);
                    }
                    else
                    {
                        // Copy into project at expected folder
                        var targetFolder = "Assets/BlazedOdyssey/UI/Art";
                        if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
                        if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/UI")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "UI");
                        if (!AssetDatabase.IsValidFolder(targetFolder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/UI", "Art");
                        var fileName = Path.GetFileName(sel);
                        var dest = Path.Combine(targetFolder, fileName).Replace('\\','/');
                        File.Copy(sel, dest, true);
                        AssetDatabase.ImportAsset(dest);
                        projPath = dest;
                    }
                    if (!string.IsNullOrEmpty(projPath))
                    {
                        var tex3 = AssetDatabase.LoadAssetAtPath<Texture2D>(projPath);
                        if (assignRaw(tex3))
                        {
                            Debug.Log($"[LoginUiBuilder] Assigned background via manual selection: {projPath}");
                            assigned = true;
                        }
                        else
                        {
                            Debug.LogError($"[LoginUiBuilder] Failed to load Texture2D at {projPath} after selection.");
                        }
                    }
                }
            }

            // UI sprites via generated fallbacks (no reliance on built-ins)
            var uiSprite = LoadUiSprite();
            var checkSprite = LoadCheckmarkSprite();

            // LogoBlock (movable stub)
            var logo = new GameObject("LogoBlock", typeof(RectTransform));
            var logoRT = logo.GetComponent<RectTransform>();
            logoRT.SetParent(canvas.transform, false);
            logoRT.anchorMin = new Vector2(0.5f, 1f); logoRT.anchorMax = new Vector2(0.5f, 1f); logoRT.pivot = new Vector2(0.5f, 1f);
            logoRT.sizeDelta = new Vector2(600, 140); logoRT.anchoredPosition = new Vector2(0, -140);
            var logoAdj = logo.AddComponent<HudAdjustable>(); logoAdj.WidgetId = "LogoBlock";

            // LoginPanel - moved down to be below background area
            var panel = new GameObject("LoginPanel", typeof(RectTransform), typeof(Image));
            var pRT = panel.GetComponent<RectTransform>(); pRT.SetParent(canvas.transform, false);
            pRT.anchorMin = new Vector2(0.5f, 0.5f); pRT.anchorMax = new Vector2(0.5f, 0.5f); pRT.pivot = new Vector2(0.5f, 0.5f);
            pRT.sizeDelta = new Vector2(520, 420);
            pRT.anchoredPosition = new Vector2(0, -100); // Move down 100 pixels
            var pImg = panel.GetComponent<Image>(); pImg.color = new Color(0,0,0,0.35f); pImg.sprite = uiSprite; pImg.type = Image.Type.Sliced;
            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter; vlg.spacing = 8; vlg.padding = new RectOffset(24,24,24,24);
            var fitter = panel.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var pAdj = panel.AddComponent<HudAdjustable>(); pAdj.WidgetId = "LoginPanel";

            // Username
            var userGO = CreateField(panel.transform, "Email / Username");
            var userInput = userGO.GetComponentInChildren<TMP_InputField>();
            // Password
            var passGO = CreateField(panel.transform, "Password", mask:true);
            var passInput = passGO.GetComponentInChildren<TMP_InputField>();

            // Remember Me toggle
            var toggleGO = new GameObject("RememberMe", typeof(RectTransform));
            var tRT = toggleGO.GetComponent<RectTransform>(); tRT.SetParent(panel.transform, false); tRT.sizeDelta = new Vector2(480, 26);
            var tToggle = toggleGO.AddComponent<Toggle>();
            var tBG = new GameObject("bg", typeof(RectTransform), typeof(Image));
            var tBGrt = tBG.GetComponent<RectTransform>(); tBGrt.SetParent(toggleGO.transform, false); tBGrt.anchorMin = new Vector2(0,0.5f); tBGrt.anchorMax = new Vector2(0,0.5f); tBGrt.pivot = new Vector2(0,0.5f); tBGrt.sizeDelta = new Vector2(18,18);
            var tBGImg = tBG.GetComponent<Image>(); tBGImg.sprite = uiSprite; tBGImg.type = Image.Type.Sliced; tBGImg.color = new Color(1,1,1,0.25f);
            var tCheck = new GameObject("check", typeof(RectTransform), typeof(Image)); var tCrt = tCheck.GetComponent<RectTransform>(); tCrt.SetParent(tBG.transform, false); tCrt.sizeDelta = new Vector2(18,18);
            var tCheckImg = tCheck.GetComponent<Image>(); tCheckImg.sprite = checkSprite; tCheckImg.color = Color.white;
            tToggle.targetGraphic = tBGImg; tToggle.graphic = tCheckImg; tToggle.isOn = false;
            var tLabelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var tLbl = tLabelGO.GetComponent<TextMeshProUGUI>(); tLbl.text = "Remember Me"; tLbl.fontSize = 20; tLbl.color = Color.white; UI.Common.UIThemeUtil.ApplyFontIfAvailable(tLbl); var tLrt = tLabelGO.GetComponent<RectTransform>();
            tLrt.SetParent(toggleGO.transform, false); tLrt.anchorMin = new Vector2(0,0.5f); tLrt.anchorMax = new Vector2(0,0.5f); tLrt.pivot = new Vector2(0,0.5f); tLrt.anchoredPosition = new Vector2(26,0); tLrt.sizeDelta = new Vector2(440, 24);

            // Buttons
            var loginBtn = CreateButton(panel.transform, "Login");
            var createBtn = CreateButton(panel.transform, "Create New Account");
            var settingsBtn = CreateButton(panel.transform, "Settings");
            var exitBtn = CreateButton(panel.transform, "Exit Game");

            // Signing-in overlay
            var overlay = new GameObject("SigningIn", typeof(RectTransform), typeof(Image));
            var oRT = overlay.GetComponent<RectTransform>(); oRT.SetParent(panel.transform, false); oRT.anchorMin = Vector2.zero; oRT.anchorMax = Vector2.one; oRT.offsetMin = Vector2.zero; oRT.offsetMax = Vector2.zero; overlay.GetComponent<Image>().color = new Color(0,0,0,0.45f); overlay.SetActive(false);
            var spin = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var sRT = spin.GetComponent<RectTransform>(); sRT.SetParent(overlay.transform, false); sRT.anchorMin = new Vector2(0.5f,0.5f); sRT.anchorMax = new Vector2(0.5f,0.5f); sRT.pivot = new Vector2(0.5f,0.5f); sRT.anchoredPosition = Vector2.zero; sRT.sizeDelta = new Vector2(300,40);
            var sTxt = spin.GetComponent<TextMeshProUGUI>(); sTxt.text = "Signing in…"; sTxt.fontSize = 28; sTxt.color = Color.white; sTxt.alignment = TextAlignmentOptions.Center; UI.Common.UIThemeUtil.ApplyFontIfAvailable(sTxt);

            // Wire controller
            var ctrl = panel.AddComponent<LoginController>();
            ctrl.GetType().GetField("usernameInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, userInput);
            ctrl.GetType().GetField("passwordInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, passInput);
            ctrl.GetType().GetField("rememberMeToggle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, tToggle);
            ctrl.GetType().GetField("loginButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, loginBtn.GetComponent<Button>());
            ctrl.GetType().GetField("createAccountButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, createBtn.GetComponent<Button>());
            ctrl.GetType().GetField("settingsButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, settingsBtn.GetComponent<Button>());
            ctrl.GetType().GetField("exitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, exitBtn.GetComponent<Button>());
            ctrl.GetType().GetField("signingInOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(ctrl, overlay);

            // Tab Navigation
            var tabNav = panel.AddComponent<TabNavigation>();
            // TabNavigation will auto-find input fields, but we can also manually add them in order
            tabNav.AddInputField(userInput);
            tabNav.AddInputField(passInput);
            tabNav.AddInputField(tToggle);
            tabNav.AddInputField(loginBtn.GetComponent<Button>());

            // Ensure EventSystem exists
            if (!Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, LoginPrefabPath, InteractionMode.UserAction);
            // Auto-create settings prefab if missing and assign to controller
            var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath);
            if (settingsPrefab == null)
            {
                settingsPrefab = SettingsUiBuilder.BuildSettings();
            }
            var ctrlSerialized = new SerializedObject(ctrl);
            ctrlSerialized.FindProperty("settingsPanelPrefab").objectReferenceValue = settingsPrefab;
            ctrlSerialized.ApplyModifiedPropertiesWithoutUndo();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        // ===== Helpers: robust sprite loaders (no GetBuiltinResource) =====
        private static Sprite LoadUiSprite()
        {
            return CreateSlicedSolidSprite(Color.white, 8);
        }

        private static Sprite LoadCheckmarkSprite()
        {
            return CreateCheckmarkSprite(Color.white, 16);
        }

        private static Sprite CreateSlicedSolidSprite(Color color, int size)
        {
            if (size < 8) size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels); tex.Apply();
            var border = new Vector4(4,4,4,4);
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static Sprite CreateCheckmarkSprite(Color color, int size)
        {
            if (size < 12) size = 12;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0,0,0,0);
            for (int x = 2; x < size/2; x++) { int y = x; tex.SetPixel(x, y, color); tex.SetPixel(x, y-1, color); }
            for (int x = size/2; x < size-2; x++) { int y = size - (x - size/2) - 3; tex.SetPixel(x, y, color); tex.SetPixel(x, y-1, color); }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f);
        }

        private static Sprite TryFindLoginBackgroundSprite()
        {
            // 1) Direct path
            var s = EnsureSpriteAtPath(BgPath);
            if (s != null) return s;
            // 2) Search known folders (Art, ART) and by name
            string[] queries = new[]
            {
                "t:Sprite login_bg_blazed_odyssey_1920x1080",
                "t:Sprite login_bg_blazed*",
            };
            foreach (var q in queries)
            {
                var guids = AssetDatabase.FindAssets(q);
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".png") && !path.EndsWith(".psd")) continue;
                    // Prefer paths that contain Art/ART folders
                    if (path.Contains("/Art/") || path.Contains("/ART/") || path.Contains("/UI/Art/") || path.Contains("/UI/ART/"))
                    {
                        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sp != null) return sp;
                    }
                }
            }
            // 3) Search our explicit Art locations for any promising sprite, prefer largest resolution
            {
                string[] artFolders = new[] {
                    "Assets/BlazedOdyssey/Art",
                    "Assets/BlazedOdyssey/ART",
                    "Assets/BlazedOdyssey/UI/Art",
                    "Assets/BlazedOdyssey/UI/ART"
                };
                Sprite bestSprite = null; int bestArea = -1;
                foreach (var folder in artFolders)
                {
                    if (!AssetDatabase.IsValidFolder(folder)) continue;
                    var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sp == null) continue;
                        int area = Mathf.RoundToInt(sp.rect.width * sp.rect.height);
                        if (area > bestArea) { bestArea = area; bestSprite = sp; }
                    }
                }
                if (bestSprite != null) return bestSprite;
            }
            // 4) As a final step, create a Sprite from the largest Texture2D found in those folders
            {
                string[] artFolders = new[] {
                    "Assets/BlazedOdyssey/Art",
                    "Assets/BlazedOdyssey/ART",
                    "Assets/BlazedOdyssey/UI/Art",
                    "Assets/BlazedOdyssey/UI/ART"
                };
                Texture2D bestTex = null; int bestArea = -1;
                foreach (var folder in artFolders)
                {
                    if (!AssetDatabase.IsValidFolder(folder)) continue;
                    var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        if (tex == null) continue;
                        int area = tex.width * tex.height;
                        if (area > bestArea) { bestArea = area; bestTex = tex; }
                    }
                }
                if (bestTex != null)
                {
                    return Sprite.Create(bestTex, new Rect(0,0,bestTex.width,bestTex.height), new Vector2(0.5f,0.5f), 100f);
                }
            }
            return null;
        }

        private static Texture2D TryFindLoginBackgroundTexture()
        {
            // Prefer explicit Art paths first (including UI/Art)
            string[] artFolders = new[] { "Assets/BlazedOdyssey/UI/Art", "Assets/BlazedOdyssey/UI/ART", "Assets/BlazedOdyssey/Art", "Assets/BlazedOdyssey/ART" };
            Texture2D best = null; int bestArea = -1;
            foreach (var folder in artFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                var gids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var g in gids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                    if (t == null) continue;
                    int area = t.width * t.height;
                    if (area > bestArea) { best = t; bestArea = area; }
                }
            }
            if (best != null) return best;

            // Next, try name-based search anywhere
            var guids = AssetDatabase.FindAssets("t:Texture2D login*");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null) return tex;
            }
            return null;
        }

        // Ensure the asset at path is imported as Sprite and return it
        private static Sprite EnsureSpriteAtPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null)
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
            // Fix importer settings to be Sprite if needed
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static GameObject CreateField(Transform parent, string placeholder, bool mask = false)
        {
            var go = new GameObject("Field_"+placeholder, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = new Vector2(480, 40);
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image)); var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(go.transform, false); bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero; bg.GetComponent<Image>().color = new Color(1,1,1,0.08f);
            var inputGO = new GameObject("Input", typeof(RectTransform), typeof(TMP_InputField)); var iRT = inputGO.GetComponent<RectTransform>(); iRT.SetParent(go.transform, false); iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one; iRT.offsetMin = new Vector2(10,8); iRT.offsetMax = new Vector2(-10,-8);
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); var tRT = textGO.GetComponent<RectTransform>(); tRT.SetParent(inputGO.transform, false); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero; var text = textGO.GetComponent<TextMeshProUGUI>(); text.fontSize = 20; text.alignment = TextAlignmentOptions.Left; text.enableWordWrapping = false; UI.Common.UIThemeUtil.ApplyFontIfAvailable(text);
            var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI)); var phRT = phGO.GetComponent<RectTransform>(); phRT.SetParent(inputGO.transform, false); phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one; phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero; var ph = phGO.GetComponent<TextMeshProUGUI>(); ph.text = placeholder; ph.fontSize = 20; ph.color = new Color(1,1,1,0.35f); UI.Common.UIThemeUtil.ApplyFontIfAvailable(ph);
            var field = inputGO.GetComponent<TMP_InputField>(); field.textViewport = iRT; field.textComponent = text; field.placeholder = ph; field.contentType = mask ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            return go;
        }

        static GameObject CreateButton(Transform parent, string label)
        {
            var go = new GameObject(label+"Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = new Vector2(480, 40);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.12f); var ui = LoadUiSprite(); img.sprite = ui; img.type = Image.Type.Sliced;
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var tRT = txtGO.GetComponent<RectTransform>(); tRT.SetParent(go.transform, false); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
            var tmp = txtGO.GetComponent<TextMeshProUGUI>(); tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = 22; UI.Common.UIThemeUtil.ApplyFontIfAvailable(tmp);
            return go;
        }
        
        /// <summary>
        /// Clean up old/duplicate canvas instances in the scene
        /// </summary>
        private static void CleanupDuplicateCanvases(string canvasName)
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            int removedCount = 0;
            
            foreach (var canvas in canvases)
            {
                if (canvas.gameObject.name == canvasName || canvas.gameObject.name.StartsWith(canvasName))
                {
                    Debug.Log($"🗑️ Removing duplicate/old canvas: {canvas.gameObject.name}");
                    Object.DestroyImmediate(canvas.gameObject);
                    removedCount++;
                }
            }
            
            if (removedCount > 0)
            {
                Debug.Log($"✅ Cleaned up {removedCount} old/duplicate {canvasName} instances");
            }
            else
            {
                Debug.Log($"ℹ️ No duplicate {canvasName} instances found to clean up");
            }
        }
    }
}


