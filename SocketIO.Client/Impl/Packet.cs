namespace SocketIO.Client.Impl
{
   internal class Packet
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

      public string QueryString { get; set; }

      protected bool Equals(Packet other)
      {
         return Type.Equals(other.Type) && string.Equals(Data, other.Data) 
            && string.Equals(Ack, other.Ack) && string.Equals(AckId, other.AckId) 
            && string.Equals(EndPoint, other.EndPoint) && string.Equals(Id, other.Id) 
            && string.Equals(Name, other.Name) && string.Equals(Args, other.Args) 
            && string.Equals(Advice, other.Advice) && string.Equals(Reason, other.Reason) 
            && string.Equals(QueryString, other.QueryString);
      }

      public override bool Equals(object other)
      {
         if (ReferenceEquals(null, other)) 
            return false;
         
         if (ReferenceEquals(this, other)) 
            return true;
         
         if (other.GetType() != GetType()) 
            return false;
         
         return Equals((Packet) other);
      }

      public override int GetHashCode()
      {
         unchecked
         {
            int hashCode = Type.GetHashCode();
            
            hashCode = (hashCode*397) ^ (Data != null ? Data.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Ack != null ? Ack.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (AckId != null ? AckId.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (EndPoint != null ? EndPoint.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Id != null ? Id.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Args != null ? Args.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Advice != null ? Advice.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (Reason != null ? Reason.GetHashCode() : 0);
            hashCode = (hashCode*397) ^ (QueryString != null ? QueryString.GetHashCode() : 0);

            return hashCode;
         }
      }
   }
}