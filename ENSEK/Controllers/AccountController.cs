using Models;
using Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;

namespace Controllers;

[ApiController]
[Route("api/Accounts")]
public class AccountController : ControllerBase
{
    private readonly IEnsekDbService _ensekDbService;
   private readonly ICSVService _csvService;

    public AccountController(IEnsekDbService ensekDbService, ICSVService csvService) {
        _csvService = csvService;
        _ensekDbService = ensekDbService;
    }

    /// <summary>
    /// This will return a list of Accounts
    /// </summary>
    /// <returns>List of Accounts within system</returns>
    /// <response code="200">Requst Successful</response>
    [HttpGet]
    public async Task<ActionResult<List<Account>>> Get() {
        return Ok((await _ensekDbService.GetAccountsAsync()).Select(x => new {
            AccountId = x.AccountId,
            FirstName = x.FirstName,
            LastName = x.LastName
        }));
    }

    /// <summary>
    /// Takes a CSV file of accounts to add into the system
    /// </summary>
    /// <param name="file">CSV file of accounts to import</param>
    /// <returns>List of Accounts within system</returns>
    /// <response code="200">Requst Successful</response>
    [HttpPost]
    public async Task<ActionResult<Account>> Post([FromForm] IFormFileCollection file) {
        var accounts = _csvService.ReadCSV<AccountDoc>(file[0].OpenReadStream());

        foreach (var account in accounts)
        {
            await _ensekDbService.CreateAccountAsync(account);
        }

        return Ok((await _ensekDbService.GetAccountsAsync()).Select(x => new {
            AccountId = x.AccountId,
            FirstName = x.FirstName,
            LastName = x.LastName
        }));
    }

    /// <summary>
    /// A Single Account with its related Readings
    /// </summary>
    /// <returns>An Account with its Readings</returns>
    /// <response code="404">Account not found</response>
    /// <response code="200">Requst Successful</response>
    [HttpGet("{acountId}")]
    public async Task<ActionResult<AccountExtended>> GetAccount(long acountId) {
        var account = await _ensekDbService.GetAccountAsync(acountId);
        if (account == null) {
            return NotFound();
        }

        return Ok(new {
            AccountId = account.AccountId,
            FirstName = account.FirstName,
            LastName = account.LastName,

            Readings = (await _ensekDbService.GetReadingForAccountAsync(account.AccountId)).Select(x => new {
                readingId = x.ReadingId,
                accountId = x.AccountId,
                meterReadingDateTime = x.MeterReadingDateTime,
                meterReadValue = x.MeterReadValue
            })
        });
    }

    /// <summary>
    /// Creates a new Single Account
    /// </summary>
    /// <returns>The account just created</returns>
    /// <response code="400">If the account object does not share the id from the url</response>
    /// <response code="500">Requst Unsuccessful, failed to create account</response>
    /// <response code="201">Requst Successful</response>
    [HttpPost("{acountId}")]
    public async Task<ActionResult<Account>> PostAccount(long acountId, Account account) {
        if (account.AccountId == acountId)
        {
            return BadRequest();
        }

        await _ensekDbService.CreateAccountAsync(new AccountDoc {
                            AccountId = account.AccountId,
                            FirstName = account.FirstName,
                            LastName = account.LastName
                        });
        var a = await _ensekDbService.GetAccountAsync(acountId);
        if (a == null) {
            return StatusCode(500);
        }
        return new ObjectResult(new {
            AccountId = a.AccountId,
            FirstName = a.FirstName,
            LastName = a.LastName
        }) { StatusCode = StatusCodes.Status201Created };
    }

    /// <summary>
    /// Updates a Single Account
    /// </summary>
    /// <returns>The account just updated</returns>
    /// <response code="404">If the account does not exist</response>
    /// <response code="200">Requst Successful</response>
    [HttpPut("{acountId}")]
    public async Task<ActionResult<Account>> PutAccount(long acountId, Account account) {
        var acc = await _ensekDbService.GetAccountAsync(acountId);
        if (acc == null) {
            return NotFound();
        }

        await _ensekDbService.UpdateAccountAsync(acountId, new AccountDoc {
                            AccountId = account.AccountId,
                            FirstName = account.FirstName,
                            LastName = account.LastName
                        });

        var a = await _ensekDbService.GetAccountAsync(acountId);
        return Ok(new {
            AccountId = a.AccountId,
            FirstName = a.FirstName,
            LastName = a.LastName
        });
    }

    /// <summary>
    /// Deletes a Single Account
    /// </summary>
    /// <response code="404">If the account does not exist</response>
    /// <response code="500">Requst Unsuccessful, failed to delete account</response>
    /// <response code="200">Requst Successful</response>
    [HttpDelete("{acountId}")]
    public async Task<ActionResult> DeleteAccount(long acountId) {
        var acc = await _ensekDbService.GetAccountAsync(acountId);
        if (acc == null) {
            return NotFound();
        }

        await _ensekDbService.RemoveAccountAsync(acountId);var a = await _ensekDbService.GetAccountAsync(acountId);
        if (a != null) {
            return StatusCode(500);
        }
        return Ok();
    }
}