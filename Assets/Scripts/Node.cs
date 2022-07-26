using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
  public readonly List<Node> Neighbours;
  public int X;
  public int Y;
  
  // Edges 
  public Node()
  { 
    Neighbours = new List<Node>(); 
  }
}  
