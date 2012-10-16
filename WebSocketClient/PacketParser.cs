using System.Text.RegularExpressions;

namespace WebSocketClient
{
   public class PacketParser
   {
      private readonly Regex PacketRegex = new Regex(@"([^:]+):([0-9]+)?(\+)?:([^:]+)?:?([\s\S]*)?");

      public object DecodePacket(string packet)
      {
         var pieces = PacketRegex.Matches(packet);


         return null;
      }
   }
}