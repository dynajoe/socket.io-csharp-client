# C# Socket.IO client
A simple C# implementation of the Socket.IO client using [WebSocket4Net](http://websocket4net.codeplex.com/).

*DISCLAIMER: This is not a complete implementation of the Socket.IO client. There are lots of missing parts. Please feel free to submit a pull-request to add whatever feature is missing that you need.*

## Usage

#### Server

```JavaScript
var io = require("socket.io").listen(3000);

io.sockets.on("connection", function (socket) {
   socket.on("data", function (data) {
      console.log("Client sent: " + data);
      
      if (data) {
         socket.emit("data", data.toUpperCase());
      }
   });
});
```

Start the server by running ```npm install``` then ```node index.js``` from the ```Examples/Server``` folder.

#### Client

```CSharp
using System;
using SocketIO.Client;

namespace SimpleClient
{
   class Example
   {
      static void Main()
      {
         var client = new SocketIOClient();

         client.On("data", (data, callback) => Console.WriteLine("Server sent: " + data));

         client.Connect("http://localhost:3000/");
         
         string line;
         
         while ((line = Console.ReadLine()) != "q")
         {
            client.Emit("data", line);
         }
      }
   }
}
```

Run the C# client and interact with the server by typing anything into the console.