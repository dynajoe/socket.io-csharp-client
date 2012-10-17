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
         
         client.Of("/Poop").On("data", (a, b) =>
         {
            Console.WriteLine(a);

            if (b != null)
            {
               b(null);
            }                                
         }).On("data2", (a,b) => { });
         
         client.Connect(args.Length > 0 ? args[0] : "http://illum-qa-alpha:8081/?sid=abc123");

         Console.ReadLine();
      }

   }
}
