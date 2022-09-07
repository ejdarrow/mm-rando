namespace MMR.ConfigPatchService.Services;

public interface IDependencyService
{

    public IList<string> ListCurrentlyDownloadedLibraries();

    /**
     * <see cref="GithubDependencyService.ListRemoteLibraries"/>
     */
    Task<IList<string>> ListRemoteLibraries();

    Task<string> DownloadLatestRelease();
    Task<string> DownloadSpecificRelease(string releaseTag);

}
