# FLUJO_COMPLETO.md — Journey Completo del SaaS Inmobiliario

## 📊 Flujo End-to-End v1.0

Este documento describe el viaje completo de un usuario (dueño de inmobiliaria, agente, usuario final) desde el registro hasta el cierre de operación.

---

## 1️⃣ Registro de la Inmobiliaria (Tenant)

**Actor:** Dueño o gerente de inmobiliaria

**Acciones:**
```
1. Navega a centroinmo.com
2. Click "Registrar inmobiliaria"
3. Completa formulario:
   - Nombre de la inmobiliaria
   - Email de contacto
   - Teléfono
   - Selecciona provincia/ciudad
   - Elige subdominio (ej: "acme")
   - Password

4. Valida email (link en correo)
5. Verifica disponibilidad de subdominio en tiempo real
```

**Backend:**
```
POST /auth/register-inmobiliaria
- Valida unicidad del subdominio
- Crea registro en tabla Inmobiliaria
- Crea usuario Admin (primer usuario)
- Crea Suscripcion con Plan FREE
- Crea Uso inicial (0/0/0)
- Envía email de bienvenida
```

**Resultado:**
```
✅ Tenant creado: acme (subdominio)
✅ Suscripción activa: Plan FREE
✅ Usuario Admin creado
✅ Acceso disponible en: acme.centroinmo.com
```

---

## 2️⃣ Configuración Inicial (Onboarding)

**Actor:** Administrador de la inmobiliaria

**Acciones:**
```
1. Login en acme.centroinmo.com
2. Ingresa al Dashboard
3. Ve wizard de onboarding:

   Paso 1: Branding
   - Logo (subir imagen)
   - Color principal
   - Color secundario

   Paso 2: Información
   - Descripción breve
   - Email de contacto
   - Teléfono
   - WhatsApp (opcional)
   - Sitio web (opcional)

   Paso 3: Plan & Integraciones
   - Visualiza límites del plan actual (FREE)
   - Opciones de upgrade (visible)
   - Integraciones disponibles (disabled si plan no permite)

   Paso 4: Configuración CRM
   - Estados personalizados de leads (según plan)
   - Tipos de actividades (llamada, visita, email, etc)
   - Notificaciones
```

**Frontend:**
```
GET /config/plan-limits          → muestra límites según plan
POST /inmobiliaria/{id}/config   → guarda configuración

UI visible según plan:
- FREE: branding + información básica
- PRO: + integraciones
- ENTERPRISE: + todo
```

**Backend:**
```
POST /inmobiliaria/{id}/update-config
- Valida InmobiliariaId = tenant actual
- Valida contra límites del plan
- Guarda en tabla Inmobiliaria
- Retorna configuración actualizada
```

**Resultado:**
```
✅ Inmobiliaria configurada (logo, colores, datos)
✅ Equipo listo para invitaciones
✅ CRM personalizado según plan
✅ Dashboard listo para usar
```

---

## 3️⃣ Invitación y Gestión del Equipo

**Actor:** Administrador de la inmobiliaria

**Acciones:**
```
1. Va a Configuración → Usuarios
2. Click "Invitar usuario"
3. Formulario:
   - Email
   - Nombre
   - Rol (Admin/Agente/Asistente)
   - Inmobiliaria (preseleccionada: acme)
4. Valida disponibilidad de usuarios según plan
5. Envía email de invitación con link
6. Invitado:
   - Recibe email
   - Click en link
   - Se registra (nombre + password)
   - Asociado automáticamente a inmobiliaria + rol
7. Accede a acme.centroinmo.com con su usuario
```

**Frontend:**
```
POST /usuarios                   → crear usuario con invitación
GET  /usuarios                   → listar usuarios de la inmobiliaria
GET  /config/usuario-limit       → validar si hay cupo para invitar
```

**Backend:**
```
POST /usuarios
- Valida InmobiliariaId
- Valida límite de usuarios según plan (402 si supera)
- Crea usuario con estado "invitado"
- Genera token de invitación (corta duración)
- Envía email
- Historial de actividad

POST /auth/accept-invitation
- Valida token de invitación
- Cambia estado a "activo"
- Usuario puede loguearse
```

**Permisos según Rol:**
```
Admin:
  ✅ Ver/editar propiedades
  ✅ Gestionar leads de todos
  ✅ Ver reports
  ✅ Invitar usuarios
  ✅ Configuración

Agente:
  ✅ Ver/editar sus propiedades
  ✅ Gestionar sus leads
  ✅ Ver sus métricas
  ❌ Invitar usuarios
  ❌ Configuración

Asistente:
  ✅ Ver propiedades (lectura)
  ✅ Ver leads (lectura)
  ✅ Registrar actividades
  ❌ Editar propiedades
  ❌ Cambiar estado de leads
```

