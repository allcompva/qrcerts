# QRCerts Frontend

Frontend React para la plataforma de certificados digitales QRCerts.

## Características

### 🔐 Autenticación
- Login con JWT
- Protección de rutas por roles
- Gestión de sesiones

### 👨‍💼 Módulo Administrador
- **Gestión de OTECs**: CRUD completo de organizaciones de capacitación
- **Usuarios OTEC**: Creación y administración de usuarios para cada OTEC
- Dashboard con estadísticas

### 🏢 Módulo OTEC
- **Gestión de Cursos**: CRUD de cursos por OTEC
- **Importación de Alumnos**: Carga masiva desde Excel
- **Emisión de Certificados**: Generación masiva con seguimiento en tiempo real
- Dashboard con métricas de curso

## Stack Tecnológico

- **React 19** con TypeScript
- **Material-UI** para componentes
- **React Router** para navegación
- **Axios** para llamadas API
- **XLSX** para manejo de archivos Excel
- **Context API** para gestión de estado

## Estructura del Proyecto

```
src/
├── components/
│   ├── Admin/           # Módulo administrador
│   ├── Auth/            # Autenticación
│   ├── Dashboard/       # Panel principal
│   ├── Layout/          # Componentes de layout
│   └── OTEC/           # Módulo OTEC
├── contexts/           # Context providers
├── services/          # Servicios API
├── types/            # Tipos TypeScript
└── config/          # Configuraciones
```

## Instalación y Desarrollo

```bash
# Instalar dependencias
npm install

# Iniciar en desarrollo
npm start

# Construir para producción
npm run build
```

## Configuración

### Variables de Entorno
El frontend está configurado para usar proxy hacia `http://localhost:5000` donde debe estar ejecutándose la API.

### Autenticación
El sistema maneja dos tipos de usuarios:
- **admin**: Acceso completo al sistema
- **otec**: Acceso limitado a sus propios cursos

## Funcionalidades Principales

### Importación de Excel
El sistema permite importar alumnos desde archivos Excel con la siguiente estructura:
```
Nombre | RUT | Curso | Calificacion | Observaciones
```

### Emisión Masiva de Certificados
- Selección de alumnos para certificar
- Generación en lotes con progreso en tiempo real
- Seguimiento de estado de emisiones
- Manejo de errores y reintentos

### Gestión de Rutas
- `/admin/*` - Rutas del administrador
- `/otec/*` - Rutas de OTEC
- `/dashboard` - Panel principal
- `/login` - Inicio de sesión

## Componentes Principales

### Admin
- `OtecList`: Lista y CRUD de OTECs
- `OtecUserList`: Gestión de usuarios OTEC

### OTEC
- `CursoList`: Gestión de cursos
- `AlumnosList`: Importación y gestión de alumnos
- `CertificadosList`: Emisión masiva de certificados

### Shared
- `Layout`: Layout principal con navegación
- `ProtectedRoute`: Protección de rutas por rol
- `AuthContext`: Gestión de autenticación

## API Integration

El frontend consume la API REST del backend ASP.NET Core, implementando:
- Interceptors para autenticación automática
- Manejo de errores centralizados
- Renovación automática de tokens
- Tipos TypeScript para todas las entidades