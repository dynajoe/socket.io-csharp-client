using System;

namespace WebSocketClient
{
   class EntryPoint
   {
      static void Main(string[] args)
      {
         var client = new SocketIOClient();
         
         client.On("data", Console.WriteLine);

         client.Connect(args.Length > 0 ? args[0] : "http://illum-qa-alpha:8081/?sid=abc123");

         Console.ReadLine();
      }

   }
}