**Resultado:**
```
✅ Equipo completo en acme.centroinmo.com
✅ Cada usuario accede con sus credenciales
✅ Permisos controlados por rol
✅ Historial de auditoría
```

---

## 4️⃣ Publicación y Gestión de Propiedades

**Actor:** Agente o Administrador

**Acciones:**
```
1. Va a Propiedades → Nueva propiedad
2. Completa formulario:
   - Tipo: Casa/Depto/Lote/Local
   - Título: "Casa 3 amb en Belgrano"
   - Descripción
   - Ubicación (calle, número, ciudad, provincia)
   - Lat/Long (mapa interactivo)
   - Precio (ARS/USD)
   - Operación: Venta/Alquiler
   - Dormitorios, baños, metros
   - Características (pileta, asador, etc)

3. Sube imágenes
   - Máx según plan (FREE: 5, PRO: 20, ENTERPRISE: ilimitado)
   - Selecciona imagen principal
   - Reordena galería

4. Valida disponibilidad según plan
5. Guarda propiedad (estado: Borrador)
6. Click "Publicar"
   - Validación final
   - Cambio de estado: Publicada
   - Timestamp PublicadaEn = ahora
```

**Frontend:**
```
POST   /propiedades              → crear (validar límites)
PUT    /propiedades/{id}         → editar
PATCH  /propiedades/{id}/publicar     → publicar
PATCH  /propiedades/{id}/despublicar  → despublicar
POST   /propiedades/{id}/imagenes     → subir imagen
DELETE /propiedades/{id}/imagenes/{id}→ eliminar imagen
```

**Backend:**
```
POST /propiedades
- Valida InmobiliariaId (tenant actual)
- Valida propiedadesCount < planLimit.MaxPropiedades (402 si supera)
- Guarda con Publicada=false
- Registra en historial

PATCH /propiedades/{id}/publicar
- Valida propiedad pertenece a tenant
- Setea Publicada=true
- Setea PublicadaEn=DateTime.UtcNow
- Registra en historial
- Dispara webhook (notificar)

POST /propiedades/{id}/imagenes
- Valida límite según plan
- Sube a Cloudflare R2
- Crea registro en tabla Imagen
- Retorna URL pública
```

**Publicación Automática en:**
```
✅ centroinmo.com/propiedades         (portal público central)
✅ acme.centroinmo.com/propiedades    (subdominio tenant)
✅ (Fase 2) Integraciones externas (Inmuebles.com, etc)
```

**Resultado:**
```
✅ Propiedad publicada
✅ Visible en portal público
✅ Asignada a agente
✅ Respeta límites del plan
✅ Listable en búsquedas
```

---

## 5️⃣ Usuario Final: Búsqueda y Contacto

**Actor:** Comprador/locatario (usuario anónimo)

**Acciones:**
```
1. Abre centroinmo.com (o acme.centroinmo.com)
2. Ve catálogo público de propiedades
3. Aplica filtros:
   - Provincia/ciudad
   - Rango de precio
   - Tipo de propiedad
   - Dormitorios
   - Búsqueda por palabra clave

4. Click en propiedad
5. Ve detalles:
   - Galería de imágenes
   - Mapa interactivo (Leaflet)
   - Descripción completa
   - Datos de contacto (agente/inmobiliaria)
   - "Contactar agente" button

6. Click "Contactar agente"
7. Formulario (sin login):
   - Nombre
   - Email
   - Teléfono
   - Mensaje
   - Checkbox: Deseo recibir comunicaciones

8. Submit
```

**Frontend:**
```
GET  /portal/propiedades               → listar públicas
GET  /portal/propiedades/{id}          → detalles
GET  /portal/inmobiliarias/{id}        → info agente
POST /portal/leads                     → crear lead desde contacto
```

**Backend:**
```
POST /portal/leads
- No requiere autenticación
- Valida email válido
- Crea Lead:
  * InmobiliariaId = de la propiedad
  * PropiedadId = contactada
  * AgenteId = responsable de la propiedad
  * NombreCliente = del formulario
  * EmailCliente = del formulario
  * TelefonoCliente = del formulario
  * FuenteId = "Portal"
  * EstadoActual = "Nuevo"
- Crea LeadMensaje (primer contacto)
- Envía email a agente: "Nuevo lead en propiedad X"
- Retorna confirmación al usuario final

GET /portal/propiedades?provincia=BA&precio_max=500000
- Filtra solo Publicada=true
- Ordena por recency, destacadas primero
- Paginado
```

**Resultado:**
```
✅ Lead creado automáticamente
✅ Asociado a propiedad, inmobiliaria, agente
✅ Agente notificado
✅ Mensaje inicial registrado
✅ Fuente de contacto rastreada
```

---

## 6️⃣ Gestión del Lead (CRM)

**Actor:** Agente (asignado) o Administrador

