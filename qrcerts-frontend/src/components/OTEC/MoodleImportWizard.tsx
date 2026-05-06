import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  Alert,
  CircularProgress,
  Stepper,
  Step,
  StepLabel,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Checkbox,
  FormControlLabel,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  Grid,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  IconButton,
  ToggleButton,
  ToggleButtonGroup
} from '@mui/material';
import {
  School,
  People,
  Settings,
  CheckCircle,
  ArrowBack,
  ArrowForward,
  Search,
  Add,
  Delete,
  Description,
  Assignment,
  CropLandscape,
  CropPortrait
} from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { apiService } from '../../services/api';
import { API_ENDPOINTS } from '../../config/api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { PlantillaCertificado } from '../../types';

interface MoodleCourse {
  moodleId: number;
  nombre: string;
  shortName: string;
  categoria: string;
  cantidadAlumnos: number;
}

interface MoodleStudent {
  moodleUserId: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  email: string;
  username: string;
  idNumber: string;
  notaFinal: number | null;
  notaFormateada: string | null;
  aprobado: boolean;
  camposCustom?: Array<{ nombre: string; shortName: string; valor: string }>;
}

interface MoodleField {
  key: string;
  label: string;
  tipo: string;
  esCustom: boolean;
  valorEjemplo?: string;
}

interface FieldMapping {
  campoMoodle: string;
  variablePlantilla: string;
}

interface StudentsResponse {
  total: number;
  aprobados: number;
  estudiantes: MoodleStudent[];
}

interface FieldsResponse {
  camposMoodle: MoodleField[];
  variablesCertificado: any[];
}

interface ImportResult {
  success: boolean;
  cursoLocalId: string;
  importados: number;
  actualizados: number;
  errores: number;
  erroresDetalle?: string[];
  message: string;
}

// Misma estructura que CursoWizard
interface CursoFormData {
  nombreReferencia: string;
  nombre_visualizar_certificado: string;
  certificate_type: 'HORIZONTAL' | 'VERTICAL';
  plantillaId: string;
  fondoPath: string;
  qrDestino: number;
  estado: number;
  vencimiento: string;
  footer_1: string;
  footer_2: string;
  contenidoHtml: string;
  footerHtml: string;
}

// Steps dinámicos según si la plantilla tiene variables de curso
const getSteps = (hasCursoVariables: boolean) => {
  if (hasCursoVariables) {
    return [
      'Seleccionar Plantilla',
      'Curso Moodle',
      'Datos del Curso',
      'Variables del Curso',
      'Mapear Campos',
      'Confirmar'
    ];
  }
  return [
    'Seleccionar Plantilla',
    'Curso Moodle',
    'Datos del Curso',
    'Mapear Campos',
    'Confirmar'
  ];
};

