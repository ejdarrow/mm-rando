using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MMR.ConfigPatchService.Services;

namespace MMR.ConfigPatchService.Controllers;


[ApiController]
[Route("[controller]")]
public class PatchGenerationController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;
    private readonly IProcessHandlerService _processHandlerService;

    public PatchGenerationController(ILogger<ConfigController> logger, IProcessHandlerService processHandlerService)
    {
        _logger = logger;
        _processHandlerService = processHandlerService;
    }

    [HttpPost(template: "patch/default", Name = "GeneratePatchWithDefaultConfig")]
    public async Task<IActionResult> GeneratePatchWithDefaultConfiguration(string? seed, string version = "latest")
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Default Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var success = await _processHandlerService.callProcess(null, seed, version);
        return handleResult(success);

    }

    [HttpPost(template: "patch", Name = "GeneratePatch")]
    public async Task<IActionResult> GetPatch(string seed, JsonContent configuration, string version = "latest")
    {
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }
        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);

        var success = await _processHandlerService.callProcess(configuration.ToString(), seed, version);
        return handleResult(success);
    }

    [HttpPost(template: "patchWithFile", Name = "GeneratePatchFromFile")]
    public async Task<IActionResult> GetPatch(string? seed, IFormFile configuration, string version = "latest")
    {
        StreamReader reader = new StreamReader(configuration.OpenReadStream());
        String configurationAsString = reader.ReadToEnd();
        bool newSeed = false;
        if (string.IsNullOrEmpty(seed))
        {
            newSeed = true;
            seed = new Random().Next().ToString();
        }

        _logger.LogInformation("Requested Patch Generation with Uploaded Configuration and seed (new = {newSeed}) {seed} at {time}", newSeed, seed, DateTime.Now);
        var success = await _processHandlerService.callProcess(configurationAsString, seed, version);
        return handleResult(success);
    }

    private IActionResult handleResult(bool result)
    {
        //var outputPatchFilename = Constants.DefaultConfiguration.OutputSettings.OutputROMFilename.Replace("z64", "mmr");
        if (result)
        {
            if (System.IO.File.Exists("./patchgenerationresults/output.zip"))
            {
                System.IO.File.Delete("./patchgenerationresults/output.zip");
            }
            System.IO.File.Delete("./patchgenerationresults/output/patch.z64");
            ZipFile.CreateFromDirectory("./patchgenerationresults/output", "./patchgenerationresults/output.zip");
            byte[] fileBytes = System.IO.File.ReadAllBytes("./patchgenerationresults/output.zip");
            return File(fileBytes, "application/force-download", "patch.zip");
        }

        return BadRequest("Process Failure - Better error logging will someday exist.");
    }
}
