using System;
using System.Collections.Generic;

namespace Repository.Model;

public partial class UserSubscription
{
    public int Id { get; set; }

    public int? StudentId { get; set; }  // Nullable - for teacher subscriptions

    public int? TeacherId { get; set; }  // Nullable - for student subscriptions

    public int PlanId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }  // Để kiểm tra subscription còn hiệu lực không

    public virtual PremiumPlan Plan { get; set; } = null!;

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