export function MoodleImportWizard() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Step 1: Plantillas
  const [plantillas, setPlantillas] = useState<PlantillaCertificado[]>([]);
  const [selectedPlantilla, setSelectedPlantilla] = useState<PlantillaCertificado | null>(null);
  const [plantillaVariables, setPlantillaVariables] = useState<string[]>([]);

  // Variables de curso (contenido_cursos de la plantilla - comunes a todos los alumnos)
  const [cursoVariables, setCursoVariables] = useState<string[]>([]);
  const [cursoVariablesValues, setCursoVariablesValues] = useState<Record<string, string>>({});

  // Step 2: Courses Moodle
  const [courses, setCourses] = useState<MoodleCourse[]>([]);
  const [selectedCourse, setSelectedCourse] = useState<MoodleCourse | null>(null);
  const [searchCourse, setSearchCourse] = useState('');

  // Step 3: Datos del curso (mismo formato que CursoWizard)
  const [formData, setFormData] = useState<CursoFormData>({
    nombreReferencia: '',
    nombre_visualizar_certificado: '',
    certificate_type: 'HORIZONTAL',
    plantillaId: '',
    fondoPath: '',
    qrDestino: 1,
    estado: 1,
    vencimiento: '',
    footer_1: '',
    footer_2: '',
    contenidoHtml: '',
    footerHtml: '',
  });

  // Step 4: Field Mapping
  const [moodleFields, setMoodleFields] = useState<MoodleField[]>([]);
  const [fieldMappings, setFieldMappings] = useState<FieldMapping[]>([]);

  // Step 5: Students & Import
  const [students, setStudents] = useState<MoodleStudent[]>([]);
  const [totalStudents, setTotalStudents] = useState(0);
  const [approvedCount, setApprovedCount] = useState(0);
  const [studentFilter, setStudentFilter] = useState<'all' | 'approved' | 'notApproved'>('all');
  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);

  useEffect(() => {
    loadPlantillas();
  }, []);

  const loadPlantillas = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await apiService.get<PlantillaCertificado[]>('/api/app/plantillas-certificados');
      setPlantillas(data);
      if (data.length === 0) {
        setError('No tiene plantillas creadas. Debe crear una plantilla primero.');
      }
    } catch (err: any) {
      setError('Error al cargar las plantillas');
    } finally {
      setLoading(false);
    }
  };

  const loadCourses = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await apiService.get<MoodleCourse[]>(API_ENDPOINTS.MOODLE_COURSES);
      setCourses(data);
    } catch (err: any) {
      if (err.response?.status === 400) {
        setError('No hay configuracion de Moodle activa. Configure la conexion primero en Integracion Moodle.');
      } else {
        setError(err.response?.data?.message || 'Error al cargar los cursos de Moodle');
      }
    } finally {
      setLoading(false);
    }
  };

  const loadStudents = async (courseId: number) => {
    try {
      setLoading(true);
      setError('');
      const data = await apiService.get<StudentsResponse>(
        `${API_ENDPOINTS.MOODLE_COURSES}/${courseId}/students`
      );
      console.log('=== LOAD STUDENTS ===');
      console.log('API Response - total:', data.total);
      console.log('API Response - aprobados:', data.aprobados);
      console.log('API Response - estudiantes.length:', data.estudiantes?.length);
      console.log('API Response - estudiantes:', data.estudiantes);
      setStudents(data.estudiantes);
      setTotalStudents(data.total);
      setApprovedCount(data.aprobados);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar los estudiantes');
    } finally {
      setLoading(false);
    }
  };

  const loadFields = async (courseId: number) => {
    try {
      const data = await apiService.get<FieldsResponse>(
        `${API_ENDPOINTS.MOODLE_COURSES}/${courseId}/fields`
      );
      setMoodleFields(data.camposMoodle);

      // Auto-mapear campos comunes si hay variables de plantilla
      if (plantillaVariables.length > 0 && fieldMappings.length === 0) {
        const autoMappings: FieldMapping[] = [];
        const commonMappings: { [key: string]: string[] } = {
          'fullname': ['NOMBRE', 'NOMBRE_COMPLETO', 'ALUMNO'],
          'email': ['EMAIL', 'CORREO'],
          'idnumber': ['RUT', 'DNI', 'DOCUMENTO'],
        };

        for (const [moodleKey, plantillaOptions] of Object.entries(commonMappings)) {
          const moodleField = data.camposMoodle.find(f => f.key === moodleKey);
          if (moodleField) {
            const plantillaVar = plantillaVariables.find(v =>
              plantillaOptions.some(opt => v.toUpperCase().includes(opt))
            );
            if (plantillaVar) {
              autoMappings.push({ campoMoodle: moodleKey, variablePlantilla: plantillaVar });
            }
          }
        }

        if (autoMappings.length > 0) {
          setFieldMappings(autoMappings);
        }
      }
    } catch (err: any) {
      console.error('Error loading fields:', err);
    }
  };

  const handleSelectPlantilla = (plantilla: PlantillaCertificado) => {
    setSelectedPlantilla(plantilla);
    setFormData(prev => ({ ...prev, plantillaId: plantilla.id || '' }));

    // Extraer variables de alumno (contenido_alumnos)
    let varsAlumno: string[] = [];
    if (plantilla.contenido_alumnos) {
      try {
        varsAlumno = JSON.parse(plantilla.contenido_alumnos);
      } catch {
        varsAlumno = plantilla.contenido_alumnos.split(',').map(v => v.trim()).filter(v => v);
      }
    }
    setPlantillaVariables(varsAlumno);

    // Extraer variables de curso (contenido_cursos) - comunes a todos los alumnos
    let varsCurso: string[] = [];
    if (plantilla.contenido_cursos) {
      try {
        varsCurso = JSON.parse(plantilla.contenido_cursos);
      } catch {
        varsCurso = plantilla.contenido_cursos.split(',').map(v => v.trim()).filter(v => v);
      }
    }
    setCursoVariables(varsCurso);

    // Inicializar valores vacíos para las variables de curso
    if (varsCurso.length > 0) {
      const initialValues: Record<string, string> = {};
      varsCurso.forEach(v => { initialValues[v] = ''; });
      setCursoVariablesValues(initialValues);
    }
  };

  const handleSelectCourse = (course: MoodleCourse) => {
    setSelectedCourse(course);
    // Pre-llenar datos del curso desde Moodle
    setFormData(prev => ({
      ...prev,
      nombreReferencia: course.nombre,
      nombre_visualizar_certificado: course.nombre,
    }));
  };

  // Obtener steps actuales
  const steps = getSteps(cursoVariables.length > 0);
  const currentStepName = steps[activeStep];

  const handleNext = async () => {
    if (currentStepName === 'Seleccionar Plantilla' && selectedPlantilla) {
      await loadCourses();
    } else if (currentStepName === 'Datos del Curso' && selectedCourse) {
      // Si no hay variables de curso, cargar campos de Moodle aquí
      if (cursoVariables.length === 0) {
        await loadFields(selectedCourse.moodleId);
      }
    } else if (currentStepName === 'Variables del Curso' && selectedCourse) {
      await loadFields(selectedCourse.moodleId);
    } else if (currentStepName === 'Mapear Campos' && selectedCourse) {
      await loadStudents(selectedCourse.moodleId);
    }
    setActiveStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setActiveStep((prev) => prev - 1);
  };

  const handleImport = async () => {
    if (!selectedCourse || !selectedPlantilla) return;

    try {
      setImporting(true);
      setError('');

      // 1. Crear el curso con el mismo payload que CursoWizard
      // Si hay variables de curso, guardarlas en layoutJson
      const layoutJson = cursoVariables.length > 0
        ? JSON.stringify(cursoVariablesValues)
        : '{}';

      const cursoPayload = {
        ...formData,
        plantillaId: formData.plantillaId || '',
        layoutJson: layoutJson,
        otecId: (user as any)?.otecId,
      };

      const cursoResponse = await apiService.post<{ id: string }>('/api/app/cursos', cursoPayload);
      const nuevoCursoId = cursoResponse.id;

      // 2. Importar alumnos desde Moodle
      const alumnosToImport = studentFilter === 'all'
        ? students
        : studentFilter === 'approved'
          ? students.filter(s => s.aprobado)
          : students.filter(s => !s.aprobado);

      console.log('=== DEBUG IMPORTACION ===');
      console.log('Total students en state:', students.length);
      console.log('studentFilter:', studentFilter);
      console.log('alumnosToImport:', alumnosToImport.length);
      console.log('fieldMappings:', fieldMappings);

      // Convertir estudiantes de Moodle al formato de bulk-import
      const alumnosData = alumnosToImport.map((student) => {
        console.log('Procesando estudiante:', student.nombreCompleto, 'camposCustom:', student.camposCustom);
        // Crear objeto con todos los campos mapeados
        const allFields: Record<string, string> = {};

        // Agregar campos estándar
        allFields['fullname'] = student.nombreCompleto;
        allFields['email'] = student.email;
        allFields['idnumber'] = student.idNumber;
        allFields['nota'] = student.notaFormateada || '';

        // Agregar campos custom si existen
        if (student.camposCustom) {
          student.camposCustom.forEach(campo => {
            console.log('Campo custom:', campo.shortName, '=', campo.valor);
            allFields[campo.shortName] = campo.valor;
            // También agregar con prefijo profile_field_ por si el mapeo usa ese formato
            allFields[`profile_field_${campo.shortName}`] = campo.valor;
          });
        }

        // Aplicar mapeos para obtener valores
        const mappedFields: Record<string, string> = {};
        fieldMappings.forEach(mapping => {
          if (mapping.campoMoodle && mapping.variablePlantilla) {
            mappedFields[mapping.variablePlantilla] = allFields[mapping.campoMoodle] || '';
          }
        });

        // Buscar nombre y RUT en los mapeos
        let nombre = student.nombreCompleto;
        let rut = student.idNumber || '';

        fieldMappings.forEach(mapping => {
          const value = allFields[mapping.campoMoodle] || '';
          if (mapping.variablePlantilla.toUpperCase().includes('NOMBRE')) {
            nombre = value || nombre;
          }
          if (mapping.variablePlantilla.toUpperCase().includes('RUT') ||
              mapping.variablePlantilla.toUpperCase().includes('DNI')) {
            rut = value || rut;
          }
        });

        return {
          NombreApellido: nombre,
          RUT: rut,
          Calificacion: student.notaFormateada || '',
          Observaciones: JSON.stringify({ ...allFields, ...mappedFields }),
          certificado_otorgado: '',
          motivo_entrega: '',
          MoodleUserId: student.moodleUserId,
          MoodleCourseId: selectedCourse?.moodleId,
        };
      });

      console.log('=== PAYLOAD FINAL ===');
      console.log('alumnosData.length:', alumnosData.length);
      console.log('alumnosData:', JSON.stringify(alumnosData, null, 2));

      // Llamar a bulk-import
      await apiService.post(`/api/app/alumnos/bulk-import/${nuevoCursoId}`, {
        Alumnos: alumnosData,
      });

      setImportResult({
        success: true,
        cursoLocalId: nuevoCursoId,
        importados: alumnosData.length,
        actualizados: 0,
        errores: 0,
        message: `Curso creado y ${alumnosData.length} alumnos importados correctamente`,
      });

    } catch (err: any) {
      setError(err.response?.data?.message || 'Error durante la importacion');
    } finally {
      setImporting(false);
    }
  };

  const filteredCourses = courses.filter(c =>
    c.nombre.toLowerCase().includes(searchCourse.toLowerCase()) ||
    c.shortName.toLowerCase().includes(searchCourse.toLowerCase())
  );

  const displayedStudents = studentFilter === 'all'
    ? students
    : studentFilter === 'approved'
      ? students.filter(s => s.aprobado)
      : students.filter(s => !s.aprobado);

  // Campos disponibles para mapeo
  const usedMoodleFields = fieldMappings.map(m => m.campoMoodle);
  const availableMoodleFields = moodleFields.filter(f => !usedMoodleFields.includes(f.key));
  const usedPlantillaVars = fieldMappings.map(m => m.variablePlantilla);
  const availablePlantillaVars = plantillaVariables.filter(v => !usedPlantillaVars.includes(v));

  const renderStepContent = () => {
    switch (currentStepName) {
      // PASO 1: Seleccionar Plantilla (combo desplegable)
      case 'Seleccionar Plantilla':
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              Seleccione la plantilla de certificado que desea usar.
            </Typography>

            {loading ? (
              <Box display="flex" justifyContent="center" p={4}>
                <CircularProgress />
              </Box>
            ) : plantillas.length === 0 ? (
              <Alert severity="warning">
                No tiene plantillas creadas.
                <Button size="small" onClick={() => navigate('/otec/plantillas')}>
                  Crear Plantilla
                </Button>
              </Alert>
            ) : (
              <FormControl fullWidth sx={{ mt: 2 }}>
                <InputLabel>Plantilla de Certificado</InputLabel>
                <Select
                  value={selectedPlantilla?.id || ''}
                  label="Plantilla de Certificado"
                  onChange={(e) => {
                    const plantilla = plantillas.find(p => p.id === e.target.value);
                    if (plantilla) handleSelectPlantilla(plantilla);
                  }}
                >
                  {plantillas.map((plantilla) => (
                    <MenuItem key={plantilla.id} value={plantilla.id}>
                      {plantilla.nombre}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}

            {selectedPlantilla && plantillaVariables.length > 0 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="caption" color="text.secondary">
                  Variables de alumno en esta plantilla:
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5 }}>
                  {plantillaVariables.map((v) => (
                    <Chip key={v} label={v} size="small" variant="outlined" />
                  ))}
                </Box>
              </Box>
            )}
          </Box>
        );

      // PASO 2: Seleccionar Curso Moodle
      case 'Curso Moodle':
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              Seleccione el curso de Moodle desde el cual importar estudiantes.
            </Typography>

            <TextField
              fullWidth
              placeholder="Buscar curso..."
              value={searchCourse}
              onChange={(e) => setSearchCourse(e.target.value)}
              InputProps={{
                startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />
              }}
              sx={{ mb: 2 }}
            />

            {loading ? (
              <Box display="flex" justifyContent="center" p={4}>
                <CircularProgress />
              </Box>
            ) : (
              <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                <Table stickyHeader size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell></TableCell>
                      <TableCell>Nombre del Curso</TableCell>
                      <TableCell>Codigo</TableCell>
                      <TableCell>Categoria</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filteredCourses.map((course) => (
                      <TableRow
                        key={course.moodleId}
                        hover
                        selected={selectedCourse?.moodleId === course.moodleId}
                        onClick={() => handleSelectCourse(course)}
                        sx={{ cursor: 'pointer' }}
                      >
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selectedCourse?.moodleId === course.moodleId}
                            onChange={() => handleSelectCourse(course)}
                          />
                        </TableCell>
                        <TableCell>{course.nombre}</TableCell>
                        <TableCell>{course.shortName}</TableCell>
                        <TableCell>{course.categoria}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Box>
        );

      // PASO 3: Datos del Curso (mismos campos que CursoWizard)
      case 'Datos del Curso':
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              Complete los datos del curso.
            </Typography>

            <Grid container spacing={3} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Nombre de Referencia"
                  value={formData.nombreReferencia}
                  onChange={(e) => setFormData(prev => ({ ...prev, nombreReferencia: e.target.value }))}
                  helperText="Nombre interno para identificar el curso"
                  required
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Nombre en Certificado"
                  value={formData.nombre_visualizar_certificado}
                  onChange={(e) => setFormData(prev => ({ ...prev, nombre_visualizar_certificado: e.target.value }))}
                  helperText="Este nombre aparecera en el certificado"
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" gutterBottom>
                  Orientacion del Certificado
                </Typography>
                <ToggleButtonGroup
                  value={formData.certificate_type}
                  exclusive
                  onChange={(_, value) => {
                    if (value) setFormData(prev => ({ ...prev, certificate_type: value }));
                  }}
                  fullWidth
                >
                  <ToggleButton value="HORIZONTAL">
                    <CropLandscape sx={{ mr: 1 }} /> Horizontal
                  </ToggleButton>
                  <ToggleButton value="VERTICAL">
                    <CropPortrait sx={{ mr: 1 }} /> Vertical
                  </ToggleButton>
                </ToggleButtonGroup>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Fecha de Vencimiento"
                  type="date"
                  value={formData.vencimiento}
                  onChange={(e) => setFormData(prev => ({ ...prev, vencimiento: e.target.value }))}
                  InputLabelProps={{ shrink: true }}
                  helperText="Opcional - Fecha hasta la cual es valido"
                />
              </Grid>
              <Grid item xs={12}>
                <Card variant="outlined">
                  <CardContent sx={{ py: 1 }}>
                    <Typography variant="caption" color="text.secondary">
                      Plantilla seleccionada:
                    </Typography>
                    <Typography variant="body2">
                      <Description sx={{ fontSize: 16, mr: 0.5, verticalAlign: 'middle' }} />
                      {selectedPlantilla?.nombre}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Box>
        );

      // PASO: Variables del Curso (solo si la plantilla tiene variables de curso)
      case 'Variables del Curso':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Variables del Curso
            </Typography>
            <Alert severity="info" sx={{ mb: 3 }}>
              Complete los valores de las variables que se aplicaran a todos los certificados de este curso.
              Estas variables son propias del curso y no cambian por alumno.
            </Alert>
            <Grid container spacing={3}>
              {cursoVariables.map((variable) => (
                <Grid item xs={12} md={6} key={variable}>
                  <TextField
                    fullWidth
                    label={variable}
                    value={cursoVariablesValues[variable] || ''}
                    onChange={(e) => setCursoVariablesValues(prev => ({
                      ...prev,
                      [variable]: e.target.value
                    }))}
                    placeholder={`Ingrese valor para {{${variable}}}`}
                  />
                </Grid>
              ))}
            </Grid>
          </Box>
        );

      // PASO: Mapear Campos
      case 'Mapear Campos':
        return (
          <Box>
            <Typography variant="body1" gutterBottom>
              Mapee los campos de Moodle con las variables de la plantilla.
            </Typography>

            <Paper sx={{ p: 2, mt: 2 }}>
              {fieldMappings.map((mapping, index) => {
                const moodleField = moodleFields.find(f => f.key === mapping.campoMoodle);
                return (
                  <Box key={index} sx={{ display: 'flex', gap: 2, mb: 2, alignItems: 'center' }}>
                    <FormControl size="small" sx={{ minWidth: 220 }}>
                      <InputLabel>Campo Moodle</InputLabel>
                      <Select
                        value={mapping.campoMoodle}
                        label="Campo Moodle"
                        onChange={(e) => {
                          const newMappings = [...fieldMappings];
                          newMappings[index] = { ...mapping, campoMoodle: e.target.value };
                          setFieldMappings(newMappings);
                        }}
                      >
                        {moodleField && (
                          <MenuItem value={moodleField.key}>
                            {moodleField.label}
                            {moodleField.esCustom && ' (Custom)'}
                          </MenuItem>
                        )}
                        {availableMoodleFields.map(field => (
                          <MenuItem key={field.key} value={field.key}>
                            {field.label}
                            {field.esCustom && ' (Custom)'}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>

                    <Typography variant="body2" color="text.secondary">→</Typography>

                    <FormControl size="small" sx={{ minWidth: 220 }}>
                      <InputLabel>Variable Plantilla</InputLabel>
                      <Select
                        value={mapping.variablePlantilla}
                        label="Variable Plantilla"
                        onChange={(e) => {
                          const newMappings = [...fieldMappings];
                          newMappings[index] = { ...mapping, variablePlantilla: e.target.value };
                          setFieldMappings(newMappings);
                        }}
                      >
                        {mapping.variablePlantilla && (
                          <MenuItem value={mapping.variablePlantilla}>
                            {mapping.variablePlantilla}
                          </MenuItem>
                        )}
                        {availablePlantillaVars.map(v => (
                          <MenuItem key={v} value={v}>{v}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>

                    <IconButton
                      color="error"
                      onClick={() => setFieldMappings(fieldMappings.filter((_, i) => i !== index))}
                      size="small"
                    >
                      <Delete />
                    </IconButton>
                  </Box>
                );
              })}

              {availableMoodleFields.length > 0 && availablePlantillaVars.length > 0 && (
                <Button
                  startIcon={<Add />}
                  onClick={() => setFieldMappings([...fieldMappings, { campoMoodle: '', variablePlantilla: '' }])}
                  sx={{ mt: 1 }}
                >
                  Agregar Mapeo
                </Button>
              )}
            </Paper>

            {plantillaVariables.length > 0 && (
              <Alert severity="info" sx={{ mt: 2 }}>
                Variables de la plantilla: {plantillaVariables.join(', ')}
              </Alert>
            )}
          </Box>
        );

      // PASO: Confirmar
      case 'Confirmar':
        if (importResult) {
          return (
            <Box>
              <Alert severity={importResult.success ? 'success' : 'warning'} sx={{ mb: 3 }}>
                {importResult.message}
              </Alert>

              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h3" color="success.main">
                        {importResult.importados}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Alumnos Importados
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={6}>
                  <Card>
                    <CardContent sx={{ textAlign: 'center' }}>
                      <Typography variant="h3" color="error.main">
                        {importResult.errores}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Errores
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
                <Button variant="contained" onClick={() => navigate('/otec/cursos')}>
                  Ir a Cursos
                </Button>
                <Button
                  variant="outlined"
                  onClick={() => {
                    setActiveStep(0);
                    setSelectedCourse(null);
                    setSelectedPlantilla(null);
                    setImportResult(null);
                    setFieldMappings([]);
                    setCursoVariables([]);
                    setCursoVariablesValues({});
                    setPlantillaVariables([]);
                    setFormData({
                      nombreReferencia: '',
                      nombre_visualizar_certificado: '',
                      certificate_type: 'HORIZONTAL',
                      plantillaId: '',
                      fondoPath: '',
                      qrDestino: 1,
                      estado: 1,
                      vencimiento: '',
                      footer_1: '',
                      footer_2: '',
                      contenidoHtml: '',
                      footerHtml: '',
                    });
                  }}
                >
                  Crear Otro Curso
                </Button>
              </Box>
            </Box>
          );
        }

        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Resumen
            </Typography>
            <Divider sx={{ my: 2 }} />

            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <List dense>
                  <ListItem>
                    <ListItemIcon><Description /></ListItemIcon>
                    <ListItemText primary="Plantilla" secondary={selectedPlantilla?.nombre} />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><School /></ListItemIcon>
                    <ListItemText primary="Curso Moodle" secondary={selectedCourse?.nombre} />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Assignment /></ListItemIcon>
                    <ListItemText primary="Nombre Referencia" secondary={formData.nombreReferencia} />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon><Settings /></ListItemIcon>
                    <ListItemText
                      primary="Mapeos"
                      secondary={fieldMappings.filter(m => m.campoMoodle && m.variablePlantilla).length}
                    />
                  </ListItem>
                </List>
              </Grid>
              <Grid item xs={12} md={6}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="subtitle2">Orientacion</Typography>
                    <Chip
                      label={formData.certificate_type}
                      size="small"
                      color={formData.certificate_type === 'VERTICAL' ? 'info' : 'default'}
                    />
                    {formData.vencimiento && (
                      <>
                        <Typography variant="subtitle2" sx={{ mt: 1 }}>Vencimiento</Typography>
                        <Typography variant="body2">{formData.vencimiento}</Typography>
                      </>
                    )}
                  </CardContent>
                </Card>
              </Grid>
            </Grid>

            <Divider sx={{ my: 2 }} />

            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="subtitle1">
                <People sx={{ verticalAlign: 'middle', mr: 1 }} />
                Estudiantes a importar
              </Typography>
              <FormControl size="small" sx={{ minWidth: 180 }}>
                <InputLabel>Filtrar estudiantes</InputLabel>
                <Select
                  value={studentFilter}
                  label="Filtrar estudiantes"
                  onChange={(e) => setStudentFilter(e.target.value as 'all' | 'approved' | 'notApproved')}
                >
                  <MenuItem value="all">Todos</MenuItem>
                  <MenuItem value="approved">Aprobados</MenuItem>
                  <MenuItem value="notApproved">No aprobados</MenuItem>
                </Select>
              </FormControl>
            </Box>

            {loading ? (
              <Box display="flex" justifyContent="center" p={4}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                <TableContainer component={Paper} sx={{ maxHeight: 250 }}>
                  <Table stickyHeader size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Nombre</TableCell>
                        <TableCell>Email</TableCell>
                        <TableCell align="center">Nota</TableCell>
                        <TableCell align="center">Estado</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {displayedStudents.slice(0, 10).map((student) => (
                        <TableRow key={student.moodleUserId}>
                          <TableCell>{student.nombreCompleto}</TableCell>
                          <TableCell>{student.email}</TableCell>
                          <TableCell align="center">{student.notaFormateada || '-'}</TableCell>
                          <TableCell align="center">
                            <Chip
                              label={student.aprobado ? 'Aprobado' : 'No aprobado'}
                              color={student.aprobado ? 'success' : 'default'}
                              size="small"
                            />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
                {displayedStudents.length > 10 && (
                  <Typography variant="caption" color="text.secondary">
                    Mostrando 10 de {displayedStudents.length} estudiantes
                  </Typography>
                )}
                <Alert severity="info" sx={{ mt: 2 }}>
                  Se importaran <strong>{displayedStudents.length}</strong> estudiantes
                </Alert>
              </>
            )}
          </Box>
        );

      default:
        return null;
    }
  };

  const canProceed = () => {
    switch (currentStepName) {
      case 'Seleccionar Plantilla': return !!selectedPlantilla;
      case 'Curso Moodle': return !!selectedCourse;
      case 'Datos del Curso': return !!formData.nombreReferencia && !!formData.nombre_visualizar_certificado;
      case 'Variables del Curso': return cursoVariables.every(v => cursoVariablesValues[v]?.trim());
      case 'Mapear Campos': return fieldMappings.filter(m => m.campoMoodle && m.variablePlantilla).length > 0;
      case 'Confirmar': return !importing && displayedStudents.length > 0;
      default: return true;
    }
  };

  return (
    <Layout>
      <Box>
        <Typography variant="h4" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <School /> Crear Curso desde Moodle
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        <Paper sx={{ p: 3, mb: 3 }}>
          <Stepper activeStep={activeStep} alternativeLabel>
            {steps.map((label) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
              </Step>
            ))}
          </Stepper>
        </Paper>

        <Paper sx={{ p: 3 }}>
          {renderStepContent()}

          {!importResult && (
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 4 }}>
              <Button
                disabled={activeStep === 0}
                onClick={handleBack}
                startIcon={<ArrowBack />}
              >
                Atras
              </Button>

              {activeStep === steps.length - 1 ? (
                <Button
                  variant="contained"
                  onClick={handleImport}
                  disabled={!canProceed()}
                  startIcon={importing ? <CircularProgress size={20} /> : <CheckCircle />}
                >
                  {importing ? 'Creando...' : 'Crear Curso e Importar'}
                </Button>
              ) : (
                <Button
                  variant="contained"
                  onClick={handleNext}
                  disabled={!canProceed() || loading}
                  endIcon={<ArrowForward />}
                >
                  Siguiente
                </Button>
              )}
            </Box>
          )}
        </Paper>
      </Box>
    </Layout>
  );
}
