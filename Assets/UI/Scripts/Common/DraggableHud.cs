using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BlazedOdyssey.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableHud : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Identity")]
        public string elementId = "element";
        public string groupId = "";
        public bool dragWholeGroup = false;

        [Header("Behavior")]
        public bool clampToScreen = true;
        public bool snapToEdges = true;
        public float snapEdgeThreshold = 12f;
        public bool snapToPixelGrid = true;
        public int pixelGrid = 1;

        [Header("Visuals (edit mode)")]
        public Color editTint = new Color(1, 1, 1, 0.06f);
        public Color outlineColor = new Color(1, 1, 1, 0.15f);

        private RectTransform _rt;
        private Canvas _canvas;
        private Image _bgImage;
        private Outline _outline;
        private Vector2 _defaultPos;
        private Vector2 _defaultSize;

        private static readonly List<DraggableHud> _all = new();

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            _bgImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            _bgImage.raycastTarget = true;

            _outline = gameObject.GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            _outline.enabled = false;

            _defaultPos = _rt.anchoredPosition;
            _defaultSize = _rt.sizeDelta;

            _all.Add(this);
            HudLayoutPersistence.Load(elementId, _rt);
        }

        private void OnDestroy()
        {
            _all.Remove(this);
        }

        private float ScaleFactor => (_canvas != null && _canvas.scaleFactor != 0f) ? _canvas.scaleFactor : 1f;
        public static IEnumerable<DraggableHud> All => _all;

        public void SetEditVisuals(bool editing)
        {
            if (_bgImage) _bgImage.color = editing ? editTint : new Color(1,1,1,0f);
            if (_outline)
            {
                _outline.effectColor = outlineColor;
                _outline.effectDistance = editing ? new Vector2(1f, -1f) : Vector2.zero;
                _outline.enabled = editing;
            }
        }

        public void ResetToDefault()
        {
            _rt.anchoredPosition = _defaultPos;
            _rt.sizeDelta = _defaultSize;
            HudLayoutPersistence.Clear(elementId);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!HudAdjustManager.IsEditing) return;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!HudAdjustManager.IsEditing) return;
            Vector2 delta = eventData.delta / ScaleFactor;
            ApplyDelta(delta, !string.IsNullOrEmpty(groupId) && dragWholeGroup);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!HudAdjustManager.IsEditing) return;
            if (!Input.GetKey(KeyCode.LeftShift))
                HudLayoutPersistence.Save(elementId, _rt);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!HudAdjustManager.IsEditing) return;
            if (eventData.button == PointerEventData.InputButton.Right)
                ResetToDefault();
        }

        private void ApplyDelta(Vector2 delta, bool includeGroup)
        {
            if (includeGroup)
            {
                foreach (var d in _all)
                    if (d != null && d.groupId == this.groupId) d.MoveBy(delta);
            }
            else MoveBy(delta);
        }

        private void MoveBy(Vector2 delta)
        {
            var pos = _rt.anchoredPosition + delta;

            if (snapToPixelGrid && pixelGrid > 0)
            {
                pos.x = Mathf.Round(pos.x / pixelGrid) * pixelGrid;
                pos.y = Mathf.Round(pos.y / pixelGrid) * pixelGrid;
            }

            _rt.anchoredPosition = pos;

            if (clampToScreen || (snapToEdges && !Input.GetKey(KeyCode.LeftShift)))
                ClampAndSnap();
        }

        private void ClampAndSnap()
        {
            if (_rt.parent is not RectTransform parent) return;

            Vector2 parentSize = parent.rect.size;
            Vector2 size = _rt.rect.size;
            Vector2 pos = _rt.anchoredPosition;

            float minX = -parentSize.x * _rt.pivot.x + size.x * _rt.pivot.x;
            float maxX = parentSize.x * (1 - _rt.pivot.x) - size.x * (1 - _rt.pivot.x);
            float minY = -parentSize.y * _rt.pivot.y + size.y * _rt.pivot.y;
            float maxY = parentSize.y * (1 - _rt.pivot.y) - size.y * (1 - _rt.pivot.y);

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            if (snapToEdges && !Input.GetKey(KeyCode.LeftShift))
            {
                if (Mathf.Abs(pos.x - minX) <= snapEdgeThreshold) pos.x = minX;
                if (Mathf.Abs(maxX - pos.x) <= snapEdgeThreshold) pos.x = maxX;
                if (Mathf.Abs(pos.y - minY) <= snapEdgeThreshold) pos.y = minY;
                if (Mathf.Abs(maxY - pos.y) <= snapEdgeThreshold) pos.y = maxY;
            }

            _rt.anchoredPosition = pos;
        }
    }
}
