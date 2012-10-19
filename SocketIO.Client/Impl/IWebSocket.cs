using System;

namespace SocketIO.Client.Impl
{
   internal interface IWebSocket
   {
      event EventHandler Opened;

      event EventHandler<MessageReceivedEventArgs> MessageReceived;

      event EventHandler<ErrorEventArgs> Error;

      event EventHandler Closed;
      
      bool Connected { get; }
      
      bool Connecting{ get; }
      
      bool EnableAutoSendPing { get; set; }

      bool AllowUnstrustedCertificate { get; set; }

      void Open();

      void Write(string data);
    
      void Close();
   }
}