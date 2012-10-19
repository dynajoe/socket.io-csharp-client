using System;
using FakeItEasy;
using Machine.Specifications;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
#pragma warning disable 169
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

   public class SocketIOClientSpec
   {
      protected static SocketIOClient subject;
      internal static ISimpleHttpGetRequest fakeHttp;
      internal static IWebSocket fakeWebSocket;
      internal static IConnectionFactory factory;
      internal static IPacketQueueProcessor packetQueueProcessor;

      protected static bool isConnected;
      protected static string handshakeResponse = "9AdwZi96P9Vf4usPhuxt:60:60:websocket,htmlfile,xhr-polling,jsonp-polling";
      
      Establish context = () =>
      {
         factory = A.Fake<IConnectionFactory>();
         fakeHttp = A.Fake<ISimpleHttpGetRequest>();
         fakeWebSocket = A.Fake<IWebSocket>();
         packetQueueProcessor = A.Fake<IPacketQueueProcessor>();
         
         A.CallTo(() => factory.CreateHttpRequest(A<string>._)).Returns(fakeHttp);
         A.CallTo(() => fakeHttp.Execute()).ReturnsLazily(() => handshakeResponse);
         A.CallTo(() => factory.CreateWebSocket(A<string>._)).Returns(fakeWebSocket);
         A.CallTo(() => fakeWebSocket.Connected).Returns(true);

         A.CallTo(() => fakeWebSocket.Open()).Invokes(() =>
         {
            isConnected = true;
            fakeWebSocket.Opened += Raise.With(fakeWebSocket, EventArgs.Empty).Now;
         });

         A.CallTo(() => fakeWebSocket.Close()).Invokes(() =>
         {
            isConnected = false;
            fakeWebSocket.Closed += Raise.With(fakeWebSocket, EventArgs.Empty).Now;
         });

         subject = new SocketIOClient(factory, packetQueueProcessor);
      };
   }

   public class WhenTheClientConnects : SocketIOClientSpec
   {
      private static bool connectRaised;

      Establish context = () => subject.On("connect", (a, b) =>
      {
         connectRaised = true;
      });

      Because of = () => subject.Connect("http://localhost:3000");

      It should_fire_the_connect_event = () => connectRaised.ShouldBeTrue();
   }

   public class WhenTheClientDisconnects : SocketIOClientSpec
   {
      private static bool disconnectRaised;

      Establish context = () => subject.On("disconnect", (a, b) =>
      {
         disconnectRaised = true;
      });

      Because of = () => { subject.Connect("http://localhost:3000"); subject.Disconnect(); };

      It should_fire_the_disconnect_event = () => disconnectRaised.ShouldBeTrue();
   }

   public class WhenAnEventIsEmitted : SocketIOClientSpec
   {
      Establish context = () =>
      {
         subject.Connect("http://localhost:3000/");
         subject.Emit("eventName", "data");
      };
   
      It should_send_the_event_packet = () => A.CallTo(() => packetQueueProcessor.Enqueue(A<Packet>._)).MustHaveHappened();
   }

#pragma warning restore 169
// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMember.Local
}



