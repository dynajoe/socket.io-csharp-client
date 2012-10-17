using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
         [JsonProperty(PropertyName = "name", Order = 0)]
         public string Name { get; set; }

         [JsonProperty(PropertyName = "args", Order = 1)]
         public object Args { get; set; }
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

      private static readonly Regex PacketRegex = new Regex(@"(?<Type>[^:]+):(?<Id>[0-9]+)?(?<Ack>\+)?:(?<EndPoint>[^:]+)?:?(?<Data>[\s\S]*)?");

      public static Packet DecodePacket(string packetData)
      {
         var match = PacketRegex.Match(packetData);
         var packet = new Packet();

         var id = match.Groups["Id"].Value;
         var type = (PacketType)int.Parse(match.Groups["Type"].Value);
         var data = match.Groups["Data"].Value;
         var endPoint = match.Groups["EndPoint"].Value;
         var ack = match.Groups["Ack"].Value;
         
         if (!string.IsNullOrEmpty(id))
         {
            packet.Id = id;
            packet.Ack = !string.IsNullOrEmpty(ack) ? "data" : "true";
         }

         packet.Type = type;
         packet.EndPoint = endPoint;
         packet.Data = data;
         
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
               var packetEvent = JsonConvert.DeserializeObject<Event>(data);
               packet.Name = packetEvent.Name;
               packet.Args = ((JContainer)packetEvent.Args).ToString(Formatting.None, null);
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
         string data = packet.Data;

         switch (packet.Type)
         {
            case PacketType.Event:
               data = JsonConvert.SerializeObject(new Event { Name = packet.Name, Args = packet.Data });
               break;
         }

         if (data != null)
         {
            parts.Add(data);
         }

         return string.Join(":", parts.ToArray());
      }
   }
}