using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField lobbyIdInputField;
    public GameObject mainMenu;
    public GameObject createLobbyMenu;
    public GameObject joinLobbyMenu;
    public GameObject game;
    public TextMeshProUGUI usernamesDisplay;

    private int initializedLobbyId;

    public static LobbyManager Instance { get; private set; }
    public WebSocketServerUnity webSocketServerUnity;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        createLobbyMenu.SetActive(false);
        joinLobbyMenu.SetActive(false);
        game.SetActive(false);
        mainMenu.SetActive(true);
    }


    public void CreateLobby()
    {
        string username = usernameInputField.text;

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogError("Username is required to create a lobby!");
            return;
        }

        mainMenu.SetActive(false);
        joinLobbyMenu.SetActive(false);
        createLobbyMenu.SetActive(true);

        Debug.Log("Lobby created with username: " + username);

        string localIP = NetworkUtils.GetLocalIPAddress();
        StartCoroutine(CreateLobbyCoroutine(localIP, username));


    }

    

    private IEnumerator CreateLobbyCoroutine(string ipAddress, string username)
    {
        string url = "http://localhost:8080/lobby/createLobby";

        WWWForm form = new WWWForm();
        form.AddField("ipAddress", ipAddress);
        form.AddField("username", username);


        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Lobby created successfully: " + www.downloadHandler.text);

                if (int.TryParse(www.downloadHandler.text, out int lobbyId))
                {
                    initializedLobbyId = lobbyId;
                    Debug.Log("Stored Lobby ID: " + initializedLobbyId);
                    Debug.Log("Lobby created with username: " + username);
                    FetchAndDisplayUsernames();
                }
                else
                {
                    Debug.LogError("Failed to parse Lobby ID from server response.");
                }
            }
            else
            {
                Debug.LogError("Failed to create lobby: " + www.error);
            }
        }
    }

    public void JoinLobby()
    {

        string username = usernameInputField.text;

        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogError("Username is required to join a lobby!");
            return;
        }


        mainMenu.SetActive(false);
        createLobbyMenu.SetActive(false);
        joinLobbyMenu.SetActive(true);

        Debug.Log("Switching to join lobby with username: " + username);
    }

    public void SubmitJoinLobby()
    {

        string username = usernameInputField.text;

        if (int.TryParse(lobbyIdInputField.text, out int lobbyId))
        {
            Debug.Log("Joining lobby with ID: " + lobbyId + " and username: " + username);

            string localIP = NetworkUtils.GetLocalIPAddress();
            StartCoroutine(JoinLobbyCoroutine(lobbyId, localIP, username));
        }
        else
        {
            Debug.LogError("Invalid lobby ID! Please enter a valid number.");
        }
    }


    private IEnumerator JoinLobbyCoroutine(int lobbyId,string ipAddress, string username)
    {
        string url = "http://localhost:8080/lobby/joinLobby";

        WWWForm form = new WWWForm();
        form.AddField("lobbyId", lobbyId);
        form.AddField("ipAddress", ipAddress);
        form.AddField("username", username);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Lobby joined successfully: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to join lobby: " + www.error);
            }
        }
    }

    public void StartGame()
    {
        string username = usernameInputField.text;

        createLobbyMenu.SetActive(false);
        joinLobbyMenu.SetActive(false);
        game.SetActive(true);
        mainMenu.SetActive(false);

        StartCoroutine(StartGameCoroutine(username));

    }

    private IEnumerator StartGameCoroutine( string username) {
        
        string url = "http://localhost:8080/lobby/startGame";

        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("lobbyId", initializedLobbyId);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Game started successfully: " + www.downloadHandler.text);

            }
            else
            { 
                Debug.LogError("Failed to start game: " + www.error);
            }
        }

        
    }

    public void GoBack()
    {

        mainMenu.SetActive(false);
        createLobbyMenu.SetActive(false);
        joinLobbyMenu.SetActive(false);

        mainMenu.SetActive(true);

        Debug.Log("Returning to Main Menu");
    }

    private IEnumerator FetchUsernamesCoroutine(int lobbyId)
    {
        string url = $"http://localhost:8080/users/lobbyParticipants?lobbyId={lobbyId}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            UsernameList result = JsonUtility.FromJson<UsernameList>("{\"usernames\":" + request.downloadHandler.text + "}");
            string usernamesText = string.Join("\n", result.usernames);
            usernamesDisplay.text = usernamesText;
            foreach (var username in result.usernames)
            {
                Debug.Log("Username: " + username);
            }
        }
        else
        {
            Debug.LogError("Failed to fetch usernames: " + request.error);
        }
    }


    public void moveCommands(GameObject buttonObject)
    {

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"No Button component found on {buttonObject.name}");
            return;
        }
        //Debug.Log($"in switch with {button.name}");
        switch (button.name)
        {
            case "MoveRight":
                //Debug.Log("in move right");
                SendCommand("moveRight");
                break;
            case "MoveUp":
                //Debug.Log("in move up");
                SendCommand("moveUp");
                break;
            case "MoveDown":
                //Debug.Log("in move down");
                SendCommand("moveDown");
                break;
            case "MoveLeft":
                //Debug.Log("in move left");
                SendCommand("moveLeft");
                break;
            default:
                Debug.Log($"broke {button.name}");
                break;

        }
    }


    public void SendCommand(string command)
    {
        string username = usernameInputField.text;

        StartCoroutine(SendCommandCoroutine(username, command));
    }

    private IEnumerator SendCommandCoroutine(string username, string command)
    {
        string url = $"http://localhost:8080/users/usernameAndCommand";

        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("command", command);


        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;

                if (webSocketServerUnity != null)
                {
                    webSocketServerUnity.ProcessMessage(json);
                }
                else
                {
                    Debug.LogError("WebSocketServerUnity is not assigned!");
                }

            }
            else
            {
                Debug.LogError("Failed to send command: " + www.error);
            }
        }
    }


    public void RenderMapFromData(GameMap gameMap)
    {
        if (gameMap != null && gameMap.terrain != null)
        {
            int[,] terrainMatrix = ConvertTo2DArray(gameMap.terrain);
            RenderMap(terrainMatrix);
        }
        else
        {
            Debug.LogError("Invalid game map data.");
        }
    }

    public GameObject landPrefab;
    public GameObject stonePrefab;
    public GameObject goldPrefab;
    public GameObject treePrefab;


    private List<GameObject> instantiatedTiles = new List<GameObject>();

    void RenderMap(int[,] mapMatrix)
    {
        foreach (GameObject tile in instantiatedTiles)
        {
            Destroy(tile);
        }
        instantiatedTiles.Clear();

        int rows = mapMatrix.GetLength(0);
        int cols = mapMatrix.GetLength(1);

        int XOffset = Screen.width / 2 - Screen.width / 20;
        int YOffset = Screen.height / 3 + Screen.height / 2;

        float tileWidth = 16.0f;
        float tileHeight = 16.0f;


        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject prefab = GetTilePrefab(mapMatrix[i, j]);
                if (prefab != null)
                {
                    GameObject tile = Instantiate(prefab);
                    tile.transform.position = new Vector3(j * tileWidth + XOffset, -i * tileHeight + YOffset, 0);
                    tile.transform.localScale = new Vector3(tileWidth, tileHeight, 1);
                    instantiatedTiles.Add(tile);
                }
                else
                {
                    Debug.LogError($"Unknown tile type {mapMatrix[i, j]} at position ({i}, {j})");
                }
            }
        }
        
    }

    GameObject GetTilePrefab(int tileType)
    {
        switch (tileType)
        {
            case -1: return stonePrefab;
            case 1: return goldPrefab;
            case 0: return landPrefab;
            default: return null;
        }
    }

    private int[,] ConvertTo2DArray(int[][] jaggedArray)
    {
        int rows = jaggedArray.Length;
        int cols = jaggedArray[0].Length;
        int[,] result = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = jaggedArray[i][j];
            }
        }

        return result;
    }

    public void FetchAndDisplayUsernames()
    {
        StartCoroutine(FetchUsernamesCoroutine(initializedLobbyId));
    }

    [System.Serializable]
    public class UsernameList
    {
        public List<string> usernames;
    }

    [System.Serializable]
    public class MapData
    {
        public int mapHeight;
        public int mapWidth;
        public int[][] terrain;
    }
}

