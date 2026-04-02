# TASK_IMAGES.md — Imágenes de Vehículos con Cloudflare R2

## Agente: Backend Agent + Frontend Agent
## Comando: `/backend-agent TASK_IMAGES` luego `/frontend-agent TASK_IMAGES`

---

## 🎯 Objetivo

Implementar subida, gestión y eliminación de imágenes de vehículos usando Cloudflare R2 como almacenamiento, con límites por plan.

---


## 🧠 Concepto
```
Usuario sube imagen
    → backend valida límite del plan
    → sube archivo a Cloudflare R2
    → guarda URL y Key en tabla Imagen
    → devuelve URL pública

Usuario elimina imagen
    → backend elimina archivo de R2
    → elimina registro de DB
```

---

## 🛠️ Configuración R2

### Variables de entorno necesarias
```env
R2_ACCOUNT_ID=
R2_ACCESS_KEY_ID=
R2_SECRET_ACCESS_KEY=
R2_BUCKET_NAME=inmobiliaria
R2_PUBLIC_URL=https://images.inmobiliaria.com
```

### SDK a usar
```
AWSSDK.S3 — compatible con R2 (protocolo S3)
```

### Configuración del cliente
```csharp
var config = new AmazonS3Config
{
    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
    ForcePathStyle = true
};
```

---

## 📤 Output esperado

### Backend

#### Servicio R2 (Infrastructure/Storage/R2StorageService.cs)
```
- UploadAsync(stream, fileName, contentType, tenantId) → url, key
- DeleteAsync(key) → void
- GenerateKey(tenantId, vehiculoId, fileName) → string
  ejemplo: "tenants/42/propiedad/123/uuid-filename.jpg"
```


#### Componentes
```
imagenes-galeria.component.ts
    → muestra grid de imágenes del vehículo
    → drag & drop para reordenar
    → botón eliminar por imagen
    → badge "Principal" en imagen destacada
    → botón "Hacer principal"

imagen-upload.component.ts
    → zona de drag & drop para subir
    → preview antes de subir
    → barra de progreso durante subida
    → validación de tipo y tamaño en cliente
    → muestra límite: "3 de 5 imágenes usadas"
    → deshabilita subida al alcanzar límite del plan
```

---

## 🚫 Restricciones

- Solo imágenes: jpg, jpeg, png, webp
- Tamaño máximo: 5MB por imagen
- Key en R2 siempre incluye TenantId (aislamiento)
- No exponer credenciales de R2 al frontend
- Al eliminar propiedad → eliminar todas sus imágenes de R2
- Multi-tenancy: un tenant nunca accede a imágenes de otro

---

## ✅ Criterio de éxito

- [ ] Subida de imagen guarda archivo en R2 y URL en DB
- [ ] Límite del plan se respeta (error 402 al superarlo)
- [ ] Eliminación borra de R2 y DB
- [ ] Reordenamiento persiste en DB
- [ ] Imagen principal se muestra en portal y listado
- [ ] Preview funciona antes de confirmar subida
- [ ] Barra de progreso durante subida
- [ ] Validación de tipo y tamaño en frontend y backend