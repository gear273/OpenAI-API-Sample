using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program{

    // Function to call ChatGPT API
    static string CallGPT(string prompt){
        // OpenAI API key
        var apiKey = "YOUR_OPENAI_API_KEY";
        using var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // Set up the request params
        var requestBody = new{model = "gpt-4",messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            }};

        var json = JsonSerializer.Serialize(requestBody);
        var response = httpClient.PostAsync("chat/completions", new StringContent(json, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        
        // Return JSON of the response
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }

    // Function to process message
    static string ProcessMessage(string response){
        // Parse response from OpenAI API
        var jsonDocument = JsonDocument.Parse(response);

        // Extract the response
        var message = jsonDocument.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        return message;
    }
    
    // Cost evaluation
    static void CostEvaluation(string response){
        var jsonDocument = JsonDocument.Parse(response);

        // Fetch token amounts
        var promptTokens = jsonDocument.RootElement.GetProperty("usage").GetProperty("prompt_tokens").GetInt32();
        var completionTokens = jsonDocument.RootElement.GetProperty("usage").GetProperty("completion_tokens").GetInt32();

        // Display token amounts
        Console.WriteLine($"\n{"Prompt Tokens Used:",-35}{promptTokens:N0}");
        Console.WriteLine($"{"Completion Tokens Used:",-35}{completionTokens:N0}");
        Console.WriteLine($"{"Total Tokens Used:",-35}{promptTokens + completionTokens:N0}");

        // Calculate cost of GPT-3.5-turbo
        decimal gpt35TurboUsageCost = (promptTokens + completionTokens) * 0.0015m / 1000m;
        Console.WriteLine($"{"GPT-3.5-Turbo Usage Cost: ",-35}${gpt35TurboUsageCost:F6}");

        // Calculate cost of GPT-4
        decimal gpt4UsageCost = (promptTokens * 0.03m / 1000m) + (completionTokens * 0.06m / 1000m);
        Console.WriteLine($"{"GPT-4 Usage Cost: ",-35}${gpt4UsageCost:F6}\n");
    }

    // Main function
    static void Main(string[] args){
        string prompt = "Hello, ChatGPT!";
        string response = CallGPT(prompt);
        string message = ProcessMessage(response);

        Console.WriteLine("Response Message: " + message);
        CostEvaluation(response);
    }
}
