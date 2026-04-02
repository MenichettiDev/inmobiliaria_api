# 🏢 Arquitectura Multi-Tenant con Modo Público - SaaS Inmobiliaria

## ✅ STATUS: IMPLEMENTACIÓN COMPLETADA

**Fecha de Implementación:** 23-Mar-2026
**Backend:** C# .NET 8 con EF Core
**Frontend:** Angular 19 Standalone Components

---

## 📋 Contexto

Estoy desarrollando una SaaS inmobiliaria en **Angular 19** con arquitectura **multi-tenant**.

### Arquitectura Actual
- ✓ Cada tenant (inmobiliaria) tiene su propio dashboard
- ✓ Cada tenant tiene su propio subdominio: `tenant1.propiedadescentro.com`, `tenant2.propiedadescentro.com`
- ✓ Sesión y contexto de tenant manejados correctamente
- ✓ Datos segmentados por tenant (aislamiento garantizado)

### Nuevo Requerimiento
Soportar un **modo "usuario público"** (sin sesión, sin tenant activo) accediendo desde el **dominio principal**.

---

## 🎯 Comportamiento Esperado

### 1. Usuario NO logueado (dominio general: `propiedadescentro.com`)
- Navega como visitante
- Ve propiedades **PUBLICADAS de TODOS los tenants**
- Sin contexto de tenant
- Sin acceso a dashboards ni datos privados

### 2. Usuario en subdominio de tenant (`tenant1.propiedadescentro.com`)
- **Sin login**: ve SOLO propiedades publicadas de ese tenant
- **Con login**: accede al dashboard y opera dentro de su tenant
- Ve únicamente datos de su tenant

---

## 🏗️ Solución Propuesta: Arquitectura Completa

### **A. Identificación de Contexto**

#### Frontend: Context Resolver
```typescript
// context.resolver.ts
@Injectable()
export class ContextResolver implements Resolve<AppContext> {
  constructor(private http: HttpClient) {}

  resolve(): Observable<AppContext> {
    const hostname = window.location.hostname;
    const subdomain = this.extractSubdomain(hostname);

    if (subdomain && subdomain !== 'www') {
      // CONTEXTO DE TENANT
      return this.http.get<TenantContext>(`/api/tenant/${subdomain}`)
        .pipe(
          map(tenant => ({
            type: 'TENANT',
            tenant,
            isPublic: false
          })),
          catchError(() => of({
            type: 'PUBLIC',
            tenant: null,
            isPublic: true
          }))
        );
    } else {
      // CONTEXTO PÚBLICO
      return of({
        type: 'PUBLIC',
        tenant: null,
        isPublic: true
      });
    }
  }

  private extractSubdomain(hostname: string): string | null {
    const parts = hostname.split('.');
    return parts.length > 2 ? parts[0] : null;
  }
}
```

#### Backend: Middleware de Tenant
```typescript
// tenant.middleware.ts
@Injectable()
export class TenantMiddleware implements NestMiddleware {
  constructor(private tenantService: TenantService) {}

  async use(req: any, res: any, next: any) {
    const hostname = req.hostname;
    const subdomain = this.extractSubdomain(hostname);

    if (subdomain && subdomain !== 'www') {
      // CONTEXTO DE TENANT
      const tenant = await this.tenantService.findBySubdomain(subdomain);
      if (!tenant) throw new NotFoundException('Tenant not found');
      req.tenant = tenant;
      req.context = 'TENANT';
    } else {
      // CONTEXTO PÚBLICO
      req.context = 'PUBLIC';
      req.tenant = null;
    }
    next();
  }

  private extractSubdomain(hostname: string): string | null {
    const parts = hostname.split('.');
    return parts.length > 2 ? parts[0] : null;
  }
}
```

---

### **B. Estrategia de Base de Datos**

#### Schema de Propiedades
```sql
CREATE TABLE properties (
  id UUID PRIMARY KEY,
  tenant_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  price DECIMAL(10, 2),
  is_published BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE,

  -- Índices críticos
  INDEX idx_tenant_id (tenant_id),
  INDEX idx_published (is_published),
  INDEX idx_tenant_published (tenant_id, is_published),
  INDEX idx_created_at (created_at)
);
```

#### Queries Optimizadas

**Obtener propiedades públicas (cross-tenant):**
```sql
SELECT * FROM properties
WHERE is_published = TRUE
ORDER BY created_at DESC
LIMIT 20;
```

**Obtener propiedades de un tenant específico:**
```sql
SELECT * FROM properties
WHERE tenant_id = $1 AND is_published = TRUE
ORDER BY created_at DESC;
```

