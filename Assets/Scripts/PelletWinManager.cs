using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class PelletWinManager : MonoBehaviour
{
    public static PelletWinManager Instance;

    [Header("Maze Tilemaps")]
    public Tilemap[] levelTilemaps;

    [Header("Normal Pellet Tiles")]
    public TileBase[] pelletTiles;

    [Header("Power Pellets")]
    public SuperPellet[] powerPellets;

    [Header("Game References")]
    public MonkeyStudentController monkey;
    public CatMovement[] cats;
    public GameTimer gameTimer;

    [Header("Victory UI")]
    public TextMeshProUGUI victoryText;

    // Prevents the level from being completed more than once.
    private bool levelCompleted = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Set the existing VictoryText message.
        if (victoryText != null)
        {
            victoryText.text = "LEVEL COMPLETE!";
        }
    }

    // Called whenever the monkey collects a normal or Power Pellet.
    public void CheckForVictory()
    {
        // Prevent multiple victory triggers.
        if (levelCompleted)
        {
            return;
        }

        // Do not allow victory while the monkey is dying or after the game has ended.
        if (LifeManager.Instance != null &&
            (LifeManager.Instance.IsGameOver ||
             LifeManager.Instance.IsDying))
        {
            return;
        }

        // Count all remaining normal and Power Pellets.
        int normalPelletsRemaining = CountNormalPellets();
        int powerPelletsRemaining = CountPowerPellets();

        Debug.Log(
            "Normal Pellets Remaining: " + normalPelletsRemaining +
            " | Power Pellets Remaining: " + powerPelletsRemaining
        );

        // Both types of pellets must be completely collected.
        if (normalPelletsRemaining == 0 &&
            powerPelletsRemaining == 0)
        {
            CompleteLevel();
        }
    }

    // Counts all normal pellet tiles across the four Tilemaps.
    private int CountNormalPellets()
    {
        int count = 0;

        if (levelTilemaps == null || pelletTiles == null)
        {
            return count;
        }

        // Check every Tilemap.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Check every cell within the Tilemap.
            foreach (Vector3Int position in
                     tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(position);

                // Check whether this tile is a normal pellet.
                foreach (TileBase pelletTile in pelletTiles)
                {
                    if (pelletTile != null && tile == pelletTile)
                    {
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }

    // Counts the remaining animated Power Pellet GameObjects.
    private int CountPowerPellets()
    {
        int count = 0;

        if (powerPellets == null)
        {
            return count;
        }

        foreach (SuperPellet pellet in powerPellets)
        {
            // A Power Pellet only counts if it has not been collected.
            if (pellet != null && !pellet.IsCollected)
            {
                count++;
            }
        }

        return count;
    }

    // Called when all normal and Power Pellets are collected.
    private void CompleteLevel()
    {
        // Prevent the victory condition from running again.
        levelCompleted = true;

        // Stop monkey movement.
        if (monkey != null)
        {
            monkey.enabled = false;
        }

        // Stop movement for all four cats.
        if (cats != null)
        {
            foreach (CatMovement cat in cats)
            {
                if (cat != null)
                {
                    cat.enabled = false;
                }
            }
        }

        // Stop the timer and save the Best Time.
        if (gameTimer != null)
        {
            gameTimer.CompleteLevel();
        }

        // Update the existing VictoryText.
        if (victoryText != null)
        {
            victoryText.text = "LEVEL COMPLETE!";
        }

        // Activate the VictoryPanel
        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowVictory();
        }

        Debug.Log("LEVEL COMPLETE! All pellets collected!");
    }
}