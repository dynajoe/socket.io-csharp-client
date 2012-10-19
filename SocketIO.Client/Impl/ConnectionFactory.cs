namespace SocketIO.Client.Impl
{
   internal class ConnectionFactory : IConnectionFactory
   {
      public virtual ISimpleHttpGetRequest CreateHttpRequest(string uri)
      {
         return new SimpleHttpGetRequest(uri);
      }

      public virtual IWebSocket CreateWebSocket(string uri)
      {
         return new WebSocketWrapper(uri);
      }
   }
}