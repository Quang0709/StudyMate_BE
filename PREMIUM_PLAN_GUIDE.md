# ?? Premium Plan API - Phân Bi?t Student & Teacher

## ? HOÀN THÀNH

?ã thêm tính n?ng hi?n th? Premium Plans theo Role:
- ? Thêm field `target_role` vào `premium_plans` table
- ? C?p nh?t Model `PremiumPlan` v?i property `TargetRole`
- ? T?o Service & Controller ?? l?y plans theo role
- ? Build thành công

---

## ?? B??C 1: Ch?y Migration SQL

**M? SQL Server Management Studio và ch?y script:**

```sql
-- Add target_role column
ALTER TABLE premium_plans
ADD target_role VARCHAR(20) NULL;

-- Update existing data (optional)
UPDATE premium_plans
SET target_role = 'student'
WHERE target_role IS NULL;

-- Add check constraint
ALTER TABLE premium_plans
ADD CONSTRAINT CK_PremiumPlan_TargetRole 
CHECK (target_role IN ('student', 'teacher'));
```

**Ho?c thêm sample data:**

```sql
-- Insert sample premium plans
INSERT INTO premium_plans (name, duration_days, price, target_role) 
VALUES 
    -- Student Plans
    ('Student Basic - 1 Tháng', 30, 99000, 'student'),
    ('Student Pro - 3 Tháng', 90, 249000, 'student'),
    ('Student Premium - 1 N?m', 365, 799000, 'student'),
    
    -- Teacher Plans
    ('Teacher Basic - 1 Tháng', 30, 149000, 'teacher'),
    ('Teacher Pro - 3 Tháng', 90, 399000, 'teacher'),
    ('Teacher Premium - 1 N?m', 365, 1299000, 'teacher');
```

---

## ?? API Endpoints

### 1. **GET `/api/premiumplan/my-plans`** 
**L?y plans theo role c?a user ?ang login**

**Authentication:** Required (Bearer Token)

**Cách ho?t ??ng:**
- Token có role = `student` ? Tr? v? plans có `targetRole = "student"`
- Token có role = `teacher` ? Tr? v? plans có `targetRole = "teacher"`

**Request:**
```bash
GET /api/premiumplan/my-plans
Authorization: Bearer {your-access-token}
```

**Response cho Student:**
```json
{
    "message": "Premium plans for student retrieved successfully",
    "count": 3,
    "data": [
        {
            "id": 1,
            "name": "Student Basic - 1 Tháng",
            "durationDays": 30,
            "price": 99000,
            "targetRole": "student"
        },
        {
            "id": 2,
            "name": "Student Pro - 3 Tháng",
            "durationDays": 90,
            "price": 249000,
            "targetRole": "student"
        },
        {
            "id": 3,
            "name": "Student Premium - 1 N?m",
            "durationDays": 365,
            "price": 799000,
            "targetRole": "student"
        }
    ]
}
```

**Response cho Teacher:**
```json
{
    "message": "Premium plans for teacher retrieved successfully",
    "count": 3,
    "data": [
        {
            "id": 4,
            "name": "Teacher Basic - 1 Tháng",
            "durationDays": 30,
            "price": 149000,
            "targetRole": "teacher"
        },
        {
            "id": 5,
            "name": "Teacher Pro - 3 Tháng",
            "durationDays": 90,
            "price": 399000,
            "targetRole": "teacher"
        },
        {
            "id": 6,
            "name": "Teacher Premium - 1 N?m",
            "durationDays": 365,
            "price": 1299000,
            "targetRole": "teacher"
        }
    ]
}
```

---

### 2. **GET `/api/premiumplan/by-role/{role}`**
**L?y plans theo role c? th? (Public - không c?n login)**

**Authentication:** Not Required

**Params:**
- `role`: `"student"` ho?c `"teacher"`

**Request:**
```bash
# L?y plans cho Student
GET /api/premiumplan/by-role/student

# L?y plans cho Teacher
GET /api/premiumplan/by-role/teacher
```

**Use case:** Hi?n th? pricing page trên landing page

---

### 3. **GET `/api/premiumplan/all`**
**L?y t?t c? plans (Admin)**

**Authentication:** Not Required (có th? thêm `[Authorize(Roles = "admin")]` n?u c?n)

**Request:**
```bash
GET /api/premiumplan/all
```

**Response:**
```json
{
    "message": "All premium plans retrieved successfully",
    "count": 6,
    "data": [
        {
            "id": 1,
            "name": "Student Basic - 1 Tháng",
            "durationDays": 30,
            "price": 99000,
            "targetRole": "student"
        },
        {
            "id": 4,
            "name": "Teacher Basic - 1 Tháng",
            "durationDays": 30,
            "price": 149000,
            "targetRole": "teacher"
        }
        // ... more plans
    ]
}
```

---

### 4. **GET `/api/premiumplan/{id}`**
**L?y chi ti?t plan theo ID**

**Authentication:** Not Required

**Request:**
```bash
GET /api/premiumplan/1
```

**Response:**
```json
{
    "message": "Premium plan retrieved successfully",
    "data": {
        "id": 1,
        "name": "Student Basic - 1 Tháng",
        "durationDays": 30,
        "price": 99000,
        "targetRole": "student"
    }
}
```

