# Build stage
FROM node:18-alpine AS build
WORKDIR /app
ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}
COPY frontend/package.json frontend/vite.config.js frontend/index.html ./
COPY frontend/src ./src
RUN npm ci || npm install
RUN npm run build

# Serve with nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
# Copy nginx configuration for HTTPS
COPY nginx/client.conf /etc/nginx/conf.d/default.conf
# Copy SSL certificates
COPY ssl /etc/nginx/ssl
EXPOSE 443 80
CMD ["nginx", "-g", "daemon off;"]


