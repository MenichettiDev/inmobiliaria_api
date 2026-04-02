# DOCKER_SETUP.md — Guía Completa Docker para SaaS Inmobiliaria

## 📚 Introducción a Docker

**Docker** es una plataforma que **empaqueta tu aplicación con todas sus dependencias** en un contenedor aislado.

### ¿Por qué Docker?

```
SIN Docker:
- "Funciona en mi máquina"
- Configuraciones diferentes en dev/test/prod
- Dependencias conflictivas
- Setup manual complejo
- Deploy lento y propenso a errores

CON Docker:
✅ Mismo contenedor en dev/staging/prod
✅ Reproducible 100%
✅ Deploy en segundos
✅ Escalable horizontalmente
✅ CI/CD automatizado
✅ Fácil rollback si hay problema
```

---

## 🎯 Beneficios para Producción

### 1. **Deploy Consistente**
```
Local:          Docker → Mismo en Prod
Dev:            Docker → Mismo en Prod
Staging:        Docker → Mismo en Prod
Producción:     Docker → Garantizado funciona

Antes: "Funciona en mi PC pero no en servidor"
Después: Funciona en 100% de lugares
```

### 2. **Escalabilidad Horizontal**
```
Usuario 1:   docker run imagen → Puerto 3001
Usuario 2:   docker run imagen → Puerto 3002
Usuario 3:   docker run imagen → Puerto 3003

Load Balancer distribuye traffic automáticamente
```

### 3. **Aislamiento de Servicios**
```
Container Backend (ASP.NET Core):    Puerto 2000, memoria limitada
Container Frontend (Angular/Nginx):  Puerto 80/443
Container Database (MariaDB):        Puerto 3306
Container Redis (Cache):             Puerto 6379

Cada uno independiente, no interfieren
```

### 4. **Rollback Instantáneo**
```
Nueva versión mala → docker pull version-anterior
Downtime: < 30 segundos
```

### 5. **CI/CD Automatizado**
```
Push a GitHub
→ GitHub Actions ejecuta tests
→ Build imagen Docker
→ Push a registry (DockerHub, ECR)
→ Deploy automático a producción
→ Monitoreo automático
```

---

## 📁 Estructura de Archivos Necesarios

```
inmobiliaria_saas/
├── inmobiliaria_api/
│   ├── Dockerfile                 ← CREAR
│   ├── .dockerignore               ← CREAR
│   ├── docker-entrypoint.sh        ← CREAR (opcional)
│   └── ... (código existente)
│
├── inmobiliaria_front/
│   ├── Dockerfile                 ← CREAR
│   ├── .dockerignore               ← CREAR
│   ├── nginx.conf                  ← CREAR
│   └── ... (código existente)
│
├── docker-compose.yml             ← CREAR (orquestación)
├── .env.docker                    ← CREAR (variables)
├── Nginx/
│   └── nginx-proxy.conf           ← CREAR (proxy reverso - opcional)
│
└── docs/
    └── DOCKER.md                  ← Esta guía
```

---

## 🔨 Paso 1: Dockerfile para Backend (ASP.NET Core)

**Archivo:** `inmobiliaria_api/Dockerfile`

```dockerfile
# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copiar archivos de proyecto
COPY ["inmobiliaria_api.csproj", "./"]

# Restaurar dependencias
RUN dotnet restore "inmobiliaria_api.csproj"

# Copiar código
COPY . .

# Build la aplicación
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copiar binarios desde etapa build
COPY --from=build /app/publish .

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:2000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:2000/health || exit 1

# Puerto expuesto
EXPOSE 2000

# Comando para ejecutar
ENTRYPOINT ["dotnet", "inmobiliaria_api.dll"]
```

**Beneficios:**
- Build multi-stage (solo 2 capas, tamaño mínimo)
- Health check integrado
- Variables de entorno configurables

---

## 🔨 Paso 2: Dockerfile para Frontend (Angular + Nginx)

**Archivo:** `inmobiliaria_front/Dockerfile`

