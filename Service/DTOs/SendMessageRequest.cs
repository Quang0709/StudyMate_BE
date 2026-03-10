using System.ComponentModel.DataAnnotations;

public class SendMessageRequest
{
    [Required]
    public int ReceiverId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 2000 characters")]
    public string Content { get; set; } = null!;
}