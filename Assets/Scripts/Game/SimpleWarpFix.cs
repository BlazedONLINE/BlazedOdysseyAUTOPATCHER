using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleWarpFix : MonoBehaviour
{
    [Header("DISABLED - Use F7TeleportController instead")]
    public bool enableWarps = false;
    
    void Update()
    {
        if (!enableWarps) return;
        
        // DISABLED WARP KEYS - Use F7TeleportController instead
        // if (Input.GetKeyDown(KeyCode.Minus))
        // {
        //     Debug.Log("WARP TO POPPY INN!");
        //     SceneManager.LoadScene("PoppyInn");
        // }
        
        // if (Input.GetKeyDown(KeyCode.Equals))
        // {
        //     Debug.Log("WARP TO STARTER MAP!");
        //     SceneManager.LoadScene("StarterMapScene");
        // }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            FixPlayer();
        }
    }
    
    void FixPlayer()
    {
        SPUM_Prefabs[] spums = Object.FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var spum in spums)
        {
            spum.tag = "Player";
            if (spum.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D col = spum.gameObject.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);
            }
        }
        Debug.Log("FIXED PLAYER TAGS AND COLLIDERS!");
    }
    
    void OnGUI()
    {
        if (!enableWarps) return;
        GUI.Label(new Rect(10, 10, 300, 50), "SIMPLE WARP FIX DISABLED\nUse F7TeleportController instead\nF9 = Fix Player");
    }
}