```dockerfile
# Etapa 1: Build Angular
FROM node:18-alpine AS build

WORKDIR /app

# Copiar package.json
COPY package*.json ./

# Instalar dependencias
RUN npm ci

# Copiar código fuente
COPY . .

# Build producción
RUN npm run build -- --configuration production

# Etapa 2: Servir con Nginx
FROM nginx:alpine

# Copiar configuración Nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Copiar aplicación compilada
COPY --from=build /app/dist/pyre/browser /usr/share/nginx/html

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost/health.html || exit 1

# Puerto
EXPOSE 80 443

# Iniciar Nginx
CMD ["nginx", "-g", "daemon off;"]
```

---

## ⚙️ Paso 3: Configuración Nginx

**Archivo:** `inmobiliaria_front/nginx.conf`

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

    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;

    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1000;
    gzip_types text/plain text/css text/xml text/javascript
               application/x-javascript application/xml+rss
               application/javascript application/json;

    # Servidor frontend
    server {
        listen 80 default_server;
        listen [::]:80 default_server;

        server_name _;
        root /usr/share/nginx/html;
        index index.html;

        # SPA: todas las rutas van a index.html
        location / {
            try_files $uri $uri/ /index.html;
            expires 1h;
            add_header Cache-Control "public, immutable";
        }

        # Assets (CSS, JS)
        location ~* ^/assets/ {
            expires 30d;
            add_header Cache-Control "public, immutable";
        }

        # API proxy al backend
        location /api/ {
            proxy_pass http://backend:2000;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;
        }

        # Health check para Docker
        location /health.html {
            access_log off;
            return 200 "healthy";
            add_header Content-Type text/plain;
        }
    }
}
```

---

## 🐳 Paso 4: docker-compose.yml (Orquestación)

**Archivo:** `docker-compose.yml` (raíz del proyecto)

```yaml
version: '3.9'

services:
  # Base de datos
  mariadb:
    image: mariadb:10.11
    container_name: inmobiliaria_db
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD:-root123}
      MYSQL_DATABASE: ${DB_NAME:-inmobiliaria}
      MYSQL_USER: ${DB_USER:-inmobiliaria_user}
      MYSQL_PASSWORD: ${DB_PASSWORD:-secure_password}
    ports:
      - "3306:3306"
    volumes:
      - mariadb_data:/var/lib/mysql
      - ./database/init.sql:/docker-entrypoint-initdb.d/init.sql:ro
    healthcheck:
      test: ["CMD", "mariadb-admin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - inmobiliaria_network

  # Backend ASP.NET Core
  backend:
    build:
      context: ./inmobiliaria_api
      dockerfile: Dockerfile
    container_name: inmobiliaria_api
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}
      ConnectionStrings__DefaultConnection: "Server=mariadb;Port=3306;Database=${DB_NAME:-inmobiliaria};User=${DB_USER:-inmobiliaria_user};Password=${DB_PASSWORD:-secure_password};"
      JWT_SECRET: ${JWT_SECRET:-change_this_secret_key_in_production}
      JWT_EXPIRATION_MINUTES: 15
      CLOUDFLARE_ACCOUNT_ID: ${CLOUDFLARE_ACCOUNT_ID}
      CLOUDFLARE_API_TOKEN: ${CLOUDFLARE_API_TOKEN}
      MERCADOPAGO_ACCESS_TOKEN: ${MERCADOPAGO_ACCESS_TOKEN}
    ports:
      - "2000:2000"
    depends_on:
      mariadb:
        condition: service_healthy
    volumes:
      - ./inmobiliaria_api:/app/src  # Para desarrollo
    networks:
      - inmobiliaria_network
    restart: unless-stopped

  # Frontend Angular + Nginx
  frontend:
    build:
      context: ./inmobiliaria_front
      dockerfile: Dockerfile
    container_name: inmobiliaria_frontend
    ports:
      - "80:80"
      - "443:443"
    environment:
      BACKEND_URL: http://backend:2000
    depends_on:
      - backend
    volumes:
      - ./inmobiliaria_front/nginx.conf:/etc/nginx/nginx.conf:ro
    networks:
      - inmobiliaria_network
    restart: unless-stopped

  # (Opcional) Redis para cache
  redis:
    image: redis:7-alpine
    container_name: inmobiliaria_redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - inmobiliaria_network
    restart: unless-stopped

