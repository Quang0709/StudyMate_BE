using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class Student
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? StudentIdCode { get; set; }

    public virtual ICollection<CourseRating> CourseRatings { get; set; } = new List<CourseRating>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
