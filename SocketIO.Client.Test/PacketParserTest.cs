using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SocketIO.Client.Test
{
   [TestClass]
   public class PacketParserTest
   {
      [TestMethod]
      public void DecodeHeartBeat()
      {
         var packet = PacketParser.DecodePacket("2::");
         Assert.AreEqual(PacketType.Heartbeat, packet.Type);
      }

      [TestMethod]
      public void DecodeConnect()
      {
         var packet = PacketParser.DecodePacket("1::");
         Assert.AreEqual(PacketType.Connect, packet.Type);
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
   }
}
