using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlazedOdyssey.UI;
using BlazedOdyssey.UI.Common;

namespace BlazedOdyssey.Tools.Editor
{
    public static class SettingsUiBuilder
    {
        private const string PrefabFolder = "Assets/BlazedOdyssey/Prefabs/UI";
        public const string SettingsPrefabPath = PrefabFolder + "/SettingsPanel.prefab";

        [MenuItem("BlazedOdyssey/UI/Build Settings Panel (Scalable)")]
        public static GameObject BuildSettings()
        {
            // Ensure folder
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
            if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Prefabs")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Prefabs", "UI");

            // Clean existing (if corrupt)
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(SettingsPrefabPath);
            }

            // Root - moved down to be below background area
            var root = new GameObject("SettingsPanel", typeof(RectTransform));
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(780, 520);
            rt.anchoredPosition = new Vector2(0, -100); // Move down 100 pixels
            root.AddComponent<HudAdjustable>().WidgetId = "SettingsPanel";

            // Dim overlay
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            var dimRT = dim.GetComponent<RectTransform>(); dimRT.SetParent(root.transform, false); dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one; dimRT.offsetMin = Vector2.zero; dimRT.offsetMax = Vector2.zero;
            var dimImg = dim.GetComponent<Image>(); dimImg.color = new Color(0,0,0,0.35f);

            // Built-in UI sprites will be loaded per-use with robust fallbacks

            // Window
            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            var wRT = window.GetComponent<RectTransform>();
            wRT.SetParent(root.transform, false);
            wRT.anchorMin = new Vector2(0.5f,0.5f); wRT.anchorMax = new Vector2(0.5f,0.5f); wRT.pivot = new Vector2(0.5f,0.5f);
            wRT.sizeDelta = new Vector2(700, 440);
            // Move further down to align with login layout
            wRT.anchoredPosition = new Vector2(0f, -260f);
            var wImg = window.GetComponent<Image>(); wImg.color = new Color(0,0,0,0.6f); wImg.sprite = LoadUiSprite(); wImg.type = Image.Type.Sliced;

            var scalerComp = window.AddComponent<BlazedOdyssey.UI.SettingsPanelScaler>();
            var v = window.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(28,28,24,24); v.spacing = 16; v.childAlignment = TextAnchor.UpperLeft;

            // Title
            var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            var tRT = title.GetComponent<RectTransform>(); tRT.SetParent(window.transform, false); tRT.sizeDelta = new Vector2(0, 36);
            var t = title.GetComponent<TextMeshProUGUI>(); t.text = "Settings"; t.fontSize = 28; t.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(t, isTitle:true);

            // Content container
            var content = new GameObject("Content", typeof(RectTransform));
            var cRT = content.GetComponent<RectTransform>(); cRT.SetParent(window.transform, false); cRT.sizeDelta = new Vector2(0, 320);
            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(320, 56); grid.spacing = new Vector2(20, 10); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2;

            // Controls
            var musicRow = CreateSliderWithMute(content.transform, "Music Volume");
            var scale = CreateSlider(content.transform, "UI Scale", 0.75f, 1.25f, 1f);
            var res = CreateDropdown(content.transform, "Resolution");
            var fullscreen = CreateToggle(content.transform, "Fullscreen");

            // Buttons row
            var buttons = new GameObject("Buttons", typeof(RectTransform));
            var bRT = buttons.GetComponent<RectTransform>(); bRT.SetParent(window.transform, false); bRT.sizeDelta = new Vector2(0, 44);
            var h = buttons.AddComponent<HorizontalLayoutGroup>(); h.spacing = 16; h.childAlignment = TextAnchor.MiddleRight;
            var filler = new GameObject("Filler", typeof(RectTransform)); filler.GetComponent<RectTransform>().SetParent(buttons.transform, false);
            var apply = CreateButton(buttons.transform, "Apply");
            var close = CreateButton(buttons.transform, "Close");

            // Component wiring
            var comp = root.AddComponent<SettingsPanel>();
            comp.window = wRT;
            comp.dimOverlay = dim;
            comp.musicVolume = musicRow.GetComponentInChildren<Slider>();
            comp.musicMute = musicRow.GetComponentInChildren<Toggle>();
            comp.uiScale = scale.GetComponentInChildren<Slider>();
            comp.resolutionDropdown = res.GetComponentInChildren<Dropdown>();
            comp.fullscreenToggle = fullscreen.GetComponentInChildren<Toggle>();
            comp.applyButton = apply.GetComponent<Button>();
            comp.closeButton = close.GetComponent<Button>();
            comp.uiRootToScale = null; // can be assigned at runtime to scale whole UI

