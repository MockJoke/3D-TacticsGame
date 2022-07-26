using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public GameObject map;
    private MapGenerator _mapGenerator;
    private MapManager _mapManager;
    public PlayerMovement player;
    
    // Raycast for the update of charHover info
    private Ray _ray;
    private RaycastHit _hit;
    
    // Cursor Info
    public int cursorX;
    public int cursorY;
    // Current tile being moused over
    public int selectedXTile;
    public int selectedYTile;
    
    public GameObject tileBeingDisplayed;

    public bool charPathExists;


    void Start()
    {
       _mapManager = map.GetComponent<MapManager>();
       _mapGenerator = map.GetComponent<MapGenerator>();
       player = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
       // Always trying to see where the mouse is pointing 
       _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       if (Physics.Raycast(_ray, out _hit))
       {
          CursorUIUpdate();
          
          // If the char is selected, highlight the current path with the UI
          if (_mapManager.selectedChar != null &&
              _mapManager.selectedChar.GetComponent<PlayerMovement>().GetMovementStates(1) ==
              _mapManager.selectedChar.GetComponent<PlayerMovement>().charMoveState)
          {
              // check if the cursor is in range, can't show movement outside the range 
              if (_mapManager.SelectedCharMoveRange.Contains(_mapGenerator.Graph[cursorX, cursorY]))
              {
                  if (cursorX != _mapManager.selectedChar.GetComponent<PlayerMovement>().x ||
                      cursorY != _mapManager.selectedChar.GetComponent<PlayerMovement>().y)
                  {
                      if (!charPathExists &&
                          _mapManager.selectedChar.GetComponent<PlayerMovement>().MovementQueue.Count == 0)
                      {
                          
                      }
                  } 
              }
          }
       }
    }
   
   // Updates the cursor for the UI
   private void CursorUIUpdate() 
   { 
       // If hovering mouse over a tile, highlight it
       if (_hit.transform.CompareTag("Tile")) 
       { 
           if (tileBeingDisplayed == null) 
           { 
               selectedXTile = _hit.transform.GetComponent<Tile>().tileX; 
               selectedYTile = _hit.transform.GetComponent<Tile>().tileY; 
               cursorX = selectedXTile; 
               cursorY = selectedYTile; 
               _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
               tileBeingDisplayed = _hit.transform.gameObject; 
           }
           else if (tileBeingDisplayed != _hit.transform.gameObject) 
           { 
               selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX; 
               selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY; 
               _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
               
               selectedXTile = _hit.transform.GetComponent<Tile>().tileX; 
               selectedYTile = _hit.transform.GetComponent<Tile>().tileY; 
               cursorX = selectedXTile; 
               cursorY = selectedYTile; 
               _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
               tileBeingDisplayed = _hit.transform.gameObject;
           }
       } 
       // If hovering mouse over a character, highlight a tile that the character is occupying
       else if (_hit.transform.CompareTag("Player")) 
       { 
           if (tileBeingDisplayed == null) 
           { 
               selectedXTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().x; 
               selectedYTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().y; 
               cursorX = selectedXTile; 
               cursorY = selectedYTile; 
               _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
               tileBeingDisplayed = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().tileBeingOccupied;
           }
           else if (tileBeingDisplayed != _hit.transform.gameObject)
           {
               if (_hit.transform.parent.gameObject.GetComponent<PlayerMovement>().MovementQueue.Count == 0)
               {
                   selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX;
                   selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY;
                   _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                   selectedXTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().x; 
                   selectedYTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().y;
                   cursorX = selectedXTile;
                   cursorY = selectedYTile;
                   _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                   tileBeingDisplayed = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().tileBeingOccupied;
               }
           }
       }
       // If not pointing at anything
       else
       { 
           _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
       }
   }

   // Moves the char
   public void MoveChar()
   {
      if (_mapManager.selectedChar != null)
      {
         _mapManager.selectedChar.GetComponent<PlayerMovement>().MoveNextTile();
      }
   }
}
