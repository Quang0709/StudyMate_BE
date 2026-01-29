using System;

namespace Repository.Model;

public partial class SectionMaterial
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string MaterialType { get; set; } = null!; // "pdf", "video", "document", "link", etc.

    public string FileUrl { get; set; } = null!; // URL or path to the file

    public string? FileName { get; set; }

    public long? FileSize { get; set; }

    public int OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual CourseSection Section { get; set; } = null!;
}
