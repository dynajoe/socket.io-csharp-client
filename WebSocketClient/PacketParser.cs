using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace WebSocketClient
{
   public enum PacketType
   {
      Disconnect,
      Connect,
      Heartbeat,
      Message,
      Json,
      Event,
      Ack,
      Error,
      NoOp
   }

   public class Packet
   {
      public PacketType Type { get; set; }

      public string Data { get; set; }

      public string Ack { get; set; }
      
      public string AckId { get; set; }

      public string EndPoint { get; set; }

      public string Id { get; set; }

      public string Name { get; set; }

      public string Args { get; set; }

      public string Advice { get; set; }

      public string Reason { get; set; }
   }

   public static class PacketParser
   {
      private class Event
      {
         public string Name { get; set; }
         public string Args { get; set; }
      }

      private static Dictionary<string, string> Reasons = new Dictionary<string, string>()
      {
         {"0", "transport not supported"},
         {"1", "client not handshaken"},
         {"2", "unauthorized"},
      };

      private static Dictionary<string, string> Advice = new Dictionary<string, string>()
      {
         {"0", "reconnect"},
      };

      private static readonly Regex PacketRegex = new Regex(@"([^:]+):([0-9]+)?(\+)?:([^:]+)?:?([\s\S]*)?");

      public static Packet DecodePacket(string packetData)
      {
         var matches = PacketRegex.Matches(packetData);
         var packet = new Packet();

         var id = matches[1].Value;
         var type = (PacketType) int.Parse(matches[0].Value);
         var data = matches[4].Value;
         var endPoint = matches[3].Value;
         var ack = matches[2].Value;
         
         if (!string.IsNullOrEmpty(id))
         {
            packet.Id = id;
            packet.Ack = !string.IsNullOrEmpty(ack) ? "data" : "true";
         }

         switch (type)
         {
            case PacketType.Error:
               var errorParts = data.Split(new[] {'+'});
               
               if (errorParts.Length >= 1)
               {
                  if (Reasons.ContainsKey(errorParts[0]))
                  {
                     packet.Reason = Reasons[errorParts[0]];
                  }
               }

               if (errorParts.Length >= 2)
               {
                  if (Advice.ContainsKey(errorParts[1]))
                  {
                     packet.Advice = Advice[errorParts[1]];
                  }
               }

               break;
            case PacketType.Event:
               var packetEvent = SimpleJson.SimpleJson.DeserializeObject<Event>(data);
               packet.Name = packetEvent.Name;
               packet.Args = packetEvent.Args;
               break;
            case PacketType.Ack:
               var ackMatches = Regex.Matches(data, @"^([0-9]+)(\+)?(.*)", RegexOptions.Compiled);
                
               if (ackMatches.Count > 0)
               {
                  packet.AckId = ackMatches[0].Value;
                  packet.Args = ackMatches[1].Value;
               }
               break;
         }

         return packet;
      }

      public static string EncodePacket(Packet packet)
      {
         var parts = new List<object>();

         parts.Add((int) packet.Type);
         parts.Add((packet.Id ?? "") + (packet.Ack == "data" ? "+" : string.Empty));
         parts.Add(packet.EndPoint ?? "");
         
         if (packet.Data != null)
         {
            parts.Add(packet.Data);
         }

         return string.Join(":", parts.ToArray());
      }
   }
}