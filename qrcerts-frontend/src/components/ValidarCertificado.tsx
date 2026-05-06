import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Alert,
  CircularProgress,
  Chip,
  Divider,
  Container,
  Button,
} from '@mui/material';
import {
  CheckCircle,
  Error,
  School,
  Person,
  DateRange,
  Download,
} from '@mui/icons-material';
import { useSearchParams } from 'react-router-dom';
import { apiService } from '../services/api';
import { useCertificatePDF } from '../hooks/useCertificatePDF';

interface ValidacionResponse {
  esValido: boolean;
  alumno?: {
    id: string;
    nombreApellido: string;
    rut: string;
    createdAt: string;
    calificacion?: string;
    observaciones?: string;
    certificado_otorgado?: string;
    motivo_entrega?: string;
  };
  curso?: {
    id: string;
    nombreReferencia: string;
    baseUrlPublica: string;
    fondoPath?: string;
    createdAt: string;
    certificate_type?: string;
    nombre_visualizar_certificado?: string;
    footer_1?: string;
    footer_2?: string;
    contenidoHtml?: string;
    footerHtml?: string;
    layoutJson?: string;
    vencimiento?: string;
    otec?: {
      id: string;
      nombre: string;
      slug: string;
    };
  };
  mensaje?: string;
  pdfFilename?: string;
  vencimiento?: string;
}