**Obtener todas las propiedades de un tenant (privado - requiere autenticación):**
```sql
SELECT * FROM properties
WHERE tenant_id = $1
ORDER BY created_at DESC;
```

---

### **C. Endpoints Diferenciados**

#### 1. Endpoints Públicos (Cross-Tenant)
```typescript
@Controller('/public')
export class PublicPropertiesController {
  constructor(private propertyService: PropertyService) {}

  @Get('properties')
  async getPublicProperties(@Query() query: PaginationDto) {
    // Obtiene propiedades publicadas de TODOS los tenants
    return this.propertyService.findPublished(query);
  }

  @Get('properties/:id')
  async getPublicProperty(@Param('id') id: string) {
    // Obtiene UNA propiedad pública específica
    const property = await this.propertyService.findById(id);
    if (!property.is_published) {
      throw new ForbiddenException('Property is not published');
    }
    return property;
  }

  @Get('properties/tenant/:tenantSlug')
  async getTenantPublicProperties(
    @Param('tenantSlug') tenantSlug: string,
    @Query() query: PaginationDto
  ) {
    // Propiedades publicadas de UN tenant específico (sin login)
    const tenant = await this.tenantService.findBySubdomain(tenantSlug);
    return this.propertyService.findByTenant(tenant.id, true, query);
  }
}
```

#### 2. Endpoints Privados (Tenant-Scoped)
```typescript
@Controller('/tenant/properties')
@UseGuards(AuthGuard, TenantGuard)
export class TenantPropertiesController {
  constructor(private propertyService: PropertyService) {}

  @Get()
  async getTenantProperties(@Req() req: any, @Query() query: PaginationDto) {
    // req.tenant está garantizado por middleware + guard
    return this.propertyService.findByTenant(req.tenant.id, null, query);
  }

  @Get(':id')
  async getProperty(@Param('id') id: string, @Req() req: any) {
    return this.propertyService.findByIdAndTenant(id, req.tenant.id);
  }

  @Post()
  async createProperty(@Body() dto: CreatePropertyDto, @Req() req: any) {
    // Forzar siempre el tenant del usuario autenticado
    dto.tenant_id = req.tenant.id;
    return this.propertyService.create(dto);
  }

  @Patch(':id')
  async updateProperty(
    @Param('id') id: string,
    @Body() dto: UpdatePropertyDto,
    @Req() req: any
  ) {
    // Validación: la propiedad debe pertenencer al tenant del usuario
    return this.propertyService.updateByIdAndTenant(id, req.tenant.id, dto);
  }

  @Delete(':id')
  async deleteProperty(@Param('id') id: string, @Req() req: any) {
    return this.propertyService.deleteByIdAndTenant(id, req.tenant.id);
  }
}
```

---

### **D. Guards y Validaciones de Seguridad**

#### Guard de Tenant
```typescript
@Injectable()
export class TenantGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const req = context.switchToHttp().getRequest();

    if (!req.tenant) {
      throw new UnauthorizedException(
        'This endpoint requires a tenant context'
      );
    }
    return true;
  }
}
```

#### Validación en Servicio (Doble Validación)
```typescript
// property.service.ts
@Injectable()
export class PropertyService {
  constructor(private db: PrismaService) {}

  async updateByIdAndTenant(
    id: string,
    tenantId: string,
    dto: UpdatePropertyDto
  ) {
    // 1. Obtener la propiedad
    const property = await this.db.property.findUnique({
      where: { id },
      include: { tenant: true }
    });

    if (!property) {
      throw new NotFoundException('Property not found');
    }

    // 2. VALIDACIÓN CRÍTICA: verificar que pertenece al tenant correcto
    if (property.tenant_id !== tenantId) {
      throw new ForbiddenException(
        'Property does not belong to your tenant'
      );
    }

    // 3. Actualizar
    return this.db.property.update({
      where: { id },
      data: dto
    });
  }

  async deleteByIdAndTenant(id: string, tenantId: string) {
    const property = await this.db.property.findUnique({ where: { id } });

    if (!property || property.tenant_id !== tenantId) {
      throw new ForbiddenException('Cannot delete this property');
    }

    return this.db.property.delete({ where: { id } });
  }
}
```

---

### **E. Context Service en Frontend**

