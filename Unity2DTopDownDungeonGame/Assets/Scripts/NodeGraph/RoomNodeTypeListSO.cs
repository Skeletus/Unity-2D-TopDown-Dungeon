using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]
public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header LISTA_DE_TIPOS_DE_NODO 
    [Space(10)]
    [Header("LISTA DE TIPOS DE NODO")]
    #endregion
    #region ToolTip
    [Tooltip("Esta lista debe estar llena con todos los Scriptable Objects 'RoomNodeType'")]
    #endregion
    public List<RoomNodeTypeSO> list;

    #region Validacion
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}
