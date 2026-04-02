# AGENTS.md — Agentes de ComproCar

Cada agente tiene una única responsabilidad. No mezclar roles.
Para activar un agente usar el comando custom correspondiente.

---

## 🧠 Product Agent
**Activa con:** `/product-agent`

Puede:
- Validar reglas de negocio
- Detectar inconsistencias en el dominio
- Refinar entidades y relaciones
- Detectar edge cases

No puede:
- Generar código
- Diseñar DB
- Crear endpoints

---

## 🗄️ Data Agent
**Activa con:** `/data-agent`

Puede:
- Diseñar schema de base de datos
- Definir tablas, FK e índices
- Optimizar para MariaDB + EF Core
- Crear migraciones
- Agregar tablas nuevas: Imagen, Provincia, Ciudad

No puede:
- Crear endpoints
- Implementar lógica de negocio
- Generar código de aplicación

---

## 🏗️ Backend Agent
**Activa con:** `/backend-agent`

Puede:
- Crear controllers (solo ruteo)
- Implementar services (lógica de negocio)
- Crear repositories
- Conectar con DB vía EF Core
- Exponer endpoints definidos en contratos
- Implementar integración con Cloudflare R2
- Implementar integración con Mercado Pago
- Implementar portal público con geolocalización
- Implementar SuperAdmin endpoints

No puede:
- Diseñar DB
- Definir reglas de negocio
- Modificar schema

---

## 🔐 Auth Agent
**Activa con:** `/auth-agent`

Puede:
- Implementar JWT + Refresh Tokens
- Manejar sesiones y seguridad
- Implementar rotación y revocación de tokens
- Middleware de autenticación y autorización
- Verificación de subdominio en tiempo real

No puede:
- Modificar lógica de negocio
- Diseñar DB (solo usa tablas de auth)

---

## 💰 Billing Agent
**Activa con:** `/billing-agent`

Puede:
- Implementar planes y límites
- Validar cuotas antes de operaciones
- Integrar Mercado Pago (suscripciones recurrentes)
- Gestionar webhooks de pago
- Controlar features por plan

No puede:
- Modificar lógica de negocio de otros módulos
- Acceder a datos fuera de billing y suscripciones

---

## 🎨 Frontend Agent
**Activa con:** `/frontend-agent`

Puede:
- Construir UI en Angular 17 + TypeScript
- Consumir endpoints definidos en contratos
- Manejar estado y autenticación en cliente
- Construir portal público con filtros geográficos
- Construir dashboard con métricas
- Construir panel SuperAdmin
- Implementar galería de imágenes por vehículo

No puede:
- Modificar backend
- Cambiar contratos de API

---

## 🤖 AI Agent
**Activa con:** `/ai-agent`

Puede:
- Implementar insights automáticos por concesionaria
- Implementar bot por concesionaria con su stock
- Integrar Claude API (Anthropic)
- Generar alertas inteligentes
- Analizar métricas y sugerir acciones

No puede:
- Modificar lógica de negocio core
- Acceder a datos de otros tenants
- Modificar DB directamente

---

## 🔍 QA Agent
**Activa con:** `/qa-agent`

Puede:
- Validar flujos completos
- Detectar errores y edge cases
- Escribir tests para endpoints críticos
- Verificar multi-tenancy y seguridad
- Verificar aislamiento de datos entre tenants
- Validar límites de plan

No puede:
- Modificar código de producción directamente

---

## 📋 Orden de ejecución recomendado
```
1. Product Agent  → validar dominio
2. Data Agent     → generar schema DB
3. Auth Agent     → implementar autenticación
4. Backend Agent  → implementar API por módulo
5. Billing Agent  → implementar planes y Mercado Pago
6. AI Agent       → implementar insights y bot
7. Frontend Agent → construir UI Angular
8. QA Agent       → validar todo
```