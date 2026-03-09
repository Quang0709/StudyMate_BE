# API Update Course - H??ng d?n s? d?ng

## Mô t?
API cho phép giáo viên c?p nh?t thông tin khóa h?c ?ã t?o, bao g?m:
- Thông tin c? b?n c?a khóa h?c (tiêu ??, mô t?, giá, danh m?c)
- Các section (ph?n h?c) và materials (tài li?u)
- Quizzes (bài ki?m tra) v?i questions và options
- Flashcards (th? ghi nh?)

## Endpoint
```
PUT /api/course/update/{id}
```

## Authorization
- **Role yêu c?u**: `teacher`
- **Header**: `Authorization: Bearer {token}`

## Quy t?c quan tr?ng

### 1. Quy?n ch?nh s?a
- Ch? giáo viên t?o khóa h?c m?i có quy?n ch?nh s?a
- Không th? ch?nh s?a khóa h?c ?ã b? xóa (soft deleted)

### 2. C?p nh?t items con (sections, quizzes, flashcards)
- **Items có `id`**: S? ???c C?P NH?T
- **Items không có `id`**: S? ???c T?O M?I
- **Items không có trong request**: S? b? XÓA

### 3. Gi?i h?n
- T?i ?a 50 câu h?i m?i quiz
- T?i ?a 50 flashcards m?i khóa h?c
- M?i câu h?i ph?i có ít nh?t 1 ?áp án ?úng

## Request Body Structure

```json
{
  "title": "string (required, max 255 chars)",
  "description": "string (optional, max 2000 chars)",
  "price": "decimal (optional, >= 0)",
  "categoryId": "int (optional)",
  "sections": [
    {
      "id": "int (optional - null ?? t?o m?i)",
      "title": "string (required, max 255 chars)",
      "description": "string (optional)",
      "orderIndex": "int (required)",
      "materials": [
        {
          "id": "int (optional - null ?? t?o m?i)",
          "title": "string (required, max 255 chars)",
          "description": "string (optional)",
          "materialType": "string (required: pdf|video|document|link|image|other)",
          "fileUrl": "string (required)",
          "fileName": "string (optional)",
          "fileSize": "long (optional)",
          "orderIndex": "int (required)"
        }
      ]
    }
  ],
  "quizzes": [
    {
      "id": "int (optional - null ?? t?o m?i)",
      "title": "string (required, max 255 chars)",
      "questions": [
        {
          "id": "int (optional - null ?? t?o m?i)",
          "questionText": "string (required)",
          "questionType": "string (required: multiple_choice|true_false)",
          "options": [
            {
              "id": "int (optional - null ?? t?o m?i)",
              "optionText": "string (required)",
              "isCorrect": "boolean (required)"
            }
          ]
        }
      ]
    }
  ],
  "flashcards": [
    {
      "id": "int (optional - null ?? t?o m?i)",
      "front": "string (required)",
      "back": "string (required)"
    }
  ]
}
```

## Ví d? Request

### 1. Update thông tin c? b?n + Thêm section m?i

```json
{
  "title": "Updated: Introduction to Programming",
  "description": "Updated description with new content",
  "price": 149.99,
  "categoryId": 2,
  "sections": [
    {
      "id": 1,
      "title": "Updated Section 1: Getting Started",
      "description": "Updated introduction",
      "orderIndex": 1,
      "materials": [
        {
          "id": 1,
          "title": "Updated Introduction PDF",
          "description": "Updated overview",
          "materialType": "pdf",
          "fileUrl": "https://example.com/files/updated-intro.pdf",
          "fileName": "updated-intro.pdf",
          "fileSize": 3048576,
          "orderIndex": 1
        },
        {
          "title": "New Video Tutorial",
          "materialType": "video",
          "fileUrl": "https://example.com/videos/new-tutorial.mp4",
          "fileName": "new-tutorial.mp4",
          "fileSize": 52428800,
          "orderIndex": 2
        }
      ]
    },
    {
      "title": "New Section 2: Advanced Topics",
      "description": "Brand new advanced section",
      "orderIndex": 2,
      "materials": []
    }
  ],
  "quizzes": [],
  "flashcards": []
}
```

### 2. Update quiz v?i câu h?i m?i

