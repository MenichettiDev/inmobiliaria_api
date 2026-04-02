# Implementación de Multi-Tenancy - Contexto de Inmobiliaria

## === RESUMEN DE IMPLEMENTACIÓN ===

El sistema implementa multi-tenancy a nivel de subdominio donde cada inmobiliaria tiene acceso únicamente a sus propios datos. La arquitectura garantiza aislamiento completo entre inquilinos.

## === TENANT CONTEXT ===

### **TenantContext Service**
- **Ubicación**: `Services/TenantContext.cs`
- **Interfaz**: `ITenantContext`
- **Registro**: Scoped service en DI container
- **Función**: Resuelve automáticamente la inmobiliaria actual basada en el subdominio

### **Métodos Principales**:

#### `GetCurrentSubdomain()`
- Extrae subdominio de la URL (ej: `demo.inmobiliaria.com` → `demo`)
- Para desarrollo local: usa header `X-Subdomain`
- Cachea resultado para optimización

#### `GetCurrentTenantId()`
- Resuelve ID de inmobiliaria basado en subdominio
- Valida que la inmobiliaria esté activa
- Cachea para evitar múltiples consultas DB
- Lanza excepción si no encuentra inmobiliaria

#### `GetCurrentInmobiliariaAsync()`
- Retorna entidad completa de inmobiliaria
- Incluye relaciones (Plan, Estado)
- Útil para validaciones de límites

## === SEGURIDAD Y FILTRADO AUTOMÁTICO ===

### **Repository Level**
Todos los repositories automáticamente inyectan `tenant_id` en consultas:

```csharp
var tenantId = _tenantContext.GetCurrentTenantId();
var query = _dbSet.Where(entity => entity.IdInmobiliaria == tenantId);
```

### **Métodos Sobrescritos**
- `GetAllAsync()`: Filtra por tenant + estado activo
- `GetByIdAsync()`: Valida pertenencia al tenant
- `CreateAsync()`: Inyecta automáticamente `IdInmobiliaria`

### **Validaciones Cross-Tenant**
- `ValidatePropertiesBelongsToTenantAsync()`: Valida propiedad
- `ValidateUsuarioAsignadoBelongsToTenantAsync()`: Valida usuario

## === EXCEPCIONES PERSONALIZADAS ===

### **LimiteDeLeadsExcedidoException**
- Se lanza cuando se alcanza límite mensual del plan
- Incluye: límite actual, leads usados, nombre del plan
- Usado en: `UsoMensualService.ValidarLimiteLeadsAsync()`

### **InmobiliariaInactivaException**
- Se lanza cuando inmobiliaria no está activa
- Incluye: ID inmobiliaria, estado actual
- Usado en: validaciones de operaciones

### **TenantAccessViolationException**
- Se lanza en intentos de acceso cross-tenant
- Incluye: tenant actual, tenant solicitado
- Usado en: validaciones de seguridad

### **LeadNotFoundException**
- Se lanza cuando lead no existe o no pertenece al tenant
- Incluye: ID del lead
- Usado en: operaciones sobre leads específicos

## === FLUJO DE AUTENTICACIÓN Y AUTORIZACIÓN ===

### **1. Request Llega al Sistema**
```
Request: GET https://demo.inmobiliaria.com/api/leads
Header: Authorization: Bearer JWT_TOKEN
```

### **2. TenantContext se Activa**
- Middleware extrae subdominio: `demo`
- Busca inmobiliaria con subdominio `demo`
- Cachea `tenant_id` para la request

### **3. Repository Filtra Automáticamente**
```sql
SELECT * FROM leads 
WHERE id_inmobiliaria = :tenant_id 
AND id_estado_admin != 3;
```

### **4. Service Layer Valida**
- Verifica límites del plan
- Valida estado de inmobiliaria
- Ejecuta lógica de negocio

## === CONFIGURACIÓN PARA DESARROLLO ===

### **Configuración Local**
Para desarrollo local sin subdominio real:

```javascript
// Frontend - Agregar header en requests
headers: {
  'X-Subdomain': 'demo',
  'Authorization': 'Bearer ' + token
}
```

### **Configuración Docker/Testing**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - TENANT_HEADER_MODE=true
```

## === PERFORMANCE Y CACHÉ ===

### **Optimizaciones Implementadas**
1. **Caché de TenantId**: Una consulta por request
2. **Caché de Subdominio**: Evita parsing múltiple
3. **Lazy Loading**: Solo consulta cuando necesita

### **Recomendaciones Futuras**
1. **Redis Cache**: Para tenant context en producción
2. **Connection String per Tenant**: Para mayor aislamiento
3. **Database Sharding**: Para escala masiva

## === PUNTOS DE EXTENSIÓN ===

### **Agregar Nueva Entidad**
1. Implementar Repository que herede de `GenericRepository<T>`
2. Inyectar `ITenantContext` en constructor
3. Sobrescribir métodos para filtrar por `IdInmobiliaria`
4. Registrar en DI container

### **Validaciones Adicionales**
```csharp
public async Task<bool> ValidateEntityBelongsToTenantAsync<T>(int entityId) 
    where T : class, ITenantEntity
{
    var tenantId = _tenantContext.GetCurrentTenantId();
    return await _context.Set<T>()
        .AnyAsync(e => e.Id == entityId && e.IdInmobiliaria == tenantId);
}
```

## === TESTING MULTI-TENANCY ===

### **Unit Tests**
- Mockear `ITenantContext`
- Testear filtrado automático
- Validar excepciones cross-tenant

### **Integration Tests**
- Configurar múltiples tenants
- Testear aislamiento de datos
- Validar headers/subdominio

Esta implementación garantiza aislamiento completo de datos, seguridad robusta y escalabilidad para el crecimiento del SaaS.