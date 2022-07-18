using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Responsible for generation of the Grid Map
public class MapGenerator : MonoBehaviour
{
    [Header("Tiles")] 
    public TileType[] tileTypes;
    private int[,] tiles;
    
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
    
    // Instantiates the map tiles 
    public void GenerateMapVisuals()
    {
        int index; 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                index = tiles[x, y];
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
