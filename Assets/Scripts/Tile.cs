using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{ 
    //The x & y co-ordinates of the tile
    public int tileX;
    public int tileY;
    
    // The character on the tile
    public GameObject charOnTile;
    
    public bool isTileOccupied = false; 
    
    public MapGenerator map;
}
