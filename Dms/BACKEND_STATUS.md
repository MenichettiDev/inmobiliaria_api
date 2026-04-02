# Estado Actual - Backend (Imágenes Cloudflare R2)

**Fecha**: 2026-03-28
**Componente**: Sistema de Imágenes de Propiedades

---

## ✅ Implementado

### Modelos
- **ImagenPropiedad** (`Models/ImagenPropiedad.cs`)
  - `Id` (int, Primary Key)
  - `IdPropiedad` (int, FK → Propiedad)
  - `Url` (string, max 255) - Almacena URLs públicas de R2 ✅
  - `R2Key` (string, max 500) - Key en Cloudflare R2 ✅
  - `Orden` (int) - Para ordenar imágenes
  - `EsPrincipal` (bool) - Marca imagen principal ✅
  - `CreadoEn` (DateTime)

- **Plan** (`Models/Plan.cs`)
  - `MaxImagenesPropiedad` (int?, nullable) - Límite de imágenes por plan ✅

- **Propiedad** - Relación bidireccional
  - `ICollection<ImagenPropiedad> Imagenes` ✅

### Servicio (ImagenPropiedadService)
- `GetImagenesByPropiedadAsync()` - Obtener todas las imágenes de una propiedad ✅
- `GetImagenesPaginatedAsync()` - Obtener imágenes con paginación ✅
- `GetByIdAndTenantAsync()` - Obtener imagen específica ✅
- `UploadImagenAsync()` - Subir imagen desde IFormFile a R2 ✅
  - Validación de tipo (jpg, jpeg, png, webp) ✅
  - Validación de tamaño (≤5MB) ✅
  - Integración con Cloudflare R2 ✅
  - Primera imagen marcada como principal automáticamente ✅
- `CreateImagenAsync()` - Crear imagen con URL externa (compatibilidad) ✅
- `UpdateImagenAsync()` - Actualizar imagen ✅
- `DeleteAsync()` - Eliminar imagen (DB + R2) ✅
- `HacerPrincipalAsync()` - Marcar imagen como principal ✅
- `ReordenarImagenesAsync()` - Reordenar imágenes ✅
- Validación de límites por plan ✅

### Controlador (ImagenPropiedadController)
- `GET /api/imagenpropiedad` - Listar imágenes (paginado) ✅
- `GET /api/imagenpropiedad/{id}` - Obtener imagen ✅
- `GET /api/imagenpropiedad/propiedad/{propiedadId}` - Imágenes de propiedad ✅
- `POST /api/imagenpropiedad` - Crear imagen con URL externa ✅
- `POST /api/imagenpropiedad/upload` - Subir imagen multipart (IFormFile) ✅
  - Retorna 201 si éxito ✅
  - Retorna 402 si se supera límite del plan ✅
- `PUT /api/imagenpropiedad/{id}` - Actualizar imagen ✅
- `PUT /api/imagenpropiedad/{id}/hacer-principal` - Marcar como principal ✅
- `DELETE /api/imagenpropiedad/{id}` - Eliminar imagen (DB + R2) ✅
- `PUT /api/imagenpropiedad/propiedad/{propiedadId}/reordenar` - Reordenar ✅
- Multi-tenancy: Usa `GetTenantId()` desde JWT ✅
- Autorización: Roles Administrador, Supervisor, Agente ✅

### DTOs
- `CreateImagenPropiedadDto` ✅
- `UpdateImagenPropiedadDto` ✅
- `ImagenPropiedadDto` (respuesta) ✅

### Repositorio
- `GetByPropiedadAndTenantAsync()` ✅
- `ValidatePropiedadInTenantAsync()` - Validación multi-tenant ✅
- `GetCountByPropiedadAsync()` ✅
- `ReordenarImagenesAsync()` ✅

---

## ✅ Completado

### Infraestructura R2 ✅
- `Services/CloudflareR2Service.cs` creado con métodos:
  - `UploadAsync(stream, key, contentType)` ✅
  - `DeleteAsync(key)` ✅
  - `GenerateKey(tenantId, propiedadId, fileName)` ✅
  - `ExistsAsync(key)` ✅

### Configuración R2 ✅
- Sección `Cloudflare:R2` en `appsettings.json` ✅
  - AccountId, AccessKeyId, SecretAccessKey, BucketName, PublicUrl

### Modelos ✅
- `Plan.cs` → `MaxImagenesPropiedad` ✅
- `ImagenPropiedad.cs` → `EsPrincipal`, `R2Key` ✅
- EF Core Migration: `AddR2ImageFields` ✅

### Validación de Límites ✅
- `PlanGateService.PuedeSubirImagenAsync()` ✅
- Endpoint `/upload` retorna 402 si se supera límite ✅

### Validaciones de Archivo ✅
- Tipos: jpg, jpeg, png, webp ✅
- Tamaño máximo: 5MB ✅
- MIME type validation ✅

### Imagen Principal ✅
- Endpoint `PUT /api/imagenpropiedad/{id}/hacer-principal` ✅
- Primera imagen automáticamente principal ✅
- Solo 1 principal por propiedad ✅

### Integración con R2 ✅
- Upload: `CloudflareR2Service.UploadAsync()` ✅
- Delete: `CloudflareR2Service.DeleteAsync()` ✅
- Key generation con aislamiento por tenant ✅

---

## 🔐 Consideraciones de Seguridad

- ✅ Multi-tenancy validado en todos los endpoints
- ✅ Autorización por rol
- ❌ Credenciales R2 no deben exponerse al frontend
- ⚠️ Validar tipos MIME reales (Magic Bytes) para evitar ataques
- ⚠️ Límite de tamaño debe validarse en backend (cliente puede engañar)

---

## 📊 Endpoints Finales Esperados

```
GET    /api/imagenpropiedad                                    # Listar (paginado)
GET    /api/imagenpropiedad/{id}                               # Obtener una
GET    /api/imagenpropiedad/propiedad/{propiedadId}            # Imágenes de propiedad
POST   /api/imagenpropiedad                                    # Crear (base64/URL)
POST   /api/imagenpropiedad/upload                             # Crear (IFormFile)
PUT    /api/imagenpropiedad/{id}                               # Actualizar
PUT    /api/imagenpropiedad/{id}/hacer-principal               # [NUEVO] Marcar principal
DELETE /api/imagenpropiedad/{id}                               # Eliminar
PUT    /api/imagenpropiedad/propiedad/{propiedadId}/reordenar  # Reordenar
```
