# Soft Delete for Course - Implementation Guide

## ?? Changes Summary

### ? Course Table Updates
Added 3 new columns to `courses` table:
- **`is_deleted`** (bit) - Boolean flag for soft delete (default: false)
- **`updated_at`** (datetime) - Last update timestamp
- **`deleted_at`** (datetime, nullable) - Deletion timestamp

## ?? Migration Details

### Migration Name
`AddSoftDeleteToCourse`

### Database Changes
```sql
-- Add columns to courses table
ALTER TABLE [courses] ADD [is_deleted] bit NOT NULL DEFAULT 0;
ALTER TABLE [courses] ADD [updated_at] datetime NULL;
ALTER TABLE [courses] ADD [deleted_at] datetime NULL;
```

## ?? Apply Migration

```bash
dotnet ef database update --project Repository --startup-project StudyMate_BE
```

## ?? Usage Examples

### 1. Soft Delete a Course

Create this method in `ICourseService`:
```csharp
Task<(bool Success, string Message)> SoftDeleteCourseAsync(int courseId, int userId);
```

Implementation in `CourseService`:
```csharp
public async Task<(bool Success, string Message)> SoftDeleteCourseAsync(int courseId, int userId)
{
    try
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
        {
            return (false, "Course not found");
        }

        // Verify ownership
        var teachers = await _unitOfWork.Teachers.GetAllAsync();
        var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
        if (teacher == null || course.TeacherId != teacher.Id)
        {
            return (false, "You don't have permission to delete this course");
        }

        // Soft delete
        course.IsDeleted = true;
        course.DeletedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, "Course deleted successfully");
    }
    catch (Exception ex)
    {
        return (false, $"Error deleting course: {ex.Message}");
    }
}
```

### 2. Restore a Deleted Course

```csharp
public async Task<(bool Success, string Message)> RestoreCourseAsync(int courseId, int userId)
{
    try
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
        {
            return (false, "Course not found");
        }

        // Verify ownership
        var teachers = await _unitOfWork.Teachers.GetAllAsync();
        var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
        if (teacher == null || course.TeacherId != teacher.Id)
        {
            return (false, "You don't have permission to restore this course");
        }

        // Restore
        course.IsDeleted = false;
        course.DeletedAt = null;
        course.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, "Course restored successfully");
    }
    catch (Exception ex)
    {
        return (false, $"Error restoring course: {ex.Message}");
    }
}
```

### 3. Get All Courses (Excluding Deleted)

```csharp
public async Task<IEnumerable<Course>> GetAllActiveCoursesAsync()
{
    var allCourses = await _unitOfWork.Courses.GetAllAsync();
    return allCourses.Where(c => !c.IsDeleted);
}
```

### 4. Get Deleted Courses (for Admin/Teacher)

```csharp
public async Task<IEnumerable<Course>> GetDeletedCoursesAsync(int teacherId)
{
    var allCourses = await _unitOfWork.Courses.GetAllAsync();
    return allCourses.Where(c => c.IsDeleted && c.TeacherId == teacherId);
}
```

### 5. Permanent Delete (Admin only)

```csharp
public async Task<(bool Success, string Message)> PermanentDeleteCourseAsync(int courseId)
{
    try
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
        {
            return (false, "Course not found");
        }

        // Delete related data first (due to foreign keys)
        // Sections, materials, quizzes, flashcards will be cascade deleted
        
        await _unitOfWork.Courses.DeleteAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course permanently deleted");
    }
    catch (Exception ex)
    {
        return (false, $"Error deleting course: {ex.Message}");
    }
}
```

## ?? Controller Examples

### Delete Course Endpoint

```csharp
/// <summary>
/// Soft delete a course (Teacher only)
/// </summary>
[HttpDelete("{courseId}")]
[Authorize(Roles = "teacher")]
public async Task<IActionResult> DeleteCourse(int courseId)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Unauthorized(new { message = "Invalid token" });
    }

    var (success, message) = await _courseService.SoftDeleteCourseAsync(courseId, userId);

    if (!success)
    {
        return BadRequest(new { message });
    }

    return Ok(new { message });
}
```

### Restore Course Endpoint

