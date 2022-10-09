
namespace MMR.ConfigPatchService.Services;

public interface IProcessHandlerService
{
    public Task<bool> callProcess(String? configuration, String seed, String version);

    public Task<bool> callProcessActorizerDebugging(string? seed, string version);

}
