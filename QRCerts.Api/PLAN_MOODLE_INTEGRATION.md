# Plan de Integración Moodle - QRCerts

## Resumen Ejecutivo

Implementar integración con Moodle para importar cursos y alumnos automáticamente,
permitiendo generar certificados sin carga manual de datos.

---

## 1. Nuevas Entidades (Backend)

### 1.1 MoodleConfig
Almacena la configuración de conexión Moodle por OTEC.

```csharp
public class MoodleConfig
{
    public Guid Id { get; set; }
    public Guid OtecId { get; set; }           // FK a Otec
    public string MoodleUrl { get; set; }       // https://moodle.ejemplo.cl
    public string Token { get; set; }           // Token de Web Service
    public bool Activo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 1.2 MoodleCursoImportado
Cache de cursos importados desde Moodle.

```csharp
public class MoodleCursoImportado
{
    public Guid Id { get; set; }
    public Guid OtecId { get; set; }
    public int MoodleCourseId { get; set; }     // ID en Moodle
    public string NombreCurso { get; set; }
    public string ShortName { get; set; }
    public Guid? CursoLocalId { get; set; }     // FK opcional a Curso local
    public DateTime UltimaSync { get; set; }
}
```

### 1.3 MoodleFieldMapping
Mapeo de campos Moodle → variables del certificado.

```csharp
public class MoodleFieldMapping
{
    public Guid Id { get; set; }
    public Guid CursoId { get; set; }           // FK a Curso local
    public string CampoMoodle { get; set; }     // ej: "firstname", "profile_field_rut"
    public string VariableCertificado { get; set; } // ej: "NombreAlumno", "RUT"
    public int Orden { get; set; }
}
```

---

## 2. Endpoints API (Backend)

### 2.1 Configuración Moodle
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/moodle/config` | Obtener config actual de la OTEC |
| POST | `/api/moodle/config` | Guardar/actualizar config |
| POST | `/api/moodle/test-connection` | Probar conexión con Moodle |

### 2.2 Cursos Moodle
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/moodle/courses` | Listar cursos disponibles en Moodle |
| GET | `/api/moodle/courses/{moodleId}/students` | Listar alumnos de un curso |
| GET | `/api/moodle/courses/{moodleId}/fields` | Obtener campos disponibles |

### 2.3 Importación
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/moodle/import` | Importar alumnos a curso local |
| GET | `/api/moodle/import/{id}/status` | Estado de importación |

---

## 3. Servicios (Backend)

### 3.1 MoodleApiService
Comunicación con la API REST de Moodle.

```csharp
public interface IMoodleApiService
{
    Task<bool> TestConnection(string url, string token);
    Task<List<MoodleCourse>> GetCourses(string url, string token);
    Task<List<MoodleUser>> GetCourseStudents(string url, string token, int courseId);
    Task<List<MoodleGrade>> GetCourseGrades(string url, string token, int courseId);
    Task<List<MoodleProfileField>> GetUserProfileFields(string url, string token);
}
```

**Funciones Moodle a usar:**
- `core_course_get_courses` - Listar cursos
- `core_enrol_get_enrolled_users` - Alumnos inscriptos
- `gradereport_user_get_grade_items` - Notas/calificaciones
- `core_user_get_users` - Datos de usuario con campos custom

### 3.2 MoodleImportService
Lógica de importación.

```csharp
public interface IMoodleImportService
{
    Task<ImportResult> ImportStudents(Guid cursoLocalId, int moodleCourseId,
                                       List<MoodleFieldMapping> mappings,
                                       bool soloAprobados = true);
}
```

---

## 4. Componentes Frontend

### 4.1 MoodleConfigPage
Página de configuración de conexión Moodle.

- Input: URL de Moodle
- Input: Token de Web Service
- Botón: Probar conexión
- Estado: Conectado/Desconectado

### 4.2 MoodleImportWizard
Wizard de importación en pasos:

**Paso 1: Seleccionar Curso Moodle**
- Lista de cursos disponibles en Moodle
- Búsqueda/filtro

**Paso 2: Configurar Curso Local**
- Crear nuevo curso o vincular existente
- Seleccionar plantilla de certificado

