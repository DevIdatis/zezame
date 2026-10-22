# ---- Etapa 1: build ----
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --include=dev

COPY . .
RUN npm run build:prod

# ---- Etapa 2: runtime ----
FROM node:20-alpine AS final
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=4000

COPY --from=build /app/package.json ./package.json
COPY --from=build /app/package-lock.json ./package-lock.json
COPY --from=build /app/dist ./dist

RUN npm ci --omit=dev

EXPOSE 4000

CMD ["node", "dist/zezame-frontend/server/server.mjs"]