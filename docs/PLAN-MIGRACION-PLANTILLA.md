# Plan de migración · Plantilla React → Angular (Zezame)

> Documento vivo. Guía la traducción de la plantilla React (`plantilla-zezame/`, ignorada por git)
> a la app Angular (`zezame-frontend/`) y la lógica de backend (`zezame-backend/`) que la sostiene.
> **Objetivo de paridad:** cada página debe quedar *visualmente idéntica* a la de React.

---

## 1. Fuente de la verdad y reglas

- **Fuente visual:** `plantilla-zezame/src/` (Vite + React + shadcn/ui + Tailwind).
- **Regla de paridad:** se replican **exactamente** las mismas clases Tailwind, estructura DOM,
  textos, emojis, gradientes y estados. Si un dato es mock en React, en Angular se mantiene el
  mismo aspecto pero cableado a datos reales cuando exista endpoint (ver §3).
- **No romper lo que ya funciona:** el timer real (start/stop/pausa/reanudar + SignalR) ya opera.
  Cualquier cambio en `TimeEntry`/`TimeService` debe conservar ese flujo.
- **Commits:** los realiza el usuario. Cada fase propone un mensaje de commit al final.

---

## 2. Estado actual (inventario React → Angular)

| Módulo / Pieza | React (origen) | Angular (destino) | Estado |
|---|---|---|---|
| Design tokens (colores, fuentes, animaciones) | `src/index.css`, `tailwind.config.ts` | `src/styles.scss`, `tailwind.config.js` | ✅ Portado |
| Landing: Navbar | `components/Navbar.tsx` | `shared/navbar` | ✅ Portado |
| Landing: Hero | `components/HeroSection.tsx` | `shared/hero-section` | ✅ Portado |
| Landing: Features | `components/FeaturesSection.tsx` | `shared/feature-section` | ✅ Portado |
| Landing: **Stats** | `components/StatsSection.tsx` | — | ❌ Falta |
| Landing: **Testimonials** | `components/TestimonialsSection.tsx` | — | ❌ Falta |
| Landing: **CTA** | `components/CTASection.tsx` | — | ❌ Falta |
| Landing: Footer | `components/Footer.tsx` | `shared/footer` | ✅ Portado |
| Login/Registro | `components/LoginModal.tsx` | `auth/login-register` | ✅ Funcional (real) |
| Dashboard Layout (sidebar+topbar) | `components/DashboardLayout.tsx` | `dashboard/dashboard` | ⚠️ Portado con **datos mock** (usuario "María García") → cablear a `AuthService` |
| Página Timer | `pages/TimerPage.tsx` | `dashboard/pages/timer` | ⚠️ Parcial: timer real OK; faltan *Proyectos activos*, *Actividad reciente*, *Meta del día* y el diálogo *AssignActivity* |
| Diálogo desglose horas | `components/HoursBreakdownDialog.tsx` | `.../timer/components/break-down-dialog` | ⚠️ Portado con **datos mock** → cablear a agregaciones reales |
| Diálogo asociar actividad | `components/AssignActivityDialog.tsx` | — | ❌ Falta |
| Página Proyectos | `pages/Projects.tsx` | `dashboard/pages/projects` | ❌ Stub vacío |
| Página Calendario | `pages/CalendarPage.tsx` | `dashboard/pages/calendar` | ❌ Stub vacío |
| Página Reportes | `pages/Reports.tsx` | `dashboard/pages/reports` | ❌ Stub vacío |
| Página Ajustes | `pages/Settings.tsx` | `dashboard/pages/settings` | ❌ Stub vacío |
| NotFound | `pages/NotFound.tsx` | `shared/not-found` | ✅ Portado |

---

## 3. Decisiones transversales (leer antes de empezar)

### 3.1 Kit de UI (shadcn → Angular)
React usa primitivos shadcn (`Button`, `Card`, `Badge`, `Input`, `Switch`, `Separator`, `Label`, `Dialog`).
En Angular **no** los tenemos. Dos opciones:

- **A (recomendada):** crear un mini-kit propio en `shared/ui/` con componentes ligeros que
  reproduzcan solo las variantes que usa la plantilla (`app-card`, `app-badge`, un `appBtn`
  directiva/clases, etc.). Mantiene DRY y paridad.
- **B:** inline de clases Tailwind en cada plantilla (copia literal). Más rápido de arrancar,
  más repetición.

