
namespace ProjectAI.Services
{
    public class MockAiService : IAiService
    {
        public Task<List<string>> DetectMainObjectsAsync(Stream imageStream) =>
            Task.FromResult(new List<string> { "стол", "ручка" });


        public Task<Dictionary<string, List<string>>> DetectMaterialsAsync(Stream imageStream, List<string> objectNames) =>
            Task.FromResult(new Dictionary<string, List<string>>
            {
                ["стол"] = new() { "дерево" },
                ["ручка"] = new() { "пластик", "металл" }
            });
    }
}
