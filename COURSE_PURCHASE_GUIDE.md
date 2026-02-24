# ?? Course Purchase System - H??ng D?n S? D?ng

## ? HOÀN THÀNH

?ã t?o xong h? th?ng mua khóa h?c riêng l? v?i các tính n?ng:
- ? Student có th? mua course và có quy?n truy c?p v?nh vi?n
- ? Không c?n thanh toán th?t (auto-completed)
- ? T? ??ng enroll sau khi mua
- ? Ng?n ch?n mua trùng (unique constraint)
- ? Xem l?ch s? mua hàng
- ? Build thành công

---

## ?? B??C 1: Ch?y Migration SQL

**M? SQL Server Management Studio và ch?y script:**

```sql
-- File: Migrations\CreateCoursePurchasesTable.sql

CREATE TABLE course_purchases (
    id INT PRIMARY KEY IDENTITY(1,1),
    student_id INT NOT NULL,
    course_id INT NOT NULL,
    purchase_price DECIMAL(10, 2) NOT NULL,
    purchase_date DATETIME NOT NULL DEFAULT GETDATE(),
    payment_status VARCHAR(20) NOT NULL DEFAULT 'completed',
    transaction_id VARCHAR(100) NULL,
    
    CONSTRAINT FK_CoursePurchase_Student FOREIGN KEY (student_id) REFERENCES students(id),
    CONSTRAINT FK_CoursePurchase_Course FOREIGN KEY (course_id) REFERENCES courses(id),
    CONSTRAINT UQ_CoursePurchase_Student_Course UNIQUE (student_id, course_id)
);

-- Create indexes
CREATE INDEX IX_CoursePurchases_StudentId ON course_purchases(student_id);
CREATE INDEX IX_CoursePurchases_CourseId ON course_purchases(course_id);
CREATE INDEX IX_CoursePurchases_PurchaseDate ON course_purchases(purchase_date);
CREATE INDEX IX_CoursePurchases_PaymentStatus ON course_purchases(payment_status);

-- Add constraint
ALTER TABLE course_purchases
ADD CONSTRAINT CK_CoursePurchase_PaymentStatus 
CHECK (payment_status IN ('pending', 'completed', 'failed'));
```

**Ho?c dùng EF Core Migration:**

```sh
dotnet ef migrations add CreateCoursePurchasesTable --project Repository --startup-project StudyMate_BE
dotnet ef database update --project Repository --startup-project StudyMate_BE
```

---

## ?? API Endpoints

### 1. **POST `/api/coursepurchase/purchase`** - Mua Khóa H?c
**Authentication:** Required (Student only)

**Request Body:**
```json
{
    "courseId": 1
}
```

**Response Success:**
```json
{
    "message": "Course purchased successfully! You now have lifetime access to this course.",
    "data": {
        "id": 1,
        "studentId": 5,
        "courseId": 1,
        "courseTitle": "Web Development Bootcamp",
        "courseDescription": "Learn full-stack web development",
        "teacherName": "John Doe",
        "purchasePrice": 299000,
        "purchaseDate": "2024-01-15T10:30:00Z",
        "paymentStatus": "completed",
        "transactionId": "TXN202401151030001234"
    }
}
```

**Response Errors:**

```json
// Already purchased
{
    "message": "You have already purchased this course"
}

// Course is free
{
    "message": "This course is free. You can enroll directly without purchasing."
}

// Course not approved
{
    "message": "This course is not available for purchase yet"
}

// Not a student
{
    "message": "Student profile not found. Only students can purchase courses."
}
```

---

### 2. **GET `/api/coursepurchase/my-purchases`** - Xem Các Khóa ?ã Mua
**Authentication:** Required (Student only)

