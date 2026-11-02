using UnityEngine;
using UnityEngine.Tilemaps;

public class SuperPellet : MonoBehaviour
{
    [Header("Tilemap Settings")]

    // References to your four mirrored Tilemaps.
    public Tilemap[] levelTilemaps;

    // Tile painted where the Power Pellet was collected.
    public TileBase emptyCorridorTile;

    [Header("Points")]

    // Points awarded for collecting a Power Pellet.
    public int points = 50;

    // Prevents duplicate collection.
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore already-collected pellets.
        if (collected)
        {
            return;
        }

        // Check whether the monkey entered the trigger.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        if (monkey == null)
        {
            return;
        }

        collected = true;

        // Paint an empty corridor tile.
        CreateEmptyCorridor();

        // Award 50 points.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(points);
        }

        // Activate scared mode for all four cats.
        if (CatScaredManager.Instance != null)
        {
            CatScaredManager.Instance.ActivateScaredMode();
        }

        Debug.Log("Power Pellet collected! +50");

        // Remove the animated pellet.
        Destroy(gameObject);
    }

    // Paints a corridor tile where the pellet was collected.
    private void CreateEmptyCorridor()
    {
        if (emptyCorridorTile == null)
        {
            Debug.LogWarning("Empty Corridor Tile is not assigned!");
            return;
        }

        if (levelTilemaps == null || levelTilemaps.Length == 0)
        {
            Debug.LogWarning("No Tilemaps assigned!");
            return;
        }

        // look for a Tilemap that already has a tile here.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Vector3Int cell =
                tilemap.WorldToCell(transform.position);

            TileBase existingTile = tilemap.GetTile(cell);

            if (existingTile != null)
            {
                tilemap.SetTile(cell, emptyCorridorTile);

                Debug.Log("Empty corridor tile created!");
                return;
            }
        }

        // If the cell is empty, find the nearest Tilemap section.
        Tilemap nearestTilemap = null;
        float shortestDistance = float.MaxValue;

        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Bounds bounds = tilemap.localBounds;

            Vector3 localPosition =
                tilemap.transform.InverseTransformPoint(
                    transform.position
                );

            Vector3 closestPoint =
                bounds.ClosestPoint(localPosition);

            float distance =
                Vector3.Distance(localPosition, closestPoint);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTilemap = tilemap;
            }
        }

        // Create the corridor in the selected Tilemap.
        if (nearestTilemap != null)
        {
            Vector3Int cell =
                nearestTilemap.WorldToCell(transform.position);

            nearestTilemap.SetTile(cell, emptyCorridorTile);

            Debug.Log("Empty corridor tile created in empty cell!");
        }
    }
}