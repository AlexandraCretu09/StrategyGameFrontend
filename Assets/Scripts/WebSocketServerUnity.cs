using Fleck;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.Networking;

public class WebSocketServerUnity : MonoBehaviour
{
    private WebSocketServer server;
    private GameMap currentGameMap;
    //public LobbyManager lobbyManager;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();


    void Start()
    {
        //Debug.Log("Starting WebSocket server on ws://127.0.0.1:8082/terrainUpdate");
        string localIP = NetworkUtils.GetLocalIPAddress();

        Debug.Log("Ip:   " + "ws://" + localIP + ":8082/terrainUpdate");


        server = new WebSocketServer("ws://" + localIP + ":8082/terrainUpdate");
        server.Start(socket =>
        {
            socket.OnOpen = () => Debug.Log("Spring connected.");
            socket.OnClose = () => Debug.Log("Spring disconnected.");
            socket.OnMessage = message =>
            {
                Debug.Log("Message received from Spring: " + message);
                messageQueue.Enqueue(message);
            };
        });

        Debug.Log("Unity WebSocket server started on ws://" + localIP + ":8082/terrainUpdate");
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            ProcessMessage(message);
        }
    }

    void OnApplicationQuit()
    {
        server.Dispose();
    }

    public void ProcessMessage(string message)
    {
        try
        {
            currentGameMap = SafeDeserialize(message);
            //Debug.Log("Deserialization successful: " + currentGameMap.mapHeight + "x" + currentGameMap.mapWidth);



            if (currentGameMap != null)
            {
                Debug.Log("Terrain array dimensions: " + currentGameMap.terrain.Length);

                StartCoroutine(RenderMapCoroutine(currentGameMap));
            }
            else
                Debug.Log("current game map is null.");

        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during deserialization: " + ex.Message);
        }
    }

    private GameMap SafeDeserialize(string json)
    {
        try
        {
            Debug.Log("Attempting to deserialize JSON...");
            GameMap deserializedMap = Newtonsoft.Json.JsonConvert.DeserializeObject<GameMap>(json);

            if (deserializedMap == null)
            {
                Debug.LogError("Deserialization returned null.");
            }
            else
            {
                Debug.Log($"Deserialization successful: {deserializedMap.mapHeight}x{deserializedMap.mapWidth}");
                if (deserializedMap.terrain != null)
                {
                    foreach (var row in deserializedMap.terrain)
                    {
                        //Debug.Log("Row length: " + (row != null ? row.Length.ToString() : "null"));
                    }
                }
                else
                {
                    Debug.LogError("Terrain array is null.");
                }
            }

            return deserializedMap;
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during deserialization: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
            return null;
        }
    }

    private IEnumerator RenderMapCoroutine(GameMap gameMap)
    {
        // Wait until the next frame to ensure everything is updated before rendering
        yield return null;

        // Call the render method after the frame has been processed
        LobbyManager.Instance.RenderMapFromData(gameMap);
    }

    private void UpdateTerrain(GameMap gameMap)
    {

        Debug.Log("Received terrain update in Unity. Map Height: " + gameMap.mapHeight + ", Map Width: " + gameMap.mapWidth);

        for (int i = 0; i < gameMap.mapHeight; i++)
        {
            for (int j = 0; j < gameMap.mapWidth; j++)
            {
                Vector3 position = new Vector3(i, 0, j);
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
}
