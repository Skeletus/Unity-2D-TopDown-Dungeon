using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;

    // node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // define node layout style
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }


    /// <summary>
    /// Draw editor GUI
    /// </summary>
    private void OnGUI()
    {
        Vector2 screenRectPosition = new Vector2(100f, 100f);
        Vector2 screenRectSize = new Vector2(nodeWidth, nodeHeight);

        GUILayout.BeginArea(new Rect(screenRectPosition, screenRectSize), roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();

        Vector2 screenRectPosition2 = new Vector2(300f, 300f);

        GUILayout.BeginArea(new Rect(screenRectPosition2, screenRectSize), roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();
    }
}
