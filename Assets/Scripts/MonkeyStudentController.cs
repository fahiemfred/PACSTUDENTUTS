using UnityEngine;
using UnityEngine.Tilemaps;

public class MonkeyStudentController : MonoBehaviour
{
    // The four Tilemaps used to build the maze.
    [Header("Maze Tilemaps")]
    public Tilemap[] levelTilemaps;

    // Tile assets used for walls, pellets and empty corridors.
    [Header("Tile Types")]
    public TileBase[] wallTiles;
    public TileBase ghostWallTile;
    public TileBase[] pelletTiles;

    // Replaces a regular pellet after it has been collected.
    public TileBase emptyCorridorTile;

    // Number of grid cells travelled per second.
    [Header("Movement")]
    public float moveSpeed = 5f;

    // Controls the monkey's directional animations.
    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer monkeySprite;

    // Audio for travelling through different corridor types.
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pelletMoveSound;
    public AudioClip emptyMoveSound;

    // Custom running dust effect.
    [Header("Particles")]
    public ParticleSystem dustParticles;

    // Stores the player's most recent direction input.
    private Vector2Int lastInput = Vector2Int.zero;

    // Stores the direction the monkey is currently travelling.
    private Vector2Int currentInput = Vector2Int.zero;

    // Positions used to move between neighbouring cells.
    private Vector3 startPosition;
    private Vector3 targetPosition;

    // Tracks progress between the current and destination cells.
    private float moveProgress = 0f;

    // Prevents movement commands from interrupting a tile transition.
    private bool isMoving = false;

    // Uses the unmirrored Tilemap as the reference grid.
    private Tilemap gridTilemap;

    void Start()
    {
        // Make sure the first Tilemap has been assigned.
        if (levelTilemaps == null ||
            levelTilemaps.Length == 0 ||
            levelTilemaps[0] == null)
        {
            Debug.LogError("Assign TilemapTopRight to Element 0!");

            enabled = false;
            return;
        }

        // Use the first Tilemap for grid calculations.
        gridTilemap = levelTilemaps[0];

        // Find the cell containing the monkey's starting position.
        Vector3Int startCell =
            gridTilemap.WorldToCell(transform.position);

        // Position the monkey in the centre of its starting cell.
        Vector3 centre =
            gridTilemap.GetCellCenterWorld(startCell);

        centre.z = transform.position.z;
        transform.position = centre;

        // Initialise the movement positions.
        startPosition = transform.position;
        targetPosition = transform.position;

        // The monkey begins stationary.
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        // Find the SpriteRenderer if it has not been assigned.
        if (monkeySprite == null)
        {
            monkeySprite =
                GetComponentInChildren<SpriteRenderer>();
        }

        // Keep the monkey upright without sprite flipping.
        transform.rotation = Quaternion.identity;

        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        // Begin facing right with the animation paused.
        if (animator != null)
        {
            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsMoving", false);

            animator.speed = 0f;
        }

        // Prevent dust particles from playing at the start.
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }

