using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.DTOs;

namespace ConfigManager.Domain.Interfaces
{
    public interface IConfigurationService
    {
        Task SeedDataAsync(string? applicationName);
        Task<ConfigurationSetting?> GetConfigurationByIdAsync(string id);
        Task<IEnumerable<ConfigurationSetting>> GetAll();
        Task<IEnumerable<ConfigurationSetting>> GetAllConfigurationsAsync(string applicationName, string? search = null);
        Task Add(ConfigurationSetting setting);
        Task DeleteConfigurationAsync(string id);
        Task UpdateConfigurationAsync(string id, ConfigurationUpdateDto updatedSetting);
        Task<IEnumerable<string>> GetAllApplicationNamesAsync();
        Task<PagedResult<ConfigurationSetting>> GetAllConfigurationsAsyncPage(string applicationName, int page = 1, int size = 10, string? search = null);
    }
}
