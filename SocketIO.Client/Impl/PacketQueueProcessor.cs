using System;
using System.Collections.Generic;
using System.Threading;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
   internal class PacketQueueProcessor : IPacketQueueProcessor
   {
      private readonly Queue<Packet> m_packetQueue = new Queue<Packet>();

      private readonly EventWaitHandle m_packetWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
      private IWebSocket m_webSocket;

      public IWebSocket WebSocket
      {
         get { return m_webSocket; }
         set
         {
            if (m_webSocket == value)
               return;
            
            if (m_webSocket != null)
            {
               m_webSocket.Opened -= OnOpened;
               m_webSocket.Closed -= OnClosed;
            }
            
            if (value != null)
            {
               value.Opened += OnOpened;
               value.Closed += OnClosed;
            }
            
            m_webSocket = value;
         }
      }

      private void OnOpened(object sender, EventArgs e)
      {
         m_packetWaitHandle.Set();
      }

      private void OnClosed(object sender, EventArgs e)
      {
         m_packetWaitHandle.Set();
      }

      public PacketQueueProcessor()
      {
         ThreadPool.QueueUserWorkItem(ProcessPackets);  
      }

      public void Enqueue(Packet packet)
      {
         m_packetQueue.Enqueue(packet);
         m_packetWaitHandle.Set();
      }

      /// <summary>
      /// This implementation is not safe for multiple threads attempting to send packets.
      /// It's intended to ensure delivery of packets when there are packets to send.
      /// </summary>
      private void ProcessPackets(object state)
      {
         while (true)
         {
            if (WebSocket == null || !WebSocket.Connected || m_packetQueue.Count == 0)
            {
               m_packetWaitHandle.WaitOne();
               continue;
            }

            var packet = m_packetQueue.Peek();

            try
            {
               WebSocket.Write(PacketParser.EncodePacket(packet));
            }
            catch
            {
               continue;
            }

            m_packetQueue.Dequeue();
         }
      }
   }
}