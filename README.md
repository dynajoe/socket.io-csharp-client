# C# Socket.IO client
A simple C# implementation of the Socket.IO client using [WebSocket4Net](http://websocket4net.codeplex.com/).

## Usage

```JavaScript

```

```CSharp
using System;
using SocketIO.Client;

namespace SimpleClient
{
   class Program
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