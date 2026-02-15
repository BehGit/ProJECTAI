
//using System.Net.Http.Headers;
//using System.Text.Json;
//using System.Text;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json.Serialization;

//namespace ProjectAI.Services
//{
//    public class GigaChatAiService : IAiService
//    {


//        private readonly HttpClient _httpClient;
//        private readonly IConfiguration _config;
//        private readonly ILogger<GigaChatAiService> _logger;
//        private string _cachedToken;
//        private DateTime _tokenExpiration;

//        public GigaChatAiService(HttpClient httpClient, IConfiguration config, ILogger<GigaChatAiService> logger)
//        {
//            _httpClient = httpClient;
//            _config = config;
//            _logger = logger;
//        }


//        private async Task<string> GetAccessTokenAsync()
//        {
//            if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiration > DateTime.UtcNow)
//                return _cachedToken;

//            var authUrl = _config["GigaChat:AuthUrl"];
//            var clientId = _config["GigaChat:ClientId"];
//            var clientSecret = _config["GigaChat:ClientSecret"];
//            var scope = _config["GigaChat:Scope"];

//            var request = new HttpRequestMessage(HttpMethod.Post, authUrl);
//            request.Headers.Add("Accept", "application/json");
//            request.Headers.Add("Authorization",
//                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))}");
//            request.Headers.Add("RqUID", Guid.NewGuid().ToString());

//            request.Content = new FormUrlEncodedContent(new[]
//            {
//                new KeyValuePair<string, string>("scope", scope)
//            });

//            var response = await _httpClient.SendAsync(request);
//            response.EnsureSuccessStatusCode();

//            var authResponse = await response.Content.ReadFromJsonAsync<GigaChatAuthResponse>();
//            _cachedToken = authResponse.AccessToken;
//            _tokenExpiration = DateTimeOffset.FromUnixTimeSeconds(authResponse.ExpiresAt).UtcDateTime;
//            return _cachedToken;
//        }


//        public async Task<List<string>> DetectMainObjectsAsync(Stream imageStream)
//        {
//            var token = await GetAccessTokenAsync();
//            var base64Image = await ConvertStreamToBase64Async(imageStream);

//            var prompt = @"Ты – система компьютерного зрения. Определи основные предметы на фотографии.";
//            var messageContent = new List<object>
//            {
//                new { type = "text", text = prompt },
//                new
//                {
//                    type = "image_url",
//                    image_url = new
//                    {
//                        url = $"data:image/jpeg;base64,{base64Image}",
//                        detail = "high"
//                    }
//                }
//            };

//            var requestBody = new GigaChatRequest
//            {
//                Messages = new List<GigaChatMessage>
//    {
//        new GigaChatMessage { Content = JsonSerializer.Serialize(messageContent) }
//    }
//            };

//            using var request = new HttpRequestMessage(HttpMethod.Post, _config["GigaChat:ApiUrl"]);
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            request.Content = JsonContent.Create(
//                requestBody,
//                options: new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//                }
//            );

//            var response = await _httpClient.SendAsync(request);
//            response.EnsureSuccessStatusCode();

//            var result = await response.Content.ReadFromJsonAsync<GigaChatResponse>();
//            var answer = result.Choices?.FirstOrDefault()?.Message?.Content;
//            if (string.IsNullOrEmpty(answer))
//                return new List<string>();

//            var jsonArray = ExtractJsonArray(answer);
//            if (!string.IsNullOrEmpty(jsonArray))
//            {
//                try
//                {
//                    return JsonSerializer.Deserialize<List<string>>(jsonArray) ?? new List<string>();
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Failed to deserialize JSON array");
//                }
//            }

//            return new List<string>();
//        }

//        public async Task<Dictionary<string, List<string>>> DetectMaterialsAsync(Stream imageStream, List<string> objectNames)
//        {
//            var token = await GetAccessTokenAsync();
//            var base64Image = await ConvertStreamToBase64Async(imageStream);

//            var itemsList = JsonSerializer.Serialize(objectNames);
//            var prompt = $@"Ты – система компьютерного зрения. На фотографии присутствуют следующие предметы: {itemsList}.";
//            var messageContent = new List<object>
//            {
//                new { type = "text", text = prompt },
//                new
//                {
//                    type = "image_url",
//                    image_url = new
//                    {
//                        url = $"data:image/jpeg;base64,{base64Image}",
//                        detail = "high"
//                    }
//                }
//            };

//            var requestBody = new GigaChatRequest
//            {
//                Messages = new List<GigaChatMessage>
//    {
//        new GigaChatMessage { Content = JsonSerializer.Serialize(messageContent) }
//    }
//            };

//            using var request = new HttpRequestMessage(HttpMethod.Post, _config["GigaChat:ApiUrl"]);
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            request.Content = JsonContent.Create(
//                requestBody,
//                options: new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//                }
//            );

//            var response = await _httpClient.SendAsync(request);
//            response.EnsureSuccessStatusCode();

//            var result = await response.Content.ReadFromJsonAsync<GigaChatResponse>();
//            var answer = result.Choices?.FirstOrDefault()?.Message?.Content;
//            if (string.IsNullOrEmpty(answer))
//                return new Dictionary<string, List<string>>();

//            var jsonObject = ExtractJsonObject(answer);
//            if (!string.IsNullOrEmpty(jsonObject))
//            {
//                try
//                {
//                    return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonObject)
//                           ?? new Dictionary<string, List<string>>();
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Failed to deserialize JSON object");
//                }
//            }

//            return new Dictionary<string, List<string>>();

//        }
//        private async Task<string> ConvertStreamToBase64Async(Stream stream)
//        {
//            using var memoryStream = new MemoryStream();
//            await stream.CopyToAsync(memoryStream);
//            return Convert.ToBase64String(memoryStream.ToArray());
//        }

//        private string ExtractJsonArray(string text)
//        {
//            var start = text.IndexOf('[');
//            var end = text.LastIndexOf(']');
//            return (start != -1 && end != -1 && end > start)
//                ? text.Substring(start, end - start + 1)
//                : null;
//        }

//        private string ExtractJsonObject(string text)
//        {
//            var start = text.IndexOf('{');
//            var end = text.LastIndexOf('}');
//            return (start != -1 && end != -1 && end > start)
//                ? text.Substring(start, end - start + 1)
//                : null;
//        }
//    }
//    public class GigaChatAuthResponse
//    {
//        [JsonPropertyName("access_token")]
//        public string AccessToken { get; set; } = string.Empty;
//        [JsonPropertyName("expires_at")]
//        public long ExpiresAt { get; set; }
//    }

//    public class GigaChatMessage
//    {
//        [JsonPropertyName("role")]
//        public string Role { get; set; } = "user";
//        [JsonPropertyName("content")]
//        public string Content { get; set; } = string.Empty;
//    }

//    public class GigaChatRequest
//    {
//        [JsonPropertyName("model")]
//        public string Model { get; set; } = "GigaChat:latest";
//        [JsonPropertyName("messages")]
//        public List<GigaChatMessage>? Messages { get; set; }
//        [JsonPropertyName("temperature")]
//        public double Temperature { get; set; } = 0.5;
//        [JsonPropertyName("max_tokens")]
//        public int MaxTokens { get; set; } = 256;
//    }

//    public class GigaChatChoice
//    {
//        [JsonPropertyName("message")]
//        public GigaChatMessage? Message { get; set; }
//    }

//    public class GigaChatResponse
//    {
//        [JsonPropertyName("choices")]
//        public List<GigaChatChoice>? Choices { get; set; }
//    }
//}
