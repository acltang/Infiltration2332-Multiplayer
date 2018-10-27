# Using python 2.7
# Linux lab uses 2.7
import sys
import getopt
import socket
import select
import threading
import Queue
import time
import signal
import json
import pprint
import logging


#Server data storage format:
#	PlayerTable:		
#		client_addr -> connection_tcp_socket, username
#	GameTable:
#		game_id -> game_name, in_progress, [clientAddr, clientAddr]
#
#The clientAddr serves as the unique identifier for each connection.

# ---- Global variables
server_must_exit = False
thread_must_exit = {}
json_msg_queue = Queue.Queue(maxsize = 1000)
PlayerTable = {}
GameTable = {}
next_game_id = 1
MESSAGE_DELIMITER = "\r\n"
MP_LEVEL_1_STR = "MPLevel1"
# ----

def main(port, debug_level):
	global server_must_exit
	global thread_must_exit
	global json_msg_queue
	
	startLogging(debug_level);
	
	
	print("Starting server.\n\thostname: '{}'\n\tport: '{}'".format(socket.gethostname(), port))
	
	# We call socket.accept() from within a new thread. This is so that the
	# main thread is available to catch keyboard interrupts from the terminal.
	# If socket.accept() is blocking in the main thread, the only way to kill
	# the program from the terminal is to use the "Crtl + Pause/Break" key combo.
	#thread.start_new_thread(acceptIncomingConnections)
	threading.Thread(target = acceptIncomingConnections, args = (port,)).start()
	
	
	while server_must_exit == False:
		try:
			msg, clientAddr = json_msg_queue.get(block = True, timeout = 5)
			handleJSONMessage(msg, clientAddr)
			json_msg_queue.task_done() # the Queue will discard the message
		except Queue.Empty as e:
			logging.info(e)
	
	for key in thread_must_exit.keys():
		thread_must_exit[key] = True
	
	
			
	
# This function continually listens for and accepts connections, and creates threads to handle them.
def acceptIncomingConnections(SERVER_PORT):
	global server_must_exit
	global thread_must_exit
	
	logging.info("acceptIncomingConnections called.")
	
	# open a socket to listen for connections
	HOST = ''
	serverSd = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # use TCP
	serverSd.bind((HOST, SERVER_PORT))
	serverSd.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, True) # when the socket is closed, have the OS make it available for re-use right away.
	serverSd.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, True)
	serverSd.listen(True)
	
	connections = []
	num_total_connections = 0
	
	#print("Beginning while loop.")
	while server_must_exit == False:
		try:
			serverSd.settimeout(10) # 10 second timeout. 
			clientConnection, clientAddr = serverSd.accept()
			num_total_connections = num_total_connections + 1
			thread_must_exit[num_total_connections] = False
			clientConnection.setblocking(False) # recv is no longer a blocking call on this new socket
			connections.append((clientConnection, clientAddr))
			#thread.start_new_thread(handleClientConnection, (clientConnection, clientAddr))
			threading.Thread(target = handleClientConnection, args = (num_total_connections, clientConnection, clientAddr)).start()
		except socket.timeout as e:
			logging.info(e)
			logging.info("acceptIncomingConnections(): socket.accept() timed out. server_must_exit: {}".format(server_must_exit))
			pass
	
	logging.info("returning from acceptIncomingConnections.")
			
