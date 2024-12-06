using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Numerics;

using Unity.VisualScripting;
using UnityEngine;
using System;
using Unity.Netcode;
public class Pathfinding : NetworkBehaviour
{
    WorldGrid grid;
    PFRequestManager requestManager;
    void Awake()
    {
        grid = GetComponent<WorldGrid>();
        requestManager = GetComponent<PFRequestManager>();
    }
    
    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (IsServer || IsOwner)
        {
            StartCoroutine(FindPath(startPos, targetPos));
        }
    }
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (startPos == null || targetPos == null)
        {
            UnityEngine.Debug.LogError("Start or end position is null!");
            yield break;
        }

        if (grid == null)
        {
            UnityEngine.Debug.LogError("Pathfinding grid is not initialized! This is in IEnumerator FindPAth Function");
            yield break;
        }


        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if(startNode.walkable && targetNode.walkable)
        {

            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            //List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode); 

            while(openSet.Count > 0)
            {   
                //heap optimization start
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if(currentNode == targetNode)
                {
                    //sw.Stop();
                    //print ("Path found: " + sw.ElapsedMilliseconds + " ms");
                    UnityEngine.Debug.Log("Path found");
                    pathSuccess = true;
                    break;
                }
                foreach(Node neighbor in grid.GetNeighbors(currentNode))
                {
                    if(!neighbor.walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if(!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
                //heap optimization end
            }
        }

        yield return null;
        if(pathSuccess)
        {
            waypoints = RestracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints,pathSuccess);       
    }

    Vector3[] RestracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i-1].gridX-path[i].gridX,path[i-1].gridY-path[i].gridY);
            if(directionNew!=directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        if(dstX>dstY)
        {
            return 14*dstY+10*(dstX-dstY);
        }
        else
        {
            return 14*dstX+10*(dstY-dstX);
        }
    }
}