using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace SocketIO.Client
{
   public class Namespace
   {
      private readonly SocketIOClient m_socket;

      private int m_ackPacketCount;

      private readonly Dictionary<string, List<Action<string, Action<string>>>> m_eventListeners =
         new Dictionary<string, List<Action<string, Action<string>>>>();

      private readonly ReaderWriterLockSlim m_eventListenerLock = new ReaderWriterLockSlim();

      private readonly Dictionary<string, Action<string>> m_acks = new Dictionary<string, Action<string>>();

      public Namespace(string name, SocketIOClient socket)
      {
         m_socket = socket;
         Name = name;
      }

      public void HandlePacket(Packet packet)
      {
         Action<string> ack = args => m_socket.SendPacket(new Packet { Type = PacketType.Ack, Args = args, AckId = packet.Id });

         switch (packet.Type)
         {
            case PacketType.Connect:
               EmitLocally("connect");
               break;
            case PacketType.Disconnect:
               EmitLocally("disconnect", packet.Reason);
               break;
            case PacketType.Message:
            case PacketType.Json:
               if (packet.Ack != null && packet.Ack != "data")
               {
                  ack(null);
               }

               EmitLocally("message", packet.Data, packet.Ack == "data" ? ack : null);
               break;
            case PacketType.Event:
               EmitLocally(packet.Name, packet.Args, packet.Ack == "data" ? ack : null);
               break;
            case PacketType.Ack:
               if (m_acks.ContainsKey(packet.AckId))
               {
                  var ackToCall = m_acks[packet.AckId];
                  m_acks.Remove(packet.AckId);
                  ackToCall(packet.Args);
               }
               break;
            case PacketType.Error:
               EmitLocally(packet.Reason == "unauthorized" ? "connect_failed" : "error", packet.Reason);
               break;
         }
      }

      public void EmitLocally(string eventName, string data = null, Action<string> ack = null)
      {
         if (string.IsNullOrEmpty(eventName) || !m_eventListeners.ContainsKey(eventName)) 
            return;
         
         m_eventListenerLock.EnterReadLock();

         var callbacks = m_eventListeners[eventName];

         foreach (var cb in callbacks)
         {
            try { cb(data, ack); } catch { /* Intentionally suppress errors blank. */ }
         }

         m_eventListenerLock.ExitReadLock();
      }

      public string Name { get; private set; }

      public void Connect()
      {
         SendPacket(new Packet { Type = PacketType.Connect });
      }
      
      public void Emit(string eventName, string data, Action<string> ack = null)
      {
         var packet = new Packet {Type = PacketType.Event, Name = eventName, Data = data};

         if (ack != null)
         {
            packet.Ack = "data";
            packet.Id = (++m_ackPacketCount).ToString(CultureInfo.InvariantCulture);
            m_acks[packet.Id] = ack;
         }

         SendPacket(packet);
      }
      
      public Namespace On(string eventName, Action<string, Action<string>> callback)
      {
         if (callback != null && eventName != null)
         {
            m_eventListenerLock.EnterWriteLock();

            if (m_eventListeners.ContainsKey(eventName))
            {
               m_eventListeners[eventName].Add(callback);
            }
            else
            {
               m_eventListeners[eventName] = new List<Action<string, Action<string>>> { callback };
            }

            m_eventListenerLock.ExitWriteLock();
         }

         return this;
      }

      public Namespace RemoveListener(string eventName, Action<string, Action<string>> callback)
      {
         if (callback != null && eventName != null && m_eventListeners.ContainsKey(eventName))
         {
            m_eventListenerLock.EnterWriteLock();
            
            m_eventListeners[eventName].Remove(callback);
          
            m_eventListenerLock.ExitWriteLock();
         }

         return this;
      }

      public void Disconnect()
      {
         if (Name == SocketIOClient.DefaultNamespace)
         {
            m_socket.Disconnect();
            return;
         }

         SendPacket(new Packet { Type = PacketType.Disconnect});
      }

      private void SendPacket(Packet packet)
      {
         packet.EndPoint = Name;

         m_socket.SendPacket(packet);
      }
   }
}
