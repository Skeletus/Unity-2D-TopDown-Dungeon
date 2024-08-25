using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty;  // use this 2d array to store movement penalties from the tilemaps to be used in AStar pathfinding
    [HideInInspector] public int[,] aStarItemObstacles; // use to store position of moveable items that are obstacles
    [HideInInspector] public Bounds roomColliderBounds;
    [HideInInspector] public List<MoveItem> moveableItemsList = new List<MoveItem>();

    #region Header OBJECT REFERENCES

    [Space(10)]
    [Header("OBJECT REFERENCES")]

    #endregion Header OBJECT REFERENCES

    #region Tooltip

    [Tooltip("Populate with the environment child placeholder gameobject ")]

    #endregion Tooltip

    [SerializeField] private GameObject environmentGameObject;


    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    private void Start()
    {
        // Update moveable item obstacles array
        UpdateMoveableObstacles();
    }

    // Trigger room changed event when player enters a room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the player triggered the collider
        if (collision.tag == Settings.playerTag &&
            room != GameManager.Instance.GetCurrentRoom())
        {
            // set room as visited
            this.room.isPreviouslyVisited = true;

            // call the room changed event
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    /// <summary>
    /// Initialise the instatiated room
    /// </summary>
    /// <param name="roomGameObject"></param>
    public void Initialise(GameObject roomGameObject)
    {
        PopulateTileMapMemberVariables(roomGameObject);

        BlockOffUnusedDoorways();

        AddObstaclesAndPreferredPaths();

        CreateItemObstaclesArray();

        AddDoorsToRooms();

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// Update obstacles used by AStar pathfinmding.
    /// </summary>
    private void AddObstaclesAndPreferredPaths()
    {
        // this array will be populated with wall obstacles 
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];


        // Loop thorugh all grid squares
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                // Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                // Add preferred path for enemies (1 is the preferred path value, default value for
                // a grid location is specified in the Settings).
                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }

            }
        }

    }

    /// <summary>
    /// Add openning doors if is not a corridor room
    /// </summary>
    private void AddDoorsToRooms()
    {
        // if the room is a corridor then return
        if (room.roomNodeType.isCorridorNS || room.roomNodeType.isCorridorEW)
        {
            return;
        }

        // Instantiate door prefabs at doorway position
        foreach(Doorway doorway in room.doorwayList)
        {
            // if the doorway prefab isn't null and the doorway is connected
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                if (doorway.orientation == Orientation.north)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);

                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance/2f,
                        doorway.position.y + tileDistance,
                        0f);
                }
                else if(doorway.orientation == Orientation.south)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);

                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance / 2f,
                        doorway.position.y,
                        0f);
                }
                else if (doorway.orientation == Orientation.east)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);

                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance,
                        doorway.position.y + tileDistance * 1.25f,
                        0f);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    // create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);

                    door.transform.localPosition = new Vector3(
                        doorway.position.x,
                        doorway.position.y + tileDistance * 1.25f,
                        0f);
                }

                // get door component
                Door doorComponent = door.GetComponent<Door>();

                // set if door is part of a boss room
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;

                    // lock the door to prevent access to the room
                    doorComponent.LockDoor();
                }
            }
        }
    }

    /// <summary>
    /// Block off unused doorways in the room
    /// </summary>
    private void BlockOffUnusedDoorways()
    {
        // loop through all doorways
        foreach(Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }

            // block connected doorways using tiles on tilemaps
            if (collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }
            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }
            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }
            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }
            if (decoration2Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }
            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Bloack a doorway on tile map layer
    /// </summary>
    /// <param name="collisionTilemap"></param>
    /// <param name="doorway"></param>
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch(doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
            case Orientation.none:
                break;
        }
    }

    /// <summary>
    /// Block doorway vertically - for east and west doorways
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int
                    (startPosition.x + xPos,
                    startPosition.y - yPos,
                    0)
                    );

                // copy tile
                Vector3Int positionTile = new Vector3Int(
                    startPosition.x + xPos,
                    startPosition.y - 1 - yPos,
                    0);
                tilemap.SetTile(positionTile, tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Block doorway horizontally - for north and south doorways
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int
                    (startPosition.x + xPos,
                    startPosition.y - yPos,
                    0)
                    );

                // copy tile
                Vector3Int positionTile = new Vector3Int(
                    startPosition.x + 1 + xPos,
                    startPosition.y - yPos,
                    0);
                tilemap.SetTile(positionTile, tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix );
            }
        }
    }

    /// <summary>
    /// Disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        // disable collision tilemap render
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    /// <summary>
    /// populate the tilemap and grid variables
    /// </summary>
    /// <param name="roomGameObject"></param>
    private void PopulateTileMapMemberVariables(GameObject roomGameObject)
    {
        // get the grid Component
        grid = roomGameObject.GetComponentInChildren<Grid>();

        // get tilemaps in children
        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach(Tilemap tilemap in tilemaps) 
        {
            if (tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }
        }
    }

    /// <summary>
    /// Disable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    /// <summary>
    /// Enable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    public void ActivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(true);
    }

    public void DeactivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(false);
    }

    /// <summary>
    /// Lock the room doors
    /// </summary>
    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        // Trigger lock doors
        foreach (Door door in doorArray)
        {
            door.LockDoor();
        }

        // Disable room trigger collider
        DisableRoomCollider();
    }

    /// <summary>
    /// Unlock the room doors
    /// </summary>
    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    /// <summary>
    /// Unlock the room doors routine
    /// </summary>
    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);

        Door[] doorArray = GetComponentsInChildren<Door>();

        // Trigger open doors
        foreach (Door door in doorArray)
        {
            door.UnlockDoor();
        }

        // Enable room trigger collider
        EnableRoomCollider();
    }

    /// <summary>
    /// Create Item Obstacles Array
    /// </summary>
    private void CreateItemObstaclesArray()
    {
        // this array will be populated during gameplay with any moveable obstacles
        aStarItemObstacles = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];
    }

    /// <summary>
    /// Update the array of moveable obstacles
    /// </summary>
    public void UpdateMoveableObstacles()
    {
        InitializeItemObstaclesArray();

        foreach (MoveItem moveItem in moveableItemsList)
        {
            Vector3Int colliderBoundsMin = grid.WorldToCell(moveItem.boxCollider2D.bounds.min);
            Vector3Int colliderBoundsMax = grid.WorldToCell(moveItem.boxCollider2D.bounds.max);

            // Loop through and add moveable item collider bounds to obstacle array
            for (int i = colliderBoundsMin.x; i <= colliderBoundsMax.x; i++)
            {
                for (int j = colliderBoundsMin.y; j <= colliderBoundsMax.y; j++)
                {
                    aStarItemObstacles[i - room.templateLowerBounds.x, j - room.templateLowerBounds.y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Initialize Item Obstacles Array With Default AStar Movement Penalty Values
    /// </summary>
    private void InitializeItemObstaclesArray()
    {
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarItemObstacles[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(environmentGameObject), environmentGameObject);
    }

#endif

    #endregion Validation
}
