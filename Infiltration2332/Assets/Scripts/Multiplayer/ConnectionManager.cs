using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManager

public class ConnectionManager : MonoBehaviour {

	GameObject hero;
	GameObject dummy;
	

	JSONTCPClient serverConnection;
	private bool connected = false;
	bool DontDestroyOnLoad_isEnabled = false;
	Queue<string> msgQueue;
	
	System.Threading.Thread myThread;
	bool threadMustShutdown = false;
	
	string player_color = "";
	
	// Use this for initialization
	void Start ()
	{
		Debug.Log("called Start()");
		msgQueue = new Queue<string>();
		
		
		// **** ATTENTION!!!! ****
		// This new thread will not shutdown (!) when the Unity IDE stops the game,
		// unless it is allowed to exit. (e.g. via a bool set in OnApplicationQuit() )
		// Any long-running 'for' or 'while' loop within EnqueueReceivedStrings must check
		// if threadMustShutdown is set.
		myThread = new System.Threading.Thread(new System.Threading.ThreadStart(EnqueueReceivedStrings));
		
		// Uncomment this to run EnqueueReceivedStrings.
		myThread.Start();
	}
	
	void Awake()
	{
		Debug.Log("called Awake()");
		if (!DontDestroyOnLoad_isEnabled)
		{
			DontDestroyOnLoad(gameObject);
			DontDestroyOnLoad_isEnabled = true;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		string nextMsg = null;
		lock (msgQueue)
		{
			if (msgQueue.Count > 0)
			{
				nextMsg = msgQueue.Dequeue();
			}
		}
		if (nextMsg != null)
		{
			handleMessage(nextMsg);
		}
	}
		
	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit() called.");
		threadMustShutdown = true;
		if (connected)
		{
			serverConnection.shutdownAndClose();
			connected = false;
		}
	}
	
	void EnqueueReceivedStrings()
	{
		Debug.Log("Started EnqueueReceivedStrings().");
		
		while (threadMustShutdown == false)
		{
			if (connected == true)
			{
				string msg = serverConnection.receiveString();
				lock (msgQueue)
				{
					msgQueue.Enqueue(msg);
				}
			}
			//System.Threading.Thread.Sleep (1000); // TODO: Replace this with a more efficient call, it seems to use a lot of CPU.
			// TODO: Problem: if we're in a game, we can't have this sleep here.
		}
		
		Debug.Log("EnqueueReceivedStrings() shutting down.");
		return;
	}
	
	// TODO
	JSONMessage message_type_only = new JSONMessage();
	void handleMessage(string msg)
	{
		JsonUtility.FromJsonOverwrite(msg, message_type_only);
		
		
		if (message_type_only.message_type == "player_move") { handlePlayerMoveMessage(msg); }
		else if (message_type_only.message_type == "join_game_response") { handleJoinGameResponse(msg); }
		else if (message_type_only.message_type == "chat_server_to_client") { handleChatServerToClient(msg); }
		else if (message_type_only.message_type == "list_games_reponse") { handleListGamesResponse(msg); }
		else if (message_type_only.message_type == "load_level") { handleLoadLevelMessage(msg); }
		else
		{
			// TODO: Handle other message types.
			Debug.Log(msg);
		}	
	}
	
	// TODO: performance.
	// TODO: check if in game, etc.
	public void handlePlayerMoveMessage(string msg)
	{
		if (	SceneManager.GetActiveScene().name == "MPLevelTest"
			||  SceneManager.GetActiveScene().name == "MPLevel1"
			||  SceneManager.GetActiveScene().name == "MPLevel2"
			||  SceneManager.GetActiveScene().name == "MPLevel3")
		{
			PlayerMoveMessage m = JsonUtility.FromJson<PlayerMoveMessage>(msg);

            DummyController dummy = GameObject.Find ("Dummy(Clone)").GetComponent<DummyController> ();
			
			dummy.ReceivePosition(m.move, m.position, m.time);
		}
	}
	
	public void handleJoinGameResponse(string msg)
	{
		//Debug.Log(msg);
		JoinGameResponse m = JsonUtility.FromJson<JoinGameResponse>(msg);
		
		if (m.join_successful == true)
		{
			player_color = m.player_color;
		}
		else
		{
			player_color = "";
		}
	}
	
	public void handleChatServerToClient(string msg)
	{
		// TODO: Handle chat messages from the server.
	}
	
	public void handleListGamesResponse(string msg)
	{
		ListGamesResponse m = JsonUtility.FromJson<ListGamesResponse>(msg);
		string textListOfGames = m.text_list_of_games;
		
		textListOfGames = textListOfGames.Replace("NEWLINE", "\n"); // work around limitation of Unity.FromJson.
		
		if (  SceneManager.GetActiveScene().name == "ClientTest_Menu"
		   || SceneManager.GetActiveScene().name == "ClientTest_Menu 1"
		   || SceneManager.GetActiveScene().name == "ClientTest_Menu 2")
		{
			GameObject t = GameObject.Find("ListGamesText");
			t.GetComponent<Text>().text = textListOfGames;
		}
	}
	
	public void openConnection(string serverHostname, int serverPort)
    {
        if (connected)
        {
            closeConnection();
        }

        serverConnection = new JSONTCPClient(serverHostname, serverPort);
		connected = true; // TODO: Need to validate that this actually connected!!!
    }
	
	public void closeConnection()
	{
		serverConnection.shutdownAndClose();
		connected = false;
	}
	
	public bool isConnected()
	{
		return connected;
	}
	
	public string getPlayerColor()
	{
		return player_color;
	}
	
	public bool sendString(string msg)
	{
		if (connected)
		{
			serverConnection.sendString(msg);
			return true;
		}
		return false;
	}	
	
	public bool sendJSON(JSONMessage msg)
	{
		if (connected)
		{
			serverConnection.sendJSON(msg);
			return true;
		}
		return false;
	}
	
	public bool sendLoadLevelMessage(string level)
	{
		LoadLevelMessage msg = new LoadLevelMessage();
		msg.level = level;
		
		sendJSON(msg);
		return true;
	}
	
	public void handleLoadLevelMessage(string msg)
	{
		LoadLevelMessage m = JsonUtility.FromJson<LoadLevelMessage>(msg);
		
		if (m.level == SceneManager.GetActiveScene().name)
		{
			hero = GameObject.Find("Hero(Clone)");
			dummy = GameObject.Find("Dummy(Clone)");
		
			//clear keycards obtained in this level
			if (hero != null)
			{
				hero.GetComponent<HeroController>().HasRedKeyCard = false;
				hero.GetComponent<HeroController>().HasBlueKeyCard = false;
			}
			
			if (dummy != null)
			{
				dummy.GetComponent<DummyController>().HasRedKeyCard = false;
				dummy.GetComponent<DummyController>().HasBlueKeyCard = false;
			}
			
			WorldController.RestartLevel();
		}
		
		WorldController.ChangeScene(m.level);
	}
}
