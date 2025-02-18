using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitedNode : MonoBehaviour
{

    public Node node;
    public List<VisitedNode> parents = new List<VisitedNode>();
    public int AcumulatedCost = 0;
    public int Heuristic = 0;

}
