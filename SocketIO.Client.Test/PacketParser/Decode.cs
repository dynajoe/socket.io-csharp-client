using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SocketIO.Client
{
   [TestClass]
   public class PacketParserDecode
   {
      [TestMethod]
      public void DecodeHeartBeat()
      {
         var packet = PacketParser.DecodePacket("2::");
         Assert.AreEqual(PacketType.Heartbeat, packet.Type);
         Assert.AreEqual("", packet.EndPoint);
      }

      [TestMethod]
      public void DecodeConnect()
      {
         var packet = PacketParser.DecodePacket("1::");
         Assert.AreEqual(PacketType.Connect, packet.Type);
      }

      [TestMethod]
      public void DecodeConnectToEndpoint()
      {
         var packet = PacketParser.DecodePacket("1::/ns");
         Assert.AreEqual("/ns", packet.EndPoint);
      }

      [TestMethod]
      public void DecodeConnectToEndpointWithQueryString()
      {
         var packet = PacketParser.DecodePacket("1::/ns:?test=1");
         Assert.AreEqual("/ns", packet.EndPoint);
         Assert.AreEqual("?test=1", packet.QueryString);
      }

      [TestMethod]
      public void DecodeEventWithArgs()
      {
         var packet = PacketParser.DecodePacket("5:::{\"name\":\"data\",\"args\":[\"Arg1Value\",\"Arg2Value\"]}");
         Assert.AreEqual(PacketType.Event, packet.Type);
         Assert.AreEqual("[\"Arg1Value\",\"Arg2Value\"]", packet.Args);
         Assert.AreEqual("data", packet.Name);
      }

      [TestMethod]
      public void DecodeEventWithIdAndAck()
      {
         var packet = PacketParser.DecodePacket("5:1+::{\"name\":\"eventName\"}");
         Assert.AreEqual(PacketType.Event, packet.Type);
         Assert.AreEqual("eventName", packet.Name);
         Assert.AreEqual("data", packet.Ack);
         Assert.AreEqual("1", packet.Id);
      }

      [TestMethod]
      public void DecodeEventWithNoArgs()
      {
         var packet = PacketParser.DecodePacket("5:::{\"name\":\"data\"}");
         Assert.AreEqual(PacketType.Event, packet.Type);
         Assert.AreEqual("[]", packet.Args);
         Assert.AreEqual("data", packet.Name);
      }

      [TestMethod]
      public void DecodePacketWithEndpoint()
      {
         var packet = PacketParser.DecodePacket("5::ns:{\"name\":\"data\",\"args\":[]}");
         Assert.AreEqual("ns", packet.EndPoint);
      }

      [TestMethod]
      public void DecodeError()
      {
         var packet = PacketParser.DecodePacket("7:::");
         Assert.AreEqual(PacketType.Error, packet.Type);
         Assert.IsNull(packet.Reason);
         Assert.IsNull(packet.Advice);
      }

      [TestMethod]
      public void DecodeErrorWithReason()
      {
         var packet = PacketParser.DecodePacket("7:::0");
         Assert.AreEqual(PacketType.Error, packet.Type);
         Assert.AreEqual("transport not supported", packet.Reason);
      }

      [TestMethod]
      public void DecodeErrorWithReasonAndAdvice()
      {
         var packet = PacketParser.DecodePacket("7:::2+0");
         Assert.AreEqual(PacketType.Error, packet.Type);
         Assert.AreEqual("unauthorized", packet.Reason);
         Assert.AreEqual("reconnect", packet.Advice);
      }
      
      [TestMethod]
      public void DecodeErrorWithEndpoint()
      {
         var packet = PacketParser.DecodePacket("7::/woot");
         Assert.AreEqual(PacketType.Error, packet.Type);
         Assert.AreEqual("/woot", packet.EndPoint);
      }

      [TestMethod]
      public void DecodeAck()
      {
         var packet = PacketParser.DecodePacket("6:::140");
         Assert.AreEqual(PacketType.Ack, packet.Type);
         Assert.AreEqual("140", packet.AckId);
         Assert.AreEqual("[]", packet.Args);
      }

      [TestMethod]
      public void DecodeJson()
      {
         var packet = PacketParser.DecodePacket("4:::\"jsonstring\"");
         Assert.AreEqual(PacketType.Json, packet.Type);
         Assert.AreEqual("\"jsonstring\"", packet.Data);
      }

      [TestMethod]
      public void DecodeJsonWithMessageIdAndAckData()
      {
         var packet = PacketParser.DecodePacket("4:1+::{\"a\":\"b\"}");
         Assert.AreEqual(PacketType.Json, packet.Type);
         Assert.AreEqual("1", packet.Id);
         Assert.AreEqual("data", packet.Ack);
         Assert.AreEqual("{\"a\":\"b\"}", packet.Data);
      }

      [TestMethod]
      public void DecodeMessage()
      {
         var packet = PacketParser.DecodePacket("3:::message");
         Assert.AreEqual(PacketType.Message, packet.Type);
         Assert.AreEqual("message", packet.Data);
      }

      [TestMethod]
      public void DecodeMessageWithIdAndEndpoint()
      {
          var packet = PacketParser.DecodePacket("3:5:/ns");
         Assert.AreEqual(PacketType.Message, packet.Type);
         Assert.AreEqual("5", packet.Id);
         Assert.AreEqual("/ns", packet.EndPoint);
      }

      [TestMethod]
      public void DecodeDisconnect()
      {
          var packet = PacketParser.DecodePacket("0::/ns");
         Assert.AreEqual(PacketType.Disconnect, packet.Type);
         Assert.AreEqual("/ns", packet.EndPoint);
      }

   }
}
