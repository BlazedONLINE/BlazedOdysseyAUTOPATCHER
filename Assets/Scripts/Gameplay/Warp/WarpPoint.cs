using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class WarpPoint : MonoBehaviour
{
    [Header("Destination")]
    public string targetScene;
    public string targetSpawnPoint;
    [Header("Trigger")]
    public bool requireButton = false;
    public KeyCode triggerKey = KeyCode.E;

    private void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (requireButton && !Input.GetKeyDown(triggerKey)) return;
        Warp();
    }

    public void Warp()
    {
        if (string.IsNullOrEmpty(targetScene)) return;
        GameState.NextSpawnPointName = targetSpawnPoint;
        SceneManager.LoadScene(targetScene);
    }
}


