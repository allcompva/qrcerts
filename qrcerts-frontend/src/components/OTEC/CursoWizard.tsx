import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Stepper,
  Step,
  StepLabel,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Alert,
  Card,
  CardContent,
  Grid,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import { Upload, ArrowBack, ArrowForward, Save, Download } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { apiService } from '../../services/api';
import { API_BASE_URL } from '../../config/api';
import { PlantillaCertificado } from '../../types';
import * as XLSX from 'xlsx';
import RichTextLite from '../RichTextLite';

interface FormData {
  nombreReferencia: string;
  nombre_visualizar_certificado: string;
  certificate_type: 'HORIZONTAL' | 'VERTICAL';
  plantillaId: string;
  fondoPath: string;
  qrDestino: number;
  estado: number;
  vencimiento: string;
  // Campos para HORIZONTAL
  footer_1: string;
  footer_2: string;
  // Campos para VERTICAL
  contenidoHtml: string;
  footerHtml: string;
}

interface ExcelRow {
  [key: string]: any;
}

// Steps dinámicos según plantilla y tipo de certificado
// Alumnos se gestionan exclusivamente desde la pantalla de alumnos del curso
const getSteps = (plantillaId: string, certificateType: 'HORIZONTAL' | 'VERTICAL', hasCursoVariables: boolean) => {
  if (plantillaId) {
    if (hasCursoVariables) {
      return ['Datos Básicos', 'Seleccionar Plantilla', 'Variables del Curso', 'Resumen'];
    }
    return ['Datos Básicos', 'Seleccionar Plantilla', 'Resumen'];
  } else if (certificateType === 'HORIZONTAL') {
    return ['Datos Básicos', 'Seleccionar Plantilla', 'Imagen de Fondo', 'Pie de Página', 'Resumen'];
  } else {
    return ['Datos Básicos', 'Seleccionar Plantilla', 'Imagen de Fondo', 'Contenido HTML', 'Resumen'];
  }
};

