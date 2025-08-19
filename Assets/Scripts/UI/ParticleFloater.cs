using UnityEngine;
using System.Collections;

public class ParticleFloater : MonoBehaviour
{
    private Vector2 startPos;
    private float floatSpeed;
    private float floatRange;
    
    public void StartFloating()
    {
        startPos = GetComponent<RectTransform>().anchoredPosition;
        floatSpeed = Random.Range(0.5f, 1.5f);
        floatRange = Random.Range(10f, 30f);
        StartCoroutine(FloatAnimation());
    }
    
    IEnumerator FloatAnimation()
    {
        var rectTransform = GetComponent<RectTransform>();
        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            rectTransform.anchoredPosition = new Vector2(startPos.x, startPos.y + yOffset);
            yield return null;
        }
    }
}