**Acciones:**
```
1. Agente recibe notificación (email/sistema)
2. Va a Leads → Ve nuevo lead
3. Click en lead
4. Ve timeline:
   - Contacto inicial (mensaje del usuario final)
   - Propiedad de interés
   - Información de contacto
   - Historial (vacío si es nuevo)

5. Estado del lead: "Nuevo"
6. Solo el agente asignado puede:
   ✅ Editar lead
   ✅ Registrar actividades
   ✅ Cambiar estado
   ✅ Ver mensajes
```

**Permisos:**
```
Agente asignado:
  ✅ Ver lead
  ✅ Editar datos
  ✅ Crear actividades
  ✅ Cambiar estado
  ✅ Enviar mensajes

Admin de inmobiliaria:
  ✅ Ver todos los leads
  ✅ Reasignar a otro agente
  ✅ Ver reportes
  ✅ No interfiere en gestión

Otro Agente:
  ❌ Ver el lead
  ❌ Editar
  ❌ Contactar
```

**Frontend:**
```
GET  /leads/{id}                       → ver detalles
PUT  /leads/{id}                       → editar datos
GET  /leads/{id}/historial             → estado + actividades
```

**Backend:**
```
GET /leads/{id}
- Valida que:
  * Lead pertenece a InmobiliariaId del usuario
  * Usuario es Admin O agente asignado
- Retorna con relaciones: propiedad, agente, actividades, mensajes

PUT /leads/{id}
- Mismas validaciones
- Actualiza datos (nombre, teléfono, etc)
- Registra cambio en historial
```

**Resultado:**
```
✅ Lead disponible para agente
✅ Trazabilidad completa desde origen
✅ Permisos controlados estrictamente
✅ Admin puede monitorear sin interferir
```

---

## 7️⃣ Contacto y Seguimiento del Lead

**Actor:** Agente asignado

**Acciones:**
```
1. Agente inicia contacto:

   Opción A: Llamada
   - Click "Registrar llamada"
   - Ingresa:
     * Duración
     * Resultado (positivo/negativo/pendiente)
     * Notas
   - Timestamp automático

   Opción B: Email
   - Click "Enviar email"
   - Compose en modal
   - Cuerpo + archivo adjunto
   - Se registra como actividad

   Opción C: WhatsApp (si plan permite)
   - Link a chat de WhatsApp
   - Se registra en historial

   Opción D: Visita
   - Click "Agendar visita"
   - Fecha/hora
   - Se notifica al cliente (vía email/SMS)
   - Marca como "Visitará"

   Opción E: Nota interna
   - Solo para agente/admin
   - No se ve del lado del cliente

2. Cada acción genera LeadActividad:
   - TipoId: Llamada/Email/WhatsApp/Visita/Nota
   - Descripcion
   - Resultado
   - FechaProgramada (si aplica)
   - FechaRealizada (si aplica)
```

**Frontend:**
```
POST /leads/{id}/actividad
- Tipo de actividad
- Descripción
- Resultado
- Fecha (si es futura)

POST /leads/{id}/mensaje
- Mensaje
- Adjunto (opcional)
- EsDelCliente (false si lo escribe agente)
```

**Backend:**
```
POST /leads/{id}/actividad
- Valida permisos (agente o admin)
- Crea LeadActividad
- Registra en timeline
- Si es Visita agendada:
  * Envía email al cliente con fecha/hora
  * Crea recordatorio para agente (día anterior)

POST /leads/{id}/mensaje
- Si EsDelCliente=false:
  * Solo agente/admin pueden crear
  * Se envía email al cliente con contenido
- Si EsDelCliente=true:
  * Vino del formulario de contacto (paso 5)
- Se almacena en LeadMensaje
- Visible en timeline
```

**Resultado:**
```
✅ Toda interacción registrada
✅ Timeline completo y auditable
✅ Cliente recibe confirmaciones/recordatorios
✅ Agente tiene historial completo
```

---

## 8️⃣ Historial de Estados del Lead

**Actor:** Sistema + Agente

**Acciones:**
```
Estados posibles (personalizables según plan):
- Nuevo          (creado automáticamente)
- Contactado     (agente lo llamó/emailed)
- Interesado     (cliente mostró interés)
- Visitó         (cliente visitó la propiedad)
- En Gestión     (seguimiento activo)
- Ganado         (cerró operación, vendido/alquilado)
- Perdido        (cliente rechazó o no responde)
- Sin Contacto   (automatización: sin actividad X horas)

Cambio de estado:
1. Agente va a Lead → Click "Cambiar estado"
2. Dropdown con estados disponibles
3. Selecciona nuevo estado
4. Opcional: nota de transición
5. Click confirmar

Sistema registra automáticamente:
- Estado anterior
- Estado nuevo
- Agente que hizo cambio
- Timestamp
- En tabla LeadEstadoHistorial (INMUTABLE)
```

