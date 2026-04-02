# DESIGN_SYSTEM.md — ComproCar Design System

## 🎨 Estilo general
- Inspirado en apps de autos premium con soporte Dark/Light mode
- Bordes redondeados: 12px en cards, 8px en inputs y botones
- Sombras suaves en cards
- Glassmorphism en formularios de auth

---

## 🌙 Paleta Dark Mode

| Token | Valor | Uso |
|---|---|---|
| --bg-primary | #0F1117 | Fondo principal |
| --bg-surface | #1E2235 | Cards y paneles |
| --bg-elevated | #252A3D | Modales y dropdowns |
| --color-primary | #3B82F6 | Botones, links, activos |
| --color-primary-hover | #2563EB | Hover de botones |
| --text-primary | #F1F5F9 | Texto principal |
| --text-secondary | #94A3B8 | Texto secundario |
| --text-muted | #475569 | Placeholder, labels |
| --border | #2D3348 | Bordes de cards e inputs |
| --success | #10B981 | Estados positivos |
| --warning | #F59E0B | Alertas |
| --danger | #EF4444 | Errores y eliminaciones |

---

## ☀️ Paleta Light Mode

| Token | Valor | Uso |
|---|---|---|
| --bg-primary | #F5F7FF | Fondo principal |
| --bg-surface | #FFFFFF | Cards y paneles |
| --bg-elevated | #F8FAFC | Modales y dropdowns |
| --color-primary | #2563EB | Botones, links, activos |
| --color-primary-hover | #1D4ED8 | Hover de botones |
| --text-primary | #1E293B | Texto principal |
| --text-secondary | #64748B | Texto secundario |
| --text-muted | #94A3B8 | Placeholder, labels |
| --border | #E2E8F0 | Bordes de cards e inputs |
| --success | #059669 | Estados positivos |
| --warning | #D97706 | Alertas |
| --danger | #DC2626 | Errores y eliminaciones |

---

## 📐 Espaciado y bordes

| Token | Valor | Uso |
|---|---|---|
| --radius-sm | 6px | Badges, chips |
| --radius-md | 8px | Inputs, botones |
| --radius-lg | 12px | Cards |
| --radius-xl | 16px | Modales, panels grandes |
| --shadow-sm | 0 1px 3px rgba(0,0,0,0.12) | Cards en light |
| --shadow-md | 0 4px 16px rgba(0,0,0,0.24) | Cards en dark |
| --shadow-lg | 0 8px 32px rgba(0,0,0,0.32) | Modales |

---

## ✍️ Tipografía

| Elemento | Fuente | Tamaño | Peso |
|---|---|---|---|
| Logo | Inter | 24px | 700 |
| Título H1 | Inter | 28px | 700 |
| Título H2 | Inter | 22px | 600 |
| Título H3 | Inter | 18px | 600 |
| Body | Inter | 14px | 400 |
| Caption | Inter | 12px | 400 |
| Botón | Inter | 14px | 600 |

---

## 🧩 Componentes clave

### Botón primario
```css
background: linear-gradient(135deg, #3B82F6, #2563EB);
border-radius: 8px;
padding: 10px 24px;
font-weight: 600;
color: white;
border: none;
transition: all 0.2s ease;
```

### Card
```css
background: var(--bg-surface);
border-radius: 12px;
border: 1px solid var(--border);
box-shadow: var(--shadow-md);
padding: 20px;
```

### Input
```css
border: 1px solid var(--border);
border-radius: 8px;
background: var(--bg-elevated);
color: var(--text-primary);
padding: 12px 16px;
transition: border-color 0.2s ease;
```

### Sidebar
```css
background: #0F1117; /* siempre dark */
width: 260px;
position: fixed;
height: 100vh;
border-right: 1px solid #2D3348;
```

### Auth card (glassmorphism)
```css
background: rgba(30, 34, 53, 0.85);
backdrop-filter: blur(20px);
border: 1px solid rgba(255,255,255,0.08);
border-radius: 16px;
padding: 40px;
```

---

## 🔄 Dark/Light toggle

- Toggle en navbar arriba a la derecha
- Icono sol ☀️ en dark mode, luna 🌙 en light mode
- Preferencia guardada en localStorage
- Clase `.dark-theme` o `.light-theme` en el `<body>`
- Sidebar siempre dark independientemente del modo

---

## 🚗 Estilo específico para vehículos

### Card de vehículo (portal y listado)
```css
border-radius: 12px;
overflow: hidden; /* imagen llega al borde */
imagen: aspect-ratio 16/9, object-fit: cover
precio: color var(--color-primary), font-weight 700, 20px
badge estado: border-radius 20px, padding 4px 12px
```

### Galería de imágenes
- Imagen principal grande arriba
- Thumbnails abajo en fila horizontal
- Imagen activa con borde azul

---

## 📋 Reglas para el Frontend Agent

- Siempre usar tokens CSS en lugar de valores hardcodeados
- Sidebar SIEMPRE dark, sin importar el modo del sistema
- Nunca usar colores fuera de esta paleta
- Todos los bordes deben ser redondeados según los tokens
- Animaciones: máximo 200ms, ease-in-out
- Imágenes de autos: siempre aspect-ratio 16/9