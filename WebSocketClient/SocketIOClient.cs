using System;
using System.Collections.Generic;
using System.Net;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace WebSocketClient
{
   public class SocketIOClient
   {
      public const string DefaultNamespace = "";

      private const WebSocketVersion SocketVersion = WebSocketVersion.Rfc6455;

      private readonly Dictionary<string, Namespace> m_nameSpaces = new Dictionary<string, Namespace>();

      private WebSocket m_socket;

      public bool AllowUnstrustedCertificate { get; set; }

      public bool Connected { get { return m_socket != null && m_socket.State == WebSocketState.Open; } }

      public bool Reconnecting { get; private set; }

      public bool Connecting { get { return m_socket != null && m_socket.State == WebSocketState.Connecting; } }

      public void Connect(string serverUrl)
      {
         if (Connected || Connecting || Reconnecting)
            return;

         ServerUrl = serverUrl;
         
         var uri = new Uri(serverUrl);

         var handshakeResult = DoHandshake(uri);

         if (handshakeResult == HandshakeResult.Success)
         {
            Publish("connecting");

            m_socket = new WebSocket(
               string.Format("{0}://{1}:{2}/socket.io/1/websocket/{3}", uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws", uri.Host, uri.Port, Id),
               string.Empty,
               SocketVersion);

            m_socket.AllowUnstrustedCertificate = AllowUnstrustedCertificate;
            m_socket.Opened += OnOpened;
            m_socket.MessageReceived += OnMessageReceived;
            m_socket.Error += OnError;
            m_socket.Closed += OnClosed;

            m_socket.Open();
         }
      }

      protected string ServerUrl { get; private set; }

      private HandshakeResult DoHandshake(Uri uri)
      {
         string responseText = null;

         try
         {
            var query = uri.Query + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") +
               "t=" + (DateTimeOffset.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            responseText = new WebClient().DownloadString(string.Format("{0}://{1}:{2}/socket.io/1/{3}", uri.Scheme, uri.Host, uri.Port, query));
            var resultParts = responseText.Split(new[] { ':' });

            Id = resultParts[0];
            HeartbeatTimeout = Int32.Parse(resultParts[1]) * 1000;
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

               Publish("error", responseText);
               return HandshakeResult.Error;
            }
         }
         catch (Exception)
         {
            Publish("error", responseText);
            return HandshakeResult.Error;
         }

         return HandshakeResult.Success;
      }

      public int HeartbeatTimeout { get; private set; }

      public string Id { get; private set; }

      public void On(string eventName, Action<string, Action<string>> callback)
      {
         Of(DefaultNamespace).On(eventName, callback);
      }

      public void Emit(string eventName, string data)
      {
         Of(DefaultNamespace).Emit(eventName, data);
      }

      public void Disconnect()
      {
         var wasConnected = Connected || Connecting;

         m_socket.Close();

         if (wasConnected)
         {
            Publish("disconnect");
         }
      }

      private void Publish(string eventName, string data = null)
      {
         foreach(var item in m_nameSpaces)
         {
            item.Value.EmitLocally(eventName, data);
         }
      }

      public Namespace Of(string name)
      {
         if (name == null || string.IsNullOrEmpty(name.Trim()))
         {
            name = DefaultNamespace;
         }

         if (m_nameSpaces.ContainsKey(name))
         {
            return m_nameSpaces[name];
         }
         
         m_nameSpaces[name] = new Namespace(name, this);
         
         if (name != DefaultNamespace)
         {
            m_nameSpaces[name].Connect();
         }

         return m_nameSpaces[name];
      }

      private void OnOpened(object sender, EventArgs e)
      {
         Publish("connect");
      }

      private void OnError(object sender, ErrorEventArgs e)
      {
         Publish("error", e.Exception.Message);
      }

      private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
      {
         var packet = PacketParser.DecodePacket(e.Message);

         if (packet.Type == PacketType.Error && packet.Advice != null)
         {
            if (packet.Advice == "reconnect" && (Connected || Connecting))
            {
               Disconnect();
               Reconnect();
            }

            Publish("error", packet.Reason);
         }
         else
         {
            Of(packet.EndPoint).HandlePacket(packet);
         }
      }

      public void Reconnect()
      {
         Reconnecting = true;

         try
         {
            Connect(ServerUrl);
         }
         finally
         {
            Reconnecting = false;
         }
      }

      public void SendPacket(Packet packet)
      {
         if (Connected)
         {
            m_socket.Send(PacketParser.EncodePacket(packet));
         }
         else
         {
            //Buffer up the packets until the connection is made
         }
      }

      private void OnClosed(object sender, EventArgs e)
      {
         Publish("disconnect");
      }
   }
}