---

## ?? Cách S? D?ng Trong FE

### 1. **Hi?n th? Plans Cho User ?ã Login**

```javascript
// Sau khi login, FE có accessToken
const accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

// G?i API ?? l?y plans phù h?p v?i role
async function fetchMyPlans() {
    const response = await fetch('/api/premiumplan/my-plans', {
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    });
    
    const result = await response.json();
    
    // result.data s? ch?a plans phù h?p v?i role
    // - N?u user là Student ? ch? có Student plans
    // - N?u user là Teacher ? ch? có Teacher plans
    
    return result.data;
}

// Render UI
function PricingPage() {
    const [plans, setPlans] = useState([]);
    
    useEffect(() => {
        fetchMyPlans().then(data => setPlans(data));
    }, []);
    
    return (
        <div className="pricing-page">
            <h1>Ch?n Gói Premium</h1>
            <div className="plans-grid">
                {plans.map(plan => (
                    <div key={plan.id} className="plan-card">
                        <h3>{plan.name}</h3>
                        <p className="price">
                            {plan.price?.toLocaleString('vi-VN')} VND
                        </p>
                        <p className="duration">
                            {plan.durationDays} ngày
                        </p>
                        <button>Ch?n Gói Này</button>
                    </div>
                ))}
            </div>
        </div>
    );
}
```

### 2. **Hi?n th? Pricing Trên Landing Page (Ch?a Login)**

```javascript
// Không c?n token, public access
async function fetchPlansByRole(role) {
    const response = await fetch(`/api/premiumplan/by-role/${role}`);
    const result = await response.json();
    return result.data;
}

// Component Landing Page
function LandingPricingSection() {
    const [activeTab, setActiveTab] = useState('student');
    const [plans, setPlans] = useState([]);
    
    useEffect(() => {
        fetchPlansByRole(activeTab).then(data => setPlans(data));
    }, [activeTab]);
    
    return (
        <div className="landing-pricing">
            <h1>B?ng Giá</h1>
            
            {/* Tab switcher */}
            <div className="role-tabs">
                <button 
                    className={activeTab === 'student' ? 'active' : ''}
                    onClick={() => setActiveTab('student')}
                >
                    H?c Viên
                </button>
                <button 
                    className={activeTab === 'teacher' ? 'active' : ''}
                    onClick={() => setActiveTab('teacher')}
                >
                    Gi?ng Viên
                </button>
            </div>
            
            {/* Plans display */}
            <div className="plans-grid">
                {plans.map(plan => (
                    <div key={plan.id} className="plan-card">
                        <h3>{plan.name}</h3>
                        <div className="price">
                            {plan.price?.toLocaleString('vi-VN')} ?
                        </div>
                        <div className="duration">
                            {plan.durationDays} ngày
                        </div>
                        <button>??ng Ký Ngay</button>
                    </div>
                ))}
            </div>
        </div>
    );
}
```

---

## ?? Lu?ng Ho?t ??ng

```
???????????????????????????????????????????
?  User Login                             ?
?  Role: Student                          ?
???????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????
?  FE: Call GET /api/premiumplan/my-plans ?
?  Header: Authorization: Bearer {token}  ?
???????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????
?  BE: Extract role from JWT token        ?
?  ? role = "student"                     ?
???????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????
?  BE: Query Database                     ?
?  SELECT * FROM premium_plans            ?
?  WHERE target_role = 'student'          ?
???????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????
?  BE: Return Student Plans Only          ?
?  [                                      ?
?    {id: 1, name: "Student Basic"...},   ?
?    {id: 2, name: "Student Pro"...},     ?
?    {id: 3, name: "Student Premium"...}  ?
?  ]                                      ?
???????????????????????????????????????????
               ?
               ?
???????????????????????????????????????????
?  FE: Display Student Plans              ?
?  ???????? ???????? ????????             ?
?  ?Basic ? ? Pro  ? ?Premium?            ?
?  ?99K   ? ?249K  ? ?799K  ?             ?
?  ???????? ???????? ????????             ?
???????????????????????????????????????????
```

---

## ?? Tóm T?t

### ? ?ã Thêm:
- **Model:** `PremiumPlan.TargetRole` (student/teacher)
- **Database:** Column `target_role` trong table `premium_plans`
- **Service:** `IPremiumPlanService` v?i method `GetPlansByRoleAsync()`
- **Controller:** `PremiumPlanController` v?i 4 endpoints

### ?? API Endpoints:
1. **`GET /api/premiumplan/my-plans`** - L?y plans theo role c?a user (c?n login)
2. **`GET /api/premiumplan/by-role/{role}`** - L?y plans theo role c? th? (public)
3. **`GET /api/premiumplan/all`** - L?y t?t c? plans (admin)
4. **`GET /api/premiumplan/{id}`** - L?y plan theo ID (public)

### ?? FE Usage:
- **User ?ã login:** G?i `/api/premiumplan/my-plans` v?i token
- **Landing page:** G?i `/api/premiumplan/by-role/student` ho?c `/by-role/teacher`
- **Chi ti?t plan:** G?i `/api/premiumplan/{id}`

---

**Chúc b?n code thành công! ??**
