using Models;
using Microsoft.Extensions.Options;
using Services;

namespace Mocks;

public class MockEnsekDbService : IEnsekDbService
{
    public List<AccountDoc> AccountCache { get; set; } = new List<AccountDoc>();
    public List<ReadingDoc> ReadingCache { get; set; } = new List<ReadingDoc>();


    public Task<List<AccountDoc>> GetAccountsAsync() => 
        Task.FromResult(AccountCache);

    public Task<List<AccountDoc>> GetAccountsWithIdsAsync(List<long> ids) => 
        Task.FromResult(AccountCache.Where(x => ids.Contains(x.AccountId)).ToList());

    public Task<AccountDoc?> GetAccountAsync(long id) => 
        Task.FromResult(AccountCache.FirstOrDefault(x => x.AccountId == id));

    public Task CreateAccountAsync(AccountDoc newAccount) {
        AccountCache.Add(newAccount);
        return Task.CompletedTask;
    }

    public async Task UpdateAccountAsync(long id, AccountDoc updatedAccount){
        await RemoveAccountAsync(id);
        await CreateAccountAsync(updatedAccount);
    }

    public Task RemoveAccountAsync(long id) {
        var account = AccountCache.FirstOrDefault(x => x.AccountId == id);
        if (account != null) {
            AccountCache.Remove(account);
        }
        return Task.CompletedTask;
    }



    public Task<List<ReadingDoc>> GetReadingsAsync()  => 
        Task.FromResult(ReadingCache);

    public Task<ReadingDoc?> GetReadingAsync(string id) => 
        Task.FromResult(ReadingCache.FirstOrDefault(x => x.ReadingId == id));

    public Task<List<ReadingDoc>> GetReadingForAccountAsync(long id) => 
        Task.FromResult(ReadingCache.Where(x => x.AccountId == id).ToList());

    public Task<ReadingDoc?> GetLastReadingForAccountAsync(long id) => 
        Task.FromResult(ReadingCache.Where(x => x.AccountId == id)
                .OrderBy(x => x.ReadingDateTime).LastOrDefault());

    public Task CreateReadingAsync(ReadingDoc newReading) {
        ReadingCache.Add(newReading);
        return Task.CompletedTask;
    }

    public async Task UpdateReadingAsync(string id, ReadingDoc updatedReading) {
        await RemoveReadingAsync(id);
        await CreateReadingAsync(updatedReading);
    }

    public Task RemoveReadingAsync(string id) {
        var reading = ReadingCache.FirstOrDefault(x => x.ReadingId == id);
        if (reading != null) {
            ReadingCache.Remove(reading);
        }
        return Task.CompletedTask;
    }
}