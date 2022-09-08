using System.IO.Compression;
using MMR.ConfigPatchService.Model;

namespace MMR.ConfigPatchService.Services;

using Octokit;

public class GithubDependencyService : IDependencyService
{
    private readonly ILogger<GithubDependencyService> _logger;
    private readonly GitHubClient _client;
    private readonly HttpClient _httpClient;

    public GithubDependencyService(ILogger<GithubDependencyService> logger)
    {
        _client = new GitHubClient(new ProductHeaderValue(Constants.GithubLookupApplicationName));
        _httpClient = new HttpClient();
        _logger = logger;
        if (!File.Exists(Constants.ReleaseDependencyFolder))
        {
            Directory.CreateDirectory(Constants.ReleaseDependencyFolder);
        }
    }

    public IList<string> ListCurrentlyDownloadedLibraries()
    {
        IList<string> releaseObjects = new List<string>(Directory.GetDirectories(Constants.ReleaseDependencyFolder));
        return releaseObjects;
    }

    public async Task<string> EnsureLatestLibraryPresent()
    {
        await DownloadLatestRelease(false);
        var latestRelease =
            await _client.Repository.Release.GetLatest(Constants.GithubReleaseOwner, Constants.GithubReleaseRepo);
        return latestRelease.TagName;
    }

    public async Task<IList<string>> ListRemoteLibraries()
    {
        var releases =
            await _client.Repository.Release.GetAll(Constants.GithubReleaseOwner, Constants.GithubReleaseRepo);
        var releaseDetails = new List<string>();
        foreach (var release in releases)
        {
            string releaseDetail =
                $"Release {release.Name} is tagged {release.TagName} and assets are at {release.AssetsUrl}";
            //_logger.LogDebug(releaseDetail);
            releaseDetails.Add(release.TagName);
        }
        _logger.LogInformation($"Fetched {releaseDetails.Count} remote releases.");
        return releaseDetails;
    }

    public async Task<string> DownloadSpecificRelease(string releaseTag, bool force)
    {
        var knownRemoteLibraries = await ListRemoteLibraries();
        if (!knownRemoteLibraries.Contains(releaseTag))
        {
            _logger.LogError($"User requested tag {releaseTag} not present in public releases");
            return
                $"Unknown tag ({releaseTag}) specified - Is it a public release on {Constants.GithubReleaseOwner}/{Constants.GithubReleaseRepo}?";
        }
        var release =
            await _client.Repository.Release.Get(Constants.GithubReleaseOwner, Constants.GithubReleaseRepo,
                releaseTag);
        return await DownloadRelease(release, force);
    }

    public async Task<string> DownloadLatestRelease(bool force)
    {
        var latestRelease =
            await _client.Repository.Release.GetLatest(Constants.GithubReleaseOwner, Constants.GithubReleaseRepo);
        return await DownloadRelease(latestRelease, force);
    }

    private async Task<string> DownloadRelease(Release release, bool forceRedownload)
    {
        var likelyAsset = release.Assets[0];
        //TODO: make sure the above asset is actually the one we care about in case we eventually have multiple.

        var expectedPath = $"{Constants.ReleaseDependencyFolder}/{release.TagName}/{likelyAsset.Name}";
        if (File.Exists(expectedPath) && !forceRedownload)
        {
            _logger.LogWarning($"Release {release.Name} already present in dependency directory");
            return release.TagName + " already local";
        }
        else
        {
            if (!Directory.Exists($"{Constants.ReleaseDependencyFolder}/{release.TagName}"))
            {
                Directory.CreateDirectory($"{Constants.ReleaseDependencyFolder}/{release.TagName}");
            }
        }

        await DownloadFileAsync(likelyAsset.BrowserDownloadUrl, expectedPath);
        ZipFile.ExtractToDirectory(expectedPath,$"{Constants.ReleaseDependencyFolder}/{release.TagName}/content");
        return release.TagName + " downloaded";
    }

    private async Task DownloadFileAsync(string uri
        , string outputPath)
    {
        Uri uriResult;

        if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
            throw new InvalidOperationException("URI is invalid.");


        byte[] fileBytes = await _httpClient.GetByteArrayAsync(uri);
        await File.WriteAllBytesAsync(outputPath, fileBytes);


    }
}