```csharp
/// <summary>
/// Restore a deleted course (Teacher only)
/// </summary>
[HttpPost("{courseId}/restore")]
[Authorize(Roles = "teacher")]
public async Task<IActionResult> RestoreCourse(int courseId)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Unauthorized(new { message = "Invalid token" });
    }

    var (success, message) = await _courseService.RestoreCourseAsync(courseId, userId);

    if (!success)
    {
        return BadRequest(new { message });
    }

    return Ok(new { message });
}
```

### Get Deleted Courses

```csharp
/// <summary>
/// Get all deleted courses for the logged-in teacher
/// </summary>
[HttpGet("deleted")]
[Authorize(Roles = "teacher")]
public async Task<IActionResult> GetDeletedCourses()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Unauthorized(new { message = "Invalid token" });
    }

    var teachers = await _unitOfWork.Teachers.GetAllAsync();
    var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
    if (teacher == null)
    {
        return NotFound(new { message = "Teacher not found" });
    }

    var deletedCourses = await _courseService.GetDeletedCoursesAsync(teacher.Id);

    return Ok(new
    {
        message = "Deleted courses retrieved successfully",
        data = deletedCourses
    });
}
```

## ?? API Testing

### Soft Delete a Course
```bash
DELETE /api/course/123
Authorization: Bearer {teacher_token}

Response:
{
  "message": "Course deleted successfully"
}
```

### Restore a Course
```bash
POST /api/course/123/restore
Authorization: Bearer {teacher_token}

Response:
{
  "message": "Course restored successfully"
}
```

### Get Deleted Courses
```bash
GET /api/course/deleted
Authorization: Bearer {teacher_token}

Response:
{
  "message": "Deleted courses retrieved successfully",
  "data": [
    {
      "id": 123,
      "title": "Deleted Course",
      "isDeleted": true,
      "deletedAt": "2024-01-29T10:30:00Z"
    }
  ]
}
```

## ?? Query Filter Examples

### Entity Framework Core Query Filter (Auto-exclude deleted)

Add this to `StudyMateContext.OnModelCreating`:

```csharp
// Global query filter - automatically exclude deleted courses
modelBuilder.Entity<Course>()
    .HasQueryFilter(c => !c.IsDeleted);
```

**Note:** To include deleted courses in specific queries, use `IgnoreQueryFilters()`:

```csharp
// Get all courses including deleted
var allCourses = await _context.Courses
    .IgnoreQueryFilters()
    .ToListAsync();
```

## ?? Important Notes

1. **Cascade Delete**: When soft deleting a course, consider if you need to also soft delete related:
   - Course sections
   - Section materials  
   - Quizzes and questions
   - Flashcards

2. **Enrollments**: Courses with active enrollments should show a warning before deletion

3. **Search/Listing**: Always filter out `IsDeleted = true` courses in public listings

4. **Permissions**:
   - Teachers can only soft delete their own courses
   - Only Admin can permanently delete courses
   - Students should never see deleted courses

## ?? Best Practices

1. **Always set `UpdatedAt`** when modifying a course
2. **Set `DeletedAt`** when soft deleting
3. **Use query filters** to automatically exclude deleted records
4. **Keep audit trail** - never hard delete unless absolutely necessary
5. **Add indexes** on `is_deleted` for better query performance:

```sql
CREATE INDEX IX_courses_is_deleted ON courses(is_deleted);
```

## ?? Database Schema

### Courses Table (After Migration)
```sql
CREATE TABLE courses (
    id INT PRIMARY KEY IDENTITY,
    teacher_id INT NOT NULL,
    category_id INT NULL,
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    price DECIMAL(10,2) DEFAULT 0,
    status VARCHAR(20) DEFAULT 'pending',
    reject_reason NVARCHAR(MAX) NULL,
    is_deleted BIT NOT NULL DEFAULT 0,        -- ? NEW
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME NULL,                  -- ? NEW
    deleted_at DATETIME NULL,                  -- ? NEW
    FOREIGN KEY (teacher_id) REFERENCES teachers(id),
    FOREIGN KEY (category_id) REFERENCES categories(id)
);
```

---

**Migration Created:** `AddSoftDeleteToCourse`

**Status:** ? Ready to apply

Run `dotnet ef database update --project Repository --startup-project StudyMate_BE` to apply!
