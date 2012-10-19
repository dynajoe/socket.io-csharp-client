using System.Net;

namespace SocketIO.Client.Impl
{
   internal class SimpleHttpGetRequest : ISimpleHttpGetRequest
   {
      private readonly string m_url;

      public SimpleHttpGetRequest(string url)
      {
         m_url = url;
      }

      public string Execute()
      {
         return new WebClient().DownloadString(m_url);
      }
   }
}