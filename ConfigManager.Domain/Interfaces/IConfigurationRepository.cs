using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.DTOs;

namespace ConfigManager.Domain.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<ConfigurationSetting?> GetByIdAsync(string id);
        Task<IEnumerable<ConfigurationSetting>> GetAll();
        Task<IEnumerable<ConfigurationSetting>> GetAllAsync(string applicationName);
        Task Add(ConfigurationSetting setting);
        Task UpdateAsync(string id, ConfigurationUpdateDto setting);
        Task DeleteAsync(string id);
        Task SeedDataAsync();
        Task<IEnumerable<string>> GetAllApplicationNamesAsync();
        Task<PagedResult<ConfigurationSetting>> GetAllByApplicationNamePage(string applicationName, int page = 1, int size = 10, string? search = null);

    }
}
