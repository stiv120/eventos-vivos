FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./

ARG ADMIN_API_KEY=dev-admin-key
RUN sed -i "s/adminApiKey: '.*'/adminApiKey: '${ADMIN_API_KEY}'/" src/environments/environment.docker.ts \
    && npm run build -- --configuration docker

FROM nginx:1.27-alpine AS final
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/eventos-vivos-ui/browser /usr/share/nginx/html
EXPOSE 80
