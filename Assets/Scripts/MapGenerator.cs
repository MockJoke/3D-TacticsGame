using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Responsible for generation of the Grid Map
public class MapGenerator : MonoBehaviour
{   
    // List of tiles that are used to generate the map
    [Header("Tiles")] 
    public TileType[] tileTypes;
    public int[,] tiles;
    
    // Node graph for path finding purposes
    public Node[,] Graph; 
 
    [SerializeField]
    private int mapSizeX = 10;
    [SerializeField]
    private int mapSizeY = 10;
    
    // Parent GameObjects (Containers) for the tiles
    [Header("Containers")] 
    public GameObject tileContainer;
    public GameObject UIQuadPossibleMovesContainer;
    public GameObject UIQuadCursorPointsContainer;
    public GameObject UICharMovementIndicatorContainer;
    
    // 2D array list of tile gameobjects on the board
    public GameObject[,] tilesOnMap;
    
    [Header("mapUI Objects")]
    // Gameobject that's used to overlay onto the tiles to show possible movements
    public GameObject mapUI;
    //Game object that is used to highlight the mouse location
    public GameObject mapCursorUI;
    //Game object that is used to highlight the path the unit is taking
    public GameObject mapCharMovementUI;
    
    // 2D array list of quadUI gameobjects on the board
    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForCharMovement;
    public GameObject[,] quadOnMapCursor;

    void Start()
    {
        // Generate the map info that will be used
        GenerateMapInfo();
        // Generate path finding graph
        GeneratePathfindingGraph();
        // With the generated info this function will read the info and produce the map
        GenerateMapVisuals();
    }
    
    // Set the tiles[x,y] to the corresponding tile
    private void GenerateMapInfo()
    {
        // Allocate map tiles
        tiles = new int[mapSizeX, mapSizeY];
        
        // Initialise map tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0; 
            }
        }
        
        tiles[2, 7] = 3;
        tiles[3, 7] = 3;
       
        tiles[6, 7] = 3;
        tiles[7, 7] = 3;

        tiles[2, 2] = 3;
        tiles[3, 2] = 3;
       
        tiles[6, 2] = 3;
        tiles[7, 2] = 3;

        tiles[0, 3] = 1;
        tiles[1, 3] = 1;
        tiles[0, 2] = 1;
        tiles[1, 2] = 1;

        tiles[0, 6] = 1;
        tiles[1, 6] = 1;
        tiles[2, 6] = 1;
        tiles[0, 7] = 1;
        tiles[1, 7] = 1;

        tiles[2, 3] = 1;
        tiles[0, 4] = 1;
        tiles[0, 5] = 1;
        tiles[1, 4] = 1;
        tiles[1, 5] = 1;
        tiles[2, 4] = 1;
        tiles[2, 5] = 1;

        tiles[4, 4] = 2;
        tiles[5, 4] = 2;
        tiles[4, 5] = 2;
        tiles[5, 5] = 2;

        tiles[7, 3] = 1;
        tiles[8, 3] = 1;
        tiles[9, 3] = 1;
        tiles[8, 2] = 1;
        tiles[9, 2] = 1;
        tiles[7, 4] = 1;
        tiles[7, 5] = 1;
        tiles[7, 6] = 1;
        tiles[8, 6] = 1;
        tiles[9, 6] = 1;
        tiles[8, 7] = 1;
        tiles[9, 7] = 1;
        tiles[8, 4] = 1;
        tiles[8, 5] = 1;
        tiles[9, 4] = 1;
        tiles[9, 5] = 1;
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
                Graph[x, y].x = x;
                Graph[x, y].y = y;
            }
        }
        
        // Calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                if (x < 0)
                {
                    Graph[x,y].Neighbours.Add(Graph[x-1, y]);
                }
                if (x < mapSizeX-1)
                {
                    Graph[x,y].Neighbours.Add(Graph[x+1, y]);
                }
                if (y < 0)
                {
                    Graph[x,y].Neighbours.Add(Graph[x, y-1]);
                }
                if (y < mapSizeY-1)
                {
                    Graph[x,y].Neighbours.Add(Graph[x, y+1]);
                }
            }
        }
    }
    // Instantiates the map tiles 
    private void GenerateMapVisuals()
    {
        // Generate list of actual tileGameObjects
        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMapForCharMovement = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
        
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                int index = tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<Tile>().tileX = x;
                newTile.GetComponent<Tile>().tileY = y;
                newTile.GetComponent<Tile>().map = this; 
                newTile.transform.SetParent(tileContainer.transform);
                tilesOnMap[x, y] = newTile;

                GameObject gridUI = Instantiate(mapUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIQuadPossibleMovesContainer.transform);
                quadOnMap[x, y] = gridUI;

                GameObject gridUIForPathFindingDisplay = Instantiate(mapCharMovementUI, new Vector3(x, 0.502f, y),Quaternion.Euler(90f, 0, 0));
                gridUIForPathFindingDisplay.transform.SetParent(UICharMovementIndicatorContainer.transform);
                quadOnMapForCharMovement[x, y] = gridUIForPathFindingDisplay;

                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIQuadCursorPointsContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor; 
            }
        }
    }
    void Update()
    {
        
    }
}
