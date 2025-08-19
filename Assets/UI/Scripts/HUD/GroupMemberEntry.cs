using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    public class GroupMemberEntry : MonoBehaviour
    {
        public Text nameText;
        public BarUI hpBar;

        public void Setup(string displayName, int hp, int hpMax)
        {
            if (nameText) nameText.text = displayName;
            if (hpBar) hpBar.Configure(hp, hpMax);
        }

        public void SetHP(int hp)
        {
            if (hpBar) hpBar.SetValue(hp);
        }
    }
}
