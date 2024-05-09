using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.Assertions.Must;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    public static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // connecting line values
    private const float connectingLineWidth = 5f;
    private const float connectingLineArrowSize = 5f;

    // grid spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

        // define node layout style
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // define selected layout style
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // load room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// Selection changed in the inspector
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }


    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in inspector
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    [OnOpenAsset(0)] // need the namespace UnityEditor.Callback
    private static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }


    /// <summary>
    /// Draw editor GUI
    /// </summary>
    private void OnGUI()
    {
        // if a scriptable object of type: RoomNodeGraphSO has been selected then
        if ( currentRoomNodeGraph != null)
        {
            // draw grid
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // draw line if being dragged
            DrawDraggedLine();

            // process events
            ProcessEvents(Event.current);

            // draw connections between room nodes
            DrawRoomConnections();

            // draw room nodes
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Draw a background grid for the room node graph editor
    /// </summary>
    /// <param name="gridSize"></param>
    /// <param name="gridOpacity"></param>
    /// <param name="gridColor"></param>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        float screenWidth = position.width;
        float screenHeight = position.height;

        int verticalLineCount = Mathf.CeilToInt( (screenWidth + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt( (screenHeight + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Vector3 startPosition = new Vector3(gridSize * i, -gridSize, 0) + gridOffset;
            Vector3 endPosition = new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset;

            Handles.DrawLine(startPosition, endPosition);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Vector3 startPosition = new Vector3(-gridSize, gridSize * j, 0) + gridOffset;
            Vector3 endPosition = new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset;

            Handles.DrawLine(startPosition, endPosition);
        }

        Handles.color = Color.white;
    }

    private void DrawRoomConnections()
    {
        // loop through all room nodes
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // lopp through child room nodes
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                    GUI.changed = true;
                }
            }
        }
    }

    /// <summary>
    /// Draw connection line between the parent and the child room node
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="roomNodeSO"></param>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // get line start and position
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // calculate midway point
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // vector from start to end position of line
        Vector2 direction = endPosition - startPosition;

        // calculate normalized perpendicular positions from mid point
        Vector2 perpendicularVectorToMidPosition = new Vector2(-direction.y, direction.x).normalized;
        Vector2 arrowTailPoint1 = midPosition - perpendicularVectorToMidPosition * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + perpendicularVectorToMidPosition * connectingLineArrowSize;

        // calculate mid point offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // draw arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, 
            Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
            Color.white, null, connectingLineWidth);

        // draw line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition,
            Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // draw line from node to line  position
            Vector3 startPosition = currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center;
            Vector3 endPosition = currentRoomNodeGraph.linePosition;
            Vector3 startTangent = currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center;
            Vector3 endTangent = currentRoomNodeGraph.linePosition;
            Color lineColor = Color.white;
            Texture2D lineTexture = null;

            Handles.DrawBezier(startPosition, endPosition, 
                startTangent, endTangent, lineColor, lineTexture, connectingLineWidth);
        }
    }

    /// <summary>
    /// Draw room nodes in the graph window
    /// </summary>
    private void DrawRoomNodes()
    {
        // loop through all room nodes and draw them
        foreach( RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    private void ProcessEvents(Event currentEvent)
    {
        // reset graph drag 
        graphDrag = Vector2.zero;
        
        // get room node that mouse is over if's null or not not currently being dragged
        if (currentRoomNode == null || currentRoomNode.isLeftClickingDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // if mouse isn't over a room node or we are currently dragging a line from the node then process graphs events
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // else process room node events
        else
        {
            // process room node events
            currentRoomNode.ProcessEvents(currentEvent);
        }
        
    }

    /// <summary>
    /// Check to see to mouse is over a room node - if so then return the room node else return null
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <returns></returns>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Process Room Node Graphs Events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {

            // process mouse.down events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            // process mouse.up events
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            // process mouse.drag events
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse up events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // if releasing the right mouse button and currently dragging a line
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // check if over a room node
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // if so set it as a child of the parent room node if it can be added
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // set parent id is child room node
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// Clear line drag from a room node
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// Process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // process right click drag event - draw line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // process left click drag event - drag node graph
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// process left mouse drag event - drag room node graph
    /// </summary>
    /// <param name="delta"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Process right mouse drag event - draw line
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// Drag connecting line from room node
    /// </summary>
    /// <param name="delta"></param>
    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Process mouse down event on the room node graph ( not over a node)
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // process right click mouse down on graph event (show context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // process left click mouse down on graph event
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Clear selection from all room nodes
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select all Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Nodes Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// Delete selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomDeletionQueue = new Queue<RoomNodeSO>();

        // loop through all nodes
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomDeletionQueue.Enqueue(roomNode);

                // iterate through child room nodes ids
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // retrieve child room node 
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // remove parent ID from child room node
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // iterate through parent room node ids
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // retrieve parent room node
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // remove child ID from parent room node
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // delete queued room nodes
        while (roomDeletionQueue.Count > 0)
        {
            // get room node from queue
            RoomNodeSO roomNodeToDelete = roomDeletionQueue.Dequeue();

            // remove node from dictionary
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // remove node from liste
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // remove asset from database
            DestroyImmediate(roomNodeToDelete, true);

            // save asset database
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Delete the links between the selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // iterate through all room nodes
        foreach (RoomNodeSO currentRoomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (currentRoomNode.isSelected && currentRoomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = currentRoomNode.childRoomNodeIDList.Count -1; i >= 0; i--)
                {
                    // get child ID room node
                    string currentRoomNodeID = currentRoomNode.childRoomNodeIDList[i];
                    RoomNodeSO currentChildRoomNode = currentRoomNodeGraph.GetRoomNode(currentRoomNodeID);

                    // if the child room node is selected
                    if (currentChildRoomNode != null && currentChildRoomNode.isSelected)
                    {
                        // remove child ID from parent room node
                        currentRoomNode.RemoveChildRoomNodeIDFromRoomNode(currentChildRoomNode.id);

                        // remove parent ID from child room node
                        currentChildRoomNode.RemoveParentRoomNodeIDFromRoomNode(currentRoomNode.id);
                    }
                }
            }
        }

        // clear all selected room nodes
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// Clear selection from all room nodes
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }

        GUI.changed = true;
    }


    /// <summary>
    /// Create a room node at the mouse position
    /// </summary>
    /// <param name="userData"></param>
    private void CreateRoomNode(object mousePositionObject)
    {
        // if current node graph empty then add entrance room node first
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            Vector2 defaultPosition = new Vector2(200f, 200f);
            CreateRoomNode(defaultPosition, roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// Create a room node at the mouse position - overloaded to also pass room node type
    /// </summary>
    /// <param name="mousePositionObject"></param>
    /// <param name="roomNodeTypeSO"></param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeTypeSO)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // add room node to current room node graph room node list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // set room node values
        Vector2 recSize = new Vector2(nodeWidth, nodeHeight);
        Rect rect = new Rect(mousePosition, recSize);

        roomNode.Initialize(rect, currentRoomNodeGraph, roomNodeTypeSO);

        // add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // refresh node graph dictionary
        currentRoomNodeGraph.OnValidate();
    }
}
