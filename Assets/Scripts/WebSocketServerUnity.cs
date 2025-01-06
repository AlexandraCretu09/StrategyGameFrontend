using Fleck;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class WebSocketServerUnity : MonoBehaviour
{
    private WebSocketServer server;
    private GameData currentGameData;
    private GameMap currentGameMap;
    private List<MapResources> currentGameResources;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();


    void Start()
    {
        string localIP = NetworkUtils.GetLocalIPAddress();
        //Debug.Log("Ip:   " + "ws://" + localIP + ":8082/terrainUpdate");
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
            currentGameData = SafeDeserialize(message);
            if (currentGameData != null)
            {
                currentGameMap = currentGameData.gameMap;
                currentGameResources = currentGameData.resources;
                StartCoroutine(RenderMapCoroutine(currentGameMap, currentGameResources));
            }
            else
                Debug.Log("current game map is null.");

        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during deserialization: " + ex.Message);
        }
    }

    private GameData SafeDeserialize(string json)
    {
        try
        {
            Debug.Log("Raw JSON: " + json);
            JObject gameDataJson = JObject.Parse(json);

            var gameMapJson = gameDataJson["gameMap"].ToString();
            GameMap gameMap = JsonConvert.DeserializeObject<GameMap>(gameMapJson);

            var resourcesJson = gameDataJson["resources"].ToString();
            List<MapResources> resources = JsonConvert.DeserializeObject<List<MapResources>>(resourcesJson);

            return new GameData
            {
                gameMap = gameMap,
                resources = resources
            };
        }
        catch (Exception ex)
        {

            Debug.LogError("Exception during deserialization: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
            return null;
        }
    }

    private IEnumerator RenderMapCoroutine(GameMap gameMap, List<MapResources> resources)
    {
        yield return null;

        LobbyManager.Instance.RenderMapFromData(gameMap, resources);
    }
}
