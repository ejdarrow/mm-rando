using MMR.DevTool.Models;
using System.CommandLine;
using System.IO;
using System.Text.Json;

namespace MMR.DevTool.CLI
{
    class Program
    {
        static void WriteUserInterfaceConfig(string path)
        {
            var config = UserInterfaceConfig.Build();
            var json = JsonSerializer.Serialize(config, JsonOptions.SerializerOptions);
            File.WriteAllText(path, json);
        }

        static void ProcessCommandLine(string uiConfigOutputPath)
        {
            WriteUserInterfaceConfig(uiConfigOutputPath);
        }

        static int Main(string[] args)
        {
            var uiConfigOutputPathOption = new Option<string>("--ui-config-output", description: "Path to write UI config.");
            uiConfigOutputPathOption.IsRequired = true;

            var rootCommand = new RootCommand
            {
                uiConfigOutputPathOption,
            };
            rootCommand.Description = "Majora's Mask Randomizer utility for development purposes.";

            rootCommand.SetHandler(static (string uiConfigOutputPath) =>
            {
                ProcessCommandLine(uiConfigOutputPath);
            }, uiConfigOutputPathOption);

            return rootCommand.Invoke(args);
        }
    }
}
