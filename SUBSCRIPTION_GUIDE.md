# ?? Premium Plan Subscription System

## ? HOÀN THÀNH

?ã t?o xong h? th?ng ??ng ký Premium Plan v?i các tính n?ng:
- ? Student và Teacher có th? ??ng ký plan phù h?p v?i role
- ? Không c?n thanh toán (mi?n phí ??ng ký)
- ? Ch? ???c ??ng ký 1 gói duy nh?t cùng lúc
- ? T? ??ng h?y gói khi h?t h?n (Background Service)
- ? Build thành công

---

## ?? B??C 1: Ch?y Migration SQL

**M? SQL Server Management Studio và ch?y script:**

```sql
-- Add teacher_id column
ALTER TABLE user_subscriptions
ADD teacher_id INT NULL;

-- Add is_active column
ALTER TABLE user_subscriptions
ADD is_active BIT NOT NULL DEFAULT 1;

-- Make student_id nullable
ALTER TABLE user_subscriptions
ALTER COLUMN student_id INT NULL;

-- Drop old foreign key
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sub_Student')
    ALTER TABLE user_subscriptions DROP CONSTRAINT FK_Sub_Student;

-- Add new foreign key constraints
ALTER TABLE user_subscriptions
ADD CONSTRAINT FK_Sub_Student FOREIGN KEY (student_id) 
REFERENCES students(id) ON DELETE CASCADE;

ALTER TABLE user_subscriptions
ADD CONSTRAINT FK_Sub_Teacher FOREIGN KEY (teacher_id) 
REFERENCES teachers(id) ON DELETE CASCADE;

-- Add check constraint
ALTER TABLE user_subscriptions
ADD CONSTRAINT CK_UserSubscription_OneRole 
CHECK (
    (student_id IS NOT NULL AND teacher_id IS NULL) 
    OR 
    (student_id IS NULL AND teacher_id IS NOT NULL)
);

-- Create indexes
CREATE INDEX IX_UserSubscriptions_TeacherId ON user_subscriptions(teacher_id);
CREATE INDEX IX_UserSubscriptions_IsActive ON user_subscriptions(is_active);
CREATE INDEX IX_UserSubscriptions_EndDate ON user_subscriptions(end_date);
```

---

## ?? API Endpoints

