using UnityEngine;
[System.Serializable]
public class Doorway 
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;
    #region Header
    [Header("La posición superior izquierda desde la que empieza a copiar")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;
    #region Header
    [Header("El ancho de las tiles en las doorway para copiar")]
    #endregion
    public int doorwayCopyTileWidth;
    #region Header
    [Header("La altura de las tiles en las doorway para copiar")]
    #endregion
    public int doorwayCopyTileHeight;
    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool isUnavailable = false;
}
