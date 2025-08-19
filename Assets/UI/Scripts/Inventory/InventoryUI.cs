using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public RectTransform window;
        public GridLayoutGroup grid;
        public InventorySlotUI slotTemplate;
        public int columns = 5;
        public int rows = 5;

        private InventorySlotUI[] _slots;

        private void Start()
        {
            BuildGrid();
            Hide();
        }

        public void Toggle()
        {
            if (!window) return;
            bool active = !window.gameObject.activeSelf;
            window.gameObject.SetActive(active);
        }

        public void Show() { if (window) window.gameObject.SetActive(true); }
        public void Hide() { if (window) window.gameObject.SetActive(false); }

        private void BuildGrid()
        {
            if (!grid || !slotTemplate) return;
            slotTemplate.gameObject.SetActive(false);

            int total = Mathf.Max(1, columns * rows);
            _slots = new InventorySlotUI[total];

            for (int i = 0; i < total; i++)
            {
                var s = GameObject.Instantiate(slotTemplate, grid.transform);
                s.gameObject.SetActive(true);
                s.index = i;
                s.SetEmpty();
                _slots[i] = s;
            }
        }

        // Simple filler to visualize
        public void FillWithDummy()
        {
            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (i % 3 == 0)
                    _slots[i].SetItem(null, Random.Range(1, 5));
                else
                    _slots[i].SetEmpty();
            }
        }
    }
}
