using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveToMapCenter : MonoBehaviour
{
    public Tilemap ground;
    void Start() {
        var worldCenter = ground.transform.TransformPoint(ground.localBounds.center);
        transform.position = worldCenter;
        Debug.Log("Map center: " + worldCenter);
        enabled = false;
    }
}