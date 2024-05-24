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

    // dimensiones layout del nodo
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // dimensiones de linea conectora
    private const float connectingLineWidth = 5f;
    private const float connectingLineArrowSize = 5f;

    // dimenciones de cuadricula
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // subscribirse al evento de cambio de seleccion en el inspector
        Selection.selectionChanged += InspectorSelectionChanged;

        // definir estilo del layout del nodo
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // definir estilo del layout del nodo seleccionado
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // cargar tipos de nodos
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // desubscribirse del evento de cambio de seleccion en el inspector
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// seleccion cambiada en el inspector
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
    /// Abre una ventana de editor de grafo
    /// si se hace doble click en un Room Node Graph Scriptable Object asset en el inspector
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    [OnOpenAsset(0)] // utilizamos el namespace UnityEditor.Callback
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
    /// Dibuja el GUI del editor
    /// </summary>
    private void OnGUI()
    {
        // si se selecciono un scriptable object de tipo Room Graph entonces:
        if ( currentRoomNodeGraph != null)
        {
            // dibujar cuadricula
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // dibujar una linea si esta siendo arrastrado
            DrawDraggedLine();

            // procesar eventos
            ProcessEvents(Event.current);

            // dibuja conecciones entre nodos
            DrawRoomConnections();

            // dibuja los nodos
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// dibujar una cuadricula de fondo para el editor de grafos
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
        // recorre todos los nodos 
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // recorre todos los nodos hijos
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                    GUI.changed = true;
                }
            }
        }
    }

    /// <summary>
    /// Dibuja una linea de conexion entre nodos padre e hijo
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="roomNodeSO"></param>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // establecer vectores de posicion inicio y final
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // calcular punto medio
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // crear vector desde la posicion inicial hasta la final de la linea
        Vector2 direction = endPosition - startPosition;

        // calcular la posicion perpendicular normalizada desde el punto medio
        Vector2 perpendicularVectorToMidPosition = new Vector2(-direction.y, direction.x).normalized;
        Vector2 arrowTailPoint1 = midPosition - perpendicularVectorToMidPosition * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + perpendicularVectorToMidPosition * connectingLineArrowSize;

        // calcular la posicion de desplazamiento del punto medio para la punta de la flecha
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // dibujar flecha
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, 
            Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
            Color.white, null, connectingLineWidth);

        // dibujar linea
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition,
            Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // dibuja una linea desde el nodo hacia una posicion
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
    /// dibujar los nodos en la ventana de editor de grafos
    /// </summary>
    private void DrawRoomNodes()
    {
        // recorrer todos los nodos y dibujarlos
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
        // restablecer el arrastre del grafo
        graphDrag = Vector2.zero;
        
        // captura el nodo q se encuentre en el cursor, si es nulo o no esta siendo arrastrado entonces
        if (currentRoomNode == null || currentRoomNode.isLeftClickingDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // si el mouse no esta sobre un nodo 
        // o actualmente estamos arrastrando una linea desde el nodo
        // entonces procesar los eventos de grafo
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // caso contrario procesa eventos de nodo
        else
        {
            // procesa eventos del nodo
            currentRoomNode.ProcessEvents(currentEvent);
        }
        
    }

    /// <summary>
    /// verificar que el mouse este sobre un nodo;
    /// si es asi, devolver el nodo
    /// caso contrario, devolver nulo
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
    /// procesar eventos del grafo
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {

            // procesar eventos mouse down
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            // procesar eventos mouse up
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            // procesar eventos mouse drag
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// procesar eventos mouse up
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // si se suelta el click derecho del mouse y actualmente se esta arrastrando una linea
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // verificar si esta sobre un nodo
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // si es así, establecer como hijo del nodo padre si se puede agregar
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // establecer el id del padre al nodo hijo
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// borrar linea conectora de un nodo
    /// </summary>
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// procesar eventos mouse drag
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // procesar evento de mouse drag con clic derecho - dibujar linea conectora
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // procesar evento de mouse drag con clic izquierdo - mover nodo
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// procesar evento de mouse drag con clic izquierdo - mover nodo
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
    /// procesar evento de mouse drag con clic derecho - dibujar linea conectora
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
    /// mover linea conectora desde el nodo
    /// </summary>
    /// <param name="delta"></param>
    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// procesar el evento mouse down en el grafo (no sobre un nodo)
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // procesar evento mouse down con click derecho - mostrar menu de acciones
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // procesar evento mouse down con click izquierdo - resetear linea conectora y deseleccionar nodo(s)
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// deseleccionar todos los nodos
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
    /// mostrar menu de acciones
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Crear Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Seleccionar todos los Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Eliminar Conexiones de todos los Room Node(s) seleccionado(s)"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Eliminar Room Node(s) selccionado(s)"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// eliminar Room Node(s) selccionado(s)
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomDeletionQueue = new Queue<RoomNodeSO>();

        // recorre todos los nodos
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomDeletionQueue.Enqueue(roomNode);

                // recorre todos los hijos del nodo
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // captura el nodo hijo
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // remover el id del padre al nodo hijo
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // recorrer todos los padres del nodo
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // obtener el padre del nodo
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // eliminar los hijos del nodo padre
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // eliminar los nodos en cola
        while (roomDeletionQueue.Count > 0)
        {
            // desencolar el nodo de la cola
            RoomNodeSO roomNodeToDelete = roomDeletionQueue.Dequeue();

            // remover al nodo del diccionario
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // remover al nodo de la lista
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // eliminar asset de la database
            DestroyImmediate(roomNodeToDelete, true);

            // guardar asset database
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// eliminar Conexiones de todos los Room Node(s) seleccionado(s)
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // recorrer todos los nodos
        foreach (RoomNodeSO currentRoomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (currentRoomNode.isSelected && currentRoomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = currentRoomNode.childRoomNodeIDList.Count -1; i >= 0; i--)
                {
                    // obtener los hijos del nodo
                    string currentRoomNodeID = currentRoomNode.childRoomNodeIDList[i];
                    RoomNodeSO currentChildRoomNode = currentRoomNodeGraph.GetRoomNode(currentRoomNodeID);

                    // si el hijo del nodo esta selccionado
                    if (currentChildRoomNode != null && currentChildRoomNode.isSelected)
                    {
                        // eliminar los hijos del nodo padre
                        currentRoomNode.RemoveChildRoomNodeIDFromRoomNode(currentChildRoomNode.id);

                        // eliminar al padre de los nodos hijos
                        currentChildRoomNode.RemoveParentRoomNodeIDFromRoomNode(currentRoomNode.id);
                    }
                }
            }
        }

        // deseleccionar todos los nodos seleccionados
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// seleccionar todos los nodos
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
    /// crear nodo en la posicion del mouse
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // si el grafo esta vacio entonces agregar primero al nodo de tipo Entrance
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            Vector2 defaultPosition = new Vector2(200f, 200f);
            CreateRoomNode(defaultPosition, roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// sobrecarga de funcion - para pasar el tipo de nodo
    /// crear nodo en la posicion del mouse
    /// </summary>
    /// <param name="mousePositionObject"></param>
    /// <param name="roomNodeTypeSO"></param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeTypeSO)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // crear room node scriptable object asset 
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // agregar nodo a la lista actual de nodos en el grafo
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // establecer dimensiones del nodo
        Vector2 recSize = new Vector2(nodeWidth, nodeHeight);
        Rect rect = new Rect(mousePosition, recSize);

        roomNode.Initialize(rect, currentRoomNodeGraph, roomNodeTypeSO);

        // agregar nodo a la database de room node graph scriptable object asset
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // actualizar el diccionario de nodos
        currentRoomNodeGraph.OnValidate();
    }
}
