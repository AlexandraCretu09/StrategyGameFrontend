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
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();


    void Start()
    {
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
        yield return null;

        LobbyManager.Instance.RenderMapFromData(gameMap);
    }
}
