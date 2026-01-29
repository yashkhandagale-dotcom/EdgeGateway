using EdgeGateway.Domain.Entities;

public interface IMessageBatchPublisher
{
    Task PublishAsync(IEnumerable<SignalData> signals);
}
