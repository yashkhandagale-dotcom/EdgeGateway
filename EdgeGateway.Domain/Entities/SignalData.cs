namespace EdgeGateway.Domain.Entities;

public class SignalData
{
    public string SignalId { get; set; } = default!;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}
