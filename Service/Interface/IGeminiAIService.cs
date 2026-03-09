using Service.DTOs;
using System.Threading.Tasks;

namespace Service.Interface;

public interface IGeminiAIService
{
    Task<(bool success, string message, ChatResponse? data)> ChatAsync(string userMessage, string? conversationId = null);
}