> **Propuesta:** empezar con **B literal por página** para garantizar paridad pixel a pixel y,
> en la Fase 8, refactorizar a un kit `shared/ui/` si aporta. Así no bloqueamos la paridad por
> diseñar un design-system primero.

### 3.2 Datos reales vs. mock (qué se cablea al back)
La plantilla es toda mock. Clasificación de qué pasa a datos reales y qué se deja como UI:

| Dato | ¿Real? | Notas |
|---|---|---|
| Usuario del layout (nombre, email) | ✅ Real | Desde `AuthService.currentUser$` (racha/nivel/avatar → placeholder de momento) |
| Timer, entradas del día, pausas | ✅ Real | Ya existe |
| Proyectos (lista, horas, progreso, miembros) | ✅ Real | Requiere ampliar entidad + endpoints (Fase 3) |
| Actividades por proyecto | ✅ Real | Endpoint `GET by project` (Fase 4) |
| Agregaciones día/semana/mes | ✅ Real | Endpoints de reporting (Fases 5–6) |
| Ranking de equipo | 🟡 Real si hay datos | Endpoint agregado por usuario; si no, placeholder |
| Racha, nivel, "Pro Timer ⚡", badges numéricos | 🟡 UI | Sin lógica de negocio aún; dejar como estático/placeholder |
| Notificaciones (toggles), Apariencia (acento) | 🟡 UI local | Persistir en `localStorage`; back opcional |
| Facturación / plan / método de pago | ⛔ Solo UI | Maqueta; **sin** integración de pagos |

### 3.3 Cambios de dominio + migración (backend)
La plantilla muestra campos que hoy **no** están en las entidades. Se ampliarán (Fase 3):

- `Project`: `Status` (`Active|Paused|Completed`), `Deadline` (DateTime?), `Emoji` (string?),
  y **derivados** calculados en servicio: `Hours` (suma de `DurationMin`), `Progress`
  (tareas hechas / totales), `MemberCount`, `TasksCompleted/Total`.
- `Activity`: `Emoji` (string?) para el diálogo de asociación.
- `TimeEntry`: al **iniciar** debe poder asociarse `ProjectId`/`ActivityId`
  (`StartEntryDto` hoy no los lleva).

> Todo cambio de entidad implica **nueva migración EF** (`dotnet ef migrations add ...`).
> Se documenta el comando en cada fase.

### 3.4 Iconos y gráficos
- Iconos: `lucide-react` → `lucide-angular` (ya instalado). Mismos nombres.
- Gráficos (Reportes/Calendario): la plantilla **no usa recharts** en estas páginas; son
  `div`/`svg` con Tailwind. Se replican igual, **sin** librería de charts.

### 3.5 Deuda técnica a resolver por el camino
- `EntryType` del front tiene `Lunch=2` inexistente en back → unificar a `Work|Break`.
- `dashboard.ts` arrastra mucho código muerto comentado → limpiar en Fase 1.
- `ProjectService.AddMemberAsync` **no persiste** el miembro → corregir en Fase 3.
- Registro permite rol arbitrario (check de Admin comentado) → endurecer en Fase 3/8.
- Secretos en `appsettings.json` → mover a user-secrets/env en Fase 8.

---

## 4. Fases

Cada fase es **independiente y commit-eable**. Orden recomendado por dependencias:
`0 → 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8`.

---

### Fase 0 — Cierre del landing (paridad rápida)
**Objetivo:** landing 100% idéntico al de React.

- **Frontend**
  - Crear `shared/stats-section` ← `StatsSection.tsx`
  - Crear `shared/testimonials-section` ← `TestimonialsSection.tsx`
  - Crear `shared/cta-section` ← `CTASection.tsx`
  - Añadirlos a `main/main.html` en el orden: navbar, hero, features, **stats, testimonials, cta**, footer.
- **Backend:** ninguno.
- **Aceptación:** `main` renderiza las 7 secciones en el mismo orden y estilo que `Index.tsx`.
- **Commit sugerido:** `feat-frontend: completar secciones del landing (stats, testimonials, cta)`

---

### Fase 1 — Dashboard Layout con datos reales
**Objetivo:** sidebar + topbar idénticos, pero usuario real.

- **Frontend**
  - `dashboard/dashboard.*`: sustituir `mockUser` por `AuthService.currentUser$`
    (nombre, email, iniciales/avatar placeholder). Mantener racha/nivel como placeholder visual.
  - Verificar badges del nav (Proyectos "4", etc.) → de momento estáticos como en React.
  - **Eliminar** el bloque grande de código comentado muerto en `dashboard.ts`.
