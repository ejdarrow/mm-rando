using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Model;

public class Constants
{
    public static Configuration DEFAULT_CONFIGURATION = new Configuration

    {
        CosmeticSettings = new CosmeticSettings(),
        GameplaySettings = new GameplaySettings
        {
            ShortenCutsceneSettings = new ShortenCutsceneSettings(),
        },
        OutputSettings = new OutputSettings()
        {
            InputROMFilename = "input.z64",
            GeneratePatch = true,
            GenerateROM = false,
            OutputVC = false
        },
    };
}