```typescript
// context.service.ts
@Injectable({ providedIn: 'root' })
export class ContextService {
  private context$ = new BehaviorSubject<AppContext | null>(null);

  constructor(private resolver: ContextResolver) {
    this.resolver.resolve().subscribe(ctx => this.context$.next(ctx));
  }

  getContext(): Observable<AppContext> {
    return this.context$.asObservable();
  }

  isPublic(): boolean {
    return this.context$.value?.isPublic ?? true;
  }

  isTenant(): boolean {
    return !this.isPublic();
  }

  getTenant(): TenantContext | null {
    return this.context$.value?.tenant || null;
  }

  getTenantId(): string | null {
    return this.context$.value?.tenant?.id || null;
  }
}
```

---

### **F. Rutas Dinámicas (Angular)**

```typescript
// app.routes.ts
const routes: Routes = [
  {
    path: '',
    resolve: { context: ContextResolver },
    children: [
      // RUTAS PÚBLICAS
      {
        path: '',
        component: PublicLayoutComponent,
        children: [
          { path: '', component: PublicPropertiesPageComponent },
          { path: 'property/:id', component: PublicPropertyDetailComponent },
          { path: 'login', component: LoginComponent },
          { path: 'register', component: RegisterComponent }
        ]
      },
      // RUTAS DE TENANT
      {
        path: 'dashboard',
        component: TenantLayoutComponent,
        canActivate: [AuthGuard, TenantGuard],
        children: [
          { path: '', component: DashboardComponent },
          { path: 'properties', component: TenantPropertiesComponent },
          { path: 'properties/new', component: CreatePropertyComponent },
          { path: 'properties/:id/edit', component: EditPropertyComponent },
          { path: 'settings', component: TenantSettingsComponent }
        ]
      }
    ]
  }
];
```

---

### **G. Interceptor de API (Automatizar Contexto)**

```typescript
// context.interceptor.ts
@Injectable()
export class ContextInterceptor implements HttpInterceptor {
  constructor(private contextService: ContextService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const tenantId = this.contextService.getTenantId();
    const isPublic = this.contextService.isPublic();

    // Agregar header de contexto para trazabilidad
    let clonedReq = req.clone({
      setHeaders: {
        'X-Context-Type': isPublic ? 'PUBLIC' : 'TENANT',
        ...(tenantId && { 'X-Tenant-Id': tenantId })
      }
    });

    return next.handle(clonedReq);
  }
}
```

---

## ✅ Checklist de Seguridad

- [ ] **Backend**: Middleware valida contexto en cada solicitud
- [ ] **Backend**: Guards garantizan autenticación en rutas privadas
- [ ] **Backend**: Validación doble en servicios (Guard + Servicio)
- [ ] **Backend**: Queries filtran por tenant_id automáticamente
- [ ] **Frontend**: ContextResolver resuelve contexto antes de navegar
- [ ] **Frontend**: Rutas privadas usan guards (AuthGuard + TenantGuard)
- [ ] **Database**: Índices optimizados en (tenant_id, is_published)
- [ ] **Logs**: Registrar accesos con contexto para auditoría

---

## 📊 Ventajas de esta Arquitectura

| Aspecto | Beneficio |
|--------|-----------|
| **Escalabilidad** | Funciona con N tenants sin cambios de código |
| **Seguridad** | Validaciones en múltiples capas (middleware → guard → servicio) |
| **Rendimiento** | Índices optimizados para queries cross-tenant y por tenant |
| **Mantenibilidad** | Lógica centralizada, fácil de modificar |
| **Claridad** | Endpoints públicos y privados claramente diferenciados |
| **Auditoría** | Headers X-Context-Type permiten trazabilidad |

---

## 🚀 Orden de Implementación Recomendado

1. **Backend - Identificación de Tenant**
   - Crear middleware de tenant
   - Crear service de tenant
   - Registrar middleware en app.module

2. **Backend - Base de Datos**
   - Crear/actualizar schema
   - Crear índices
   - Migrations

3. **Backend - Endpoints Públicos**
   - Controller de propiedades públicas
   - Servicio de propiedades con filters

4. **Backend - Endpoints Privados**
   - Controller de tenant properties
   - Guards de validación
   - Validación doble en servicio

5. **Frontend - Context**
   - ContextResolver
   - ContextService
   - Guards en rutas

6. **Frontend - Rutas y Componentes**
   - Actualizar app.routes.ts
   - Crear componentes públicos/privados
   - Interceptor de API

---

## 📝 Restricciones Cumplidas

✓ No hay soluciones hardcodeadas
✓ Escalable a muchos tenants
✓ Fácil de mantener y extender
✓ Patrones reutilizables (middleware, guards, resolvers)
✓ Validación en backend obligatoria
✓ Aislamiento de datos garantizado

---

## 🚀 Implementación Realizada

### Backend (C# .NET 8)

