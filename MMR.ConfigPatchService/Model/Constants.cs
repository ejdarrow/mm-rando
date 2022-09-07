using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Model;

public static class Constants
{
    public static readonly Configuration DefaultConfiguration = new Configuration

    {
        CosmeticSettings = new CosmeticSettings(),
        GameplaySettings = new GameplaySettings
        {
            ShortenCutsceneSettings = new ShortenCutsceneSettings(),
        },
        OutputSettings = new OutputSettings()
        {
            InputROMFilename = "default.z64",
            GeneratePatch = true,
            GenerateROM = false,
            OutputVC = false,
            OutputROMFilename = "patchgenerationresults/patch.z64",
        },
    };
}
