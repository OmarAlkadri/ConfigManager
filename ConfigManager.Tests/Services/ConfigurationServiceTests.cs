using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace ConfigManager.Tests.Services
{
    public class ConfigurationServiceTests
    {
        private readonly Mock<IConfigurationRepository> _mockRepository;
        private readonly Mock<IMessageBroker> _mockMessageBroker;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly IConfigurationService _service;

        public ConfigurationServiceTests()
        {
            _mockRepository = new Mock<IConfigurationRepository>();
            _mockMessageBroker = new Mock<IMessageBroker>();
            _mockCache = new Mock<IMemoryCache>();
            _service = new IConfigurationService(_mockRepository.Object, _mockMessageBroker.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetConfigurationAsync_ShouldReturnSetting_WhenExists()
        {
            // Arrange
            var expectedSetting = new ConfigurationSetting("SiteName", SettingType.String, "example.com", true, "SERVICE-A");
            _mockRepository.Setup(r => r.GetByNameAsync("SiteName", "SERVICE-A")).ReturnsAsync(expectedSetting);

            // Mock TryGetValue to return the expected setting when cache is checked
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out expectedSetting))
                      .Returns(true);

            // Act
            var result = await _service.GetConfigurationAsync("SiteName", "SERVICE-A");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("SiteName");
            result.Value.Should().Be("example.com");

            // Verify that TryGetValue was called once
            _mockCache.Verify(c => c.TryGetValue(It.IsAny<object>(), out expectedSetting), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateConfigurationAsync_ShouldPublishEvent_WhenCalled()
        {
            // Arrange
            var setting = new ConfigurationSetting("SiteName", SettingType.String, "example.com", true, "SERVICE-A");
            _mockRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<ConfigurationSetting>()))
                           .Returns(Task.CompletedTask);

            // Mock Set to simulate adding/updating the setting in the cache
            _mockCache.Setup(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan>()));

            // Act
            await _service.AddOrUpdateConfigurationAsync("SiteName", SettingType.String, "example.com", true, "SERVICE-A");

            // Assert
            _mockMessageBroker.Verify(mb => mb.PublishConfigurationUpdateAsync("SiteName"), Times.Once);

            // Verify that Set was called once in the cache
            _mockCache.Verify(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
