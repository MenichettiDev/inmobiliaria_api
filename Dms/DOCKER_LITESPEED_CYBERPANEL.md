# DOCKER_LITESPEED_CYBERPANEL.md — Guía Docker en VPS con CyberPanel + LiteSpeed

## 🔍 Situación Actual

**Tu VPS tiene:**
```
CyberPanel (Panel de control)
└─ LiteSpeed (Servidor web)
    ├─ Dominios
    ├─ SSL/TLS
    ├─ Backups
    └─ ...
```

**Con Docker:**
```
Docker containers:
├─ Backend (ASP.NET)
├─ Frontend (Nginx)
├─ MariaDB
└─ Redis

LiteSpeed seguirá en la VPS
```

---

## 🎯 3 Opciones Principales

### OPCIÓN 1: Docker Puro (Recomendado - Moderno)

Elimina LiteSpeed/CyberPanel y usa solo Docker

```
✅ Pros:
- Más control
- Menor overhead (sin LiteSpeed)
- Mejor para multi-tenant
- Fácil reproducir en otra VPS
- Git-based deploy (más moderno)

❌ Contras:
- Pierdes GUI de CyberPanel
- Tienes que manejar SSL manualmente
- Tienes que hacer backups con scripts
```

**Setup:**
```bash
# En la VPS:
git clone https://github.com/tuusuario/inmobiliaria_saas.git
cd inmobiliaria_saas

# Copiar variables (IMPORTANT: no from git)
cp .env.docker .env.prod
# Editar con valores reales

# Levantar servicios
docker-compose up -d

# SSL automático con Let's Encrypt
sudo certbot certonly --standalone -d tudominio.com -d acme.tudominio.com
# Copiar certificados a Docker
```

---

### OPCIÓN 2: LiteSpeed como Proxy Reverso (Híbrido)

Mantener CyberPanel/LiteSpeed pero usarlo para proxear hacia Docker

```
Usuario
  └─ LiteSpeed:80/443 (proxy)
      ├─ /api/      → Docker Backend :2000
      ├─ /         → Docker Frontend :80
      └─ SSL via CyberPanel
```

**Pros:**
- Mantienes GUI de CyberPanel
- Seguís usando SSL de CyberPanel
- Backups integrados
- Menos cambios

**Contras:**
- Overhead de LiteSpeed
- Configuración más compleja
- LiteSpeed no está optimizado para ser proxy

---

### OPCIÓN 3: Minimal (Usar CyberPanel solo para SSL/DNS)

Mantener CyberPanel activo pero Docker maneja todo

```
CyberPanel:
├─ Gestiona dominios
├─ Genera certificados SSL
├─ Hace backups
└─ Panel control

Docker:
├─ Corre aplicación
├─ Corre frontend
└─ Corre BD
```

---

## ⚡ RECOMENDACIÓN: OPCIÓN 1 (Docker Puro)

**Por qué:**
- Para un SaaS moderno, Docker es mejor
- Multi-tenant es más fácil
- Escalabilidad horizontal
- CI/CD automatizado
- Costo: mismo VPS, menos overhead

---

## 🚀 Implementación OPCIÓN 1: Docker Puro

### Paso 1: Preparar la VPS

```bash
# SSH a tu VPS
ssh root@tu_vps.com

# Actualizar sistema
apt update && apt upgrade -y

# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Instalar Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verificar
docker --version
docker-compose --version

# Opcional: Certbot para SSL
apt install certbot python3-certbot-nginx -y
```

### Paso 2: Clonar Proyecto

```bash
# Como root o con sudo
cd /opt
git clone https://github.com/tuusuario/inmobiliaria_saas.git
cd inmobiliaria_saas

# Crear .env.prod con valores REALES
cp .env.docker .env.prod
nano .env.prod

# Contenido ejemplo:
# DB_PASSWORD=very_secure_password_123
# JWT_SECRET=your_jwt_secret_key_here
# CLOUDFLARE_API_TOKEN=your_token
# etc...

# Proteger archivo
chmod 600 .env.prod
```

### Paso 3: Configurar docker-compose.yml

```yaml
# Editar docker-compose.yml - Cambios específicos para producción:

services:
  mariadb:
    # ... resto igual
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD}
      MYSQL_PASSWORD: ${DB_PASSWORD}
    ports:
      - "127.0.0.1:3306:3306"  # ← Solo localhost
    volumes:
      - mariadb_data:/var/lib/mysql
      - /backups/mysql:/backups  # ← Ruta de backups

  backend:
    # ... resto igual
    environment:
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "127.0.0.1:2000:2000"  # ← Solo localhost
    volumes:
      - ./inmobiliaria_api:/app/src:ro  # ← Read-only en prod

  frontend:
    # ... resto igual
    ports:
      - "127.0.0.1:80:80"      # ← Solo localhost
      - "127.0.0.1:443:443"
```

### Paso 4: Obtener SSL con Let's Encrypt

