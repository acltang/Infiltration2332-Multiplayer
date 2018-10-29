# Infiltration2332 Multiplayer

Top-down stealth/puzzle game with singleplayer and multiplayer modes.

## Compiling and Running the Code
The code must be compiled using the Unity editor (developed on version 5.6.0f3), or through the provided Infiltration 2332.exe executable. 
	
The server code can be viewed in the directory ClientServerTest as server.py. It can be run using a compiler that supports Python 2.7. The “--verbose” option may be used for verbose output.

Client-side and application-specific protocol scripts can be viewed in the directory Infiltration 2332>Assets>Scripts>Multiplayer (the scripts outside this folder are mostly for the game’s single player mode, although some are used in both modes).

server.py must be active for multiplayer functionality.

## How to Play the Game

- Run Infiltration 2332.exe
- Click on “JOIN GAME” for multiplayer (“PLAY GAME” is single player mode)
- Opening a connection
  - Enter the address of the server into the text field (e.g. uw1-320-00.uwb.edu or localhost if run on the same machine)
  - Click on “OPEN CONNECTION”
- Registering a player
  - Enter a username (anything) into the text field
  - Click on “REGISTER”
- Starting a game
  - Creating a game
    - Enter a game name (anything) into the text field next to “CREATE GAME”
    - Click on “CREATE GAME” (this will automatically put you into this game as well) to create a game with the input text as its name
  - Viewing available games
    - Click on “LIST GAMES” to view a list of all currently created games and the registered users in each game
  - Joining a game
    - Enter an existing game name in the text field next to “JOIN GAME”
    - Click on “JOIN GAME” to register your user into a game

The game will automatically start once two people are registered in the same game. There are 4 game levels. Both players must reach the checkered objective to proceed to the next level. When the final level is completed, players will be returned to the main menu (connections close automatically). Click “EXIT” to close the game.


## Client/Server Architecture
For both the client and server, worker threads are used to hold connections and check for incoming data. The client has only one worker thread, while the server may have many. The worker threads part messages into JSON and then enqueue them into a thread-safe queue. The main thread is used to dequeue, handle, and send messages.

The server holds two very simple tables for managing state. The PlayerTable uses the client’s (IP, port) as a primary key, and stores a reference to the client’s TCP socket, so that the server’s main thread knows where to send messages, and stores the client’s username. The GameTable maps the unique id for each game to the game name, as well as the list of players currently in the game.


Made with Unity3D.
