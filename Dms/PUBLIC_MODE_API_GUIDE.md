# Public Mode API Guide - Testing & Verification

## 🎯 Feature Overview

The **Public Mode** feature enables:
- **Cross-tenant property viewing**: Browse all published properties from any organization via `/api/public/propiedades`
- **Tenant-specific browsing**: View properties from specific organizations via `/api/public/tenant/{subdominio}`
- **No authentication required**: Anyone can view published properties without login

---

## 🔌 Backend API Endpoints

### 1. Get Cross-Tenant Published Properties (with Pagination)
```
GET /api/public/propiedades
```

**Query Parameters:**
- `page` (int, default: 1) — Page number
- `pageSize` (int, default: 10) — Records per page
- `titulo` (string, optional) — Filter by property title
- `precioMin` (decimal, optional) — Minimum price filter
- `precioMax` (decimal, optional) — Maximum price filter

**Request Example:**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades?page=1&pageSize=10&titulo=departamento&precioMin=100000&precioMax=500000" \
  -H "Accept: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 1,
        "titulo": "Departamento Luminoso Centro",
        "descripcion": "Hermoso departamento...",
        "precio": 250000,
        "direccion": "Av. Corrientes 1000, CABA",
        "latitud": -34.6037,
        "longitud": -58.3816,
        "publicadaEn": "2026-03-24T15:30:00Z",
        "idInmobiliaria": 1,
        "inmobiliariaNombre": "ProPiedades Elite",
        "inmobiliariaSubdominio": "elite",
        "urlImagenes": [
          "https://storage.example.com/prop1-img1.jpg",
          "https://storage.example.com/prop1-img2.jpg"
        ]
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalRecords": 150,
    "totalPages": 15,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "Propiedades públicas obtenidas correctamente"
}
```

---

### 2. Get Published Property Detail
```
GET /api/public/propiedades/{id}
```

**Path Parameters:**
- `id` (int) — Property ID

**Request Example:**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades/1" \
  -H "Accept: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "titulo": "Departamento Luminoso Centro",
    "descripcion": "Hermoso departamento con vista al río...",
    "precio": 250000,
    "direccion": "Av. Corrientes 1000, CABA",
    "latitud": -34.6037,
    "longitud": -58.3816,
    "publicadaEn": "2026-03-24T15:30:00Z",
    "idInmobiliaria": 1,
    "inmobiliariaNombre": "ProPiedades Elite",
    "inmobiliariaSubdominio": "elite",
    "urlImagenes": [...]
  },
  "message": "Propiedad encontrada"
}
```

**Error (404 Not Found):**
```json
{
  "success": false,
  "message": "Propiedad no encontrada o no está publicada."
}
```

---

### 3. Get Properties by Tenant Subdomain
```
GET /api/public/tenant/{subdominio}
```

**Path Parameters:**
- `subdominio` (string) — Tenant subdomain (e.g., "elite", "centro", "norte")

**Query Parameters:** Same as endpoint #1

**Request Example:**
```bash
curl -X GET "http://localhost:5000/api/public/tenant/elite?page=1&pageSize=20" \
  -H "Accept: application/json"
```

**Response:** Same format as endpoint #1 (paginated PropiedadPublicaDto array)

---

## 🔐 Admin Endpoints (Requires Authentication)

### Publish a Property
```
PATCH /api/propiedad/{id}/publicar
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Request Example:**
```bash
curl -X PATCH "http://localhost:5000/api/propiedad/5/publicar" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Propiedad publicada correctamente."
}
```

---

### Unpublish a Property
```
PATCH /api/propiedad/{id}/despublicar
```

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Propiedad despublicada correctamente."
}
```

---

## 🧪 Testing Scenarios

### Scenario 1: Browse Cross-Tenant Catalog
```bash
# User visits public portal (no subdomain, no login)
# Frontend calls:
curl "http://localhost:5000/api/public/propiedades?page=1&pageSize=12"

# User can view properties from ALL organizations
# Sidebar is hidden, simple header with Inicio/Propiedades/Ingresar
```

### Scenario 2: Browse Organization's Public Properties
```bash
# User visits tenant subdomain without login
# http://elite.localhost:4200/portal

# Frontend detects subdomain via ContextService
# Calls:
curl "http://localhost:5000/api/public/tenant/elite?page=1&pageSize=12"

# Shows only Elite's published properties
# Sidebar still hidden, shows tenant branding
```

### Scenario 3: Admin Publishes Property
```bash
# Admin logged in to tenant dashboard
# Clicks "Publicar" on property ID 5

# Frontend calls:
curl -X PATCH "http://localhost:5000/api/propiedad/5/publicar" \
  -H "Authorization: Bearer {TOKEN}"

# Property appears in /api/public/propiedades
# Property appears in /api/public/tenant/elite
# es_publicada = true, publicada_en = NOW()
```

---

## 📋 Database Schema

### Propiedad Table Updates
```sql
ALTER TABLE propiedades ADD COLUMN es_publicada BOOLEAN DEFAULT FALSE;
ALTER TABLE propiedades ADD COLUMN publicada_en DATETIME NULL;
CREATE INDEX idx_propiedades_publicada ON propiedades(es_publicada, id_estado_admin);
```

