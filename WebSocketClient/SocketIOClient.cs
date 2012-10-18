using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Timers;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using Timer = System.Timers.Timer;

namespace WebSocketClient
{
   public class SocketIOClient
   {
      public const string DefaultNamespace = "";

      private const WebSocketVersion SocketVersion = WebSocketVersion.Rfc6455;

      private readonly Queue<Packet> m_packetQueue = new Queue<Packet>();
      
      private readonly Dictionary<string, Namespace> m_nameSpaces = new Dictionary<string, Namespace>();

      private readonly Timer m_heartBeatTimer;

      private WebSocket m_socket;

      private readonly EventWaitHandle m_packetWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

      public SocketIOClient()
      {
         m_heartBeatTimer = new Timer { Enabled = true, AutoReset = true };
         m_heartBeatTimer.Elapsed += OnHeartBeat;

         ThreadPool.QueueUserWorkItem(ProcessPackets);
      }

      public bool AllowUnstrustedCertificate { get; set; }

      public int HeartbeatTimeout { get; private set; }

      public string Id { get; private set; }

      protected string ServerUrl { get; private set; }

      public bool Connected { get { return m_socket != null && m_socket.State == WebSocketState.Open; } }

      public bool Reconnecting { get; private set; }

      public bool Connecting { get { return m_socket != null && m_socket.State == WebSocketState.Connecting; } }

      public void Connect(string serverUrl)
      {
         if (Connected || Connecting || Reconnecting)
            return;

         m_heartBeatTimer.Stop();

         ServerUrl = serverUrl;

         var uri = new Uri(serverUrl);

         var handshakeResult = DoHandshake(uri);

         if (handshakeResult == HandshakeResult.Success)
         {
            Publish("connecting");
            var socketUri = string.Format("{0}://{1}:{2}/socket.io/1/websocket/{3}",
               uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws", uri.Host, uri.Port, Id);

            m_socket = new WebSocket(socketUri, string.Empty, SocketVersion);

            m_socket.AllowUnstrustedCertificate = AllowUnstrustedCertificate;
            m_socket.Opened += OnOpened;
            m_socket.MessageReceived += OnMessageReceived;
            m_socket.Error += OnError;
            m_socket.Closed += OnClosed;
            m_socket.EnableAutoSendPing = false; //Socket.IO has a different mechanism for heartbeats

            m_socket.Open();

            m_heartBeatTimer.Interval = HeartbeatTimeout;
            m_heartBeatTimer.Start();
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

      public void SendPacket(Packet packet)
      {
         m_packetQueue.Enqueue(packet);
         m_packetWaitHandle.Set();
      }

      private void OnHeartBeat(object sender, ElapsedEventArgs e)
      {
         SendPacket(new Packet { Type = PacketType.Heartbeat });
      }

      private HandshakeResult DoHandshake(Uri uri)
      {
         string responseText = null;

         try
         {
            var query = uri.Query + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") +
                        "t=" + Math.Round((DateTimeOffset.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds, 0);

            var handshakeUrl = string.Format("{0}://{1}:{2}/socket.io/1/{3}", uri.Scheme, uri.Host, uri.Port, query);

            responseText = new WebClient().DownloadString(handshakeUrl);

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
         m_packetWaitHandle.Set();
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

      /// <summary>
      /// This implementation is not safe for multiple threads attempting to send packets.
      /// It's intended to ensure delivery of packets when there are packets to send.
      /// </summary>
      private void ProcessPackets(object state)
      {
         while (true)
         {
            if (!Connected || m_packetQueue.Count == 0)
            {
               m_packetWaitHandle.WaitOne();
               continue;
            }

            var packet = m_packetQueue.Peek();

            try
            {
               m_socket.Send(PacketParser.EncodePacket(packet));
            }
            catch
            {
               continue;
            }

            m_packetQueue.Dequeue();
         }
      }

      private void OnClosed(object sender, EventArgs e)
      {
         Publish("disconnect");
      }
   }
}
