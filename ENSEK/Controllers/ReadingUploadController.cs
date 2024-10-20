using Models;
using Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;

namespace Controllers;

[ApiController]
[Route("meter-reading-uploads")]
public class ReadingUploadController : ControllerBase
{
    private readonly IEnsekDbService _ensekDbService;
    private readonly ICSVService _csvService;

    public ReadingUploadController(IEnsekDbService ensekDbService, ICSVService csvService) {
        _csvService = csvService;
        _ensekDbService = ensekDbService;
    }

    /// <summary>
    /// Takes a CSV file of readings for multiple accounts and processes them
    /// </summary>
    /// <param name="file">CSV file of readings to import</param>
    /// <returns>Summary of the processed Readings, including valid, invalid and unknown readings</returns>
    /// <response code="200">Requst Successful</response>
    [HttpPost()]
    public async Task<ActionResult> Post([FromForm] IFormFileCollection file) {
        var readings = _csvService.ReadCSV<ReadingDoc>(file[0].OpenReadStream()).ToList();

        // Group Readings by Account as this how we validate Reading on a account by account bases
        var readingsGroupedByAccount = readings.GroupBy(x => x.AccountId).ToList();

        // Grab all account Ids in CSV, then grab all know accounts with these Ids
        var accountIds = readingsGroupedByAccount.Select(x => x.Key).Distinct().ToList();
        var accounts = await _ensekDbService.GetAccountsWithIdsAsync(accountIds);

        // Setup lists to store valid and invalid readings for summarising at the end
        var unknownAccountReadings = new List<Reading>();
        var invalidValueReadings = new List<Reading>();
        var validValueReadings = new List<Reading>();
        foreach (var readingGroup in readingsGroupedByAccount)
        {
            // Grab the account for this group of recordings
            var account = accounts.FirstOrDefault(x => x.AccountId == readingGroup.Key);
            if (account == null) {
                // If null record this is a unknown account and move on to the next group
                unknownAccountReadings.InsertRange(0, readingGroup.Select(x => new Reading {
                            ReadingId = x.ReadingId,
                            AccountId = x.AccountId,
                            MeterReadingDateTime = x.MeterReadingDateTime,
                            MeterReadValue = x.MeterReadValue
                        }).ToList());
            } else {
                // Grab the last reading, I'm making the assumption you can't submit reading older then the latest record reading
                var lastRecoredReading = await _ensekDbService.GetLastReadingForAccountAsync(readingGroup.Key);
                // Value to store the latest reading, this will allow us to make sure the reading are increasing not decreasing
                long lastReading = lastRecoredReading?.MeterReadValue ?? 0;
                // Order Readings so they can be recored and handled in Date order
                var orderedReading = readingGroup.ToList().OrderBy(x => x.ReadingDateTime).ToList();
                foreach (var reading in readingGroup.ToList().OrderBy(x => x.ReadingDateTime).ToList()) {
                    // Ignore last recored reading is its null, and make sure the meter value is increasing and that is a valid NNNNN
                    if ((lastRecoredReading == null || 
                        reading.ReadingDateTime > lastRecoredReading.ReadingDateTime) &&
                        reading.MeterReadValue >= lastReading && reading.MeterReadValue < 99999) {

                        // Store in MongoDB as is a valid Reading, add to list for summary
                        await _ensekDbService.CreateReadingAsync(reading);
                        validValueReadings.Add(new Reading {
                            ReadingId = reading.ReadingId,
                            AccountId = reading.AccountId,
                            MeterReadingDateTime = reading.MeterReadingDateTime,
                            MeterReadValue = reading.MeterReadValue
                        });
                        lastReading = reading.MeterReadValue ?? 0; // Update the last meter value for next comparison
                    } else {
                        invalidValueReadings.Add(new Reading {
                            ReadingId = reading.ReadingId,
                            AccountId = reading.AccountId,
                            MeterReadingDateTime = reading.MeterReadingDateTime,
                            MeterReadValue = reading.MeterReadValue
                        }); // Record this meter value / reasding is invalid
                    }
                }
            }
        }

        // Build summary object to return to uploader for review
        return Ok(new Summary {
            totalReadingsUploaded = readings.Count,
            totalUniqueAccounts = accountIds.Count,
            totalValidAccounts = accounts.Count,

            totalValidReadings = validValueReadings.Count,
            totalUnknownAccountReadings = unknownAccountReadings.Count,
            totalInvalidReadings = invalidValueReadings.Count,

            validValueReadings = validValueReadings,
            invalidValueReadings = invalidValueReadings,
            unknownAccountReadings = unknownAccountReadings
        });
    }
}