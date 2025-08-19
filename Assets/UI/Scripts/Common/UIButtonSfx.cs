using UnityEngine;
using UnityEngine.EventSystems;

namespace BlazedOdyssey.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public AudioClip hoverClip;
        public AudioClip clickClip;

        private AudioSource _src;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverClip) _src.PlayOneShot(hoverClip);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickClip) _src.PlayOneShot(clickClip);
        }
    }
}
