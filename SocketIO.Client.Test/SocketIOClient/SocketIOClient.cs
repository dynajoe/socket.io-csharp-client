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
      private const string HandshakeResponse = "9AdwZi96P9Vf4usPhuxt:60:60:websocket,htmlfile,xhr-polling,jsonp-polling";

      protected static SocketIOClient subject;
      internal static ISimpleHttpGetRequest fakeHttp;
      internal static IWebSocket fakeWebSocket;
      internal static IConnectionFactory factory;
      protected static bool isConnected;
      
      Establish context = () =>
      {
         factory = A.Fake<IConnectionFactory>();
         fakeHttp = A.Fake<ISimpleHttpGetRequest>();
         fakeWebSocket = A.Fake<IWebSocket>();

         A.CallTo(() => factory.CreateHttpRequest(A<string>._)).Returns(fakeHttp);
         A.CallTo(() => fakeHttp.Execute()).Returns(HandshakeResponse);
         A.CallTo(() => factory.CreateWebSocket(A<string>._)).Returns(fakeWebSocket);

         A.CallTo(() => fakeWebSocket.Open()).Invokes(() =>
         {
            isConnected = true;
            A.CallTo(() => fakeWebSocket.Connected).ReturnsLazily(() => isConnected);
            fakeWebSocket.Opened += Raise.With(fakeWebSocket, EventArgs.Empty).Now;
         });

         A.CallTo(() => fakeWebSocket.Close()).Invokes(() =>
         {
            isConnected = false;
            A.CallTo(() => fakeWebSocket.Connected).ReturnsLazily(() => isConnected);
            fakeWebSocket.Closed += Raise.With(fakeWebSocket, EventArgs.Empty).Now;
         });

         subject = new SocketIOClient(factory);
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

#pragma warning restore 169
// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMember.Local
}



