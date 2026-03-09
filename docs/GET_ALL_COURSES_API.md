# API Get All Courses - H??ng d?n s? d?ng

## Mô t?
API công khai cho phép **M?I NG??I** (không c?n ??ng nh?p) xem danh sách t?t c? các khóa h?c có phí (paid courses).

## Endpoint
```
GET /api/course/all
```

## Authorization
- **Không yêu c?u authentication** - Endpoint công khai
- Ai c?ng có th? truy c?p

## ?i?u ki?n l?c
API ch? tr? v? các khóa h?c th?a mãn:
- ? Status = "paid" (khóa h?c có phí/tr? phí)
- ? IsDeleted = false (ch?a b? xóa)
- ? Các khóa h?c có status khác ("pending", "approved", "rejected") s? KHÔNG ???c hi?n th?
- ? Các khóa h?c ?ã b? soft delete s? KHÔNG ???c hi?n th?

## Response Structure

### Success Response (200 OK)
```json
{
  "message": "Retrieved X course(s) successfully",
  "count": 2,
  "data": [
    {
      "id": 1,
      "title": "Introduction to Programming",
      "description": "Learn the basics of programming with C#",
      "price": 99.99,
      "status": "paid",
      "rejectReason": null,
      "isDeleted": false,
      "categoryId": 1,
      "categoryName": "Programming",
      
      // Teacher Information
      "teacherName": "John Doe",
      "teacherAvatar": "https://example.com/avatars/john.jpg",
      
      // Rating Information
      "averageRating": 4.5,
      "totalRatings": 125,
      
      // Timestamps
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z",
      "deletedAt": null,
      
      // Statistics
      "totalSections": 5,
      "totalMaterials": 15,
      "totalQuizzes": 3,
      "totalQuestions": 25,
      "totalFlashcards": 50,
      "totalEnrollments": 350,
      
      // Full Content
      "sections": [
        {
          "id": 1,
          "title": "Section 1: Getting Started",
          "description": "Introduction to programming concepts",
          "orderIndex": 1,
          "createdAt": "2024-01-15T10:35:00Z",
          "materials": [
            {
              "id": 1,
              "title": "Course Overview PDF",
              "description": "Comprehensive course overview",
              "materialType": "pdf",
              "fileUrl": "https://example.com/files/overview.pdf",
              "fileName": "overview.pdf",
              "fileSize": 2048576,
              "orderIndex": 1
            },
            {
              "id": 2,
              "title": "Introduction Video",
              "description": "Welcome video from instructor",
              "materialType": "video",
              "fileUrl": "https://example.com/videos/intro.mp4",
              "fileName": "intro.mp4",
              "fileSize": 45678912,
              "orderIndex": 2
            }
          ]
        }
      ],
      "quizzes": [
        {
          "id": 1,
          "title": "Quiz 1: Basics",
          "questions": [
            {
              "id": 1,
              "questionText": "What is a variable?",
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
                  "id": 3,
                  "optionText": "A loop",
                  "isCorrect": false
                }
              ]
            }
          ]
        }
      ],
      "flashcards": [
        {
          "id": 1,
          "front": "What is OOP?",
          "back": "Object-Oriented Programming"
        },
        {
          "id": 2,
          "front": "What is a variable?",
          "back": "A named storage location in memory"
        }
      ]
    }
  ]
}
```

### Error Response (400 Bad Request)
```json
{
  "message": "Error retrieving courses: [error details]"
}
```

## Các tr??ng d? li?u quan tr?ng

### Thông tin c? b?n
| Field | Type | Description |
|-------|------|-------------|
| `id` | int | ID c?a khóa h?c |
| `title` | string | Tiêu ?? khóa h?c |
| `description` | string | Mô t? chi ti?t |
| `price` | decimal | Giá khóa h?c (VND) |
| `status` | string | Luôn là "paid" |
| `categoryId` | int? | ID c?a category |
| `categoryName` | string? | Tên category |

### Thông tin giáo viên
| Field | Type | Description |
|-------|------|-------------|
| `teacherName` | string? | Tên ??y ?? c?a giáo viên |
| `teacherAvatar` | string? | URL avatar c?a giáo viên |

