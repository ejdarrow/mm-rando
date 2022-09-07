using MMR.Randomizer;
using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Services;

public interface IReflectionService
{
    public Task<string> CallConfigurationProcessorViaVersion(Configuration configuration, string version, string seed);

}
