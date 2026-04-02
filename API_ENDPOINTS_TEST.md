# 📋 Pruebas de Endpoints - Imágenes de Propiedades

## Estado de los Endpoints ✅

Todos los endpoints están **implementados y respondiendo** en `http://localhost:2000`

### Verificación realizada (2026-04-01):
- ✅ **401 Unauthorized** sin token JWT → Autenticación funcionando
- ✅ **Rutas no conflictúan** → `/propiedad/{propiedadId}` resuelta correctamente antes de `/{id}`

---

## 📤 Endpoints Implementados

### 1. **Listar Imágenes (Paginado)**
```http
GET /api/imagenpropiedad?page=1&pageSize=10
Authorization: Bearer {token}
```
**Respuesta esperada:** 200 OK con lista paginada de imágenes

---

### 2. **Listar Imágenes por Propiedad**
```http
GET /api/imagenpropiedad/propiedad/{propiedadId}
Authorization: Bearer {token}
```
**Respuesta esperada:** 200 OK con imágenes de la propiedad

---

### 3. **Obtener Imagen Específica**
```http
GET /api/imagenpropiedad/{id}
Authorization: Bearer {token}
```
**Respuesta esperada:** 200 OK con datos de la imagen

---

### 4. **Subir Imagen a Cloudflare R2** ⭐
```http
POST /api/imagenpropiedad/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

Form Data:
  - IdPropiedad: (integer)
  - Archivo: (file - JPG/PNG/WEBP, máx 5MB)
```

**Respuesta esperada:** 
- 201 Created → Imagen subida exitosamente
- 402 Payment Required → Límite de imágenes alcanzado (plan insuficiente)
- 400 Bad Request → Archivo inválido

**Validaciones implementadas:**
- ✅ Tipos permitidos: JPG, PNG, WEBP
- ✅ Tamaño máximo: 5 MB
- ✅ Límite por plan: Verifica con `PlanGateService`
- ✅ Aislamiento multi-tenant: Clave R2 incluye `IdInmobiliaria`

---

### 5. **Crear Imagen con URL externa**
```http
POST /api/imagenpropiedad
Authorization: Bearer {token}
Content-Type: application/json

{
  "idPropiedad": 1,
  "url": "https://example.com/imagen.jpg",
  "orden": 1
}
```
**Respuesta esperada:** 201 Created

---

### 6. **Actualizar Imagen**
```http
PUT /api/imagenpropiedad/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 1,
  "url": "https://example.com/nueva-imagen.jpg",
  "orden": 2
}
```
**Respuesta esperada:** 200 OK

---

### 7. **Eliminar Imagen** (borra R2 + DB)
```http
DELETE /api/imagenpropiedad/{id}
Authorization: Bearer {token}
```
**Respuesta esperada:** 200 OK
- ✅ Elimina archivo de Cloudflare R2
- ✅ Elimina registro de base de datos

---

### 8. **Marcar como Principal**
```http
PUT /api/imagenpropiedad/{id}/hacer-principal
Authorization: Bearer {token}
```
**Respuesta esperada:** 200 OK
- ✅ Desmarca imagen principal anterior
- ✅ Marca esta como principal

---

### 9. **Reordenar Imágenes**
```http
PUT /api/imagenpropiedad/propiedad/{propiedadId}/reordenar
Authorization: Bearer {token}
Content-Type: application/json

[1, 3, 2]
```
**Respuesta esperada:** 200 OK (actualiza orden de IDs proporcionados)

---

## 🔐 Seguridad Implementada

- ✅ **Autenticación JWT** en todos los endpoints
- ✅ **Roles**: Administrador, Supervisor, Agente
- ✅ **Validación multi-tenant**: Usuario solo accede a sus datos
- ✅ **Aislamiento R2**: Archivos almacenados en `tenants/{tenantId}/propiedad/{propiedadId}/...`
- ✅ **Control de plan**: PlanGateService valida límites
- ✅ **Validación en servidor**: Tipos, tamaño, extensiones

---

## ⚠️ Problemas Corregidos

### ✅ Conflicto de rutas
**Antes:** `GET /api/imagenpropiedad/{id}` se resolvía antes que `GET /api/imagenpropiedad/propiedad/{propiedadId}`
**Después:** Reordenadas rutas específicas ANTES que genéricas

```csharp
// AHORA CORRECTO:
[HttpGet("propiedad/{propiedadId}")]  // ← Va primero (específico)
[HttpGet("{id}")]                      // ← Va después (genérico)
```

---

## 📝 Próximos Pasos para Testing Manual

1. **Obtener token JWT:**
   ```bash
   curl -X POST http://localhost:2000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"usuario@example.com","password":"password"}'
   ```

2. **Probar upload:**
   ```bash
   curl -X POST http://localhost:2000/api/imagenpropiedad/upload \
     -H "Authorization: Bearer {TOKEN}" \
     -F "IdPropiedad=1" \
     -F "Archivo=@imagen.jpg"
   ```

3. **Verificar en R2:**
   - URL: https://pub-c9ad666b6db046819ecda2c916a2bdf7.r2.dev
   - Estructura: `tenants/{tenantId}/propiedad/{propiedadId}/{uuid}.{ext}`

---

## 🎯 Criterios de Éxito

- [x] Endpoint `/api/imagenpropiedad/upload` implementado
- [x] Validación de plan (límite de imágenes)
- [x] Subida a Cloudflare R2
- [x] Almacenamiento de URL en DB
- [x] Eliminación de R2 + DB sincronizadas
- [x] Reordenamiento de imágenes
- [x] Imagen principal
- [x] Multi-tenancy seguro
- [x] Rutas sin conflicto
