using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using MMR.ConfigPatchService.Model;
using MMR.Randomizer;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Services;

public class MirrorShieldService : IReflectionService // It uses reflection. Get it?
{
    private readonly ILogger<MirrorShieldService> _logger;
    private readonly IDependencyService _dependencyService;

    public MirrorShieldService(ILogger<MirrorShieldService> logger, IDependencyService dependencyService)
    {
        _logger = logger;
        _dependencyService = dependencyService;
    }

    public async Task<string> CallConfigurationProcessorViaVersion(Configuration configuration, string version, string seed)
    {
        var realVersion = "";
        if (version == "latest")
        {
            realVersion = await _dependencyService.EnsureLatestLibraryPresent();
        }
        /*else //TODO: Test other release versions once I have reflection working.
        {
            _dependencyService.DownloadSpecificRelease(version, false); //If present already, this does nothing and just returns happily.
        }*/
        var relativePathToMmr = $"{Constants.ReleaseDependencyFolder}/{realVersion}/content/MMR.Randomizer.dll";
        var absolutePathToMmr = "";
        if (File.Exists(relativePathToMmr))
        {
            absolutePathToMmr = Path.GetFullPath(relativePathToMmr);
        }

        var relativePathToMmrCli = $"{Constants.ReleaseDependencyFolder}/{realVersion}/content/MMR.CLI.dll";
        var absolutePathToMmrCli = "";
        if (File.Exists(relativePathToMmrCli))
        {
            absolutePathToMmrCli = Path.GetFullPath(relativePathToMmrCli);
        }

        var mmrDLL = Assembly.LoadFile(absolutePathToMmr);
        var mmrCliDLL = Assembly.LoadFile(absolutePathToMmrCli);

        // foreach (var exportedType in mmrDLL.GetTypes())
        // {
        //     _logger.LogInformation(exportedType.FullName);
        // }




        var configProcessorType = mmrDLL.GetType("MMR.Randomizer.ConfigurationProcessor");
        var configType = mmrDLL.GetType("MMR.Randomizer.Models.Settings.Configuration");

        var textWriterProgressReporterType = mmrCliDLL.GetType("MMR.CLI.Program+TextWriterProgressReporter");
        //Can we please make this not required in the future?
        Type progressReporterInterface = textWriterProgressReporterType.GetInterfaces()[0];





        var fromJsonMethod = configType.GetMethod("FromJson");
        dynamic typedConfiguration = fromJsonMethod.Invoke(null, new object[] { configuration.ToString() });

        MethodInfo processMethod = configProcessorType.GetMethod("Process");
        var configProcessorArgTypes = processMethod.GetParameters();
        foreach (var configProcessorParam in configProcessorArgTypes)
        {
            _logger.LogInformation(configProcessorParam.Name);
            _logger.LogInformation("Assignable: "+progressReporterInterface.IsAssignableTo(configProcessorParam.ParameterType));
            var type = configProcessorParam.GetType();
            _logger.LogInformation(type.Assembly.FullName);
            _logger.LogInformation(type.IsInterface.ToString());
        }



        return (string)processMethod.Invoke(null, new object[] {typedConfiguration, int.Parse(seed), null});

    }
}
