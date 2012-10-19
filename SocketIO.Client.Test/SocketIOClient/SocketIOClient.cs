using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SocketIO.Client
{
   [TestClass]
   class SocketIOClientTest
   {
      [TestMethod]
      public void ConnectEventFires()
      {
         var client = new SocketIOClient();

         client.On("connect", (a, b) =>
         {
                                  
         });

         client.Connect("http://localhost:3000");
      }
   }
}
