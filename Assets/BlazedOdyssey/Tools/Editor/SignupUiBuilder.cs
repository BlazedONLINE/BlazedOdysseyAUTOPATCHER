using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlazedOdyssey.UI.Common;
using BlazedOdyssey.UI.Login;

namespace BlazedOdyssey.Tools.Editor
{
    public static class SignupUiBuilder
    {
        private const string PrefabFolder = "Assets/BlazedOdyssey/Prefabs/UI";
        private const string SignupPrefabPath = PrefabFolder + "/SignupCanvas.prefab";
        private const string BgPath = "Assets/BlazedOdyssey/UI/Art/login_bg_blazed_odyssey_1920x1080.png";

        [MenuItem("BlazedOdyssey/UI/Build Signup Canvas (Scalable)")]
        public static void BuildSignup()
        {
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Prefabs")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Prefabs", "UI");

            // Clean up old/duplicate instances in scene
            CleanupDuplicateCanvases("SignupCanvas");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(SignupPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(SignupPrefabPath);
            }

            var root = new GameObject("SignupCanvas");
            var canvas = root.AddComponent<Canvas>(); 
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Above background (0) but below overlays (20+)
            root.AddComponent<GraphicRaycaster>();
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Background: match Login background (RawImage texture assignment)
            var bg = new GameObject("Background", typeof(RectTransform));
            var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(canvas.transform, false); bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            AssignBackgroundTexture(bg);

            // Panel - moved down to be below background area
            var panel = new GameObject("SignupPanel", typeof(RectTransform), typeof(Image));
            var pRT = panel.GetComponent<RectTransform>(); 
            pRT.SetParent(canvas.transform, false); 
            pRT.anchorMin = new Vector2(0.5f,0.5f); 
            pRT.anchorMax = new Vector2(0.5f,0.5f); 
            pRT.pivot = new Vector2(0.5f,0.5f); 
            pRT.sizeDelta = new Vector2(560, 520);
            pRT.anchoredPosition = new Vector2(0, -100); // Move down 100 pixels
            var pImg = panel.GetComponent<Image>(); pImg.color = new Color(0,0,0,0.35f);
            panel.AddComponent<HudAdjustable>().WidgetId = "SignupPanel";
            var vlg = panel.AddComponent<VerticalLayoutGroup>(); vlg.childAlignment = TextAnchor.UpperCenter; vlg.spacing = 8; vlg.padding = new RectOffset(24,24,24,24);
            panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Fields: Username, Email, Password, Confirm Password, Character Name
            var username = CreateField(panel.transform, "Username");
            var usernameInput = username.GetComponentInChildren<TMP_InputField>();
            var email = CreateField(panel.transform, "Email");
            var emailInput = email.GetComponentInChildren<TMP_InputField>();
            var pass = CreateField(panel.transform, "Password", mask:true);
            var passInput = pass.GetComponentInChildren<TMP_InputField>();
            var confirm = CreateField(panel.transform, "Confirm Password", mask:true);
            var confirmInput = confirm.GetComponentInChildren<TMP_InputField>();
            var charName = CreateField(panel.transform, "Character Name (min 3 chars)");
            var charNameInput = charName.GetComponentInChildren<TMP_InputField>();

            // Buttons
            var signupBtn = CreateButton(panel.transform, "Create Account");
            var backBtn = CreateButton(panel.transform, "Back to Login");

            // Controller
            var ctrl = panel.AddComponent<SignupController>();
            ctrl.Bind(usernameInput, emailInput, passInput, confirmInput, charNameInput,
                signupBtn.GetComponent<Button>(), backBtn.GetComponent<Button>());

            // Tab Navigation
            var tabNav = panel.AddComponent<TabNavigation>();
            // TabNavigation will auto-find input fields, but we can also manually add them in order
            tabNav.AddInputField(usernameInput);
            tabNav.AddInputField(emailInput);
            tabNav.AddInputField(passInput);
            tabNav.AddInputField(confirmInput);
            tabNav.AddInputField(charNameInput);
            tabNav.AddInputField(signupBtn.GetComponent<Button>());

            // EventSystem
            if (!Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, SignupPrefabPath, InteractionMode.UserAction);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        private static void AssignBackgroundTexture(GameObject target)
        {
            AssetDatabase.Refresh();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(BgPath);
            if (tex == null)
            {
                tex = FindLargestTextureInArtFolders();
            }
            var raw = target.GetComponent<UnityEngine.UI.RawImage>();
            if (raw == null) raw = target.AddComponent<UnityEngine.UI.RawImage>();
            if (tex != null)
            {
                raw.texture = tex; raw.color = Color.white;
                Debug.Log($"[SignupUiBuilder] Assigned background: {AssetDatabase.GetAssetPath(tex)}");
            }
            else
            {
                raw.color = new Color(0,0,0,0.25f);
                Debug.LogWarning("[SignupUiBuilder] Background texture not found; using tinted backdrop.");
            }
        }

        private static Texture2D FindLargestTextureInArtFolders()
        {
            string[] folders = new[] { "Assets/BlazedOdyssey/UI/Art", "Assets/BlazedOdyssey/UI/ART", "Assets/BlazedOdyssey/Art", "Assets/BlazedOdyssey/ART" };
            Texture2D best = null; int bestArea = -1;
            foreach (var folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex == null) continue;
                    int area = tex.width * tex.height;
                    if (area > bestArea) { best = tex; bestArea = area; }
                }
            }
            return best;
        }

        private static GameObject CreateField(Transform parent, string placeholder, bool mask = false)
        {
            var go = new GameObject("Field_"+placeholder, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = new Vector2(500, 42);
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image)); var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(go.transform, false); bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero; bg.GetComponent<Image>().color = new Color(1,1,1,0.08f);
            var inputGO = new GameObject("Input", typeof(RectTransform), typeof(TMP_InputField)); var iRT = inputGO.GetComponent<RectTransform>(); iRT.SetParent(go.transform, false); iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one; iRT.offsetMin = new Vector2(10,8); iRT.offsetMax = new Vector2(-10,-8);
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); var tRT = textGO.GetComponent<RectTransform>(); tRT.SetParent(inputGO.transform, false); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero; var text = textGO.GetComponent<TextMeshProUGUI>(); text.fontSize = 20; text.alignment = TextAlignmentOptions.Left; text.enableWordWrapping = false; UIThemeUtil.ApplyFontIfAvailable(text);
            var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI)); var phRT = phGO.GetComponent<RectTransform>(); phRT.SetParent(inputGO.transform, false); phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one; phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero; var ph = phGO.GetComponent<TextMeshProUGUI>(); ph.text = placeholder; ph.fontSize = 20; ph.color = new Color(1,1,1,0.35f); UIThemeUtil.ApplyFontIfAvailable(ph);
            var field = inputGO.GetComponent<TMP_InputField>(); field.textViewport = iRT; field.textComponent = text; field.placeholder = ph; field.contentType = mask ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string label)
        {
            var go = new GameObject(label+"Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = new Vector2(500, 42);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.12f);
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var tRT = txtGO.GetComponent<RectTransform>(); tRT.SetParent(go.transform, false); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
            var tmp = txtGO.GetComponent<TextMeshProUGUI>(); tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = 22; UIThemeUtil.ApplyFontIfAvailable(tmp);
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