**Key fields:**
- `es_publicada` (bool) — Published status
- `publicada_en` (datetime) — Publishing timestamp
- `id_inmobiliaria` (int) — FK to inmobiliarias table
- `id_estado_admin` (int) — Must be 1 (active) to be visible publicly

---

## 🛠️ Frontend Architecture

### ContextService (Public/Tenant Detection)
```typescript
this.contextService.isPublic()      // true if no subdomain
this.contextService.isTenant()      // true if subdomain exists
this.contextService.getSubdomain()  // returns subdomain or null
```

### Routes Structure
```
/portal                          ← PublicLayoutComponent
├── /portal                       ← PublicPropiedadesComponent (grid)
├── /portal/propiedades           ← PublicPropiedadesComponent (grid)
└── /portal/propiedades/:id       ← PublicPropiedadDetalleComponent (detail)

/login                           ← Login (without layout)
/dashboard                       ← ProtectedLayout (with sidebar)
```

### API Service Call Logic
```typescript
// In PublicPropiedadesComponent.ngOnInit():
if (contextService.isTenant() && subdominio) {
  // Tenant mode: call /api/public/tenant/{subdominio}
  propiedadesService.getPropiedadesByTenant(subdominio, page, size)
} else {
  // Public mode: call /api/public/propiedades
  propiedadesService.getPropiedadesPublicas(page, size)
}
```

---

## ✅ Verification Checklist

### Backend Tests (Postman/cURL)

- [ ] **Public Endpoints Respond**
  - [ ] GET /api/public/propiedades → 200 with data
  - [ ] GET /api/public/propiedades/1 → 200 if published, 404 if not
  - [ ] GET /api/public/tenant/elite → 200 with filtered data

- [ ] **Filtering Works**
  - [ ] ?titulo=apartamento → returns matching titles
  - [ ] ?precioMin=100000 → filters by minimum price
  - [ ] ?precioMax=500000 → filters by maximum price

- [ ] **Authentication Not Required**
  - [ ] /api/public/* endpoints respond WITHOUT Authorization header
  - [ ] No JWT token needed

- [ ] **Only Published Properties Visible**
  - [ ] es_publicada=true appears in /api/public/
  - [ ] es_publicada=false does NOT appear
  - [ ] id_estado_admin=0 (inactive) does NOT appear

- [ ] **Admin Endpoints Secure**
  - [ ] PATCH /api/propiedad/{id}/publicar without token → 401
  - [ ] PATCH /api/propiedad/{id}/publicar with invalid token → 401

### Frontend Tests (Browser)

- [ ] **Public Portal Works**
  - [ ] http://localhost:4200/portal loads without login
  - [ ] Properties grid displays
  - [ ] Pagination works
  - [ ] Filters (título, precio) work
  - [ ] No sidebar visible

- [ ] **Tenant Portal Works**
  - [ ] http://elite.localhost:4200/portal loads without login
  - [ ] ContextService shows isTenant() = true
  - [ ] Only Elite's properties show
  - [ ] Subdomain badge visible on cards

- [ ] **Detail Page Works**
  - [ ] Click "Ver Detalle" opens detail page
  - [ ] URL changes to /portal/propiedades/:id
  - [ ] Property details load
  - [ ] Gallery images work
  - [ ] "Volver al catálogo" link returns to grid

- [ ] **Publish/Unpublish Works**
  - [ ] Admin logs in and navigates to propiedades
  - [ ] Clicks "Publicar" button
  - [ ] Toast shows success
  - [ ] Property appears in /portal

---

## 🐛 Common Issues & Fixes

### Issue: Properties Not Appearing in Public API
**Solution:**
- Verify `es_publicada = true` in database
- Check `id_estado_admin = 1` (active)
- Verify `publicada_en` is not null

### Issue: Subdomain Not Detected
**Solution:**
- Check browser console: `context service.getSubdomain()`
- Verify domain format: `tenant.localhost` or `tenant.dominio.com`
- Check auth.interceptor sends `X-Subdomain` header

### Issue: 401 Unauthorized on Public Endpoints
**Solution:**
- Verify PublicController has `[AllowAnonymous]` attribute
- Check route doesn't have `[Authorize]`

---

## 📝 API Documentation

**Base URL (Development):**
```
http://localhost:5000
```

**Base URL (Production):**
```
https://api.tudominio.com
```

**Public API Root:**
```
/api/public
```

**Response Format:**
```json
{
  "success": boolean,
  "data": T,
  "message": string,
  "errors": string[] (optional)
}
```

---

## 🚀 Next Steps

1. **Run Backend:**
   ```bash
   cd inmobiliaria_api
   dotnet run
   # Listens on http://localhost:5000
   ```

2. **Run Frontend:**
   ```bash
   cd inmobiliaria_front
   npm start
   # Opens http://localhost:4200
   ```

3. **Test Public Portal:**
   - Visit `http://localhost:4200/portal`
   - Should load without login
   - Grid should display published properties

4. **Test Admin Publishing:**
   - Login to `http://localhost:4200/login`
   - Navigate to Propiedades
   - Click Publicar on any property
   - Verify it appears in /portal

---

**Last Updated:** 2026-03-24
**Status:** ✅ Fully Implemented
