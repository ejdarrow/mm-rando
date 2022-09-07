using Microsoft.AspNetCore.Mvc;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer.Models.Settings;
using MMR.Randomizer.Utils;

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
        return Constants.DefaultConfiguration;
    }

    [HttpGet(template: "validaterom", Name = "Validate")]
    public string ValidateLocalFiles()
    {
        var fileExists = System.IO.File.Exists("default.z64");
        var fileValid = RomUtils.ValidateROM("default.z64");
        return $"Expected local default MM file exists: {fileExists}. File is Valid: {fileValid}";
    }




}
