namespace SocketIO.Client.Impl
{
   internal interface IPacketQueueProcessor
   {
      IWebSocket WebSocket { get; set; }
      
      void Enqueue(Packet packet);
   }
}