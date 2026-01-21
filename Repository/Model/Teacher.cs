using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Teacher
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? TeacherIdCode { get; set; }

    public string? Bio { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual User User { get; set; } = null!;
}
