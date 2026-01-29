using Microsoft.AspNetCore.Mvc;
using EdgeGateway.Application.Services;

namespace EdgeGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineSignalsController : ControllerBase
{
    private readonly SignalSyncService _syncService;

    public MachineSignalsController(SignalSyncService syncService)
    {
        _syncService = syncService;
    }

    // Trigger from Swagger
    // POST /api/machinesignals/sync?from=2026-01-22T00:00:00Z&to=2026-01-22T23:59:59Z
    //[HttpPost("sync")]
    //public async Task<IActionResult> Sync(
    //    [FromQuery] DateTime from,
    //    [FromQuery] DateTime to)
    //{
    //    await _syncService.SyncAsync(from, to);
    //    return Ok("Signals synced from InfluxDB to RabbitMQ");
    //}
    //[HttpPost("sync")]
    //public async Task<IActionResult> Sync(
    //[FromQuery] DateTime from,
    //[FromQuery] DateTime to)
    //{
    //    if (from == default || to == default)
    //        return BadRequest("from and to must be provided");

    //    if (from >= to)
    //        return BadRequest("from must be earlier than to");

    //    await _syncService.SyncAsync(from, to);
    //    return Ok("Signals synced successfully");
    //}

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(
    [FromQuery] DateTime from,
    [FromQuery] DateTime to)
    {
        if (from == default || to == default)
            return BadRequest("from and to must be provided");

        if (from >= to)
            return BadRequest("from must be earlier than to");

        await _syncService.SyncAsync(from, to);
        return Ok("Signals synced successfully");
    }

}
