using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB

    #region Tooltip

    [Tooltip("El prefab del gameobject para el room (este contendra todos los tilemaps del room y los objetos del entorno)")]

    #endregion Tooltip

    public GameObject prefab;

    // se usa para regenerar el GUID (global unique identifier)
    // si el objeto ScriptableObject (SO) se copia y el prefab se cambia.
    [HideInInspector] public GameObject previousPrefab;


    #region Header CONFIGURACION ROOM

    [Space(10)]
    [Header("CONFIGURACION ROOM")]

    #endregion Header CONFIGURACION ROOM

    #region Tooltip

    [Tooltip("El room node type SO. Los tipos de room node corresponden a los room nodes usados en el grafo. Las excepciones son con los corridors. En el grafo solo hay tipo de corridor llamado 'Corridor'. Para las room templates hay 2 tipos de corridor - CorridorNS y CorridorEW.")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("Si imaginas un rectangulo alrededor del room tilemap que lo encierra por completo, El room lower bounds representan la esquina inferior izquierda de ese rectangulo. Este debe determinarse a partir del tilemap para el room")]

    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("Si imaginas un rectangulo alrededor del room tilemap que lo encierra por completo, El room upper bounds representan la esquina superior derecha de ese rectangulo. Este debe determinarse a partir del tilemap para el room")]

    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("Debe haber un max de 4 doorways para un room - una para cada direccion (NSEW).  Estas deben tener un tamanio de 3 tile de apertura,siendo la posicion del tile medio la 'position' del doorway")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("Cada posible posicion de aparicion (usada para enemigos o cofres) para el room en las coordenadas del tilemap debe ser agregada al arreglo")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// devuelve la lista de Doorways para el room template
    /// </summary>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR

    // validacion de campos
    private void OnValidate()
    {
        // settear el GUID si esta vacio o si hay cambios en el prefab
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        // verificacion de puntos de aparicion (spawn position array)
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }

#endif

    #endregion Validation
}