using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition, and adds 
    /// movement steps to the returned stack, returns null if no path is found
    /// </summary>
    /// <param name="room"></param>
    /// <param name="startGridPosition"></param>
    /// <param name="endGridPosition"></param>
    /// <returns></returns>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        // adjust positions by lower bounds
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        // create open list and closed hashset
        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        // create gridnodes for path finding
        int width = room.templateUpperBounds.x - room.templateLowerBounds.x + 1;
        int height = room.templateUpperBounds.y - room.templateLowerBounds.y + 1;
        GridNodes gridNodes = new GridNodes(width, height);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, room.instantiatedRoom);

        if (endPathNode != null)
        {
            return CreatPathStack(endPathNode, room);
        }

        return null;
    }

    /// <summary>
    /// Create a Stack<Vector3> containing the movement path
    /// </summary>
    /// <param name="endPathNode"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private static Stack<Vector3> CreatPathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        // get mid point of cell
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize *0.5f;
        cellMidPoint.z = 0f;

        while(nextNode != null)
        {
            // convert grid position to world position
            Vector3Int cellPosition = new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0);
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(cellPosition);

            // set the world position to the middle of the grid cell
            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }

    /// <summary>
    /// Find the shortest path - returns the end node if a path has been found , else return null
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="openNodeList"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // add start node to open list
        openNodeList.Add(startNode);

        // loop through open node list until empty
        while(openNodeList.Count > 0)
        {
            // sort list
            openNodeList.Sort();

            // current node - the node in the open list with the lowest FCost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            // if the current node - target node then finish
            if (currentNode == targetNode)
            {
                return currentNode;
            }

            // add current node to the closed list
            closedNodeHashSet.Add(currentNode);

            // evaluate FCost for each neighbour of the currentNode
            EvaluateCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);
        }

        return null;
    }

    /// <summary>
    /// Evaluate neighobur nodes
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void EvaluateCurrentNodeNeighbours(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        // loop through all directions
        for (int i=-1; i <= 1; i++)
        {
            for(int j= -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                validNeighbourNode = GetValidNeighbourNode(currentNodeGridPosition.x + i,
                    currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                if (validNeighbourNode != null)
                {
                    // calculate new cost of neighbour
                    int newCostToNeighbour;

                    newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// returns the distance int between node A and node B
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="validNeighbourNodes"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distanceY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if(distanceX > distanceY)
        {
            // 10 used instead of 1, and 14 is a pythagoras approximation SQRT(10*10 + 10*10) - to avoid using floats
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }

        return 14 * distanceX + 10 * (distanceY-distanceX);
    }

    /// <summary>
    /// Evaluate a neihgbour node at neighbour node X position, neighbour node Y position using the
    /// specified gridNodes, closed node hashset and instatiate room. Returns null if the node isn't valid
    /// </summary>
    /// <param name="neigbourNodeXPosition"></param>
    /// <param name="neigbourNodeYPosition"></param>
    /// <param name="gridNodes"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns></returns>
    private static Node GetValidNeighbourNode(int neigbourNodeXPosition, int neigbourNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // if neighbour node position is beyond grid then return null
        if (neigbourNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x ||
            neigbourNodeXPosition < 0 ||
            neigbourNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y ||
            neigbourNodeYPosition < 0)
        {
            return null;
        }

        // get a neighbour node
        Node neighbourNode = gridNodes.GetGridNode(neigbourNodeXPosition, neigbourNodeYPosition);

        // if neighbour is in the closed list then skip
        if(closedNodeHashSet.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }
}
