using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Domain.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ConfigManager.Application.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configRepository;
        private readonly IMessageBroker _messageBroker;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public ConfigurationService(
            IConfigurationRepository configRepository,
            IMessageBroker messageBroker,
            IMemoryCache cache,
            ILogger<ConfigurationService> logger)
        {
            _configRepository = configRepository;
            _messageBroker = messageBroker;
            _cache = cache;
            _logger = logger;
        }

        public async Task SeedDataAsync(string? applicationName)
        {
            await _configRepository.SeedDataAsync(applicationName);
        }

        public async Task<ConfigurationSetting?> GetConfigurationByIdAsync(string id)
        {
            try
            {
                return await _configRepository.GetByIdAsync(id) ?? throw new Exception("Configuration not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching configuration by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ConfigurationSetting>> GetAll()
        {
            return await _configRepository.GetAll();
        }

        public async Task<IEnumerable<ConfigurationSetting>> GetAllConfigurationsAsync(string applicationName, string? search = null)
        {
            if (_cache.TryGetValue(applicationName, out IEnumerable<ConfigurationSetting> cachedConfigs))
            {
                return cachedConfigs;
            }

            var configurations = await _configRepository.GetAllAsync(applicationName);
            configurations = configurations.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                configurations = configurations.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            _cache.Set(applicationName, configurations, _cacheDuration);
            return configurations;
        }

        public async Task<PagedResult<ConfigurationSetting>> GetAllConfigurationsAsyncPage(string applicationName, int page = 1, int size = 10, string? search = null)
        {
            return await _configRepository.GetAllByApplicationNamePage(applicationName, page, size, search);
        }

        public async Task Add(ConfigurationSetting setting)
        {
            try
            {
                await _configRepository.Add(setting);
                _cache.Remove(setting.ApplicationName);
                await _messageBroker.PublishConfigurationUpdateAsync($"Added: {setting.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding configuration: {SettingName}", setting.Name);
            }
        }

        public async Task DeleteConfigurationAsync(string id)
        {
            try
            {
                var config = await _configRepository.GetByIdAsync(id);
                if (config == null) return;
                
                await _configRepository.DeleteAsync(id);
                _cache.Remove(config.ApplicationName);
                await _messageBroker.PublishConfigurationUpdateAsync($"Deleted: {config.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration: {Id}", id);
            }
        }

        public async Task UpdateConfigurationAsync(string id, ConfigurationUpdateDto updatedSetting)
        {
            try
            {
                await _configRepository.UpdateAsync(id, updatedSetting);
                _cache.Remove(updatedSetting.ApplicationName);
                await _messageBroker.PublishConfigurationUpdateAsync($"Updated: {updatedSetting.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration: {Id}", id);
            }
        }

        public async Task<IEnumerable<string>> GetAllApplicationNamesAsync()
        {
            return await _configRepository.GetAllApplicationNamesAsync();
        }
    }
}