```bash
# Crear certificados para dominio principal + subdominio
sudo certbot certonly --standalone \
  -d tudominio.com \
  -d www.tudominio.com \
  -d *.tudominio.com

# Los certificados estarán en:
# /etc/letsencrypt/live/tudominio.com/

# Copiar a carpeta de Docker
mkdir -p ./certs
sudo cp /etc/letsencrypt/live/tudominio.com/fullchain.pem ./certs/
sudo cp /etc/letsencrypt/live/tudominio.com/privkey.pem ./certs/
sudo chown 1000:1000 ./certs/*
```

### Paso 5: Configurar Nginx para SSL

**Archivo:** `inmobiliaria_front/nginx.conf` (actualizar)

```nginx
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    access_log /var/log/nginx/access.log;

    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;

    gzip on;
    gzip_types text/plain text/css text/javascript application/json;

    # HTTP → HTTPS redirige
    server {
        listen 80;
        listen [::]:80;
        server_name _;

        location /.well-known/acme-challenge/ {
            root /var/www/certbot;
        }

        location / {
            return 301 https://$host$request_uri;
        }
    }

    # HTTPS
    server {
        listen 443 ssl http2;
        listen [::]:443 ssl http2;
        server_name _;

        # SSL Certificates
        ssl_certificate /etc/nginx/certs/fullchain.pem;
        ssl_certificate_key /etc/nginx/certs/privkey.pem;

        # SSL Config (recomendado)
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;
        ssl_prefer_server_ciphers on;
        ssl_session_cache shared:SSL:10m;
        ssl_session_timeout 10m;

        root /usr/share/nginx/html;
        index index.html;

        # SPA routing
        location / {
            try_files $uri $uri/ /index.html;
            add_header Cache-Control "public, max-age=3600";
        }

        # Assets
        location ~* ^/assets/ {
            expires 30d;
            add_header Cache-Control "public, immutable";
        }

        # API proxy
        location /api/ {
            proxy_pass http://backend:2000;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Health
        location /health.html {
            return 200 "ok";
            add_header Content-Type text/plain;
        }
    }
}
```

**Actualizar docker-compose.yml:**

```yaml
frontend:
  build:
    context: ./inmobiliaria_front
  ports:
    - "127.0.0.1:80:80"
    - "127.0.0.1:443:443"
  volumes:
    - ./inmobiliaria_front/nginx.conf:/etc/nginx/nginx.conf:ro
    - ./certs:/etc/nginx/certs:ro  # ← Certificados SSL
  depends_on:
    - backend
```

### Paso 6: Renovar SSL Automáticamente

**Script:** `scripts/renew-ssl.sh`

```bash
#!/bin/bash

# Renovar certificados
sudo certbot renew --quiet

# Copiar certificados actualizados
sudo cp /etc/letsencrypt/live/tudominio.com/fullchain.pem /opt/inmobiliaria_saas/certs/
sudo cp /etc/letsencrypt/live/tudominio.com/privkey.pem /opt/inmobiliaria_saas/certs/
sudo chown 1000:1000 /opt/inmobiliaria_saas/certs/*

# Recargar Nginx
cd /opt/inmobiliaria_saas
docker-compose exec frontend nginx -s reload

echo "SSL renovado: $(date)" >> /var/log/ssl-renewal.log
```

**Agregar cron job (ejecuta cada 2 meses):**

```bash
sudo crontab -e

# Agregar línea:
0 3 1 * * /opt/inmobiliaria_saas/scripts/renew-ssl.sh
```

### Paso 7: Levantar Servicios

```bash
cd /opt/inmobiliaria_saas

# Build images
docker-compose build

# Levantar en background
docker-compose up -d

# Ver estado
docker-compose ps

# Ver logs
docker-compose logs -f backend
docker-compose logs -f frontend

# Verificar conectividad
curl http://localhost:2000/health      # Backend
curl http://localhost/health.html      # Frontend
```

### Paso 8: Crear Base de Datos (Migraciones)

```bash
# Ejecutar migrations
docker-compose exec backend dotnet ef database update

# Crear usuario Admin inicial
docker-compose exec backend dotnet run -- seed-admin
# (requiere seed script en backend)
```

---

## 🔄 Renovación Automática de SSL

```bash
# Configurar renovación automática de Certbot
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

# Verificar
sudo systemctl status certbot.timer
```

---

## 📊 OPCIÓN 2: LiteSpeed como Proxy (Si quieres mantener CyberPanel)

Si **prefieres mantener LiteSpeed/CyberPanel**, aquí va:

### Configuración en LiteSpeed

**En CyberPanel:**
1. Crear dominio normal (tudominio.com)
2. Generar SSL via CyberPanel
3. Ir a: LiteSpeed WebConsole → Static Context

**Crear contexto para proxy:**

```
URI: /api/
Location: N/A
Allowbrowse: No
Enable Expires: Yes
Passthrough:
  Engine: Proxy
  Address: http://127.0.0.1:2000  ← Tu backend Docker
```

