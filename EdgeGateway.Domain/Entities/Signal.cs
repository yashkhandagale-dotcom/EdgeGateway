namespace EdgeGateway.Domain.Entities
{
    public class Signal   // <-- make it PUBLIC
    {
        public string DeviceId { get; set; }
        public string AssetId { get; set; }
        public string SignalTypeId { get; set; }
        public string MappingId { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
