using UnityEngine;
using UnityEngine.Tilemaps;

public class MonkeyStudentController : MonoBehaviour
{
    [Header("Maze Tilemaps")]
    public Tilemap[] levelTilemaps;

    [Header("Tile Types")]
    public TileBase[] wallTiles;
    public TileBase ghostWallTile;
    public TileBase[] pelletTiles;
    public TileBase emptyCorridorTile;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer monkeySprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pelletMoveSound;
    public AudioClip emptyMoveSound;

    [Header("Particles")]
    public ParticleSystem dustParticles;

    [Header("Wall Collision Effect")]
    public ParticleSystem wallCollisionEffect;
    public float wallEffectCooldown = 0.3f;

    [Header("Teleporters")]
    public Transform teleportLeft;
    public Transform teleportRight;
    public float teleportCooldown = 0.5f;

    // Stores the player's requested and current directions.
    private Vector2Int lastInput = Vector2Int.zero;
    private Vector2Int currentInput = Vector2Int.zero;

    // Positions used for grid-based movement.
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float moveProgress = 0f;
    private bool isMoving = false;

    // Death and respawning.
    private bool isDead = false;
    private Vector3 respawnPosition;

    // Reference Tilemap.
    private Tilemap gridTilemap;

    // Effect and teleport cooldowns.
    private float lastWallEffectTime = -10f;
    private float lastTeleportTime = -10f;

    void Start()
    {
        // Check that the reference Tilemap is assigned.
        if (levelTilemaps == null ||
            levelTilemaps.Length == 0 ||
            levelTilemaps[0] == null)
        {
            Debug.LogError(
                "Assign TilemapTopRight to Element 0!"
            );

            enabled = false;
            return;
        }

        gridTilemap = levelTilemaps[0];

        // Snap the monkey to the centre of its starting cell.
        Vector3Int startCell =
            gridTilemap.WorldToCell(transform.position);

        Vector3 centre =
            gridTilemap.GetCellCenterWorld(startCell);

        centre.z = transform.position.z;

        transform.position = centre;

        // Initialise movement and respawn positions.
        startPosition = transform.position;
        targetPosition = transform.position;
        respawnPosition = transform.position;

        isMoving = false;
        isDead = false;

        // Find the monkey's SpriteRenderer if needed.
        if (monkeySprite == null)
        {
            monkeySprite =
                GetComponentInChildren<SpriteRenderer>();
        }

        // Start facing right.
        transform.rotation = Quaternion.identity;

        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsMoving", false);
            animator.speed = 0f;
        }

