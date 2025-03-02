using ConfigManager.Domain.ValueObjects;

namespace ConfigManager.Domain.DTOs
{
    public record ConfigurationCreateDto(
        string Name,
        SettingType Type,
        string Value,
        bool IsActive,
        string ApplicationName
    );
}
