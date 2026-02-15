using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAI.Data;
using ProjectAI.Data.Entities;
using ProjectAI.DTOs;
using ProjectAI.Services;

namespace ProjectAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ImageDownloader _imageDownloader;
        private readonly AppDbContext _dbContext;

        private readonly HttpClient _httpClient;

        

        public AnalysisController(IAiService aiService, ImageDownloader imageDownloader, AppDbContext dbContext,IHttpClientFactory factory)
        {
            _aiService = aiService;
            _imageDownloader = imageDownloader;
            _dbContext = dbContext;
            _httpClient = factory.CreateClient("MyClient");
        }
        [HttpPost]

        public async Task<ActionResult<AnalysisResultDto>> Analyze([FromQuery] string imageUrl)
        {
            using var imageStream = await _imageDownloader.DownloadAsync(imageUrl);

            var detectedObjects = await _aiService.DetectMainObjectsAsync(imageStream);

            var request = new Request
            {
                ImageUrl = imageUrl,
                AiRawResponse = JsonSerializer.Serialize(detectedObjects),
                CreatedAt = DateTime.UtcNow,
                Items = detectedObjects.Select(obj => new Item
                {
                    Name = obj,
                    IsUserCorrected = false,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };
            _dbContext.Requests.Add(request);
            await _dbContext.SaveChangesAsync();

            return Ok(new AnalysisResultDto
            (
                request.Id,
                detectedObjects
            ));
        }
        [HttpPost("{id}/materials")]
        public async Task<ActionResult<CorrectionResultDto>> DetermineMaterials(int id, [FromBody] CorrectionRequestDto correction)
        {
            var request = await _dbContext.Requests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();

            using var imageStream = await _imageDownloader.DownloadAsync(request.ImageUrl);
            var materialsDict = await _aiService.DetectMaterialsAsync(imageStream, correction.Items);

            foreach (var objName in correction.Items)
            {
                var item = new Item
                {
                    RequestId = id,
                    Name = objName,
                    IsUserCorrected = true,
                    CreatedAt = DateTime.UtcNow,
                    Materials = materialsDict.TryGetValue(objName, out var mats)
                        ? mats.Select(m => new Material { Name = m }).ToList()
                        : new List<Material>()
                };
                _dbContext.Items.Add(item);
            }
            await _dbContext.SaveChangesAsync();

            var resultItems = correction.Items.Select(obj => new ItemMaterialDto(
                obj,
                materialsDict.GetValueOrDefault(obj) ?? new List<string>()
            )).ToList();

            return Ok(new CorrectionResultDto(resultItems)); 
        }
    }
}