        // Stop dust particles until movement begins.
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }

        if (emptyCorridorTile == null)
        {
            Debug.LogWarning(
                "Empty Corridor Tile is not assigned!"
            );
        }
    }

    void Update()
    {
        // Prevent movement during the death animation.
        if (isDead)
        {
            return;
        }

        // Read WASD input.
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

        // Continue moving towards the current destination.
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
        else if (CanMove(currentInput))
        {
            // Continue forward if the requested turn is blocked.
            StartMoving(currentInput);
        }
        else
        {
            // Stop when no valid movement is possible.
            StopMoving();

            // Only play the effect when an actual wall is ahead.
            if (lastInput != Vector2Int.zero &&
                IsWallInDirection(lastInput))
            {
                PlayWallCollisionEffect(lastInput);
            }
        }
    }

    // Begins travelling towards the next grid cell.
    void StartMoving(Vector2Int direction)
    {
        startPosition = transform.position;

        Vector3 nextPosition = startPosition +
            new Vector3(direction.x, direction.y, 0f);

        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        targetPosition =
            gridTilemap.GetCellCenterWorld(nextCell);

        targetPosition.z = startPosition.z;

        moveProgress = 0f;
        isMoving = true;

        FaceDirection(direction);

        // Play the directional movement animation.
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.speed = 1f;
        }

        PlayMovementSound(nextCell);

        // Start running dust particles.
        if (dustParticles != null &&
            !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }
    }

    // Moves smoothly between grid cells.
    void Move()
    {
        moveProgress += Time.deltaTime * moveSpeed;

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

            CollectRegularPellet();
        }
    }

    // Stops movement sounds, animation and particles.
    void StopMoving()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.speed = 0f;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (dustParticles != null &&
            dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }

    // Checks whether the next cell can be entered.
    bool CanMove(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return false;
        }

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

            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
            {
                continue;
            }

            // Block normal walls.
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

            // Block ghost-only walls.
            if (ghostWallTile != null &&
                tile == ghostWallTile)
            {
                return false;
            }

            foundWalkableTile = true;
        }

        if (foundWalkableTile)
        {
            return true;
        }

        // Allow movement into animated super pellet cells.
        GameObject[] superPellets =
            GameObject.FindGameObjectsWithTag("SuperPellet");

        foreach (GameObject pellet in superPellets)
        {
            Vector3Int pelletCell =
                gridTilemap.WorldToCell(
                    pellet.transform.position
                );

            if (pelletCell == nextCell)
            {
                return true;
            }
        }

        return false;
    }

    // Checks whether a blocked direction contains a wall.
    bool IsWallInDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return false;
        }

        Vector3 nextPosition = transform.position +
            new Vector3(direction.x, direction.y, 0f);

        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
            {
                continue;
            }

            // Detect normal walls.
            if (wallTiles != null)
            {
                foreach (TileBase wall in wallTiles)
                {
                    if (wall != null && tile == wall)
                    {
                        return true;
                    }
                }
            }

            // Detect ghost-only walls.
            if (ghostWallTile != null &&
                tile == ghostWallTile)
            {
                return true;
            }
        }

        return false;
    }

    // Creates a particle burst when the monkey hits a wall.
    void PlayWallCollisionEffect(Vector2Int direction)
    {
        if (wallCollisionEffect == null)
        {
            return;
        }

        // Limit how frequently the effect can appear.
        if (Time.time - lastWallEffectTime < wallEffectCooldown)
        {
            return;
        }

        lastWallEffectTime = Time.time;

        // Spawn the effect slightly in front of the monkey.
        Vector3 effectPosition = transform.position +
            new Vector3(direction.x, direction.y, 0f) * 0.5f;

        ParticleSystem effect = Instantiate(
            wallCollisionEffect,
            effectPosition,
            Quaternion.identity
        );

        effect.Play();

        Destroy(effect.gameObject, 2f);
    }

    // Updates the monkey's directional animation.
    void FaceDirection(Vector2Int direction)
    {
        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }

    // Plays the correct sound for the destination cell.
    void PlayMovementSound(Vector3Int nextCell)
    {
        if (audioSource == null)
        {
            return;
        }

        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool hasPellet = false;

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

        AudioClip selectedSound =
            hasPellet ? pelletMoveSound : emptyMoveSound;

        if (selectedSound == null)
        {
            audioSource.Stop();
            return;
        }

        if (audioSource.clip != selectedSound ||
            !audioSource.isPlaying)
        {
            audioSource.clip = selectedSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // Collects regular pellets and awards 10 points.
    void CollectRegularPellet()
    {
        Vector3Int currentCell =
            gridTilemap.WorldToCell(transform.position);

        Vector3 currentWorld =
            gridTilemap.GetCellCenterWorld(currentCell);

        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Vector3Int cell =
                tilemap.WorldToCell(currentWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null || pelletTiles == null)
            {
                continue;
            }

            foreach (TileBase pellet in pelletTiles)
            {
                if (pellet != null && tile == pellet)
                {
                    if (emptyCorridorTile == null)
                    {
                        Debug.LogError(
                            "Empty Corridor Tile is not assigned!"
                        );
                        return;
                    }

                    // Replace the pellet with a walkable corridor.
                    tilemap.SetTile(cell, emptyCorridorTile);

                    // Award points.
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(10);
                    }

                    Debug.Log("Regular Pellet Collected! +10");
                    return;
                }
            }
        }
    }

    // Stops the monkey and plays its death animation.
    public void PlayDeathAnimation()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        isMoving = false;
        moveProgress = 0f;

        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (dustParticles != null)
        {
            dustParticles.Stop();
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("MonkeyDeath", 0, 0f);
        }

        Debug.Log("Monkey death animation started!");
    }

    // Returns the monkey to its original starting position.
    public void RespawnMonkey()
    {
        transform.position = respawnPosition;

        startPosition = respawnPosition;
        targetPosition = respawnPosition;

        moveProgress = 0f;
        isMoving = false;

        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        transform.rotation = Quaternion.identity;

        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("MonkeyMovement", 0, 0f);

            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsMoving", false);

            animator.speed = 0f;
        }

        isDead = false;

        Debug.Log("Monkey respawned!");
    }

    // Teleports the monkey between the maze's left and right entrances.
    public void TeleportMonkey(bool teleportToRight)
    {
        // Do not teleport during the death animation.
        if (isDead)
        {
            return;
        }

        // Prevent rapid repeated teleportation.
        if (Time.time - lastTeleportTime < teleportCooldown)
        {
            return;
        }

        // Select the opposite entrance.
        Transform destination = teleportToRight
            ? teleportRight
            : teleportLeft;

        if (destination == null)
        {
            Debug.LogWarning(
                "Teleporter destination is not assigned!"
            );
            return;
        }

        lastTeleportTime = Time.time;

        // Place the monkey at the destination.
        Vector3 newPosition = destination.position;
        newPosition.z = transform.position.z;

        // Snap to the centre of the destination grid cell.
        if (gridTilemap != null)
        {
            Vector3Int destinationCell =
                gridTilemap.WorldToCell(newPosition);

            Vector3 cellCentre =
                gridTilemap.GetCellCenterWorld(destinationCell);

            cellCentre.z = newPosition.z;
            newPosition = cellCentre;
        }

        transform.position = newPosition;

        // Reset movement positions so Lerp does not pull
        // the monkey back to its previous location.
        startPosition = newPosition;
        targetPosition = newPosition;

        moveProgress = 0f;
        isMoving = false;

        // Clear stored movement directions.
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        // Stop movement effects until the player moves again.
        StopMoving();

        Debug.Log("Monkey teleported!");
    }
}