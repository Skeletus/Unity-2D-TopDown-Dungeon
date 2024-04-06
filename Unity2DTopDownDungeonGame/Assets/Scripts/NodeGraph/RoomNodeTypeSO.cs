using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNode Types that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("One type should be a corridor")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("One type should be a corridor North South")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("One type should be a corridor East West")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("One type should be an Entrance")]
    #endregion
    public bool isEntrance;
    #region Header
    [Header("One type should be a Boss Room")]
    #endregion
    public bool isBossRoom;
    #region Header
    [Header("One type should be None (Unassigned)")]
    #endregion
    public bool isNone;
}
