using ConfigManager.Domain.ValueObjects;

namespace ConfigManager.Domain.Entities
{
    public class ConfigurationSetting
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public SettingType Type { get; private set; }
        public string Value { get; private set; }
        public bool IsActive { get; private set; }
        public string ApplicationName { get; private set; }

        private ConfigurationSetting() { }

        public ConfigurationSetting(string name, SettingType type, string value, bool isActive, string applicationName)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            IsActive = isActive;
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }

        public void UpdateValue(string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                throw new ArgumentException("Value cannot be empty.", nameof(newValue));

            Value = newValue;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
