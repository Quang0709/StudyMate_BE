using System;
using System.ComponentModel.DataAnnotations;

namespace Service.DTOs;

public class EditProfileDto
{
    [StringLength(255)]
    public string? FullName { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    public DateTime? Birthday { get; set; }

    [StringLength(2000)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}