#### 1. Modelo y Base de Datos
- ✅ Agregado campo `EsPublicada` (bool) en modelo `Propiedad`
- ✅ Migration EF Core: `AddEsPublicadaToPropiedad`
- ✅ Actualización de DTOs: `PropiedadResponseDto` + nuevo `PropiedadPublicaDto`

#### 2. Repositorio y Servicios
- ✅ **PropiedadRepository**: métodos públicos
  - `GetPublicadasCrossTenantPagedAsync()` — todas las propiedades publicadas
  - `GetPublicadasBySubdominioPagedAsync()` — propiedades de un tenant específico
  - `GetPublicadaByIdAsync()` — detalle de una propiedad publicada

- ✅ **PropiedadService**: métodos de publicación + métodos públicos
  - `PublicarAsync(id, tenantId)` — setea `EsPublicada=true`, `PublicadaEn=UtcNow`
  - `DespublicarAsync(id, tenantId)` — setea `EsPublicada=false`, `PublicadaEn=null`
  - `GetPublicadasCrossTenantPagedAsync(...)` — orquesta queries públicas
  - `GetPublicadasBySubdominioPagedAsync(...)` — filtra por tenant
  - `GetPublicadaByIdAsync(...)` — detalle público

#### 3. Controladores
- ✅ **PropiedadController**: endpoints PATCH
  - `PATCH /api/propiedad/{id}/publicar` [Roles: Admin, Supervisor]
  - `PATCH /api/propiedad/{id}/despublicar` [Roles: Admin, Supervisor]

- ✅ **PublicController** (nuevo) — sin `[Authorize]`, `[AllowAnonymous]`
  - `GET /api/public/propiedades` — cross-tenant paginado
  - `GET /api/public/propiedades/{id}` — detalle individual
  - `GET /api/public/tenant/{subdominio}` — por subdominio

#### Archivos Backend Creados/Modificados

```
✅ Models/Propiedad.cs — +EsPublicada
✅ Dtos/Propiedad/PropiedadPublicaDto.cs (NUEVO)
✅ Repositories/PropiedadRepository.cs — +3 métodos públicos
✅ Services/PropiedadService.cs — +5 métodos (publicar, métodos públicos)
✅ Controllers/PropiedadController.cs — +2 endpoints PATCH
✅ Controllers/PublicController.cs (NUEVO)
✅ Migrations/[timestamp]_AddEsPublicadaToPropiedad.cs
✅ inmobiliaria_api.csproj — actualización EF Core versions (8.0.10)
```

---

### Frontend (Angular 19)

#### 1. Servicios
- ✅ **ContextService** (nuevo)
  - `getContext()` → `AppContext` con tipo ('PUBLIC' | 'TENANT')
  - `isPublic()`, `isTenant()`
  - `getSubdomain()` → extrae subdominio del hostname
  - Resuelve contexto automáticamente en constructor

- ✅ **PublicPropiedadesService** (nuevo)
  - `getPropiedadesPublicas()` — cross-tenant
  - `getPropiedadPublica(id)` — detalle
  - `getPropiedadesByTenant(subdominio)` — por tenant específico
  - Sin autenticación (sin token)

#### 2. Interceptor
- ✅ **auth.interceptor.ts** — actualizado
  - Inyecta `ContextService`
  - Agrega header `X-Subdomain` automáticamente cuando existe subdominio
  - Mantiene lógica de refresh token intacta

#### 3. Componentes Visuales (Standalone)
- ✅ **PublicLayoutComponent** (nuevo)
  - Header con navegación simple (sin sidebar)
  - Footer
  - RouterOutlet para rutas hijas

- ✅ **PublicPropiedadesComponent** (nuevo)
  - Grilla responsiva de cards con PrimeNG-style
  - Filtros: título, precio min/max
  - Paginación
  - Cambio dinámico según contexto (PUBLIC vs TENANT)

- ✅ **PublicPropiedadDetalleComponent** (nuevo)
  - Galería de imágenes con thumbnails
  - Información detallada
  - Coordenadas de ubicación
  - Info de contacto de la inmobiliaria

#### 4. Rutas
- ✅ **app.routes.ts** — actualizado
  - `/portal` → PublicLayoutComponent (sin guards)
  - `/portal/propiedades` → PublicPropiedadesComponent
  - `/portal/propiedades/:id` → PublicPropiedadDetalleComponent
  - Ruta raíz redirige a `/portal`

#### Archivos Frontend Creados/Modificados

