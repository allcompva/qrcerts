# QRCerts — Mapeo de Variables para Generación de Certificados

## Resumen del flujo

```
Plantilla (.docx) con variables: {{NOMBRE}}, {{RUT}}, {{QR}}, {{INSTRUCTOR}}, {{NOMBRE_CURSO}}
                |
                v
        Se clasifican en:
        ┌─────────────────────────────────────────────────────────────────┐
        │  contenido_alumnos → REGISTRO_ALUMNOS.observaciones (x alumno) │
        │  contenido_cursos  → Cursos.LayoutJson (1 vez por curso)       │
        │  automaticas       → QR (imagen) + NOMBRE_CURSO (SQL)          │
        └─────────────────────────────────────────────────────────────────┘
                |
                v
        GetReplacementJson() fusiona todo en un solo JSON
                |
                v
        Se reemplaza en el .docx → se convierte a PDF → se sube a Drive
```

---

## 1. La consulta SQL que arma el JSON de reemplazo

**Archivo:** `DAL/Plantilla_certificados.cs` — método `GetReplacementJson()` (línea 108)

```sql
SELECT
  CASE LayoutJson
    WHEN '' THEN
      LEFT(Observaciones, LEN(Observaciones) - 1) + ', ' +
      '"NOMBRE_CURSO": "' + nombre_visualizar_certificado + '"}'
    ELSE
      LEFT(Observaciones, LEN(Observaciones) - 1) + ',' +
      SUBSTRING(LayoutJson, 2, LEN(LayoutJson) - 2) + ',' +
      '"NOMBRE_CURSO": "' + nombre_visualizar_certificado + '"}'
  END
FROM REGISTRO_ALUMNOS A
  INNER JOIN Cursos B ON A.id_curso = B.Id
WHERE id_alumno = @alumnoGuid
  AND id_curso = @courseGuid
```

### Qué hace

Concatena 3 fuentes de datos en un solo JSON string:

| Fuente | Tabla.Campo | Tipo | Ejemplo |
|--------|-------------|------|---------|
| Variables de alumno | `REGISTRO_ALUMNOS.observaciones` | Dinámicas por alumno | `{"NOMBRE": "Juan", "RUT": "12345"}` |
| Variables de curso | `Cursos.LayoutJson` | Comunes a todos los alumnos | `{"INSTRUCTOR": "Prof. Garcia", "HORAS": "40"}` |
| Nombre del curso | `Cursos.nombre_visualizar_certificado` | Hardcodeada como NOMBRE_CURSO | `"Seguridad Eléctrica"` |

### Resultado final (ejemplo)

```json
{
  "NOMBRE": "Juan Perez",
  "RUT": "12345678-k",
  "Calificacion": "85",
  "INSTRUCTOR": "Prof. Garcia",
  "HORAS": "40",
  "NOMBRE_CURSO": "Seguridad Eléctrica"
}
```

Cada clave se usa para reemplazar `{{CLAVE}}` en la plantilla .docx.
El `{{QR}}` se reemplaza aparte con una imagen PNG (no viene de esta consulta).

---

## 2. Modelo que mapea la consulta

No hay modelo dedicado. La consulta devuelve un **string** (JSON) que se deserializa a `Dictionary<string, string>` en:

- `UploadController.cs` línea 240
- `PdfGenerationService.cs` línea 71
- `CertificadosService.cs` (nuevo)

---

## 3. Métodos que populan las tablas

### 3A. REGISTRO_ALUMNOS.observaciones — Variables de alumno

Contiene un JSON con todas las variables específicas de cada alumno.

#### Backend — Métodos que escriben en la tabla

| Método | Archivo | Línea | Endpoint | Cuándo se invoca |
|--------|---------|-------|----------|------------------|
| `BulkImport` | `AlumnosController.cs` | 416 | `POST /api/app/alumnos/bulk-import/{cursoId}` | Import masivo desde Excel o Moodle |
| `CreateWithRegistro` | `AlumnosController.cs` | 273 | `POST /api/app/alumnos/create-with-registro` | Crear alumno individual |
| `UpdateRegistro` | `AlumnosController.cs` | 317 | `PUT /api/app/alumnos/{alumnoId}/registro/{cursoId}` | Editar alumno existente |
| `CommitImportAsync` | `AlumnosService.cs` | 233 | `POST /api/app/alumnos/import/{cursoId}/commit` | Import Excel via CursoWizard |

#### Frontend — Quién construye el JSON

