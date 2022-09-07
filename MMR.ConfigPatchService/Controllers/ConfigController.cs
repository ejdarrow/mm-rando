using Microsoft.AspNetCore.Mvc;
using MMR.ConfigPatchService.Model;
using MMR.ConfigPatchService.Services;
using MMR.Randomizer.Models.Settings;
using MMR.Randomizer.Utils;

namespace MMR.ConfigPatchService.Controllers;

//TODO: More logging on requests
[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;

    private readonly IDependencyService _dependencyService;
    public ConfigController(ILogger<ConfigController> logger, IDependencyService dependencyService)
    {
        _logger = logger;
        _dependencyService = dependencyService;
    }


    [HttpGet(template: "defaultConfig", Name = "DefaultConfig")]
    public Configuration GetDefaultConfig()
    {
        return Constants.DefaultConfiguration;
    }

    [HttpGet(template: "validateLocalConfig", Name = "Validate")]
    public IActionResult ValidateLocalFiles()
    {
        var fileExists = System.IO.File.Exists("default.z64");
        var fileValid = RomUtils.ValidateROM("default.z64");
        var message = $"Expected local default MM file exists: {fileExists}. File is Valid: {fileValid}";
        if (fileExists && fileValid)
        {
            return Ok(message);
        }

        return !fileExists ? NotFound(message) : Problem(message);
    }

    [HttpGet(template: "fetchRemoteReleaseDependencyInfo", Name = "FetchRemoteReleaseInfo")]
    public async Task<IActionResult> FetchRemoteReleaseDependencyInfo()
    {
        var releaseDetails = await _dependencyService.ListRemoteLibraries();
        return Ok(releaseDetails);
    }

    [HttpGet(template: "fetchLocalReleaseDependencies", Name = "FetchLocallyDownloadedReleases")]
    public IActionResult FetchLocalReleaseDependencies()
    {
        return Ok(_dependencyService.ListCurrentlyDownloadedLibraries());
    }

    [HttpPost(template: "downloadLatestRelease", Name = "DownloadLatestRelease")]
    public async Task<IActionResult> DownloadLatestRelease(bool force = false)
    {
        string result = await _dependencyService.DownloadLatestRelease(force);
        return Ok(result);
    }

    [HttpPost(template: "downloadRelease/{tag}", Name = "DownloadSpecificRelease")]
    public async Task<IActionResult> DownloadSpecificRelease(string tag, bool force)
    {
        string result = await _dependencyService.DownloadSpecificRelease(tag, force);
        return Ok(result);
    }




}
