using UnityEngine;

namespace BlazedOdyssey.UI
{
    public static class HudLayoutPersistence
    {
        public static void Save(string id, RectTransform rt)
        {
            if (rt == null || string.IsNullOrEmpty(id)) return;
            var pfx = "bo_hud_" + id + "_";
            PlayerPrefs.SetFloat(pfx + "x", rt.anchoredPosition.x);
            PlayerPrefs.SetFloat(pfx + "y", rt.anchoredPosition.y);
            PlayerPrefs.SetFloat(pfx + "w", rt.sizeDelta.x);
            PlayerPrefs.SetFloat(pfx + "h", rt.sizeDelta.y);
            PlayerPrefs.Save();
        }

        public static bool Load(string id, RectTransform rt)
        {
            if (rt == null || string.IsNullOrEmpty(id)) return false;
            var pfx = "bo_hud_" + id + "_";
            if (!PlayerPrefs.HasKey(pfx + "x")) return false;
            var x = PlayerPrefs.GetFloat(pfx + "x");
            var y = PlayerPrefs.GetFloat(pfx + "y");
            var w = PlayerPrefs.GetFloat(pfx + "w", rt.sizeDelta.x);
            var h = PlayerPrefs.GetFloat(pfx + "h", rt.sizeDelta.y);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return true;
        }

        public static void Clear(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var pfx = "bo_hud_" + id + "_";
            PlayerPrefs.DeleteKey(pfx + "x");
            PlayerPrefs.DeleteKey(pfx + "y");
            PlayerPrefs.DeleteKey(pfx + "w");
            PlayerPrefs.DeleteKey(pfx + "h");
        }
    }
}