| Componente | Archivo | Línea | Contexto |
|------------|---------|-------|----------|
| CursoWizard | `CursoWizard.tsx` | 437-451 | Crear curso con Excel: todas las columnas del Excel → `JSON.stringify(allFields)` |
| AlumnosList | `AlumnosList.tsx` | 230-250 | Import Excel desde lista de alumnos: todas las columnas → `JSON.stringify(allFields)` |
| AlumnosList | `AlumnosList.tsx` | 370-380 | Agregar/editar alumno manual: campos dinámicos del formulario → `JSON.stringify(sanitizedFields)` |
| MoodleImportWizard | `MoodleImportWizard.tsx` | 440-451 | Import Moodle: campos Moodle + field mappings → `JSON.stringify({...allFields, ...mappedFields})` |

#### Estructura del JSON (ejemplo)

```json
{
  "Nombre": "Camila Rojas",
  "RUT": "17.468.198-8",
  "Calificacion": "6.5",
  "CustomField1": "valor1"
}
```

Las claves dependen de las columnas del Excel o los campos del formulario dinámico.

---

### 3B. Cursos.LayoutJson — Variables de curso

Contiene un JSON con las variables comunes a todos los alumnos del curso.

#### Backend — Métodos que escriben en la tabla

| Método | Archivo | Línea | Endpoint | Cuándo se invoca |
|--------|---------|-------|----------|------------------|
| `CreateAsync` | `CursosService.cs` | 61 | `POST /api/app/cursos` | Al crear el curso |
| `UpdateAsync` | `CursosService.cs` | 126 | `PUT /api/app/cursos/{id}` | Al editar el curso |
| `UpdateLayoutAsync` | `LayoutService.cs` | 49 | `PUT /api/app/cursos/{cursoId}/layout` | Al guardar layout |

#### Frontend — Quién construye el JSON

| Componente | Archivo | Línea | Contexto |
|------------|---------|-------|----------|
| CursoWizard | `CursoWizard.tsx` | 415-417 | Al crear/editar curso: `JSON.stringify(cursoVariablesValues)` |
| MoodleImportWizard | `MoodleImportWizard.tsx` | 375-377 | Al importar desde Moodle |

#### Estructura del JSON (ejemplo)

```json
{
  "INSTRUCTOR": "Prof. Garcia",
  "HORAS": "40",
  "FECHA_CURSO": "2026-03-15"
}
```

Las claves provienen de `Plantilla_certificados.contenido_cursos` — las variables que la plantilla clasifica como "de curso".

---

### 3C. Cursos.nombre_visualizar_certificado — Nombre del curso (hardcodeada)

Se agrega siempre como `NOMBRE_CURSO` en el JSON final por la consulta SQL.

| Método | Archivo | Línea | Endpoint | Cuándo se invoca |
|--------|---------|-------|----------|------------------|
| `CreateAsync` | `CursosService.cs` | 56-57 | `POST /api/app/cursos` | Al crear el curso |
| `UpdateAsync` | `CursosService.cs` | 83-84 | `PUT /api/app/cursos/{id}` | Al editar el curso |

Se establece desde el campo "Nombre a visualizar en certificado" del formulario del curso.

---

## 4. Clasificación de variables en la plantilla

Cuando se sube un .docx como plantilla, el sistema extrae las variables `{{KEY}}`.
Después el usuario las clasifica en dos grupos:

| Clasificación | Campo en Plantilla_certificados | Destino | Cuándo se llenan |
|--------------|-------------------------------|---------|------------------|
| Variables de alumno | `contenido_alumnos` | `REGISTRO_ALUMNOS.observaciones` | Al importar/crear cada alumno |
| Variables de curso | `contenido_cursos` | `Cursos.LayoutJson` | Al crear/editar el curso |

Variables automáticas (no requieren clasificación):
- `{{QR}}` → Se reemplaza con imagen PNG del código QR
- `{{NOMBRE_CURSO}}` → Se agrega automáticamente desde `Cursos.nombre_visualizar_certificado`

---

## 5. Tablas involucradas

| Tabla | Campos relevantes | Rol |
|-------|-------------------|-----|
| `REGISTRO_ALUMNOS` | `observaciones` (JSON string) | Variables por alumno |
| `Cursos` | `LayoutJson` (JSON string), `nombre_visualizar_certificado` (string) | Variables por curso |
| `Plantilla_certificados` | `contenido_alumnos`, `contenido_cursos`, `path_docx` | Definición de variables y archivo template |
| `Alumnos` | `NombreApellido`, `RUT` | Datos base del alumno (usados para el registro) |
