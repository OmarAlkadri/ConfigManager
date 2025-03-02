using ConfigManager.Domain.Entities;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using ConfigManager.Domain.DTOs;

namespace ConfigManager.API.Controllers
{
    [ApiController]
    [Route("api/configurations")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpGet("app/page/{applicationName}")]
        public async Task<ActionResult<PagedResult<ConfigurationSetting>>> GetAllByApplicationName(string applicationName, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null)
        {
            var configurations = await _configurationService.GetAllConfigurationsAsyncPage(applicationName, page, size, search);
            return Ok(configurations);
        }


        [HttpGet("app/{applicationName}")]
        public async Task<ActionResult<IEnumerable<ConfigurationSetting>>> GetAllByApplicationName(string applicationName, [FromQuery] string? search = null)
        {
            var configurations = await _configurationService.GetAllConfigurationsAsync(applicationName, search);
            if (configurations == null || !configurations.Any())
            {
                return Ok(new List<ConfigurationSetting>());
            }

            return Ok(configurations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfigurationSetting>> GetById(string id)
        {
            var config = await _configurationService.GetConfigurationByIdAsync(id);
            if (config == null)
            {
                return NotFound(new { Message = $"Configuration with ID '{id}' not found." });
            }
            return Ok(config);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ConfigurationCreateDto configDto)
        {
            if (configDto == null || string.IsNullOrEmpty(configDto.Name) || string.IsNullOrEmpty(configDto.ApplicationName))
            {
                return BadRequest(new { Message = "Invalid configuration data." });
            }

            var setting = new ConfigurationSetting(
                configDto.Name,
                configDto.Type,
                configDto.Value,
                configDto.IsActive,
                configDto.ApplicationName,
                null
            );

            await _configurationService.Add(setting);
            return Ok(new { Message = "Configuration saved successfully.", setting._id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var config = await _configurationService.GetConfigurationByIdAsync(id);
            if (config == null)
            {
                return NotFound(new { Message = $"Configuration with ID '{id}' not found." });
            }

            await _configurationService.DeleteConfigurationAsync(id);
            return NoContent();
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ConfigurationUpdateDto configDto)

        {
            try
            {
                if (configDto == null || string.IsNullOrEmpty(configDto.Value))
                {
                    return BadRequest(new { Message = "Invalid configuration update data." });
                }

                Console.WriteLine($"Attempting to update configuration with ID {id}");

                var existingConfig = await _configurationService.GetConfigurationByIdAsync(id);
                if (existingConfig == null)
                {
                    return NotFound(new { Message = $"Configuration with ID '{id}' not found." });
                }

                var updatedConfig = new ConfigurationUpdateDto(
                    configDto.Name,
                    configDto.Type,
                    configDto.ApplicationName,
                    configDto.Value
                );

                await _configurationService.UpdateConfigurationAsync(id, updatedConfig);

                return Ok(new { Message = "Configuration updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<string>>> GetApplicationNames()
        {
            var applicationNames = await _configurationService.GetAllApplicationNamesAsync();
            if (applicationNames == null || !applicationNames.Any())
            {
                return NotFound(new { Message = "No applications found." });
            }
            return Ok(applicationNames);
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedData(string? applicationName)
        {
            try
            {
                await _configurationService.SeedDataAsync(applicationName);
                return Ok(new { Message = "Seed data has been successfully added." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
