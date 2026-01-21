using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class UserSubscription
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int PlanId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public virtual PremiumPlan Plan { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