**Timeline ejemplo:**
```
14:30 - Lead creado por sistema (Nuevo)
14:32 - Agente registra llamada (Contactado)
14:45 - Agente registra visita programada (Interesado)
16:00 - Cliente visita propiedad
16:15 - Agente registra visita realizada (Visitó)
19:30 - Cliente envía email diciendo que quiere
20:00 - Agente marca como Ganado
20:05 - Sistema actualiza propiedad a Vendida
```

**Frontend:**
```
GET /leads/{id}/historial       → timeline de estados
POST /leads/{id}/estado         → cambiar estado
```

**Backend:**
```
POST /leads/{id}/estado
- Valida transición válida
- Crea registro en LeadEstadoHistorial (INSERT, nunca UPDATE)
- Actualiza Lead.EstadoActual
- Si nuevo estado = "Ganado" o "Vendido":
  * Dispara actualización de propiedad
  * Cambio de estado: Vendida/Alquilada

GET /leads/{id}/historial
- Retorna todos los registros de LeadEstadoHistorial
- Ordena por fecha DESC
- Información: estado, agente, timestamp, notas
- Inmutable (solo lectura)
```

**Visibilidad:**
```
Agente:
  ✅ Ve su historial
  ✅ Puede cambiar estado

Admin:
  ✅ Ve historial completo
  ✅ Puede forzar cambio de estado

Cliente final:
  ❌ No ve historial (privado)
```

**Resultado:**
```
✅ Historial completo e inmutable
✅ Trazabilidad legal/auditoria
✅ Sin borrados, solo registros nuevos
✅ Decisiones documentadas
```

---

## 9️⃣ Automatizaciones (Planes PRO+)

**Actor:** Sistema

**Condiciones:**
```
Automatización 1: Sin contacto
- Trigger: Lead existe hace X horas sin actividad
- Acción: Cambiar estado a "Sin Contacto"
- Config: X horas (default 24)
- Plan: PRO+

Automatización 2: Visita registrada
- Trigger: Se registra actividad tipo "Visita"
- Acción: Cambiar estado a "Visitó"
- Plan: PRO+

Automatización 3: Múltiples intentos fallidos
- Trigger: N intentos de contacto sin respuesta
- Acción: Cambiar estado a "Perdido"
- Config: N intentos (default 3)
- Plan: ENTERPRISE

Automatización 4: Lead ganado → Propiedad vendida
- Trigger: Lead marcado como "Ganado"
- Acción:
  * Propiedad → Estado = "Vendida/Alquilada"
  * Propiedad → Publicada = false (despublicar)
  * Notificar a inmobiliaria/agente
- Plan: Todos

Automatización 5: Recordatorio de visita
- Trigger: Visita agendada para mañana
- Acción: Enviar email recordatorio a cliente
- Plan: PRO+
```

**Implementación:**
```
Backend (Hangfire / Background Jobs):
- Job ejecuta cada X minutos
- Busca leads con condiciones
- Aplica automatización
- Registra en LeadEstadoHistorial
- Envía notificaciones

Config en appsettings.json:
{
  "Automations": {
    "SinContactoHoras": 24,
    "IntentosFailSinRespuesta": 3,
    "RecordatorioVisitasMinutosAntes": 1440
  }
}

Permisos:
- Admin puede habilitar/deshabilitar por inmobiliaria
- Configurar parámetros (si plan permite)
```

**Resultado:**
```
✅ Leads no se olvidan
✅ Workflow automático según plan
✅ Ahorra tiempo a agentes
✅ Consistencia en procesos
```

---

## 🔟 Cierre de Operación

**Actor:** Agente

**Acciones:**
```
1. Acuerdo comercial finalizado
2. Agente va a Lead → Cambiar estado
3. Selecciona: "Ganado" (operación vendida/alquilada)
4. Opcional: Comisión registrada, notas
5. Click confirmar

Sistema automáticamente:
- Registra en LeadEstadoHistorial
- Actualiza Propiedad:
  * Estado = "Vendida" o "Alquilada"
  * Publicada = false
  * PublicadaEn = null
- Oculta de portal público
- Envía email de confirmación a inmobiliaria
- Genera reporte de comisión (si admin)
- Registra en histórico de agente
```

**Histórico:**
```
Propiedad NO se elimina:
✅ Se oculta del portal
✅ Se mantiene en base de datos
✅ Disponible en histórico
✅ Auditable: cuándo se vendió, quién cerró, comisión
```

**Métricas Actualizadas:**
```
Dashboard del agente:
- ✅ Leads cerrados +1
- ✅ Propiedades vendidas +1
- ✅ Comisión acumulada +$X

Dashboard de inmobiliaria:
- ✅ Operaciones cerradas +1
- ✅ Ingresos +$X
- ✅ Performance por agente
```

