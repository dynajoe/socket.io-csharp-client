using System;
using SocketIO.Client;

namespace SimpleClient
{   class Example
   {
      static void Main()
      {
         var io = new SocketIOClient();
         
         var socket = io.Connect("http://localhost:3000/");

         socket.On("data", (data, callback) => Console.WriteLine("Server sent: " + data));
         
         string line;
         
         while ((line = Console.ReadLine()) != "q")
         {
            socket.Emit("data", line);
         }
      }
   }
}