volumes:
  mariadb_data:
    driver: local
  redis_data:
    driver: local

networks:
  inmobiliaria_network:
    driver: bridge
```

---

## 🔐 Paso 5: Archivo .env.docker

**Archivo:** `.env.docker` (raíz, gitignore obligatorio)

```bash
# Seguridad
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=your_super_secret_key_change_in_prod
JWT_EXPIRATION_MINUTES=15

# Base de datos
DB_NAME=inmobiliaria
DB_USER=inmobiliaria_user
DB_PASSWORD=secure_password_change_in_prod
DB_ROOT_PASSWORD=root_password_change_in_prod

# APIs externas
CLOUDFLARE_ACCOUNT_ID=your_cloudflare_id
CLOUDFLARE_API_TOKEN=your_cloudflare_token
MERCADOPAGO_ACCESS_TOKEN=your_mp_token

# Configuración
ASPNETCORE_URLS=http://+:2000
BACKEND_URL=http://backend:2000
FRONTEND_URL=http://localhost
```

---

## 🚀 Paso 6: .dockerignore (Optimización)

**Backend:** `inmobiliaria_api/.dockerignore`
```
bin
obj
.vs
.vscode
*.user
.git
.gitignore
README.md
.github
node_modules
```

**Frontend:** `inmobiliaria_front/.dockerignore`
```
node_modules
dist
.angular
.git
.gitignore
README.md
.github
coverage
.vscode
*.log
```

---

## 📋 Comandos Docker Básicos

### Desarrollo Local

```bash
# Construir imágenes
docker-compose build

# Levantar todos los servicios
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f backend

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (⚠️ borra datos)
docker-compose down -v

# Ejecutar comando en contenedor
docker-compose exec backend dotnet ef migrations add MigracionNueva

# Rebuild sin caché
docker-compose build --no-cache
```

### Verificar Estado

```bash
# Ver servicios corriendo
docker-compose ps

# Ver detalles de red
docker-compose networks ls

# Ver volúmenes
docker volume ls

# Inspeccionar contenedor
docker inspect inmobiliaria_api
```

---

## 🔄 Flujo de Deploy en Producción

### Opción 1: En un VPS (Servidor Virtual)

```bash
# 1. SSH al servidor
ssh root@tu_servidor.com

# 2. Clonar repo
git clone https://github.com/tuusuario/inmobiliaria_saas.git
cd inmobiliaria_saas

# 3. Configurar variables (IMPORTANTE - no usar .env de desarrollo)
cp .env.docker .env.prod
# Editar .env.prod con valores REALES
vim .env.prod

# 4. Build e iniciar
docker-compose -f docker-compose.yml up -d

# 5. Ver logs
docker-compose logs -f

# 6. Verificar health
curl http://localhost:2000/health
curl http://localhost/health.html
```

### Opción 2: En AWS/GCP/Azure (Kubernetes)

```bash
# Esto requiere Kubernetes (K8s) - más complejo
# Pero Docker ya está listo para esto

# Básicamente:
# 1. Push imagen a ECR/GCR/ACR
# 2. Crear manifiestos K8s (deployment.yaml)
# 3. kubectl apply -f deployment.yaml
```

### Opción 3: Docker Swarm (Simple)

```bash
# Inicializar swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml inmobiliaria

# Ver servicios
docker service ls

# Ver logs
docker service logs inmobiliaria_backend
```

---

## 🛡️ Seguridad en Producción

### 1. Variables Sensibles
```bash
❌ NO COMMITEAR .env a Git
✅ Usar AWS Secrets Manager / GCP Secret Manager
✅ O variables de entorno en servidor

