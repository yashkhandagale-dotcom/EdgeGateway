//using EdgeGateway.Domain.Entities;
//public interface IInfluxSignalReader
//{
//    Task<IEnumerable<SignalData>> ReadAsync(DateTime from, DateTime to);
//}

using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Application.Interfaces;

public interface IInfluxSignalReader
{
    Task<IEnumerable<SignalData>> ReadAsync(DateTime from, DateTime to);
}

