import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  Chip,
  Card,
  CardContent,
  LinearProgress,
} from '@mui/material';
import {
  Upload,
  Add,
  Edit,
  Delete,
  Download,
  ArrowBack
} from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { Alumno, Inscripcion, Curso, PlantillaCertificado } from '../../types';
import { apiService } from '../../services/api';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import * as XLSX from 'xlsx';
import { getBackendError } from '../../utils/errorHandler';

export function AlumnosList() {
  const { cursoId } = useParams<{ cursoId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [curso, setCurso] = useState<Curso | null>(null);
  const [inscripciones, setInscripciones] = useState<Inscripcion[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [openUploadDialog, setOpenUploadDialog] = useState(false);
  const [editingInscripcion, setEditingInscripcion] = useState<Inscripcion | null>(null);
  const [formData, setFormData] = useState({
    alumnoNombre: '',
    alumnoRut: '',
    cursoTexto: '',
    texto1: '',
    texto2: '',
    certificadoOtorgado: '',
    motivoEntrega: '',
  });
  const [uploadData, setUploadData] = useState<any[]>([]);
  const [error, setError] = useState('');
  const [dynamicFields, setDynamicFields] = useState<Record<string, string>>({});
  const [hasDynamicData, setHasDynamicData] = useState(false);
  const [plantilla, setPlantilla] = useState<PlantillaCertificado | null>(null);
  const [plantillaVariables, setPlantillaVariables] = useState<string[]>([]);

  useEffect(() => {
    if (cursoId && user) {
      loadData();
    }
  }, [cursoId, user]);

  const loadData = async () => {
    try {
      const [cursoData, alumnosData] = await Promise.all([
        apiService.get<Curso>(`/api/app/cursos/${cursoId}`),
        apiService.get<Alumno[]>(`/api/app/alumnos/GetByCursoId/${cursoId}`),
      ]);
      setCurso(cursoData);

      // Detectar plantillaId (puede venir como plantillaId o PlantillaId)
      const plantillaIdValue = cursoData.plantillaId || (cursoData as any).PlantillaId;
      console.log('📋 Curso cargado:', cursoData);
      console.log('📋 PlantillaId detectado:', plantillaIdValue);

      // Si el curso tiene plantilla, cargarla para obtener las variables
      if (plantillaIdValue) {
        try {
          const plantillaData = await apiService.get<PlantillaCertificado>(
            `/api/app/plantillas-certificados/${plantillaIdValue}`
          );
          console.log('📋 Plantilla cargada:', plantillaData);
          setPlantilla(plantillaData);

          // Usar contenido_alumnos si existe (son las variables para el Excel)
          // Si no, usar variables/contenido y filtrar
          let varsParaExcel: string[] = [];

          if (plantillaData.contenido_alumnos) {
            try {
              varsParaExcel = JSON.parse(plantillaData.contenido_alumnos);
            } catch {
              varsParaExcel = plantillaData.contenido_alumnos.split(',').map((v: string) => v.trim()).filter((v: string) => v);
            }
          } else {
            // Fallback: usar todas las variables y filtrar las automáticas
            const varsContent = plantillaData.variables || plantillaData.contenido || '[]';
            let vars: string[] = [];
            try {
              vars = JSON.parse(varsContent);
            } catch {
              vars = varsContent.split(',').map((v: string) => v.trim()).filter((v: string) => v);
            }
            // Excluir QR y NOMBRE_CURSO (son automáticos, no vienen del Excel)
            varsParaExcel = vars.filter((v: string) =>
              !['QR', 'NOMBRE_CURSO'].includes(v.toUpperCase())
            );
          }

          console.log('📋 Variables para Excel:', varsParaExcel);
          setPlantillaVariables(varsParaExcel);
        } catch (e) {
          console.error('Error al cargar plantilla:', e);
        }
      }

      // Convertir alumnos a formato de inscripciones para mantener la compatibilidad con la UI
      const inscripcionesData = alumnosData.map(alumno => ({
        id: alumno.id,
        alumnoId: alumno.id,
        cursoId: cursoId || '',
        alumno: alumno,
        cursoTexto: cursoData.nombreReferencia,
        texto1: alumno.calificacion || '',
        texto2: alumno.observaciones || '',
        certificado_otorgado: alumno.certificado_otorgado || '',
        motivo_entrega: alumno.motivo_entrega || '',
        createdAt: alumno.createdAt
      }));
      setInscripciones(inscripcionesData);
    } catch (error) {
      setError('Error al cargar los datos');
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: 'array' });
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];
        const jsonData = XLSX.utils.sheet_to_json(worksheet);

        if (jsonData.length === 0) {
          setError('El archivo Excel está vacío');
          return;
        }

        // Validar columnas obligatorias
        const columnas = Object.keys(jsonData[0] as any);
        const tieneNombre = columnas.some(c => ['Nombre', 'NOMBRE', 'NombreAlumno', 'NombreApellido', 'nombre_alumno'].includes(c));
        const tieneRut = columnas.some(c => ['RUT', 'Rut', 'rut'].includes(c));

        if (!tieneNombre || !tieneRut) {
          const faltantes = [];
          if (!tieneNombre) faltantes.push('Nombre (o NOMBRE, NombreApellido)');
          if (!tieneRut) faltantes.push('RUT');
          setError(`El Excel debe contener las columnas obligatorias: ${faltantes.join(', ')}`);
          return;
        }

        // Validar que no haya filas sin nombre o RUT
        const filasInvalidas = (jsonData as any[]).filter((row, idx) => {
          const nombre = findFieldValue(row, ['Nombre', 'NOMBRE', 'NombreAlumno', 'NombreApellido', 'nombre_alumno']);
          const rut = findFieldValue(row, ['RUT', 'Rut', 'rut']);
          return !nombre.trim() || !rut.trim();
        });

        if (filasInvalidas.length > 0) {
          setError(`${filasInvalidas.length} fila(s) del Excel tienen Nombre o RUT vacíos. Verifique el archivo.`);
          return;
        }

        setError('');
        setUploadData(jsonData);
        setOpenUploadDialog(true);
      } catch (error) {
        setError('Error al leer el archivo Excel');
      }
    };
    reader.readAsArrayBuffer(file);
  };

  // Función helper para buscar un campo en el row ignorando mayúsculas/minúsculas
  const findFieldValue = (row: any, fieldNames: string[]): string => {
    const keys = Object.keys(row);
    for (const fieldName of fieldNames) {
      // Buscar coincidencia exacta primero
      if (row[fieldName] !== undefined && row[fieldName] !== null) {
        return String(row[fieldName]);
      }
      // Buscar ignorando mayúsculas/minúsculas
      const foundKey = keys.find(k => k.toLowerCase() === fieldName.toLowerCase());
      if (foundKey && row[foundKey] !== undefined && row[foundKey] !== null) {
        return String(row[foundKey]);
      }
    }
    return '';
  };

  // Función para obtener el nombre del alumno (desde nombreApellido o desde observaciones JSON)
  const getAlumnoNombre = (inscripcion: Inscripcion): string => {
    // Primero intentar desde nombreApellido
    if (inscripcion.alumno?.nombreApellido) {
      return inscripcion.alumno.nombreApellido;
    }

    // Si no hay nombreApellido, buscar en observaciones (JSON)
    const obsText = inscripcion.alumno?.observaciones || inscripcion.texto2 || '';
    try {
      if (obsText.trim().startsWith('{') && obsText.trim().endsWith('}')) {
        const parsed = JSON.parse(obsText);
        // Buscar variantes del campo nombre
        return parsed['Nombre'] || parsed['NOMBRE'] || parsed['nombre'] ||
               parsed['NombreAlumno'] || parsed['NombreApellido'] || '';
      }
    } catch {
      // No es JSON válido
    }
    return '';
  };

  // Función para obtener el RUT del alumno (desde rut o desde observaciones JSON)
  const getAlumnoRut = (inscripcion: Inscripcion): string => {
    // Primero intentar desde rut
    if (inscripcion.alumno?.rut) {
      return inscripcion.alumno.rut;
    }

    // Si no hay rut, buscar en observaciones (JSON)
    const obsText = inscripcion.alumno?.observaciones || inscripcion.texto2 || '';
    try {
      if (obsText.trim().startsWith('{') && obsText.trim().endsWith('}')) {
        const parsed = JSON.parse(obsText);
        return parsed['RUT'] || parsed['Rut'] || parsed['rut'] || '';
      }
    } catch {
      // No es JSON válido
    }
    return '';
  };

  const processUpload = async () => {
    setUploading(true);
    setError('');

    try {
      const alumnosData = uploadData.map((row: any) => {
        // Buscar nombre en múltiples variantes (case-insensitive)
        const nombre = findFieldValue(row, ['Nombre', 'NOMBRE', 'NombreAlumno', 'NombreApellido', 'nombre_alumno']);
        const rut = findFieldValue(row, ['RUT', 'Rut', 'rut']);

        // Todos los campos del Excel van como JSON en Observaciones
        const allFields: Record<string, string> = {};
        Object.keys(row).forEach(key => {
          allFields[key] = row[key] !== undefined && row[key] !== null ? String(row[key]) : '';
        });

        return {
          NombreApellido: nombre,
          RUT: rut,
          Calificacion: String(row['Calificacion'] || row['calificacion'] || row['Texto1'] || ''),
          Observaciones: JSON.stringify(allFields),
          certificado_otorgado: row['CertificadoOtorgado'] || row['certificado_otorgado'] || '',
          motivo_entrega: row['MotivoEntrega'] || row['motivo_entrega'] || '',
        };
      });

      await apiService.post(`/api/app/alumnos/bulk-import/${cursoId}`, {
        Alumnos: alumnosData,
      });

      setOpenUploadDialog(false);
      setUploadData([]);
      loadData();
    } catch (error) {
      setError('Error al procesar la carga masiva');
    } finally {
      setUploading(false);
    }
  };

  const handleOpenDialog = (inscripcion?: Inscripcion) => {
    if (inscripcion) {
      setEditingInscripcion(inscripcion);

      // Detectar si hay datos dinámicos (JSON en observaciones)
      let parsedDynamic: Record<string, string> = {};
      let isDynamic = false;

      // Intentar obtener observaciones desde alumno directamente
      const obsText = inscripcion.alumno?.observaciones || inscripcion.texto2 || '';

      try {
        if (obsText.trim().startsWith('{') && obsText.trim().endsWith('}')) {
          parsedDynamic = JSON.parse(obsText);
          isDynamic = true;
        }
      } catch (e) {
        // No es JSON válido
        isDynamic = false;
      }

      // Si el curso tiene plantilla, siempre usar campos dinámicos
      if (plantilla && plantillaVariables.length > 0) {
        isDynamic = true;
        // Inicializar todos los campos de la plantilla, usando valores existentes si hay
        const fieldsWithDefaults: Record<string, string> = {};
        plantillaVariables.forEach(variable => {
          fieldsWithDefaults[variable] = parsedDynamic[variable] || '';
        });
        parsedDynamic = fieldsWithDefaults;
      }

      setHasDynamicData(isDynamic);
      setDynamicFields(parsedDynamic);

      setFormData({
        alumnoNombre: inscripcion.alumno?.nombreApellido || '',
        alumnoRut: inscripcion.alumno?.rut || '',
        cursoTexto: inscripcion.cursoTexto,
        texto1: inscripcion.texto1,
        texto2: isDynamic ? '' : inscripcion.texto2, // Vacío si es dinámico
        certificadoOtorgado: (inscripcion as any).certificado_otorgado || '',
        motivoEntrega: (inscripcion as any).motivo_entrega || '',
      });
    } else {
      setEditingInscripcion(null);

      // Si el curso tiene plantilla, inicializar campos dinámicos
      if (plantilla && plantillaVariables.length > 0) {
        const initialFields: Record<string, string> = {};
        plantillaVariables.forEach(variable => {
          initialFields[variable] = '';
        });
        setHasDynamicData(true);
        setDynamicFields(initialFields);
      } else {
        setHasDynamicData(false);
        setDynamicFields({});
      }

      setFormData({
        alumnoNombre: '',
        alumnoRut: '',
        cursoTexto: curso?.nombreReferencia || '',
        texto1: '',
        texto2: '',
        certificadoOtorgado: '',
        motivoEntrega: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingInscripcion(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      let observacionesData = formData.texto2;
      let nombreApellido = formData.alumnoNombre;
      let rut = formData.alumnoRut;

      // Si hay datos dinámicos, validar y serializar
      if (hasDynamicData) {
        // Obtener Nombre y RUT desde dynamicFields
        nombreApellido = dynamicFields['NOMBRE'] || '';
        rut = dynamicFields['RUT'] || '';

        // Validar obligatorios
        if (!nombreApellido.trim()) {
          setError('El campo NOMBRE es obligatorio');
          return;
        }
        if (!rut.trim()) {
          setError('El campo RUT es obligatorio');
          return;
        }

        // Validar que no haya campos vacíos
        const emptyFields = Object.entries(dynamicFields).filter(([key, value]) => !value || value.trim() === '');
        if (emptyFields.length > 0) {
          setError(`Los siguientes campos no pueden estar vacíos: ${emptyFields.map(([k]) => k).join(', ')}`);
          return;
        }

        // Sanitizar dynamicFields: convertir todos los valores a strings válidos
        const sanitizedFields: Record<string, string> = {};
        Object.entries(dynamicFields).forEach(([key, value]) => {
          // Asegurar que el valor es string y no undefined/null
          sanitizedFields[key] = (value !== undefined && value !== null) ? String(value).trim() : '';
        });

        // Validar que el JSON se puede serializar correctamente
        try {
          observacionesData = JSON.stringify(sanitizedFields);
          // Verificar que se puede parsear de vuelta
          JSON.parse(observacionesData);
        } catch (error) {
          console.error('Error al serializar JSON:', sanitizedFields, error);
          setError('Error al procesar los datos dinámicos. Verifica que no haya caracteres especiales inválidos.');
          return;
        }
      } else {
        // Formulario simple (sin plantilla dinámica)
        if (!nombreApellido.trim()) {
          setError('El nombre del alumno es obligatorio');
          return;
        }
        if (!rut.trim()) {
          setError('El RUT del alumno es obligatorio');
          return;
        }
      }

      if (editingInscripcion) {
        await apiService.put(`/api/app/alumnos/${editingInscripcion.alumnoId}/registro/${cursoId}`, {
          calificacion: formData.texto1,
          observaciones: observacionesData,
          certificado_otorgado: formData.certificadoOtorgado,
          motivo_entrega: formData.motivoEntrega,
          nombreApellido: nombreApellido,
          rut: rut
        });
      } else {
        await apiService.post(`/api/app/alumnos/create-with-registro`, {
          nombreApellido: nombreApellido,
          rut: rut,
          cursoId: cursoId,
          calificacion: formData.texto1,
          observaciones: observacionesData,
          certificado_otorgado: formData.certificadoOtorgado,
          motivo_entrega: formData.motivoEntrega
        });
      }
      handleCloseDialog();
      loadData();
    } catch (error: any) {
      console.error('Error al guardar:', error);
      setError(error.response?.data?.message || error.message || 'Error al guardar el alumno');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('¿Está seguro de eliminar este alumno del curso?')) {
      try {
        await apiService.delete(`/api/app/alumnos/${id}/registro/${cursoId}`);
        loadData();
      } catch (error) {
        setError('Error al eliminar el alumno del curso');
      }
    }
  };

  const downloadTemplate = () => {
    let template: any[];

    if (plantilla && plantillaVariables.length > 0) {
      // Curso con plantilla: usar las variables de la plantilla
      const exampleRow: Record<string, string> = {};
      plantillaVariables.forEach((variable: string) => {
        // Poner ejemplos para Nombre y RUT
        if (variable.toUpperCase() === 'NOMBRE') {
          exampleRow[variable] = 'Juan Pérez';
        } else if (variable.toUpperCase() === 'RUT') {
          exampleRow[variable] = '12345678-9';
        } else {
          exampleRow[variable] = '';
        }
      });
      template = [exampleRow];
    } else {
      // Curso sin plantilla: modelo básico
      template = [
        {
          Nombre: 'Juan Pérez',
          RUT: '12345678-9',
        },
      ];
    }

    const worksheet = XLSX.utils.json_to_sheet(template);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Alumnos');
    const fileName = plantilla ? `plantilla_${plantilla.nombre.replace(/\s+/g, '_')}.xlsx` : 'plantilla_alumnos.xlsx';
    XLSX.writeFile(workbook, fileName);
  };

  if (loading || !user) return <Layout><Typography>Cargando...</Typography></Layout>;

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/otec/cursos')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
            Alumnos - {curso?.nombreReferencia}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<Download />}
              onClick={downloadTemplate}
            >
              Descargar Plantilla
            </Button>
            <Button
              variant="outlined"
              component="label"
              startIcon={<Upload />}
            >
              Importar Excel
              <input
                type="file"
                hidden
                accept=".xlsx,.xls"
                onChange={handleFileUpload}
              />
            </Button>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => handleOpenDialog()}
            >
              Agregar Alumno
            </Button>
          </Box>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Alumno</TableCell>
                <TableCell>RUT</TableCell>
                <TableCell>Fecha Inscripción</TableCell>
                <TableCell align="center">Acciones</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {inscripciones.map((inscripcion) => (
                <TableRow key={inscripcion.id}>
                  <TableCell>{getAlumnoNombre(inscripcion)}</TableCell>
                  <TableCell>{getAlumnoRut(inscripcion)}</TableCell>
                  <TableCell>
                    {new Date(inscripcion.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(inscripcion)}
                    >
                      <Edit />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(inscripcion.id)}
                      color="error"
                    >
                      <Delete />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Dialog para inscripción individual */}
        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <form onSubmit={handleSubmit}>
            <DialogTitle>
              {editingInscripcion ? 'Editar Inscripción' : 'Nueva Inscripción'}
            </DialogTitle>
            <DialogContent>
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}
              {/* Solo mostrar campos normales si NO hay datos dinámicos */}
              {!hasDynamicData && (
                <>
                  <TextField
                    fullWidth
                    label="Nombre del Alumno"
                    value={formData.alumnoNombre}
                    onChange={(e) =>
                      setFormData({ ...formData, alumnoNombre: e.target.value })
                    }
                    margin="normal"
                    required
                  />
                  <TextField
                    fullWidth
                    label="RUT"
                    value={formData.alumnoRut}
                    onChange={(e) =>
                      setFormData({ ...formData, alumnoRut: e.target.value })
                    }
                    margin="normal"
                    required
                  />
                </>
              )}

              {/* Campos dinámicos para alumnos con datos JSON */}
              {hasDynamicData && (
                <Box sx={{ mt: 2 }}>
                  <Alert severity="info" sx={{ mb: 2 }}>
                    Este alumno tiene datos dinámicos. Edite los campos a continuación.
                  </Alert>
                  {Object.entries(dynamicFields).map(([key, value]) => (
                    <TextField
                      key={key}
                      fullWidth
                      label={key}
                      value={value}
                      onChange={(e) =>
                        setDynamicFields({ ...dynamicFields, [key]: e.target.value })
                      }
                      margin="normal"
                      required
                    />
                  ))}
                </Box>
              )}

            </DialogContent>
            <DialogActions>
              <Button onClick={handleCloseDialog}>Cancelar</Button>
              <Button type="submit" variant="contained">
                {editingInscripcion ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogActions>
          </form>
        </Dialog>

        {/* Dialog para carga masiva */}
        <Dialog open={openUploadDialog} onClose={() => setOpenUploadDialog(false)} maxWidth="lg" fullWidth>
          <DialogTitle>
            Previsualización de Carga Masiva
            {plantilla && <Chip label={`Plantilla: ${plantilla.nombre}`} size="small" sx={{ ml: 2 }} />}
          </DialogTitle>
          <DialogContent>
            {uploading && <LinearProgress sx={{ mb: 2 }} />}
            <Typography variant="body2" sx={{ mb: 2 }}>
              Se procesarán {uploadData.length} registros
            </Typography>
            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
              <Table stickyHeader size="small">
                <TableHead>
                  <TableRow>
                    {plantilla && plantillaVariables.length > 0 ? (
                      // Mostrar columnas de la plantilla
                      plantillaVariables.map((col) => (
                        <TableCell key={col}>{col}</TableCell>
                      ))
                    ) : (
                      // Mostrar columnas básicas
                      <>
                        <TableCell>Nombre</TableCell>
                        <TableCell>RUT</TableCell>
                      </>
                    )}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {uploadData.slice(0, 10).map((row: any, index) => (
                    <TableRow key={index}>
                      {plantilla && plantillaVariables.length > 0 ? (
                        // Mostrar valores de las columnas de la plantilla
                        plantillaVariables.map((col) => (
                          <TableCell key={col}>{row[col] ?? 'N/A'}</TableCell>
                        ))
                      ) : (
                        // Mostrar valores básicos
                        <>
                          <TableCell>{row['Nombre'] || row['nombre'] || 'N/A'}</TableCell>
                          <TableCell>{row['RUT'] || row['rut'] || 'N/A'}</TableCell>
                        </>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {uploadData.length > 10 && (
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                Mostrando 10 de {uploadData.length} registros
              </Typography>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenUploadDialog(false)}>Cancelar</Button>
            <Button
              onClick={processUpload}
              variant="contained"
              disabled={uploading}
            >
              {uploading ? 'Procesando...' : 'Confirmar Carga'}
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Layout>
  );
}