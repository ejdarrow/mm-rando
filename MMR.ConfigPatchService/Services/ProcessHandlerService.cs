using System.Diagnostics;
using MMR.ConfigPatchService.Model;

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

    public async Task<bool> callProcessActorizerDebugging(string? seed, string version)
    {
        var patchDirectory = "./patchgenerationresults";
        var realVersion = "";
        if (version == "latest")
        {
            realVersion = await _dependencyService.EnsureLatestLibraryPresent();
        }
        else
        {
            realVersion = await _dependencyService.EnsureSpecificLibraryPresent(version);
        }

        //Relative to patchDirectory
        var versionPath = $"{Constants.ReleaseDependencyFolder}/{realVersion}/content";
        var seedint = 0;
        if (!int.TryParse(seed, out seedint))
        {
            seedint = new Random().Next();
        }

        string? settingsPath = patchDirectory + "/actorizerTestingSettings.json";

        return await GeneratePatch(versionPath, seedint, "patch", settingsPath);
    }
    public async Task<bool> callProcess(String? configuration, string? seed, string version)
    {
        var patchDirectory = "./patchgenerationresults";
        var realVersion = "";
        if (version == "latest")
        {
            realVersion = await _dependencyService.EnsureLatestLibraryPresent();
        }
        else
        {
            realVersion = await _dependencyService.EnsureSpecificLibraryPresent(version);
        }

        //Relative to patchDirectory
        var versionPath = $"{Constants.ReleaseDependencyFolder}/{realVersion}/content";
        var seedint = 0;
        if (!int.TryParse(seed, out seedint))
        {
            seedint = new Random().Next();
        }

        string? settingsPath = null;
        if (configuration != null)
        {
            await File.WriteAllTextAsync($"{patchDirectory}/settings.json", configuration.ToString());
            settingsPath = "settings.json";
        }
        return await GeneratePatch(versionPath, seedint, "patch", settingsPath);
    }

    private async Task<bool> GeneratePatch(string versionPath, int seed, string filename, string? settingsPath)
    {

        var cliDllPath = Path.Combine(versionPath, "MMR.CLI.dll");
        if (!File.Exists(cliDllPath))
        {
            throw new Exception("cliDLLPath doesn't exist");
        }
        var output = Path.Combine("output", filename);
        var processInfo = new ProcessStartInfo("dotnet");
        processInfo.WorkingDirectory = ".";
        processInfo.Arguments = $"{cliDllPath} -input \"./default.z64\" -output \"./patchgenerationresults/{output}.z64\" -seed {seed} -spoiler -outputpatch";
        processInfo.Arguments += $" -maxImportanceWait 150";
        if (settingsPath != null)
        {
            processInfo.Arguments += $" -settings \"./patchgenerationresults/{settingsPath}";
        }

        processInfo.ErrorDialog = false;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;
        var proc = Process.Start(processInfo);
        proc.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Trace.WriteLine(errorLine.Data); };
        proc.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) Trace.WriteLine(outputLine.Data); };
        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();

        proc.WaitForExit();
        return proc.ExitCode == 0;
    }
}
