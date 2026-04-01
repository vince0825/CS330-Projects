using System.Text;
using System.Text.Json;

namespace Spring2026_Project3_vcmadu.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API key not set.");
        }

        public async Task<List<string>> GenerateMovieReviewsAsync(string movieTitle)
        {
            var prompt = $"Generate exactly 5 distinct movie reviews for \"{movieTitle}\". " +
                         "Number each review 1 through 5. Each review should be 2-3 sentences. " +
                         "Format:\n1. [review]\n2. [review]\n3. [review]\n4. [review]\n5. [review]";

            var raw = await CallOpenAIAsync(prompt);
            return ParseNumberedList(raw, 5);
        }

        public async Task<List<string>> GenerateActorTweetsAsync(string actorName)
        {
            var prompt = $"Generate exactly 10 tweets about the actor \"{actorName}\". " +
                         "Number each tweet 1 through 10. Each tweet should be under 280 characters. " +
                         "Format:\n1. [tweet]\n2. [tweet]\n...";

            var raw = await CallOpenAIAsync(prompt);
            return ParseNumberedList(raw, 10);
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 1000
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"OpenAI error {response.StatusCode}: {error}");
                    return string.Empty;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI exception: {ex.Message}");
                return string.Empty;
            }
        }

        private List<string> ParseNumberedList(string raw, int count)
        {
            var results = new List<string>();
            var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // Match lines starting with "1." "2." etc.
                if (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && trimmed[1] == '.')
                {
                    results.Add(trimmed.Substring(2).Trim());
                }
                if (results.Count == count) break;
            }

            return results;
        }
    }
}