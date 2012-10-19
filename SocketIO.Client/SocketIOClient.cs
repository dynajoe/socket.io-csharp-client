using System;
using System.Collections.Generic;
using System.Net;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
   public class SocketIOClient
   {
      private readonly IConnectionFactory m_connectionFactory;
      private readonly IPacketQueueProcessor m_packetQueueProcessor;

      public const string DefaultNamespace = "";
      
      private readonly Dictionary<string, Namespace> m_nameSpaces = new Dictionary<string, Namespace>();

      private IWebSocket m_socket;

      internal SocketIOClient(IConnectionFactory connectionFactory, IPacketQueueProcessor packetQueueProcessor)
      {
         m_connectionFactory = connectionFactory;
         m_packetQueueProcessor = packetQueueProcessor;
      }

      public SocketIOClient()
         : this(new ConnectionFactory(), new PacketQueueProcessor())
      {
      }

      public bool AllowUnstrustedCertificate { get; set; }

      public int HeartbeatTimeout { get; private set; }

      public string Id { get; private set; }

      protected string ServerUrl { get; private set; }

      public bool Connected { get { return m_socket != null && m_socket.Connected; } }

      public bool Reconnecting { get; private set; }

      public bool Connecting { get { return m_socket != null && m_socket.Connecting; } }

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

            var socketUri = string.Format("{0}://{1}:{2}/socket.io/1/websocket/{3}",
               uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws", uri.Host, uri.Port, Id);

            m_socket = m_connectionFactory.CreateWebSocket(socketUri);
            
            m_socket.AllowUnstrustedCertificate = AllowUnstrustedCertificate;
            m_socket.Opened += OnOpened;
            m_socket.MessageReceived += OnMessageReceived;
            m_socket.Error += OnError;
            m_socket.Closed += OnClosed;

            m_socket.Open();
         }
      }

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

      internal void SendPacket(Packet packet)
      {
         m_packetQueueProcessor.Enqueue(packet);
      }

      private HandshakeResult DoHandshake(Uri uri)
      {
         string responseText = null;

         try
         {
            var query = uri.Query + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") +
                        "t=" + Math.Round((DateTimeOffset.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds, 0);

            var handshakeUrl = string.Format("{0}://{1}:{2}/socket.io/1/{3}", uri.Scheme, uri.Host, uri.Port, query);

            responseText = m_connectionFactory.CreateHttpRequest(handshakeUrl).Execute();

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
            }

            return HandshakeResult.Error;
         }
         catch (Exception)
         {
            Publish("error", responseText);
            return HandshakeResult.Error;
         }

         return HandshakeResult.Success;
      }

      private void Publish(string eventName, string data = null)
      {
         foreach (var item in m_nameSpaces)
         {
            item.Value.EmitLocally(eventName, data);
         }
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

     
      private void OnClosed(object sender, EventArgs e)
      {
         Publish("disconnect");
      }
   }
}
