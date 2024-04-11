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

        // display a pop up using the room node type values that can be selected from (default to the currently set room node type)
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

        roomNodeType = roomNodeTypeList.list[selection];
        
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


#endif

    #endregion
}