export function ValidarCertificado() {
  const [searchParams] = useSearchParams();
  const { downloadCertificate, loading: pdfLoading, error: pdfError } = useCertificatePDF();
  const [loading, setLoading] = useState(true);
  const [validacion, setValidacion] = useState<ValidacionResponse | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    const data = searchParams.get('data');
    if (data) {
      validarCertificado(data);
    } else {
      setError('No se proporcionaron datos para validar');
      setLoading(false);
    }
  }, [searchParams]);

  const validarCertificado = async (data: string) => {
    try {
      setLoading(true);
      setError('');

      // Decodificar base64 para obtener alumnoId,cursoId
      const decodedData = atob(data);
      const [alumnoId, cursoId] = decodedData.split(',');

      if (!alumnoId || !cursoId) {
        throw new (Error as any)('Datos de certificado inválidos');
      }

      const response = await apiService.get<ValidacionResponse>(`/api/public/validar-certificado?alumnoId=${alumnoId}&cursoId=${cursoId}`);

      setValidacion(response);
    } catch (error: any) {
      setError(error.message || 'Error al validar el certificado');
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadCertificate = async () => {
    if (!validacion?.alumno?.id || !validacion?.curso?.id) return;

    try {
      await downloadCertificate(validacion.alumno.id, validacion.curso.id);
    } catch (error) {
      console.error('Error al descargar certificado:', error);
      setError('Error al descargar el certificado');
    }
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
          <Box textAlign="center">
            <CircularProgress size={60} />
            <Typography variant="h6" sx={{ mt: 2 }}>
              Validando certificado...
            </Typography>
          </Box>
        </Box>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Error sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
            <Typography variant="h4" gutterBottom color="error">
              Error de Validación
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {error}
            </Typography>
          </CardContent>
        </Card>
      </Container>
    );
  }

  if (!validacion) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Error sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
            <Typography variant="h4" gutterBottom color="error">
              Certificado No Válido
            </Typography>
            <Typography variant="body1" color="text.secondary">
              No se pudo validar el certificado
            </Typography>
          </CardContent>
        </Card>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Card>
        <CardContent sx={{ p: 4 }}>
          {/* Nombre de la OTEC en lugar destacado */}
          {validacion?.curso?.otec && (
            <Box textAlign="center" mb={3}>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                {validacion.curso.otec.nombre}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Organismo Técnico de Capacitación
              </Typography>
            </Box>
          )}

          {/* Header de validación */}
          <Box textAlign="center" mb={4}>
            {validacion.esValido ? (
              <>
                <CheckCircle sx={{ fontSize: 80, color: 'darkcyan', mb: 2 }} />
                <Typography variant="h3" gutterBottom sx={{ color: 'darkcyan' }}>
                  Certificado Verificado
                </Typography>

                {/* Estado de vigencia del certificado (solo si está vencido) */}
                {(() => {
                  const vencimiento = (validacion as any).vencimiento;

                  if (!vencimiento) {
                    return null;
                  }

                  // Parsear fecha en formato DD/MM/YYYY a YYYY-MM-DD
                  let fechaVencimiento: Date;
                  try {
                    const parts = vencimiento.split('/');
                    if (parts.length === 3) {
                      // DD/MM/YYYY -> YYYY-MM-DD
                      const isoDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
                      fechaVencimiento = new Date(isoDate + 'T23:59:59');
                    } else {
                      // Si ya viene en otro formato, intentar parsearlo directamente
                      fechaVencimiento = new Date(vencimiento + 'T23:59:59');
                    }
                  } catch (e) {
                    console.error('Error parsing vencimiento:', e);
                    return null;
                  }

                  // Fechas con año < 1900 se consideran "sin vencimiento" (ej: 01/01/0001)
                  if (fechaVencimiento.getFullYear() < 1900) {
                    return null;
                  }

                  const fechaActual = new Date();
                  fechaActual.setHours(0, 0, 0, 0);
                  const estaVencido = fechaVencimiento < fechaActual;

                  // Solo mostrar alerta si está vencido
                  if (!estaVencido) return null;

                  return (
                    <Alert
                      severity="error"
                      sx={{
                        mt: 2,
                        maxWidth: 600,
                        mx: 'auto',
                        backgroundColor: '#E91E63',
                        color: 'white',
                        '& .MuiAlert-icon': { color: 'white' }
                      }}
                    >
                      {`Certificado no vigente. Vencido el ${fechaVencimiento.toLocaleDateString()}`}
                    </Alert>
                  );
                })()}
              </>
            ) : (
              <>
                <Error sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
                <Typography variant="h3" gutterBottom color="error">
                  Certificado No Válido
                </Typography>
                <Chip
                  label="✗ NO VERIFICADO"
                  color="error"
                  size="medium"
                  sx={{ fontSize: '1.1rem', px: 2, py: 1 }}
                />
              </>
            )}
          </Box>

          {validacion.esValido && validacion.alumno && validacion.curso && (
            <>
              <Divider sx={{ my: 3 }} />

              {/* Información del Alumno */}
              <Box mb={3}>
                <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Person color="primary" />
                  Información del Alumno
                </Typography>
                <Card variant="outlined" sx={{ p: 2, backgroundColor: 'grey.50' }}>
                  <Typography variant="h6" gutterBottom>
                    {validacion.alumno.nombreApellido}
                  </Typography>
                  <Typography variant="body1" color="text.secondary" gutterBottom>
                    <strong>RUT:</strong> {validacion.alumno.rut}
                  </Typography>
                  {validacion.alumno.calificacion && (
                    <Typography variant="body1" color="text.secondary" gutterBottom>
                      <strong>Calificación:</strong> {validacion.alumno.calificacion}
                    </Typography>
                  )}
                  {validacion.alumno.observaciones && (
                    <Typography variant="body1" color="text.secondary" gutterBottom>
                      <strong>Observaciones:</strong> {validacion.alumno.observaciones}
                    </Typography>
                  )}
                  <Typography variant="caption" color="text.secondary">
                    <DateRange sx={{ fontSize: 16, mr: 0.5, verticalAlign: 'middle' }} />
                    Inscrito el: {new Date(validacion.alumno.createdAt).toLocaleDateString()}
                  </Typography>
                </Card>
              </Box>

              {/* Información del Curso */}
              <Box mb={3}>
                <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <School color="primary" />
                  Información del Curso
                </Typography>
                <Card variant="outlined" sx={{ p: 2, backgroundColor: 'grey.50' }}>
                  <Typography variant="h6" gutterBottom>
                    {validacion.curso.nombre_visualizar_certificado || validacion.curso.nombreReferencia}
                  </Typography>
                  {validacion.curso.otec && (
                    <Typography variant="body1" color="text.secondary" gutterBottom>
                      <strong>OTEC:</strong> {validacion.curso.otec.nombre}
                    </Typography>
                  )}
                  {validacion.alumno.certificado_otorgado && (
                    <Typography variant="body1" color="text.secondary" gutterBottom>
                      <strong>Tipo de Certificado:</strong> {validacion.alumno.certificado_otorgado}
                    </Typography>
                  )}
                  {validacion.alumno.motivo_entrega && (
                    <Typography variant="body1" color="text.secondary" gutterBottom>
                      <strong>Motivo de Entrega:</strong> {validacion.alumno.motivo_entrega}
                    </Typography>
                  )}
                  <Typography variant="caption" color="text.secondary">
                    <DateRange sx={{ fontSize: 16, mr: 0.5, verticalAlign: 'middle' }} />
                    Curso creado el: {new Date(validacion.curso.createdAt).toLocaleDateString()}
                  </Typography>
                </Card>
              </Box>

              {/* Información adicional */}
              <Alert severity="info" sx={{ mt: 3 }}>
                <Typography variant="body2">
                  Este certificado ha sido validado exitosamente. Los datos mostrados corresponden al alumno y curso registrados en el sistema.
                </Typography>
              </Alert>

              {/* Mostrar error de PDF si existe */}
              {pdfError && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {pdfError}
                </Alert>
              )}

              {/* Botón de descarga del certificado */}
              {validacion.alumno && validacion.curso && (
                <Box sx={{ mt: 3, textAlign: 'center' }}>
                  <Button
                    variant="contained"
                    color="primary"
                    startIcon={pdfLoading ? <CircularProgress size={20} color="inherit" /> : <Download />}
                    onClick={handleDownloadCertificate}
                    disabled={pdfLoading}
                    sx={{
                      fontSize: '1.1rem',
                      px: 3,
                      py: 1.5,
                      borderRadius: 2
                    }}
                  >
                    {pdfLoading ? 'Descargando...' : 'Descargar Certificado'}
                  </Button>
                </Box>
              )}
            </>
          )}

          {!validacion.esValido && validacion.mensaje && (
            <>
              <Divider sx={{ my: 3 }} />
              <Alert severity="error">
                <Typography variant="body1">
                  {validacion.mensaje}
                </Typography>
              </Alert>
            </>
          )}
        </CardContent>
      </Card>
    </Container>
  );
}
