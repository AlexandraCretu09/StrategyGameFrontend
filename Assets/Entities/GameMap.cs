using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GameMap
{
    [JsonProperty("mapHeight")]
    public int mapHeight { get; set; }
    [JsonProperty("mapWidth")]
    public int mapWidth { get; set; }
    [JsonProperty("terrain")]
    public int[][] terrain { get; set; }

    public GameMap()
    {
        terrain = new int[0][];
    }
}

[System.Serializable]
public class MapResources
{
    [JsonProperty("type")]
    public string type;
    [JsonProperty("x")]
    public int x;
    [JsonProperty("y")]
    public int y; 

    public MapResources() { }

    public int GetX()
    {
        return this.x;
    }
    public int GetY()
    {
        return this.y;
    }
    public string GetResType()
    {
        return this.type;
    }
}

[System.Serializable]
public class GameData
{
    public GameMap gameMap;
    public List<MapResources> resources;

    public GameData() { }
}


