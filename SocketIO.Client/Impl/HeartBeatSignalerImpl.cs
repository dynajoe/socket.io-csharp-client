using System.Timers;

namespace SocketIO.Client.Impl
{
   internal class HeartBeatSignaler : IHeartBeatSignaler
   {
      private IWebSocket m_socket;
      private readonly Timer m_heartBeatTimer;

      public HeartBeatSignaler()
      {
         m_heartBeatTimer = new Timer { Enabled = true, AutoReset = true, Interval = 60000 };
         m_heartBeatTimer.Elapsed += OnHeartBeat;
      }

      private void OnHeartBeat(object sender, ElapsedEventArgs e)
      {
         if (m_socket.Connected)
         {
            m_socket.Write(PacketParser.EncodePacket(new Packet { Type = PacketType.Heartbeat }));
         }
      }

      public void Start(IWebSocket socket, int interval)
      {
         m_socket = socket;
         
         m_heartBeatTimer.Interval = interval;
         m_heartBeatTimer.Start();
      }

      public void Stop()
      {
         m_heartBeatTimer.Stop();
      }
   }
}