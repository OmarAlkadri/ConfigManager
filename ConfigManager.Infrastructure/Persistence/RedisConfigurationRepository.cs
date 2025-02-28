using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.Repositories;
using ConfigManager.Domain.ValueObjects;
using StackExchange.Redis;

namespace ConfigManager.Infrastructure.Persistence
{
    public class RedisConfigurationRepository : IConfigurationRepository
    {
        private readonly IDatabase _database;
        private readonly string _prefix;

        public RedisConfigurationRepository(string applicationName)
        {
            _database = RedisConnectionManager.Connection.GetDatabase();
            _prefix = $"config:{applicationName}:";
        }

        public async Task SeedDataAsync()
        {
            var existingKeys = (await GetAllAsync("SERVICE-A")).Any() || (await GetAllAsync("SERVICE-B")).Any();
            if (existingKeys) return;

            var seedData = new List<ConfigurationSetting>
            {
                new("SiteName", SettingType.String, "soty.io", true, "SERVICE-A"),
                new("IsBasketEnabled", SettingType.Boolean, "true", true, "SERVICE-B"),
                new("MaxItemCount", SettingType.Integer, "50", false, "SERVICE-A"),
                new("DefaultCurrency", SettingType.String, "USD", true, "SERVICE-A"),
                new("ApiEndpoint", SettingType.String, "https://api.soty.io", true, "SERVICE-B"),
                new("EnableLogging", SettingType.Boolean, "false", false, "SERVICE-A"),
                new("MaxUsers", SettingType.Integer, "1000", true, "SERVICE-B"),
                new("CacheExpiration", SettingType.Integer, "300", true, "SERVICE-A"),
                new("EnableFeatureX", SettingType.Boolean, "true", true, "SERVICE-B"),
                new("RetryAttempts", SettingType.Integer, "3", true, "SERVICE-A"),
            };

            foreach (var setting in seedData)
            {
                var key = $"{_prefix}{setting.Name}";
                var json = JsonSerializer.Serialize(setting);
                await _database.StringSetAsync(key, json);
            }
        }

        public async Task<ConfigurationSetting?> GetByNameAsync(string name, string applicationName)
        {
            var key = $"{_prefix}{name}";
            var json = await _database.StringGetAsync(key);
            return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ConfigurationSetting>(json!);
        }

        public async Task<IEnumerable<ConfigurationSetting>> GetAllAsync(string applicationName)
        {
            var server = RedisConnectionManager.Connection.GetServer(RedisConnectionManager.Connection.Configuration);
            var keys = server.Keys(pattern: $"{_prefix}*").ToArray();
            var tasks = keys.Select(key => _database.StringGetAsync(key));
            var results = await Task.WhenAll(tasks);

            return results.Where(r => !r.IsNullOrEmpty).Select(r => JsonSerializer.Deserialize<ConfigurationSetting>(r!)!);
        }

        public async Task AddOrUpdateAsync(ConfigurationSetting setting)
        {
            var key = $"{_prefix}{setting.Name}";
            var json = JsonSerializer.Serialize(setting);
            await _database.StringSetAsync(key, json);
        }

        public async Task DeleteAsync(Guid id)
        {
            var keys = (await GetAllAsync("")).Where(s => s.Id == id).Select(s => $"{_prefix}{s.Name}");
            foreach (var key in keys)
                await _database.KeyDeleteAsync(key);
        }
    }
}
