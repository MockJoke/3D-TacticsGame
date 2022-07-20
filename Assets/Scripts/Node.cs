using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
  public readonly List<Node> Neighbours;
  public int x;
  public int y;
  
  // Edges 
  public Node()
  {
    Neighbours = new List<Node>(); 
  }
}  