**Resultado:**
```
✅ Operación cerrada
✅ Propiedad fuera del mercado
✅ Historial completo conservado
✅ Comisiones registradas
✅ Métricas actualizadas
```

---

## 1️⃣1️⃣ Facturación y Planes (SaaS)

**Actor:** Admin de inmobiliaria + Sistema

**Ciclo de Facturación:**
```
Mes 1: Registro
- Plan: FREE
- Costo: $0
- Suscripción creada

Mes N: Uso del plan
- MaxUsuarios: 2
- MaxPropiedades: 5
- MaxLeads/mes: 10
- MaxImagenes/propiedad: 5

Límite alcanzado:
- Usuario intenta crear usuario #3
- Backend: 402 Conflict
- Mensaje: "Límite de usuarios alcanzado. Upgrade a PRO"
- Admin ve opción de upgrade en dashboard

Upgrade PRO:
- Admin click "Mejorar plan"
- Elige: Mensual ($49) o Anual ($490)
- Pago con Mercado Pago (fase 2)
- Confirmación: Suscripción actualizada
- Beneficios activos: maxUsuarios=5, maxPropiedades=20, etc
- Datos NO se pierden

Downgrade:
- Admin puede bajar de plan
- Datos se mantienen
- Solo se bloquean features nuevas
- Ejemplo: bajó de PRO a FREE
  * Propiedades existentes: se mantienen
  * Usuarios extra: se desactivan
  * Features PRO: se desactivan (CRM avanzado)
```

**Planes:**
```
FREE
├─ MaxUsuarios: 2
├─ MaxPropiedades: 5
├─ MaxLeads/mes: 10
├─ MaxImagenes/prop: 5
├─ CRM básico: Solo estados estándar
├─ Sin automatizaciones
├─ Sin reportes avanzados
└─ Costo: $0

PRO
├─ MaxUsuarios: 5
├─ MaxPropiedades: 20
├─ MaxLeads/mes: 50
├─ MaxImagenes/prop: 20
├─ CRM avanzado: Estados personalizados
├─ Automatizaciones básicas
├─ Reportes por agente
├─ Dominio personalizado (opcional)
├─ WhatsApp integrado
└─ Costo: $49/mes o $490/año

ENTERPRISE
├─ MaxUsuarios: Ilimitado
├─ MaxPropiedades: Ilimitado
├─ MaxLeads/mes: Ilimitado
├─ MaxImagenes/prop: Ilimitado
├─ Todas las features
├─ Automatizaciones complejas
├─ Analytics avanzados
├─ Integraciones personalizadas
├─ Soporte prioritario
└─ Costo: Custom (contacto directo)
```

**Validación de Límites:**
```
Backend - Cada creación valida:

POST /usuarios
- Cuenta usuarios activos
- Si count >= planLimit.MaxUsuarios
  → 402 Conflict
  → Mensaje: "Límite de usuarios alcanzado"

POST /propiedades
- Cuenta propiedades activas
- Si count >= planLimit.MaxPropiedades
  → 402 Conflict
  → Mensaje: "Límite de propiedades alcanzado"

POST /propiedades/{id}/imagenes
- Cuenta imágenes de esta propiedad
- Si count >= planLimit.MaxImagenesPorPropiedad
  → 402 Conflict
  → Mensaje: "Límite de imágenes por propiedad alcanzado"

POST /leads
- Cuenta leads creados este mes
- Si count >= planLimit.MaxLeadsPorMes
  → 402 Conflict
  → Mensaje: "Límite de leads mensuales alcanzado"
```

**Resultado:**
```
✅ Facturación clara por plan
✅ Upgrade/downgrade sin pérdida de datos
✅ Límites hardcodeados en backend
✅ Escalabilidad predecible
✅ Monetización B2B SaaS estándar
```

---

## 1️⃣2️⃣ Monitoreo y Escalabilidad

**Actor:** Admin + Sistema

**Registros y Métricas:**
```
Dashboard Admin - Uso Mensual:
┌─────────────────────────────────┐
│ Usuarios: 3/5                   │
│ Propiedades: 8/20               │
│ Leads creados: 24/50            │
│ Imágenes subidas: 145/400       │
│ Almacenamiento: 2.3 GB / 50 GB  │
└─────────────────────────────────┘

Dashboard de Agentes:
- Leads asignados: 12
- Leads cerrados: 3
- Tasa de cierre: 25%
- Propiedades activas: 8
- Propiedades vendidas: 1
- Comisión acumulada: $X

Historial de Actividad:
- Última propiedad publicada: hoy
- Último lead creado: hace 2 horas
- Última actividad de agente: hace 1 hora

Reportes (si plan permite):
- Por agente (leads, cierre, comisión)
- Por propiedad (visitas, interés)
- Temporal (MoM, YoY)
- Por fuente (portal, llamada, referral)
```

