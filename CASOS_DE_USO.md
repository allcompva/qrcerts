# QRCerts — Casos de Uso

## Actores

| Actor | Descripción |
|-------|-------------|
| **Admin Sitio** | Administrador de la plataforma QRCerts. Gestiona empresas (OTECs), usuarios y cuotas comerciales |
| **Usuario OTEC** | Usuario de una empresa cliente. Gestiona cursos, alumnos y certificados de su organización |
| **Visitante** | Persona que escanea un QR para verificar la validez de un certificado |

---

## 1. Gestión de Empresas (Admin Sitio)

### CU-01: Crear OTEC
- **Actor:** Admin Sitio
- **Descripción:** El admin crea una nueva empresa cliente (OTEC) con nombre, slug único y estado activo/inactivo
- **Precondición:** Admin autenticado
- **Flujo:** Admin → Administración → Nueva OTEC → Completa nombre y slug → Guardar
- **Resultado:** OTEC creado en el sistema, disponible para asignarle usuarios

### CU-02: Gestionar usuarios OTEC
- **Actor:** Admin Sitio
- **Descripción:** El admin crea, modifica o desactiva usuarios para una empresa
- **Precondición:** OTEC existente
- **Flujo:** Admin → Selecciona OTEC → Usuarios → Crear/Editar usuario con username y contraseña
- **Resultado:** Usuario OTEC puede iniciar sesión y operar dentro de su empresa

### CU-03: Configurar cuota de certificados
- **Actor:** Admin Sitio
- **Descripción:** El admin activa/desactiva el modelo de cuota para una empresa y crea órdenes de compra
- **Precondición:** OTEC existente
- **Flujo:**
  1. Admin → OTEC → Cuota → Activar switch de cuota
  2. Crear orden: ingresa cantidad comprada + fecha de caducidad + notas opcionales
  3. La nueva orden reemplaza automáticamente a la anterior (la anterior pasa al historial)
- **Resultado:** La empresa solo puede emitir certificados mientras tenga saldo disponible y la orden no esté vencida. Si la cuota está desactivada, la emisión es libre
- **Reglas:**
  - Órdenes no se modifican ni cancelan, solo se crean nuevas
  - Tanto emisiones nuevas como re-emisiones consumen cuota
  - El historial de órdenes queda como auditoría

### CU-04: Consultar historial de órdenes
- **Actor:** Admin Sitio
- **Descripción:** El admin consulta todas las órdenes emitidas para una empresa y su registro de auditoría
- **Flujo:** Admin → OTEC → Cuota → Historial
- **Resultado:** Lista de órdenes con cantidad comprada, usada, estado y eventos

---

## 2. Gestión de Plantillas (Usuario OTEC)

### CU-05: Crear plantilla de certificado
- **Actor:** Usuario OTEC
- **Descripción:** El usuario sube un documento Word (.docx) como plantilla, define las variables del certificado
- **Flujo:**
  1. Usuario sube archivo .docx con placeholders (ej: `{{NOMBRE}}`, `{{RUT}}`, `{{QR}}`, `{{NOMBRE_CURSO}}`)
  2. El sistema detecta las variables y las clasifica en:
     - Variables de curso (se llenan una vez al crear el curso)
     - Variables de alumno (se llenan por cada alumno)
  3. Usuario confirma la clasificación
- **Resultado:** Plantilla disponible para asociar a cursos
- **Variables obligatorias:** `{{QR}}`, `{{NOMBRE_CURSO}}`, `{{NOMBRE}}`

---

## 3. Gestión de Cursos (Usuario OTEC)

### CU-06: Crear curso
- **Actor:** Usuario OTEC
- **Descripción:** El usuario crea un curso asociado a una plantilla, define los datos del curso
- **Flujo:**
  1. Nuevo curso → Nombre de referencia, nombre para certificado, tipo de certificado
  2. Selecciona plantilla → Completa variables de curso (las definidas como "contenido_cursos")
  3. Opcionalmente sube imagen de fondo, configura footers
- **Resultado:** Curso creado, listo para inscribir alumnos

### CU-07: Modificar curso
- **Actor:** Usuario OTEC
- **Descripción:** El usuario modifica cualquier dato del curso en cualquier momento
- **Precondición:** Curso existente
- **Flujo:** Usuario edita nombre, layout, plantilla, variables de curso, fondo, etc.
- **Resultado:** Cambios guardados. Los certificados ya emitidos NO se alteran. Los cambios aplican solo a nuevos certificados o certificados re-emitidos
- **Regla importante:** No hay bloqueo de edición. El curso siempre es modificable

---

## 4. Gestión de Alumnos (Usuario OTEC)

### CU-08: Importar alumnos desde Excel
- **Actor:** Usuario OTEC
- **Descripción:** El usuario sube un archivo Excel con datos de alumnos para inscribirlos en un curso
- **Flujo:**
  1. Selecciona curso → Importar Excel
  2. Sube archivo .xlsx con columnas según la plantilla del curso
  3. Sistema previsualiza los datos
  4. Confirma importación
- **Resultado:** Alumnos creados e inscritos en el curso
- **Reglas:**
  - Si el alumno ya existe (mismo RUT + misma empresa): se actualizan sus datos, no se duplica
  - Si el alumno ya está inscrito en el curso: se omite
  - Si el alumno no existe: se crea y se inscribe
  - La importación es opcional y repetible en cualquier momento