**Crear contexto para frontend:**

```
URI: /
Location: /var/www/html  ← Donde sirves frontend
Allowbrowse: No
Index Files: index.html
```

**Problema:** Frontend es SPA (Angular), necesita rewrite de URL

```
# En LiteSpeed:
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule ^/(.*)$ /index.html [L,QSA]
```

**Complejidad:** Media-Alta

---

## 🗂️ Estructura Final (OPCIÓN 1 - Docker Puro)

```
/opt/inmobiliaria_saas/
├── docker-compose.yml
├── .env.prod (gitignore)
├── certs/
│   ├── fullchain.pem
│   └── privkey.pem
├── inmobiliaria_api/
│   ├── Dockerfile
│   └── ... (código)
├── inmobiliaria_front/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── ... (código)
├── scripts/
│   └── renew-ssl.sh
├── backups/
│   ├── mysql/
│   └── app/
└── logs/
    ├── backend/
    └── frontend/
```

---

## ✅ Checklist para Implementación

- [ ] SSH a VPS
- [ ] Instalar Docker + Docker Compose
- [ ] Clonar repositorio
- [ ] Crear `.env.prod` con valores reales
- [ ] Generar certificados SSL con Certbot
- [ ] Copiar certs a `/opt/inmobiliaria_saas/certs/`
- [ ] Actualizar `nginx.conf` con SSL
- [ ] Actualizar `docker-compose.yml` (puertos localhost)
- [ ] Crear `scripts/renew-ssl.sh`
- [ ] Configurar cron para renovación
- [ ] Build: `docker-compose build`
- [ ] Levantar: `docker-compose up -d`
- [ ] Verificar: `docker-compose ps`
- [ ] Probar: `curl https://tudominio.com`
- [ ] Ejecutar migraciones DB
- [ ] Crear usuario admin

---

## 🚨 Consideraciones Importantes

### 1. Desinstalar CyberPanel (si vas OPCIÓN 1)

```bash
# BACKUP PRIMERO!
# Backup de datos
mysqldump -u root -p --all-databases > /backups/all_databases.sql

# Luego:
/opt/cyberpanel/uninstall.sh

# O simplemente no lo uses, corre Docker en paralelo
```

### 2. Ports en la VPS

```
LiteSpeed corre en:     :7080 (console), :80, :443
Docker Frontend:        :80, :443 (conflicto)

Solución:
- Desinstalar LiteSpeed
- O correr Docker en puertos diferentes (:8080, :8443)
```

### 3. Backups

**Sin CyberPanel, necesitas script de backup:**

```bash
#!/bin/bash
# /opt/inmobiliaria_saas/scripts/backup.sh

BACKUP_DIR="/backups/$(date +%Y-%m-%d)"
mkdir -p $BACKUP_DIR

# Backup BD
docker-compose exec -T mariadb \
  mysqldump -u root -p${DB_ROOT_PASSWORD} --all-databases \
  > $BACKUP_DIR/databases.sql

# Backup volúmenes
docker run --rm \
  -v inmobiliaria_saas_mariadb_data:/data \
  -v $BACKUP_DIR:/backup \
  alpine tar czf /backup/db_volume.tar.gz -C /data .

echo "Backup realizado: $BACKUP_DIR"
```

**Agregar a cron:**

```bash
# Backup diario a las 2 AM
0 2 * * * /opt/inmobiliaria_saas/scripts/backup.sh
```

---

## 🎯 Recomendación Final

| Opción | Caso Ideal | Setup | Mantenimiento |
|--------|-----------|-------|---------------|
| **OPCIÓN 1** (Docker Puro) | SaaS moderno, multi-tenant | 2-3h | Minimal (automation) |
| **OPCIÓN 2** (LiteSpeed Proxy) | Mantener CyberPanel | 3-4h | Medio (dos sistemas) |
| **OPCIÓN 3** (CyberPanel + Docker) | Transición gradual | 1h | Alto (dos sistemas) |

**Para tu caso (SaaS inmobiliaria):**
```
→ OPCIÓN 1 (Docker Puro) ✅
Razones:
- Multi-tenant es más fácil con Docker
- Escalabilidad horizontal
- Menor overhead
- Más moderno y profesional
```

---

## 🆘 Problemas Comunes

### "Connection refused: localhost:2000"

```bash
# Verificar que backend corre
docker-compose ps

# Si no está running, ver logs
docker-compose logs backend

# Rebuild
docker-compose build --no-cache backend
docker-compose up -d
```

### "SSL certificate not found"

```bash
# Verificar certificados
ls -la ./certs/

# Regenerar
sudo certbot certonly --standalone -d tudominio.com
sudo cp /etc/letsencrypt/live/tudominio.com/* ./certs/
sudo chown 1000:1000 ./certs/*
```

### "Docker: command not found"

```bash
# Reinstalar Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
newgrp docker
```

---

**¿Necesitas ayuda con algún paso específico?** 🚀
