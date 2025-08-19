using UnityEngine;
using UnityEngine.UI;

public class BackgroundAnimator : MonoBehaviour
{
    private Image backgroundImage;
    private float time = 0f;
    
    void Start()
    {
        backgroundImage = GetComponent<Image>();
    }
    
    void Update()
    {
        time += Time.deltaTime * 0.2f;
        
        // Create subtle color pulsing effect
        float pulse = Mathf.Sin(time) * 0.1f + 0.9f;
        Color baseColor = new Color(0.05f, 0.1f, 0.2f, 1f);
        backgroundImage.color = baseColor * pulse;
    }
}