**Response Success:**
```json
{
    "message": "Purchased courses retrieved successfully",
    "count": 3,
    "data": [
        {
            "id": 3,
            "studentId": 5,
            "courseId": 10,
            "courseTitle": "Advanced React & Redux",
            "courseDescription": "Master React and state management",
            "teacherName": "Jane Smith",
            "purchasePrice": 399000,
            "purchaseDate": "2024-01-20T14:20:00Z",
            "paymentStatus": "completed",
            "transactionId": "TXN202401201420005678"
        },
        {
            "id": 2,
            "studentId": 5,
            "courseId": 5,
            "courseTitle": "Python for Data Science",
            "courseDescription": "Learn Python and data analysis",
            "teacherName": "Mike Johnson",
            "purchasePrice": 350000,
            "purchaseDate": "2024-01-18T09:15:00Z",
            "paymentStatus": "completed",
            "transactionId": "TXN202401180915003456"
        },
        {
            "id": 1,
            "studentId": 5,
            "courseId": 1,
            "courseTitle": "Web Development Bootcamp",
            "courseDescription": "Full-stack web development",
            "teacherName": "John Doe",
            "purchasePrice": 299000,
            "purchaseDate": "2024-01-15T10:30:00Z",
            "paymentStatus": "completed",
            "transactionId": "TXN202401151030001234"
        }
    ]
}
```

---

### 3. **GET `/api/coursepurchase/check-purchased/{courseId}`** - Ki?m Tra ?ã Mua Ch?a
**Authentication:** Required (Student only)

**Request:**
```
GET /api/coursepurchase/check-purchased/1
```

**Response (?ã mua):**
```json
{
    "purchased": true,
    "message": "Course already purchased"
}
```

**Response (Ch?a mua):**
```json
{
    "purchased": false,
    "message": "Course not purchased yet"
}
```

---

### 4. **GET `/api/coursepurchase/summary`** - T?ng Quan Mua Hàng
**Authentication:** Required (Student only)

**Response Success:**
```json
{
    "message": "Purchase summary retrieved successfully",
    "data": {
        "totalPurchased": 5,
        "totalSpent": 1495000,
        "purchases": [
            {
                "id": 5,
                "courseId": 15,
                "courseTitle": "Machine Learning A-Z",
                "purchasePrice": 450000,
                "purchaseDate": "2024-01-22T11:00:00Z",
                "paymentStatus": "completed"
            },
            // ... more purchases
        ]
    }
}
```

---

## ?? Quy T?c Mua Course

### ? ???C PHÉP:
1. **Student** mua course có giá (price > 0)
2. Course ph?i có tr?ng thái **approved**
3. Course **ch?a b? xóa** (IsDeleted = false)
4. Student **ch?a mua** course này tr??c ?ó

### ? KHÔNG ???C PHÉP:
1. Mua course **mi?n phí** (price = 0 ho?c null) - Nên enroll tr?c ti?p
2. Mua course **ch?a approved** ho?c **b? reject**
3. Mua course **?ã b? xóa**
4. Mua **trùng l?p** (database có unique constraint ng?n ch?n)
5. **Teacher** không th? mua course

### ?? Sau Khi Mua:
- ? Student ???c **t? ??ng enroll** vào course
- ? Có quy?n truy c?p **v?nh vi?n** (lifetime access)
- ? Không c?n thanh toán th?t (payment_status = "completed" t? ??ng)
- ? Nh?n transaction ID ?? theo dõi

---

## ?? Cách S? D?ng Trong FE

### 1. **Hi?n Th? Nút "Mua Course"**

```javascript
// Check if user ?ã mua course ch?a
async function checkCoursePurchased(token, courseId) {
    const response = await fetch(`/api/coursepurchase/check-purchased/${courseId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    const result = await response.json();
    return result.purchased;
}

