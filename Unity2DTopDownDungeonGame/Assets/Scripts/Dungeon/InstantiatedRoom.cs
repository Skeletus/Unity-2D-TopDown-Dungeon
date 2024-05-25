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
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // guardar limites del collider del room
        roomColliderBounds = boxCollider2D.bounds;
    }

    /// <summary>
    /// inicializar Instantiated Room
    /// </summary>
    /// <param name="roomGameObject"></param>
    public void Initialise(GameObject roomGameObject)
    {
        PopulateTileMapMemberVariables(roomGameObject);

        BlockOffUnusedDoorways();

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// bloquear doorways sin utilizar en el room
    /// </summary>
    private void BlockOffUnusedDoorways()
    {
        // recorrer todas las doorways
        foreach(Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }

            // bloquear doorways conectadas usando tiles en tilemaps
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
    /// bloquear un doorway en el layer de tilemap
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
    /// bloquear doorway verticalmente - para doorways EAST y WEAST
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // recorrer todas las tiles a copiar
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // capturar la rotacion del tile que esta siendo copiado
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int
                    (startPosition.x + xPos,
                    startPosition.y - yPos,
                    0)
                    );

                // copiar tile
                Vector3Int positionTile = new Vector3Int(
                    startPosition.x + xPos,
                    startPosition.y - 1 - yPos,
                    0);
                tilemap.SetTile(positionTile, tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // establecer rotacion del tile copiado
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// bloquear doorway horizontalmente - para doorways NORTH y SOUTH
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // recorrer todas las tiles a copiar
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // capturar la rotacion del tile que esta siendo copiado
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int
                    (startPosition.x + xPos,
                    startPosition.y - yPos,
                    0)
                    );

                // copiar tile
                Vector3Int positionTile = new Vector3Int(
                    startPosition.x + 1 + xPos,
                    startPosition.y - yPos,
                    0);
                tilemap.SetTile(positionTile, tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // establecer rotacion del tile copiado
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix );
            }
        }
    }

    /// <summary>
    /// desactivar collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        // desactiva la collision tilemap render
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    /// <summary>
    /// llenar las variables el tilemap y el grid (cuadricula)
    /// </summary>
    /// <param name="roomGameObject"></param>
    private void PopulateTileMapMemberVariables(GameObject roomGameObject)
    {
        // capturar el grid component
        // get the grid Component
        grid = roomGameObject.GetComponentInChildren<Grid>();

        // capturar los tilemaps de los hijos
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
}
