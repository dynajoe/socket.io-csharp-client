using System;
using SocketIO.Client;

namespace SimpleClient
{   class Example
   {
      static void Main()
      {
         var io = new SocketIOClient();
         
         var socket = io.Connect("http://localhost:3000/");

         socket.On("data", (args, callback) =>
         {
            Console.WriteLine("Server sent:");

            for (int i = 0; i < args.Length; i++)
            {
               Console.WriteLine("[" + i + "] => " + args[i]);
            }
         });
         
         string line;
         
         while ((line = Console.ReadLine()) != "q")
         {
            socket.Emit("data", line);
         }
      }
   }
}