// Component Course Detail
function CourseDetailPage({ course, token }) {
    const [isPurchased, setIsPurchased] = useState(false);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        checkPurchaseStatus();
    }, []);

    async function checkPurchaseStatus() {
        const purchased = await checkCoursePurchased(token, course.id);
        setIsPurchased(purchased);
    }

    async function handlePurchase() {
        setLoading(true);
        
        const response = await fetch('/api/coursepurchase/purchase', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ courseId: course.id })
        });

        const result = await response.json();
        
        if (response.ok) {
            alert('Mua khóa h?c thành công! B?n có th? truy c?p ngay.');
            setIsPurchased(true);
            // Redirect to course content
            window.location.href = `/course/${course.id}/learn`;
        } else {
            alert('L?i: ' + result.message);
        }
        
        setLoading(false);
    }

    return (
        <div className="course-detail">
            <h1>{course.title}</h1>
            <p>{course.description}</p>
            <div className="price">{course.price?.toLocaleString('vi-VN')} VND</div>

            {isPurchased ? (
                <button className="btn-access" onClick={() => window.location.href = `/course/${course.id}/learn`}>
                    ? Truy C?p Khóa H?c
                </button>
            ) : (
                <button 
                    className="btn-purchase" 
                    onClick={handlePurchase}
                    disabled={loading}
                >
                    {loading ? '?ang x? lý...' : '?? Mua Khóa H?c'}
                </button>
            )}
        </div>
    );
}
```

### 2. **Hi?n Th? Danh Sách Courses ?ã Mua**

```javascript
function MyPurchasedCoursesPage({ token }) {
    const [purchases, setPurchases] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadPurchases();
    }, []);

    async function loadPurchases() {
        const response = await fetch('/api/coursepurchase/my-purchases', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        const result = await response.json();
        setPurchases(result.data || []);
        setLoading(false);
    }

    if (loading) return <div>Loading...</div>;

    return (
        <div className="my-purchases-page">
            <h1>Khóa H?c ?ã Mua ({purchases.length})</h1>
            
            <div className="courses-grid">
                {purchases.map(purchase => (
                    <div key={purchase.id} className="course-card">
                        <h3>{purchase.courseTitle}</h3>
                        <p className="teacher">Gi?ng viên: {purchase.teacherName}</p>
                        <p className="price">
                            ?ã mua: {purchase.purchasePrice.toLocaleString('vi-VN')} VND
                        </p>
                        <p className="date">
                            Ngày mua: {new Date(purchase.purchaseDate).toLocaleDateString('vi-VN')}
                        </p>
                        <button onClick={() => window.location.href = `/course/${purchase.courseId}/learn`}>
                            H?c Ngay
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
}
```

### 3. **Hi?n Th? T?ng Quan Mua Hàng**

```javascript
function PurchaseSummaryWidget({ token }) {
    const [summary, setSummary] = useState(null);

    useEffect(() => {
        loadSummary();
    }, []);

    async function loadSummary() {
        const response = await fetch('/api/coursepurchase/summary', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        const result = await response.json();
        setSummary(result.data);
    }

    if (!summary) return null;

    return (
        <div className="purchase-summary">
            <h2>Th?ng Kê Mua Hàng</h2>
            <div className="stats">
                <div className="stat-item">
                    <span className="label">T?ng khóa h?c ?ã mua:</span>
                    <span className="value">{summary.totalPurchased}</span>
                </div>
                <div className="stat-item">
                    <span className="label">T?ng chi tiêu:</span>
                    <span className="value">
                        {summary.totalSpent.toLocaleString('vi-VN')} VND
                    </span>
                </div>
            </div>
        </div>
    );
}
```

---

## ?? Lu?ng Ho?t ??ng

```
????????????????????????????????????????????
?  Student Browse Courses                  ?
????????????????????????????????????????????
               ?
               ?
????????????????????????????????????????????
?  Click "View Course Detail"              ?
?  Course ID: 1                            ?
????????????????????????????????????????????
               ?
               ?
????????????????????????????????????????????
?  FE: Check if purchased                  ?
?  GET /api/coursepurchase/check-purchased/1?
????????????????????????????????????????????
               ?
       ??????????????????
       ?                ?
       ?                ?
???????????????  ????????????????????
? Purchased   ?  ? Not Purchased    ?
? Show Access ?  ? Show Buy Button  ?
???????????????  ????????????????????
                          ?
                          ?
                 ????????????????????
                 ? Student Clicks   ?
                 ? "Buy Course"     ?
                 ????????????????????
                          ?
                          ?
                 ????????????????????????????
                 ? POST /api/coursepurchase/?
                 ?      purchase            ?
                 ? Body: { courseId: 1 }    ?
                 ????????????????????????????
                          ?
                          ?
                 ????????????????????????????
                 ? BE Validates:            ?
                 ? - User is student?       ?
                 ? - Course exists?         ?
                 ? - Course approved?       ?
                 ? - Has price?             ?
                 ? - Not purchased before?  ?
                 ????????????????????????????
                          ?
                          ?
                 ????????????????????????????
                 ? Create Purchase Record:  ?
                 ? - payment_status=completed?
                 ? - transaction_id generated?
                 ? - purchase_price saved   ?
                 ????????????????????????????
                          ?
                          ?
                 ????????????????????????????
                 ? Auto Enroll Student:     ?
                 ? - Create enrollment      ?
                 ? - enrolled_at = NOW      ?
                 ????????????????????????????
                          ?
                          ?
                 ????????????????????????????
                 ? Return Success           ?
                 ? "Lifetime Access Granted"?
                 ????????????????????????????
```

---

## ??? Database Schema

```sql
CREATE TABLE course_purchases (
    id INT PRIMARY KEY,
    student_id INT NOT NULL,          -- FK to students
    course_id INT NOT NULL,           -- FK to courses
    purchase_price DECIMAL(10,2),     -- Giá t?i th?i ?i?m mua
    purchase_date DATETIME,           -- Ngày mua
    payment_status VARCHAR(20),       -- "completed" (auto)
    transaction_id VARCHAR(100),      -- "TXNyyyyMMddHHmmss1234"
    
    UNIQUE (student_id, course_id)    -- Ng?n mua trùng
);
```

**Example Data:**
```
| id | student_id | course_id | purchase_price | purchase_date       | payment_status | transaction_id      |
|----|------------|-----------|----------------|---------------------|----------------|---------------------|
| 1  | 5          | 1         | 299000         | 2024-01-15 10:30:00 | completed      | TXN202401151030001234|
| 2  | 5          | 5         | 350000         | 2024-01-18 09:15:00 | completed      | TXN202401180915003456|
| 3  | 8          | 1         | 299000         | 2024-01-20 14:20:00 | completed      | TXN202401201420005678|
```

---

## ?? Tóm T?t

### ? Tính N?ng:
- **Mua course:** Student mua và có lifetime access
- **Không thanh toán th?t:** Auto-complete (payment_status = "completed")
- **T? ??ng enroll:** Sau khi mua t? ??ng enroll vào course
- **Ng?n mua trùng:** Unique constraint (student_id, course_id)
- **L?ch s? mua hàng:** Xem t?t c? courses ?ã mua
- **T?ng quan:** T?ng s? course ?ã mua, t?ng chi tiêu

### ?? API Endpoints:
1. `POST /api/coursepurchase/purchase` - Mua course
2. `GET /api/coursepurchase/my-purchases` - Xem courses ?ã mua
3. `GET /api/coursepurchase/check-purchased/{courseId}` - Ki?m tra ?ã mua ch?a
4. `GET /api/coursepurchase/summary` - T?ng quan mua hàng

### ?? Files ?ã T?o:
1. `Repository\Model\CoursePurchase.cs` - Model
2. `Service\DTOs\CoursePurchaseDto.cs` - DTOs
3. `Service\Interface\ICoursePurchaseService.cs` - Interface
4. `Service\Implement\CoursePurchaseService.cs` - Service logic
5. `StudyMate_BE\Controllers\CoursePurchaseController.cs` - API endpoints
6. `Migrations\CreateCoursePurchasesTable.sql` - Migration script

---

**Chúc b?n code thành công! ??**
