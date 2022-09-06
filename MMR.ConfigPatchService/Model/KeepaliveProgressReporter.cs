using Microsoft.AspNetCore.Authentication;
using MMR.Randomizer;

namespace MMR.ConfigPatchService.Model;

//TODO: make this send keepalive packets while the patch is being made.
public class KeepaliveProgressReporter : IProgressReporter
{
    public void ReportProgress(int percentProgress, string message, CancellationTokenSource ctsItemImportance)
    {
        var i = 1;
    }
}
