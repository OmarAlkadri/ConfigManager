using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.ValueObjects;
using ConfigManager.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using ConfigManager.API.Controllers;

namespace ConfigManager.Tests.Controllers
{
    public class ConfigurationControllerTests
    {
        private readonly Mock<IConfigurationService> _mockService;
        private readonly ConfigurationController _controller;

        public ConfigurationControllerTests()
        {
            _mockService = new Mock<IConfigurationService>(null!, null!);
            _controller = new ConfigurationController(_mockService.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenConfigurationDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetConfigurationAsync("InvalidSetting", "SERVICE-A")).ReturnsAsync((ConfigurationSetting?)null);

            // Act
            var result = await _controller.Get("SERVICE-A", "InvalidSetting");

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Add_ShouldReturnOk_WhenConfigurationIsAdded()
        {
            // Arrange
            var configDto = new ConfigurationController.ConfigurationDto("SiteName", SettingType.String, "example.com", true, "SERVICE-A");

            // Act
            var result = await _controller.Add(configDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
