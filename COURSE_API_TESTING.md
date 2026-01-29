# API Create Course - Testing Guide

## ?? Prerequisites

- User ph?i có **role = "teacher"**
- User ?ã ??ng nh?p và có **JWT token h?p l?**
- Token ph?i ???c thêm vào header: `Authorization: Bearer {token}`

## ?? Authorization

API này yêu c?u:
- ? **Authentication**: Bearer Token (JWT)
- ? **Authorization**: Role = "teacher"

Ch? có user v?i role **teacher** m?i có th? t?o course.

## ?? API Endpoint

```
POST /api/course/create
Content-Type: application/json
Authorization: Bearer {your_jwt_token}
```

## ?? Request Body

### Minimal Example (Ch? thông tin c? b?n)

```json
{
  "title": "Introduction to C# Programming",
  "description": "Learn C# from scratch",
  "price": 49.99,
  "categoryId": 1
}
```

### Full Example (V?i Sections, Quizzes, Flashcards)

```json
{
  "title": "Complete Web Development Course",
  "description": "Master HTML, CSS, JavaScript, and React",
  "price": 199.99,
  "categoryId": 2,
  "sections": [
    {
      "title": "Section 1: HTML Basics",
      "description": "Learn HTML fundamentals",
      "orderIndex": 1,
      "materials": [
        {
          "title": "HTML Introduction PDF",
          "description": "Complete guide to HTML tags",
          "materialType": "pdf",
          "fileUrl": "https://example.com/files/html-intro.pdf",
          "fileName": "html-intro.pdf",
          "fileSize": 2048576,
          "orderIndex": 1
        },
        {
          "title": "HTML Video Tutorial",
          "materialType": "video",
          "fileUrl": "https://youtube.com/watch?v=example",
          "orderIndex": 2
        }
      ]
    },
    {
      "title": "Section 2: CSS Fundamentals",
      "description": "Style your web pages",
      "orderIndex": 2,
      "materials": [
        {
          "title": "CSS Basics",
          "materialType": "document",
          "fileUrl": "https://example.com/css-basics.docx",
          "orderIndex": 1
        }
      ]
    }
  ],
  "quizzes": [
    {
      "title": "HTML Quiz",
      "questions": [
        {
          "questionText": "What does HTML stand for?",
          "questionType": "multiple_choice",
          "options": [
            {
              "optionText": "Hyper Text Markup Language",
              "isCorrect": true
            },
            {
              "optionText": "High Tech Modern Language",
              "isCorrect": false
            },
            {
              "optionText": "Home Tool Markup Language",
              "isCorrect": false
            },
            {
              "optionText": "Hyperlinks and Text Markup Language",
              "isCorrect": false
            }
          ]
        },
        {
          "questionText": "Is HTML a programming language?",
          "questionType": "true_false",
          "options": [
            {
              "optionText": "True",
              "isCorrect": false
            },
            {
              "optionText": "False",
              "isCorrect": true
            }
          ]
        }
      ]
    }
  ],
  "flashcards": [
    {
      "front": "What is CSS?",
      "back": "Cascading Style Sheets - used to style HTML elements"
    },
    {
      "front": "What is JavaScript?",
      "back": "A programming language for web interactivity"
    },
    {
      "front": "What is React?",
      "back": "A JavaScript library for building user interfaces"
    }
  ]
}
```

## ?? Response

### Success Response (201 Created)

```json
{
  "message": "Course created successfully",
  "data": {
    "courseId": 1,
    "title": "Complete Web Development Course",
    "status": "pending",
    "sectionCount": 2,
    "quizCount": 1,
    "flashcardCount": 3
  }
}
```

### Error Responses

#### 401 Unauthorized (No token or invalid token)

```json
{
  "message": "Invalid token"
}
```

#### 403 Forbidden (Not a teacher)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

#### 400 Bad Request (Validation error)

```json
{
  "message": "Quiz 'HTML Quiz' has more than 50 questions"
}
```

```json
{
  "message": "Course cannot have more than 50 flashcards"
}
```

```json
{
  "message": "Question 'What is HTML?' must have at least one correct answer"
}
```