- **Backend:** ninguno (opcional: endpoint `GET /api/auth/me`).
- **Aceptación:** el layout coincide con `DashboardLayout.tsx`; el saludo usa el nombre real logueado.
- **Commit sugerido:** `refactor-frontend: layout dashboard con usuario real y limpieza de código muerto`

---

### Fase 2 — Módulo Timer (página principal) completo
**Objetivo:** `TimerPage.tsx` replicado al 100%, con datos reales donde exista back.

- **Frontend**
  - Completar `timer.html`: secciones **Proyectos Activos**, **Actividad Reciente** y
    **Meta del día** (barra de progreso) según `TimerPage.tsx`.
  - Integrar **AssignActivityDialog** (Fase 4 lo hace real; aquí se maqueta el disparador `+`).
  - `break-down-dialog`: mantener maqueta hasta Fase 5/6 (se cablea allí).
- **Backend**
  - Ampliar `StartEntryDto` con `ProjectId?`/`ActivityId?` y persistirlos en `StartAsync`.
  - (Si se quiere "Actividad reciente" real) endpoint `GET /api/timeEntries/recent`.
- **Aceptación:** la página Timer coincide con la de React; iniciar timer puede llevar proyecto/actividad.
- **Commit sugerido:** `feat: timer con proyectos activos, actividad reciente y asociación de entrada`

---

### Fase 3 — Módulo Proyectos
**Objetivo:** `Projects.tsx` (grid/list, filtros, búsqueda, stats) con CRUD real.

- **Backend**
  - Entidad `Project`: añadir `Status`, `Deadline`, `Emoji`. Migración EF.
  - Repositorio: `GetAllWithStatsAsync(userId)` (horas, progreso, nº tareas, nº miembros).
  - **Corregir** `AddMemberAsync` (persistir `ProjectMember`; añadir método al repositorio).
  - Endpoints: `GET /api/projects` (lista con stats + filtro `status`), `GET /api/projects/{id}`,
    `PUT /api/projects/{id}`, `DELETE /api/projects/{id}` (soft-delete).
  - Endurecer creación por rol (Coordinator/Admin) — ya hay policy.
- **Frontend**
  - `projects.*`: header, fila de stats, filtros (`all/active/paused/completed`), búsqueda,
    toggle grid/list, tarjetas y filas idénticas a `Projects.tsx`.
  - Servicio `ProjectService` (front) con `list()`, `create()`, `update()`, `delete()`.
- **Aceptación:** paridad visual + lista real; filtros y toggle funcionan; crear proyecto persiste.
- **Migración:** `dotnet ef migrations add ProjectStatusDeadlineEmoji -p Zezame.Infrastructure -s Zezame.Api`
- **Commit sugerido:** `feat: módulo de proyectos (entidad ampliada, endpoints CRUD y página con grid/list)`

---

### Fase 4 — Módulo Actividades + AssignActivityDialog real
**Objetivo:** `AssignActivityDialog.tsx` cableado a actividades reales por proyecto.

- **Backend**
  - Entidad `Activity`: añadir `Emoji`. Migración EF.
  - Endpoint `GET /api/activities/by-project/{projectId}`.
  - (Ya existe `POST /api/activities` para "crear nueva actividad" desde el diálogo.)
- **Frontend**
  - Crear `dashboard/pages/timer/components/assign-activity-dialog` ← `AssignActivityDialog.tsx`
    (dos columnas: proyectos / actividades + buscador + "crear nueva").
  - Al asociar: setear proyecto/actividad de la sesión actual (y usarlos en `start()` de Fase 2).
- **Aceptación:** el diálogo lista proyectos y sus actividades reales; asociar afecta a la entrada.
- **Migración:** `dotnet ef migrations add ActivityEmoji -p Zezame.Infrastructure -s Zezame.Api`
- **Commit sugerido:** `feat: diálogo de asociación de actividad con datos reales por proyecto`

---

### Fase 5 — Módulo Calendario + desglose real (día)
**Objetivo:** `CalendarPage.tsx` con heatmap mensual y detalle de día reales.