### Thông tin ?ánh giá
| Field | Type | Description |
|-------|------|-------------|
| `averageRating` | double | ?i?m ?ánh giá trung bình (0-5) |
| `totalRatings` | int | T?ng s? l??t ?ánh giá |

### Th?ng kê
| Field | Type | Description |
|-------|------|-------------|
| `totalSections` | int | S? l??ng sections |
| `totalMaterials` | int | T?ng s? tài li?u trong t?t c? sections |
| `totalQuizzes` | int | S? l??ng quizzes |
| `totalQuestions` | int | T?ng s? câu h?i trong t?t c? quizzes |
| `totalFlashcards` | int | S? l??ng flashcards |
| `totalEnrollments` | int | S? h?c viên ?ã ??ng ký |

### N?i dung khóa h?c
- **Sections**: Danh sách các ph?n h?c, m?i section có:
  - Thông tin section
  - Danh sách materials (tài li?u) ???c s?p x?p theo `orderIndex`

- **Quizzes**: Danh sách bài ki?m tra, m?i quiz có:
  - Thông tin quiz
  - Danh sách questions v?i options
  - **L?u ý**: `isCorrect` c?ng ???c tr? v? (?? hi?n th? sau khi làm quiz)

- **Flashcards**: Danh sách th? ghi nh? v?i front (câu h?i) và back (câu tr? l?i)

## Ví d? s? d?ng

### 1. L?y t?t c? khóa h?c (JavaScript/Fetch)
```javascript
fetch('https://api.example.com/api/course/all')
  .then(response => response.json())
  .then(data => {
    console.log(`Found ${data.count} courses`);
    data.data.forEach(course => {
      console.log(`${course.title} - ${course.price} VND`);
      console.log(`Teacher: ${course.teacherName}`);
      console.log(`Rating: ${course.averageRating}/5 (${course.totalRatings} reviews)`);
    });
  });
```

### 2. L?y t?t c? khóa h?c (cURL)
```bash
curl -X GET "https://api.example.com/api/course/all" \
  -H "Content-Type: application/json"
```

### 3. L?y t?t c? khóa h?c (C#)
```csharp
using System.Net.Http;
using System.Text.Json;

var client = new HttpClient();
var response = await client.GetAsync("https://api.example.com/api/course/all");
var content = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<CourseListResponse>(content);

Console.WriteLine($"Found {result.Count} courses");
foreach (var course in result.Data)
{
    Console.WriteLine($"{course.Title} - {course.Price} VND");
    Console.WriteLine($"Teacher: {course.TeacherName}");
    Console.WriteLine($"Rating: {course.AverageRating}/5");
}
```

### 4. L?y t?t c? khóa h?c (Python)
```python
import requests

response = requests.get('https://api.example.com/api/course/all')
data = response.json()

print(f"Found {data['count']} courses")
for course in data['data']:
    print(f"{course['title']} - {course['price']} VND")
    print(f"Teacher: {course['teacherName']}")
    print(f"Rating: {course['averageRating']}/5 ({course['totalRatings']} reviews)")
```

## Use Cases

### 1. Hi?n th? danh sách khóa h?c trên trang ch?
```javascript
async function loadCourses() {
  const response = await fetch('/api/course/all');
  const { data } = await response.json();
  
  const courseList = document.getElementById('course-list');
  courseList.innerHTML = data.map(course => `
    <div class="course-card">
      <h3>${course.title}</h3>
      <p>${course.description}</p>
      <div class="course-meta">
        <span class="price">${course.price} VND</span>
        <span class="rating">? ${course.averageRating.toFixed(1)} (${course.totalRatings})</span>
      </div>
      <div class="teacher">
        <img src="${course.teacherAvatar}" alt="${course.teacherName}">
        <span>${course.teacherName}</span>
      </div>
      <div class="stats">
        <span>?? ${course.totalSections} sections</span>
        <span>?? ${course.totalQuizzes} quizzes</span>
        <span>?? ${course.totalEnrollments} students</span>
      </div>
    </div>
  `).join('');
}
```

