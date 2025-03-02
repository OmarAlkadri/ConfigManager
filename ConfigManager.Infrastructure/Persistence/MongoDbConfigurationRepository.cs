using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Domain.ValueObjects;
using MongoDB.Driver;
using MongoDB.Bson;
using ConfigManager.Domain.DTOs;
using Bogus;

namespace ConfigManager.Infrastructure.Persistence
{
    public class MongoDbConfigurationRepository : IConfigurationRepository
    {
        private readonly IMongoCollection<ConfigurationSetting> _configCollection;
        private readonly IMessageBroker _messageBroker;

        public MongoDbConfigurationRepository(IMongoDatabase database, string collectionName, IMessageBroker messageBroker)
        {
            _configCollection = database.GetCollection<ConfigurationSetting>(collectionName);
            _messageBroker = messageBroker;
        }

        public async Task<ConfigurationSetting?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }

            var filter = Builders<ConfigurationSetting>.Filter.Eq(c => c._id, objectId.ToString());
            return await _configCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PagedResult<ConfigurationSetting>> GetAllByApplicationNamePage(string applicationName, int page = 1, int size = 10, string? search = null)
        {
            var filter = Builders<ConfigurationSetting>.Filter.And(
                Builders<ConfigurationSetting>.Filter.Eq(c => c.ApplicationName, applicationName),
                Builders<ConfigurationSetting>.Filter.Eq(c => c.IsActive, true)
            );
        
            if (!string.IsNullOrEmpty(search))
            {
                var searchFilter = Builders<ConfigurationSetting>.Filter.Regex(c => c.Name, new BsonRegularExpression(search, "i"));
                filter = Builders<ConfigurationSetting>.Filter.And(filter, searchFilter);
            }
        
            var totalRecords = await _configCollection.CountDocumentsAsync(filter);
            var items = await _configCollection.Find(filter)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();
        
            return new PagedResult<ConfigurationSetting>
            {
                Items = items,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)size),
                TotalItems = (int)totalRecords
            };
        }


        public async Task<IEnumerable<ConfigurationSetting>> GetAll()
        {
            return await _configCollection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<ConfigurationSetting>> GetAllAsync(string applicationName)
        {
            return await _configCollection.Find(c => c.ApplicationName == applicationName && c.IsActive).ToListAsync();
        }

        public async Task Add(ConfigurationSetting setting)
        {
            await _configCollection.InsertOneAsync(setting);
            await _messageBroker.PublishConfigurationUpdateAsync(setting.Name);
        }

        public async Task UpdateAsync(string id, ConfigurationUpdateDto updatedSetting)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException($"Invalid ObjectId format: {id}");
            }

            var filter = Builders<ConfigurationSetting>.Filter.Eq(c => c._id, objectId.ToString());

            var update = Builders<ConfigurationSetting>.Update
                .Set(c => c.Name, updatedSetting.Name)
                .Set(c => c.Type, updatedSetting.Type)
                .Set(c => c.Value, updatedSetting.Value)
                .Set(c => c.ApplicationName, updatedSetting.ApplicationName);

            var result = await _configCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Configuration with ID '{id}' not found.");
            }

            await _messageBroker.PublishConfigurationUpdateAsync(updatedSetting.Name);
        }




        public async Task DeleteAsync(string id)
        {
            var objectId = new ObjectId(id.ToString()).ToString();

            var filter = Builders<ConfigurationSetting>.Filter.Eq(c => c._id, objectId);
            await _configCollection.DeleteOneAsync(filter);
            await _messageBroker.PublishConfigurationUpdateAsync(id.ToString());
        }


        public async Task SeedDataAsync(string? applicationName)
        {
            applicationName ??= $"App-{Guid.NewGuid()}";

            var faker = new Faker<ConfigurationSetting>()
                .CustomInstantiator(f => new ConfigurationSetting(
                    f.Lorem.Word(),
                    f.PickRandom<SettingType>(),
                    "",
                    f.Random.Bool(),
                    applicationName,
                    null
                ))
                .RuleFor(c => c.Value, (f, c) => c.Type switch
                {
                    SettingType.String => f.Internet.DomainName(),
                    SettingType.Boolean => f.Random.Bool().ToString().ToLower(),
                    SettingType.Integer => f.Random.Int(1, 1000).ToString(),
                    SettingType.Double => f.Random.Double(1, 1000).ToString(),
                    _ => f.Lorem.Word()
                });

            var sampleData = new List<ConfigurationSetting>
            {
                new ConfigurationSetting("SiteName", SettingType.String, "example.com", true, applicationName, null),
                new ConfigurationSetting("IsFeatureXEnabled", SettingType.Boolean, "true", true, applicationName, null),
                new ConfigurationSetting("MaxUsers", SettingType.Integer, "100", true, applicationName, null)
            };

            sampleData.AddRange(faker.Generate(20)); 

            await _configCollection.InsertManyAsync(sampleData);
        }





        public async Task<IEnumerable<string>> GetAllApplicationNamesAsync()
        {
            return await _configCollection.Distinct(c => c.ApplicationName, FilterDefinition<ConfigurationSetting>.Empty).ToListAsync();
        }
    }
}
