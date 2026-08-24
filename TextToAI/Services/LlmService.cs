using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TextToAI.Models;

namespace TextToAI.Services
{
    public class LlmService
    {
        private static readonly HttpClient _httpClient = new();

        /// <param name="promptTemplate">The triggering action's prompt, with {text} as the placeholder.</param>
        public async Task<LlmResult> SendAsync(string text, AppConfig config, string promptTemplate)
        {
            var provider = ProviderCatalog.Get(config.Provider);

            if (string.IsNullOrWhiteSpace(config.ActiveApiKey))
            {
                return LlmResult.Error($"Please configure your {provider.DisplayName} API key in Settings");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return LlmResult.Error("No text selected");
            }

            try
            {
                var prompt = string.IsNullOrWhiteSpace(promptTemplate)
                    ? text
                    : promptTemplate.Replace("{text}", text);

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

                using var request = new HttpRequestMessage(HttpMethod.Post, provider.ChatCompletionsUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ActiveApiKey);

                if (config.Provider == LlmProvider.OpenRouter)
                {
                    // Optional attribution headers - they identify the app on OpenRouter.
                    request.Headers.Add("HTTP-Referer", "https://github.com/romanmaksymiv/TextToAI");
                    request.Headers.Add("X-Title", "TextToAI");
                }

                request.Content = content;

                using var response = await _httpClient.SendAsync(request);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return LlmResult.Error(DescribeError(response.StatusCode, responseJson, config, provider));
                }

                var result = JsonSerializer.Deserialize<LlmResponse>(responseJson);
                var message = result?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrWhiteSpace(message))
                {
                    return LlmResult.Error("No response received");
                }

                return LlmResult.Success(message);
            }
            catch (HttpRequestException)
            {
                return LlmResult.Error("Connection failed. Check internet.");
            }
            catch (TaskCanceledException)
            {
                return LlmResult.Error("Request timed out");
            }
            catch (Exception ex)
            {
                return LlmResult.Error($"Error: {ex.Message}");
            }
        }

        private static string DescribeError(
            System.Net.HttpStatusCode statusCode,
            string responseJson,
            AppConfig config,
            ProviderInfo provider)
        {
            return statusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => $"Invalid {provider.DisplayName} API key",
                System.Net.HttpStatusCode.PaymentRequired => "Insufficient credits",
                System.Net.HttpStatusCode.NotFound => $"Model not found: {config.Model}",
                System.Net.HttpStatusCode.TooManyRequests => "Rate limited. Try again later.",
                _ => ExtractApiMessage(responseJson) ?? $"API error: {statusCode}"
            };
        }

        /// <summary>
        /// Both providers return {"error":{"message":"..."}}. OpenRouter's messages are
        /// usually the most useful thing available (unsupported model, no provider, etc.).
        /// </summary>
        private static string? ExtractApiMessage(string responseJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<ApiErrorBody>(responseJson);
                var message = body?.Error?.Message;
                return string.IsNullOrWhiteSpace(message) ? null : message;
            }
            catch
            {
                return null;
            }
        }
    }

    public class LlmResult
    {
        public bool IsSuccess { get; private set; }
        public string? Content { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static LlmResult Success(string content) => new() { IsSuccess = true, Content = content };
        public static LlmResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    public class LlmResponse
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

    public class ApiErrorBody
    {
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public ApiError? Error { get; set; }
    }

    public class ApiError
    {
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
