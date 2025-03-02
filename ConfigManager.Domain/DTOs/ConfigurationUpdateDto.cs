using ConfigManager.Domain.ValueObjects;

namespace ConfigManager.Domain.DTOs
{
    public record ConfigurationUpdateDto(
        string Name,
        SettingType Type,
        string Value,
        string ApplicationName
    );
}
