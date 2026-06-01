# ═════════════════════════ EduTrack.WEB ═════════════════════════
# Multi-stage: Node build Angular prod → nginx serve static
# Dùng cho tự host. Cho Vercel KHÔNG cần file này (auto-detect Angular).
# ════════════════════════════════════════════════════════════════

FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install --legacy-peer-deps

COPY . .
RUN npm run build:prod

# ─── Serve stage ───
FROM nginx:alpine AS serve
COPY --from=build /app/dist/edutrack.web-prod/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
