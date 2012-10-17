using System;

namespace WebSocketClient
{
   class EntryPoint
   {
      static void Main(string[] args)
      {
         var client = new SocketIOClient();
         
         client.On("data", (a, b) =>
         {
            Console.WriteLine(a);
            
            if (b != null)
            {
               b(null);
            }            
         });

         client.Connect(args.Length > 0 ? args[0] : "http://localhost:3000");

         Console.ReadLine();
      }

   }
}
