using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: StringTCPClient needs to have a boolean that causes receiveString to terminate, in case the thread needs to shut down.

// Same as a regular TCP connection for a client,
// except that we can send/receive individual strings.
// 
// The constructor opens a new connection, and shutdownAndClose() must be called to end the connection.
//
// The TCP socket connection is created inside the class so that we 
// can create one of these classes to run in its own thread.
public class StringTCPClient
{
    string serverHostname;
    int serverPort;
    Socket tcpSocket;
    Encoding stringEncoding = Encoding.UTF8; // Client and server use UTF8. 
	
	const int SEND_TIMEOUT_MS = 1000;
	//const int RECEIVE_TIMEOUT_MS = 1000;

    // TODO: Error handling prevents exceptions from rising outside the class,
    // but still blocks the main thread for too long.
    public StringTCPClient(string hostname, int port)
    {
        try
        {
            serverHostname = hostname;
            serverPort = port;

			
            IPHostEntry ipHostInfo = Dns.GetHostEntry(serverHostname);
			
			for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
			{
				IPAddress ipAddress = ipHostInfo.AddressList[i];
				
				Debug.Log("StringTCPClient(): ipAddress is " + ipAddress);
				IPEndPoint serverEP = new IPEndPoint(ipAddress, serverPort);
				
				tcpSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					Debug.Log("Inner try");
					// set some timeout values before we call Connect, in case the server is not up.
					// This way, the thread/game will not stay blocked for ~10 seconds until the default timeout.
					// TODO: Timeout doesn't seem to take effect.
					tcpSocket.SendTimeout = SEND_TIMEOUT_MS;
					//tcpSocket.ReceiveTimeout = RECEIVE_TIMEOUT_MS;

					tcpSocket.NoDelay = true; // Don't package small packets together before sending. Potentially lower latency.
					
					// connect to the server. If we successfully connect, break out of the for loop.
					tcpSocket.Connect(serverEP);
					break;
				}
				catch (ArgumentNullException ane)
				{
					Debug.Log("ArgumentNullException: " + ane.ToString());
				}
				catch (SocketException se)
				{
					Debug.Log("SocketException: " + se.ToString());
				}
				catch (Exception e)
				{
					Debug.Log("Unexpected exception: " + e.ToString());
				}  
			}
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

	private string escapeString(string msg)
	{
		return msg.Replace("\r", "\\r").Replace("\n", "\\n");
	}
	
	private string unescapeString(string msg)
	{
		return msg.Replace("\\r", "\r").Replace("\\n", "\n");
	}
	
    public void sendString(string msg)
    {
        // escape newlines and carriages returns before sending.
        // append an \r\n as well, as the message separator.
        msg = escapeString(msg) + "\r\n";

        byte[] send = this.stringEncoding.GetBytes(msg);

        // check to make sure the whole message is sent. throw an exception if it's not.
        // TODO: handle an incomplete send more gracefully.
        int bytesSent = tcpSocket.Send(send);
        if (bytesSent != send.Length)
        {
            throw new Exception("sendString failed to send to " + serverHostname + "on port " + serverPort + ". " +
                                "Expected to send " + send.Length + " bytes, sent " + bytesSent + " instead.");
        }
    }
	
	int IndexOfStringInUTF8ByteArray(byte[] buffer, string toFind, int startIdx, int count)
	{
		byte[] bytesToFind = Encoding.UTF8.GetBytes(toFind);
		
		for (int i = startIdx; i < startIdx + count - bytesToFind.Length + 1; i++)
		{
			bool match = true;
			for (int j = 0; j < bytesToFind.Length; j++)
			{
				if (!bytesToFind[j].Equals(buffer[i+j]))
				{
					match = false;
					break;
				}
			}
			if (match)
			{ 
				return i; 
			}
		}
		
		return -1;
	}
	
	// fields for receiveString
	String currentString = "";
	bool haveCompleteString = false;
	bool unhandledDataInBufferFromPreviousCall = false;
	byte[] receiveBuffer = new byte[2048];
	int curIdx = 0;
	int bytesRead = 0;
    public string receiveString()
    {
		//Debug.Log("Start of receiveString()");
		const string MESSAGE_DELIMITER = "\r\n";
		String toReturn = "";
		
		while (!haveCompleteString)
		{
			if (unhandledDataInBufferFromPreviousCall == false)
			{
				//Debug.Log("unhandledDataInBufferFromPreviousCall == false");
				// Get some new data into the buffer
				bool haveReadData = false;
				const int ONE_MILLISEC_USEC = 1000;
				while (!haveReadData)
				{
					try
					{
						if (tcpSocket.Poll(ONE_MILLISEC_USEC, SelectMode.SelectRead))
						{
							bytesRead = tcpSocket.Receive(receiveBuffer, curIdx, receiveBuffer.Length - curIdx, SocketFlags.None);
							haveReadData = true;
						}
					}
					catch (System.ObjectDisposedException e) 
					{
						// This exception occurs if main thread closes the connection. Just return, Unity is closing atm.
						return "";
					}
				}
				//Debug.Log("Returning from unhandledDataInBufferFromPreviousCall == false");
			}
			else
			{
				// curIdx and bytesRead were set appropriately in a previous call, they don't need to be modified here.
				//Debug.Log("unhandledDataInBufferFromPreviousCall == true");
			}

			int firstNewlineInReadDataIdx = IndexOfStringInUTF8ByteArray(receiveBuffer, MESSAGE_DELIMITER, curIdx, bytesRead);
			//Debug.Log("firstNewlineInReadDataIdx: " + firstNewlineInReadDataIdx.ToString());
			if (firstNewlineInReadDataIdx == -1) // if we didn't find a newline
			{
				//Debug.Log("firstNewlineInReadDataIdx == -1");
				// add the data into currentString.
				currentString += System.Text.Encoding.UTF8.GetString(receiveBuffer, curIdx, bytesRead);
				
				//Debug.Log("currentString: " + currentString);
				
				// We have handled all the data that we read, so set unhandledDataInBufferFromPreviousCall to false.
				unhandledDataInBufferFromPreviousCall = false;
				
				curIdx = (curIdx + bytesRead) % receiveBuffer.Length;
				bytesRead = 0;
				
				haveCompleteString = false;
				//Debug.Log("Returning from firstNewlineInReadDataIdx == -1");
			}
			else // we DID find a newline
			{
				//Debug.Log("firstNewlineInReadDataIdx != -1");
				// add the data before the newline into currentString
				currentString += System.Text.Encoding.UTF8.GetString(receiveBuffer, curIdx, firstNewlineInReadDataIdx - curIdx);
				toReturn = currentString;
				
				currentString = "";
				
				bytesRead -= (firstNewlineInReadDataIdx - curIdx) + MESSAGE_DELIMITER.Length;
				curIdx = (firstNewlineInReadDataIdx + MESSAGE_DELIMITER.Length) % receiveBuffer.Length;
				
				if (bytesRead > 0)
				{
					unhandledDataInBufferFromPreviousCall = true;
				}
				else
				{
					unhandledDataInBufferFromPreviousCall = false;
				}

				haveCompleteString = true;
				//Debug.Log("Returning from firstNewlineInReadDataIdx != -1");
			}
		}

		//Debug.Log("Returning from receiveString()");
		haveCompleteString = false;
		return unescapeString(toReturn);
    }


    public void shutdownAndClose()
    {
        // end the connection
        tcpSocket.Shutdown(SocketShutdown.Both);
        tcpSocket.Close();
    }
}



// JSONTCPClient inherits from StringTCPClient,
// adds the sendJSON and receiveJSON methods.
public class JSONTCPClient : StringTCPClient
{

