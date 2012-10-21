using System;
using WebSocket4Net;

namespace SocketIO.Client.Impl
{
   internal class WebSocketWrapper : IWebSocket
   {
      private readonly WebSocket m_webSocket;

      public WebSocketWrapper(string uri)
      {
         m_webSocket = new WebSocket(uri, string.Empty, WebSocketVersion.Rfc6455);
         m_webSocket.EnableAutoSendPing = false;
         m_webSocket.MessageReceived += OnMessageReceived;
         m_webSocket.Error += OnError;
      }

      public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

      public event EventHandler<ErrorEventArgs> Error = delegate { };

      public event EventHandler Opened
      {
         add { m_webSocket.Opened += value; }
         remove { m_webSocket.Opened -= value; }
      }

      public event EventHandler Closed
      {
         add { m_webSocket.Closed += value; }
         remove { m_webSocket.Closed -= value; }
      }

      public bool Connected { get { return m_webSocket.State == WebSocketState.Open; } }

      public bool Connecting { get { return m_webSocket.State == WebSocketState.Connecting; } }

      public bool AllowUnstrustedCertificate
      {
         get { return m_webSocket.AllowUnstrustedCertificate; }
         set { m_webSocket.AllowUnstrustedCertificate = value; }
      }

      public void Open()
      {
         m_webSocket.Open();
      }

      public void Write(string data)
      {
         m_webSocket.Send(data);
      }

      public void Close()
      {
         m_webSocket.Close();
      }

      private void OnMessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
      {
         MessageReceived(this, new MessageReceivedEventArgs(e.Message));
      }

      private void OnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
      {
         Error(this, new ErrorEventArgs(e.Exception));
      }
   }
}