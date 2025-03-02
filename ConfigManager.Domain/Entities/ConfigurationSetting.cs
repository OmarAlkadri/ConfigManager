using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ConfigManager.Domain.ValueObjects;

namespace ConfigManager.Domain.Entities
{
    public class ConfigurationSetting
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; private set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; private set; } = string.Empty;

        [BsonElement("type")]
        public SettingType Type { get; private set; } = SettingType.String;

        [BsonElement("value")]
        public string Value { get; private set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; private set; } = false;

        [BsonElement("applicationName")]
        public string ApplicationName { get; private set; } = string.Empty;

        private ConfigurationSetting() { }

        public ConfigurationSetting( string name, SettingType type, string value, bool isActive, string applicationName,string? _id)
        {
            this._id = string.IsNullOrEmpty(_id) ? ObjectId.GenerateNewId().ToString() : ObjectId.Parse(_id).ToString();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            IsActive = isActive;
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }
    }
}
