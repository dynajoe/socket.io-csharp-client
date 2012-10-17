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
      private static readonly Regex PacketRegex = new Regex(@"([^:]+):([0-9]+)?(\+)?:([^:]+)?:?([\s\S]*)?");

      public static Packet DecodePacket(string packet)
      {
         var matches = PacketRegex.Matches(packet);

         return new Packet
         {
            Id = matches[1].Value,
            Type = (PacketType) int.Parse(matches[0].Value),
            Data = matches[4].Value,
            EndPoint = matches[3].Value,
            Ack = matches[2].Value
         };
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