### 2. Tìm ki?m khóa h?c theo t? khóa
```javascript
async function searchCourses(keyword) {
  const response = await fetch('/api/course/all');
  const { data } = await response.json();
  
  const filtered = data.filter(course => 
    course.title.toLowerCase().includes(keyword.toLowerCase()) ||
    course.description.toLowerCase().includes(keyword.toLowerCase())
  );
  
  return filtered;
}
```

### 3. L?c khóa h?c theo category
```javascript
async function getCoursesByCategory(categoryId) {
  const response = await fetch('/api/course/all');
  const { data } = await response.json();
  
  return data.filter(course => course.categoryId === categoryId);
}
```

### 4. S?p x?p khóa h?c
```javascript
async function getSortedCourses(sortBy = 'rating') {
  const response = await fetch('/api/course/all');
  const { data } = await response.json();
  
  switch(sortBy) {
    case 'rating':
      return data.sort((a, b) => b.averageRating - a.averageRating);
    case 'popular':
      return data.sort((a, b) => b.totalEnrollments - a.totalEnrollments);
    case 'newest':
      return data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    case 'price-low':
      return data.sort((a, b) => a.price - b.price);
    case 'price-high':
      return data.sort((a, b) => b.price - a.price);
    default:
      return data;
  }
}
```

### 5. Hi?n th? chi ti?t course preview
```javascript
async function showCoursePreview(courseId) {
  const response = await fetch('/api/course/all');
  const { data } = await response.json();
  
  const course = data.find(c => c.id === courseId);
  if (!course) return;
  
  console.log('Course:', course.title);
  console.log('Description:', course.description);
  console.log('Price:', course.price, 'VND');
  console.log('Teacher:', course.teacherName);
  console.log('Rating:', course.averageRating, '/ 5');
  console.log('\nCourse Content:');
  console.log('- Sections:', course.totalSections);
  console.log('- Materials:', course.totalMaterials);
  console.log('- Quizzes:', course.totalQuizzes);
  console.log('- Flashcards:', course.totalFlashcards);
  console.log('\nSection Details:');
  course.sections.forEach(section => {
    console.log(`  ${section.title} (${section.materials.length} materials)`);
  });
}
```

## Tính n?ng n?i b?t

? **Public Access**: Không c?n ??ng nh?p
? **Complete Data**: Tr? v? ??y ?? thông tin khóa h?c
? **Teacher Info**: Bao g?m tên và avatar giáo viên
? **Rating System**: ?i?m ?ánh giá trung bình và s? l??t ?ánh giá
? **Rich Statistics**: Th?ng kê ??y ?? v? n?i dung
? **Full Content**: Sections, materials, quizzes, flashcards
? **Sorted by Date**: Khóa h?c m?i nh?t hi?n th? tr??c

## L?u ý quan tr?ng

1. **Ch? paid courses**: API ch? tr? v? khóa h?c có phí
2. **No deleted courses**: Khóa h?c ?ã xóa không ???c hi?n th?
3. **Full content visible**: T?t c? n?i dung ??u ???c hi?n th? (sections, quizzes, flashcards)
4. **Quiz answers included**: `isCorrect` trong options c?ng ???c tr? v?
5. **Free to use**: Không c?n authentication, phù h?p cho landing page, course catalog
6. **Real-time data**: D? li?u c?p nh?t real-time t? database

## Khác bi?t v?i các API khác

| API | Authentication | Data | Purpose |
|-----|---------------|------|---------|
| `GET /api/course/all` | ? None | All paid courses | Public course catalog |
| `GET /api/course/my-courses` | ? Teacher | Own courses only | Teacher management |
| `GET /api/course/{id}` | ? Teacher | Single course detail | Teacher view own course |

## Performance

- Response time: < 500ms (thông th??ng)
- Tr? v? toàn b? khóa h?c trong 1 request
- Recommend: Cache ? client-side n?u c?n
- Có th? implement pagination trong t??ng lai n?u s? l??ng khóa h?c l?n

## Testing

```bash
# Simple test
curl http://localhost:5000/api/course/all

# Pretty print JSON
curl http://localhost:5000/api/course/all | jq

# Check response time
curl -w "\nTime: %{time_total}s\n" http://localhost:5000/api/course/all

# Save to file
curl http://localhost:5000/api/course/all -o courses.json
