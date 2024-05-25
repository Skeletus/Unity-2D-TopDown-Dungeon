using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header DETALLES_BASICOS_NIVEL
    [Space(10)]
    [Header("DETALLES BASICOS DEL NIVEL")]
    #endregion Header DETALLES_BASICOS_NIVEL

    #region Tooltip
    [Tooltip("Nombre del nivel")]
    #endregion Tooltip

    public string levelName;

    #region Header ROOM_TEMPLATES_PARA_EL_NIVEL
    [Space(10)]
    [Header("Lista de Room templates para el nivel")]
    #endregion Header ROOM_TEMPLATES_PARA_EL_NIVEL

    #region Tooltip
    [Tooltip("Llenar la lista con los room templates q quieres q sean parte del nivel" +
        "Asegurate q lo room templates esten incluidos para todos los room nodes types " +
        "que son espcificados en el grafo para el nivel")]
    #endregion Tooltip

    public List<RoomTemplateSO> roomTemplateList;

    #region Header GRAFOS_PARA_EL_NIVEL
    [Space(10)]
    [Header("Grafos para el nivel")]
    #endregion

    #region Toolip
    [Tooltip("Lllena esta lista con los grafos que deben ser aleatorios para el nivel")]
    #endregion Tooltip

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region VALIDACIONES
#if UNITY_EDITOR

    // validar detalles en los scriptable objetcs
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyStrings(this, nameof(levelName), levelName);

        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
        {
            return;
        }

        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
        {
            return;
        }

        // verificar para segurar que todos los templates esten especificados para todos los tipos de nodos en el grafo especificado

        // primer revisar los corridor north/south, east/west y la entrance han sido especificados
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        // recorre todos los room template para revisar que este tipo de nodo ha sido especificado
        foreach(RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
            {
                return;
            }

            if (roomTemplateSO.roomNodeType.isCorridorEW)
            {
                isEWCorridor = true;
            }

            if (roomTemplateSO.roomNodeType.isCorridorNS)
            {
                isNSCorridor = true;
            }

            if (roomTemplateSO.roomNodeType.isEntrance)
            {
                isEntrance = true;
            }
        }

        if (isEWCorridor == false )
        {
            Debug.Log("En " + this.name.ToString() + " : No se especifico el room type E/W Corridor");
        }
        if (isNSCorridor == false)
        {
            Debug.Log("En " + this.name.ToString() + " : No se especifico el room type N/S Corridor");
        }
        if (isEntrance == false)
        {
            Debug.Log("En " + this.name.ToString() + " : No se especifico el room Type Entrance");
        }

        // recorre todos los grafos
        foreach(RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
            {
                return;
            }

            // recorre todos los nodos en el grafo 
            foreach(RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                {
                    continue;
                }

                // verifica que el room template ha sido especificado para cada room node type

                // corridors y entrance ya han sido verificados
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW ||
                    roomNodeSO.roomNodeType.isCorridorNS || roomNodeSO.roomNodeType.isCorridor ||
                    roomNodeSO.roomNodeType.isNone)
                {
                    continue;
                }

                bool isRoomNodeTypeFound = false;

                // loop throguh all room templates to check that this node type has been specified
                foreach(RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                    {
                        continue;
                    }
                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound)
                {
                    Debug.Log("En " + this.name.ToString() + " : no se encontro un room template " + roomNodeSO.roomNodeType.name.ToString() +
                        " para el grafo " + roomNodeGraph.name.ToString());
                }
            }
        }
    }
#endif
    #endregion VALIDACIONES
}
