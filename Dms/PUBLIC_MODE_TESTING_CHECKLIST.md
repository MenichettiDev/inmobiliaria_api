# Public Mode - Testing Checklist ✅

## Quick Start

```bash
# Terminal 1: Backend
cd inmobiliaria_api
dotnet run
# Expected: "Now listening on: http://localhost:5000"

# Terminal 2: Frontend
cd inmobiliaria_front
npm start
# Expected: "Application bundle generation complete"
```

---

## 🧪 Backend Testing (Using cURL or Postman)

### 1. Public Endpoints (No Authentication)

**Test 1.1: Get All Published Properties**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades?page=1&pageSize=5"
```
- [ ] Returns 200 OK
- [ ] Response has `"success": true`
- [ ] Data contains array of properties
- [ ] Each property has: id, titulo, precio, direccion, inmobiliariaNombre
- [ ] No sensitive fields (no IdAgenteResponsable)

**Test 1.2: Filter by Title**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades?page=1&pageSize=5&titulo=departamento"
```
- [ ] Returns only properties with "departamento" in title
- [ ] Pagination still works

**Test 1.3: Filter by Price Range**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades?page=1&pageSize=5&precioMin=100000&precioMax=500000"
```
- [ ] Returns only properties within price range
- [ ] Properties above/below range not included

**Test 1.4: Get Single Published Property**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades/1"
```
- [ ] Returns 200 OK if property exists and is published
- [ ] Response has property detail with images
- [ ] Returns 404 if property doesn't exist or is unpublished

**Test 1.5: Get Properties by Tenant**
```bash
curl -X GET "http://localhost:5000/api/public/tenant/elite?page=1&pageSize=5"
```
- [ ] Returns 200 OK
- [ ] All properties have `inmobiliariaSubdominio: "elite"`
- [ ] Properties from other tenants NOT included
- [ ] Returns empty array if tenant has no published properties

**Test 1.6: Tenant Not Found**
```bash
curl -X GET "http://localhost:5000/api/public/tenant/nonexistent?page=1"
```
- [ ] Returns 200 OK with empty data array (or appropriate response)
- [ ] No 500 error

---

### 2. Admin Endpoints (Require JWT)

**Test 2.1: Publish a Property (Success)**
```bash
# First, get a JWT token by logging in
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password123"}'
# Copy the token from response

# Then publish property #5
curl -X PATCH "http://localhost:5000/api/propiedad/5/publicar" \
  -H "Authorization: Bearer {TOKEN_HERE}"
```
- [ ] Returns 200 OK
- [ ] Response message: "Propiedad publicada correctamente."
- [ ] In database: es_publicada = true, publicada_en = NOW()

**Test 2.2: Verify Property Now Appears in Public API**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades"
```
- [ ] Property #5 appears in results

**Test 2.3: Unpublish Property**
```bash
curl -X PATCH "http://localhost:5000/api/propiedad/5/despublicar" \
  -H "Authorization: Bearer {TOKEN_HERE}"
```
- [ ] Returns 200 OK
- [ ] Response message: "Propiedad despublicada correctamente."
- [ ] In database: es_publicada = false, publicada_en = null

**Test 2.4: Verify Property Removed from Public API**
```bash
curl -X GET "http://localhost:5000/api/public/propiedades"
```
- [ ] Property #5 no longer appears

**Test 2.5: Unauthorized Access (No Token)**
```bash
curl -X PATCH "http://localhost:5000/api/propiedad/5/publicar"
```
- [ ] Returns 401 Unauthorized
- [ ] Response message: "Unauthorized"

**Test 2.6: Invalid Token**
```bash
curl -X PATCH "http://localhost:5000/api/propiedad/5/publicar" \
  -H "Authorization: Bearer invalid_token_xyz"
