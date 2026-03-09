using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class PremiumPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? DurationDays { get; set; }

    public decimal? Price { get; set; }

    public string? TargetRole { get; set; }  // "student" or "teacher"

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
