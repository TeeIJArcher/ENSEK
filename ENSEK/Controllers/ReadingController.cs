using Models;
using Services;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingController : ControllerBase
{
    private readonly IEnsekDbService _ensekDbService;

    public ReadingController(IEnsekDbService ensekDbService) {
        _ensekDbService = ensekDbService;
    }

    /// <summary>
    /// This will return a list of Readings
    /// </summary>
    /// <returns>List of Readings within system</returns>
    /// <response code="200">Requst Successful</response>
    [HttpGet]
    public async Task<ActionResult> Get() {
        var readings = await _ensekDbService.GetReadingsAsync();
        return Ok(readings.Select(x => new {
            readingId = x.ReadingId,
            accountId = x.AccountId,
            meterReadingDateTime = x.MeterReadingDateTime,
            meterReadValue = x.MeterReadValue
        }));
    }
}