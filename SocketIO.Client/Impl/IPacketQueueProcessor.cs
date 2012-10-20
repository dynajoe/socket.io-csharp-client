using SocketIO.Client.Impl;

namespace SocketIO.Client
{
   internal interface IPacketQueueProcessor
   {
      IWebSocket WebSocket { get; set; }
      
      void Enqueue(Packet packet);
   }
}