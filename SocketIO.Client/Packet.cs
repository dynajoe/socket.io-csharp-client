namespace SocketIO.Client
{
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
}