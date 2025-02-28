using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManager.Domain.Entities;
using ConfigManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace ConfigManager.API.Controllers
{
    [ApiController]
    [Route("api/configurations")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationService _configurationService;

        public ConfigurationController(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpGet("{applicationName}")]
        public async Task<ActionResult<IEnumerable<ConfigurationSetting>>> GetAll(string applicationName)
        {
            var configurations = await _configurationService.GetAllConfigurationsAsync(applicationName);
            return Ok(configurations);
        }

        [HttpGet("{applicationName}/{name}")]
        public async Task<ActionResult<ConfigurationSetting>> Get(string applicationName, string name)
        {
            var config = await _configurationService.GetConfigurationAsync(name, applicationName);
            if (config == null) return NotFound();
            return Ok(config);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate([FromBody] ConfigurationDto configDto)
        {
            await _configurationService.AddOrUpdateConfigurationAsync(configDto.Name, configDto.Type, configDto.Value, configDto.IsActive, configDto.ApplicationName);
            return Ok(new { Message = "Configuration saved successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _configurationService.DeleteConfigurationAsync(id);
            return NoContent();
        }
        public record ConfigurationDto(string Name, SettingType Type, string Value, bool IsActive, string ApplicationName);
    }
}
