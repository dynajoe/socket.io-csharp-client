using System;
using FakeItEasy;
using NUnit.Framework;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
   // ReSharper disable InconsistentNaming
   [TestFixture]
   public class Client
   {
      internal ISimpleHttpGetRequest fakeHttp;
      internal IWebSocket fakeWebSocket;
      internal IConnectionFactory factory;
      internal IPacketQueueProcessor packetQueueProcessor;
      protected bool isConnected;
      protected string handshakeResponse = "9AdwZi96P9Vf4usPhuxt:60:60:websocket,htmlfile,xhr-polling,jsonp-polling";
      private SocketIOClient io;
      private Namespace socket;
      private IHeartBeatSignaler heartBeatSignaler;

      [SetUp]
      public void Setup()
      {
         factory = A.Fake<IConnectionFactory>();
         fakeHttp = A.Fake<ISimpleHttpGetRequest>();
         fakeWebSocket = A.Fake<IWebSocket>();
         packetQueueProcessor = A.Fake<IPacketQueueProcessor>();
         heartBeatSignaler = A.Fake<IHeartBeatSignaler>();

         A.CallTo(() => factory.CreateHttpRequest(A<string>._)).Returns(fakeHttp);
         A.CallTo(() => fakeHttp.Execute()).ReturnsLazily(() => handshakeResponse);
         A.CallTo(() => factory.CreateWebSocket(A<string>._)).Returns(fakeWebSocket);
         A.CallTo(() => fakeWebSocket.Connected).ReturnsLazily(() => isConnected);

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

         io = new SocketIOClient(factory, packetQueueProcessor, heartBeatSignaler);

         socket = io.Connect("http://localhost:3000");
      }

      [TearDown]
      public void TearDown()
      {
         isConnected = false;
         factory = null;
         fakeHttp = null;
         fakeWebSocket = null;
         packetQueueProcessor = null;
         socket = null;
      }

      [Test]
      public void WhenTheClientConnects_ItShouldRaiseTheConnectedEvent()
      {
         bool connectRaised = false;

         socket.On("connect", (a, b) =>
         {
            connectRaised = true;
         });

         Assert.IsTrue(connectRaised);
      }

      [Test]
      public void WhenTheClientDisconnects_ItShouldRaiseTheDisconnectEvent()
      {
         var disconnected = false;

         socket.On("disconnect", (a, b) =>
         {
            disconnected = true;
         });

         socket.Disconnect();

         Assert.IsTrue(disconnected);
      }

      [Test]
      public void WhenEmitOnTheClient_ItShouldBeEmittedWithTheDefaultNamespace()
      {
         var expected = new Packet
         {
            Type = PacketType.Event,
            Name = "eventName",
            Data = "[\"data\"]",
            EndPoint = string.Empty
         };

         socket.Emit("eventName", "data");
         
         A.CallTo(() => packetQueueProcessor.Enqueue(A<Packet>.That.Matches(a => a.Equals(expected)))).MustHaveHappened();
      }

      [Test]
      public void WhenEmitOnTheDefaultNamespace_ItShouldSendTheCorrectPacket()
      {
         var expected = new Packet
         {
            Type = PacketType.Event,
            Name = "eventName",
            Data = "[\"data\"]",
            EndPoint = string.Empty
         };

         io.Of("").Emit("eventName", "data");

         io.Of(null).Emit("eventName", "data");

         A.CallTo(() => packetQueueProcessor.Enqueue(A<Packet>.That.Matches(a => a.Equals(expected)))).MustHaveHappened(Repeated.Exactly.Times(2));
      }

      [Test]
      public void WhenEmitOnASpecificNamespace_ItShouldSendTheCorrectPacket()
      {
         var expected = new Packet
         {
            Type = PacketType.Event,
            Name = "eventName",
            Data = "[\"data\"]",
            EndPoint = "/ns"
         };

         io.Of("/ns").Emit("eventName", "data");

         A.CallTo(() => packetQueueProcessor.Enqueue(A<Packet>.That.Matches(a => a.Equals(expected)))).MustHaveHappened();
      }
      [Test]
      public void WhenEmitWithMultipleArgs_ItShouldSendTheCorrectPacket()
      {
         var expected = new Packet
         {
            Type = PacketType.Event,
            Name = "eventName",
            Data = "[\"a\",\"b\",\"c\",\"d\"]",
            EndPoint = ""
         };

         socket.Emit("eventName", "a", "b", "c", "d");

         A.CallTo(() => packetQueueProcessor.Enqueue(A<Packet>.That.Matches(a => a.Equals(expected)))).MustHaveHappened();
      }

      [Test]
      public void WhenAnEventComesIn_ItShouldExecuteEventListeners()
      {
         var expected = new Packet
         {
            Type = PacketType.Event,
            Name = "eventName",
            Args = "[\"data\"]",
            EndPoint = ""
         };

         object[] actualData = null;

         socket.On("data", (data, callback) => { actualData = data; });

         fakeWebSocket.MessageReceived += Raise.With(fakeWebSocket, new MessageReceivedEventArgs(PacketParser.EncodePacket(expected))).Now;
         
         Assert.AreEqual(expected.Data, actualData);
      }
   }
   // ReSharper restore InconsistentNaming
}
