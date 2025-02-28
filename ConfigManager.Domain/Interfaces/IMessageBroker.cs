using System.Threading.Tasks;

namespace ConfigManager.Domain.Interfaces
{
    public interface IMessageBroker
    {
        Task PublishConfigurationUpdateAsync(string settingName);
    }
}
