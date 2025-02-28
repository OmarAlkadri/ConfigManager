using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;

namespace ConfigManager.Domain.Repositories
{
    public interface IConfigurationRepository
    {
        Task<ConfigurationSetting?> GetByNameAsync(string name, string applicationName);
        Task<IEnumerable<ConfigurationSetting>> GetAllAsync(string applicationName);
        Task AddOrUpdateAsync(ConfigurationSetting setting);
        Task DeleteAsync(Guid id);
    }
}
