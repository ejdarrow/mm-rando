using System.Text.Encodings.Web;
using System.Text.Json;

namespace MMR.DevTool
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            WriteIndented = true,
        };
    }
}