### 1. **POST `/api/premiumplan/subscribe`** - ??ng Ký Plan
**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
    "planId": 1
}
```

**Response Success:**
```json
{
    "message": "Successfully subscribed to premium plan",
    "data": {
        "id": 1,
        "studentId": 5,
        "teacherId": null,
        "planId": 1,
        "planName": "Student Basic - 1 Tháng",
        "durationDays": 30,
        "price": 99000,
        "targetRole": "student",
        "startDate": "2024-01-15T10:00:00Z",
        "endDate": "2024-02-14T10:00:00Z",
        "isActive": true,
        "daysRemaining": 30
    }
}
```

**Response Error (Already subscribed):**
```json
{
    "message": "You already have an active subscription. Please cancel it before subscribing to a new plan."
}
```

**Response Error (Wrong role):**
```json
{
    "message": "This plan is for teacher, but you are a student"
}
```

---

### 2. **GET `/api/premiumplan/my-subscription`** - Xem Subscription Hi?n T?i
**Authentication:** Required

**Response Success:**
```json
{
    "message": "Active subscription retrieved successfully",
    "data": {
        "id": 1,
        "studentId": 5,
        "teacherId": null,
        "planId": 1,
        "planName": "Student Basic - 1 Tháng",
        "durationDays": 30,
        "price": 99000,
        "targetRole": "student",
        "startDate": "2024-01-15T10:00:00Z",
        "endDate": "2024-02-14T10:00:00Z",
        "isActive": true,
        "daysRemaining": 25
    }
}
```

**Response Error (No subscription):**
```json
{
    "message": "No active subscription found"
}
```

**Response Error (Expired):**
```json
{
    "message": "Your subscription has expired"
}
```

---

### 3. **POST `/api/premiumplan/cancel-subscription`** - H?y Subscription
**Authentication:** Required

**Response Success:**
```json
{
    "message": "Subscription cancelled successfully"
}
```

**Response Error:**
```json
{
    "message": "No active subscription found"
}
```

---

## ?? Quy T?c ??ng Ký

### ? ???C PHÉP:
1. **Student** ??ng ký plan có `targetRole = "student"`
2. **Teacher** ??ng ký plan có `targetRole = "teacher"`
3. ??ng ký MI?N PHÍ (không c?n thanh toán)
4. H?y subscription b?t k? lúc nào
5. Sau khi h?y, có th? ??ng ký plan m?i

### ? KHÔNG ???C PHÉP:
1. ??ng ký plan không kh?p v?i role (student ??ng ký teacher plan)
2. ??ng ký nhi?u h?n 1 plan cùng lúc
3. ??ng ký plan m?i khi ?ang có subscription active

### ? T? ??ng H?y:
- Background Service ch?y m?i **1 gi?** ?? ki?m tra
- T? ??ng set `isActive = false` cho subscriptions h?t h?n
- Sau khi h?t h?n, user có th? ??ng ký plan m?i

---

## ?? Cách S? D?ng Trong FE

### 1. **Hi?n Th? Danh Sách Plans**

```javascript
// Get plans phù h?p v?i role c?a user
async function fetchMyPlans(token) {
    const response = await fetch('/api/premiumplan/my-plans', {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    const result = await response.json();
    return result.data; // Danh sách plans
}
```

### 2. **Ki?m Tra Subscription Hi?n T?i**

```javascript
async function checkCurrentSubscription(token) {
    try {
        const response = await fetch('/api/premiumplan/my-subscription', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            return {
                hasSubscription: true,
                data: result.data
            };
        } else {
            return {
                hasSubscription: false,
                data: null
            };
        }
    } catch (error) {
        return {
            hasSubscription: false,
            data: null
        };
    }
}
```

### 3. **??ng Ký Plan**

```javascript
async function subscribeToPlan(token, planId) {
    const response = await fetch('/api/premiumplan/subscribe', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ planId })
    });
    
    const result = await response.json();
    
    if (response.ok) {
        alert('??ng ký thành công!');
        return result.data;
    } else {
        alert('L?i: ' + result.message);
        return null;
    }
}
```

### 4. **H?y Subscription**

```javascript
async function cancelSubscription(token) {
    const response = await fetch('/api/premiumplan/cancel-subscription', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    
    const result = await response.json();
    
    if (response.ok) {
        alert('H?y thành công!');
    } else {
        alert('L?i: ' + result.message);
    }
}
```

### 5. **Component Example (React)**

```jsx
import React, { useState, useEffect } from 'react';

function PremiumSubscriptionPage({ token }) {
    const [plans, setPlans] = useState([]);
    const [currentSubscription, setCurrentSubscription] = useState(null);
    const [hasSubscription, setHasSubscription] = useState(false);

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        // Load available plans
        const plansData = await fetchMyPlans(token);
        setPlans(plansData);

        // Check current subscription
        const subData = await checkCurrentSubscription(token);
        setHasSubscription(subData.hasSubscription);
        setCurrentSubscription(subData.data);
    }

    async function handleSubscribe(planId) {
        if (hasSubscription) {
            alert('B?n ?ã có subscription. Vui lòng h?y tr??c!');
            return;
        }

        const result = await subscribeToPlan(token, planId);
        if (result) {
            loadData(); // Reload data
        }
    }

    async function handleCancel() {
        if (confirm('B?n có ch?c mu?n h?y subscription?')) {
            await cancelSubscription(token);
            loadData(); // Reload data
        }
    }

    return (
        <div className="subscription-page">
            <h1>Premium Plans</h1>

            {hasSubscription && currentSubscription && (
                <div className="current-subscription">
                    <h2>Gói Hi?n T?i</h2>
                    <div className="sub-card">
                        <h3>{currentSubscription.planName}</h3>
                        <p>Giá: {currentSubscription.price?.toLocaleString('vi-VN')} VND</p>
                        <p>Th?i h?n: {currentSubscription.durationDays} ngày</p>
                        <p>Ngày h?t h?n: {new Date(currentSubscription.endDate).toLocaleDateString('vi-VN')}</p>
                        <p>S? ngày còn l?i: {currentSubscription.daysRemaining} ngày</p>
                        <button onClick={handleCancel} className="btn-cancel">
                            H?y Subscription
                        </button>
                    </div>
                </div>
            )}

            {!hasSubscription && (
                <div className="available-plans">
                    <h2>Ch?n Gói Premium</h2>
                    <div className="plans-grid">
                        {plans.map(plan => (
                            <div key={plan.id} className="plan-card">
                                <h3>{plan.name}</h3>
                                <div className="price">
                                    {plan.price?.toLocaleString('vi-VN')} VND
                                </div>
                                <p className="duration">{plan.durationDays} ngày</p>
                                <button 
                                    onClick={() => handleSubscribe(plan.id)}
                                    className="btn-subscribe"
                                >
                                    ??ng Ký Ngay (Mi?n Phí)
                                </button>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
```

---

## ?? Lu?ng Ho?t ??ng

```
????????????????????????????????????????????
?  User Login (Student/Teacher)            ?
????????????????????????????????????????????
               ?
               ?
????????????????????????????????????????????
?  Check Current Subscription              ?
?  GET /api/premiumplan/my-subscription    ?
????????????????????????????????????????????
               ?
       ??????????????????
       ?                ?
       ?                ?
???????????????  ????????????????????
? Has Active  ?  ? No Subscription  ?
? Subscription?  ?                  ?
???????????????  ????????????????????
       ?                  ?
       ?                  ?
       ?         ????????????????????
       ?         ? Show Plans List  ?
       ?         ? (by user's role) ?
       ?         ????????????????????
       ?                  ?
       ?                  ?
       ?         ????????????????????
       ?         ? User Clicks      ?
       ?         ? "Subscribe"      ?
       ?         ????????????????????
       ?                  ?
       ?                  ?
       ?         ????????????????????????????
       ?         ? POST /api/premiumplan/   ?
       ?         ?      subscribe           ?
       ?         ? Body: { planId: 1 }      ?
       ?         ????????????????????????????
       ?                  ?
       ?                  ?
       ?         ????????????????????????????
       ?         ? BE Validates:            ?
       ?         ? - Plan exists?           ?
       ?         ? - Plan matches role?     ?
       ?         ? - No active sub?         ?
       ?         ????????????????????????????
       ?                  ?
       ?                  ?
       ?         ????????????????????????????
       ?         ? Create Subscription:     ?
       ?         ? - Start: NOW             ?
       ?         ? - End: NOW + 30 days     ?
       ?         ? - IsActive: TRUE         ?
       ?         ????????????????????????????
       ?                  ?
       ???????????????????????????????????????
                                             ?
                                             ?
                                ??????????????????????????
                                ? Show Subscription Info ?
                                ? - Days Remaining       ?
                                ? - End Date             ?
                                ? - [Cancel Button]      ?
                                ??????????????????????????
```

---

## ?? Background Service - Auto Expire

**Service:** `SubscriptionExpiryService`
**Ch?y m?i:** 1 gi?
**Ch?c n?ng:**
- Tìm subscriptions có `isActive = true` và `endDate < NOW`
- Set `isActive = false` cho nh?ng subscriptions này
- Log k?t qu?

**Logs Example:**
```
[INFO] Subscription Expiry Service started
[INFO] Found 3 expired subscriptions
[INFO] Subscription ID 15 has been deactivated (expired on 2024-01-14 10:00:00)
[INFO] Subscription ID 22 has been deactivated (expired on 2024-01-14 11:30:00)
[INFO] Subscription ID 31 has been deactivated (expired on 2024-01-14 15:00:00)
[INFO] Successfully deactivated 3 expired subscriptions
```

---

## ?? Tóm T?t

### ? Tính N?ng:
- **??ng ký plan:** Mi?n phí, không c?n thanh toán
- **Gi?i h?n:** Ch? 1 subscription active/user
- **Role-based:** Student ch? ??ng ký student plans, Teacher ch? ??ng ký teacher plans
- **T? ??ng h?y:** Background service h?y subscriptions h?t h?n m?i 1 gi?
- **Qu?n lý:** Xem, h?y subscription b?t k? lúc nào

### ?? API Endpoints:
1. `POST /api/premiumplan/subscribe` - ??ng ký plan
2. `GET /api/premiumplan/my-subscription` - Xem subscription hi?n t?i
3. `POST /api/premiumplan/cancel-subscription` - H?y subscription

### ?? Files ?ã T?o/S?a:
1. `Repository\Model\UserSubscription.cs` - Thêm TeacherId, IsActive
2. `Repository\Model\Teacher.cs` - Thêm UserSubscriptions
3. `Repository\DbContext\StudyMateContext.cs` - C?p nh?t mapping
4. `Migrations\UpdateUserSubscriptionsForTeacher.sql` - Migration script
5. `Service\DTOs\SubscriptionDto.cs` - DTOs m?i
6. `Service\Interface\IPremiumPlanService.cs` - Thêm subscription methods
7. `Service\Implement\PremiumPlanService.cs` - Implement logic
8. `StudyMate_BE\Controllers\PremiumPlanController.cs` - Thêm endpoints
9. `StudyMate_BE\BackgroundServices\SubscriptionExpiryService.cs` - Auto expire service
10. `StudyMate_BE\Program.cs` - ??ng ký background service

---

**Chúc b?n code thành công! ??**
