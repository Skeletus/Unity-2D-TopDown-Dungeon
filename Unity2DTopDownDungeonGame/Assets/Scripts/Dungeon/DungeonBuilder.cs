using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        // cargar la lista de room node types
        LoadRoomNodeTypeList();

        // establecer el dimmed material para visibildad completa
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    /// <summary>
    /// carga la lista de room node types
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// genera una dungeon aleatoria
    /// retorna true si se construyo, 
    /// false si fallo 
    /// </summary>
    /// <param name="currentDungeonLevel"></param>
    /// <returns></returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // llena los room templates al diccionario
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        // mientras que
        // la dungeon no se haya generado exitosamente 
        // y
        // la dungeon no haya alcanzado el maximo de intentos
        while(!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // selecciona un grafo aleatorio de la lista
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonReBuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // entra en bucle hasta q la dungeon se construya con exito
            // y
            // q no alcance el maximo de re intentos
            while(!dungeonBuildSuccessful && 
                dungeonReBuildAttemptsForNodeGraph <= Settings.maxDungeonReBuildAttemptsFromRoomGraph)
            {
                // limpiar los rooms de la dungeon y el diccionario de rooms
                ClearDungeon();

                dungeonReBuildAttemptsForNodeGraph++;

                // intenta construir una dungeon para el grafo seleccionado
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }

            if (dungeonBuildSuccessful)
            {
                // instanciar rooms
                InstantiateRoomGameObjects();
            }
        }

        return true;
    }

    /// <summary>
    /// instancia los room a partir de los prefabs
    /// </summary>
    private void InstantiateRoomGameObjects()
    {
        // recorre todos los rooms
        foreach(KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            // calcular la posicion del room (recodar q la posicion de instanciacion debe ajustarse 
            // por los room template lower bounds)
            Vector3 roomPosition = new Vector3(
                (room.lowerBounds.x - room.templateLowerBounds.x),
                (room.lowerBounds.y - room.templateLowerBounds.y),
                0f
                );

            // instanciar room
            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // captura el componnet InstantiatedRoom del prefab instanciado
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            // inicializar el room instanciado
            instantiatedRoom.Initialise(roomGameObject);

            // salvar la referencia del gameobject
            room.instantiatedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// Intenta construir aleatoreamente la dungeon para el grafo especificado
    /// devuelve true si se genero un layout exitoso
    /// caso contrario devuelve false si un problema fue encontrado y otro intento es requerido
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <returns></returns>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // crea una cola de room nodes abierta
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // agregar el nodo entrance a la cola de nodos del grafo
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("Sin nodo de entrada");
            return false; // dungeon no construida
        }

        // establecer que no hay superposicion de rooms
        bool noRoomOverLaps = true;

        // procesar cola de nodos abierta
        noRoomOverLaps = ProcessRoomInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverLaps);

        // si todos los room nodes han sido procesados y no ha habido una superposicion de room 
        // devuelve truee
        if (openRoomNodeQueue.Count == 0 && noRoomOverLaps)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// procesar los rooms en la cola abierta de nodos
    /// devuelve true si no hay superposicion de room
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <param name="openRoomNodeQueue"></param>
    /// <param name="noRoomOverLaps"></param>
    /// <returns></returns>
    private bool ProcessRoomInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverLaps)
    {
        // mientras la cola no este vacia y no se haya detectado superposicion
        while(openRoomNodeQueue.Count > 0 && noRoomOverLaps == true)
        {
            // captura el sigiuente nodo de la cola
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // agregar nodos hijos a la cola desde el grafo (con enlaces a este room padre)
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // si el room es de tipo entrance marcar como posicionado y agregar al diccionario
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // agregar room al diccionario de rooms
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }

            // caso contrario si la habitacion no es una entrance
            else
            {
                // obtener el padre para el nodo
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // verificar si se puede colocar sin superposicion
                noRoomOverLaps = CanPlaceRoomWithoutOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverLaps;
    }

    /// <summary>
    /// intenta colocar un room en la dungeon
    /// si el puede colocarse devuelve true, caso contrario false
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="parentRoom"></param>
    /// <returns></returns>
    private bool CanPlaceRoomWithoutOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // inicializar y asumir superposicion hasta q se pruebe lo contrario
        bool roomOverlaps = true;

        // mientras que haya superposicion, intenta colocar todas las doorway disponibles del padre
        // hasta q el room sea colocado exitosamente sin superposicion
        while(roomOverlaps)
        {
            // selecciona una doorway desconectada aleatoria disponible para el padre
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                // si no hay mas doorways para intentar entonces hay falla de superposicion
                return false; // el room se suporne
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            // captura un room template aleatoria de la lista que  es consitente con la orientacion de la door del parent
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // crea un room
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // coloca el room - devuelve true si el room no se superpone
            if (PlaceRoom(parentRoom, doorwayParent, room))
            {
                // si el room no se superpone entonces establecer falso y salir del loop while
                roomOverlaps = false;

                // maracar al room como colocado
                room.isPositioned = true;

                // agregar el room al diccionario
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }

        }

        return true; // no hay superposicion de room
    }

    private bool PlaceRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // capturar la posicion del doorway actual
        Doorway doorway = GetOpositeDoorway(doorwayParent, room.doorwayList);

        // devuelve false si no hay una doorway que sea opuesta a la doorway padre.
        if (doorway == null)
        {
            // solo marcar a la doorway padre como no disponible para ya no intentar conectarla de nuevo
            doorwayParent.isUnavailable = true;

            return false;
        }

        // calcular la posicion de la doorway padre en el grid
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        // calcular el ajuste de la posicion basada en la posicion de la doorway parent q estamos intentando conectar
        // (ejemplo, si esta doorway es West, entonces necesitamos agregar (1,0) a la doorway parent East)
        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;

        }

        // calcular los lower y upper bounds basdandonos en el posicionamiento para alinear la doorway parent
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        // verificar si el nuevo room se superpone con algun room existente
        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            // markar doorways como conectadas y no disponibles
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            // devolver true para indicar q todos los room han sido conectados sin superposicion
            return true;
        }
        else
        {
            // solo marca la parent doorway como no disponible para no intentar conectar otra vez
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    /// <summary>
    /// verificar si hay rooms q se superpongan a los lower y upper bounds
    /// y si hay rooms superpuestos entonces devolver el room caso contrario nulo
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns></returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        // recorrer todos los rooms
        foreach(KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            // omitir si el mismo room que el room para realizar la prueba o
            // si el room aun no se ha colocado
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            // si hay superposicion
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// verificar si 2 rooms se superponen entre si
    /// devuelve true si hay superposicion
    /// caso contrario false si no hay superposicion
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <returns></returns>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverLappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x,
                                                    room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverLappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y,
                                                    room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverLappingX && isOverLappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// verifica si el intervalo 1 se superpone al intervalo 2
    /// el metodo es usado por el metodo IsOverLappgingRoom  
    /// </summary>
    /// <param name="imin1"></param>
    /// <param name="imax1"></param>
    /// <param name="imin2"></param>
    /// <param name="imax2"></param>
    /// <returns></returns>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2)) 
        { 
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// devuelve una doorway que tiene la orientacion contraria hacia la doorway padre la o 
    /// desde la doorway list
    /// </summary>
    /// <param name="parentDoorway"></param>
    /// <param name="doorwayList"></param>
    /// <returns></returns>
    private Doorway GetOpositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach(Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east &&
                doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west &&
                    doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north &&
                    doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south &&
                    doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }

        return null;
    }

    /// <summary>
    /// devuelve un room template aleatorio para el nodo teniendo en cuenta la orietacion del doorway parent
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        // si el room node es de tipo corridor
        // entonces selecciona un room template de corridor basade en la orientacion del doorway padre
        if (roomNode.roomNodeType.isCorridor)
        {
            switch(doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        // caso contrario seleccion un room template aleatorio
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    /// <summary>
    /// devuelve las doorways no conectadas
    /// </summary>
    /// <param name="doorwayList"></param>
    /// <returns></returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorwayList)
    {
        // loop through doorway list
        foreach(Doorway doorway in doorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    /// <summary>
    /// crea un room basado en el room template y el layout del nodo
    /// devuelve el room creado
    /// </summary>
    /// <param name="roomTemplate"></param>
    /// <param name="roomNode"></param>
    /// <returns></returns>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // inicializar room desde el template
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        // establecer el ID padre para el room
        if (roomNode.parentRoomNodeIDList.Count == 0) // entrance
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// crea una deep-copy de las doorway list
    /// </summary>
    /// <param name="oldDoorwayList"></param>
    /// <returns></returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach(Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }


    /// <summary>
    /// crea una deep-coy de una string list
    /// </summary>
    /// <param name="oldStringList"></param>
    /// <returns></returns>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach(string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    /// <summary>
    /// captura un room template aleatorio, desde la lista de room template, que coincida con el
    /// room node type y la devuelve
    /// </summary>
    /// <param name="roomNodeType"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // recorre la lista de room template
        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // agrega el room template que coincida
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // devuelve null si la lista esta vacia
        if (matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        // selecciona un room template aleatoria de la lista y devolverlo
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    /// <summary>
    /// devuelve un room template de acuerdo al ID
    /// devuelve nulo si el ID no existe
    /// </summary>
    /// <param name="roomTemplateID"></param>
    /// <returns></returns>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// devuelve un room por su ID
    /// si no existe un room con ese ID devuelve nulo
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// limpia los gameobjects del los room y limpia el dictionario de rooms
    /// </summary>
    private void ClearDungeon()
    {
        // destroy instantiated dungeon gameobjcets and clear dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach(KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
            {
                Room room = keyValuePair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    /// <summary>
    /// selecciona un un grafo aleatorio de la lista de grafos
    /// </summary>
    /// <param name="roomNodeGraphList"></param>
    /// <returns></returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No existen grafos en la lista");
            return null;
        }
    }

    /// <summary>
    /// carga los room templates al diccionario
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        // limpia el diccionario de room template
        roomTemplateDictionary.Clear();

        // carga la lista de room templates al diccionario
        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Room template key duplicada en: " + roomTemplateList);
            }
        }
    }
}
