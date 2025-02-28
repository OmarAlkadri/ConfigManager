using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Domain.ValueObjects;
using ConfigManager.Domain.Repositories;
using System.Collections.Concurrent;


public class ConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IMessageBroker _messageBroker;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentBag<string> _cacheKeys = new ConcurrentBag<string>();

    public ConfigurationService(IConfigurationRepository configurationRepository, IMessageBroker messageBroker, IMemoryCache cache)
    {
        _configurationRepository = configurationRepository ?? throw new ArgumentNullException(nameof(configurationRepository));
        _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<ConfigurationSetting?> GetConfigurationAsync(string name, string applicationName)
    {
        string cacheKey = $"config_{applicationName}_{name}";
        if (!_cache.TryGetValue(cacheKey, out ConfigurationSetting? setting))
        {
            setting = await _configurationRepository.GetByNameAsync(name, applicationName);
            if (setting != null)
            {
                _cache.Set(cacheKey, setting, _cacheDuration);
                _cacheKeys.Add(cacheKey);
            }
        }
        return setting;
    }

    public async Task<IEnumerable<ConfigurationSetting>> GetAllConfigurationsAsync(string applicationName)
    {
        string cacheKey = $"config_all_{applicationName}";
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<ConfigurationSetting>? settings))
        {
            settings = await _configurationRepository.GetAllAsync(applicationName);
            if (settings != null)
            {
                _cache.Set(cacheKey, settings, _cacheDuration);
                _cacheKeys.Add(cacheKey);
            }
        }
        return settings ?? new List<ConfigurationSetting>();
    }

    public async Task AddOrUpdateConfigurationAsync(string name, SettingType type, string value, bool isActive, string applicationName)
    {
        var setting = new ConfigurationSetting(name, type, value, isActive, applicationName);
        await _configurationRepository.AddOrUpdateAsync(setting);
        await _messageBroker.PublishConfigurationUpdateAsync(name);

        string cacheKey = $"config_{applicationName}_{name}";
        _cache.Set(cacheKey, setting, _cacheDuration);
        _cacheKeys.Add(cacheKey);

        _cache.Remove($"config_all_{applicationName}");
    }

    public async Task DeleteConfigurationAsync(Guid id)
    {
        await _configurationRepository.DeleteAsync(id);
        RemoveCacheEntriesStartingWith("config_"); 
    }

    private void RemoveCacheEntriesStartingWith(string prefix)
    {
        foreach (var key in _cacheKeys)
        {
            if (key.StartsWith(prefix))
            {
                _cache.Remove(key);
            }
        }
    }
}

public static class MemoryCacheExtensions
{
    public static void RemoveWhereKeyStartsWith(this IMemoryCache cache, IEnumerable<string> keys, string keyPrefix)
    {
        foreach (var key in keys)
        {
            if (key.StartsWith(keyPrefix))
            {
                cache.Remove(key);
            }
        }
    }
}
