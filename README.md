# iSpy Matchmaker
This program, called the iSpyMatchmaker, is intended to be the room handler for iSpy game. This acts as a receptionist and room handler program for Unity clients who intended to play in one of the matchmaker-made-available Unity servers. The matchmaker will accept connection from any Unity clients. The matchmaker will also open a specified number of Unity servers (called rooms) which will exchange their states with the matchmaker, which will be stored and shared to all clients. Players can then join their intended room and play with friends.

## How to Use
  Before using this program, you must have the [iSpy Unity Server build](https://github.com/marvel-natanael/iSpy.git). Once you have the server build, you must place it inside a folder within the same directory as the matchmaker named "server", which you can create yourself or be created by the matchmaker. To run the matchmaker itself, you have to use a terminal or some sort. Once you run the program, you will:
1. Be asked the full name of the Unity server build which should be located in the server folder within the same directory as the matchmaker.
    - if the server folder doesn't exist, the matchmaker will instantly exit. Requiring you to place the Unity server build inside.
    - The same result will happen if the name of the program isn't valid or the program is not found.
2. Be asked of how many rooms you want to open.
    - Try not to create too many rooms as each rooms are equal to one instance of a whole server build. Creating too many rooms will result in a drop of memory space and response speed.

  After all the inputs are given, the matchmaker will automatically open connnection for both Unity rooms and clients.
  
> **NOTICE:** When the matchmaker is just started, **do not attempt to do any connections from outside of the machine**. This will result in a faulty handler for the rooms.

  After a while, the servers are fully set up and the matchmaker is ready to accept foreign connections. While the servers and matchmaker listener is working on the background, you can input some commands inside the matchmaker terminal. Currently, there are three four commands:
1. Servers: this command will show all Unity room sockets. Useful to see whether all the servers are connected after all inputs were satisfied.
2. Clients: this command will show all Unity client sockets. Useful to see if anything goes wrong with the handler.
3. Entries: this command will show all entries in the matchmaker database. Useful to see servers' port, as well as their max connection count, current player count, and running state.
4. Quit: this command will exit the matchmaker and kills all the processes within RoomHandler.
  
## About The Program
### Transport Protocol
   This software uses TCP as its main transport for communication, both within the same machine and to outside clients. The protocol requires me to differenciate between a Unity room connection and a Unity client connection. Each type of connection has their own packet handlers as they have different things to send. This program is based on [Tom Weiland's Unity Networking Framework](https://github.com/tom-weiland/tcp-udp-networking/), but very customized by me to fit my need for this project.
   
### Room Managing
  For the room managing and executing, I mainly use the [Process class](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0) from Windows. There exists some functions that are very useful in my goal of creating a program that can open multiple instance of the same program, but each with different arguments. Most of the room handling are managed by the [RoomHandler.cs class](https://github.com/LeonardBrowntail/iSpy-Matchmaker/blob/main/RoomHandler.cs).

### Building and Publishing
  This program is intended for Linux use since this is deployed on a Linux server, however a Windows build also exists. Since .NET is a part of Windows, many of the functions behave as intended. The same cannot be said for the Linux build, unfortunately. One of the key thing that is missing from the Linux build was a way for the matchmaker-launched rooms to each have their own separate terminals. On Windows, it works the way it should, but not on Linux. The rooms themselves are still working as intended, it's just that they are all running in the same terminal. _Which sometimes actually makes debugging a bit of a pain..._

### Unity Clients
  This software wouldn't ever work without codes I have implemented within the Unity rooms and clients. This program is only a part of a whole network. To learn more about this program, you should really go and check out [the codes I implemented for the Unity clients and rooms](https://github.com/marvel-natanael/iSpy.git).