            // Save prefab and keep the current scene object connected to it
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, SettingsPrefabPath, InteractionMode.UserAction);
            // Ensure there is a Canvas and parent the panel under it so it renders
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("UICanvas", typeof(Canvas), typeof(GraphicRaycaster));
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
            // Ensure EventSystem exists
            if (!Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
            // 'root' is the in-scene instance; parent it under the canvas
            root.transform.SetParent(canvas.transform, false);
            Selection.activeObject = root;
            EditorGUIUtility.PingObject(root);
            return root;
        }

        private static GameObject CreateSlider(Transform parent, string label, float min = 0f, float max = 1f, float defaultValue = 1f)
        {
            var row = new GameObject(label.Replace(" ", "") + "Row", typeof(RectTransform));
            var rRT = row.GetComponent<RectTransform>(); rRT.SetParent(parent, false); rRT.sizeDelta = new Vector2(320, 56);
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var lRT = lblGO.GetComponent<RectTransform>(); lRT.SetParent(row.transform, false); lRT.anchorMin = new Vector2(0, 0.5f); lRT.anchorMax = new Vector2(0, 0.5f); lRT.pivot = new Vector2(0,0.5f); lRT.anchoredPosition = new Vector2(0, 0); lRT.sizeDelta = new Vector2(140, 30);
            var lbl = lblGO.GetComponent<TextMeshProUGUI>(); lbl.text = label; lbl.fontSize = 20; lbl.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(lbl);
            var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var sRT = sliderGO.GetComponent<RectTransform>(); sRT.SetParent(row.transform, false); sRT.anchorMin = new Vector2(0, 0.5f); sRT.anchorMax = new Vector2(1, 0.5f); sRT.pivot = new Vector2(0,0.5f); sRT.anchoredPosition = new Vector2(150, 0); sRT.sizeDelta = new Vector2(-150, 24);
            var slider = sliderGO.GetComponent<Slider>(); slider.minValue = min; slider.maxValue = max; slider.value = defaultValue;
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(sliderGO.transform, false); bgRT.anchorMin = new Vector2(0,0.5f); bgRT.anchorMax = new Vector2(1,0.5f); bgRT.sizeDelta = new Vector2(0, 8); var bgImg = bg.GetComponent<Image>(); bgImg.sprite = LoadBackgroundSprite(); bgImg.type = Image.Type.Sliced; bgImg.color = new Color(1,1,1,0.2f);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform)); var faRT = fillArea.GetComponent<RectTransform>(); faRT.SetParent(sliderGO.transform, false); faRT.anchorMin = new Vector2(0,0.5f); faRT.anchorMax = new Vector2(1,0.5f); faRT.sizeDelta = new Vector2(-20, 8); faRT.anchoredPosition = new Vector2(10,0);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); var fRT = fill.GetComponent<RectTransform>(); fRT.SetParent(fillArea.transform, false); fRT.anchorMin = new Vector2(0,0); fRT.anchorMax = new Vector2(1,1); var fillImg = fill.GetComponent<Image>(); fillImg.sprite = LoadUiSprite(); fillImg.type = Image.Type.Sliced; fillImg.color = new Color(1,1,1,0.5f);
            var handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform)); var hRT = handleSlideArea.GetComponent<RectTransform>(); hRT.SetParent(sliderGO.transform, false); hRT.anchorMin = new Vector2(0,0); hRT.anchorMax = new Vector2(1,1); hRT.sizeDelta = new Vector2(0,0);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image)); var hhRT = handle.GetComponent<RectTransform>(); hhRT.SetParent(handleSlideArea.transform, false); hhRT.sizeDelta = new Vector2(16,16); var hImg = handle.GetComponent<Image>(); hImg.sprite = LoadKnobSprite();
            slider.targetGraphic = hImg; slider.fillRect = fRT; slider.handleRect = hhRT;
            return row;
        }

        private static GameObject CreateSliderWithMute(Transform parent, string label)
        {
            var row = new GameObject(label.Replace(" ", "") + "Row", typeof(RectTransform));
            var rRT = row.GetComponent<RectTransform>(); rRT.SetParent(parent, false); rRT.sizeDelta = new Vector2(320, 56);
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var lRT = lblGO.GetComponent<RectTransform>(); lRT.SetParent(row.transform, false); lRT.anchorMin = new Vector2(0, 0.5f); lRT.anchorMax = new Vector2(0, 0.5f); lRT.pivot = new Vector2(0,0.5f); lRT.anchoredPosition = new Vector2(0, 0); lRT.sizeDelta = new Vector2(120, 30);
            var lbl = lblGO.GetComponent<TextMeshProUGUI>(); lbl.text = label; lbl.fontSize = 20; lbl.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(lbl);
            var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var sRT = sliderGO.GetComponent<RectTransform>(); sRT.SetParent(row.transform, false); sRT.anchorMin = new Vector2(0, 0.5f); sRT.anchorMax = new Vector2(1, 0.5f); sRT.pivot = new Vector2(0,0.5f); sRT.anchoredPosition = new Vector2(130, 0); sRT.sizeDelta = new Vector2(-230, 24);
            var slider = sliderGO.GetComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.6f;
            var muteGO = new GameObject("Mute", typeof(RectTransform), typeof(Toggle));
            var mRT = muteGO.GetComponent<RectTransform>(); mRT.SetParent(row.transform, false); mRT.anchorMin = new Vector2(1, 0.5f); mRT.anchorMax = new Vector2(1, 0.5f); mRT.pivot = new Vector2(1,0.5f); mRT.anchoredPosition = new Vector2(0, 0); mRT.sizeDelta = new Vector2(90, 24);
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(muteGO.transform, false); bgRT.sizeDelta = new Vector2(18,18);
            var ck = new GameObject("Checkmark", typeof(RectTransform), typeof(Image)); var ckRT = ck.GetComponent<RectTransform>(); ckRT.SetParent(bg.transform, false); ckRT.sizeDelta = new Vector2(18,18);
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); var lr = labelGO.GetComponent<RectTransform>(); lr.SetParent(muteGO.transform, false); lr.anchorMin = new Vector2(0,0.5f); lr.anchorMax = new Vector2(0,0.5f); lr.pivot = new Vector2(0,0.5f); lr.anchoredPosition = new Vector2(22,0); lr.sizeDelta = new Vector2(66,18);
            var ltxt = labelGO.GetComponent<TextMeshProUGUI>(); ltxt.text = "Mute"; ltxt.fontSize = 18; ltxt.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(ltxt);
            var bgImg = bg.GetComponent<Image>(); bgImg.sprite = LoadUiSprite(); bgImg.type = Image.Type.Sliced; bgImg.color = new Color(1,1,1,0.25f);
            var ckImg = ck.GetComponent<Image>(); ckImg.sprite = LoadCheckmarkSprite(); ckImg.color = Color.white;
            var tgl = muteGO.GetComponent<Toggle>(); tgl.targetGraphic = bgImg; tgl.graphic = ckImg;
            return row;
        }

        private static GameObject CreateDropdown(Transform parent, string label)
        {
            var row = new GameObject(label.Replace(" ", "") + "Row", typeof(RectTransform));
            var rRT = row.GetComponent<RectTransform>(); rRT.SetParent(parent, false); rRT.sizeDelta = new Vector2(320, 56);
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var lRT = lblGO.GetComponent<RectTransform>(); lRT.SetParent(row.transform, false); lRT.anchorMin = new Vector2(0, 0.5f); lRT.anchorMax = new Vector2(0, 0.5f); lRT.pivot = new Vector2(0,0.5f); lRT.anchoredPosition = new Vector2(0, 0); lRT.sizeDelta = new Vector2(140, 30);
            var lbl = lblGO.GetComponent<TextMeshProUGUI>(); lbl.text = label; lbl.fontSize = 20; lbl.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(lbl);
            var ddGO = new GameObject("Dropdown", typeof(RectTransform), typeof(Dropdown));
            var dRT = ddGO.GetComponent<RectTransform>(); dRT.SetParent(row.transform, false); dRT.anchorMin = new Vector2(0, 0.5f); dRT.anchorMax = new Vector2(1, 0.5f); dRT.pivot = new Vector2(0,0.5f); dRT.anchoredPosition = new Vector2(150, 0); dRT.sizeDelta = new Vector2(-150, 28);
            var dd = ddGO.GetComponent<Dropdown>();
            var bg = ddGO.AddComponent<Image>(); var uiSprite = LoadUiSprite(); bg.sprite = uiSprite; bg.type = Image.Type.Sliced; bg.color = new Color(1,1,1,0.12f);
            dd.targetGraphic = bg;
            return row;
        }

        private static GameObject CreateToggle(Transform parent, string label)
        {
            var row = new GameObject(label.Replace(" ", "") + "Row", typeof(RectTransform));
            var rRT = row.GetComponent<RectTransform>(); rRT.SetParent(parent, false); rRT.sizeDelta = new Vector2(320, 56);
            var tGO = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            var tRT = tGO.GetComponent<RectTransform>(); tRT.SetParent(row.transform, false); tRT.anchorMin = new Vector2(0, 0.5f); tRT.anchorMax = new Vector2(0, 0.5f); tRT.pivot = new Vector2(0,0.5f); tRT.anchoredPosition = new Vector2(0, 0); tRT.sizeDelta = new Vector2(22, 22);
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(tGO.transform, false); bgRT.sizeDelta = new Vector2(22,22);
            var ck = new GameObject("Checkmark", typeof(RectTransform), typeof(Image)); var ckRT = ck.GetComponent<RectTransform>(); ckRT.SetParent(bg.transform, false); ckRT.sizeDelta = new Vector2(22,22);
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var lRT = labelGO.GetComponent<RectTransform>(); lRT.SetParent(row.transform, false); lRT.anchorMin = new Vector2(0, 0.5f); lRT.anchorMax = new Vector2(0, 0.5f); lRT.pivot = new Vector2(0,0.5f); lRT.anchoredPosition = new Vector2(28, 0); lRT.sizeDelta = new Vector2(180, 28);
            var lbl = labelGO.GetComponent<TextMeshProUGUI>(); lbl.text = label; lbl.fontSize = 20; lbl.color = Color.white; UIThemeUtil.ApplyFontIfAvailable(lbl);
            var bgImg = bg.GetComponent<Image>(); var uiSprite = LoadUiSprite(); bgImg.sprite = uiSprite; bgImg.type = Image.Type.Sliced; bgImg.color = new Color(1,1,1,0.25f);
            var ckImg = ck.GetComponent<Image>(); var checkSprite = LoadCheckmarkSprite(); ckImg.sprite = checkSprite; ckImg.color = Color.white;
            var tgl = tGO.GetComponent<Toggle>(); tgl.targetGraphic = bgImg; tgl.graphic = ckImg; tgl.isOn = Screen.fullScreen;
            return row;
        }

        private static GameObject CreateButton(Transform parent, string label)
        {
            var go = new GameObject(label+"Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = new Vector2(140, 40);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.12f); var uiSprite = LoadUiSprite(); img.sprite = uiSprite; img.type = Image.Type.Sliced;
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var tRT = txtGO.GetComponent<RectTransform>(); tRT.SetParent(go.transform, false); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
            var tmp = txtGO.GetComponent<TextMeshProUGUI>(); tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = 22; UIThemeUtil.ApplyFontIfAvailable(tmp);
            return go;
        }

        // ===== Helpers: Robust sprite loaders with fallbacks =====
        private static Sprite LoadUiSprite()
        {
            // Always return a generated sliced solid sprite to avoid engine error logs on missing built-ins
            return CreateSlicedSolidSprite(Color.white, 8);
        }

        private static Sprite LoadBackgroundSprite()
        {
            return CreateSlicedSolidSprite(Color.white, 8);
        }

        private static Sprite LoadKnobSprite()
        {
            return CreateSolidSprite(new Color(1f,1f,1f,1f));
        }

        private static Sprite LoadCheckmarkSprite()
        {
            return CreateCheckmarkSprite(Color.white, 16);
        }

        private static Sprite CreateSolidSprite(Color color, int size = 8)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f);
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
            // Transparent background
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0,0,0,0);
            // Simple V shape
            for (int x = 2; x < size/2; x++)
            {
                int y = x; tex.SetPixel(x, y, color); tex.SetPixel(x, y-1, color);
            }
            for (int x = size/2; x < size-2; x++)
            {
                int y = size - (x - size/2) - 3; tex.SetPixel(x, y, color); tex.SetPixel(x, y-1, color);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), 100f);
        }
    }
}


