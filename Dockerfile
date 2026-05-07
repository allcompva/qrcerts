# ============================================
# STAGE 1: Build Frontend React
# ============================================
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Copiar package.json primero para cache de dependencias
COPY qrcerts-frontend/package*.json ./

# Instalar dependencias
RUN npm ci

# Copiar el resto del código frontend
COPY qrcerts-frontend/ ./

# Configurar la URL de la API para producción (mismo origen)
ENV VITE_API_BASE_URL=

# Build de producción
RUN npm run build

# ============================================
# STAGE 2: Build Backend .NET 8.0
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build

WORKDIR /app

# Copiar csproj y restaurar dependencias
COPY QRCerts.Api/*.csproj ./QRCerts.Api/
RUN dotnet restore ./QRCerts.Api/QRCerts.Api.csproj

# Copiar el resto del código backend
COPY QRCerts.Api/ ./QRCerts.Api/

# Garantizar que appsettings.json exista (el .gitignore lo excluye del repo).
# Los valores reales se inyectan via env vars desde docker-compose en runtime.
RUN if [ ! -f ./QRCerts.Api/appsettings.json ]; then \
      cp ./QRCerts.Api/appsettings.example.json ./QRCerts.Api/appsettings.json; \
    fi

# Publicar la aplicación
RUN dotnet publish ./QRCerts.Api/QRCerts.Api.csproj -c Release -o /app/publish --no-restore

# ============================================
# STAGE 3: Runtime con LibreOffice 25.x (Trixie)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Instalar LibreOffice 25.x desde Debian Trixie para mejor renderizado PDF
RUN apt-get update \
    && echo "deb http://deb.debian.org/debian trixie main" > /etc/apt/sources.list.d/trixie.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends -t trixie \
        libreoffice-writer \
        libreoffice-calc \
    && apt-get install -y --no-install-recommends \
        fonts-liberation \
        fonts-dejavu \
        fontconfig \
        curl \
    && rm -rf /var/lib/apt/lists/* \
    && fc-cache -f -v

# Crear directorio para la app
WORKDIR /app

# Copiar la aplicación .NET publicada
COPY --from=backend-build /app/publish .

# Copiar el frontend compilado a wwwroot/app
# (Vite buildea con base: '/app/' y Program.cs sirve la SPA desde /app/)
COPY --from=frontend-build /app/frontend/dist ./wwwroot/app

# Copiar landing.html y otras paginas estaticas a wwwroot/
# (Program.cs mapea GET / -> wwwroot/landing.html)
COPY landing.html ./wwwroot/landing.html
COPY presentacion.html ./wwwroot/presentacion.html
COPY mockup-ui.html ./wwwroot/mockup-ui.html

# Crear directorios necesarios
RUN mkdir -p /app/wwwroot/uploads/images \
    && mkdir -p /app/wwwroot/uploads/docx \
    && mkdir -p /app/logs

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV SOFFICE_PATH=/usr/bin/soffice

# Exponer puerto
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Comando de inicio
ENTRYPOINT ["dotnet", "QRCerts.Api.dll"]
