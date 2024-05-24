using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Marcar solo los tipos de RoomNode que deben ser visibles en el editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("Un tipo deberia ser un corridor")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("Un tipo deberia ser un corridor North-South")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("Un tipo deberia ser un corridor East-West")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("Un tipo deberia ser una Entrance")]
    #endregion
    public bool isEntrance;
    #region Header
    [Header("Un tipo deberia ser un Boss-Room")]
    #endregion
    public bool isBossRoom;
    #region Header
    [Header("Un tipo deberia ser None (Sin Asignar)")]
    #endregion
    public bool isNone;

    #region Validacion
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyStrings(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
