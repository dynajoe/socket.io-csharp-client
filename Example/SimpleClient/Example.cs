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
