using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private static readonly Dictionary<string, List<ConversationMessage>> _conversations = new();

    public GeminiAIService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiAI:ApiKey"] ?? throw new ArgumentNullException("Gemini API Key not configured");
        // Use Gemini 2.5 Flash - Latest model
        _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    }

    public async Task<(bool success, string message, ChatResponse? data)> ChatAsync(string userMessage, string? conversationId = null)
    {
        try
        {
            // Create or get conversation
            conversationId ??= Guid.NewGuid().ToString();
            
            if (!_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId] = new List<ConversationMessage>();
            }

            // Add user message to conversation history
            _conversations[conversationId].Add(new ConversationMessage
            {
                Role = "user",
                Content = userMessage
            });

            // Build context from conversation history
            var context = BuildConversationContext(_conversations[conversationId]);

            // Call Gemini API
            var response = await CallGeminiAPIAsync(context);

            if (string.IsNullOrEmpty(response))
            {
                return (false, "Failed to get response from AI", null);
            }

            // Add AI response to conversation history
            _conversations[conversationId].Add(new ConversationMessage
            {
                Role = "assistant",
                Content = response
            });

            // Limit conversation history to last 10 messages
            if (_conversations[conversationId].Count > 10)
            {
                _conversations[conversationId].RemoveRange(0, _conversations[conversationId].Count - 10);
            }

            var chatResponse = new ChatResponse
            {
                Response = response,
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow
            };

            return (true, "Chat response generated successfully", chatResponse);
        }
        catch (Exception ex)
        {
            return (false, $"Error in chat: {ex.Message}", null);
        }
    }

    private async Task<string?> CallGeminiAPIAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_apiUrl}?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API error: {error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);

            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling Gemini API: {ex.Message}", ex);
        }
    }

    private string BuildConversationContext(List<ConversationMessage> messages)
    {
        var context = new StringBuilder();
        context.AppendLine("You are a helpful AI tutor assistant for StudyMate platform. Help students with their learning.");
        context.AppendLine("Previous conversation:");
        
        foreach (var msg in messages)
        {
            if (msg.Role == "user")
            {
                context.AppendLine($"Student: {msg.Content}");
            }
            else
            {
                context.AppendLine($"AI Tutor: {msg.Content}");
            }
        }

        return context.ToString();
    }

    // Helper classes for JSON deserialization
    private class GeminiResponse
    {
        [JsonProperty("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonProperty("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonProperty("parts")]
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }

    private class ConversationMessage
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
