# CLAUDE_SAAS.md — Sistema Operativo para Construcción de SaaS B2B

Eres un **arquitecto de software senior + CTO + builder de SaaS**, especializado en construir productos B2B multi-tenant que generan ingresos.

No escribes código aislado. Construyes **SaaS completos, escalables, seguros y listos para producción**.

Tu prioridad absoluta es:

> llevar ideas a producción lo más rápido posible, con calidad estructural real.

---

# 🎯 OBJETIVO

Transformar cualquier idea en:

- un SaaS funcional
- desplegado en VPS
- con autenticación real
- con monetización activa
- listo para usuarios reales

---

# 👥 MULTI-TENANCY (NO NEGOCIABLE)

Todos los sistemas deben incluir:

- Tenants (empresas)
- Usuarios por tenant
- Roles y permisos
- Aislamiento de datos

Nunca creas sistemas single-tenant.

---

# 🔐 AUTENTICACIÓN (ESTÁNDAR)

Siempre implementas:

- Access Token (JWT)
- Refresh Token
- Rotación de tokens
- Revocación
- Expiración segura

Nunca implementas auth básica incompleta.

---

# 💰 MONETIZACIÓN

Siempre defines:

- Planes (free / pro / enterprise)
- Límites por tenant
- Feature gating

Integración obligatoria con Mercado Pago.

---

# 🧠 TIPO DE PRODUCTO

Por defecto construyes:

> SaaS B2B de automatización + IA

Ejemplos:

- automatización de procesos
- scraping inteligente
- generación de leads
- workflows empresariales

---

# 📦 ESTRUCTURA DEL REPO (MONOREPO)

```
saas-app/
├── backend/
│   ├── src/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   ├── tests/
│   └── Dockerfile
│
├── frontend/
│   ├── src/
│   └── Dockerfile
│
├── infra/
│   ├── docker-compose.yml
│   └── nginx/
│
├── docs/
├── .env
├── .env.example
└── README.md
```

---

# 🐳 INFRAESTRUCTURA (OBLIGATORIO)

Siempre:

- Dockerizas backend, frontend y DB
- Usas docker-compose
- Configuras Nginx
- Preparas entorno reproducible

Nunca deploy manual sin contenedores.

---

# 🧪 CALIDAD (NIVEL MEDIO)

Siempre incluyes:

- Validación de inputs
- Manejo de errores
- Logs claros
- Tests en endpoints críticos

---

# 🚫 PROHIBIDO

- Hardcodear credenciales
- Lógica en controllers
- Saltarse multi-tenancy
- Crear features innecesarias
- Usar tecnologías fuera del stack
- SQL manual sin control (usar EF Core)
- Auth sin refresh tokens
- Deploy manual sin Docker
- Código sin manejo de errores

---

# 🤖 COMPORTAMIENTO DEL AGENTE (MODO CTO)

Siempre ejecutas:

1. Cuestionas la idea
2. Defines MVP claro
3. Diseñas arquitectura
4. Generas estructura completa
5. Implementas código funcional
6. Preparas deploy
7. Defines siguiente iteración

No pides confirmación innecesaria.

---

# ⚡ MODO EJECUCIÓN COMPLETA (CRÍTICO)

Debes trabajar como agente autónomo.

Cuando recibes una tarea:

- NO divides en múltiples respuestas innecesarias
- NO explicas de más
- NO repites contexto

Siempre:

1. Asumes decisiones razonables
2. Construyes TODO de inicio a fin
3. Entregas sistema funcional completo

---

# 🧠 OPTIMIZACIÓN DE TOKENS (MUY IMPORTANTE)

Reglas obligatorias:

- No explicas conceptos básicos
- No repites código ya definido
- No generas variantes innecesarias
- No haces over-documentation

Código > explicación

Prefieres:

- outputs compactos
- estructuras claras
- respuestas directas

---

# 📤 FORMATO DE RESPUESTA

Siempre entregas:

1. Arquitectura breve
2. Estructura de carpetas
3. Código completo
4. Instrucciones de ejecución
5. Instrucciones de deploy
6. Próximo paso

---

# 🧩 REGLA DE ORO

Si el usuario pide algo pequeño:

→ lo integras dentro de un SaaS real
→ sin sobreingeniería
→ listo para producción

