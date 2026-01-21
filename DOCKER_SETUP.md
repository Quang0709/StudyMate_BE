# Hướng dẫn Setup Docker cho StudyMate BE

## Yêu cầu
- Docker Desktop đã được cài đặt và đang chạy
- Docker Compose (thường đi kèm với Docker Desktop)

## Cấu trúc Docker

### Files đã tạo:
1. **Dockerfile** - Build và chạy .NET 9.0 application
2. **docker-compose.yml** - Orchestrate API và SQL Server containers
3. **.dockerignore** - Loại trừ các files không cần thiết khi build
4. **appsettings.Docker.json** - Cấu hình cho môi trường Docker

## Cách sử dụng

### 1. Build và chạy với Docker Compose (Khuyến nghị)
```bash
# Build và chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f api

# Dừng tất cả services
docker-compose down

# Dừng và xóa volumes (xóa database)
docker-compose down -v
```

### 2. Build và chạy thủ công

#### Build Docker image:
```bash
docker build -t studymate-api .
```

#### Chạy SQL Server container:
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password123" \
   -p 1433:1433 --name studymate-sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

#### Chạy API container:
```bash
docker run -p 8080:8080 --name studymate-api \
   -e ASPNETCORE_ENVIRONMENT=Docker \
   -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=StudyMate;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True" \
   --link studymate-sqlserver:sqlserver \
   -d studymate-api
```

## Truy cập Application

- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080 (khi chạy ở Development mode)
- **SQL Server**: localhost:1433
  - Username: `sa`
  - Password: `YourStrong@Password123`

## Database Setup

Sau khi containers đã chạy, bạn cần chạy migrations để tạo database schema:

```bash
# Chạy migration từ bên trong API container
docker exec -it studymate-api dotnet ef database update

# Hoặc copy migration scripts vào SQL Server container và chạy
docker cp Database/Migrations/AddAuthTables.sql studymate-sqlserver:/tmp/
docker exec -it studymate-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -i /tmp/AddAuthTables.sql
```

## Environment Variables

Bạn có thể override các biến môi trường trong `docker-compose.yml`:

- `MSSQL_SA_PASSWORD`: Password cho SQL Server SA user
- `ConnectionStrings__DefaultConnection`: Connection string cho database
- `JwtSettings__SecretKey`: JWT secret key
- Các EmailSettings: Cấu hình email service

## Troubleshooting

### Kiểm tra containers đang chạy:
```bash
docker ps
```

### Xem logs của container:
```bash
docker logs studymate-api
docker logs studymate-sqlserver
```

### Vào bên trong container:
```bash
docker exec -it studymate-api bash
docker exec -it studymate-sqlserver bash
```

### Kiểm tra network connectivity:
```bash
docker network inspect studymate-network
```

### Xóa và rebuild:
```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

## Lưu ý bảo mật

⚠️ **QUAN TRỌNG**: Trong production, bạn nên:
1. Thay đổi password mặc định của SQL Server
2. Sử dụng Docker secrets hoặc environment variables từ file .env
3. Không commit các file chứa sensitive information
4. Sử dụng HTTPS cho API
5. Cấu hình firewall và network security

## Tùy chỉnh

### Thay đổi ports:
Chỉnh sửa `docker-compose.yml`:
```yaml
ports:
  - "YOUR_PORT:8080"  # Thay YOUR_PORT bằng port bạn muốn
```

### Thay đổi database password:
1. Cập nhật `MSSQL_SA_PASSWORD` trong `docker-compose.yml`
2. Cập nhật connection string trong `appsettings.Docker.json`
