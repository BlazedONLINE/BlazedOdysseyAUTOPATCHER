using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        public Image portrait;
        public Text nameText;
        public Text levelText;
        public BarUI hpBar;
        public BarUI mpBar;
        public BarUI xpBar;
        public Text goldText;
        public Text coordsText;

        public void Configure(string playerName, int level, int hp, int hpMax, int mp, int mpMax, Sprite portraitSprite = null)
        {
            if (portrait && portraitSprite) portrait.sprite = portraitSprite;
            if (nameText) nameText.text = playerName;
            if (levelText) levelText.text = $"Lv {level}";
            if (hpBar) hpBar.Configure(hp, hpMax);
            if (mpBar)
            {
                mpBar.Configure(mp, mpMax);
                mpBar.highColor = new Color(0.20f, 0.55f, 0.95f, 1f);
                mpBar.lowColor = new Color(0.10f, 0.25f, 0.60f, 1f);
            }
        }

        public void UpdateGold(int amount)
        {
            if (goldText) goldText.text = $"Gold: {amount}";
        }

        public void UpdateCoords(Vector2 pos)
        {
            if (coordsText) coordsText.text = $"X:{Mathf.RoundToInt(pos.x)} Y:{Mathf.RoundToInt(pos.y)}";
        }

        public void UpdateExperience(int current, int required)
        {
            if (xpBar) xpBar.Configure(current, Mathf.Max(1, required));
        }

        public void UpdateHealth(int current, int max)
        {
            if (hpBar != null)
            {
                hpBar.Configure(current, max);
            }
        }

        public void UpdateMana(int current, int max)
        {
            if (mpBar) mpBar.Configure(current, max);
        }
    }
}
