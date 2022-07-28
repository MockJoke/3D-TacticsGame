using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Responsible for generation of the Grid Map
public class MapGenerator : MonoBehaviour
{
    // List of tiles that are used to generate the map
    [Header("Tiles")] 
    public TileType[] tileTypes;
    public int[,] Tiles;

    // Node graph for path finding purposes
    public Node[,] Graph;
    
    [HideInInspector] public int mapSizeX = 10;
    [HideInInspector] public int mapSizeY = 10;

    // Parent GameObjects (Containers) for the tiles
    [Header("Containers")] 
    public GameObject tileContainer;
    public GameObject uiQuadPossibleMovesContainer;
    public GameObject uiQuadCursorPointsContainer;
    public GameObject uiCharMovementIndicatorContainer;

    // 2D array list of tile gameobjects on the board
    public GameObject[,] TilesOnMap;

    [Header("mapUI Objects")]
    // Gameobject that's used to overlay onto the tiles to show possible movements
    public GameObject mapUI;
    //Game object that is used to highlight the mouse location
    public GameObject mapCursorUI;
    //Game object that is used to highlight the path the unit is taking
    public GameObject mapCharMovementUI;

    // 2D array list of quadUI gameobjects on the board
    public GameObject[,] QuadOnMap;
    public GameObject[,] QuadOnMapForCharMovement;
    public GameObject[,] QuadOnMapCursor;

    [HideInInspector] 
    public int ObstaclePosX;
    public int obstaclePosY;
    public int obstacleTt; 

    void Start()
    {
        MapManager mapManager = GetComponent<MapManager>();
        GameManager gameManager = GetComponent<GameManager>();
        // Generate the map info that will be used
        GenerateMapInfo();
        // Generate path finding graph
        GeneratePathfindingGraph();
        // With the generated info this function will read the info and produce the map
        GenerateMapVisuals();
        // Check if there're any pre-existing chars on the board
        mapManager.SetIfTileIsOccupied();
    }

    // Set the tiles[x,y] to the corresponding tile
    private void GenerateMapInfo()
    {
        // Allocate map tiles
        Tiles = new int[mapSizeX, mapSizeY];

        // Initialise map tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Tiles[x, y] = 0;
            }
        }

        Tiles[2, 7] = 3;
        Tiles[3, 7] = 3;

        Tiles[6, 7] = 3;
        Tiles[7, 7] = 3;

        Tiles[2, 2] = 3;
        Tiles[3, 2] = 3;

        Tiles[6, 2] = 3;
        Tiles[7, 2] = 3;

        Tiles[0, 3] = 1;
        Tiles[1, 3] = 1;
        Tiles[0, 2] = 1;
        Tiles[1, 2] = 1;

        Tiles[0, 6] = 1;
        Tiles[1, 6] = 1;
        Tiles[2, 6] = 1;
        Tiles[0, 7] = 1;
        Tiles[1, 7] = 1;

        Tiles[2, 3] = 1;
        Tiles[0, 4] = 1;
        Tiles[0, 5] = 1;
        Tiles[1, 4] = 1;
        Tiles[1, 5] = 1;
        Tiles[2, 4] = 1;
        Tiles[2, 5] = 1;

        Tiles[4, 4] = 2;
        Tiles[5, 4] = 2;
        Tiles[4, 5] = 2;
        Tiles[5, 5] = 2;

        Tiles[7, 3] = 1;
        Tiles[8, 3] = 1;
        Tiles[9, 3] = 1;
        Tiles[8, 2] = 1;
        Tiles[9, 2] = 1;
        Tiles[7, 4] = 1;
        Tiles[7, 5] = 1;
        Tiles[7, 6] = 1;
        Tiles[8, 6] = 1;
        Tiles[9, 6] = 1;
        Tiles[8, 7] = 1;
        Tiles[9, 7] = 1;
        Tiles[8, 4] = 1;
        Tiles[8, 5] = 1;
        Tiles[9, 4] = 1;
        Tiles[9, 5] = 1;
    }

    // Creates the graph for pathfinding & sets up the neighbours
    private void GeneratePathfindingGraph()
    {
        Graph = new Node[mapSizeX, mapSizeY];

        // Initialise the graph
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Graph[x, y] = new Node();
                Graph[x, y].X = x;
                Graph[x, y].Y = y;
            }
        }

        // Calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (x > 0)
                {
                    Graph[x, y].Neighbours.Add(Graph[x - 1, y]);
                }

                if (x < mapSizeX - 1)
                {
                    Graph[x, y].Neighbours.Add(Graph[x + 1, y]);
                }

                if (y > 0)
                {
                    Graph[x, y].Neighbours.Add(Graph[x, y - 1]);
                }

                if (y < mapSizeY - 1)
                {
                    Graph[x, y].Neighbours.Add(Graph[x, y + 1]);
                }
            }
        }
    }

    // Instantiates the map tiles 
    private void GenerateMapVisuals()
    {
        // Generate list of actual tileGameObjects
        TilesOnMap = new GameObject[mapSizeX, mapSizeY];
        QuadOnMap = new GameObject[mapSizeX, mapSizeY];
        QuadOnMapForCharMovement = new GameObject[mapSizeX, mapSizeY];
        QuadOnMapCursor = new GameObject[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                int index = Tiles[x, y];
                GameObject newTile =
                    Instantiate(tileTypes[index].tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<Tile>().tileX = x;
                newTile.GetComponent<Tile>().tileY = y;
                newTile.GetComponent<Tile>().map = this;
                newTile.transform.SetParent(tileContainer.transform);
                TilesOnMap[x, y] = newTile;

                GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(uiQuadPossibleMovesContainer.transform);
                QuadOnMap[x, y] = gridUI;

                GameObject gridUIForPathFindingDisplay = Instantiate(mapCharMovementUI, new Vector3(x, 0.502f, y),
                    Quaternion.Euler(90f, 0, 0));
                gridUIForPathFindingDisplay.transform.SetParent(uiCharMovementIndicatorContainer.transform);
                QuadOnMapForCharMovement[x, y] = gridUIForPathFindingDisplay;

                GameObject gridUICursor =
                    Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(uiQuadCursorPointsContainer.transform);
                QuadOnMapCursor[x, y] = gridUICursor;
            }
        }
    }
    
    // In: x & y coordinates of a tile  || Out: returns a vector3 of the tile in world space, they're 0.75f off to zero in y-dir
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }
}