---

# 🚀 DEFINICIÓN DE ÉXITO

El trabajo está terminado SOLO si:

- El sistema corre
- Se puede desplegar
- Tiene auth funcional
- Soporta múltiples tenants
- Puede cobrar

Si falta uno → NO está terminado.

---
---
---

# 📌 Project Overview

**InmobiliariaSaaS** es una plataforma SaaS B2B para el sector inmobiliario argentino, orientada a inmobiliarias que gestionan propiedades, leads, clientes y operaciones. Cada inmobiliaria es un *tenant* independiente que accede al sistema bajo su propio subdominio (ej: `demo.buscopropiedades.com.ar`).

El sistema permite:
- Gestión de un catálogo de propiedades con imágenes.
- CRM de leads con historial de estados, asignación a agentes y fuentes de contacto.
- Gestión de clientes, usuarios internos (agentes, supervisores, administradores).
- Suscripciones a planes diferenciados con límites de uso (propiedades, usuarios, leads/mes).
- Historial de transacciones y uso mensual por tenant.

**Stack actual:**
- **Backend:** ASP.NET Core 8 (.NET 8), Entity Framework Core 9 + Pomelo (MariaDB 10.4.32), JWT, Swagger.
- **Frontend:** Angular 19 con SSR (`@angular/ssr`), PrimeNG 19, Bootstrap 5, standalone components, lazy loading.
- **BD:** MariaDB (shared schema, row-level isolation por `id_inmobiliaria`).
- **Deploy:** `buscopropiedades.com.ar` y `test.buscopropiedades.com.ar` (puerto API 2000).

---

# 🧱 Architecture

## Patrón general

Monolito modular con separación de capas clásica (N-tier):

```
[Angular SPA / SSR]  →  HTTP + JWT  →  [ASP.NET Core Web API]  →  [MariaDB]
```

No existe API Gateway, message broker, ni microservicios. Todo corre en un único proceso backend.

## Backend — Capas actuales

| Capa | Responsabilidad |
|---|---|
| `Controllers/` | Entrada HTTP, validación básica, extracción de `tenantId` del JWT, delegación al Service |
| `Services/` | Lógica de negocio, orquestación, mapeo a DTOs |
| `Repositories/` | Acceso a datos, filtrado por tenant, queries específicas |
| `Models/` | Entidades EF Core con Data Annotations |
| `Dtos/` | Objetos de transferencia organizados por entidad (carpetas) |
| `Middleware/` | `TenantValidationMiddleware` — valida claim `IdInmobiliaria` |
| `Services/TenantContext.cs` | Resolución de tenant por subdominio + cache in-request |
| `Extensions/` | `ClaimsPrincipalExtensions` — helpers para extraer claims |
| `Exceptions/` | Excepciones de dominio (`LeadsExceptions.cs`) |
| `Data/` | `ApplicationDbContext` + `db.sql` |

## Multi-Tenancy: Shared Schema

El aislamiento es **row-level**: todas las tablas de negocio tienen columna `id_inmobiliaria`. El tenant se resuelve de dos formas:

1. **Claim JWT** (`IdInmobiliaria`): extraído por cada controller con `GetTenantId()`. Mecanismo principal en producción.
2. **Subdominio HTTP** + `TenantContext` service: extrae el subdominio de `request.Host` o del header `X-Subdomain` (desarrollo local).

## Frontend — Arquitectura Angular 19

```
src/app/
 ├── views/           # Feature modules (lazy loaded): auth, dashboard, propiedades, leads, etc.
 ├── services/        # Servicios transversales (alertas, sidebar, page-title)
 ├── guards/          # authGuard (functional)
 ├── interceptors/    # authInterceptor (functional HTTP)
 ├── shared/          # Componentes compartidos (sidebar, etc.)
 └── models/          # Interfaces/clases TypeScript
```

- **17 módulos de vistas** con lazy loading, cada uno con sus propias rutas.
- Standalone components (Angular 19 modern API).
- SSR habilitado (`@angular/ssr`).

---

# 🔧 Backend — Cómo debe estar

### Secretos y configuración

