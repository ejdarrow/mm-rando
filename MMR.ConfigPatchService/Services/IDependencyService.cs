namespace MMR.ConfigPatchService.Services;

public interface IDependencyService
{

    public IList<string> ListCurrentlyDownloadedLibraries();

    public Task<string> EnsureLatestLibraryPresent();

    public Task<string> EnsureSpecificLibraryPresent(string version);


    /**
     * <see cref="GithubDependencyService.ListRemoteLibraries"/>
     */
    Task<IList<string>> ListRemoteLibraries();

    Task<string> DownloadLatestRelease(bool force);
    Task<string> DownloadSpecificRelease(string releaseTag, bool force);

}
