 using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TextToAI.Models;

namespace TextToAI.Services
{
    public class OpenAiService
    {
        private static readonly HttpClient _httpClient = new();
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public async Task<OpenAiResult> SendAsync(string text, AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                return OpenAiResult.Error("Please configure API key in Settings");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return OpenAiResult.Error("No text selected");
            }

            try
            {
                var prompt = config.Prompt.Replace("{text}", text);

                var requestBody = new
                {
                    model = config.Model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
                request.Content = content;

                using var response = await _httpClient.SendAsync(request);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => OpenAiResult.Error("Invalid API key"),
                        System.Net.HttpStatusCode.TooManyRequests => OpenAiResult.Error("Rate limited. Try again later."),
                        _ => OpenAiResult.Error($"API error: {response.StatusCode}")
                    };
                }

                var result = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);
                var message = result?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrWhiteSpace(message))
                {
                    return OpenAiResult.Error("No response received");
                }

                return OpenAiResult.Success(message);
            }
            catch (HttpRequestException)
            {
                return OpenAiResult.Error("Connection failed. Check internet.");
            }
            catch (TaskCanceledException)
            {
                return OpenAiResult.Error("Request timed out");
            }
            catch (Exception ex)
            {
                return OpenAiResult.Error($"Error: {ex.Message}");
            }
        }
    }

    public class OpenAiResult
    {
        public bool IsSuccess { get; private set; }
        public string? Content { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static OpenAiResult Success(string content) => new() { IsSuccess = true, Content = content };
        public static OpenAiResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    public class OpenAiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
