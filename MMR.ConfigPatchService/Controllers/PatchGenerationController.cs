using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MMR.Common.Utils;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Controllers;

[ApiController]
[Route("[controller]")]
public class PatchGenerationController : ControllerBase
{
    private readonly ILogger<ConfigGenerationController> _logger;

    public PatchGenerationController(ILogger<ConfigGenerationController> logger)
    {
        _logger = logger;
    }

    [HttpPost(template: "patch/default", Name = "GeneratePatchWithDefaultConfig")]
    public string GeneratePatchWithDefaultConfiguration(string? seed)
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Default Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        return ConfigurationProcessor.Process(Constants.DEFAULT_CONFIGURATION, int.Parse(seed), keepaliveProgressReporter);
    }

    [HttpPost(template: "patch", Name = "GeneratePatch")]
    public string GetPatch(string seed, Configuration configuration)
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        return ConfigurationProcessor.Process(configuration, int.Parse(seed), keepaliveProgressReporter);
    }

    [HttpPost(template: "patchWithFile", Name = "GeneratePatchFromFile")]
    public string GetPatch(string? seed, IFormFile configuration)
    {

        StreamReader reader = new StreamReader(configuration.OpenReadStream());
        String configurationAsString = reader.ReadToEnd();
        var configurationFromFile = JsonSerializer.Deserialize<Configuration>(configurationAsString);
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        return ConfigurationProcessor.Process(configurationFromFile, int.Parse(seed), keepaliveProgressReporter);
    }
}