def handleClientConnection(connectionNumber, conn, clientAddr):
	global server_must_exit
	global thread_must_exit
	global json_msg_queue
	global PlayerTable
	
	CLIENT_CONNECTION_TIMEOUT = 120
	BUFFER_TIMEOUT_SEC = 2
	
	print("New connection accepted from {}. Connection number {}.".format(clientAddr, connectionNumber))
	
	PlayerTable[clientAddr] = {'connection_tcp_socket': conn,
							  'username': None}
		
	
	
	# TODO: implement a way to detect:
	#  - that the client closed the connection gracefully.
	#	  (When the client presses the "Close Connection" button, it doesn't work)
	#  - that the client disconnected without calling shutdown/close.
	# Currently there's no way to close these connections except with Crtl + Z, pkill -9 python
	buffer = ''
	oldtime = time.time()
	while server_must_exit == False and thread_must_exit[connectionNumber] == False:
		if len(buffer) > 0 and time.time() - oldtime > BUFFER_TIMEOUT_SEC:
			# Reset the buffer because for some reason data was lost.
			# It's been more than BUFFER_TIMEOUT_SEC and we still have leftover data in the buffer.
			buffer = ''
			
		buffer, nextString = getNextString(conn, buffer, CLIENT_CONNECTION_TIMEOUT, connectionNumber)
		if thread_must_exit[connectionNumber] == True:
			break
		logging.info(nextString)
		try:
			json_msg = json.loads(nextString)
			json_msg_queue.put((json_msg, clientAddr))
		except ValueError as e:
			print(e)
			buffer = ''
			pass # TODO: Better error handling / error message on connection timeout.
	
	thread_must_exit[connectionNumber] = True
	conn.shutdown(2)
	conn.close()
	
	removePlayerFromAllGames(clientAddr)
	print("Closed thread for connection number {}".format(connectionNumber))

# Take in the socket for the current open connection, and a buffer (a string) of characters read so far.
# Return the updated buffer and the next string received from the connection.
def getNextString(conn, buffer, TIMEOUT, connectionNumber):
	global server_must_exit
	global thread_must_exit
	
	DEFAULT_MAX_RECV_SIZE = 4096 # recommended size
	
	oldtime = time.time()
	firstNewline = buffer.find('\n')
	while firstNewline == -1 and server_must_exit == False and thread_must_exit[connectionNumber] == False:
		readableSockets = []
		try:
			readableSockets, writeableSockets, errorSockets = select.select([conn], [], [conn], TIMEOUT)
		except select.error:
			thread_must_exit[connectionNumber] = True
			buffer = ''
			firstNewline = -1
			to_return = ''
		if len(errorSockets) > 0:
			logging.info("getNextString(): 'len(errorSockets) > 0' in connection number: {}. This likely means the other end has closed. Setting thread_must_shutdown for this thread.".format(connectionNumber))
			thread_must_exit[connectionNumber] = True
			buffer = ''
			firstNewline = -1
			to_return = ''
		elif len(readableSockets) > 0:
			new_data = conn.recv(DEFAULT_MAX_RECV_SIZE)
			if new_data == '': # This means the client disconnected, but we didn't get sent anything.
				thread_must_exit[connectionNumber] = True
			buffer = ''.join([buffer, new_data]) # concatenate the buffer and new data, store in new string.
		elif len(readableSockets) == 0 and len(writeableSockets) == 0 and len(errorSockets) == 0: # indicates a timeout has occurred
			thread_must_exit[connectionNumber] = True
			buffer = ''
			firstNewline = -1
			to_return = ''
		#Now that we have received data, look for a newline char
		firstNewline = buffer.find('\n')
	
	# We found a newline, so there's a string we're ready to return.
	to_return = buffer[0:firstNewline]
	buffer = buffer[firstNewline + 1:]
	
	return buffer, to_return

# This method takes in a dictionary object (parsed from JSON)
def handleJSONMessage(msg, clientAddr):
	global PlayerTable

	if isValidClientAddr(clientAddr) == False:
		return
	
	message_type = msg['message_type']
	
	if message_type != "player_move":
		# ---- For testing. Send the client whatever the server received.
		conn = PlayerTable[clientAddr]['connection_tcp_socket']
		conn.send(toUTF8(json.dumps(msg) + MESSAGE_DELIMITER))
		# ----
		
		username = PlayerTable[clientAddr]['username']
		if username != None:
			print("Received '{}' from '{}'".format(message_type, username))
		else:
			print("Received '{}' from '{}'".format(message_type, clientAddr))
	
	
	if message_type == "register":
		handleRegisterMessage(msg, clientAddr)
		
	elif message_type == "list_games":
		handleListGamesMessage(msg, clientAddr)
		
	elif message_type == "create_game":
		handleCreateGameMessage(msg, clientAddr)
		
	elif message_type == "join_game":
		handleJoinGameMessage(msg, clientAddr)
		
	elif message_type == "exit_game":
		handleExitGameMessage(msg, clientAddr)
		
	elif message_type == "unregister":
		handleUnregisterMessage(msg, clientAddr)
		
	elif message_type == "player_move":
		handlePlayerMoveMessage(msg, clientAddr)
	
	elif message_type == "chat_incoming":
		handleChatIncomingMessage(msg, clientAddr)
		
	elif message_type == "load_level":
		handleLoadLevelMessage(msg, clientAddr)
		
	else:
		print("Unrecognized message type: {}".format(message_type))
		pass

