using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WindowAnimator : MonoBehaviour
{
    private Image windowImage;
    private float baseAlpha;
    
    public void StartAnimation()
    {
        windowImage = GetComponent<Image>();
        baseAlpha = windowImage.color.a;
        StartCoroutine(AnimateWindow());
    }
    
    IEnumerator AnimateWindow()
    {
        while (true)
        {
            // Flickering light effect
            float flicker = Mathf.Sin(Time.time * Random.Range(1f, 3f)) * 0.3f + 0.7f;
            Color currentColor = windowImage.color;
            windowImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, baseAlpha * flicker);
            
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }
    }
}
