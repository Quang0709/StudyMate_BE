# Quick Start Guide - StudyMate Authentication

## ?? Default Admin Account

H? th?ng t? ??ng t?o tài kho?n admin m?c ??nh khi kh?i ??ng:

**Credentials:**
- **Email**: `admin@studymate.com`
- **Password**: `Admin@123456`
- **Role**: `admin`

?? **L?u ý b?o m?t**: ??i password ngay sau khi ??ng nh?p l?n ??u trong production!

### ??ng nh?p Admin:
```json
POST /api/auth/login
{
  "email": "admin@studymate.com",
  "password": "Admin@123456"
}
```

---

## B??c 1: Chu?n b? Database (5 phút)

1. M? SQL Server Management Studio
2. K?t n?i ??n SQL Server instance c?a b?n
3. Ch?y script t?o b?ng:

```sql
USE StudyMate;
GO

-- T?o b?ng refresh_tokens
CREATE TABLE refresh_tokens (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    token NVARCHAR(500) NOT NULL,
    expiry_date DATETIME NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    is_revoked BIT DEFAULT 0,
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_Token ON refresh_tokens(token);
CREATE INDEX IX_RefreshTokens_UserId ON refresh_tokens(user_id);

-- T?o b?ng otp_codes
CREATE TABLE otp_codes (
    id INT PRIMARY KEY IDENTITY(1,1),
    email VARCHAR(255) NOT NULL,
    code VARCHAR(6) NOT NULL,
    expiry_date DATETIME NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    is_used BIT DEFAULT 0
);

CREATE INDEX IX_OtpCodes_Email ON otp_codes(email);
CREATE INDEX IX_OtpCodes_Code ON otp_codes(code);
```

## B??c 2: C?u hình Gmail cho g?i OTP (10 phút)

### T?o App Password trong Gmail:

1. ??ng nh?p Gmail c?a b?n
2. Truy c?p: https://myaccount.google.com/security
3. B?t "2-Step Verification" (n?u ch?a b?t)
4. Truy c?p: https://myaccount.google.com/apppasswords
5. Ch?n app: "Mail", device: "Other" ? ??t tên "StudyMate"
6. Click "Generate" ? Copy mã 16 ký t?

### C?p nh?t appsettings.json:

M? file `StudyMate_BE/appsettings.json` và c?p nh?t:

```json
"EmailSettings": {
  "FromEmail": "your-email@gmail.com",      // ? Email c?a b?n
  "FromName": "StudyMate",
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "Username": "your-email@gmail.com",       // ? Email c?a b?n
  "Password": "xxxx xxxx xxxx xxxx"         // ? App Password (16 ký t?)
}
```

### (Optional) Thay ??i Admin m?c ??nh:

```json
"DefaultAdmin": {
  "Email": "admin@studymate.com",
  "Password": "Admin@123456",
  "FullName": "System Administrator"
}
```

## B??c 3: Ch?y ?ng d?ng (2 phút)

```bash
cd StudyMate_BE
dotnet restore
dotnet run
```

?ng d?ng s? t? ??ng:
1. ? T?o admin user n?u ch?a t?n t?i
2. ? M? browser v?i Swagger UI

**URLs:**
- **HTTPS**: `https://localhost:7259`
- **HTTP**: `http://localhost:5149`

Ho?c b?m F5 trong Visual Studio ?? ch?y.

## B??c 4: Test API v?i Swagger (5 phút)

Swagger UI s? t? ??ng m? khi ch?y ?ng d?ng. N?u không, truy c?p th? công:
- `https://localhost:7259` ho?c
- `http://localhost:5149`

### Test Login Admin:
   - Click `/api/auth/login`
   - Click "Try it out"
   - Paste:
   ```json
   {
     "email": "admin@studymate.com",
     "password": "Admin@123456"
   }
   ```
   - Click "Execute"
   - Copy `accessToken` t? response

### Test Authorize:
   - Click nút "Authorize" ? ??u trang
   - Nh?p: `Bearer {paste-access-token-here}`
   - Click "Authorize"
   - Th? g?i `/api/auth/me` ? S? th?y role: "admin"

### Test Register Student/Teacher:
   - Click `/api/auth/register`
   - Click "Try it out"
   - Paste:
   ```json
   {
     "fullName": "Test User",
     "email": "your-real-email@gmail.com",
     "password": "Test123!",
     "role": "student"
   }
   ```
   - Click "Execute"
   - Check email ?? l?y OTP

### Test Verify OTP:
   - Click `/api/auth/verify-otp`
   - Paste:
   ```json
   {
     "email": "your-real-email@gmail.com",
     "code": "123456"
   }
   ```
   - Click "Execute"
   - Copy `accessToken` t? response

## Lu?ng test nhanh:

```
1. Login Admin (test ngay không c?n OTP)
   ?
2. Register Student/Teacher (email th?t ?? nh?n OTP)
   ?
3. Check email ? l?y mã OTP
   ?
4. Verify OTP ? nh?n tokens
   ?
5. Login Student/Teacher
   ?
6. Call /me (test protected endpoint)
   ?
7. Refresh token (sau 15 phút)
   ?
8. Logout
```

## Troubleshooting nhanh:

### Admin không ???c t?o
- ? Check console logs khi start app
- ? Check database: `SELECT * FROM users WHERE role = 'admin'`
- ? N?u l?i, xóa user c? và restart app

### Swagger không m?
- ? Ch?y b?ng `dotnet run` ho?c F5 trong Visual Studio
- ? Truy c?p th? công: `https://localhost:7259`
- ? Ki?m tra firewall không block port

### "Failed to send OTP email"
- ? Check email settings trong appsettings.json
- ? Dùng App Password, không dùng password Gmail th??ng
- ? B?t "2-Step Verification" trong Gmail

### "Invalid email or password"
- ? Password ph?i có ít nh?t 6 ký t?
- ? Email ph?i ?úng format

### "OTP has expired"
- ? OTP ch? có hi?u l?c 10 phút
- ? Register l?i ?? nh?n OTP m?i

### "Email is already registered"
- ? M?i email ch? ??ng ký 1 l?n
- ? Dùng email khác ho?c xóa user trong database

### Token không work
- ? Check format: "Bearer {token}" (có d?u cách)
- ? Token expire sau 15 phút, dùng refresh token

## Ki?m tra Database:

```sql
-- Check admin user
SELECT * FROM users WHERE role = 'admin';

-- Check users ?ã ??ng ký
SELECT * FROM users;

-- Check students/teachers
SELECT * FROM students;
SELECT * FROM teachers;

-- Check OTP codes
SELECT * FROM otp_codes ORDER BY created_at DESC;

-- Check refresh tokens
SELECT * FROM refresh_tokens WHERE is_revoked = 0;
```

## Production Checklist:

- [ ] **??I ADMIN PASSWORD** (quan tr?ng!)
- [ ] ??i JWT SecretKey thành key m?nh h?n
- [ ] L?u secrets trong environment variables
- [ ] Enable HTTPS
- [ ] Configure CORS ?úng domain
- [ ] Add rate limiting
- [ ] Setup logging và monitoring
- [ ] Backup database
- [ ] Test v?i nhi?u users
- [ ] Disable admin auto-seed trong production

## Support:

N?u g?p v?n ??:
1. Check console logs
2. Check SQL Server logs
3. Check email settings
4. ??c README_AUTH.md ?? bi?t chi ti?t

Happy coding! ??
