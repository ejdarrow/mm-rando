using System.Diagnostics;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Services;

public class ProcessHandlerService : IProcessHandlerService
{
    private readonly ILogger<ProcessHandlerService> _logger;
    private readonly IDependencyService _dependencyService;

    public ProcessHandlerService(ILogger<ProcessHandlerService> logger, IDependencyService dependencyService)
    {
        _logger = logger;
        _dependencyService = dependencyService;
    }


    public async Task<bool> callProcess(Configuration configuration, string? seed, string version)
    {
        //TODO: Enable multi-version after testing singles
        var latest = await _dependencyService.EnsureLatestLibraryPresent();
        var versionPath = $"{Constants.ReleaseDependencyFolder}/{latest}/content";
        var seedint = 0;
        if (!int.TryParse(seed, out seedint))
        {
            seedint = new Random().Next();
        }

        //TODO: Leverage wine?
        string gameplaySettings = configuration.GameplaySettings.ToString();
        await File.WriteAllTextAsync($"{versionPath}/output/settings.json", configuration.ToString());
        return await GeneratePatch(versionPath, seedint, "patch", $"{versionPath}/output/settings.json");
    }

    private async Task<bool> GeneratePatch(string versionPath, int seed, string filename, string settingsPath)
    {

        var cliDllPath = Path.Combine(versionPath, "MMR.CLI.dll");
        if (!File.Exists(cliDllPath))
        {
            throw new Exception("cliDLLPath doesn't exist");
        }
        var output = Path.Combine("output", filename);
        var processInfo = new ProcessStartInfo("dotnet");
        processInfo.WorkingDirectory = versionPath;
        processInfo.Arguments = $"{cliDllPath} -output \"{output}.z64\" -seed {seed} -spoiler -patch";
        processInfo.Arguments += $" -maxImportanceWait 150 -settings \"{settingsPath}\"";

        processInfo.ErrorDialog = false;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;
        _logger.LogInformation(processInfo.FileName);
        _logger.LogInformation(processInfo.Verb);
        _logger.LogInformation(processInfo.Arguments);
        var proc = Process.Start(processInfo);
        proc.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Trace.WriteLine(errorLine.Data); };
        proc.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) Trace.WriteLine(outputLine.Data); };
        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();

        proc.WaitForExit();
        return proc.ExitCode == 0;
    }
}
