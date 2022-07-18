using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds all the data correspond to a particular tile
[System.Serializable]
public class TileType
{
    public string name;
    public GameObject tilePrefab;
    public int movementCost;
    public bool isWalkable; 
}
