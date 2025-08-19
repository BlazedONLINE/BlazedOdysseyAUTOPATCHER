#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace BlazedOdyssey.UI
{
    /// <summary>
    /// Helper component that warns if a HudMovable is directly under a LayoutGroup parent.
    /// LayoutGroups will override anchoredPosition, breaking HUD editing.
    /// </summary>
    public class LayoutWrapper : MonoBehaviour
    {
        [Tooltip("Set to false to disable the warning check")]
        public bool checkForLayoutGroup = true;

        void Start()
        {
            if (!checkForLayoutGroup) return;

            var hudMovable = GetComponent<HudMovable>();
            if (!hudMovable) return;

            var parent = transform.parent;
            if (parent == null) return;

            var layoutGroup = parent.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                Debug.LogWarning(
                    $"[LayoutWrapper] HudMovable '{hudMovable.Id}' is directly under a LayoutGroup '{layoutGroup.GetType().Name}'. " +
                    "This will prevent HUD editing from working properly as the LayoutGroup will override anchoredPosition. " +
                    "Consider wrapping this element in a neutral GameObject to avoid conflicts.",
                    gameObject
                );
            }
        }

        /// <summary>
        /// Creates a neutral wrapper GameObject around the specified transform to isolate it from LayoutGroup effects.
        /// </summary>
        public static GameObject CreateNeutralWrapper(Transform target, string wrapperName = "Wrapper")
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));

            var parent = target.parent;
            var siblingIndex = target.GetSiblingIndex();

            // Create wrapper
            var wrapper = new GameObject(wrapperName, typeof(RectTransform));
            var wrapperRect = wrapper.GetComponent<RectTransform>();
            
            // Position wrapper in the same place as target
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.SetSiblingIndex(siblingIndex);
            
            // Copy target's RectTransform properties to wrapper
            var targetRect = target.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                wrapperRect.anchorMin = targetRect.anchorMin;
                wrapperRect.anchorMax = targetRect.anchorMax;
                wrapperRect.anchoredPosition = targetRect.anchoredPosition;
                wrapperRect.sizeDelta = targetRect.sizeDelta;
                wrapperRect.pivot = targetRect.pivot;
                wrapperRect.localScale = targetRect.localScale;
                wrapperRect.localRotation = targetRect.localRotation;
            }

            // Move target under wrapper and reset its transform to be neutral
            target.SetParent(wrapper.transform, false);
            if (targetRect != null)
            {
                targetRect.anchorMin = Vector2.zero;
                targetRect.anchorMax = Vector2.one;
                targetRect.anchoredPosition = Vector2.zero;
                targetRect.sizeDelta = Vector2.zero;
                targetRect.pivot = new Vector2(0.5f, 0.5f);
                targetRect.localScale = Vector3.one;
                targetRect.localRotation = Quaternion.identity;
            }

            Debug.Log($"[LayoutWrapper] Created neutral wrapper '{wrapperName}' for '{target.name}'");
            return wrapper;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Neutral Wrapper")]
        void CreateWrapperForThis()
        {
            CreateNeutralWrapper(transform, $"{gameObject.name}_Wrapper");
        }

        [ContextMenu("Check Layout Group Conflicts")]
        void CheckLayoutGroupConflicts()
        {
            checkForLayoutGroup = true;
            Start();
        }
#endif
    }
}