param(
  [string]$SshUser = "studymate-shmg",
  [string]$ServerIp = "14.225.212.91",
  [string]$ApiDomain = "api.fptstudymate.id.vn"
)

$ErrorActionPreference = "Stop"

Write-Host "[1/5] Publish backend"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet publish .\StudyMate_BE\StudyMate_BE.csproj -c Release -o .\publish

Write-Host "[2/5] Ensure setup script is executable payload"
$setupScript = Join-Path $PSScriptRoot "vps-setup.sh"
if (-not (Test-Path $setupScript)) {
  throw "Missing deploy/vps-setup.sh"
}

Write-Host "[3/5] Upload files to VPS (you will be prompted for password)"
ssh -o StrictHostKeyChecking=accept-new "$SshUser@${ServerIp}" "mkdir -p /var/www/studymate-api /tmp"
scp -o StrictHostKeyChecking=accept-new -r .\publish\* "$SshUser@${ServerIp}:/var/www/studymate-api/"
scp -o StrictHostKeyChecking=accept-new "$setupScript" "$SshUser@${ServerIp}:/tmp/vps-setup.sh"

Write-Host "[4/5] Run server setup + service"
ssh -o StrictHostKeyChecking=accept-new "$SshUser@${ServerIp}" "chmod +x /tmp/vps-setup.sh; API_DOMAIN=$ApiDomain /tmp/vps-setup.sh"

Write-Host "[5/5] SSL setup"
ssh -o StrictHostKeyChecking=accept-new "$SshUser@${ServerIp}" "sudo certbot --nginx -d $ApiDomain"

Write-Host "Deploy completed. Test: https://$ApiDomain"
