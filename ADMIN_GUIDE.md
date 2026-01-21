# Admin Account Management

## Default Admin Account

Khi ?ng d?ng ch?y l?n ??u, h? th?ng t? ??ng t?o tài kho?n admin v?i credentials:

```
Email: admin@studymate.com
Password: Admin@123456
Role: admin
```

## C?u hình Admin

File: `StudyMate_BE/appsettings.json`

```json
"DefaultAdmin": {
  "Email": "admin@studymate.com",
  "Password": "Admin@123456",
  "FullName": "System Administrator"
}
```

B?n có th? thay ??i email, password và full name tr??c khi ch?y ?ng d?ng l?n ??u.

## Login Admin

### S? d?ng Swagger:
1. Truy c?p `https://localhost:7259`
2. Click `/api/auth/login`
3. Click "Try it out"
4. Nh?p:
```json
{
  "email": "admin@studymate.com",
  "password": "Admin@123456"
}
```
5. Click "Execute"
6. Copy `accessToken` t? response
7. Click nút "Authorize" và paste: `Bearer {accessToken}`

### S? d?ng cURL:
```bash
curl -X POST https://localhost:7259/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@studymate.com",
    "password": "Admin@123456"
  }'
```

## Admin Permissions

Admin có toàn quy?n truy c?p t?t c? API endpoints:
- ? Xem t?t c? users
- ? Ban/Unban users
- ? Approve/Reject courses
- ? Qu?n lý n?i dung
- ? Xem th?ng kê

## Ki?m tra Admin trong Database

```sql
-- Check admin user
SELECT * FROM users WHERE role = 'admin';

-- Check admin login history
SELECT * FROM refresh_tokens 
WHERE user_id = (SELECT id FROM users WHERE role = 'admin')
ORDER BY created_at DESC;
```

## ??i Admin Password (Recommended)

### B??c 1: Login v?i admin
```json
POST /api/auth/login
{
  "email": "admin@studymate.com",
  "password": "Admin@123456"
}
```

### B??c 2: ??i password trong database
```sql
-- T?o hash cho password m?i (ví d?: NewSecurePassword@2024)
-- Hash b?ng SHA256

UPDATE users 
SET password = 'YourNewHashedPasswordHere'
WHERE email = 'admin@studymate.com';
```

**Ho?c** t?o API endpoint riêng cho change password (recommended).

## T?o Admin m?i

### Cách 1: Thêm tr?c ti?p vào database
```sql
-- Hash password tr??c (SHA256)
INSERT INTO users (email, password, full_name, role, is_banned, created_at)
VALUES (
  'newadmin@studymate.com',
  'HashedPasswordHere',
  'New Admin Name',
  'admin',
  0,
  GETUTCDATE()
);
```

### Cách 2: Thêm vào appsettings và restart app
```json
"AdditionalAdmins": [
  {
    "Email": "admin2@studymate.com",
    "Password": "Admin2@123456",
    "FullName": "Second Administrator"
  }
]
```

## Xóa/Disable Admin

### Disable (Ban):
```sql
UPDATE users 
SET is_banned = 1 
WHERE email = 'admin@studymate.com';
```

### Xóa hoàn toàn:
```sql
-- Xóa refresh tokens tr??c
DELETE FROM refresh_tokens WHERE user_id = (
  SELECT id FROM users WHERE email = 'admin@studymate.com'
);

-- Xóa user
DELETE FROM users WHERE email = 'admin@studymate.com';
```

## Security Best Practices

### Development:
- ? S? d?ng admin default ?? test
- ? Không commit appsettings.json v?i password th?t

### Production:
- ?? **??I PASSWORD NGAY** sau deploy l?n ??u
- ?? S? d?ng Environment Variables thay vì appsettings.json
- ?? Enable 2FA cho admin (n?u có)
- ?? Log t?t c? admin actions
- ?? Gi?i h?n IP access cho admin
- ?? S? d?ng password m?nh (>= 12 ký t?, có ký t? ??c bi?t)

### Environment Variables (Production):
```bash
# Linux/Mac
export DefaultAdmin__Email="admin@studymate.com"
export DefaultAdmin__Password="YourSecurePassword"

# Windows PowerShell
$env:DefaultAdmin__Email="admin@studymate.com"
$env:DefaultAdmin__Password="YourSecurePassword"

# Docker
docker run -e DefaultAdmin__Email=admin@studymate.com \
           -e DefaultAdmin__Password=YourSecurePassword \
           studymate-api
```

## Troubleshooting

### Admin không ???c t?o
**Ki?m tra:**
1. Check console logs khi start app:
```
info: AdminSeederService[0]
      Default admin user created successfully: admin@studymate.com
```

2. Check database:
```sql
SELECT * FROM users WHERE role = 'admin';
```

3. N?u l?i:
```sql
-- Xóa admin c? và restart app
DELETE FROM users WHERE email = 'admin@studymate.com';
```

### Admin login failed
**Nguyên nhân:**
- Email/password sai
- Account b? ban
- Token configuration sai

**Gi?i pháp:**
```sql
-- Check admin status
SELECT email, role, is_banned FROM users WHERE email = 'admin@studymate.com';

-- Unban n?u c?n
UPDATE users SET is_banned = 0 WHERE email = 'admin@studymate.com';

-- Reset password
UPDATE users 
SET password = 'ReHashedPassword'
WHERE email = 'admin@studymate.com';
```

## Monitoring Admin Activity

```sql
-- Admin login count today
SELECT COUNT(*) as login_count
FROM refresh_tokens rt
JOIN users u ON rt.user_id = u.id
WHERE u.role = 'admin' 
AND rt.created_at >= CAST(GETDATE() AS DATE);

-- Last admin login
SELECT TOP 1 u.email, rt.created_at as last_login
FROM refresh_tokens rt
JOIN users u ON rt.user_id = u.id
WHERE u.role = 'admin'
ORDER BY rt.created_at DESC;

-- All admin users
SELECT id, email, full_name, created_at, is_banned
FROM users 
WHERE role = 'admin'
ORDER BY created_at;
```

## Next Steps

- [ ] Implement password change API
- [ ] Add admin activity logging
- [ ] Implement 2FA for admin
- [ ] Add IP whitelist for admin access
- [ ] Create admin dashboard
- [ ] Add email notification for admin login