- Todas las credenciales (JWT key, SMTP password, salt, connection strings) deben vivir en **variables de entorno** o un gestor de secretos (Azure Key Vault, AWS Secrets Manager, `dotnet user-secrets` en dev).
- `appsettings.json` solo contiene valores no sensibles. `appsettings.Production.json` no se committea al repositorio.
- El naming de configuración debe ser consistente: usar `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` en un solo bloque y en todos los archivos de config que lo usen.

### Paquetes y dependencias

- `Microsoft.AspNetCore.Authentication.JwtBearer` debe coincidir con el target framework (net8 → v8.x).
- Dependencias sin uso (ej.: `Microsoft.EntityFrameworkCore.SqlServer` cuando se usa Pomelo/MySQL) deben ser eliminadas.
- AutoMapper: si está instalado, usarlo. Si no se usa, desinstalarlo.

### Controladores

- Los controllers solo deben extraer datos del request (tenant, userId, DTO), delegar al service y devolver la respuesta. Sin lógica de negocio.
- `ApplicationDbContext` no debe inyectarse directamente en controllers ni services. El acceso a BD es responsabilidad exclusiva de los repositories.
- El código de scaffolding (ej. `WeatherForecast`) debe eliminarse antes de producción.

### Servicios

- Todo método que opere sobre datos de un tenant debe recibir `tenantId` como parámetro explícito y forzar `entity.IdInmobiliaria = tenantId` antes de persistir.
- No deben existir overloads sin `tenantId` para entidades con aislamiento por tenant.
- Los estados de entidades deben modelarse con `enum` o constantes nombradas, nunca con magic numbers.
- Las excepciones de dominio definidas deben usarse consistentemente; el catch general solo es fallback.

### TenantContext y caché

- La resolución de `tenantId` desde subdominio debe cachearse con `IMemoryCache` (TTL de ~5 min) para evitar una query a BD por request.
- La interfaz `ITenantContext` debe usarse con métodos `async` para evitar bloqueos síncronos en contexto async.
- En el futuro: migrar cache a Redis para soportar múltiples instancias del API.

### Autenticación y sesión

- Junto al JWT de corta duración (≤1h recomendado) debe emitirse un **Refresh Token** persistido en tabla `refresh_tokens`.
- El endpoint `POST /api/auth/refresh` valida el refresh token, rota el par (nuevo JWT + nuevo refresh token) e invalida el anterior.
- El endpoint `POST /api/auth/login` debe tener **rate limiting** (máx. 5 intentos/min por IP) implementado con `app.UseRateLimiter()`.

### Seguridad de datos

- Todos los endpoints de lectura de entidades con aislamiento deben usar métodos `GetByIdAndTenantAsync(id, tenantId)` — nunca buscar solo por `id`.
- La serialización JSON debe tener `JsonNamingPolicy.CamelCase` para consistencia con Angular.

### Upload de archivos

- Las imágenes deben subirse a almacenamiento externo (S3, Azure Blob, Cloudinary). No se guardan en `wwwroot` local porque impide escalar horizontalmente.
- El tamaño máximo de upload debe estar configurado y validado en el backend.

### Tests

- Los servicios críticos (`LeadService`, `UsuarioService`, `SuscripcionService`) deben tener tests unitarios con mocks de repositorios.
- Los endpoints de auth deben tener tests de integración.

---

# 🎨 Frontend — Cómo debe estar

### Comunicación con el API

- Debe existir un servicio base `ApiService` (o equivalente) que centralice la URL base del API, el manejo de errores HTTP y la lógica de reintentos.
- La URL base del API se lee de `environment.ts` (dev) y `environment.prod.ts` (prod). No se hardcodea en ningún servicio.

### Interceptor

- El `authInterceptor` debe incluir el header `X-Subdomain` en desarrollo, leyéndolo de `environment.ts`.
- Además de manejar 401, debe manejar el caso de token expirado activando el flujo de refresh token antes de reintentar la petición.

### Autorización

- El modelo de autorización debe ser consistente: si el backend usa roles por nombre string (`"Administrador"`, `"Supervisor"`, `"Agente"`), el frontend debe leer el claim de rol del JWT decodificado y usarlo, no un campo `id_acceso` numérico separado.
- El guard debe redirigir a una ruta `/acceso-denegado` que exista y esté enrutada.

### Manejo de errores

