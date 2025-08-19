using UnityEngine;

namespace BlazedOdyssey.UI.Login
{
    public static class RememberMeStore
    {
        const string KEY_REMEMBER = "bo.rememberMe";
        const string KEY_USER = "bo.rememberedUser";

        public static bool GetRememberMe() => PlayerPrefs.GetInt(KEY_REMEMBER, 0) == 1;
        public static string GetUser() => PlayerPrefs.GetString(KEY_USER, string.Empty);

        public static void SetRememberMe(bool v) => PlayerPrefs.SetInt(KEY_REMEMBER, v ? 1 : 0);
        public static void SetUser(string u)
        {
            if (!string.IsNullOrEmpty(u)) PlayerPrefs.SetString(KEY_USER, u);
        }
    }
}


