using System.Text.Json.Serialization;

namespace ProjectAI.Services
{
    public interface IAiService
    {
        Task<List<string>> DetectMainObjectsAsync(Stream imageStream);
        Task<Dictionary<string, List<string>>> DetectMaterialsAsync(Stream imageStream, List<string> objectNames);
    }
}