- Debe existir un interceptor o handler global para errores HTTP distintos de 401 (500, 403, 422) que muestre una notificación al usuario y registre el error.
- No deben quedar `console.debug` con información de sesión o permisos en el código de producción.

### SSR

- El acceso a APIs del browser (`document`, `window`, `localStorage`) debe hacerse siempre detrás de una verificación `isPlatformBrowser(PLATFORM_ID)`, no con try/catch.

### Vistas y rutas

- Todos los módulos en `views/` que tengan componentes deben estar enrutados en `app.routes.ts`.
- Las vistas en construcción que no estén listas deben tener una pantalla de "próximamente" o estar detrás de un feature flag, no simplemente huérfanas.

### Tests

- Cada servicio Angular que consuma el API debe tener al menos un spec con `HttpClientTestingModule` que valide el contrato esperado.

---

# 🔗 Integration — Cómo debe estar

- El contrato de serialización JSON debe ser consistente en ambas direcciones: backend con `CamelCase`, frontend con interfaces TypeScript que matcheen exactamente los campos del API.
- El upload de imágenes debe usar un contrato uniforme: `multipart/form-data` estándar para todos los casos (create y update), sin JSON embebido en campos de formulario.
- El header `X-Subdomain` debe enviarse en todas las peticiones del frontend en entornos sin subdominio real (dev/staging).
- Las URLs de CORS deben configurarse desde variables de entorno, no hardcodeadas en `Program.cs`.

---

# ⚠️ Key Issues — Orden de prioridad

| # | Área | Acción requerida |
|---|---|---|
| 1 | Config seguridad | Mover credenciales a env vars. Eliminar del repositorio. |
| 2 | Config JWT | Unificar naming `Jwt:Key` en código y config. |
| 3 | Tenant cache | Implementar `IMemoryCache` en `TenantContext`. |
| 4 | Aislamiento datos | Eliminar métodos sin `tenantId` en entidades con aislamiento. |
| 5 | Autenticación | Implementar refresh token + endpoint `/api/auth/refresh`. |
| 6 | Rate limiting | Agregar rate limiter en `/api/auth/login`. |
| 7 | Autorización | Unificar modelo de roles entre backend y frontend. |
| 8 | Interceptor frontend | Agregar `X-Subdomain` al interceptor en dev. |
| 9 | Imágenes | Migrar a almacenamiento externo (S3/Cloudinary). |
| 10 | Billing | Integrar MercadoPago + webhooks de activación/suspensión. |
| 11 | Separación capas | Extraer `DbContext` de services y controllers. |
| 12 | Tests | Agregar tests unitarios y de integración mínimos. |
| 13 | Serialización | Agregar `JsonNamingPolicy.CamelCase`. |
| 14 | Estados | Reemplazar magic numbers por enums. |
| 15 | Paquetes | Actualizar versiones y eliminar dependencias sin uso. |

---

# 🚀 SaaS Readiness Assessment

| Área | Estado actual | Nivel |
|---|---|---|
| **Multi-tenancy (aislamiento)** | Shared schema, filtrado por `id_inmobiliaria` | **MEDIO-ALTO** |
| **Autenticación** | JWT funcional con claims de rol y tenant | **MEDIO** — falta refresh token, MFA |
| **Autorización (RBAC)** | Roles: Administrador, Supervisor, Agente | **MEDIO** — sin permisos granulares por recurso |
| **Planes y límites** | Modelo `Plan` con límites definidos | **MEDIO** — enforcement no siempre aplicado |
| **Suscripciones / Billing** | Modelo en BD, sin integración de pago | **BAJO** |
| **Onboarding de tenant** | Sin flujo self-service | **MUY BAJO** |
| **Auditoría** | Historial de estado de leads solamente | **BAJO** |
| **Configuración por tenant** | Vía `Plan` (features on/off) | **BAJO** |
| **Observabilidad** | Solo logging a consola con ILogger | **BAJO** |
| **Escalabilidad horizontal** | Sin estado compartido, archivos locales | **MUY BAJO** |
| **Seguridad OWASP básica** | JWT, CORS, hash passwords. Sin rate limiting | **BAJO-MEDIO** |
| **Self-service admin** | Vista `gestion-saas` no enrutada — incompleta | **MUY BAJO** |

---

# 🛠 Recommended Improvements

## Inmediatas (sprint 1)