        // Warn if the empty corridor tile has not been assigned.
        if (emptyCorridorTile == null)
        {
            Debug.LogWarning(
                "Assign an Empty Corridor Tile to the MonkeyStudentController!"
            );
        }
    }

    void Update()
    {
        // Remember the most recently pressed WASD direction.
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = Vector2Int.right;
        }

        // Finish travelling to the next cell before changing direction.
        if (isMoving)
        {
            Move();
            return;
        }

        // Prioritise the most recently requested direction.
        if (CanMove(lastInput))
        {
            currentInput = lastInput;
            StartMoving(currentInput);
        }

        // Continue in the current direction if the new direction is blocked.
        else if (CanMove(currentInput))
        {
            StartMoving(currentInput);
        }

        // Stop when neither direction is available.
        else
        {
            StopMoving();
        }
    }

    void StartMoving(Vector2Int direction)
    {
        // Save the monkey's current world position.
        startPosition = transform.position;

        // Calculate the next position in world coordinates.
        // This supports the four mirrored Tilemaps.
        Vector3 nextPosition = startPosition +
            new Vector3(direction.x, direction.y, 0f);

        // Convert the destination into a grid cell.
        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        // Find the exact centre of the destination cell.
        targetPosition =
            gridTilemap.GetCellCenterWorld(nextCell);

        targetPosition.z = startPosition.z;

        // Begin a new tile movement.
        moveProgress = 0f;
        isMoving = true;

        // Update the directional Blend Tree.
        FaceDirection(direction);

        // Play the appropriate movement animation.
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.speed = 1f;
        }

        // Play the correct movement sound.
        PlayMovementSound(nextCell);

        // Start producing dust particles.
        if (dustParticles != null &&
            !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }
    }

    void Move()
    {
        // Increase movement progress using elapsed time.
        moveProgress += Time.deltaTime * moveSpeed;

        // Move linearly between the starting and destination cells.
        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            Mathf.Clamp01(moveProgress)
        );

        // Finish the movement when the destination is reached.
        if (moveProgress >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;

            // Check for a regular pellet at the destination.
            CollectRegularPellet();
        }
    }

    void StopMoving()
    {
        // Pause the current directional animation.
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.speed = 0f;
        }

        // Stop the movement sound.
        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Stop emitting dust particles.
        if (dustParticles != null &&
            dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }

    bool CanMove(Vector2Int direction)
    {
        // Movement is not possible without a direction.
        if (direction == Vector2Int.zero)
        {
            return false;
        }

        // Calculate the destination in world space.
        Vector3 nextPosition = transform.position +
            new Vector3(direction.x, direction.y, 0f);

        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool foundWalkableTile = false;

        // Check every Tilemap at the destination position.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Convert the destination to this Tilemap's local grid.
            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
            {
                continue;
            }

            // Normal wall tiles always block movement.
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

            // Ghost wall tiles also block the monkey.
            if (ghostWallTile != null &&
                tile == ghostWallTile)
            {
                return false;
            }

            // A tile exists that is not a wall.
            foundWalkableTile = true;
        }

        // Allow movement through regular corridors.
        if (foundWalkableTile)
        {
            return true;
        }

        // Check animated super pellets stored as separate GameObjects.
        GameObject[] superPellets =
            GameObject.FindGameObjectsWithTag("SuperPellet");

        foreach (GameObject pellet in superPellets)
        {
            // Find the reference grid cell containing this super pellet.
            Vector3Int pelletCell =
                gridTilemap.WorldToCell(
                    pellet.transform.position
                );

            // Allow the monkey to enter a super pellet cell.
            if (pelletCell == nextCell)
            {
                return true;
            }
        }

        // Prevent movement outside valid maze cells.
        return false;
    }

    void FaceDirection(Vector2Int direction)
    {
        // Stops flipping
        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        // Update the Blend Tree parameters.
        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }

    void PlayMovementSound(Vector3Int nextCell)
    {
        // Do nothing if the AudioSource has not been assigned.
        if (audioSource == null)
        {
            return;
        }

        // Find the world position of the next cell.
        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool hasPellet = false;

        // Check the Tilemaps for a regular pellet.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null || pelletTiles == null)
            {
                continue;
            }

            // Check whether the tile is one of the regular pellet types.
            foreach (TileBase pellet in pelletTiles)
            {
                if (pellet != null && tile == pellet)
                {
                    hasPellet = true;
                    break;
                }
            }

            if (hasPellet)
            {
                break;
            }
        }

        // Choose the pellet sound or empty corridor sound.
        AudioClip selectedSound =
            hasPellet ? pelletMoveSound : emptyMoveSound;

        // Stop audio if the selected clip is missing.
        if (selectedSound == null)
        {
            audioSource.Stop();
            return;
        }

        // Avoid restarting the same clip unnecessarily.
        if (audioSource.clip != selectedSound ||
            !audioSource.isPlaying)
        {
            audioSource.clip = selectedSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void CollectRegularPellet()
    {
        // Find the monkey's current reference grid cell.
        Vector3Int currentCell =
            gridTilemap.WorldToCell(transform.position);

        Vector3 currentWorld =
            gridTilemap.GetCellCenterWorld(currentCell);

        // Search all four Tilemaps for a regular pellet.
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Find the corresponding cell on this Tilemap.
            Vector3Int cell =
                tilemap.WorldToCell(currentWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null || pelletTiles == null)
            {
                continue;
            }

            // Compare the current tile with the assigned pellet tiles.
            foreach (TileBase pellet in pelletTiles)
            {
                if (pellet != null && tile == pellet)
                {
                    // Replace the collected pellet with an empty corridor tile. This keeps the cell walkable after collection.
                    if (emptyCorridorTile == null)
                    {
                        Debug.LogError(
                            "Cannot collect pellet: Empty Corridor Tile is not assigned!"
                        );

                        return;
                    }

                    tilemap.SetTile(cell, emptyCorridorTile);

                    // Award 10 points.
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(10);
                    }

                    Debug.Log("Regular Pellet Collected! +10");

                    // Stop after collecting this pellet.
                    return;
                }
            }
        }
    }
}