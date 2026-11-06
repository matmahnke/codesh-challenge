# ---------- build ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# A URL da API é embutida no bundle em tempo de build.
# Ela é acessada pelo navegador, então deve ser uma URL alcançável a partir do host.
ARG VITE_API_URL=http://localhost:5164
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# ---------- runtime ----------
FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
