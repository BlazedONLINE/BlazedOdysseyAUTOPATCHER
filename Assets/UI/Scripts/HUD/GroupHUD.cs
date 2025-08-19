using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    public class GroupHUD : MonoBehaviour
    {
        [Header("Template & Root")]
        public RectTransform listRoot;
        public GroupMemberEntry entryTemplate;
        public int maxMembers = 6;

        private readonly List<GroupMemberEntry> _entries = new();
        private int _dummyCount = 0;

        private void Start()
        {
            if (entryTemplate) entryTemplate.gameObject.SetActive(false);
        }

        public void ClearAll()
        {
            foreach (var e in _entries)
                if (e) GameObject.Destroy(e.gameObject);
            _entries.Clear();
        }

        public GroupMemberEntry AddMember(string displayName, int hp, int hpMax)
        {
            if (_entries.Count >= maxMembers || !entryTemplate || !listRoot) return null;
            var inst = GameObject.Instantiate(entryTemplate, listRoot);
            inst.gameObject.SetActive(true);
            inst.Setup(displayName, hp, hpMax);
            _entries.Add(inst);
            return inst;
        }

        // Debug helper – call from bootstrap with key P
        public void AddDummyMember()
        {
            _dummyCount++;
            AddMember($"Player {_dummyCount}", Random.Range(40, 100), 100);
        }
    }
}
