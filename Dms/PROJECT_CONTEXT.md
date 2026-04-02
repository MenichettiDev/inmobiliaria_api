# PROJECT_CONTEXT.md — InmobiliariaSaaS

## 📌 Overview

SaaS B2B para inmobiliarias.

Cada inmobiliaria = tenant.

Funcionalidades:

- gestión de propiedades
- CRM de leads
- gestión de clientes
- usuarios internos
- planes con límites

---

## 🧱 Arquitectura

Monolito modular:

Frontend (Angular SSR) → API (ASP.NET Core) → MariaDB

Patrón:

Controller → Service → Repository

---

## 👥 Multi-tenancy

Modelo: shared schema

- columna: id_inmobiliaria
- tenant desde JWT (claim)
- fallback: subdominio

---

## 🔐 Auth

- JWT con roles
- sin refresh token (pendiente)

---

## ⚠️ Problemas críticos

1. ❌ Sin refresh tokens
2. ❌ Sin billing real
3. ❌ Sin onboarding self-service
4. ❌ Sin cache de tenant
5. ❌ Posibles queries sin tenant enforcement
6. ❌ Imágenes en storage local
7. ❌ Sin rate limiting

---

## 🚀 Estado SaaS

- Multi-tenant: OK (medio)
- Auth: incompleto
- Billing: no implementado
- Escala: limitada
- Seguridad: media-baja

---

## 📂 Backend

- ASP.NET Core 8
- EF Core
- MariaDB
- JWT auth

Problemas:

- falta refresh tokens
- falta cache tenant
- posibles inconsistencias en filtros

---

## 🎨 Frontend

- Angular 19 SSR
- interceptor JWT
- lazy loading

Problemas:

- no maneja refresh token
- modelo de roles inconsistente
- falta manejo global de errores

---

## 🔗 Integración

- REST JSON
- posible inconsistencia naming
- falta estandarización completa

---

## ⚠️ Riesgos

- sesiones inseguras
- no monetiza
- no escala horizontalmente
- alto riesgo de fuga de datos si falla tenant filter

---

## 🧭 Roadmap resumido

1. Refresh tokens
2. Tenant cache
3. Rate limiting
4. Storage externo
5. Billing (MercadoPago)
6. Onboarding
7. Auditoría
8. Observabilidad

---

## 🎯 NEXT ACTION (OBLIGATORIO)

Implementar:

REFRESH TOKEN SYSTEM

Requisitos:

- tabla refresh_tokens
- endpoint POST /api/auth/refresh
- rotación de tokens
- invalidación del anterior
- expiración segura

No avanzar a otra tarea hasta completar esto.
