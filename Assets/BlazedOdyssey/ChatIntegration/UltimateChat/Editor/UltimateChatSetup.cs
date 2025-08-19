using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class UltimateChatSetup
{
    [MenuItem("BlazedOdyssey/Chat/Setup Ultimate Chat (Bottom-Left)")]
    public static void SetupChat()
    {
        // Ensure a Canvas exists
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Find an Ultimate Chat Box in scene
        var chatBox = Object.FindObjectsOfType<MonoBehaviour>(true)
            .FirstOrDefault(m => m != null && m.GetType().FullName != null && m.GetType().FullName.EndsWith("UltimateChatBox"));

        if (chatBox == null)
        {
            EditorUtility.DisplayDialog("Ultimate Chat Box", "No Ultimate Chat Box instance found in the scene. Drag one prefab (e.g., ChatBox_Outline) under the Canvas, then run this again.", "OK");
            return;
        }

        var chatTransform = (chatBox as Component).transform as RectTransform;
        Undo.RecordObject(chatTransform, "Position Chat");
        chatTransform.SetParent(canvas.transform, true);
        chatTransform.anchorMin = new Vector2(0, 0);
        chatTransform.anchorMax = new Vector2(0, 0);
        chatTransform.pivot = new Vector2(0, 0);
        chatTransform.anchoredPosition = new Vector2(16, 16);

        // Ensure EventSystem exists
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Assign a TMP Emoji Sprite Asset if available and if the component exposes it
        var chatType = chatBox.GetType();
        var emojiProp = chatType.GetField("emojiAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            ?? chatType.GetProperty("EmojiAsset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) as System.Reflection.MemberInfo;

        if (emojiProp != null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_SpriteAsset emoji");
            if (guids.Length == 0)
                guids = AssetDatabase.FindAssets("t:TMP_SpriteAsset");

            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(path);
                if (spriteAsset != null)
                {
                    var field = emojiProp as System.Reflection.FieldInfo;
                    var prop = emojiProp as System.Reflection.PropertyInfo;
                    Undo.RecordObject(chatBox, "Assign Emoji Asset");
                    if (field != null) field.SetValue(chatBox, spriteAsset);
                    if (prop != null && prop.CanWrite) prop.SetValue(chatBox, spriteAsset);
                }
            }
        }

        // Ensure adapter exists and is configured for per-scene edits
        var adapter = Object.FindObjectOfType<UltimateChatAdapter>();
        if (adapter == null)
        {
            var go = new GameObject("ChatAdapter");
            adapter = go.AddComponent<UltimateChatAdapter>();
        }
        Undo.RecordObject(adapter, "Configure Chat Adapter");
        var persistField = typeof(UltimateChatAdapter).GetField("persistAcrossScenes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (persistField != null) persistField.SetValue(adapter, false);

        EditorUtility.DisplayDialog("Ultimate Chat Box", "Chat positioned bottom-left and adapter configured. You can now drag/resize in Scene view and Save.", "OK");
    }
}


