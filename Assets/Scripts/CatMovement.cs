using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CatMovement : MonoBehaviour
{
    [Header("Maze Tilemaps")]

    // The four Tilemaps that make up the maze.
    public Tilemap[] levelTilemaps;

    [Header("Tile Types")]

    // Normal wall tiles that cats cannot pass through.
    public TileBase[] wallTiles;

    [Header("Movement Settings")]

    // Number of grid cells travelled per second.
    public float moveSpeed = 3f;

    // Probability of choosing a different direction.
    [Range(0f, 1f)]
    public float randomTurnChance = 0.5f;

    [Header("Animation")]

    // The Animator on this cat.
    public Animator animator;

    // Reference Tilemap for calculating grid positions.
    private Tilemap gridTilemap;

    // Current movement direction.
    private Vector2Int currentDirection = Vector2Int.zero;

    // Positions used for smooth movement.
    private Vector3 startPosition;
    private Vector3 targetPosition;

    // Progress between grid cells.
    private float moveProgress = 0f;

    // Whether the cat is currently moving.
    private bool isMoving = false;

    // Four possible directions.
    private readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    void Start()
    {
        // Make sure a reference Tilemap is assigned.
        if (levelTilemaps == null ||
            levelTilemaps.Length == 0 ||
            levelTilemaps[0] == null)
        {
            Debug.LogError(
                "CatMovement: Assign TilemapTopRight to Element 0!"
            );

            enabled = false;
            return;
        }

        // Use the first Tilemap as the reference grid.
        gridTilemap = levelTilemaps[0];

        // Snap the cat to the centre of its starting cell.
        Vector3Int startCell =
            gridTilemap.WorldToCell(transform.position);

        Vector3 centre =
            gridTilemap.GetCellCenterWorld(startCell);

        centre.z = transform.position.z;

        transform.position = centre;

        // Initialise movement positions.
        startPosition = transform.position;
        targetPosition = transform.position;

        isMoving = false;

        // Automatically find the Animator if not assigned.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Start facing right.
        if (animator != null)
        {
            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsMoving", false);

            // Pause animation until movement begins.
            animator.speed = 0f;
        }
    }

    void Update()
    {
        // Continue travelling towards the destination.
        if (isMoving)
        {
            Move();
            return;
        }

        // Choose a new direction at the current grid cell.
        ChooseDirection();

        // Start moving if a valid direction exists.
        if (currentDirection != Vector2Int.zero)
        {
            StartMoving();
        }
    }

    // Finds the available directions and selects one.
    void ChooseDirection()
    {
        List<Vector2Int> availableDirections =
            new List<Vector2Int>();

        // Check all four directions.
        foreach (Vector2Int direction in directions)
        {
            if (CanMove(direction))
            {
                availableDirections.Add(direction);
            }
        }

        // Stop if there are no available directions.
        if (availableDirections.Count == 0)
        {
            currentDirection = Vector2Int.zero;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.speed = 0f;
            }

            return;
        }

        // Continue forward most of the time.
        if (availableDirections.Contains(currentDirection) &&
            Random.value > randomTurnChance)
        {
            return;
        }

        // Avoid immediately reversing direction unless necessary.
        Vector2Int reverseDirection = -currentDirection;

        if (availableDirections.Count > 1)
        {
            availableDirections.Remove(reverseDirection);
        }

        // Choose a random available direction.
        int randomIndex =
            Random.Range(0, availableDirections.Count);

        currentDirection = availableDirections[randomIndex];
    }

    // Checks whether the next grid cell is walkable.
    bool CanMove(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return false;
        }

        // Calculate the next world position.
        Vector3 nextPosition = transform.position +
            new Vector3(direction.x, direction.y, 0f);

        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool foundWalkableTile = false;

        // Check all four mirrored Tilemaps.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Convert the world position into this Tilemap's cell.
            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
            {
                continue;
            }

            // Check whether the tile is a normal wall.
            if (wallTiles != null)
            {
                foreach (TileBase wall in wallTiles)
                {
                    if (wall != null && tile == wall)
                    {
                        return false;
                    }
                }
            }

            // Any non-wall tile is walkable.
            foundWalkableTile = true;
        }

        return foundWalkableTile;
    }

    // Starts moving to the next grid cell.
    void StartMoving()
    {
        startPosition = transform.position;

        // Calculate the next position.
        Vector3 nextPosition = startPosition +
            new Vector3(
                currentDirection.x,
                currentDirection.y,
                0f
            );

        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        targetPosition =
            gridTilemap.GetCellCenterWorld(nextCell);

        targetPosition.z = startPosition.z;

        // Begin smooth movement.
        moveProgress = 0f;
        isMoving = true;

        // Play the animation matching the movement direction.
        if (animator != null)
        {
            animator.SetFloat("MoveX", currentDirection.x);
            animator.SetFloat("MoveY", currentDirection.y);
            animator.SetBool("IsMoving", true);

            animator.speed = 1f;
        }
    }

    // Moves the cat smoothly between grid cells.
    void Move()
    {
        // Advance movement using elapsed time.
        moveProgress += Time.deltaTime * moveSpeed;

        // Interpolate between grid cells.
        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            Mathf.Clamp01(moveProgress)
        );

        // Finish movement when the destination is reached.
        if (moveProgress >= 1f)
        {
            transform.position = targetPosition;

            isMoving = false;
            moveProgress = 0f;
        }
    }
}