# Comandos de Docker — Zezame

Guía rápida de los comandos de Docker y Docker Compose usados en este proyecto.
Stack: **Angular 20 (SSR)** + **.NET 8 API** + **PostgreSQL 18** + **Migrator**.

> Todos los comandos se ejecutan desde la **raíz del proyecto**, donde está el `docker-compose.yml`.

---

## 1. Levantar el stack

### `docker compose up -d`
Levanta todos los servicios en segundo plano (`-d` = detached).
- Construye las imágenes **solo si no existen**.
- Si ya existen, las reutiliza.
- La primera vez tarda más porque descarga las imágenes base y compila.

```bash
docker compose up -d
```

### `docker compose up -d --build`
Igual que el anterior, pero **fuerza la reconstrucción** de las imágenes.
Úsalo cuando hayas cambiado código, `Dockerfile`, `package.json`, `.csproj`, etc.

```bash
docker compose up -d --build
```

### `docker compose up -d --build backend`
Reconstruye y levanta **solo un servicio** (útil para iterar rápido).
Puedes sustituir `backend` por `frontend`, `migrator` o `db`.

```bash
docker compose up -d --build backend
```

---

## 2. Ver el estado y los logs

### `docker compose ps`
Lista los contenedores del stack con su estado, puertos y nombre.
Con `-a` incluye también los que han terminado (útil para ver el Migrator).

```bash
docker compose ps
docker compose ps -a
```

### `docker compose logs -f <servicio>`
Muestra los logs en tiempo real (`-f` = follow).
Sin `-f` muestra los logs y termina.

```bash
docker compose logs -f backend
docker compose logs -f frontend
docker compose logs -f db
docker compose logs -f migrator
```

### `docker compose logs -f --tail=100`
Muestra solo las últimas 100 líneas y sigue en tiempo real.

```bash
docker compose logs -f --tail=100 backend
```

---

## 3. Detener el stack

### `docker compose stop`
Detiene los contenedores **sin eliminarlos**. Los datos y el estado se conservan.
El próximo `up -d` los reutiliza.

```bash
docker compose stop
```

### `docker compose down`
Detiene y **elimina los contenedores y la red**, pero **conserva los volúmenes**.
La base de datos sigue intacta.

```bash
docker compose down
```

### ⚠️ `docker compose down -v`
Como `down`, pero **elimina también los volúmenes** (¡incluida la base de datos!).
Úsalo solo cuando quieras empezar de cero.

```bash
docker compose down -v
```

---

## 4. Reconstruir imágenes

### `docker compose build`
Construye todas las imágenes **sin levantar** los contenedores.

```bash
docker compose build
```

### `docker compose build --no-cache`
Construye **ignorando la caché** de capas. Útil cuando algo falla y sospechas
que la caché está dando problemas.

```bash
docker compose build --no-cache
```

### `docker compose build --no-cache backend`
Solo un servicio, sin caché.

```bash
docker compose build --no-cache backend
```

---

## 5. Inspeccionar contenedores

### `docker compose exec <servicio> <comando>`
Ejecuta un comando **dentro de un contenedor en ejecución**.
Útil para revisar variables de entorno, ficheros, conexiones, etc.

```bash
# Ver variables de entorno del backend
docker compose exec backend env

# Ver variables que contengan "Connection" o "Jwt"
docker compose exec backend env | grep -i connection
docker compose exec backend env | grep -i jwt

# Abrir una shell dentro del contenedor
docker compose exec backend sh
docker compose exec db sh

# Conectarse a PostgreSQL desde dentro del contenedor
docker compose exec db psql -U zezame_user -d zezame_db
```

### `docker compose run --rm <servicio>`
Ejecuta un contenedor **nuevo y temporal** (no el que ya está corriendo).
Con `--rm` se elimina al terminar. Muy útil para lanzar el Migrator
manualmente.

```bash
docker compose run --rm migrator
```

---

## 6. Gestión de volúmenes

### `docker volume ls`
Lista todos los volúmenes del sistema.

```bash
docker volume ls
```

### `docker volume inspect zezame_postgres_data`
Muestra detalles del volumen (ruta física en el host, etc.).

```bash
docker volume inspect zezame_postgres_data
```

### ⚠️ `docker volume rm zezame_postgres_data`
Elimina el volumen de la base de datos. **Destruye los datos**.
Solo funciona si ningún contenedor lo está usando (haz `docker compose down` antes).

```bash
docker compose down
docker volume rm zezame_postgres_data
```

### Limpiar volúmenes huérfanos
```bash
docker volume prune
```

---

## 7. Limpieza general

### `docker system df`
Muestra cuánto espacio ocupan imágenes, contenedores y volúmenes.

```bash
docker system df
```

### `docker system prune`
Elimina contenedores parados, redes no usadas e imágenes colgantes.
**No elimina volúmenes** salvo que añadas `--volumes`.

```bash
docker system prune
```

### `docker builder prune`
Limpia la caché de build (útil cuando el disco se llena).

```bash
docker builder prune -f
```

### `docker compose config`
Muestra el `docker-compose.yml` **resuelto** (con las variables de entorno
sustituidas). Muy útil para verificar que `${POSTGRES_PASSWORD}` y demás
se están interpolando correctamente.

```bash
docker compose config
```

---

## 8. Flujo típico de trabajo

### Cambio en el código del backend
```bash
docker compose up -d --build backend
docker compose logs -f backend
```

### Cambio en el código del frontend
```bash
docker compose up -d --build frontend
docker compose logs -f frontend
```

### Cambio en el `docker-compose.yml`
```bash
docker compose down
docker compose up -d
```

### Empezar de cero (borrar BD incluida)
```bash
docker compose down -v
docker compose up -d --build
```

### Aplicar migraciones manualmente
```bash
docker compose run --rm migrator
```

---

## 9. Notas específicas de este proyecto

- **PostgreSQL 18**: el volumen se monta en `/var/lib/postgresql` (sin `/data`).
  No cambies esa ruta o el contenedor no arrancará.
- **Migrator**: es un servicio de corta duración. Debe terminar con código `0`
  para que el `backend` arranque (`condition: service_completed_successfully`).
  Comprueba con `docker compose ps -a` que aparece como `Exited (0)`.
- **Frontend SSR**: no usa Nginx. Sirve con Node en el puerto interno `4000`,
  mapeado al `4200` del host.
- **Backend**: escucha en HTTP (`http://+:8080`) dentro del contenedor, mapeado
  al `7107` del host. No uses `https://localhost:7107` desde el frontend, usa
  `http://localhost:7107`.
- **Secretos**: las variables `POSTGRES_PASSWORD`, `JWT_KEY` y `RESEND_APITOKEN`
  se leen del archivo `.env` en la raíz. **No subas `.env` al repositorio.**

---

## 10. Atajos rápidos (chuleta)

| Objetivo | Comando |
|---|---|
| Levantar todo | `docker compose up -d` |
| Levantar reconstruyendo | `docker compose up -d --build` |
| Parar sin borrar datos | `docker compose stop` |
| Parar y borrar contenedores | `docker compose down` |
| Parar y borrar TODO (incl. BD) | `docker compose down -v` |
| Ver logs en vivo | `docker compose logs -f backend` |
| Ver estado (incl. terminados) | `docker compose ps -a` |
| Ejecutar migrator | `docker compose run --rm migrator` |
| Ver env de un contenedor | `docker compose exec backend env` |
| Entrar a PostgreSQL | `docker compose exec db psql -U zezame_user -d zezame_db` |
| Ver compose resuelto | `docker compose config` |