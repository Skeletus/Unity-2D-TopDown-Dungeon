using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region CONFIGURACION DE CONSTRUCCION DE DUNGEON
    // maximo numero de intentos de reconstruccion de dungeon desde el grafo de rooms
    public const int maxDungeonReBuildAttemptsFromRoomGraph = 1000;
    // maximo numero de intentos de construccion de dungeon
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region CONFIGURACION DE ROOM
    /* numero maximo de corridors hijos que salen de un room - el maximo deberia ser 3 aunque no se recomienda
    ya que esto puede hacer que la construcción de la dungeon falle debido a que es mas probable 
    que las habitaciones no encajen entre si */
    public const int maxChildCorridors = 3;
    #endregion
}
