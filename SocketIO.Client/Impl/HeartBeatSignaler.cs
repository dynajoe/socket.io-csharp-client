namespace SocketIO.Client.Impl
{
   internal interface IHeartBeatSignaler
   {
      void Start(IWebSocket socket, int timeout);

      void Stop();
   }
}