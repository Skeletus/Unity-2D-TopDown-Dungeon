using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region EditorCode

    // the following code should only be run in the unity editor
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickingDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// Initialize Node
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="nodeGraph"></param>
    /// <param name="roomNodeType"></param>
    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // load room ndoe type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw node with the nodeStyle
    /// </summary>
    /// <param name="nodeStyle"></param>
    public void Draw(GUIStyle nodeStyle)
    {
        // draw node box using begin area method
        GUILayout.BeginArea(rect , nodeStyle);

        // start region to detect pop up selection changes
        EditorGUI.BeginChangeCheck();

        // if the room node has a parent or is of type entrance then display a label else display a popup
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // display a label that can't be changed
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // display a pop up using the room node type values that can be selected from (default to the currently set room node type)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // if the room type selection has changed making child connections potentially invalid
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // if a room node type has been changed and it already has children then delete the parent child links since we need to revalidate any
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // get chil room node
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRomNode(childRoomNodeIDList[i]);

                        // if the child room node it's not null
                        if (childRoomNode != null)
                        {
                            // remove child ID from parent room node
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // remove parent ID from child room node
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// Add child ID to the node (returns true if the node has been added, false otherwise)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // check child node can be added validly to parent
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// check the child node can be validly added to the parent node - return true if it otherwise return false
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    private bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossRoomAlready = false;
        // check if there is already a connected boss room in the node graph
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossRoomAlready = true;
            }
        }

        //if the child node has a type of boss room and there is already a connected boss room node then return false
        RoomNodeSO childRoomNode = roomNodeGraph.GetRomNode(childID);
        if (childRoomNode.roomNodeType.isBossRoom && isConnectedBossRoomAlready)
        {
            return false;
        }

        // if the child node has a type of none then return false;
        if (childRoomNode.roomNodeType.isNone)
        {
            return false;
        }

        // if the node already has a child with this child id return false
        if (childRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // if the node id and the child id are the same return false
        if (id == childID)
        {
            return false;
        }

        // if this childID is already in the parentID list return false
        if (parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // if the child node already has a parent return false
        if (childRoomNode.parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // if the child is a corridor and this node is a corridor return false
        if (childRoomNode.roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }

        // if the child is not a corridor and this node is not a corridor return false
        if (!childRoomNode.roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        // if adding a corridor check that this node has < the maxium permited child corridors
        if (childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }

        // if the child room is an entrance return false - the entrance must always be the top level parent node
        if (childRoomNode.roomNodeType.isEntrance)
        {
            return false;
        }

        // if adding a room to a corridor check that this corridor node doesn't already have a room node added
        if (!childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Add parent ID to the node (returns true if the node has been added, false otherwise)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// remove child ID from the node (returns true if the node has been removed, false otherwsie)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // if the node contains the child ID then remove it
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// remove parent ID from the node (returns true if the node has been removed, false otherwsie)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // if the node contains the parent ID then remove it
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Process events for the node
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessEvents(Event currentEvent)
    {
        switch ( currentEvent.type )
        {
            // process mouse down events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            // process mouse down events
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            // process mouse down events
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // process left click drag event
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// Process left mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickingDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drag Node
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Process mouse up events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // if left click up
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// Process left click up events
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickingDragging)
        {
            isLeftClickingDragging = false;
        }
    }

    /// <summary>
    /// Process mouse down events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // left click down
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvents();
        }
        // right click down 
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvents(currentEvent);
        }
    }

    /// <summary>
    /// Process right click down events
    /// </summary>
    private void ProcessRightClickDownEvents(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLines(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// Process left click down event
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ProcessLeftClickDownEvents()
    {
        Selection.activeObject = this;

        // toggle node selection
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }

    }

#endif

    #endregion
}
