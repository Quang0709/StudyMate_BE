namespace Repository.Model;

/// <summary>
/// Course status enumeration
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Course is in draft mode, not submitted for review
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Course submitted and waiting for admin approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Course approved by admin, ready to be published
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Course is live and available to students
    /// </summary>
    Published = 3,

    /// <summary>
    /// Course rejected by admin
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Course archived/soft deleted (not visible to users)
    /// </summary>
    Archived = 5
}