export function CursoWizard() {
  const navigate = useNavigate();
  const { cursoId } = useParams<{ cursoId: string }>();
  const { user } = useAuth();
  const [activeStep, setActiveStep] = useState(0);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // Form data
  const [formData, setFormData] = useState<FormData>({
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

  // Plantillas disponibles
  const [plantillas, setPlantillas] = useState<PlantillaCertificado[]>([]);
  const [selectedPlantilla, setSelectedPlantilla] = useState<PlantillaCertificado | null>(null);

  // Excel data
  const [excelData, setExcelData] = useState<ExcelRow[]>([]);
  const [excelColumns, setExcelColumns] = useState<string[]>([]);

  // Variables de curso (desde contenido_cursos de la plantilla)
  const [cursoVariables, setCursoVariables] = useState<string[]>([]);
  const [cursoVariablesValues, setCursoVariablesValues] = useState<Record<string, string>>({});

  // Image upload
  const [uploadingImage, setUploadingImage] = useState(false);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);

  // Load plantillas on mount
  useEffect(() => {
    loadPlantillas();
  }, []);

  // Load curso data when editing
  useEffect(() => {
    if (cursoId) {
      loadCurso();
    }
  }, [cursoId]);

  const loadCurso = async () => {
    try {
      setLoading(true);
      const curso = await apiService.get<any>(`/api/app/cursos/${cursoId}`);
      console.log('Curso cargado:', curso);
      // Detectar plantillaId (puede venir como plantillaId o PlantillaId)
      const cursoPlantillaId = curso.plantillaId || curso.PlantillaId || '';
      console.log('PlantillaId del curso:', cursoPlantillaId);
      setFormData({
        nombreReferencia: curso.nombreReferencia || '',
        nombre_visualizar_certificado: curso.nombre_visualizar_certificado || '',
        certificate_type: curso.certificate_type || 'HORIZONTAL',
        plantillaId: cursoPlantillaId,
        fondoPath: curso.fondoPath || '',
        qrDestino: curso.qrDestino ?? 1,
        estado: curso.estado ?? 1,
        vencimiento: curso.vencimiento ? curso.vencimiento.split('T')[0] : '',
        footer_1: curso.footer_1 || '',
        footer_2: curso.footer_2 || '',
        contenidoHtml: curso.contenidoHtml || '',
        footerHtml: curso.footerHtml || '',
      });

      // Cargar variables de curso desde layoutJson
      if (curso.layoutJson) {
        try {
          const layoutData = JSON.parse(curso.layoutJson);
          setCursoVariablesValues(layoutData);
        } catch {
          console.warn('Error parsing layoutJson');
        }
      }

      // Si tiene imagen de fondo, mostrar preview
      if (curso.fondoPath) {
        setImagePreviewUrl(`${API_BASE_URL}/uploads/images/${curso.fondoPath}`);
      }
    } catch (err: any) {
      setError(err.message || 'Error al cargar el curso');
    } finally {
      setLoading(false);
    }
  };

  // Load plantilla when selected
  useEffect(() => {
    if (formData.plantillaId && plantillas.length > 0) {
      const plantilla = plantillas.find(p => String(p.id) === String(formData.plantillaId));
      if (plantilla) {
        setSelectedPlantilla(plantilla);

        // Cargar variables de curso (contenido_cursos)
        let varsDelCurso: string[] = [];
        if (plantilla.contenido_cursos) {
          try {
            varsDelCurso = JSON.parse(plantilla.contenido_cursos);
          } catch {
            varsDelCurso = plantilla.contenido_cursos.split(',').map(v => v.trim()).filter(v => v);
          }
        }
        setCursoVariables(varsDelCurso);

        // Si no hay valores previos, inicializar vacíos
        if (varsDelCurso.length > 0 && Object.keys(cursoVariablesValues).length === 0) {
          const initialValues: Record<string, string> = {};
          varsDelCurso.forEach(v => { initialValues[v] = ''; });
          setCursoVariablesValues(initialValues);
        }
      } else {
        // La plantilla seleccionada no existe, resetear
        console.warn('Plantilla no encontrada, reseteando:', formData.plantillaId);
        setFormData(prev => ({ ...prev, plantillaId: '' }));
        setSelectedPlantilla(null);
        setCursoVariables([]);
        setCursoVariablesValues({});
      }
    } else {
      setSelectedPlantilla(null);
      setCursoVariables([]);
      setCursoVariablesValues({});
    }
  }, [formData.plantillaId, plantillas]);

  const loadPlantillas = async () => {
    try {
      const data = await apiService.get<PlantillaCertificado[]>('/api/app/plantillas-certificados');
      console.log('Plantillas cargadas:', data);
      // Asegurar que data es un array
      setPlantillas(Array.isArray(data) ? data : []);
    } catch (err: any) {
      console.error('Error al cargar plantillas:', err);
      setError(err.message || 'Error al cargar plantillas');
    }
  };

  // Obtener los steps actuales basados en plantilla y tipo
  const steps = getSteps(formData.plantillaId, formData.certificate_type, cursoVariables.length > 0);

  const handleNext = () => {
    const currentStep = steps[activeStep];

    // Validaciones por paso
    if (currentStep === 'Datos Básicos') {
      if (!formData.nombreReferencia || !formData.nombre_visualizar_certificado) {
        setError('Complete todos los campos obligatorios');
        return;
      }
    }
    // Plantilla es opcional - no validamos
    if (currentStep === 'Imagen de Fondo') {
      if (!formData.fondoPath) {
        setError('Suba una imagen de fondo');
        return;
      }
    }
    if (currentStep === 'Pie de Página') {
      if (!formData.footer_1 || !formData.footer_2) {
        setError('Complete ambas líneas de pie de página');
        return;
      }
    }
    // Contenido Extra es opcional, no validamos
    // Cargar Alumnos ya no es paso del wizard - se gestiona en la pantalla de alumnos

    setError('');
    setActiveStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setError('');
    setActiveStep((prev) => prev - 1);
  };

  const handleImageUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setUploadingImage(true);
    setError('');

    const previewUrl = URL.createObjectURL(file);
    setImagePreviewUrl(previewUrl);

    try {
      const response = await apiService.uploadFile('/api/app/upload/image', file);
      setFormData(prev => ({ ...prev, fondoPath: response.fileName }));
    } catch (err: any) {
      setError(err.message || 'Error al subir la imagen');
      URL.revokeObjectURL(previewUrl);
      setImagePreviewUrl(null);
    } finally {
      setUploadingImage(false);
    }
  };

  const handleDownloadExcelTemplate = () => {
    if (!selectedPlantilla) {
      console.warn('No hay plantilla seleccionada');
      return;
    }

    try {
      console.log('Plantilla seleccionada para Excel:', selectedPlantilla);

      // Primero intentar usar contenido_alumnos (variables clasificadas para alumnos)
      // Si no existe, usar variables/contenido y filtrar las especiales
      let variablesParaExcel: string[] = [];

      if (selectedPlantilla.contenido_alumnos) {
        try {
          variablesParaExcel = JSON.parse(selectedPlantilla.contenido_alumnos);
        } catch {
          variablesParaExcel = selectedPlantilla.contenido_alumnos.split(',').map((v: string) => v.trim()).filter((v: string) => v);
        }
        console.log('Variables de contenido_alumnos:', variablesParaExcel);
      } else {
        // Fallback: usar todas las variables y filtrar las especiales
        const contenido = selectedPlantilla.contenido || selectedPlantilla.variables || '';
        let variables: string[];
        try {
          variables = JSON.parse(contenido);
        } catch {
          variables = contenido.split(',').map((v: string) => v.trim()).filter((v: string) => v);
        }

        // Excluir variables especiales que no vienen del Excel (se obtienen de otros lados)
        const variablesEspeciales = ['QR', 'NOMBRE_CURSO'];
        variablesParaExcel = variables.filter((v: string) =>
          !variablesEspeciales.includes(v.toUpperCase())
        );
        console.log('Variables desde fallback (contenido/variables):', variablesParaExcel);
      }

      if (variablesParaExcel.length === 0) {
        setError('La plantilla no tiene variables de alumno definidas');
        return;
      }

      // Crear un objeto de ejemplo con las columnas
      const exampleRow: any = {};
      variablesParaExcel.forEach((variable: string) => {
        // Poner ejemplos para Nombre y RUT
        if (variable.toUpperCase() === 'NOMBRE') {
          exampleRow[variable] = 'Juan Pérez';
        } else if (variable.toUpperCase() === 'RUT') {
          exampleRow[variable] = '12345678-9';
        } else {
          exampleRow[variable] = '';
        }
      });

      console.log('Fila de ejemplo para Excel:', exampleRow);

      // Crear workbook y worksheet
      const wb = XLSX.utils.book_new();
      const ws = XLSX.utils.json_to_sheet([exampleRow]);

      // Agregar el worksheet al workbook
      XLSX.utils.book_append_sheet(wb, ws, 'Alumnos');

      // Descargar el archivo
      XLSX.writeFile(wb, `Plantilla_${selectedPlantilla.nombre.replace(/\s+/g, '_')}.xlsx`);
    } catch (err) {
      console.error('Error al generar modelo Excel:', err);
      setError('Error al generar el modelo Excel');
    }
  };

  const handleExcelUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    try {
      const data = await file.arrayBuffer();
      const workbook = XLSX.read(data);
      const sheetName = workbook.SheetNames[0];
      const worksheet = workbook.Sheets[sheetName];
      const jsonData: ExcelRow[] = XLSX.utils.sheet_to_json(worksheet);

      if (jsonData.length === 0) {
        setError('El archivo Excel está vacío');
        return;
      }

      // Validar que las columnas coincidan con las variables de la plantilla
      // (excluyendo las variables especiales que no vienen del Excel)
      if (selectedPlantilla) {
        // contenido puede ser JSON array o string separado por comas
        let allVariables: string[];
        const contenido = selectedPlantilla.contenido || selectedPlantilla.variables || '';
        try {
          allVariables = JSON.parse(contenido);
        } catch {
          allVariables = contenido.split(',').map(v => v.trim()).filter(v => v);
        }
        const variablesEspeciales = ['QR', 'NOMBRE_CURSO'];
        const expectedColumns = allVariables.filter((col: string) =>
          !variablesEspeciales.includes(col.toUpperCase())
        );
        const excelColumns = Object.keys(jsonData[0]);

        const missingColumns = expectedColumns.filter((col: string) => !excelColumns.includes(col));
        if (missingColumns.length > 0) {
          setError(`Faltan columnas en el Excel: ${missingColumns.join(', ')}`);
          return;
        }
      }

      setExcelData(jsonData);
      setExcelColumns(Object.keys(jsonData[0]));
      setError('');
    } catch (err: any) {
      setError('Error al procesar el archivo Excel');
    }
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);
      setError('');

      // Si hay variables de curso, guardarlas en layoutJson
      const layoutJson = cursoVariables.length > 0
        ? JSON.stringify(cursoVariablesValues)
        : '';

      const payload = {
        ...formData,
        plantillaId: formData.plantillaId || '', // Asegurar cadena vacía, nunca null
        layoutJson: layoutJson,
        otecId: (user as any)?.otecId,
      };

      let nuevoCursoId = cursoId;

      if (cursoId) {
        await apiService.put(`/api/app/cursos/${cursoId}`, payload);
      } else {
        const response = await apiService.post<{ id: string }>('/api/app/cursos', payload);
        nuevoCursoId = response.id;
      }

      navigate('/otec/cursos');
    } catch (err: any) {
      setError(err.message || 'Error al guardar el curso');
    } finally {
      setLoading(false);
    }
  };

  const currentStepName = steps[activeStep];

  const renderStep = () => {
    // Usar el nombre del paso para determinar qué renderizar
    switch (currentStepName) {
      case 'Datos Básicos':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Datos Básicos del Curso
            </Typography>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Nombre de Referencia"
                  value={formData.nombreReferencia}
                  onChange={(e) => setFormData({ ...formData, nombreReferencia: e.target.value })}
                  required
                  helperText="Nombre interno para identificar el curso"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Nombre a Visualizar en Certificado"
                  value={formData.nombre_visualizar_certificado}
                  onChange={(e) => setFormData({ ...formData, nombre_visualizar_certificado: e.target.value })}
                  required
                  helperText="Nombre que aparecerá en el certificado"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Tipo de Certificado</InputLabel>
                  <Select
                    value={formData.certificate_type}
                    label="Tipo de Certificado"
                    onChange={(e) => setFormData({ ...formData, certificate_type: e.target.value as 'HORIZONTAL' | 'VERTICAL' })}
                  >
                    <MenuItem value="HORIZONTAL">Horizontal (A4 apaisado)</MenuItem>
                    <MenuItem value="VERTICAL">Vertical (A4 retrato)</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="date"
                  label="Fecha de Vencimiento"
                  value={formData.vencimiento}
                  onChange={(e) => setFormData({ ...formData, vencimiento: e.target.value })}
                  InputLabelProps={{ shrink: true }}
                  helperText="Opcional: fecha hasta la cual el certificado es válido"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Destino del QR</InputLabel>
                  <Select
                    value={formData.qrDestino}
                    label="Destino del QR"
                    onChange={(e) => setFormData({ ...formData, qrDestino: Number(e.target.value) })}
                  >
                    <MenuItem value={1}>PDF</MenuItem>
                    <MenuItem value={2}>Página de verificación</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Estado</InputLabel>
                  <Select
                    value={formData.estado}
                    label="Estado"
                    onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })}
                  >
                    <MenuItem value={1}>Activo</MenuItem>
                    <MenuItem value={0}>Inactivo</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Box>
        );

      case 'Seleccionar Plantilla':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Seleccionar Plantilla de Certificado (Opcional)
            </Typography>
            <Alert severity="info" sx={{ mb: 2 }}>
              Si no selecciona una plantilla, podrá configurar el certificado manualmente con pie de página y contenido extra.
            </Alert>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>Plantilla</InputLabel>
                  <Select
                    value={formData.plantillaId}
                    label="Plantilla"
                    onChange={(e) => setFormData({ ...formData, plantillaId: e.target.value })}
                  >
                    <MenuItem value="">
                      <em>No utilizar plantilla (flujo anterior)</em>
                    </MenuItem>
                    {plantillas
                      .filter(p => {
                        // Siempre incluir la plantilla actualmente seleccionada
                        if (formData.plantillaId && String(p.id) === String(formData.plantillaId)) {
                          return true;
                        }
                        // Filtrar por tipo si existe, sino mostrar todas
                        if (!p.tipo) return true;
                        return p.tipo.toUpperCase() === formData.certificate_type;
                      })
                      .map((plantilla) => (
                        <MenuItem key={plantilla.id} value={plantilla.id}>
                          {plantilla.nombre} {plantilla.tipo ? `(${plantilla.tipo})` : ''}
                        </MenuItem>
                      ))}
                  </Select>
                </FormControl>
              </Grid>

              {selectedPlantilla && (
                <Grid item xs={12}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="subtitle1" gutterBottom>
                        Variables de la Plantilla:
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                        {(() => {
                          let vars: string[];
                          const contenido = selectedPlantilla.contenido || selectedPlantilla.variables || '';
                          try {
                            vars = JSON.parse(contenido);
                          } catch {
                            vars = contenido.split(',').map(v => v.trim()).filter(v => v);
                          }
                          return vars.map((variable: string, idx: number) => (
                            <Chip key={idx} label={variable} color="primary" variant="outlined" />
                          ));
                        })()}
                      </Box>
                      <Button
                        variant="contained"
                        startIcon={<Download />}
                        onClick={handleDownloadExcelTemplate}
                        fullWidth
                      >
                        Descargar Modelo Excel
                      </Button>
                      <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                        Descargue este modelo, complete los datos y súbalo en el siguiente paso
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              )}
            </Grid>
          </Box>
        );

      case 'Imagen de Fondo':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Imagen de Fondo del Certificado
            </Typography>
            <Card variant="outlined">
              <CardContent>
                <Button
                  variant="outlined"
                  component="label"
                  startIcon={<Upload />}
                  disabled={uploadingImage}
                  fullWidth
                  sx={{ mb: 2 }}
                >
                  {uploadingImage ? 'Subiendo...' : 'Seleccionar Imagen'}
                  <input type="file" hidden accept="image/*" onChange={handleImageUpload} />
                </Button>

                {imagePreviewUrl && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                      Imagen cargada correctamente
                    </Typography>
                    <Box
                      component="img"
                      src={imagePreviewUrl}
                      alt="Vista previa"
                      sx={{
                        width: '100%',
                        maxHeight: 400,
                        aspectRatio: formData.certificate_type === 'VERTICAL' ? '210/297' : '297/210',
                        objectFit: 'contain',
                        border: '2px solid #e0e0e0',
                        borderRadius: 2,
                        backgroundColor: '#f5f5f5',
                      }}
                    />
                  </Box>
                )}

                {formData.fondoPath && !imagePreviewUrl && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                      Imagen actual: {formData.fondoPath}
                    </Typography>
                    <Box
                      component="img"
                      src={`${API_BASE_URL}/uploads/images/${formData.fondoPath}`}
                      alt="Fondo actual"
                      sx={{
                        width: '100%',
                        maxHeight: 400,
                        aspectRatio: formData.certificate_type === 'VERTICAL' ? '210/297' : '297/210',
                        objectFit: 'contain',
                        border: '2px solid #4caf50',
                        borderRadius: 2,
                        backgroundColor: '#f5f5f5',
                      }}
                    />
                  </Box>
                )}
              </CardContent>
            </Card>
          </Box>
        );

      case 'Pie de Página':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Pie de Página del Certificado
            </Typography>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Línea de Pie 1"
                  value={formData.footer_1}
                  onChange={(e) => setFormData({ ...formData, footer_1: e.target.value })}
                  required
                  helperText="Primera línea del pie del certificado"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Línea de Pie 2"
                  value={formData.footer_2}
                  onChange={(e) => setFormData({ ...formData, footer_2: e.target.value })}
                  required
                  helperText="Segunda línea del pie del certificado"
                />
              </Grid>
            </Grid>
          </Box>
        );

      case 'Contenido HTML':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Contenido HTML (Certificado Vertical)
            </Typography>
            <Alert severity="info" sx={{ mb: 2 }}>
              Configure el contenido HTML y footer para el certificado vertical.
            </Alert>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Contenido HTML</Typography>
                <RichTextLite
                  value={formData.contenidoHtml}
                  onChange={(html) => setFormData({ ...formData, contenidoHtml: html })}
                  minHeight={200}
                />
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>Footer HTML</Typography>
                <RichTextLite
                  value={formData.footerHtml}
                  onChange={(html) => setFormData({ ...formData, footerHtml: html })}
                  minHeight={150}
                />
              </Grid>
            </Grid>
          </Box>
        );

      case 'Variables del Curso':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Variables del Curso
            </Typography>
            <Alert severity="info" sx={{ mb: 3 }}>
              Complete los valores de las variables que se aplicarán a todos los certificados de este curso.
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

      case 'Resumen':
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Resumen del Curso
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary">
                      Nombre de Referencia
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.nombreReferencia}
                    </Typography>

                    <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                      Nombre en Certificado
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {formData.nombre_visualizar_certificado}
                    </Typography>

                    <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                      Tipo
                    </Typography>
                    <Chip
                      label={formData.certificate_type}
                      color={formData.certificate_type === 'VERTICAL' ? 'info' : 'default'}
                      size="small"
                    />

                    {formData.plantillaId ? (
                      <>
                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Plantilla
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {selectedPlantilla?.nombre || 'No seleccionada'}
                        </Typography>

                        <Alert severity="info" sx={{ mt: 2 }}>
                          Una vez creado el curso, podrá cargar los alumnos desde la sección
                          <strong> Alumnos</strong> del curso (importar Excel, carga manual o Moodle).
                        </Alert>
                      </>
                    ) : formData.certificate_type === 'HORIZONTAL' ? (
                      <>
                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Pie de Página 1
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {formData.footer_1 || 'No configurado'}
                        </Typography>

                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Pie de Página 2
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {formData.footer_2 || 'No configurado'}
                        </Typography>
                      </>
                    ) : (
                      <>
                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Contenido HTML
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {formData.contenidoHtml ? 'Configurado' : 'No configurado'}
                        </Typography>

                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Footer HTML
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {formData.footerHtml ? 'Configurado' : 'No configurado'}
                        </Typography>
                      </>
                    )}

                    {formData.vencimiento && (
                      <>
                        <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                          Vencimiento
                        </Typography>
                        <Typography variant="body1" gutterBottom>
                          {new Date(formData.vencimiento).toLocaleDateString()}
                        </Typography>
                      </>
                    )}
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <Layout>
      <Box sx={{ maxWidth: 900, mx: 'auto', py: 3 }}>
        <Typography variant="h4" gutterBottom>
          {cursoId ? 'Editar Curso' : 'Nuevo Curso'}
        </Typography>

        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Paper sx={{ p: 3, mb: 3 }}>
          {renderStep()}
        </Paper>

        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
          <Button
            onClick={() => navigate('/otec/cursos')}
            startIcon={<ArrowBack />}
          >
            Cancelar
          </Button>

          <Box sx={{ display: 'flex', gap: 1 }}>
            {activeStep > 0 && (
              <Button onClick={handleBack} startIcon={<ArrowBack />}>
                Anterior
              </Button>
            )}

            {activeStep < steps.length - 1 ? (
              <Button variant="contained" onClick={handleNext} endIcon={<ArrowForward />}>
                Siguiente
              </Button>
            ) : (
              <Button
                variant="contained"
                onClick={handleSubmit}
                startIcon={<Save />}
                disabled={loading}
              >
                {loading ? 'Guardando...' : 'Guardar Curso'}
              </Button>
            )}
          </Box>
        </Box>
      </Box>
    </Layout>
  );
}
