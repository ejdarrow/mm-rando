using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MMR.Common.Utils;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Controllers;


//TODO: Return Zip of all resources rather than just mmr file
//TODO: Verify and validate config file upload deserialization

[ApiController]
[Route("[controller]")]
public class PatchGenerationController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;

    public PatchGenerationController(ILogger<ConfigController> logger)
    {
        _logger = logger;
    }

    [HttpPost(template: "patch/default", Name = "GeneratePatchWithDefaultConfig")]
    public IActionResult GeneratePatchWithDefaultConfiguration(string? seed)
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Default Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        var config = Constants.DefaultConfiguration;
        config.OutputSettings.GenerateROM = false; //For overrides for local testing
        return handleResult(ConfigurationProcessor.Process(config, int.Parse(seed), keepaliveProgressReporter));

    }

    [HttpPost(template: "patch", Name = "GeneratePatch")]
    public IActionResult GetPatch(string seed, Configuration configuration)
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        configuration.OutputSettings.GenerateROM = false;
        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        return handleResult(ConfigurationProcessor.Process(configuration, int.Parse(seed), keepaliveProgressReporter));

    }

    [HttpPost(template: "patchWithFile", Name = "GeneratePatchFromFile")]
    public IActionResult GetPatch(string? seed, IFormFile configuration)
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

        if (configurationFromFile == null)
        {
            return BadRequest("Could not deserialize config file");
        }
        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var keepaliveProgressReporter = new KeepaliveProgressReporter();
        configurationFromFile.OutputSettings.GenerateROM = false;
        return handleResult(ConfigurationProcessor.Process(configurationFromFile, int.Parse(seed), keepaliveProgressReporter));
    }

    private IActionResult handleResult(string? result)
    {
        var outputPatchFilename = Constants.DefaultConfiguration.OutputSettings.OutputROMFilename.Replace("z64", "mmr");
        if (result == null)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(outputPatchFilename);
            return File(fileBytes, "application/force-download", "patch.mmr");
        }

        return BadRequest(result);
    }
}