**Base de Datos:**
```
Tablas principales registran:
- CreatedAt: cuando se creó
- UpdatedAt: última modificación
- DeletedAt: soft delete (si aplica)
- ChangedBy: quién hizo el cambio
- Changelog: JSON con historial de cambios

Auditoría:
- Toda acción registrada
- Usuario responsable
- Timestamp exacto
- Cambios anteriores conservados
```

**Escalabilidad:**
```
Fase 1 (MVP):
- 100 inmobiliarias
- 1000 propiedades
- 10K leads

Fase 2:
- Sharding por InmobiliariaId
- Read replicas para reportes
- Cache Redis para búsquedas
- Async jobs (imágenes, emails)

Fase 3:
- ML para recommendations
- Predictive analytics
- Marketplace de servicios
```

**Resultado:**
```
✅ Monitoreo completo de uso
✅ Auditoría legal/compliance
✅ Datos para decisiones
✅ Visibilidad de performance
```

---

## 1️⃣3️⃣ (Opcional) Agentes Independientes

**Actor:** Corredor sin inmobiliaria

**Diferencia:**
```
Inmobiliaria Tradicional:
├─ Dueño/Gerente registra inmobiliaria
├─ Invita agentes
├─ Paga suscripción
├─ Admin gestiona

Agente Independiente (Corredor):
├─ Se registra como "Corredor" (tenant individual)
├─ juancorredor.centroinmo.com
├─ Es el único admin
├─ Puede invitar asistentes (si plan permite)
├─ Paga suscripción personal
├─ Funciona como inmobiliaria de 1 persona
```

**Registro:**
```
POST /auth/register-corredor (variante de register-inmobiliaria)
- Nombre: "Juan Corredor"
- Subdominio: juancorredor (único)
- Email, teléfono
- Role automático: Admin (no hay otro usuario)
- Plan: FREE
- Acceso: juancorredor.centroinmo.com
```

**Límites:**
```
FREE (corredor):
├─ 1 usuario (el corredor)
├─ 5 propiedades
├─ 10 leads/mes
├─ Costo: $0

PRO (corredor):
├─ 1 usuario principal + 1 asistente
├─ 20 propiedades
├─ 50 leads/mes
├─ Costo: $29/mes (descuento vs inmobiliaria)
```

**Diferencias en UI:**
```
vs Inmobiliaria:
- Sin sección "Equipo" (solo él es el usuario)
- Sin invitaciones
- Botones de "Actualizar perfil" en lugar de "Config empresa"
- Dashboard simplificado
```

**Resultado:**
```
✅ Corredores independientes pueden usar plataforma
✅ Modelo B2B2C (negocio a negocio a consumidor)
✅ Monetización adicional
✅ Mismo backend, distinto frontend
```

---

## ✅ Resultado Final

**Inmobiliaria:**
```
✔ Gestiona su equipo en un solo sistema
✔ Publica propiedades automáticamente en portal público
✔ Recibe leads de múltiples fuentes
✔ Agentes gestionan leads sin interferencias
✔ Historial completo de cada operación
✔ Métricas en tiempo real
✔ Escala según plan (pay-as-you-grow)
```

**Agente:**
```
✔ Acceso simple con sus credenciales
✔ Solo ve sus leads y propiedades asignadas
✔ Registra todas las interacciones
✔ Trazabilidad de comisiones
✔ Dashboard de performance
✔ Notificaciones de nuevos leads
```

**Usuario Final (Comprador/Locatario):**
```
✔ Busca propiedades en centroinmo.com
✔ Ve múltiples inmobiliarias
✔ Contacta agentes sin registro
✔ Seguimiento de consultas (si se registra)
```

**SaaS (Negocio):**
```
✔ Modelo B2B claro
✔ Monetización por plan y features
✔ Escalabilidad predecible
✔ Defensibilidad (datos cautivos)
✔ Upsells y cross-sells (fase 2)
✔ Múltiples segmentos (inmobiliarias + corredores)
```

---

## 📊 Diagrama de Relaciones

```
┌──────────────────────────────────────────────────────────┐
│  PLATAFORMA: centroinmo.com                            │
│                                                          │
│  Portal Público (sin login)                              │
│  ├─ GET /propiedades (todas las publicadas)             │
│  └─ POST /leads (usuario final contacta)                │
│                                                          │
└──────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────┐
│  INMOBILIARIAS (Tenants)                                 │
│                                                          │
│  acme.centroinmo.com     recoleta.centroinmo.com    │
│  ├─ Propiedades: 8         ├─ Propiedades: 12           │
│  ├─ Agentes: 3             ├─ Agentes: 5                │
│  └─ Leads: 24              └─ Leads: 45                 │
│                                                          │
└──────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────┐
│  AGENTES (Usuarios dentro de Inmobiliaria)               │
│                                                          │
│  Usuario: pedro@acme.com                                 │
│  ├─ Propiedades asignadas: 3                            │
│  ├─ Leads activos: 12                                   │
│  ├─ Leads cerrados: 3                                   │
│  └─ Comisión: $5000                                     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 🔄 Ciclomaticidad de Datos

```
Flujo de Datos Entrada-Salida:

