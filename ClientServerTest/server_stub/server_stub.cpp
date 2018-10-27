/*
 * Daniel Mamaghani
 * CSS 432
 * 4/7/18
 */
#include <sys/types.h>    // socket, bind
#include <sys/socket.h>   // socket, bind, listen, inet_ntoa
#include <netinet/in.h>   // htonl, htons, inet_ntoa
#include <arpa/inet.h>    // inet_ntoa
#include <netdb.h>        // gethostbyname
#include <unistd.h>       // read, write, close
#include <strings.h>      // bzero
#include <netinet/tcp.h>  // SO_REUSEADDR
#include <sys/uio.h>      // writev

#include <iostream>
#include <iomanip>
#include <sys/time.h>     // gettimeofday
#include <cstdio>         // perror
#include <cstdlib>        // exit
#include <pthread.h>      

using namespace std;

const int ERROR = 1;

/*
 * Print a timestamp (hour:minute:second:millisecond) followed by a message.
 */
void log(const string& message) {
    
    // Use this to turn logging on/off.
    static const bool PRINT_LOG_MESSAGES = true;
    
    if (PRINT_LOG_MESSAGES) {
        time_t currentTime;
        tm *localTime;

        timeval curTimeVal;
        
        time(&currentTime);
        gettimeofday(&curTimeVal, NULL);
        
        uint32_t curTimeUSec = curTimeVal.tv_usec;
        localTime = localtime(&currentTime);
        
        cout << setfill('0') << setw(2) << localTime->tm_hour;
        cout << ":" << setfill('0') << setw(2) << localTime->tm_min;
        cout << ":" << setfill('0') << setw(2) << localTime->tm_sec;
        cout << ":" << setfill('0') << setw(3) << curTimeUSec/1000 << ": ";
        cout << message << endl;
    }
}

void* handleConnection(void* arg) {
    
    int curSd = *(int *)arg;
    
    const uint BUFSIZE = 2000;
    char databuf[BUFSIZE];
    for (int i = 0; i < BUFSIZE; i++) {
		databuf[i] = '\0';
	}

	// read data from the socket, print it to cout immediately.
	// runs until manually terminated with Crtl + C.
	int nRead;
	while (true) {
		nRead = 0;
		nRead += read(curSd, databuf, BUFSIZE - 1);
		if (nRead > 0) {
			log("");
			databuf[nRead] = '\0';
			cout << databuf;
			cout.flush();
		}
	}
    
    pthread_exit(NULL);
}

int main(){
    log("Server started.");
    int port = 15000;
    sockaddr_in acceptSockAddr;
    bzero( (char*)&acceptSockAddr, sizeof( acceptSockAddr ) );
    acceptSockAddr.sin_family      = AF_INET; // Address Family Internet
    acceptSockAddr.sin_addr.s_addr = htonl( INADDR_ANY );
    acceptSockAddr.sin_port        = htons( port );
	
    int serverSd = socket( AF_INET, SOCK_STREAM, 0 );
    if (serverSd == -1) { perror("socket"); }
    
    const int ON = 1;
    setsockopt( serverSd, SOL_SOCKET, SO_REUSEADDR, (char *)&ON, 
                sizeof( int ) );
				
    bind( serverSd, ( sockaddr* )&acceptSockAddr, sizeof( acceptSockAddr ) );
	
    const int MAX_CONNECTIONS = 30;
    listen( serverSd, MAX_CONNECTIONS );

    log("Listening.");
    
    sockaddr_in newSockAddr;
    socklen_t newSockAddrSize = sizeof( newSockAddr );
    
    const uint MAX_NUM_THREADS = 100;
    pthread_t threads[MAX_NUM_THREADS];
    uint num_threads_created = 0;
    
    while (true) {
        int newSd = accept( serverSd, ( sockaddr *)&newSockAddr, &newSockAddrSize );
        if (newSd == -1) { perror("accept"); return(ERROR); }

        log("New connection accepted.");
        
        pthread_create(&threads[num_threads_created], NULL, handleConnection, &newSd);
        num_threads_created++;
    }

    log("Done.");
    
    return 0;
}

