# Get My Courses API - Teacher Guide

## ?? Overview

API cho phép **teacher** xem t?t c? khóa h?c **?ang ho?t ??ng** (không bao g?m courses ?ã xóa m?m) mà mình ?ã t?o v?i ??y ?? thông tin chi ti?t.

## ?? Authorization

- ? **Required**: Bearer Token (JWT)
- ? **Role**: `teacher`

## ?? API Endpoints

### 1. Get All My Active Courses

**Endpoint:** `GET /api/course/my-courses`

**Note:** API này **LUÔN L?C B?** các courses ?ã xóa m?m (`isDeleted = true`). Ch? tr? v? courses ?ang ho?t ??ng.

**Headers:**
```
Authorization: Bearer {your_jwt_token}
```

#### Example Request

```bash
GET https://localhost:7259/api/course/my-courses
Authorization: Bearer {token}
```

#### Success Response (200 OK)

```json
{
  "message": "Retrieved 3 active course(s) successfully",
  "count": 3,
  "data": [
    {
      "id": 1,
      "title": "Complete Web Development Course",
      "description": "Learn HTML, CSS, JavaScript, React",
      "price": 299000,
      "status": "approved",
      "rejectReason": null,
      "isDeleted": false,
      "categoryId": 2,
      "categoryName": "Web Development",
      "createdAt": "2024-01-29T10:30:00Z",
      "updatedAt": null,
      "deletedAt": null,
      "totalSections": 3,
      "totalMaterials": 8,
      "totalQuizzes": 2,
      "totalQuestions": 15,
      "totalFlashcards": 10,
      "totalEnrollments": 25,
      "sections": [...],
      "quizzes": [...],
      "flashcards": [...]
    }
  ]
}
```

**Important:** T?t c? courses trong response ??u có `"isDeleted": false`. Courses ?ã xóa m?m s? **KHÔNG** xu?t hi?n trong danh sách.

---

**Happy Coding! ??**
