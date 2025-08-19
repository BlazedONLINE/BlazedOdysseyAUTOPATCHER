using UnityEngine;

// Attach this in every scene that can be loaded via warps. On scene load, it will move the Player to the SpawnPoint whose name matches GameState.NextSpawnPointName (if any).
public class SceneSpawnResolver : MonoBehaviour
{
    private void Start()
    {
        if (string.IsNullOrEmpty(GameState.NextSpawnPointName)) return;
        var spawns = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i=0;i<spawns.Length;i++)
        {
            if (spawns[i] != null && spawns[i].spawnName == GameState.NextSpawnPointName)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) player.transform.position = spawns[i].transform.position;
                break;
            }
        }
        GameState.NextSpawnPointName = "";
    }
}


