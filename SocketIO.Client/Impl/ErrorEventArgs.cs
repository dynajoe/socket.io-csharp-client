using System;

namespace SocketIO.Client.Impl
{
   internal class ErrorEventArgs : EventArgs
   {
      public ErrorEventArgs(Exception exception)
      {
         Exception = exception;
      }

      public Exception Exception { get; private set; }
   }
}