# .gitignore
.env.prod
.env.local
```

### 2. HTTPS/SSL en Nginx

```nginx
# Agregar a nginx.conf
server {
    listen 443 ssl http2;
    ssl_certificate /etc/letsencrypt/live/tudominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/tudominio.com/privkey.pem;

    # Redirigir HTTP a HTTPS
    if ($scheme != "https") {
        return 301 https://$server_name$request_uri;
    }
}
```

### 3. Limitar Recursos

```yaml
# En docker-compose.yml
backend:
  deploy:
    resources:
      limits:
        cpus: '1'
        memory: 512M
      reservations:
        cpus: '0.5'
        memory: 256M
```

### 4. Monitoreo

```bash
# Healthcheck automático
docker-compose ps  # muestra salud de servicios

# Monitoreo avanzado
docker stats        # CPU, RAM, I/O
```

---

## 📊 Comparativa: Local vs Producción

```
LOCAL (Sin Docker):
├─ Setup complejo (instalar .NET, Node, MySQL)
├─ 30 minutos para nuevo developer
├─ "Funciona en mi máquina"
├─ Difícil reproducir bugs
└─ Deploy manual y propenso a errores

PRODUCCIÓN (Sin Docker):
├─ Configuración manual en servidor
├─ Diferentes versiones en dev/prod
├─ Downtime largo en updates
├─ Difícil escalar
└─ Rollback lento (> 5 minutos)

────────────────────────────────

LOCAL (Con Docker):
├─ docker-compose up -d
├─ 2 minutos setup
├─ Mismo entorno dev/prod/ci
├─ Reproducible 100%
└─ Deploy consistente

PRODUCCIÓN (Con Docker):
├─ docker-compose up -d (mismo comando)
├─ Garantizado funciona
├─ Downtime casi cero (< 30s)
├─ Escala en segundos
└─ Rollback instantáneo
```

---

## 🎯 Checklist de Implementación

### Backend
- [ ] Crear `inmobiliaria_api/Dockerfile`
- [ ] Crear `inmobiliaria_api/.dockerignore`
- [ ] Probar: `docker build -t inmobiliaria_api .`
- [ ] Probar: `docker run -p 2000:2000 inmobiliaria_api`

### Frontend
- [ ] Crear `inmobiliaria_front/Dockerfile`
- [ ] Crear `inmobiliaria_front/nginx.conf`
- [ ] Crear `inmobiliaria_front/.dockerignore`
- [ ] Probar: `docker build -t inmobiliaria_frontend .`
- [ ] Probar: `docker run -p 80:80 inmobiliaria_frontend`

### Orquestación
- [ ] Crear `docker-compose.yml`
- [ ] Crear `.env.docker`
- [ ] Agregar `.env*` a `.gitignore`
- [ ] Probar: `docker-compose up -d`
- [ ] Verificar: `docker-compose ps`

### Producción
- [ ] Configurar `.env.prod` con valores REALES
- [ ] Configurar HTTPS en nginx
- [ ] Agregar health checks
- [ ] Configurar backups de data
- [ ] Monitoreo y alertas

---

## 📈 Ventajas Finales para Tu SaaS

| Aspecto | Beneficio |
|---------|-----------|
| **Onboarding** | Nuevo dev: `docker-compose up` en lugar de 30 min de setup |
| **Deploy** | De 30 minutos → 30 segundos |
| **Escalabilidad** | Agregar instancias sin cambiar código |
| **Debugging** | Reproducir bugs exactamente como en prod |
| **Multi-tenant** | Fácil crear instancias separadas por tenant |
| **Backup/Disaster** | Snapshot de contenedores + volúmenes |
| **Testing** | CI/CD automático con imágenes idénticas |
| **Costos** | Usar recursos solo cuando los necesitas |

---

## 🚀 Próximos Pasos

1. **Crear los Dockerfiles** (Backend + Frontend)
2. **Crear docker-compose.yml**
3. **Probar localmente** (`docker-compose up -d`)
4. **Setup CI/CD** (GitHub Actions + Docker Registry)
5. **Deploy a producción** (AWS ECS / VPS)

¿Necesitas ayuda con alguno de estos pasos? 🚀
