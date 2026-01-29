using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class CourseSection
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<SectionMaterial> SectionMaterials { get; set; } = new List<SectionMaterial>();
}