def isValidClientAddr(clientAddr):
	global PlayerTable
	
	player_data = PlayerTable.get(clientAddr)
	if player_data == None:
		logging.info("isValidClientAddr returning False for clientAddr \"{}\".".format(clientAddr))
		return False
	else:
		return True

def removePlayerFromAllGames(clientAddr):
	global GameTable
	
	for game_id, game in GameTable.items():
		i = 0;
		while i < len(game['client_addrs']):
			if game['client_addrs'][i] == clientAddr:
				del game['client_addrs'][i]
				# next player is now at the current index, so don't increment i
			else:
				i = i + 1
		
		
def handleRegisterMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	player_data = PlayerTable[clientAddr]	
	if player_data['username'] == None: # player hasn't registered yet
		player_data['username'] = msg['username']
		print("Registered new user '{}'. {}"
				.format(player_data['username'], clientAddr))
	else:
		old_username = player_data['username']
		player_data['username'] = msg['username']
		print("'{}' is now known as '{}'. {}"
				.format(old_username, player_data['username'], clientAddr))
	
	handleListGamesMessage(msg, clientAddr)


def handleListGamesMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	conn = PlayerTable[clientAddr]['connection_tcp_socket']
	
	response = {}
	response['message_type'] = 'list_games_reponse'
	#response['text_list_of_games'] = getTextListOfGames().replace('\n', "\\n").replace('\r', "\\r")
	response['text_list_of_games'] = getTextListOfGames().replace('\n', "\\n").replace('\r', "\\r")
	
	sendToClientAddr(clientAddr, response)
	#conn.send(toUTF8(json.dumps(GameTable, sort_keys=True, indent=4) + MESSAGE_DELIMITER))
	print("Sent GameTable to '{}'".format(PlayerTable[clientAddr]['username']))

	
#def getTextListOfGames():
##	GameTable:
##		game_id -> game_name, in_progress, [clientAddr, clientAddr]
#	header = "Games:\n"
#	body = ""
#	for game_id, game in sorted(GameTable.items()):
#		if len(game['client_addrs']) > 0:
#			entry = ""
#			entry += "'" + game['game_name'].strip('\"\'') + "'" + "\n"
#			for clientAddr in game['client_addrs']:
#				entry += "\t" + PlayerTable[clientAddr]['username'] + "\n"
#			#for emptySpaceInGame in range(2 - len(game['client_addrs'])):
#			#	entry += "\t" + "" + "\n"
#			body += entry
#	return header + bodys
	
def getTextListOfGames():
	NEWLINE_CHAR = "NEWLINE"
#	GameTable:
#		game_id -> game_name, in_progress, [clientAddr, clientAddr]
	header = "Games: " + NEWLINE_CHAR
	body = ""
	for game_id, game in sorted(GameTable.items()):
		if len(game['client_addrs']) > 0:
			entry = ""
			entry += game['game_name'] + ":"
			for clientAddr in game['client_addrs']:
				entry += " " + PlayerTable[clientAddr]['username'] + ""
			#for emptySpaceInGame in range(2 - len(game['client_addrs'])):
			#	entry += "\t" + "" + "\n"
			entry += NEWLINE_CHAR
			body += entry
	return header + body
		

# TODO: Validate user has registered. Work out all validation issues, everywhere.
def handleCreateGameMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	global next_game_id
	
	player_data = PlayerTable[clientAddr]
	
	game_id = next_game_id
	next_game_id = next_game_id + 1
	
	game = {'game_name': msg['game_name'],
			'in_progress': False,
			'client_addrs': []}
			
	GameTable[game_id] = game
	
	print("'{}' created game '{}'."
			.format(player_data['username'], game['game_name']))
			
	join_msg_dict = {}
	join_msg_dict['message_type'] = "join_game"
	join_msg_dict['game_name'] = game['game_name']
	handleJoinGameMessage(join_msg_dict, clientAddr)
	handleListGamesMessage(msg, clientAddr)
	
	
	

def handleJoinGameMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	global MP_LEVEL_1_STR
	
	removePlayerFromAllGames(clientAddr)
	
	player_data = PlayerTable[clientAddr]
	
	# find the game to join
	gameToJoin = None
	gameToJoin_game_id = None
	for game_id, game in sorted(GameTable.items()):
		if game['game_name'] == msg['game_name']:
			gameToJoin = game
			gameToJoin_game_id = game_id
			break;
	
	response = {}
	response['message_type'] = "join_game_response"
	if gameToJoin != None:
		
		# if the game is joinable, add the player
		if len(game['client_addrs']) == 0:
			response['player_color'] = "grey"
			game['client_addrs'].append(clientAddr)
			response['join_successful'] = True
			
		elif len(game['client_addrs']) == 1:
			response['player_color'] = "red"
			game['client_addrs'].append(clientAddr)
			response['join_successful'] = True
			
		else:
			response['player_color'] = ""
			response['join_successful'] = False
		
	else:
		response['player_color'] = ""
		response['join_successful'] = False
	
	# send message
	conn = PlayerTable[clientAddr]['connection_tcp_socket']
	conn.send(toUTF8(json.dumps(response) + MESSAGE_DELIMITER))
	if response['join_successful'] == True:
		print("'{}' joined '{}'."
		.format(player_data['username'], game['game_name']))
	else:
		print("'{}' was unable to join '{}'."
		.format(player_data['username'], game['game_name']))
		
	if (len(game['client_addrs']) == 2):
		sendLoadLevelMessageToGameID(gameToJoin_game_id, MP_LEVEL_1_STR)
		
def handleLoadLevelMessage(msg, clientAddr):
	current_game_id = getGameIDForClientAddr(clientAddr)
	sendLoadLevelMessageToGameID(current_game_id, msg['level'])
	
def getGameIDForClientAddr(clientAddr):
	current_game_id = None
	for game_id, game in GameTable.items():
		if clientAddr in game['client_addrs']:
			current_game_id = game_id
			break
	return current_game_id
		
def sendLoadLevelMessageToGameID(current_game_id, level_to_load_str):
	global PlayerTable
	global GameTable
	
	msg_dict = {}
	msg_dict['message_type'] = 'load_level'
	msg_dict['level'] = level_to_load_str
	sendToGameID(current_game_id, msg_dict)
	
def sendToGameID(current_game_id, msg_dict):
	global PlayerTable
	global GameTable
	
	current_game = None
	for game_id, game in GameTable.items():
		if game_id == current_game_id:
			current_game = game
			break
	
	if (current_game == None):
		return
	
	for clientAddr in current_game['client_addrs']:
		sendToClientAddr(clientAddr, msg_dict)
	
def sendToClientAddr(clientAddr, msg_dict):
	global PlayerTable
	global GameTable
	
	conn = PlayerTable[clientAddr]['connection_tcp_socket']
	conn.send(toUTF8(json.dumps(msg_dict) + MESSAGE_DELIMITER))
	
def handleExitGameMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	removePlayerFromAllGames(clientAddr)
	
def handleUnregisterMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	removePlayerFromAllGames(clientAddr)
	
	del PlayerTable[clientAddr]
	
# TODO: Raise performance. 
# TODO: Currently only the expected condition (2 unique players in one game) is handled, all errors are silently dropped. If a player joins a game twice, it's going to cause a problem.
def handlePlayerMoveMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	#print(locals())
	sendToOtherPlayerInGame(msg, clientAddr)

