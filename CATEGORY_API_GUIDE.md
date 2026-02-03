# H??ng D?n S? D?ng Category API

## ?? T?ng Quan

Tôi ?ã t?o m?t API endpoint m?i ?? FE có th? l?y danh sách t?t c? các categories t? database. API này tr? v? **ch? ID và Name** c?a category (không có Description nh? b?n yêu c?u).

## ?? API Endpoint

### GET `/api/category/all`

**Mô t?:** L?y danh sách t?t c? các categories  
**Authentication:** Không c?n (Public access)  
**Method:** GET

### Response Format

```json
{
    "message": "Categories retrieved successfully",
    "count": 5,
    "data": [
        {
            "id": 1,
            "name": "Programming"
        },
        {
            "id": 2,
            "name": "Design"
        },
        {
            "id": 3,
            "name": "Business"
        },
        {
            "id": 4,
            "name": "Marketing"
        },
        {
            "id": 5,
            "name": "Photography"
        }
    ]
}
```

## ?? Cách S? D?ng Trong FE

### 1. L?y danh sách categories khi load trang Create/Edit Course

```javascript
// React/Vue/Angular example
async function fetchCategories() {
    try {
        const response = await fetch('http://localhost:5000/api/category/all');
        const result = await response.json();
        
        if (result.data) {
            // result.data ch?a m?ng categories
            // Có th? dùng ?? render dropdown/select
            return result.data;
        }
    } catch (error) {
        console.error('Error fetching categories:', error);
    }
}
```

### 2. Render dropdown/select trong form

```jsx
// React example
function CourseForm() {
    const [categories, setCategories] = useState([]);
    const [selectedCategoryId, setSelectedCategoryId] = useState(null);

    useEffect(() => {
        fetchCategories().then(data => setCategories(data));
    }, []);

    return (
        <select 
            value={selectedCategoryId} 
            onChange={(e) => setSelectedCategoryId(e.target.value)}
        >
            <option value="">-- Select Category --</option>
            {categories.map(category => (
                <option key={category.id} value={category.id}>
                    {category.name}
                </option>
            ))}
        </select>
    );
}
```

### 3. G?i categoryId khi t?o ho?c ch?nh s?a course

```javascript
// Khi t?o course m?i
const createCourseData = {
    title: "My Course",
    description: "Course description",
    price: 99.99,
    categoryId: 2,  // ? ID c?a category ?ã ch?n
    sections: [...],
    quizzes: [...],
    flashcards: [...]
};

fetch('http://localhost:5000/api/course/create', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(createCourseData)
});
```

```javascript
// Khi update course
const updateCourseData = {
    title: "Updated Course",
    description: "Updated description",
    price: 149.99,
    categoryId: 3,  // ? ID c?a category m?i
    sections: [...],
    quizzes: [...],
    flashcards: [...]
};

fetch('http://localhost:5000/api/course/update/1', {
    method: 'PUT',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(updateCourseData)
});
```

## ?? Lu?ng Ho?t ??ng

1. **FE g?i** `GET /api/category/all` ? Nh?n danh sách categories
2. **FE hi?n th?** dropdown/select cho user ch?n category
3. **User ch?n** m?t category t? dropdown
4. **FE g?i** `categoryId` trong request body khi t?o/update course
5. **BE l?u** course v?i categoryId t??ng ?ng

## ?? L?u Ý

- `categoryId` là **optional** (có th? null) khi t?o/update course
- API `/api/category/all` **không c?n authentication** - FE có th? g?i tr?c ti?p
- Categories ???c s?p x?p theo **Name (A-Z)**
- Response **ch? bao g?m Id và Name**, không có Description

## ?? Ví D? UI Flow

```
[Create Course Page]
??? Title: [Input field]
??? Description: [Textarea]
??? Price: [Number input]
??? Category: [Dropdown]  ? Dùng data t? /api/category/all
?   ??? -- Select Category --
?   ??? Programming
?   ??? Design
?   ??? Business
??? Sections: [...]
??? [Create Button]
```

## ?? Files ?ã T?o

1. **Service\DTOs\CategoryDto.cs** - DTO response (ch? có Id và Name)
2. **Service\Interface\ICategoryService.cs** - Interface
3. **Service\Implement\CategoryService.cs** - Business logic
4. **StudyMate_BE\Controllers\CategoryController.cs** - API endpoint
5. **Program.cs** - ?ã ??ng ký CategoryService vào DI

## ? Testing

B?n có th? test API này trong Swagger UI:
1. M? browser: `http://localhost:5000`
2. Tìm endpoint: `GET /api/category/all`
3. Click "Try it out"
4. Click "Execute"
5. Xem response

Ho?c dùng curl:
```bash
curl -X GET "http://localhost:5000/api/category/all"
```

---

**Chúc b?n code vui v?! ??**
