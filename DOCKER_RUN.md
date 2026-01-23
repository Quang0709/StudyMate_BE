# Hướng dẫn chạy Docker cho StudyMate BE

## ✅ Trạng thái hiện tại

- ✅ Docker image đã được build thành công
- ✅ API container đang chạy trên port 8080
- ✅ Kết nối với SQL Server container hiện có thành công
- ✅ Database đã được tạo và admin user đã được seed

## 📋 Thông tin kết nối

### API Container
- **Container name**: `studymate-api`
- **Port**: `8080`
- **URL**: http://localhost:8080
- **Status**: Đang chạy ✅

### SQL Server Container (hiện có)
- **Container name**: `quirky_jones`
- **IP**: `172.17.0.2`
- **Port**: `1433`
- **Username**: `sa`
- **Password**: `123456aA@`
- **Database**: `StudyMate`

## 🚀 Các lệnh hữu ích

### Xem trạng thái containers
```powershell
docker ps
```

### Xem logs của API
```powershell
docker logs studymate-api
docker logs studymate-api -f  # Theo dõi logs real-time
```

### Dừng API container
```powershell
docker stop studymate-api
```

### Khởi động lại API container
```powershell
docker start studymate-api
```

### Xóa và tạo lại API container
```powershell
docker stop studymate-api
docker rm studymate-api
docker run -d --name studymate-api --network bridge -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Docker -e ASPNETCORE_URLS=http://+:8080 -e "ConnectionStrings__DefaultConnection=Server=172.17.0.2;Database=StudyMate;User Id=sa;Password=123456aA@;TrustServerCertificate=True;MultipleActiveResultSets=True" studymate_be-api
```

### Rebuild Docker image
```powershell
docker-compose build
# hoặc
docker build -t studymate_be-api .
```

### Vào bên trong container
```powershell
docker exec -it studymate-api bash
```

### Kiểm tra network connectivity
```powershell
docker exec studymate-api ping -c 3 172.17.0.2
```

## 🔧 Troubleshooting

### Nếu API không kết nối được với SQL Server

1. **Kiểm tra SQL Server container có đang chạy:**
```powershell
docker ps --filter "name=quirky_jones"
```

2. **Kiểm tra IP của SQL Server container:**
```powershell
docker inspect quirky_jones --format='{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'
```

3. **Nếu IP thay đổi, cập nhật connection string và restart API:**
```powershell
docker stop studymate-api
docker rm studymate-api
# Chạy lại với IP mới
docker run -d --name studymate-api --network bridge -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Docker -e ASPNETCORE_URLS=http://+:8080 -e "ConnectionStrings__DefaultConnection=Server=<IP_MỚI>;Database=StudyMate;User Id=sa;Password=123456aA@;TrustServerCertificate=True;MultipleActiveResultSets=True" studymate_be-api
```

### Nếu port 8080 đã được sử dụng

Thay đổi port mapping:
```powershell
docker run -d --name studymate-api --network bridge -p 8081:8080 ...
```

## 📝 Lưu ý

1. **IP của SQL Server container có thể thay đổi** khi container được restart. Nếu gặp lỗi kết nối, kiểm tra lại IP.

2. **Để tránh vấn đề IP thay đổi**, bạn có thể:
   - Sử dụng docker-compose để quản lý cả 2 containers cùng lúc
   - Hoặc tạo một custom network và kết nối cả 2 containers vào đó

3. **Swagger UI** chỉ được bật trong Development mode. Trong Docker environment, bạn cần truy cập các API endpoints trực tiếp.

## 🎯 Test API

Sau khi API đã chạy, bạn có thể test các endpoints:

```powershell
# Test health check (nếu có)
curl http://localhost:8080/api/health

# Test register endpoint
curl -X POST http://localhost:8080/api/auth/register -H "Content-Type: application/json" -d '{"email":"test@example.com","password":"Test123!","fullName":"Test User"}'
```

## 📚 Thông tin thêm

- Xem file `DOCKER_SETUP.md` để biết cách setup Docker từ đầu
- Xem file `docker-compose.yml` để biết cách chạy cả API và SQL Server cùng lúc
