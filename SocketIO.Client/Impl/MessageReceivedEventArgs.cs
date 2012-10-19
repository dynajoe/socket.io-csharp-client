using System;

namespace SocketIO.Client.Impl
{
   internal class MessageReceivedEventArgs : EventArgs
   {
      public MessageReceivedEventArgs(string message)
      {
         Message = message;
      }

      public string Message { get; private set; }
   }
}