```json
{
  "message": "Category with ID 999 not found"
}
```

## ?? Validation Rules

### Course
- ? **Title**: Required, max 255 characters
- ? **Description**: Optional, max 2000 characters
- ? **Price**: Optional, must be >= 0
- ? **CategoryId**: Optional, must be a valid category ID from database

### Sections
- ? Unlimited sections per course
- ? **Title**: Required, max 255 characters
- ? **OrderIndex**: Required

### Materials
- ? Unlimited materials per section
- ? **MaterialType**: Must be one of: `pdf`, `video`, `document`, `link`, `image`, `other`
- ? **FileUrl**: Required

### Quizzes
- ? Unlimited quizzes per course
- ? **Max 50 questions per quiz**
- ? Each question must have at least 1 correct answer

### Quiz Questions
- ? **QuestionType**: Must be `multiple_choice` or `true_false`
- ? **Options**: Min 2, Max 6 options per question
- ? At least ONE option must have `isCorrect: true`

### Flashcards
- ? **Maximum 50 flashcards per course**
- ? **Front**: Required
- ? **Back**: Required

## ?? Testing Steps

### 1. Register as Teacher

```bash
POST /api/auth/register
{
  "fullName": "John Teacher",
  "email": "teacher@example.com",
  "password": "Teacher@123",
  "role": "teacher"
}
```

### 2. Verify Email

Click verification link in email or use:
```
GET /api/auth/verify-email?token={token_from_email}
```

### 3. Login

```bash
POST /api/auth/login
{
  "email": "teacher@example.com",
  "password": "Teacher@123"
}
```

**Save the `accessToken` from response!**

### 4. Create Course

```bash
POST /api/course/create
Authorization: Bearer {accessToken_from_step_3}
Content-Type: application/json

{
  "title": "My First Course",
  "description": "This is a test course",
  "price": 99.99,
  "categoryId": 1
}
```

## ?? Testing with Swagger

1. Run the application: `dotnet run --project StudyMate_BE`
2. Open Swagger: `https://localhost:5001` or `http://localhost:5000`
3. Click **Authorize** button (??)
4. Enter: `Bearer {your_access_token}`
5. Click **Authorize**
6. Test the `/api/course/create` endpoint

## ??? Testing with Postman

1. Create new request
2. Method: `POST`
3. URL: `https://localhost:5001/api/course/create`
4. Headers:
   - `Authorization: Bearer {your_token}`
   - `Content-Type: application/json`
5. Body: Select `raw` > `JSON` and paste request body
6. Click **Send**

## ?? Common Issues

### Issue 1: "Invalid token"
**Solution**: Make sure you're using a valid JWT token from login response

### Issue 2: 403 Forbidden
**Solution**: User must have role = "teacher"

### Issue 3: "Teacher profile not found for this user"
**Solution**: After registering with role="teacher", the system should auto-create a teacher profile. Check the `teachers` table.

### Issue 4: Validation errors
**Solution**: Check request body matches the validation rules above

## ?? Database Structure

After creating a course, data is stored in:

- ? `categories` - Course categories (pre-populated)
- ? `courses` - Course information (includes `category_id` foreign key)
- ? `course_sections` - Course sections/modules
- ? `section_materials` - Materials for each section
- ? `quizzes` - Quiz information
- ? `quiz_questions` - Questions for each quiz
- ? `quiz_options` - Answer options for each question
- ? `flashcards` - Flashcards for the course

## ?? Insert Sample Categories

Before testing, insert some categories into database:

```sql
-- Insert sample categories
INSERT INTO categories (name, description) VALUES 
('Programming', 'Programming and software development courses'),
('Web Development', 'Frontend and backend web development'),
('Mobile Development', 'iOS and Android app development'),
('Data Science', 'Data analysis, machine learning, AI'),
('Business', 'Business management and entrepreneurship'),
('Design', 'UI/UX design and graphic design'),
('Marketing', 'Digital marketing and SEO');
```

Or using Swagger/Postman, you can create an admin endpoint to manage categories later.

## ?? Course Status

New courses are created with **status = "pending"**

This allows for admin review before publishing.

---

**Happy Testing! ??**