**Paso 3: Mapear Campos**
- Columna izquierda: campos disponibles en Moodle
- Columna derecha: variables del certificado
- Drag & drop o selects

**Paso 4: Previsualizar e Importar**
- Vista previa de alumnos a importar
- Filtro: solo aprobados
- Botón: Importar

### 4.3 MoodleSyncStatus
Componente para mostrar estado de sincronización en la lista de cursos.

---

## 5. Flujo de Usuario

```
┌─────────────────────────────────────────────────────────────┐
│  1. OTEC configura conexión Moodle (URL + Token)            │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  2. Sistema lista cursos disponibles en Moodle              │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Operador selecciona curso a importar                    │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Sistema muestra campos disponibles (estándar + custom)  │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Operador mapea campos Moodle → variables certificado    │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  6. Sistema importa alumnos APROBADOS al curso local        │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  7. Generar certificados (flujo existente)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. Campos Estándar vs Personalizados

### Campos Estándar Moodle (siempre disponibles)
| Campo Moodle | Variable Certificado | Descripción |
|--------------|---------------------|-------------|
| `firstname` | NombreAlumno | Nombre |
| `lastname` | ApellidoAlumno | Apellido |
| `email` | Email | Correo electrónico |
| `username` | Usuario | Nombre de usuario |
| `idnumber` | RUT | Número de identificación |
| `finalgrade` | NotaFinal | Calificación final |
| `status` | Estado | Aprobado/No aprobado |

### Campos Personalizados (profile fields)
Se obtienen dinámicamente de Moodle:
- `profile_field_rut` → RUT
- `profile_field_empresa` → Empresa
- (cualquier campo custom definido en Moodle)

---

## 7. Reglas de Negocio

1. **Solo alumnos aprobados**: Por defecto, solo se importan alumnos con estado aprobado
2. **Sin duplicados**: Si un alumno ya existe (por RUT), se actualiza en lugar de duplicar
3. **Mapeo obligatorio**: NombreApellido y RUT son obligatorios para el certificado
4. **Token seguro**: El token de Moodle se almacena encriptado
5. **Sin SCORM**: No se soportan cursos SCORM (según requerimientos)

---

## 8. Plan de Implementación

### Fase 1: Backend Base (8-10 horas)
- [ ] Crear modelos: MoodleConfig, MoodleCursoImportado, MoodleFieldMapping
- [ ] Crear MoodleApiService (conexión con Moodle REST API)
- [ ] Crear MoodleController con endpoints básicos
- [ ] Migración de base de datos

### Fase 2: Backend Importación (6-8 horas)
- [ ] Implementar MoodleImportService
- [ ] Lógica de mapeo de campos
- [ ] Lógica de detección de aprobados
- [ ] Endpoint de importación

### Fase 3: Frontend Configuración (4-6 horas)
- [ ] Página MoodleConfigPage
- [ ] Componente de test de conexión
- [ ] Integración con menú OTEC

### Fase 4: Frontend Wizard (8-10 horas)
- [ ] MoodleImportWizard completo
- [ ] Selector de cursos
- [ ] Mapeador de campos
- [ ] Vista previa de importación

### Fase 5: Testing e Integración (4-6 horas)
- [ ] Tests con Moodle real/sandbox
- [ ] Manejo de errores
- [ ] Documentación

**Total estimado: 30-40 horas**

---

## 9. Dependencias Técnicas

### Backend
- HttpClient para llamadas REST a Moodle
- Encriptación para token (AES o similar)

### Frontend
- Ninguna dependencia nueva significativa

### Moodle (requisitos para la OTEC)
- Moodle 3.x o superior
- Web Services habilitados
- Token con permisos:
  - `moodle/course:view`
  - `moodle/user:viewdetails`
  - `moodle/grades:view`

---

## 10. Riesgos y Mitigaciones

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Moodle sin Web Services | Alto | Documentar requisitos, verificar en config |
| Campos custom no estándar | Medio | UI flexible de mapeo |
| Volumen alto de alumnos | Bajo | Paginación, procesamiento async |
| Token expirado | Medio | Verificar conexión, alertar usuario |

---

## Aprobación

- [ ] Revisado por: ________________
- [ ] Fecha: ________________
- [ ] Comentarios: ________________