```json
{
  "title": "Programming Course",
  "description": "Learn programming",
  "price": 99.99,
  "quizzes": [
    {
      "id": 1,
      "title": "Updated Quiz 1: Basics",
      "questions": [
        {
          "id": 1,
          "questionText": "What is a variable? (Updated)",
          "questionType": "multiple_choice",
          "options": [
            {
              "id": 1,
              "optionText": "A storage location for data",
              "isCorrect": true
            },
            {
              "id": 2,
              "optionText": "A function",
              "isCorrect": false
            },
            {
              "optionText": "A new option - A class method",
              "isCorrect": false
            }
          ]
        },
        {
          "questionText": "New question: What is OOP?",
          "questionType": "multiple_choice",
          "options": [
            {
              "optionText": "Object-Oriented Programming",
              "isCorrect": true
            },
            {
              "optionText": "Operational Programming",
              "isCorrect": false
            }
          ]
        }
      ]
    }
  ],
  "sections": [],
  "flashcards": []
}
```

### 3. Update flashcards

```json
{
  "title": "Programming Course",
  "description": "Learn programming",
  "price": 99.99,
  "flashcards": [
    {
      "id": 1,
      "front": "Updated: What is OOP?",
      "back": "Updated: Object-Oriented Programming - A programming paradigm"
    },
    {
      "id": 2,
      "front": "What is a variable?",
      "back": "A named storage location"
    },
    {
      "front": "New: What is a function?",
      "back": "A reusable block of code"
    }
  ],
  "sections": [],
  "quizzes": []
}
```

### 4. Xóa t?t c? sections và quizzes

```json
{
  "title": "Minimal Course",
  "description": "Course with flashcards only",
  "price": 49.99,
  "sections": [],
  "quizzes": [],
  "flashcards": [
    {
      "front": "Term 1",
      "back": "Definition 1"
    }
  ]
}
```

## Response

### Success Response (200 OK)
```json
{
  "message": "Course updated successfully",
  "data": {
    "courseId": 1,
    "title": "Updated Course Title",
    "status": "pending",
    "sectionCount": 2,
    "quizCount": 1,
    "flashcardCount": 3
  }
}
```

### Error Responses

#### 400 Bad Request - Validation Error
```json
{
  "message": "Quiz 'Quiz 1' has more than 50 questions"
}
```

#### 400 Bad Request - No Permission
```json
{
  "message": "You don't have permission to edit this course"
}
```

#### 400 Bad Request - Course Not Found
```json
{
  "message": "Course not found"
}
```

#### 400 Bad Request - Deleted Course
```json
{
  "message": "Cannot edit a deleted course"
}
```

#### 401 Unauthorized
```json
{
  "message": "Invalid token"
}
```

## Các k?ch b?n s? d?ng th??ng g?p

### 1. Ch? update thông tin c? b?n
G?i request v?i sections, quizzes, flashcards gi?ng nh? d? li?u hi?n t?i (bao g?m id)

### 2. Thêm section m?i vào khóa h?c
- Gi? nguyên các sections c? (có id)
- Thêm section m?i (không có id)

### 3. Xóa m?t section
- Ch? g?i các sections mu?n gi? l?i (có id)
- Section không có trong request s? b? xóa

### 4. Thay th? hoàn toàn quizzes
- G?i quizzes m?i (không có id)
- Không g?i quizzes c?
- T?t c? quizzes c? s? b? xóa

### 5. Update n?i dung quiz hi?n có
- Gi? quiz id
- Update questions: gi? id cho questions c?, không có id cho questions m?i
- Update options t??ng t?

## L?u ý quan tr?ng

1. **Luôn l?y d? li?u hi?n t?i tr??c khi update** b?ng GET `/api/course/{id}`
2. **B?o toàn id** c?a các items mu?n gi? l?i
3. **Xem xét cascading deletes**: Khi xóa section, t?t c? materials trong section ?ó c?ng s? b? xóa
4. **Validate tr??c khi g?i**: ??m b?o ?áp ?ng các gi?i h?n (50 questions, 50 flashcards)
5. **Order matters**: S? d?ng orderIndex ?? s?p x?p sections và materials

## Testing v?i cURL

```bash
curl -X PUT "https://api.example.com/api/course/update/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Course",
    "description": "Updated description",
    "price": 149.99,
    "sections": [],
    "quizzes": [],
    "flashcards": []
  }'
```

## Workflow khuy?n ngh?

1. **GET** `/api/course/{id}` - L?y d? li?u hi?n t?i
2. **Modify** - Thay ??i d? li?u theo nhu c?u
3. **PUT** `/api/course/update/{id}` - G?i d? li?u ?ã s?a
4. **Verify** - Ki?m tra response ho?c GET l?i ?? xác nh?n

## Support
N?u g?p v?n ??, vui lòng ki?m tra:
- Token có h?p l? và có role "teacher"
- Course ID có t?n t?i
- B?n có ph?i là ng??i t?o course
- D? li?u g?i ?i có ?úng format
- Các gi?i h?n ?ã ???c tuân th?