1. Credenciales → variables de entorno. Eliminar `appsettings.Production.json` del repo. Agregar a `.gitignore`.
2. Unificar naming JWT en config y código.
3. Actualizar `JwtBearer` → v8.x. Eliminar `EF.SqlServer` (sin uso).
4. Rate limiting en `/api/auth/login`.
5. Eliminar métodos `GetAllXxx()` sin filtro de tenant donde aplica aislamiento.
6. Limpiar `appsettings.json` de referencias a proyectos ajenos.
7. Eliminar código scaffold (`WeatherForecast`, `DbSet` duplicado).

## Corto plazo (sprint 2-3)

8. `IMemoryCache` en `TenantContext` con TTL de 5 min.
9. Refresh token — tabla `refresh_tokens` + endpoint `/api/auth/refresh`.
10. `X-Subdomain` en interceptor Angular desde `environment.ts`.
11. Enums de estados (`EstadoUsuario`, `EstadoLead`, `EstadoSuscripcion`).
12. `JsonNamingPolicy.CamelCase` en serialización.
13. Migrar imágenes a storage externo (S3/Cloudinary).
14. Extraer `LeadEstadoHistorial` del `LeadService` a un repository dedicado.
15. Global error handler en Angular (interceptor HTTP para 500/403/422).

## Mediano plazo (sprint 4-6)

16. Integrar MercadoPago — webhook activa/suspende suscripción.
17. Flujo de onboarding self-service — registro → subdominio → trial → pago.
18. Audit log general — tabla `audit_logs` con entidad, acción, usuario, tenant, timestamp.
19. Feature flags por tenant controlados por `Plan`.
20. Tests unitarios de servicios críticos + tests de integración de auth.
21. OpenTelemetry + Sentry para observabilidad en producción.
22. Completar y enrutar panel `gestion-saas` (super-admin SaaS).
23. MFA opcional para administradores.

---

# 🧭 Refactoring Roadmap

```
Sprint 1: Seguridad y estabilidad crítica
├── Secretos → env vars
├── Unificar config JWT
├── Rate limiting en login
├── Fix métodos sin tenant filter
└── Cleanup deps y scaffold

Sprint 2: Fundamentos SaaS
├── TenantContext → IMemoryCache
├── Refresh token
├── X-Subdomain en interceptor
├── CamelCase JSON
└── Enums de estados

Sprint 3: Calidad de código
├── Extraer DbContext de services/controllers
├── Storage externo para imágenes
├── Global error handler Angular
├── Tests básicos backend
└── Unificar modelo de autorización

Sprint 4-5: SaaS features
├── MercadoPago + webhooks
├── Onboarding self-service
├── Audit log
├── Feature flags por Plan
└── Panel gestion-saas completo

Sprint 6: Observabilidad y escala
├── OpenTelemetry + Sentry
├── Redis para tenant cache
├── Load testing y fix queries N+1
└── MFA opcional
```

---

# 📂 Suggested SaaS Architecture

## Arquitectura objetivo

```
                    ┌──────────────────────────────────────┐
                    │         CDN / Load Balancer           │
                    └──────────┬──────────────┬────────────┘
                               │              │
                    ┌──────────▼──┐   ┌───────▼────────┐
                    │ Angular SSR │   │ ASP.NET Core   │
                    │(Vercel/Node)│   │  API (x2+)     │
                    └─────────────┘   └──────┬─────────┘
                                             │
                    ┌────────────────────────┼────────────────┐
                    │                        │                │
             ┌──────▼──────┐       ┌─────────▼───┐  ┌────────▼──────┐
             │   MariaDB   │       │    Redis    │  │ Object Store  │
             │ (pri+réplica)│      │(tenant cache│  │(S3/Cloudinary)│
             └─────────────┘       │ rate limit) │  └───────────────┘
                                   └─────────────┘
```

## Estructura de módulos objetivo

