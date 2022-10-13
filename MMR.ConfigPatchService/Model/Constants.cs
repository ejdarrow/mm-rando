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

    public static readonly string GithubReleaseOwner = "ZoeyZolotova";
    public static readonly string GithubReleaseRepo = "mm-rando";
    public static readonly string GithubLookupApplicationName = "mmr-patch-webservice";
    public static readonly string ReleaseDependencyFolder = "ReleaseLibraries";
}
