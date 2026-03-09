using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class ChatRequest
{
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; set; } = null!;
    
    public string? ConversationId { get; set; }  // Optional: for maintaining conversation context
}

public class ChatResponse
{
    public string Response { get; set; } = null!;
    public string ConversationId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
