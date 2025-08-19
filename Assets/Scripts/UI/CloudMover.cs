using UnityEngine;
using System.Collections;

public class CloudMover : MonoBehaviour
{
    private float speed;
    private float screenWidth = 1920f;
    
    public void StartDrifting()
    {
        speed = Random.Range(10f, 30f);
        StartCoroutine(DriftAnimation());
    }
    
    IEnumerator DriftAnimation()
    {
        var rectTransform = GetComponent<RectTransform>();
        while (true)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            currentPos.x -= speed * Time.deltaTime;
            
            // Reset position when off screen
            if (currentPos.x < -screenWidth)
            {
                currentPos.x = screenWidth + 200f;
            }
            
            rectTransform.anchoredPosition = currentPos;
            yield return null;
        }
    }
}
