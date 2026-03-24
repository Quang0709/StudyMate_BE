#!/usr/bin/env bash
set -euo pipefail

API_DOMAIN="${API_DOMAIN:-api.fptstudymate.id.vn}"
APP_DIR="/var/www/studymate-api"
SERVICE_NAME="studymate-api"

echo "[1/7] Update apt and install nginx + certbot"
sudo apt update
sudo apt install -y nginx certbot python3-certbot-nginx

echo "[2/7] Install Microsoft package repo if needed"
if ! command -v dotnet >/dev/null 2>&1; then
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
  sudo dpkg -i /tmp/packages-microsoft-prod.deb
  sudo apt update
fi

echo "[3/7] Install ASP.NET Core runtime 9.0"
sudo apt install -y aspnetcore-runtime-9.0

echo "[4/7] Prepare application directory"
sudo mkdir -p "$APP_DIR"
sudo chown -R www-data:www-data "$APP_DIR"

echo "[5/7] Install systemd service"
sudo tee /etc/systemd/system/${SERVICE_NAME}.service >/dev/null <<'EOF'
[Unit]
Description=StudyMate Backend API
After=network.target

[Service]
WorkingDirectory=/var/www/studymate-api
ExecStart=/usr/bin/dotnet /var/www/studymate-api/StudyMate_BE.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=studymate-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000

[Install]
WantedBy=multi-user.target
EOF

echo "[6/7] Install nginx config"
sudo tee /etc/nginx/sites-available/${API_DOMAIN} >/dev/null <<EOF
server {
    listen 80;
    server_name ${API_DOMAIN};

    client_max_body_size 50M;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_read_timeout 300;
    }

    location /hub/chat {
        proxy_pass http://127.0.0.1:5000/hub/chat;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/${API_DOMAIN} /etc/nginx/sites-enabled/${API_DOMAIN}
sudo nginx -t
sudo systemctl reload nginx

echo "[7/7] Enable service"
sudo systemctl daemon-reload
sudo systemctl enable ${SERVICE_NAME}
sudo systemctl restart ${SERVICE_NAME}
sudo systemctl status ${SERVICE_NAME} --no-pager

echo "Setup done. If DNS is ready, run SSL command:"
echo "sudo certbot --nginx -d ${API_DOMAIN}"
