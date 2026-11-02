using UnityEngine;
using UnityEngine.Tilemaps;

public class SuperPellet : MonoBehaviour
{
    [Header("Tilemap Settings")]

    // References to the four mirrored Tilemaps.
    public Tilemap[] levelTilemaps;

    // Tile used to replace the collected Power Pellet location.
    public TileBase emptyCorridorTile;

    [Header("Points")]

    // Points awarded for collecting a Power Pellet.
    public int points = 50;

    // Prevents collecting the same Power Pellet twice.
    private bool collected = false;

    // Allows PelletWinManager to check whether this pellet
    // has already been collected.
    public bool IsCollected
    {
        get { return collected; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions if this pellet was already collected.
        if (collected)
        {
            return;
        }

        // Check whether the object touching the pellet is the monkey.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        // Only the monkey can collect Power Pellets.
        if (monkey == null)
        {
            return;
        }

        // Mark this Power Pellet as collected.
        collected = true;

        // Replace the underlying tile with an empty corridor.
        CreateEmptyCorridor();

        // Award 50 points.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(points);
        }

        // Activate scared mode for the cats.
        if (CatScaredManager.Instance != null)
        {
            CatScaredManager.Instance.ActivateScaredMode();
        }

        // Check whether all normal and Power Pellets
        // have now been collected.
        if (PelletWinManager.Instance != null)
        {
            PelletWinManager.Instance.CheckForVictory();
        }

        Debug.Log("Power Pellet collected! +50");

        // Remove the collected Power Pellet GameObject.
        Destroy(gameObject);
    }

    // Replaces the tile underneath the Power Pellet.
    private void CreateEmptyCorridor()
    {
        // Stop if required references are missing.
        if (emptyCorridorTile == null ||
            levelTilemaps == null ||
            levelTilemaps.Length == 0)
        {
            return;
        }

        // Look for a Tilemap containing a tile
        // at the Power Pellet's position.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Convert the Power Pellet position into a tile cell.
            Vector3Int cell =
                tilemap.WorldToCell(transform.position);

            // Get the existing tile.
            TileBase existingTile = tilemap.GetTile(cell);

            // Replace the tile if one exists.
            if (existingTile != null)
            {
                tilemap.SetTile(cell, emptyCorridorTile);
                return;
            }
        }

        // Find the closest Tilemap if no tile was found.
        Tilemap nearestTilemap = null;

        float shortestDistance = float.MaxValue;

        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Get the Tilemap's bounds.
            Bounds bounds = tilemap.localBounds;

            // Convert the Power Pellet's world position
            // into the Tilemap's local position.
            Vector3 localPosition =
                tilemap.transform.InverseTransformPoint(
                    transform.position
                );

            // Find the closest point within the Tilemap.
            Vector3 closestPoint =
                bounds.ClosestPoint(localPosition);

            // Calculate the distance.
            float distance =
                Vector3.Distance(localPosition, closestPoint);

            // Remember the closest Tilemap.
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTilemap = tilemap;
            }
        }

        // Replace the tile on the closest Tilemap.
        if (nearestTilemap != null)
        {
            Vector3Int cell =
                nearestTilemap.WorldToCell(transform.position);

            nearestTilemap.SetTile(cell, emptyCorridorTile);
        }
    }
}