# Takes in the clientAddr of one player, and send the message to other player in same game.
def sendToOtherPlayerInGame(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	# There should be a game with two players in it.
	# Find the game this player is in, and forward the player_move
	# to the other player in the game.
	current_game = None
	for game_id, game in GameTable.items():
		if clientAddr in game['client_addrs']:
			current_game = game
			break
	
	# get the other player's clientAddr
	other_clientAddr = None
	if current_game != None and len(current_game['client_addrs']) == 2:
		if current_game['client_addrs'][0] == clientAddr:
			other_clientAddr = current_game['client_addrs'][1]
		else:
			other_clientAddr = current_game['client_addrs'][0]
	
	# forward the message to the other player's connection
	if other_clientAddr != None:
		other_conn = PlayerTable[other_clientAddr]['connection_tcp_socket']
		other_conn.send(toUTF8(json.dumps(msg) + MESSAGE_DELIMITER))
		logging.info("Forwarded '{}' from '{}' to '{}'.".format(
			msg['message_type'], 
			PlayerTable[clientAddr]['username'], 
			PlayerTable[other_clientAddr]['username']))
	else:
		logging.info("Got a '{}' from '{}', but did not forward it.".format(
			msg['message_type'], 
			PlayerTable[clientAddr]['username']))

def sendToAllPlayers(msg):
	global PlayerTable
	global GameTable
	
	for other_clientAddr in GameTable.keys():
		other_conn = GameTable[other_clientAddr]['connection_tcp_socket']
		try:
			other_conn.send(toUTF8(json.dumps(msg) + MESSAGE_DELIMITER))
		except Exception as e:
			print(e)
	logging.info("Sent '{}' message to all.".format(
					msg['message_type']))
	
	
def sendToAllOtherPlayers(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	for other_clientAddr in GameTable.keys():
		if other_clientAddr != clientAddr:
			other_conn = GameTable[other_clientAddr]['connection_tcp_socket']
			try:
				other_conn.send(toUTF8(json.dumps(msg) + MESSAGE_DELIMITER))
			except Exception as e:
				print(e)
	logging.info("Sent '{}' message from '{}' to all.".format(
					msg['message_type'],
					PlayerTable[clientAddr]['username']))
	
	
		
def handleChatIncomingMessage(msg, clientAddr):
	global PlayerTable
	global GameTable
	
	msg_outgoing = None
	try:
		msg_outgoing = {'message_type': 'chat_outgoing',
						'scope': msg['scope'],
						'text': msg['text'],
						'from': PlayerTable[clientAddr]['username']}
	except KeyError as e:
		print(e)
		return
	
	if msg_outgoing['scope'] == 'game':
		sendToOtherPlayerInGame(msg_outgoing, clientAddr)
	elif msg_outgoing['scope'] == 'all':
		sendToAllPlayers(msg_outgoing)
	
		
def startLogging(debug_level):
	#logging.basicConfig(stream = sys.stdout, level = logging.DEBUG) # Show all log messages.
	#logging.basicConfig(stream = sys.stdout, level = logging.CRITICAL) # Show no log messages.
	
	logging.basicConfig(stream = sys.stdout, level = debug_level) # Show no log messages.

# Any time the server send data to a client, the string should be UTF-8 encoded.
# example:
#	mySocket.send(toUTF8("Hello world!"))
def toUTF8(myString):
	return unicode(myString, "utf-8", 'replace')
	
def handleCrtlC(arg1, arg2):
	global server_must_exit
	global thread_must_exit
	
	server_must_exit = True
	for key in thread_must_exit.keys():
		thread_must_exit[key] = True
	print("Closing server. Waiting for socket.accept() to timeout.")
	sys.exit()
	
def printHelp(currentFilename):
	print("Usage: {}".format(currentFilename))
	print("")
	print("optional arguments:")
	print("  -p, --port=PORT")
	print("  -v, --verbose")
	
if __name__ == '__main__':
	signal.signal(signal.SIGINT, handleCrtlC)
	
	DEFAULT_PORT = 15000
	port = DEFAULT_PORT
	
	DEFAULT_DEBUG_LEVEL = logging.CRITICAL # Show only messages of level CRITICAL or higher. (Shows nothing)
	debug_level = DEFAULT_DEBUG_LEVEL
	
	# Read command line args.
	argsWithoutFilename = sys.argv[1:]
	try:
		opts, args = getopt.getopt(argsWithoutFilename, "p:v",["port=", "verbose"])
	except getopt.GetoptError as e:
		printHelp(sys.argv[0])
		sys.exit()
	for opt, arg in opts:
		if opt in ("-p", "--port"):
			port = int(arg)
		if opt in ("-v", "--verbose"):
			debug_level = logging.DEBUG
	
	main(port, debug_level)