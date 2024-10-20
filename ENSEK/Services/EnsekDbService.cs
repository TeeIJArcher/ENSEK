using Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace Services;

public class EnsekDbService : IEnsekDbService
{
    private readonly IOptions<EnsekDatabaseSettings> _ensekDatabaseSettings;
    private readonly IMongoCollection<AccountDoc> _accountsCollection;
    private readonly IMongoCollection<ReadingDoc> _readingsCollection;

    public EnsekDbService(
        IOptions<EnsekDatabaseSettings> ensekDatabaseSettings)
    {
        _ensekDatabaseSettings = ensekDatabaseSettings;

        var mongoClient = new MongoClient(
            ensekDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            ensekDatabaseSettings.Value.DatabaseName);

        BsonClassMap.RegisterClassMap<AccountDoc>(cm =>
        {
            cm.MapIdField(x => x.AccountId);
            cm.MapMember(x => x.FirstName);
            cm.MapMember(x => x.LastName);
        });
        BsonClassMap.RegisterClassMap<ReadingDoc>(cm =>
        {
            cm.MapIdField(x => x.ReadingId);
            cm.MapMember(x => x.AccountId);
            cm.MapMember(x => x.MeterReadingDateTime);
            cm.MapMember(x => x.MeterReadValue);
        });

        _accountsCollection = mongoDatabase.GetCollection<AccountDoc>(
            ensekDatabaseSettings.Value.CollectionNames[0]);

        _readingsCollection = mongoDatabase.GetCollection<ReadingDoc>(
            ensekDatabaseSettings.Value.CollectionNames[1]);
    }

    public async Task<List<AccountDoc>> GetAccountsAsync() =>
        await _accountsCollection.Find(_ => true).ToListAsync();

    public async Task<List<AccountDoc>> GetAccountsWithIdsAsync(List<long> ids) =>
        await _accountsCollection.Find(x => ids.Contains(x.AccountId)).ToListAsync();

    public async Task<AccountDoc?> GetAccountAsync(long id) =>
        await _accountsCollection.Find(x => x.AccountId == id).FirstOrDefaultAsync();

    public async Task CreateAccountAsync(AccountDoc newAccount) =>
        await _accountsCollection.InsertOneAsync(newAccount);

    public async Task UpdateAccountAsync(long id, AccountDoc updatedAccount) =>
        await _accountsCollection.ReplaceOneAsync(x => x.AccountId == id, updatedAccount);

    public async Task RemoveAccountAsync(long id) =>
        await _accountsCollection.DeleteOneAsync(x => x.AccountId == id);



    public async Task<List<ReadingDoc>> GetReadingsAsync() =>
        await _readingsCollection.Find(_ => true).ToListAsync();

    public async Task<ReadingDoc?> GetReadingAsync(string id) =>
        await _readingsCollection.Find(x => x.ReadingId == id).FirstOrDefaultAsync();

    public async Task<List<ReadingDoc>> GetReadingForAccountAsync(long id) =>
        (await _readingsCollection.Find(x => x.AccountId == id).ToListAsync())
                .OrderBy(x => x.ReadingDateTime).ToList();

    public async Task<ReadingDoc?> GetLastReadingForAccountAsync(long id) =>
        (await _readingsCollection.Find(x => x.AccountId == id).ToListAsync())
                .OrderBy(x => x.ReadingDateTime).LastOrDefault();

    public async Task CreateReadingAsync(ReadingDoc newReading) =>
        await _readingsCollection.InsertOneAsync(newReading);

    public async Task UpdateReadingAsync(string id, ReadingDoc updatedReading) =>
        await _readingsCollection.ReplaceOneAsync(x => x.ReadingId == id, updatedReading);

    public async Task RemoveReadingAsync(string id) =>
        await _readingsCollection.DeleteOneAsync(x => x.ReadingId == id);
}