    // call the StringTCPClient constructor
    public JSONTCPClient(string hostname, int port)
        : base(hostname, port)
    {

    }

    public void sendJSON(JSONMessage send)
    {
        sendString(JsonUtility.ToJson(send));
    }
}



[Serializable]
public class JSONMessage
{
    public string message_type;
}

[Serializable]
public class RegisterMessage : JSONMessage
{
    public string username;

    public RegisterMessage()
    {
        this.message_type = "register";
    }
}

[Serializable]
public class ListGamesMessage : JSONMessage
{
    public ListGamesMessage()
    {
        this.message_type = "list_games";
    }
}

[Serializable]
public class CreateGameMessage : JSONMessage
{
    public string game_name;

    public CreateGameMessage()
    {
        this.message_type = "create_game";
    }
}

[Serializable]
public class JoinGameMessage : JSONMessage
{
    public string game_name;

    public JoinGameMessage()
    {
        this.message_type = "join_game";
    }
}

[Serializable]
public class ExitGameMessage : JSONMessage
{
    public ExitGameMessage()
    {
        this.message_type = "exit_game";
    }
}

[Serializable]
public class UnregisterMessage : JSONMessage
{
    public UnregisterMessage()
    {
        this.message_type = "unregister";
    }
}

[Serializable]
public class PlayerMoveMessage : JSONMessage
{
    public Vector3 position;
    public Vector3 move;
	public float time;

    public PlayerMoveMessage()
    {
        this.message_type = "player_move";
    }
}

[Serializable]
public class JoinGameResponse : JSONMessage
{
    public bool join_successful;
	public string player_color;
}

[Serializable]
public class LoadLevelMessage : JSONMessage
{
	public string level;
	
	public LoadLevelMessage()
    {
        this.message_type = "load_level";
    }
}

[Serializable]
public class ListGamesResponse : JSONMessage
{
	public string text_list_of_games;
}