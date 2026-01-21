# H??ng d?n ch?y Migration cho Database

## B??c 1: T?o b?ng RefreshTokens và OtpCodes

### S? d?ng SQL Script (Khuy?n ngh?)
1. M? SQL Server Management Studio ho?c Azure Data Studio
2. K?t n?i ??n database `StudyMate`
3. M? file `Database/Migrations/AddAuthTables.sql`
4. Execute script

### Ho?c s? d?ng Entity Framework Migrations

#### 1. Cài ??t EF Core tools (n?u ch?a có)
```bash
dotnet tool install --global dotnet-ef
```

#### 2. Add Migration
```bash
cd Repository
dotnet ef migrations add AddAuthenticationTables --startup-project ../StudyMate_BE
```

#### 3. Update Database
```bash
dotnet ef database update --startup-project ../StudyMate_BE
```

## B??c 2: Xác nh?n b?ng ?ã ???c t?o

Ch?y query sau ?? ki?m tra:

```sql
USE StudyMate;
GO

-- Ki?m tra b?ng refresh_tokens
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'refresh_tokens';

-- Ki?m tra b?ng otp_codes
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'otp_codes';

-- Ki?m tra c?u trúc b?ng
EXEC sp_help 'refresh_tokens';
EXEC sp_help 'otp_codes';
```

## B??c 3: Test d? li?u (Optional)

### Insert test OTP
```sql
INSERT INTO otp_codes (email, code, expiry_date, is_used)
VALUES ('test@example.com', '123456', DATEADD(MINUTE, 10, GETDATE()), 0);

SELECT * FROM otp_codes;
```

### Xóa test data
```sql
DELETE FROM otp_codes WHERE email = 'test@example.com';
```

## Troubleshooting

### L?i: Database does not exist
```sql
-- T?o database n?u ch?a có
CREATE DATABASE StudyMate;
GO
USE StudyMate;
```

### L?i: Foreign key constraint
- ??m b?o b?ng `users` ?ã t?n t?i tr??c khi t?o `refresh_tokens`
- Ki?m tra column `id` trong b?ng `users` là PRIMARY KEY

### L?i: Connection string
- Ki?m tra connection string trong `appsettings.json`
- ??m b?o SQL Server ?ang ch?y
- Test connection v?i SQL Server Management Studio

## Rollback (n?u c?n)

```sql
-- Xóa b?ng refresh_tokens
DROP TABLE IF EXISTS refresh_tokens;

-- Xóa b?ng otp_codes
DROP TABLE IF EXISTS otp_codes;
```
