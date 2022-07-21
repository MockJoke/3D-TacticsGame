using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private MapGenerator map;
    
    public GameObject tileBeingDisplayed;
    
    // Cursor Info
    public int cursorX;
    public int cursorY;
    // Current tile being moused over
    public int selectedXTile;
    public int selectedYTile;
    
    //Raycast for the update for mouseHover 
    private Ray ray;
    private RaycastHit hit;

    private void Start()
    {
        map = GetComponent<MapGenerator>(); 
    }

    public void cursorUIUpdate()
    {
        // If hovering mouse over a tile, highlight it
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject; 
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
        }
        
        // If not pointing at anything
        else
        {
            map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