```
- [ ] Returns 401 Unauthorized

---

## 🖥️ Frontend Testing (Browser)

### 3. Public Portal (Cross-Tenant)

**Test 3.1: Access Without Login**
- [ ] Open browser: http://localhost:4200/portal
- [ ] Page loads WITHOUT redirecting to login
- [ ] No error messages

**Test 3.2: Verify Public Layout**
- [ ] Sidebar is NOT visible
- [ ] Header shows: Logo, "Inicio", "Propiedades", "Ingresar"
- [ ] Footer shows: copyright info

**Test 3.3: Properties Grid Displays**
- [ ] Cards appear in grid (responsive: 3 columns on desktop)
- [ ] Each card shows: Image, Title, Address (📍), Price, Tenant Badge, Description
- [ ] Clicking card "Ver Detalle" works

**Test 3.4: Pagination Works**
- [ ] "Anterior" button disabled on page 1
- [ ] "Siguiente" button enabled if more pages exist
- [ ] Clicking "Siguiente" loads next page
- [ ] Page number updates

**Test 3.5: Filters Work**
- [ ] Type in "Título" filter → grid updates
- [ ] Type "Precio mín" → grid updates
- [ ] Type "Precio máx" → grid updates
- [ ] Combining filters works

**Test 3.6: Console Logs Show Context**
- [ ] Open Browser DevTools → Console
- [ ] Should see: `✅ Public context`
- [ ] Should NOT see: subdomain logs

---

### 4. Tenant Portal (Subdomain)

**Test 4.1: Access Tenant Subdomain Without Login**
- [ ] Open: http://elite.localhost:4200/portal (if using localhost)
  - *Or use actual subdomain: http://elite.yourdomain.com/portal*
- [ ] Page loads WITHOUT redirecting to login
- [ ] Same layout, but title says "Propiedades Disponibles"

**Test 4.2: Console Shows Tenant Context**
- [ ] Open Browser DevTools → Console
- [ ] Should see: `✅ Tenant context: elite`
- [ ] Should see subdomain extracted correctly

**Test 4.3: Only Tenant's Properties Show**
- [ ] Grid only contains properties from "elite" organization
- [ ] All cards have same "Tenant Badge" (elite's name)
- [ ] Properties from other tenants NOT visible

**Test 4.4: Tenant Badge Visible**
- [ ] Top-right corner of each card shows tenant name/badge
- [ ] Clicking tenant badge should (eventually) link to tenant info

---

### 5. Property Detail Page

**Test 5.1: Open Detail Page**
- [ ] From grid, click "Ver Detalle" on any card
- [ ] URL changes to: `/portal/propiedades/1` (or respective ID)
- [ ] Detail page loads without login

**Test 5.2: Gallery Displays**
- [ ] Large image shows at top
- [ ] If multiple images: thumbnails appear below
- [ ] Clicking thumbnail changes main image

**Test 5.3: Property Details Visible**
- [ ] Title displays prominently
- [ ] Price shows in large font
- [ ] Address shows with 📍 emoji
- [ ] Tenant badge visible
- [ ] Coordinates display (if available)
- [ ] Published date shows

**Test 5.4: Description Shows**
- [ ] Full property description visible
- [ ] Map section shows (placeholder or Leaflet)

**Test 5.5: Back Button Works**
- [ ] Click "← Volver al catálogo"
- [ ] Returns to grid view
- [ ] Grid maintains pagination position (if possible)

---

### 6. Login Integration

**Test 6.1: Login Link Works**
- [ ] From public portal, click "Ingresar" in header
- [ ] Redirected to `/login`
- [ ] Login form appears

**Test 6.2: After Login Redirect**
- [ ] Login with admin credentials
- [ ] Redirected to `/dashboard`
- [ ] Sidebar NOW appears (not public mode)

**Test 6.3: Admin Can See Publish Controls**
- [ ] Go to Propiedades (admin view)
- [ ] See "Publicar" button on unpublished properties
- [ ] See "Despublicar" button on published properties

---

## 📱 Mobile/Responsive Testing

**Test 7.1: Grid Layout Mobile**
- [ ] Resize browser to 480px width (or open on phone)
- [ ] Cards stack in 1 column (not 3)
- [ ] Touch-friendly spacing
- [ ] No horizontal scrolling

**Test 7.2: Images Responsive**
- [ ] Images scale properly
- [ ] No distortion or overflow

**Test 7.3: Filters Mobile**
- [ ] Filter inputs stack vertically
- [ ] Still functional on small screens

---

## 🔗 Network/API Testing

**Test 8.1: Network Tab Inspection**
- [ ] Open DevTools → Network tab
- [ ] Refresh `/portal` page
- [ ] Should see:
  - ✅ Initial page load (index.html)
  - ✅ GET `/api/public/propiedades?page=1...`
  - ✅ Image URLs loading (from storage)
  - ❌ NO auth token in headers (for public requests)

**Test 8.2: X-Subdomain Header**
- [ ] Open DevTools → Network → Filter "api/public"
- [ ] Click on request detail
- [ ] On PUBLIC portal: X-Subdomain header should be absent or null
- [ ] On TENANT portal (elite.localhost): X-Subdomain header = "elite"

**Test 8.3: CORS Testing**
- [ ] If frontend & backend on different ports:
  - [ ] Public requests should work (CORS allowed)
  - [ ] No "CORS policy" errors in console

---

## 🐛 Edge Cases & Error Handling

**Test 9.1: Non-existent Property**
- [ ] Try to access: `/portal/propiedades/99999`
- [ ] Should show: "Propiedad no encontrada o no está publicada."
- [ ] No 500 error

**Test 9.2: Unpublished Property**
- [ ] Manually change published property to unpublished
- [ ] Try to access detail page
- [ ] Should show error message

**Test 9.3: Network Failure**
- [ ] Open DevTools → Network → Throttling (Offline)
- [ ] Try to load `/portal`
- [ ] Should show error message gracefully
- [ ] No hang or freeze

**Test 9.4: Empty Results**
- [ ] Tenant with no published properties
- [ ] Visit `/api/public/tenant/empty-tenant`
- [ ] Should show: "No se encontraron propiedades"
- [ ] No error state

**Test 9.5: Slow API**
- [ ] Throttle network speed in DevTools
- [ ] Should see "Cargando propiedades..." spinner
- [ ] Page becomes responsive after load

---

## 📊 Database Verification

**Test 10.1: Verify Table Structure**
```sql
DESCRIBE propiedades;
```
- [ ] Column `es_publicada` exists (BOOLEAN/TINYINT)
- [ ] Column `publicada_en` exists (DATETIME, nullable)

**Test 10.2: Verify Data**
```sql
SELECT id, titulo, es_publicada, publicada_en FROM propiedades LIMIT 5;
```
- [ ] Some properties have `es_publicada = 1` (true)
- [ ] Some have `es_publicada = 0` (false)
- [ ] Published properties have `publicada_en` timestamp
- [ ] Unpublished have `publicada_en = NULL`

**Test 10.3: Verify Indexes**
```sql
SHOW INDEX FROM propiedades;
```
- [ ] Index on `es_publicada` exists
- [ ] Index on `(id_inmobiliaria, es_publicada)` exists

---

## ✅ Final Verification

| Feature | Status | Notes |
|---------|--------|-------|
| **Public API Endpoints** | ⬜ | GET /api/public/* work |
| **Admin Endpoints** | ⬜ | PATCH /api/propiedad/*/publicar work |
| **Public Portal Grid** | ⬜ | All properties display |
| **Tenant Portal** | ⬜ | Only tenant properties display |
| **Detail Page** | ⬜ | Full property info loads |
| **Filters Work** | ⬜ | titulo, precioMin, precioMax |
| **Pagination Works** | ⬜ | Previous/Next buttons |
| **Mobile Responsive** | ⬜ | Works on mobile devices |
| **No Auth Required** | ⬜ | Public endpoints accessible |
| **Admin Auth Required** | ⬜ | Publish/Unpublish need JWT |
| **Subdomain Detection** | ⬜ | ContextService works |
| **X-Subdomain Header** | ⬜ | Sent in tenant mode |
| **Database Fields** | ⬜ | es_publicada & publicada_en exist |
| **Error Handling** | ⬜ | Graceful error messages |

---

## 🚀 Success Criteria

✅ **Public Mode is READY when:**
1. ✅ Can view properties on `/portal` without login
2. ✅ Can view tenant-specific properties on `tenant.localhost/portal`
3. ✅ Can click through to property details
4. ✅ Filters and pagination work
5. ✅ Admins can publish/unpublish properties
6. ✅ Published properties appear in public API
7. ✅ Unpublished properties don't appear in public API
8. ✅ No login required for browsing
9. ✅ No errors in console or network tab
10. ✅ Mobile layout is responsive

---

## 📝 Notes Section

```
Testing Date: _______________
Tested By: ____________________
Environment: Development [ ] / Staging [ ] / Production [ ]

Issues Found:
1. ____________________________
2. ____________________________
3. ____________________________

Performance Notes:
- Page load time: _____ ms
- API response time: _____ ms
- Database queries: _____

Approved For Production: Yes [ ] / No [ ]
```

---

## 🎯 Next Steps After Verification

1. **Run in Docker** → See DOCKER_SETUP.md
2. **Deploy to VPS** → See DOCKER_LITESPEED_CYBERPANEL.md
3. **Set up CDN** → Optimize image delivery
4. **Add Caching** → Redis for homepage
5. **Monitor Performance** → Track metrics

---

**Created:** 2026-03-24
**Last Updated:** 2026-03-24
**Status:** Ready for Testing ✅
