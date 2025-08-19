using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BlazedOdyssey.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        public Image icon;
        public Image frame;
        public Text stackText;
        public int index;

        public void SetEmpty()
        {
            if (icon) { icon.enabled = true; icon.sprite = null; icon.color = new Color(1,1,1,0.08f); }
            if (stackText) stackText.text = "";
        }

        public void SetItem(Sprite itemIcon, int stack)
        {
            if (icon)
            {
                icon.enabled = true;
                icon.sprite = itemIcon;
                icon.color = Color.white;
            }
            if (stackText) stackText.text = stack > 1 ? stack.ToString() : "";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Clicked inventory slot {index}");
        }
    }
}
