using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace EventManager.Controllers
{
    [Authorize]
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAIApiKey;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _openAIApiKey = configuration["OpenAI:ApiKey"];
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt cannot be empty");

            var client = _httpClientFactory.CreateClient();

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = request.Prompt }
                }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            httpRequest.Headers.Add("Authorization", $"Bearer {_openAIApiKey}");
            httpRequest.Content = requestContent;

            var response = await client.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest($"API error: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { reply = content });
        }

        public class PromptRequest
        {
            public string Prompt { get; set; }
        }
    }
}
