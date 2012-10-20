using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SocketIO.Client.Impl
{
   internal static class PacketParser
   {
      private class Event
      {
         [JsonProperty(PropertyName = "name", Order = 0)]
         public string Name { get; set; }

         [JsonProperty(PropertyName = "args", Order = 1)]
         public object Args { get; set; }
      }

      private static readonly Dictionary<string, string> Reasons = new Dictionary<string, string>
      {
         {"0", "transport not supported"},
         {"1", "client not handshaken"},
         {"2", "unauthorized"},
      };

      private static readonly Dictionary<string, string> Advice = new Dictionary<string, string>
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
            case PacketType.Connect:
               packet.Data = null;
               packet.QueryString = data;
               break;
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
               packet.Args = packetEvent.Args != null ? ((JContainer)packetEvent.Args).ToString(Formatting.None, null) : "[]";
               break;
            case PacketType.Ack:
               var ackMatches = Regex.Match(data, @"^(?<AckId>[0-9]+)(\+)?(?<Args>.*)", RegexOptions.Compiled);
                
               if (ackMatches.Success)
               {
                  packet.AckId = ackMatches.Groups["AckId"].Value;
                  packet.Args = string.IsNullOrEmpty(ackMatches.Groups["Args"].Value) ? "[]" : ackMatches.Groups["Args"].Value;
               }

               break;
         }

         return packet;
      }

      public static string EncodePacket(Packet packet)
      {
         var parts = new List<string>();

         parts.Add(((int) packet.Type).ToString(CultureInfo.InvariantCulture));
         parts.Add((packet.Id ?? "") + (packet.Ack == "data" ? "+" : string.Empty));
         parts.Add(packet.EndPoint ?? "");
         
         string data = packet.Data;

         switch (packet.Type)
         {
             case PacketType.Connect:
                 data = packet.QueryString;
                 break;
             case PacketType.Event:
               data = JsonConvert.SerializeObject(new { name = packet.Name, args = string.IsNullOrEmpty(packet.Data) ? null : JArray.Parse(data) }, Formatting.None, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
               break;
            case PacketType.Ack:
               data = packet.AckId + (!string.IsNullOrEmpty(packet.Args) ? "+" + packet.Args : string.Empty) ;
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