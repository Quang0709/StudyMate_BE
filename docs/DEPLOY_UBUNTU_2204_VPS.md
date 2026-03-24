# Deploy StudyMate BE lên Ubuntu 22.04 VPS

Thông tin đang dùng trong mẫu này:
- VPS IP: `14.225.212.91`
- FE domain: `https://fptstudymate.id.vn`
- API domain đề xuất: `api.fptstudymate.id.vn`

> Bạn cần trỏ DNS A record của `api.fptstudymate.id.vn` về `14.225.212.91` trước khi xin SSL.

---

## 1) Chuẩn bị cấu hình appsettings

File cần sửa: `StudyMate_BE/StudyMate_BE/appsettings.json`

Điền các placeholder:
- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`, `Issuer`, `Audience`
- `EmailSettings:*`
- `PayOS:*`
- `GeminiAI:ApiKey`
- `DefaultAdmin:*`

Đã có sẵn:
- `AppSettings:FrontendBaseUrl = https://fptstudymate.id.vn`
- `AppSettings:BaseUrl = https://api.fptstudymate.id.vn`
- `Cors:AllowedOrigins` cho domain FE

> Dự án chạy với `ASPNETCORE_ENVIRONMENT=Production` vẫn dùng bình thường khi chỉ có `appsettings.json`.

---

## 2) Publish BE từ local

Chạy ở máy local (Windows):

```powershell
Set-Location d:\EXE\StudyMate_BE

dotnet publish .\StudyMate_BE\StudyMate_BE.csproj -c Release -o .\publish
```

---

## 3) Copy build lên VPS

```powershell
scp -r d:\EXE\StudyMate_BE\publish\* <VPS_USER>@14.225.212.91:/var/www/studymate-api/
```

Nếu thư mục chưa có, tạo trên VPS:

```bash
sudo mkdir -p /var/www/studymate-api
sudo chown -R www-data:www-data /var/www/studymate-api
```

---

## 4) Cài runtime và nginx trên VPS

SSH vào VPS rồi chạy:

```bash
sudo apt update
sudo apt install -y nginx

# Cài ASP.NET runtime 9.0 (nếu repo Microsoft chưa có thì thêm repo trước)
sudo apt install -y aspnetcore-runtime-9.0
```

Kiểm tra dotnet:

```bash
dotnet --info
```

---

## 5) Tạo systemd service

Copy file mẫu trong repo:
- `StudyMate_BE/deploy/systemd/studymate-api.service`

Nội dung đẩy lên VPS:

```bash
sudo cp /var/www/studymate-api/studymate-api.service /etc/systemd/system/studymate-api.service
```

Nếu bạn không copy kèm file này thì tạo trực tiếp bằng nano:

```bash
sudo nano /etc/systemd/system/studymate-api.service
```

Dán nội dung từ file mẫu rồi chạy:

```bash
sudo systemctl daemon-reload
sudo systemctl enable studymate-api
sudo systemctl start studymate-api
sudo systemctl status studymate-api
```

Xem log realtime:

```bash
journalctl -u studymate-api -f
```

---

## 6) Cấu hình Nginx reverse proxy

File mẫu trong repo:
- `StudyMate_BE/deploy/nginx/api.fptstudymate.id.vn.conf`

Trên VPS:

```bash
sudo cp /var/www/studymate-api/api.fptstudymate.id.vn.conf /etc/nginx/sites-available/api.fptstudymate.id.vn
sudo ln -s /etc/nginx/sites-available/api.fptstudymate.id.vn /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 7) Cấp SSL bằng Let's Encrypt

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.fptstudymate.id.vn
```

Kiểm tra auto renew:

```bash
sudo systemctl status certbot.timer
```

---

## 8) Mở firewall (nếu dùng UFW)

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
sudo ufw status
```

---

## 9) Smoke test

- Test API qua domain:

```bash
curl -I https://api.fptstudymate.id.vn
```

- Nếu API có endpoint health/public thì test endpoint đó.

- Kiểm tra FE gọi API:
  - FE env trên Vercel đặt `VITE_API_BASE_URL=https://api.fptstudymate.id.vn`

---

## 10) Checklist bảo mật bắt buộc

- Không commit secret thật vào git.
- Đổi toàn bộ secret đã từng lộ: DB, SMTP, JWT, PayOS, Gemini.
- `JwtSettings:SecretKey` tối thiểu 32 ký tự mạnh.
- Chỉ cho phép CORS từ FE domain thật.
