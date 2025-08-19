using UnityEngine;
using BlazedOdyssey.UI;

public class StarterMapBootstrap : MonoBehaviour
{
	public Vector2 spawnWorldPosition = new Vector2(100f, 100f); // center of 200x200

	private void Start()
	{
		// Ensure MapBounds exists on the grid for clamping
		var grid = GameObject.Find("WorldGrid");
		if (grid == null)
		{
			var anyGrid = GameObject.FindObjectOfType<UnityEngine.Grid>();
			if (anyGrid != null) grid = anyGrid.gameObject;
		}
		if (grid != null && grid.GetComponent<MapBounds>() == null)
		{
			grid.AddComponent<MapBounds>();
		}

		// If a map is present, prefer spawning at its center
		if (grid != null)
		{
			var bounds = grid.GetComponent<MapBounds>();
			if (bounds != null)
			{
				bounds.ComputeFromChildTilemaps();
				var c = bounds.worldRect.center;
				spawnWorldPosition = new Vector2(c.x, c.y);
			}
		}

		var player = new GameObject("Player");
		player.transform.position = new Vector3(spawnWorldPosition.x, spawnWorldPosition.y, 0f);
		player.AddComponent<StarterMapPlayerController>();
		var cam = Camera.main;
		if (cam != null)
		{
			cam.orthographic = true;
			cam.orthographicSize = 4.5f; // closer view similar to Drakantos/NexusTK
			cam.transform.position = new Vector3(spawnWorldPosition.x, spawnWorldPosition.y, -10f);
			var follow = cam.gameObject.GetComponent<CameraFollow2D>();
			if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow2D>();
			follow.target = player.transform;
			follow.offset = new Vector3(0f, 0f, -10f);
			follow.smoothTime = 0.08f;
		}

		// Attach HUD
		var hudRoot = new GameObject("HUD");
		hudRoot.AddComponent<PlayerHUD>();

		// Do not auto-create specific spawners; use scene-placed MobSpawner instead
	}
}


