using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(SignalData signal);
}

