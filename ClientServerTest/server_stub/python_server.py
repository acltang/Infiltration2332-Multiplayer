# Using python 2.7
# Linux lab uses 2.7
import sys
import socket
import threading
import time
import signal

server_must_exit = False

def main():
	global server_must_exit

	# We start the server itself in a new thread. This is so that the
	# main thread is available to catch keyboard interrupts from the terminal.
	# If socket.accept() is blocking in the main thread, the only way to kill
	# the program from the terminal is to use the "Crtl + Pause/Break" key combo.
	#thread.start_new_thread(runServer)
	threading.Thread(target = runServer).start()
	
	# while the server is running, sleep the main thread for a long time.
	# Calling sleep minimizes the performance cost of this thread.
	# If the server is left running for more than 1 day, it will close.
	ONE_DAY_IN_SECONDS = 86400
	time.sleep(ONE_DAY_IN_SECONDS)
	server_must_exit = True
	
def runServer():
	print("runServer called.")
	global server_must_exit
	# open a socket to listen for connections
	HOST = '' # The empty string for HOST means, "Run the server locally." It is also acceptable to use 'localhost' or '127.0.0.1' instead.
	SERVER_PORT = 15000
	serverSd = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # use TCP
	serverSd.bind((HOST, SERVER_PORT))
	serverSd.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, True) # when the socket is closed, have the OS make it available for re-use right away.
	serverSd.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, True)
	serverSd.listen(True)
	
	connections = []
	
	#print("Beginning while loop.")
	while server_must_exit == False:
		try:
			serverSd.settimeout(1) # 1 second timeout. 
			clientConnection, clientAddr = serverSd.accept()
			connections.append((clientConnection, clientAddr))
			#thread.start_new_thread(handleClientConnection, (clientConnection, clientAddr))
			threading.Thread(target = handleClientConnection, args = (clientConnection, clientAddr)).start()
		except socket.timeout:
			#print("socket.accept() timed out. server_must_exit: {}".format(server_must_exit))
			pass
	
	print("returning from runServer.")
			
def handleClientConnection(conn, clientAddr):
	global server_must_exit
	print("sorry! TODO")
		


def handleCrtlC(arg1, arg2):
	global server_must_exit
	server_must_exit = True
	print("Closing server. Waiting for socket.accept() to timeout.")
	sys.exit()
	
if __name__ == '__main__':
	signal.signal(signal.SIGINT, handleCrtlC)
	main()