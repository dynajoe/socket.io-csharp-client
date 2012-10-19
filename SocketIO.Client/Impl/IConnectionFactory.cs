namespace SocketIO.Client.Impl
{
   internal interface IConnectionFactory
   {
      ISimpleHttpGetRequest CreateHttpRequest(string uri);

      IWebSocket CreateWebSocket(string uri);
   }
}