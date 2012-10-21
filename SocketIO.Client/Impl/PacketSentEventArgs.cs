using System;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
   internal class PacketSentEventArgs : EventArgs
   {
      public PacketSentEventArgs(Packet packet)
      {
         Packet = packet;
      }

      public Packet Packet { get; private set; }
   }
}