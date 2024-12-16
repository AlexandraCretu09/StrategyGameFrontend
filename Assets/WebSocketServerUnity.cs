using Fleck;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketServerUnity : MonoBehaviour
{
    private WebSocketServer server;
    private GameMap currentGameMap;

    void Start()
    {
        Debug.Log("Starting WebSocket server on ws://127.0.0.1:8082/terrainUpdate");
        server = new WebSocketServer("ws://127.0.0.1:8082/terrainUpdate");
        server.Start(socket =>
        {
            socket.OnOpen = () => Debug.Log("Spring connected.");
            socket.OnClose = () => Debug.Log("Spring disconnected.");
            socket.OnMessage = message =>
            {
                Debug.Log("Message received from Spring: " + message);
                currentGameMap = JsonUtility.FromJson<GameMap>(message);
                UpdateTerrain(currentGameMap);
            };
        });

        Debug.Log("Unity WebSocket server started on ws://127.0.0.1:8082/terrainUpdate");

    }

    private void UpdateTerrain(GameMap gameMap)
    {

        Debug.Log("Received terrain update in Unity. Map Height: " + gameMap.mapHeight + ", Map Width: " + gameMap.mapWidth);

        for (int i = 0; i < gameMap.mapHeight; i++)
        {
            for (int j = 0; j < gameMap.mapWidth; j++)
            {
                Vector3 position = new Vector3(i, j, 0); // params are x y z
                if (gameMap.terrain[i][j] == 0)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = position;
                }
                if (gameMap.terrain[i][j] > 0)
                {
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.transform.position = position;
                }
                if (gameMap.terrain[i][j] < 0)
                {
                    GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cylinder.transform.position = position;
                }
            }
        }
    }
    void OnApplicationQuit()
    {

        server.Dispose();
    }
}
