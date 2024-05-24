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

    // todo el codigo de abajo solo debe ejecutarse en el editor de Unity
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickingDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// inicializar nodo
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

        // cargar lista de tipos de RoomNodes
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// dibujar nodo con un estilo
    /// </summary>
    /// <param name="nodeStyle"></param>
    public void Draw(GUIStyle nodeStyle)
    {
        // dibujar la caja del nodo usando el metodo BeginArea
        GUILayout.BeginArea(rect , nodeStyle);

        // iniciar la region para detectar cambios en la seleccion del pop-up
        EditorGUI.BeginChangeCheck();

        // si el nodo tiene un padre o es de tipo entrada, mostrar una label, de lo contrario, mostrar un pop-up
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // mostrar una label que no se puede cambiar
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // mostrar un pop-up utilizando los valores del tipo de nodo que se pueden seleccionar
            // (establecer por defecto al tipo de nodo actualmente configurado)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // si la seleccion del tipo de nodo ha cambiado,
            // haciendo que las conexiones de los hijos sean potencialmente invalidas
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // si se ha cambiado el tipo de nodo y ya tiene hijos,
                // eliminar los enlaces padre-hijo ya que necesitamos revalidar
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // obtener el nodo hijo
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // si el nodo hijo no es nulo
                        if (childRoomNode != null)
                        {
                            // eliminar ID hijo del nodo padre
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // eliminar ID padre del nodo hijo
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
    /// Agregar ID hijo al nodo (devuelve true si el nodo ha sido agregado, false en caso contrario)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // verificar que el nodo hijo puede ser agregado al padre
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// verificar que el nodo hijo puede ser agregado al nodo padre - 
    /// devuelve true si es así, en caso contrario devuelve false
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    private bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossRoomAlready = false;
        // verificar si ya hay un nodo de tipo boss-room conectado en el grafo
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossRoomAlready = true;
            }
        }

        // si el nodo hijo es de tipo boss-room y ya hay un nodo boss-room conectado, devolver false
        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childID);
        if (childRoomNode.roomNodeType.isBossRoom && isConnectedBossRoomAlready)
        {
            return false;
        }

        // si el nodo hijo es de tipo ninguno, devolver false;
        if (childRoomNode.roomNodeType.isNone)
        {
            return false;
        }

        // si el nodo ya tiene un hijo con este ID hijo, devolver false
        if (childRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // si el ID del nodo y el ID del hijo son iguales, devolver false
        if (id == childID)
        {
            return false;
        }

        // si este ID hijo ya esta en la lista de ID padres, devolver false
        if (parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // si el nodo hijo ya tiene un padre, devolver false
        if (childRoomNode.parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // si el hijo es un corridor y este nodo es un corridor, devolver false
        if (childRoomNode.roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }

        // si el hijo no es un corridor y este nodo no es un corridor, devolver false
        if (!childRoomNode.roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        // si se agrega un corridor, verificar que este nodo tenga menos del maximo permitido de corredores hijos, devolver false
        if (childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }

        // si el hijo es de tipo entrance, devolver false - el room entrance siempre debe ser el nodo padre de nivel superior
        if (childRoomNode.roomNodeType.isEntrance)
        {
            return false;
        }

        // si se agrega un room a un corredor, verificar que este nodo corridor no tenga ya un nodo room agregado
        if (!childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Agregar ID padre al nodo (devuelve true si el nodo ha sido agregado, false en caso contrario)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// eliminar ID hijo del nodo (devuelve true si el nodo ha sido eliminado, false en caso contrario)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // si el nodo contiene el ID hijo, eliminarlo
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// eliminar ID padre del nodo (devuelve true si el nodo ha sido eliminado, false en caso contrario)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // si el nodo contiene el ID padre, eliminarlo
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// procesar eventos para el nodo
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessEvents(Event currentEvent)
    {
        switch ( currentEvent.type )
        {
            // procesar eventos de mouse down
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            // procesar eventos de mouse up
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            // procesar eventos de mouse drag
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// procesar evento de mouse drag
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // procesar evento de drag con click izquierdo
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// procesar evento de drag con click izquierdo
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickingDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// arrastrar Nodo
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// procesar eventos de mouse up
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
    /// procesar eventos de mouse up de clic izquierdo
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickingDragging)
        {
            isLeftClickingDragging = false;
        }
    }

    /// <summary>
    /// procesar eventos de mouse down
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // clic izquierdo abajo
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvents();
        }
        // clic derecho abajo 
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvents(currentEvent);
        }
    }

    /// <summary>
    /// procesar eventos de mouse down de clic derecho
    /// </summary>
    private void ProcessRightClickDownEvents(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLines(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// procesar eventos de mouse down de clic izquierdo
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ProcessLeftClickDownEvents()
    {
        Selection.activeObject = this;

        // alternar seleccion del nodo
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