- **Backend**
  - Endpoint `GET /api/reports/calendar?month=YYYY-MM` → por día: `hours`, `events`.
  - Endpoint `GET /api/reports/day?date=YYYY-MM-DD` → entradas del día (con pausas) y totales.
  - Cablear la pestaña **"day"** de `break-down-dialog` a este endpoint.
- **Frontend**
  - `calendar.*`: rejilla mensual con colores por intensidad, leyenda, panel de día,
    resumen semanal — idéntico a `CalendarPage.tsx`.
- **Aceptación:** el calendario muestra horas reales por día; el desglose "día" usa datos reales.
- **Commit sugerido:** `feat: módulo calendario con heatmap mensual y detalle de día reales`

---

### Fase 6 — Módulo Reportes + desglose real (semana/mes)
**Objetivo:** `Reports.tsx` (barras semanales, tarta por proyecto, ranking, actividad diaria).

- **Backend**
  - `GET /api/reports/summary?from&to` → totales, media diaria, focus, etc.
  - `GET /api/reports/by-project?from&to` → horas y % por proyecto (tarta).
  - `GET /api/reports/weekly?month` → horas vs objetivo por semana.
  - `GET /api/reports/team?month` → ranking por usuario (si hay permisos/datos).
  - Cablear pestañas **"week"/"month"** de `break-down-dialog`.
- **Frontend**
  - `reports.*`: replicar las 4 stats, gráfico semanal (barras div), tarta SVG, ranking y
    grid de actividad diaria — idéntico a `Reports.tsx`. Botón *Exportar* → UI (o CSV simple).
- **Aceptación:** paridad visual + agregaciones reales; desglose semana/mes real.
- **Commit sugerido:** `feat: módulo de reportes con agregaciones reales (resumen, por proyecto, semanal, equipo)`

---

### Fase 7 — Módulo Ajustes
**Objetivo:** `Settings.tsx` con pestañas Perfil / Notificaciones / Apariencia / Seguridad / Facturación.

- **Backend**
  - `PUT /api/users/me` (nombre, email, zona horaria, idioma).
  - `POST /api/auth/change-password`.
  - (Notificaciones/apariencia → `localStorage`; facturación → solo UI.)
- **Frontend**
  - `settings.*`: barra lateral de pestañas + contenido de cada pestaña idéntico a `Settings.tsx`.
    Perfil cableado a usuario real; Apariencia (tema claro/oscuro/sistema) aplica clase `.dark`.
- **Aceptación:** paridad visual; perfil y tema persisten; contraseña se cambia.
- **Commit sugerido:** `feat: módulo de ajustes (perfil real, tema persistente, seguridad)`

---

### Fase 8 — Limpieza, seguridad y QA
**Objetivo:** cerrar deuda y dejar el proyecto pulido.

- Unificar `EntryType` (quitar `Lunch`).
- (Opcional) refactor a kit `shared/ui/` si se ve repetición excesiva.
- Endurecer registro de roles; sacar secretos de `appsettings.json` (user-secrets/env).
- Revisar responsive (mobile menu, grids) contra la plantilla.
- Pasar `prettier`/lint; specs mínimas de los componentes nuevos.
- **Commit sugerido:** `chore: limpieza, seguridad y QA post-migración`

---

## 5. Riesgos / notas

- **Migraciones EF** en Fases 3 y 4: requieren la BBDD arriba (contenedor `animenel-db`, ya activo)
  y aplicar `dotnet ef database update`.
- **Paridad exacta**: el mayor riesgo son clases Tailwind dinámicas de React tipo
  `bg-${color}/10` o `from-${color}`. En Angular hay que **listar esas clases de forma estática**
  (o usar `[ngClass]` con nombres completos) para que Tailwind no las purgue.
- **Racha/nivel/badges/facturación**: son cosméticos; no inventar lógica de negocio salvo que se pida.
- Cada fase se entrega para que **tú** hagas el commit con el mensaje propuesto.

---

## 6. Checklist rápido

- [ ] Fase 0 — Landing (stats, testimonials, cta)
- [ ] Fase 1 — Layout con usuario real + limpieza
- [ ] Fase 2 — Timer completo
- [ ] Fase 3 — Proyectos (entidad + CRUD + página)
- [ ] Fase 4 — Actividades + AssignActivityDialog
- [ ] Fase 5 — Calendario + desglose día
- [ ] Fase 6 — Reportes + desglose semana/mes
- [ ] Fase 7 — Ajustes
- [ ] Fase 8 — Limpieza / seguridad / QA
