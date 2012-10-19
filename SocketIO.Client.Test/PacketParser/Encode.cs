using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketIO.Client.Impl;

namespace SocketIO.Client
{
    [TestClass]
    public class PacketParserEncode
    {
        [TestMethod]
        public void EncodeHeartBeat()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Heartbeat                                           
            });

            Assert.AreEqual("2::", encoded);
        }

        [TestMethod]
        public void EncodeConnect()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Connect
            });

            Assert.AreEqual("1::", encoded);
        }

        [TestMethod]
        public void EncodeConnectToEndpoint()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Connect,
                EndPoint = "/ns"
            });

            Assert.AreEqual("1::/ns", encoded);
        }

        [TestMethod]
        public void EncodeConnectToEndpointWithQueryString()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Connect,
                EndPoint = "/ns",
                QueryString = "?test=1"
            });

            Assert.AreEqual("1::/ns:?test=1", encoded);
        }

        [TestMethod]
        public void EncodeAckPacket()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Ack,
                AckId = "140"
            });

            Assert.AreEqual("6:::140", encoded);
        }

        [TestMethod]
        public void EncodeJson()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Json,
                Data = "\"12\"",
            });

            Assert.AreEqual("4:::\"12\"", encoded);
        }

        [TestMethod]
        public void EncodeJsonWithMessageIdAndData()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Json,
                Id = "1",
                Ack = "data",
                Data = "{\"a\":\"b\"}"
            });

            Assert.AreEqual("4:1+::{\"a\":\"b\"}", encoded);
        }

        [TestMethod]
        public void EncodeAckPacketWithArgs()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Ack,
                AckId = "12",
                Args = "[\"woot\",\"wa\"]"
            });

            Assert.AreEqual("6:::12+[\"woot\",\"wa\"]", encoded);
        }

        [TestMethod]
        public void EncodeEvent()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Event,
                Name = "woot"
            });

            Assert.AreEqual("5:::{\"name\":\"woot\"}", encoded);
        }

        [TestMethod]
        public void EncodeEventWithArgs()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Event,
                Name = "woot",
                Data = "[\"a\"]"
            });

            Assert.AreEqual("5:::{\"name\":\"woot\",\"args\":[\"a\"]}", encoded);
        }

        [TestMethod]
        public void EncodeEventWithMessageIdAndAck()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Event,
                Name = "woot",
                Id = "1",
                Ack = "data"
            });

            Assert.AreEqual("5:1+::{\"name\":\"woot\"}", encoded);
        }

        [TestMethod]
        public void EncodeMessage()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Message,
                Data = "woot"
            });

            Assert.AreEqual("3:::woot", encoded);
        }

        [TestMethod]
        public void EncodeMessageWithIdAndEndpoint()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Message,
                Id = "5",
                Ack = "true",
                EndPoint = "/ns"
            });

            Assert.AreEqual("3:5:/ns", encoded);
        }

        [TestMethod]
        public void EncodeDisconnect()
        {
            var encoded = PacketParser.EncodePacket(new Packet
            {
                Type = PacketType.Disconnect,
                EndPoint = "/ns"
            });

            Assert.AreEqual("0::/ns", encoded);
        }
    }
}

