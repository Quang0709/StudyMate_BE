# User Profile Update - Migration Guide

## ?? Changes Summary

### ? User Table Updates
Added 4 new columns to `users` table (all nullable):
- **`birthday`** (date) - User's date of birth
- **`bio`** (nvarchar(max)) - User biography/about me
- **`avatar_url`** (nvarchar(500)) - Profile picture URL
- **`address`** (nvarchar(500)) - User address

### ? Teacher Table Updates
Removed:
- **`bio`** column (moved to `users` table)

## ?? Migration Details

### Migration Name
`UpdateUserProfileAndRemoveTeacherBio`

### Database Changes
```sql
-- Add columns to users table
ALTER TABLE [users] ADD [birthday] date NULL;
ALTER TABLE [users] ADD [bio] nvarchar(max) NULL;
ALTER TABLE [users] ADD [avatar_url] nvarchar(500) NULL;
ALTER TABLE [users] ADD [address] nvarchar(500) NULL;

-- Remove bio column from teachers table
ALTER TABLE [teachers] DROP COLUMN [bio];
```

## ?? Apply Migration

Run one of the following commands:

### Option 1: Using EF Core CLI
```bash
dotnet ef database update --project Repository --startup-project StudyMate_BE
```

### Option 2: Auto Migration on Startup
Just run the application - migrations will be applied automatically:
```bash
dotnet run --project StudyMate_BE
```

## ?? API Usage

### Edit Profile Endpoint

**Endpoint:** `PUT /api/profile/edit`

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "birthday": "1990-01-15",
  "bio": "Software developer passionate about education and technology",
  "avatarUrl": "https://example.com/avatars/john.jpg",
  "address": "123 Main Street, City, Country"
}
```

**Note:** All fields are optional. Only send fields you want to update.

### Sample Requests

#### Update only full name
```json
{
  "fullName": "John Smith"
}
```

#### Update birthday and bio
```json
{
  "birthday": "1992-05-20",
  "bio": "Experienced teacher with 5 years in education"
}
```

#### Update avatar only
```json
{
  "avatarUrl": "https://cdn.example.com/users/avatar123.png"
}
```

## ?? Response

### Success (200 OK)
```json
{
  "message": "Profile updated successfully"
}
```

### Error (400 Bad Request)
```json
{
  "message": "Invalid email format"
}
```

### Error (404 Not Found)
```json
{
  "message": "User not found"
}
```

## ?? Validation Rules

- **FullName**: Max 255 characters
- **Email**: Valid email format, max 255 characters
- **Birthday**: Valid date format (YYYY-MM-DD)
- **Bio**: Max 2000 characters
- **AvatarUrl**: Max 500 characters
- **Address**: Max 500 characters

## ?? Get User Profile

**Endpoint:** `GET /api/auth/me`

**Authorization:** Required (Bearer Token)

**Response:**
```json
{
  "message": "User retrieved successfully",
  "data": {
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "role": "teacher",
    "birthday": "1990-01-15",
    "bio": "Software developer passionate about education",
    "avatarUrl": "https://example.com/avatars/john.jpg",
    "address": "123 Main Street, City, Country",
    "isBanned": false,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

## ?? Migration Benefits

### For All Users
- ? **Birthday field** - Can display age, send birthday wishes
- ? **Bio field** - Users can describe themselves
- ? **Avatar URL** - Profile pictures for better UX
- ? **Address** - Location information for local courses

### For Teachers
- ? Bio is now in the `users` table, accessible for both students and teachers
- ? Simplified data structure
- ? No need to join `teachers` table just to get bio

### For Students
- ? Students can now also have bios
- ? Unified profile structure for all user types

## ?? Important Notes

1. **Existing teacher bio data will be lost** during migration. If you need to preserve it:
   ```sql
   -- Before running migration, backup teacher bios
   SELECT t.id, t.user_id, t.bio, u.email
   FROM teachers t
   JOIN users u ON t.user_id = u.id
   WHERE t.bio IS NOT NULL;
   
   -- After migration, manually update users table if needed
   UPDATE users SET bio = 'teacher bio here' WHERE id = user_id;
   ```

2. **All new fields are nullable** - existing users will have NULL values initially

3. **Frontend should handle NULL values** gracefully

## ?? Testing

### Test Edit Profile
```bash
# 1. Login as user
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "Password123"
}

# 2. Edit profile
PUT /api/profile/edit
Authorization: Bearer {token}
{
  "birthday": "1995-03-20",
  "bio": "Hello world!",
  "avatarUrl": "https://i.pravatar.cc/300",
  "address": "Hanoi, Vietnam"
}

# 3. Get profile
GET /api/auth/me
Authorization: Bearer {token}
```

## ?? Database Schema

### Users Table (After Migration)
```sql
CREATE TABLE users (
    id INT PRIMARY KEY IDENTITY,
    full_name NVARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL,
    is_banned BIT DEFAULT 0,
    birthday DATE NULL,                    -- ? NEW
    bio NVARCHAR(MAX) NULL,                -- ? NEW
    avatar_url NVARCHAR(500) NULL,         -- ? NEW
    address NVARCHAR(500) NULL,            -- ? NEW
    created_at DATETIME DEFAULT GETDATE()
);
```

### Teachers Table (After Migration)
```sql
CREATE TABLE teachers (
    id INT PRIMARY KEY IDENTITY,
    user_id INT NOT NULL,
    teacher_id_code VARCHAR(20) UNIQUE,
    -- bio REMOVED ?
    FOREIGN KEY (user_id) REFERENCES users(id)
);
```

---

**Migration Created:** `UpdateUserProfileAndRemoveTeacherBio`

**Status:** ? Ready to apply

Run `dotnet ef database update --project Repository --startup-project StudyMate_BE` to apply!