### CU-09: Agregar alumno manualmente
- **Actor:** Usuario OTEC
- **Descripción:** El usuario agrega un alumno individual completando los campos de la plantilla
- **Flujo:** Curso → Agregar alumno → Completa campos dinámicos (según plantilla) → Guardar
- **Resultado:** Alumno creado e inscrito en el curso con sus variables de certificado

### CU-10: Modificar alumno
- **Actor:** Usuario OTEC
- **Descripción:** El usuario modifica datos de un alumno inscrito en un curso
- **Flujo:** Curso → Selecciona alumno → Editar → Modifica datos → Guardar
- **Resultado:** Datos actualizados. Certificados ya emitidos no se alteran

### CU-11: Eliminar alumno de un curso
- **Actor:** Usuario OTEC
- **Descripción:** El usuario desvincula un alumno de un curso
- **Flujo:** Curso → Alumno → Eliminar inscripción
- **Resultado:** El alumno ya no aparece en el listado del curso. El registro del alumno sigue existiendo en el sistema

---

## 5. Emisión de Certificados (Usuario OTEC)

### CU-12: Emitir certificados
- **Actor:** Usuario OTEC
- **Descripción:** El usuario genera certificados para uno o varios alumnos de un curso
- **Precondición:** Curso con alumnos inscritos. Si la cuota está activa, debe tener saldo disponible
- **Flujo:**
  1. Curso → Selecciona alumnos → Emitir certificados
  2. Sistema valida cuota (si aplica)
  3. Para cada alumno: genera PDF reemplazando variables en la plantilla .docx, inserta QR, convierte a PDF
  4. Registra certificado en base de datos
- **Resultado:** Certificados generados y descargables
- **Error de cuota:** Si no hay saldo o la orden está vencida, el sistema muestra "No dispone de saldo suficiente para emitir certificados"

### CU-13: Descargar certificado individual
- **Actor:** Usuario OTEC
- **Descripción:** Descarga el PDF de un certificado específico
- **Flujo:** Curso → Alumno → Descargar certificado
- **Resultado:** Archivo PDF descargado

### CU-14: Descargar certificados en lote (ZIP)
- **Actor:** Usuario OTEC
- **Descripción:** Descarga múltiples certificados en un archivo ZIP
- **Flujo:** Curso → Selecciona alumnos → Descargar ZIP
- **Resultado:** Archivo ZIP con todos los PDFs + manifiesto JSON

### CU-15: Eliminar y re-emitir certificado
- **Actor:** Usuario OTEC
- **Descripción:** El usuario elimina un certificado emitido y lo vuelve a generar (por ejemplo, tras corregir datos)
- **Flujo:**
  1. Eliminar certificado existente
  2. Volver a emitir
- **Resultado:** Nuevo certificado generado con los datos actuales del alumno y curso
- **Reglas:**
  - El certificado anterior queda invalidado
  - La re-emisión consume cuota (si está activa)
  - Los datos usados son los vigentes al momento de la nueva emisión

---

## 6. Verificación Pública

### CU-16: Verificar certificado por QR
- **Actor:** Visitante
- **Descripción:** Una persona escanea el código QR de un certificado para verificar su autenticidad
- **Flujo:**
  1. Escanea QR → Se abre URL de verificación (https://certificadosqr.store/#/validar?data=...)
  2. El sistema decodifica los datos y busca el certificado
  3. Muestra datos del certificado si es válido o mensaje de error si no existe
- **Resultado:** Confirmación visual de la validez del certificado

---

## 7. Integración Moodle (Usuario OTEC)

### CU-17: Configurar integración Moodle
- **Actor:** Usuario OTEC
- **Descripción:** Configura la conexión a su instancia de Moodle
- **Precondición:** Moodle habilitado para la empresa por el admin
- **Flujo:** Moodle → Configuración → Ingresa URL de Moodle + Token API → Guardar
- **Resultado:** Conexión establecida, cursos de Moodle disponibles para importar

### CU-18: Importar alumnos desde Moodle
- **Actor:** Usuario OTEC
- **Descripción:** Importa alumnos y sus datos desde un curso de Moodle, mapeando campos de Moodle a variables de la plantilla
- **Flujo:**
  1. Selecciona curso Moodle
  2. Mapea campos (nombre Moodle → variable plantilla)
  3. Previsualiza alumnos
  4. Confirma importación
- **Resultado:** Alumnos importados con sus datos de Moodle mapeados a las variables del certificado

---

## 8. Indicador de Cuota (Usuario OTEC)

### CU-19: Visualizar estado de cuota
- **Actor:** Usuario OTEC
- **Descripción:** El usuario ve en la barra de navegación el estado de su cuota de certificados
- **Precondición:** Cuota activada para la empresa
- **Flujo:** Automático al iniciar sesión
- **Resultado:** Chip en la navbar mostrando certificados disponibles y días restantes
- **Indicadores de color:**
  - **Verde:** Más de 10 días para vencer Y menos del 50% utilizado
  - **Amarillo:** Menos de 7 días para vencer O más del 70% utilizado
  - **Rojo:** 3 días o menos para vencer O 90% o más utilizado
- **Si la cuota no está activa:** No se muestra ningún indicador (emisión libre)
