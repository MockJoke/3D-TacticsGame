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
    private int[,] _tiles;
    
    // Node graph for path finding purposes
    public Node[,] Graph; 
 
    [SerializeField]
    private int mapSizeX = 10;
    [SerializeField]
    private int mapSizeY = 10;
    
    // Parent GameObjects (Containers) for the tiles
    [Header("Containers")] 
    public GameObject tileContainer;
    
    void Start()
    {
        //Generate the map info that will be used
        GenerateMapInfo();
        //With the generated info this function will read the info and produce the map
        GenerateMapVisuals();
    }
    
    // Set the tiles[x,y] to the corresponding tile
    private void GenerateMapInfo()
    {
        // Allocate map tiles
        _tiles = new int[mapSizeX, mapSizeY];
        
        // Initialise map tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                _tiles[x, y] = 0; 
            }
        }
        
        _tiles[2, 7] = 3;
        _tiles[3, 7] = 3;
       
        _tiles[6, 7] = 3;
        _tiles[7, 7] = 3;

        _tiles[2, 2] = 3;
        _tiles[3, 2] = 3;
       
        _tiles[6, 2] = 3;
        _tiles[7, 2] = 3;

        _tiles[0, 3] = 1;
        _tiles[1, 3] = 1;
        _tiles[0, 2] = 1;
        _tiles[1, 2] = 1;

        _tiles[0, 6] = 1;
        _tiles[1, 6] = 1;
        _tiles[2, 6] = 1;
        _tiles[0, 7] = 1;
        _tiles[1, 7] = 1;

        _tiles[2, 3] = 1;
        _tiles[0, 4] = 1;
        _tiles[0, 5] = 1;
        _tiles[1, 4] = 1;
        _tiles[1, 5] = 1;
        _tiles[2, 4] = 1;
        _tiles[2, 5] = 1;

        _tiles[4, 4] = 2;
        _tiles[5, 4] = 2;
        _tiles[4, 5] = 2;
        _tiles[5, 5] = 2;

        _tiles[7, 3] = 1;
        _tiles[8, 3] = 1;
        _tiles[9, 3] = 1;
        _tiles[8, 2] = 1;
        _tiles[9, 2] = 1;
        _tiles[7, 4] = 1;
        _tiles[7, 5] = 1;
        _tiles[7, 6] = 1;
        _tiles[8, 6] = 1;
        _tiles[9, 6] = 1;
        _tiles[8, 7] = 1;
        _tiles[9, 7] = 1;
        _tiles[8, 4] = 1;
        _tiles[8, 5] = 1;
        _tiles[9, 4] = 1;
        _tiles[9, 5] = 1;
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
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                int index = _tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<Tile>().tileX = x;
                newTile.GetComponent<Tile>().tileY = y;
                newTile.GetComponent<Tile>().map = this; 
                newTile.transform.SetParent(tileContainer.transform);
            }
        }
    }
    void Update()
    {
        
    }
}
