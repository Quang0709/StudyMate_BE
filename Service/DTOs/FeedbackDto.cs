using System;

namespace Service.DTOs;

public class CreateFeedbackDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateFeedbackDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class FeedbackResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FeedbackAverageDto
{
    public double AverageRating { get; set; }
}