```
✅ services/context.service.ts (NUEVO)
✅ services/public-propiedades.service.ts (NUEVO)
✅ interceptors/auth.interceptor.ts — +ContextService, +X-Subdomain
✅ views/public/public-layout/public-layout.component.ts (NUEVO)
✅ views/public/public-propiedades/public-propiedades.component.ts (NUEVO)
✅ views/public/public-propiedad-detalle/public-propiedad-detalle.component.ts (NUEVO)
✅ app.routes.ts — +rutas públicas
```

---

## 🔐 Seguridad Implementada

### Backend
1. **Validación de Publicación**: Solo propiedades con `EsPublicada=true` en endpoints `/api/public/`
2. **Filtro Automático**: Repository filtra por `IdEstadoAdmin==1` + `EsPublicada==true`
3. **Aislamiento por Tenant**: Métodos privados validann pertenencia al tenant
4. **Cross-Tenant Safety**: Queries públicas NO inyectan filtro de tenant

### Frontend
1. **Rutas Públicas**: `/portal/*` sin `canActivate: [authGuard]`
2. **Rutas Privadas**: `/dashboard/*` mantienen `canActivate: [authGuard]`
3. **Contexto Automático**: ContextService resuelve contexto en construcción
4. **Interceptor Inteligente**: Agrega X-Subdomain solo si existe

---

## 🧪 Cómo Probar

### Desarrollo Local

#### 1. Configurar Hosts File
```bash
# Windows: C:\Windows\System32\drivers\etc\hosts
127.0.0.1  propiedadescentro.local
127.0.0.1  tenant1.propiedadescentro.local
127.0.0.1  tenant2.propiedadescentro.local
127.0.0.1  www.propiedadescentro.local
```

#### 2. Backend
```bash
cd inmobiliaria_api
dotnet ef database update
dotnet run
# Backend en http://localhost:2000
```

#### 3. Frontend
```bash
cd inmobiliaria_front
npm start
# Frontend en http://localhost:4200
```

#### 4. Pruebas Manuales

**Contexto Público (cross-tenant):**
```
http://propiedadescentro.local:4200/portal
http://propiedadescentro.local:4200/portal/propiedades
http://propiedadescentro.local:4200/portal/propiedades/1
```

**Contexto Tenant 1:**
```
http://tenant1.propiedadescentro.local:4200/portal
http://tenant1.propiedadescentro.local:4200/portal/propiedades
```

**Contexto Tenant 2:**
```
http://tenant2.propiedadescentro.local:4200/portal
```

#### 5. Verificación en Consola
```typescript
// En DevTools → Console
// Debe mostrar:
✅ Tenant context: tenant1
// o
✅ Public context
```

#### 6. API Tests (curl)
```bash
# Propiedades públicas cross-tenant
curl http://localhost:2000/api/public/propiedades

# Propiedades de tenant específico
curl http://localhost:2000/api/public/tenant/tenant1

# Publicar propiedad (requiere JWT)
curl -X PATCH \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  http://localhost:2000/api/propiedad/1/publicar

# Despublicar
curl -X PATCH \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  http://localhost:2000/api/propiedad/1/despublicar
```

---

## 📊 Diagrama de Flujo

```
┌─────────────────────────────────────────────────────────┐
│                     USUARIO ACCEDE                      │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ├─→ propiedadescentro.local
                       │   └─→ ContextService.isPublic() = true
                       │       └─→ GET /api/public/propiedades
                       │           └─→ Todas las propiedades publicadas
                       │
                       └─→ tenant1.propiedadescentro.local
                           └─→ ContextService.isTenant() = true
                               ├─→ SIN LOGIN: GET /api/public/tenant/tenant1
                               │   └─→ Solo propiedades publicadas de tenant1
                               │
                               └─→ CON LOGIN: /dashboard
                                   └─→ GET /api/propiedad (con AuthGuard)
                                       └─→ Todas las propiedades del tenant
```

---

## 🎯 Próximos Pasos Opcionales

1. **Integración Leaflet/Google Maps**: En `PublicPropiedadDetalleComponent`
2. **Favoritos Públicos**: LocalStorage para propiedades favoritas
3. **Contacto Directo**: Formulario de contacto en detalle
4. **Búsqueda Avanzada**: Localidad, tipo de propiedad, etc.
5. **Caché**: Redis para queries públicas frecuentes
6. **SEO**: Meta tags dinámicos para propiedades públicas

---

## 📝 Restricciones Cumplidas

✅ No hay soluciones hardcodeadas
✅ Escalable a muchos tenants
✅ Fácil de mantener y extender
✅ Patrones reutilizables (middleware, guards, resolvers)
✅ Validación en backend obligatoria
✅ Aislamiento de datos garantizado
✅ Componentes visuales completos
✅ Rutas públicas sin autenticación
