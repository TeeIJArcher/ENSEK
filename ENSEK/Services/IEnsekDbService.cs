using Models;

namespace Services;

public interface IEnsekDbService
{
    public Task<List<AccountDoc>> GetAccountsAsync();

    public Task<List<AccountDoc>> GetAccountsWithIdsAsync(List<long> ids);

    public Task<AccountDoc?> GetAccountAsync(long id);

    public Task CreateAccountAsync(AccountDoc newAccount);

    public Task UpdateAccountAsync(long id, AccountDoc updatedAccount);

    public Task RemoveAccountAsync(long id);



    public Task<List<ReadingDoc>> GetReadingsAsync();

    public Task<ReadingDoc?> GetReadingAsync(string id);

    public Task<List<ReadingDoc>> GetReadingForAccountAsync(long id);

    public Task<ReadingDoc?> GetLastReadingForAccountAsync(long id);

    public Task CreateReadingAsync(ReadingDoc newReading);

    public Task UpdateReadingAsync(string id, ReadingDoc updatedReading);

    public Task RemoveReadingAsync(string id);
}