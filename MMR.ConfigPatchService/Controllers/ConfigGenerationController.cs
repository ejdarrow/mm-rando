using Microsoft.AspNetCore.Mvc;
using MMR.Common.Utils;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigGenerationController : ControllerBase
{
    private readonly ILogger<ConfigGenerationController> _logger;

    public ConfigGenerationController(ILogger<ConfigGenerationController> logger)
    {
        _logger = logger;
    }


    [HttpGet(template: "default", Name = "DefaultConfig")]
    public Configuration GetDefaultConfig()
    {
        return Constants.DEFAULT_CONFIGURATION;
    }




}
