using System;
using System.Collections.Generic;
using System.Net;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace WebSocketClient
{
   public class SocketIOClient
   {
      private static WebSocketVersion SocketVersion = WebSocketVersion.Rfc6455;

      private readonly Dictionary<string, List<Action<string>>> m_eventListeners =
         new Dictionary<string, List<Action<string>>>();

      private WebSocket m_socket;

      public SocketIOClient()
      {
      }

      public bool AllowUnstrustedCertificate { get; set; }

      public bool Connected { get { return m_socket != null && m_socket.State == WebSocketState.Open; } }

      public bool Reconnecting { get; private set; }

      public bool Connecting { get { return m_socket != null && m_socket.State == WebSocketState.Connecting; } }

      public void Connect(string serverUrl)
      {
         if (Connected || Connecting || Reconnecting)
            return;

         var uri = new Uri(serverUrl);

         var handshakeResult = DoHandshake(uri);

         if (handshakeResult == HandshakeResult.Success)
         {
            m_socket = new WebSocket(
               string.Format("{0}://{1}:{2}/socket.io/1/websocket/{3}", uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws", uri.Host, uri.Port, Id),
               string.Empty,
               SocketVersion);

            m_socket.AllowUnstrustedCertificate = AllowUnstrustedCertificate;
            m_socket.Opened += OnOpened;
            m_socket.MessageReceived += OnMessageReceived;
            m_socket.Error += OnError;
            m_socket.DataReceived += OnDataReceived;
            m_socket.Closed += OnClosed;

            m_socket.Open();
         }

      }

      private HandshakeResult DoHandshake(Uri uri)
      {
         string responseText = null;

         try
         {
            var query = uri.Query + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") + 
               "t=" + (DateTimeOffset.UtcNow - new DateTime(1970, 1, 1, 0,0,0, DateTimeKind.Utc)).TotalMilliseconds;

            responseText = new WebClient().DownloadString(string.Format("{0}://{1}:{2}/socket.io/1/{3}", uri.Scheme, uri.Host, uri.Port, query));
            var resultParts = responseText.Split(new[] { ':' });

            Id = resultParts[0];
            HeartbeatTimeout = Int32.Parse(resultParts[1]);
            ConnectionTimeout = Int32.Parse(resultParts[2]);
         }
         catch (WebException we)
         {
            if (we.Status == WebExceptionStatus.ProtocolError)
            {
               var resp = we.Response as HttpWebResponse;

               if (resp != null && resp.StatusCode == HttpStatusCode.Forbidden)
               {
                  return HandshakeResult.Forbidden;
               }
               
               Raise("error", responseText);
               return HandshakeResult.Error;
            }
         }
         catch (Exception)
         {
            Raise("error", responseText);
            return HandshakeResult.Error;
         }

         return HandshakeResult.Success;
      }

      public int HeartbeatTimeout { get; private set; }

      public int ConnectionTimeout { get; private set; }

      public string Id { get; private set; }
      
      private void Raise(string eventName, string data)
      {
      }

      public void On(string eventName, Action<string> callback)
      {
         if (m_eventListeners.ContainsKey(eventName))
         {
            m_eventListeners[eventName].Add(callback);
         }
         else
         {
            m_eventListeners[eventName] = new List<Action<string>> { callback };
         }
      }

      public void RemoveListener(string eventName, Action<string> callback)
      {
         if (m_eventListeners.ContainsKey(eventName))
         {
            m_eventListeners[eventName].Remove(callback);
         }
      }

      public void Send(string message)
      {

      }

      public void Emit(string eventName, string data)
      {

      }

      public void Disconnect()
      {

      }

      private void OnOpened(object sender, EventArgs e)
      {

      }

      private void OnError(object sender, ErrorEventArgs e)
      {

      }

      private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
      {
         var messageParts = e.Message.Split(new[] { ':' }, 4);

         Console.WriteLine(e.Message);
      }

      private void OnDataReceived(object sender, DataReceivedEventArgs e)
      {
         Console.WriteLine(e.Data);
      }

      private void OnClosed(object sender, EventArgs e)
      {

      }
   }
}