ENTRADA:
1. Usuario Final registra en formulario
   → Crea Lead
   → Genera email a agente
   → Notificación en dashboard

2. Agente publica propiedad
   → Aparece en portal público
   → Lead puede contactar
   → Ciclo closes

SALIDA (Reportes):
3. Admin genera reporte
   → Agrega leads por mes
   → Suma propiedades vendidas
   → Calcula comisiones
   → CSV/PDF

4. Dashboard en tiempo real
   → Actualiza métricas
   → Muestra actividad reciente
   → Alertas de límites próximos
```

---

---

## 🆕 FEATURE EN DESARROLLO: Particulares Publican Propiedades

> ⚠️ **Estado:** Fase 2 (En diseño)
> **Objetivo:** Permitir que particulares sin inmobiliaria publiquen sus propiedades pagando con Mercado Libre

### 📍 Flujo Simplificado

```
Particular registra → Carga datos propiedad → Sube max 5 imágenes
   → Elige duración (30/60/90 días) → Calcula precio ($200/$350/$450)
   → Paga con botón Mercado Libre → Si aprobado: Publicada en portal
   → Recibe leads/contactos → Responde directamente
```

### 🏷️ Entidades Nuevas

```
PropiedadParticular: Id, Email, Nombre, Telefono, PropiedadId, FechaExpiracion, MontoAbonado
PagoPublicacionParticular: Id, PropiedadParticularId, MercadoLibreId, Estado, Monto, Fecha
```

### 💳 Integración Mercado Libre

```
POST /particulares/pagar
- Crea preferencia MP con monto
- Retorna URL checkout
- Webhook confirma pago
- Si approved: Propiedad se publica automáticamente
```

### 📊 Montos y Duración

```
30 días  → $200 ARS
60 días  → $350 ARS
90 días  → $450 ARS

Expiración: Sistema verifica cada 6h, despublica si pasó fecha
```

### ✅ Resultado

```
✅ Particulares publican sin costo de suscripción
✅ Monetización por publicación ($200-450 por propiedad/mes)
✅ Control de calidad (solo publicadas si pagan)
✅ Renovación fácil (botón en dashboard)
```

---

## 🆕 FEATURE PREMIUM: Tenant Destacado (Marketplace de Oportunidades)

> 🟡 **Estado:** Fase 2-3 (Compleja - Requiere IA)
> **Objetivo:** Inmobiliarias SELECCIONADAS ven datos de particulares + compradores para negociar arbitraje

### 🎯 Concepto: Ganar con Diferencial

```
SITUACIÓN:
Particular quiere vender propiedad por $100k (podría vender por $80k)
Comprador tiene $120k de presupuesto

ANTES (Sin feature):
- Particular vende por $100k (sin saber podría vender más)
- Comprador no se conecta
- Oportunidad perdida

DESPUÉS (Con Tenant Destacado):
- Inmobiliaria X ve que particular puede vender por $105k
- Inmobiliaria X ve que comprador tiene $120k disponible
- Negocia: Particular a $105k, Comprador a $107k
- Inmobiliaria gana: $2k de diferencial (2% comisión)
```

### 🏢 Nuevo Campo en Inmobiliaria

```sql
ALTER TABLE Inmobiliaria ADD (
  EsDestacada TINYINT DEFAULT 0,      -- Feature premium
  PrecioComisionPorcentaje DECIMAL(5,2) -- % que cobra (2-5%)
);

Solo SuperAdmin puede asignar (es feature ENTERPRISE)
```

### 📊 Nuevas Tablas

```
OportunidadArbitraje:
- Id, InmobiliariaDestacadaId, PropiedadParticularId
- EstimacionPrecioVenta (IA predice)
- PrecioSolicitado
- MargenOportunidad (%)
- NivelConfianza (0-100)
- Estado: detectada/contactado/negociando/cerrada/expirada
- FechaExpiracion (30 días vigencia)

PerfilComprador:
- Id, Email, Nombre, Presupuesto (min/max)
- Ubicacion, TipoPropiedad, Caracteristicas (JSON)
- UrgenciaCompra, EstadoInteres

MatchOportunidad:
- OportunidadArbitrajeId, CompradorId
- PrecioProposalVenta, Diferencial, ComisionInmobiliaria
- EstadoNegociacion: propuesta/aceptada/cerrada
```

### 🔄 Flujo Completo

**Fase 1: Detección Automática**
```
1. Particular publica propiedad ($100k)
2. IA Valúa: Mercado estima $120k (confianza 85%)
3. Sistema crea OportunidadArbitraje
4. Dashboard Tenant Destacado:
   "Casa Belgrano: $100k → Mercado $120k"
   "Margen: 20% ($20k) | Confianza: 85%"
   [CONTACTAR PARTICULAR]
