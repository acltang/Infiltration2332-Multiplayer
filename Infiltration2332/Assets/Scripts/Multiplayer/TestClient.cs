/*
 * Team Verb
 * CSS 432
 */
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // InputField


public class TestClient : MonoBehaviour
{

    // Client/Server field declarations
    static ConnectionManager gameConnection;
    static string uuid;
    static string username;

    void Start()
    {
        gameConnection = GameObject.Find ("Game Connection").GetComponent<ConnectionManager> ();

        // Use C# standard library to generate a Global Unique Identifier for this player.
        uuid = Guid.NewGuid().ToString();
        username = "";
    }

    
    public void OpenConnection()
    {
        if (gameConnection.isConnected())
        {
            gameConnection.closeConnection();
        }

        // user input
        InputField hostnameInput = GameObject.Find ("OpenConnectionInputField").GetComponent<InputField> ();
        string serverHostname = hostnameInput.text;
        Debug.Log("OpenConnectionInputField is: " + serverHostname);

        // default values
        //string serverHostname = "uw1-320-00.uwb.edu";
        int serverPort = 15000;

        gameConnection.openConnection(serverHostname, serverPort);
    }

    // TODO: Should be able to handle "invalid username" response from the server.
    public void Register()
    {

        // user input
        InputField usernameInput = GameObject.Find("RegisterInputField").GetComponent<InputField>();
        username = usernameInput.text;
        Debug.Log("RegisterInputField is: " + username);


        RegisterMessage msg = new RegisterMessage();
        msg.username = username;

        gameConnection.sendJSON(msg);
    }

    public void ListGames()
    {
        ListGamesMessage msg = new ListGamesMessage();

        gameConnection.sendJSON(msg);
    }

    public void CreateGame()
    {
        // user input
        InputField newGameNameInput = GameObject.Find("CreateGameInputField").GetComponent<InputField>();
        string newGameName = newGameNameInput.text;
        Debug.Log("CreateGameInputField is: " + newGameName);


        CreateGameMessage msg = new CreateGameMessage();
        msg.game_name = newGameName;

        gameConnection.sendJSON(msg);
    }

    public void JoinGame()
    {
        // user input
        InputField joinGameNameInput = GameObject.Find("JoinGameInputField").GetComponent<InputField>();
        string joinGameName = joinGameNameInput.text;
        Debug.Log("JoinGameInputField is: " + joinGameName);


        JoinGameMessage msg = new JoinGameMessage();
        msg.game_name = joinGameName;

        gameConnection.sendJSON(msg);
    }

    public void ExitGame()
    {
        ExitGameMessage msg = new ExitGameMessage();

        gameConnection.sendJSON(msg);
    }

    public void Unregister()
    {
        UnregisterMessage msg = new UnregisterMessage();

        gameConnection.sendJSON(msg);
    }

    public void CloseConnection()
    {
        if (gameConnection.isConnected())
        {
            gameConnection.closeConnection();
        }
    }
}
