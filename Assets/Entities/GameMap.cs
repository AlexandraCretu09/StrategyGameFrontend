using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameMap
{
    //public int id;
    public int mapHeight;
    public int mapWidth;
    public int[][] terrain;

    public GameMap()
    {
        terrain = new int[0][];
    }
}