```
inmobiliaria_api/
 ├── Modules/
 │    ├── Auth/          # Login, JWT, refresh token, MFA
 │    ├── Tenancy/       # TenantContext, middleware, onboarding
 │    ├── Properties/    # Propiedades + imágenes
 │    ├── Leads/         # CRM leads, historial, asignación
 │    ├── Clients/       # Clientes
 │    ├── Users/         # Usuarios internos
 │    ├── Billing/       # Planes, suscripciones, MercadoPago
 │    ├── Reporting/     # UsoMensual, reportes
 │    └── Admin/         # Super-admin (gestión de tenants)
 ├── CrossCutting/
 │    ├── Auditing/      # Audit log interceptor
 │    ├── FeatureFlags/  # Validación de límites por Plan
 │    ├── Storage/       # Abstracción de almacenamiento
 │    └── Caching/       # Redis / IMemoryCache abstraction
```

## Tablas nuevas requeridas

| Tabla | Propósito |
|---|---|
| `audit_logs` | Registro inmutable de acciones relevantes |
| `refresh_tokens` | Tokens de renovación de sesión |
| `invitaciones` | Onboarding de nuevos tenants |
| `feature_flags_tenant` | Override de features por tenant |
| `notificaciones` | Centro de notificaciones in-app |

---

# 🧠 Context for AI Agents

## ¿Qué hace el sistema?

CRM + gestión inmobiliaria SaaS B2B. Cada inmobiliaria (tenant) gestiona su catálogo de propiedades, leads de clientes interesados, agentes asignados, y tiene un plan que limita sus capacidades (max propiedades, leads/mes, usuarios).

## ¿Cómo está estructurado?

**Backend**: ASP.NET Core 8, MariaDB, EF Core. Patrón N-tier: `Controller → Service → Repository`. Multi-tenancy shared-schema con `id_inmobiliaria` en cada tabla. Tenant se resuelve desde claim JWT `IdInmobiliaria`. JWT firmado con HMACSHA256.

Roles: `Administrador`, `Supervisor`, `Agente` (en JWT claim `ClaimTypes.Role`). Autorizados en controllers con `[Authorize(Roles = "...")]`.

**Frontend**: Angular 19, lazy loading, standalone, SSR. Auth via JWT interceptor. Guard con `requiredAccess` numérico por ruta. PrimeNG + Bootstrap UI.

**Comunicación**: REST JSON. No hay GraphQL, WebSockets ni message bus.

## ¿Qué debe hacerse a continuación?

**Orden recomendado:**

1. **Mover secretos a variables de entorno** — no tocar lógica, solo config.
2. **Alinear `Jwt:Key`** en `appsettings.json` y `Program.cs` / `AuthController`.
3. **Agregar `IMemoryCache` en TenantContext** — reduce queries a BD por request.
4. **Implementar refresh token** — tabla `refresh_tokens` + endpoint `POST /api/auth/refresh`.
5. **Agregar endpoint de onboarding** — `POST /api/public/registro-inmobiliaria` → crea inmobiliaria + usuario admin + suscripción trial.
6. **Integrar MercadoPago** para activar billing automatizado.
7. **Unificar modelo de autorización** — frontend lee el rol del JWT decodificado.

## Endpoints clave

| Endpoint | Método | Auth | Descripción |
|---|---|---|---|
| `/api/auth/login` | POST | ❌ | Login, devuelve JWT + datos usuario |
| `/api/Propiedad` | GET | ✅ Rol | Lista paginada de propiedades del tenant |
| `/api/Lead` | GET/POST/PUT | ✅ Rol | CRUD leads del tenant |
| `/api/Lead/cambiar-estado` | POST | ✅ | Cambia estado de lead con historial |
| `/api/Suscripcion` | GET/POST | ✅ Admin | Gestión de suscripciones |
| `/api/Plan` | GET | ✅ | Lista de planes disponibles |
| `/api/Usuario` | GET/POST/PUT | ✅ Admin | Gestión de usuarios del tenant |
| `/api/Inmobiliaria` | GET/PUT | ✅ Admin | Datos de la inmobiliaria actual |

## Convenciones del codebase

- Todos los services retornan `BaseResponseDto<T>` con `{ Success, Data, Message, Errors }`.
- Los controllers llaman `GetTenantId()` al inicio de cada action y validan `tenantId > 0`.
- Los services tienen firma `XxxAsync(dto, tenantId)` y fuerzan `entity.IdInmobiliaria = tenantId` antes de persistir.
- Los repositories usan `GetByIdAndTenantAsync(id, tenantId)` para enforcing de aislamiento.
- Las eliminaciones son **lógicas** (`Activo = false`), no físicas.
