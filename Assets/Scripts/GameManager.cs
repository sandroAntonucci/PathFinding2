using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;
    private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;
    private GameObject[,] CircleMatrix;
    public int startPosx, startPosy;
    public int endPosx, endPosy;
    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/
        
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while(endPosx== startPosx || endPosy== startPosy);

        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CircleMatrix = new GameObject[Size, Size];
        CreateNodes();
    }
    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i,j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
        DebugMatrix();
        StartCoroutine(SearchWay());
    }

    public IEnumerator SearchWay()
    {
        Node startingNode = NodeMatrix[startPosx, startPosy];
        Node currentNode = startingNode;

        Dictionary<Node, float> openList = new Dictionary<Node, float>();
        List<Node> closedList = new List<Node>();

        openList.Add(startingNode, startingNode.Heuristic);
        startingNode.acumulatedCost = 0;

        while (openList.Count > 0)
        {

            yield return new WaitForSeconds(0.1f);

            float optimalWay = openList.Values.Min();
            Node nextNode = openList.FirstOrDefault(x => x.Value == optimalWay).Key;

            // Remove from open list and add to closed list
            openList.Remove(nextNode);
            closedList.Add(nextNode);

            // Reached the end
            if (nextNode.Heuristic == 0)
            {
                CircleMatrix[nextNode.PositionX, nextNode.PositionY].GetComponent<SpriteRenderer>().color = Color.cyan;
                StartCoroutine(ShowWay());
                yield break;
            }

            // Updates the open list
            foreach (var way in nextNode.WayList)
            {
                Node wayNode = way.NodeDestiny;
                float newCost = nextNode.acumulatedCost + way.Cost;

                if (!closedList.Contains(wayNode)) 
                {
                    if (!openList.ContainsKey(wayNode) || newCost < wayNode.acumulatedCost)
                    {
                        wayNode.acumulatedCost = newCost;
                        wayNode.NodeParent = nextNode;
                        openList[wayNode] = newCost + wayNode.Heuristic;
                    }
                }

            }

            if (nextNode != startingNode)
            {
                CircleMatrix[nextNode.PositionX, nextNode.PositionY].GetComponent<SpriteRenderer>().color = Color.blue;
            }

        }
    }

    private IEnumerator ShowWay()
    {
        Node currentNode = NodeMatrix[endPosx, endPosy];

        while (currentNode.NodeParent != null)
        {
            yield return new WaitForSeconds(0.1f);
            CircleMatrix[currentNode.PositionX, currentNode.PositionY].GetComponent<SpriteRenderer>().color = Color.yellow;
            currentNode = currentNode.NodeParent;
        }
    }


    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                GameObject circle = Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                CircleMatrix[i, j] = circle;

                circle.GetComponentInChildren<TextMeshPro>().text = (Mathf.Round(NodeMatrix[i, j].Heuristic * 100f) / 100f).ToString();

                if (i == startPosx && j == startPosy)
                {
                    circle.GetComponent<SpriteRenderer>().color = Color.green;
                }

                /*
                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);
                Debug.Log("Ways: ");


                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }
                */
            }
        }
    }
    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

}
