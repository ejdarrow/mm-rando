using MMR.Randomizer.Models.Settings;

namespace MMR.ConfigPatchService.Services;

public interface IProcessHandlerService
{
    public Task<bool> callProcess(Configuration configuration, String seed, String version);
}
