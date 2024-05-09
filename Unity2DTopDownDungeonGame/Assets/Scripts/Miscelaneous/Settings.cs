using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonReBuildAttemptsFromRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    /* max number of child corridors leading from a room - max should be 3 although this is not recomended
     since it cause the dungeon building to fail since the rooms are more likely to not fit together*/
    public const int maxChildCorridors = 3;
    #endregion
}
