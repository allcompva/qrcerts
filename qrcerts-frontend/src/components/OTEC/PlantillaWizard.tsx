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
  Chip,
  Grid,
  CircularProgress,
} from '@mui/material';
import { Upload, ArrowBack, ArrowForward, Save } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { useNavigate, useParams } from 'react-router-dom';
import { apiService } from '../../services/api';
import { PlantillaCertificado } from '../../types';

const steps = ['Datos Básicos', 'Subir Documento', 'Clasificar Parámetros', 'Resumen'];

interface FormData {
  nombre: string;
  tipo: 'HORIZONTAL' | 'VERTICAL';
  docxPath: string;
}

export function PlantillaWizard() {
  const navigate = useNavigate();
  const { plantillaId } = useParams<{ plantillaId: string }>();
  const [activeStep, setActiveStep] = useState(0);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(!!plantillaId);
  const [uploading, setUploading] = useState(false);

  // Form data
  const [formData, setFormData] = useState<FormData>({
    nombre: '',
    tipo: 'HORIZONTAL',
    docxPath: '',
  });

  // Variables extraídas del Word
  const [allVariables, setAllVariables] = useState<string[]>([]);
  const [unassignedVariables, setUnassignedVariables] = useState<string[]>([]);
  const [cursoVariables, setCursoVariables] = useState<string[]>([]);
  const [alumnoVariables, setAlumnoVariables] = useState<string[]>([]);

  // Archivo seleccionado
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  // Variables obligatorias
  const variablesObligatorias = ['QR', 'NOMBRE_CURSO', 'NOMBRE'];

  // Cargar datos de la plantilla si estamos editando
  useEffect(() => {
    const loadPlantilla = async () => {
      if (!plantillaId) return;

      try {
        setLoadingData(true);
        const plantilla = await apiService.get<PlantillaCertificado>(`/api/app/plantillas-certificados/${plantillaId}`);

        // Cargar datos básicos
        setFormData({
          nombre: plantilla.nombre || '',
          tipo: (plantilla.tipo as 'HORIZONTAL' | 'VERTICAL') || 'HORIZONTAL',
          docxPath: plantilla.docxPath || plantilla.path_docx || '',
        });

        // Parsear variables
        const parseJsonArray = (jsonStr?: string): string[] => {
          if (!jsonStr) return [];
          try {
            const parsed = JSON.parse(jsonStr);
            return Array.isArray(parsed) ? parsed : [];
          } catch {
            return jsonStr.split(',').map(v => v.trim()).filter(v => v);
          }
        };

        const allVars = parseJsonArray(plantilla.variables || plantilla.contenido);
        const cursoVars = parseJsonArray(plantilla.contenido_cursos);
        const alumnoVars = parseJsonArray(plantilla.contenido_alumnos);

        setAllVariables(allVars);
        setCursoVariables(cursoVars);
        setAlumnoVariables(alumnoVars);

        // Las variables sin asignar son las que no están en curso ni en alumno (excluyendo QR y NOMBRE_CURSO)
        const asignadas = [...cursoVars, ...alumnoVars];
        const sinAsignar = allVars.filter(v =>
          !asignadas.includes(v) && !['QR', 'NOMBRE_CURSO'].includes(v.toUpperCase())
        );
        setUnassignedVariables(sinAsignar);

      } catch (err: any) {
        setError(err.message || 'Error al cargar la plantilla');
      } finally {
        setLoadingData(false);
      }
    };

    loadPlantilla();
  }, [plantillaId]);

  const handleNext = () => {
    // Validaciones por paso
    if (activeStep === 0) {
      if (!formData.nombre) {
        setError('Ingrese un nombre para la plantilla');
        return;
      }
    }
    if (activeStep === 1) {
      if (!formData.docxPath) {
        setError('Suba un archivo Word');
        return;
      }
    }
    if (activeStep === 2) {
      // Validar que todas las variables estén clasificadas
      if (unassignedVariables.length > 0) {
        setError('Debe clasificar todos los parámetros');
        return;
      }
    }

    setError('');
    setActiveStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setError('');
    setActiveStep((prev) => prev - 1);
  };

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.name.endsWith('.docx')) {
      setError('Solo se permiten archivos .docx');
      return;
    }

    setSelectedFile(file);
    setError('');
    setUploading(true);

    try {
      const response = await apiService.uploadFile('/api/app/upload/docx', file) as {
        docxPath: string;
        variables: string[]
      };

      // Validar variables obligatorias
      const variablesEncontradas = response.variables.map((v: string) => v.toUpperCase());
      const faltantes = variablesObligatorias.filter(v => !variablesEncontradas.includes(v));

      if (faltantes.length > 0) {
        setError(`La plantilla debe contener las variables obligatorias: ${faltantes.map(v => `{{${v}}}`).join(', ')}`);
        setSelectedFile(null);
        return;
      }

      setFormData(prev => ({ ...prev, docxPath: response.docxPath }));
      setAllVariables(response.variables);

      // Excluir QR y NOMBRE_CURSO de la clasificación (son automáticos)
      const variablesParaClasificar = response.variables.filter((v: string) =>
        !['QR', '.'].includes(v.toUpperCase())
      );
      setUnassignedVariables(variablesParaClasificar);
      setCursoVariables([]);
      setAlumnoVariables([]);
    } catch (err: any) {
      setError(err.message || 'Error al subir archivo');
      setSelectedFile(null);
    } finally {
      setUploading(false);
    }
  };

  // Drag and Drop handlers
  const handleDragStart = (e: React.DragEvent, variable: string, source: string) => {
    e.dataTransfer.setData('variable', variable);
    e.dataTransfer.setData('source', source);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDropToCurso = (e: React.DragEvent) => {
    e.preventDefault();
    const variable = e.dataTransfer.getData('variable');
    const source = e.dataTransfer.getData('source');

    if (source === 'unassigned') {
      setUnassignedVariables(prev => prev.filter(v => v !== variable));
    } else if (source === 'alumno') {
      setAlumnoVariables(prev => prev.filter(v => v !== variable));
    }

    if (!cursoVariables.includes(variable)) {
      setCursoVariables(prev => [...prev, variable]);
    }
  };

  const handleDropToAlumno = (e: React.DragEvent) => {
    e.preventDefault();
    const variable = e.dataTransfer.getData('variable');
    const source = e.dataTransfer.getData('source');

    if (source === 'unassigned') {
      setUnassignedVariables(prev => prev.filter(v => v !== variable));
    } else if (source === 'curso') {
      setCursoVariables(prev => prev.filter(v => v !== variable));
    }

    if (!alumnoVariables.includes(variable)) {
      setAlumnoVariables(prev => [...prev, variable]);
    }
  };

  const handleDropToUnassigned = (e: React.DragEvent) => {
    e.preventDefault();
    const variable = e.dataTransfer.getData('variable');
    const source = e.dataTransfer.getData('source');

    if (source === 'curso') {
      setCursoVariables(prev => prev.filter(v => v !== variable));
    } else if (source === 'alumno') {
      setAlumnoVariables(prev => prev.filter(v => v !== variable));
    }

    if (!unassignedVariables.includes(variable)) {
      setUnassignedVariables(prev => [...prev, variable]);
    }
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);
      setError('');

      const payload = {
        nombre: formData.nombre,
        tipo: formData.tipo,
        docxPath: formData.docxPath,
        variables: JSON.stringify(allVariables),
        contenido_cursos: JSON.stringify(cursoVariables),
        contenido_alumnos: JSON.stringify(alumnoVariables),
      };

      if (plantillaId) {
        await apiService.put(`/api/app/plantillas-certificados/${plantillaId}`, payload);
      } else {
        await apiService.post('/api/app/plantillas-certificados', payload);
      }

      navigate('/otec/plantillas');
    } catch (err: any) {
      setError(err.message || 'Error al guardar la plantilla');
    } finally {
      setLoading(false);
    }
  };

  const renderStep = () => {
    switch (activeStep) {
      case 0:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Datos Básicos de la Plantilla
            </Typography>
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Nombre de la Plantilla"
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                  required
                  helperText="Nombre descriptivo para identificar la plantilla"
                />
              </Grid>
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>Tipo de Certificado</InputLabel>
                  <Select
                    value={formData.tipo}
                    label="Tipo de Certificado"
                    onChange={(e) => setFormData({ ...formData, tipo: e.target.value as 'HORIZONTAL' | 'VERTICAL' })}
                  >
                    <MenuItem value="HORIZONTAL">Horizontal (A4 apaisado)</MenuItem>
                    <MenuItem value="VERTICAL">Vertical (A4 retrato)</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Box>
        );

      case 1:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Subir Documento Word
            </Typography>
            <Alert severity="info" sx={{ mb: 3 }}>
              El documento debe contener las variables obligatorias: {variablesObligatorias.map(v => `{{${v}}}`).join(', ')}
            </Alert>
            <Card variant="outlined">
              <CardContent>
                <Button
                  variant="outlined"
                  component="label"
                  startIcon={uploading ? <CircularProgress size={20} /> : <Upload />}
                  disabled={uploading}
                  fullWidth
                  sx={{ mb: 2 }}
                >
                  {selectedFile ? selectedFile.name : 'Seleccionar archivo .docx'}
                  <input
                    type="file"
                    hidden
                    accept=".docx"
                    onChange={handleFileUpload}
                  />
                </Button>
                {formData.docxPath && (
                  <>
                    <Alert severity="success" sx={{ mb: 2 }}>
                      Archivo cargado correctamente
                    </Alert>
                    <Typography variant="subtitle2" gutterBottom>
                      Variables detectadas:
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                      {allVariables.map((variable, idx) => (
                        <Chip
                          key={idx}
                          label={`{{${variable}}}`}
                          color={variablesObligatorias.includes(variable.toUpperCase()) ? 'primary' : 'default'}
                          variant="outlined"
                        />
                      ))}
                    </Box>
                  </>
                )}
              </CardContent>
            </Card>
          </Box>
        );

      case 2:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Clasificar Parámetros
            </Typography>
            <Alert severity="info" sx={{ mb: 3 }}>
              Arrastre cada parámetro al grupo correspondiente. Los parámetros de <strong>Curso</strong> se llenan una vez por curso.
              Los parámetros de <strong>Alumno</strong> vienen del Excel de importación.
            </Alert>

            {/* Variables sin asignar */}
            {unassignedVariables.length > 0 && (
              <Card
                variant="outlined"
                sx={{ mb: 3, bgcolor: '#fff3e0', minHeight: 80 }}
                onDragOver={handleDragOver}
                onDrop={handleDropToUnassigned}
              >
                <CardContent>
                  <Typography variant="subtitle2" color="warning.main" gutterBottom>
                    Sin clasificar ({unassignedVariables.length})
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {unassignedVariables.map((variable, idx) => (
                      <Chip
                        key={idx}
                        label={`{{${variable}}}`}
                        draggable
                        onDragStart={(e) => handleDragStart(e, variable, 'unassigned')}
                        sx={{ cursor: 'grab', '&:active': { cursor: 'grabbing' } }}
                      />
                    ))}
                  </Box>
                </CardContent>
              </Card>
            )}

            <Grid container spacing={3}>
              {/* Parámetros de Curso */}
              <Grid item xs={12} md={6}>
                <Card
                  variant="outlined"
                  sx={{
                    minHeight: 200,
                    bgcolor: '#e3f2fd',
                    border: '2px dashed #1976d2',
                  }}
                  onDragOver={handleDragOver}
                  onDrop={handleDropToCurso}
                >
                  <CardContent>
                    <Typography variant="subtitle1" color="primary" gutterBottom>
                      Parámetros de Curso
                    </Typography>
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
                      Se configuran una vez al crear el curso
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', minHeight: 60 }}>
                      {cursoVariables.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                          Arrastre parámetros aquí
                        </Typography>
                      ) : (
                        cursoVariables.map((variable, idx) => (
                          <Chip
                            key={idx}
                            label={`{{${variable}}}`}
                            color="primary"
                            draggable
                            onDragStart={(e) => handleDragStart(e, variable, 'curso')}
                            sx={{ cursor: 'grab', '&:active': { cursor: 'grabbing' } }}
                          />
                        ))
                      )}
                    </Box>
                  </CardContent>
                </Card>
              </Grid>

              {/* Parámetros de Alumno */}
              <Grid item xs={12} md={6}>
                <Card
                  variant="outlined"
                  sx={{
                    minHeight: 200,
                    bgcolor: '#e8f5e9',
                    border: '2px dashed #2e7d32',
                  }}
                  onDragOver={handleDragOver}
                  onDrop={handleDropToAlumno}
                >
                  <CardContent>
                    <Typography variant="subtitle1" color="success.main" gutterBottom>
                      Parámetros de Alumno
                    </Typography>
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
                      Vienen del Excel de importación de alumnos
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', minHeight: 60 }}>
                      {alumnoVariables.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                          Arrastre parámetros aquí
                        </Typography>
                      ) : (
                        alumnoVariables.map((variable, idx) => (
                          <Chip
                            key={idx}
                            label={`{{${variable}}}`}
                            color="success"
                            draggable
                            onDragStart={(e) => handleDragStart(e, variable, 'alumno')}
                            sx={{ cursor: 'grab', '&:active': { cursor: 'grabbing' } }}
                          />
                        ))
                      )}
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Box>
        );

      case 3:
        return (
          <Box>
            <Typography variant="h6" gutterBottom>
              Resumen de la Plantilla
            </Typography>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="subtitle2" color="text.secondary">
                  Nombre
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {formData.nombre}
                </Typography>

                <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                  Tipo
                </Typography>
                <Chip label={formData.tipo} size="small" />

                <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                  Archivo
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {selectedFile?.name || formData.docxPath}
                </Typography>

                <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 2 }}>
                  Parámetros de Curso ({cursoVariables.length})
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                  {cursoVariables.map((v, idx) => (
                    <Chip key={idx} label={`{{${v}}}`} color="primary" size="small" />
                  ))}
                </Box>

                <Typography variant="subtitle2" color="text.secondary">
                  Parámetros de Alumno ({alumnoVariables.length})
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  {alumnoVariables.map((v, idx) => (
                    <Chip key={idx} label={`{{${v}}}`} color="success" size="small" />
                  ))}
                </Box>
              </CardContent>
            </Card>
          </Box>
        );

      default:
        return null;
    }
  };

  if (loadingData) {
    return (
      <Layout>
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
          <CircularProgress />
        </Box>
      </Layout>
    );
  }

  return (
    <Layout>
      <Box sx={{ maxWidth: 900, mx: 'auto', py: 3 }}>
        <Typography variant="h4" gutterBottom>
          {plantillaId ? 'Editar Plantilla' : 'Nueva Plantilla'}
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
            onClick={() => navigate('/otec/plantillas')}
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
                {loading ? 'Guardando...' : 'Guardar Plantilla'}
              </Button>
            )}
          </Box>
        </Box>
      </Box>
    </Layout>
  );
}