```

**Fase 2: Negociación con Particular**
```
1. Admin del Tenant click "CONTACTAR PARTICULAR"
2. Email: "Hola, somos inmobiliaria X. Tu propiedad nos interesa."
3. Particular puede aceptar/rechazar
4. Si acepta: conversación privada
```

**Fase 3: Buscar Compradores**
```
1. Sistema matchea OportunidadArbitraje vs PerfilesComoradores
2. Encontró: Comprador A ($120k), Comprador C ($140k)
3. Dashboard muestra: "2 compradores con interés en esta zona"
```

**Fase 4: Cerrar Operación**
```
1. Admin negocia:
   - Particular: "Podemos venderlo a $105k"
   - Comprador: "Tenemos una casa a $107k"
2. Se crea MatchOportunidad
3. Diferencial: $107k - $105k = $2k
4. Comisión: $2k × 2% = $40 ganancia inmobiliaria
5. Propiedad vendida, operación cerrada
```

### 🤖 IA Predictions

```
Para cada PropiedadParticular, IA analiza:
- Ubicación (zona, seguridad, transporte)
- Características (edad, estado, amenities)
- Comparables históricos
- Tendencia de precios

Output:
- PrecioRealMercado: $120k ± $5k
- NivelConfianza: 85%
- TiempoVentaPromedio: 45 días
- Recomendación: "Precio competitivo. Margen de negociación."
```

### 📈 Dashboard Tenant Destacado

```
┌───────────────────────────────────────────────┐
│ TENANT DESTACADO: Centro Inmobiliario        │
│                                              │
│ 📈 Oportunidades: 23 detectadas / 7 cerradas │
│ 💰 Ganancia mes: $45,230                     │
│ 📊 Tasa cierre: 30%                          │
│                                              │
│ 🎯 ACTIVAS:                                   │
│ • Casa Belgrano: $100k → $120k (20%)         │
│   [2 compradores] [EN NEGOCIACIÓN]           │
│                                              │
│ • Depto Recoleta: $80k → $95k (19%)          │
│   [4 compradores] [DETECTADA]                │
│                                              │
│ 📉 CERRADAS (Historial):                      │
│ • Casa Belgrano: $105k → $110k (+$5k)        │
│ • Depto San Telmo: $120k → $135k (+$12k)     │
│ • Lote Flores: $60k → $68k (+$6k)            │
│                                              │
└───────────────────────────────────────────────┘
```

### 💳 Monetización Híbrida (Recomendada)

```
Base: $999/mes (acceso a feature Tenant Destacado)
Variable: 2% del diferencial cerrado

Ejemplo:
- Mes 1: $999 + $1,200 (4 ops cerradas) = $2,199
- Mes 2: $999 + $600 (2 ops) = $1,599

Beneficio: Ingresos seguros + incentivo a negociar bien
```

### 🔒 Restricciones

```
✅ Solo Tenant ENTERPRISE puede acceder
✅ SuperAdmin asigna (no es automático)
✅ Máx 1 oportunidad por particular/mes
✅ Máx 3 contactos por oportunidad
✅ Oportunidades expiran en 30 días
✅ Datos de particular protegidos
✅ Historial auditable
```

### 🛠️ Endpoints Principales

```
GET  /oportunidades                 → listar activas
GET  /oportunidades/{id}/compradores → matching
POST /oportunidades/{id}/contactar   → contactar particular
POST /oportunidades/{id}/cerrar      → registrar operación
GET  /dashboard/destacado            → dashboard
GET  /predicciones/{propiedadId}     → IA analysis
```

### ✅ Resultado Final

```
Para Particular:      Vende más caro, menos tiempo en mercado
Para Comprador:       Compra más barato, mayor selección
Para Inmobiliaria:    Gana comisión por arbitraje + ingresos base
Para Plataforma:      Feature premium con monetización clara
```

---

## 📝 Notas de Implementación

**Prioridad MVP:**
1. ✅ Auth + Tenant creation
2. ✅ CRUD Propiedades + Publicación
3. ✅ Portal Público + Creación de Leads
4. ✅ CRM básico (leads + estados)
5. ✅ Dashboard con métricas
6. ⏳ Planes + Validación de límites
7. ⏳ SuperAdmin

**Fase 2:**
- 🆕 Particulares publican (pago $200-450 con MP)
- 🆕 Tenant Destacado (feature premium $999+)
- 🆕 IA Valuaciones automáticas
- Automatizaciones avanzadas
- WhatsApp API
- Analytics

**Fase 3:**
- Marketplace público de oportunidades
- Integraciones externas (Inmuebles.com, etc)
- Mobile app
- Bot IA conversacional
- Reportes avanzados
