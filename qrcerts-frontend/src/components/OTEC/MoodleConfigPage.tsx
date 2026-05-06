import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  TextField,
  Alert,
  CircularProgress,
  Card,
  CardContent,
  Chip,
  Divider,
  InputAdornment,
  IconButton,
  Grid
} from '@mui/material';
import {
  School,
  Link as LinkIcon,
  Key,
  CheckCircle,
  Error as ErrorIcon,
  Visibility,
  VisibilityOff,
  Refresh,
  Save
} from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { apiService } from '../../services/api';
import { API_ENDPOINTS } from '../../config/api';

interface MoodleConfigResponse {
  configured: boolean;
  moodleUrl?: string;
  activo?: boolean;
  ultimaConexionExitosa?: string;
  hasToken?: boolean;
}

interface TestConnectionResponse {
  success: boolean;
  siteName?: string;
  siteUrl?: string;
  moodleVersion?: string;
  error?: string;
}

export function MoodleConfigPage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [config, setConfig] = useState<MoodleConfigResponse | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [testResult, setTestResult] = useState<TestConnectionResponse | null>(null);

  const [formData, setFormData] = useState({
    moodleUrl: '',
    token: ''
  });
  const [showToken, setShowToken] = useState(false);

  useEffect(() => {
    loadConfig();
  }, []);

  const loadConfig = async () => {
    try {
      setLoading(true);
      const data = await apiService.get<MoodleConfigResponse>(API_ENDPOINTS.MOODLE_CONFIG);
      setConfig(data);
      if (data.configured && data.moodleUrl) {
        setFormData(prev => ({
          ...prev,
          moodleUrl: data.moodleUrl || ''
        }));
      }
    } catch (err: any) {
      setError('Error al cargar la configuracion de Moodle');
    } finally {
      setLoading(false);
    }
  };

  const handleTestConnection = async () => {
    if (!formData.moodleUrl) {
      setError('Ingrese la URL de Moodle');
      return;
    }

    try {
      setTesting(true);
      setError('');
      setTestResult(null);

      const result = await apiService.post<TestConnectionResponse>(
        API_ENDPOINTS.MOODLE_TEST_CONNECTION,
        {
          moodleUrl: formData.moodleUrl,
          token: formData.token || undefined
        }
      );

      setTestResult(result);
      if (!result.success) {
        setError(result.error || 'Error de conexion');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al probar la conexion');
    } finally {
      setTesting(false);
    }
  };

  const handleSave = async () => {
    if (!formData.moodleUrl) {
      setError('Ingrese la URL de Moodle');
      return;
    }

    try {
      setSaving(true);
      setError('');
      setSuccess('');

      await apiService.post(API_ENDPOINTS.MOODLE_CONFIG, {
        moodleUrl: formData.moodleUrl,
        token: formData.token
      });

      setSuccess('Configuracion guardada exitosamente');
      setFormData(prev => ({ ...prev, token: '' }));
      await loadConfig();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al guardar la configuracion');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Layout>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
          <CircularProgress />
        </Box>
      </Layout>
    );
  }

  return (
    <Layout>
      <Box>
        <Typography variant="h4" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <School /> Integracion Moodle
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
          Configure la conexion con su plataforma Moodle para poder importar estudiantes desde sus cursos.
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}
        {success && (
          <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess('')}>
            {success}
          </Alert>
        )}

        <Grid container spacing={3}>
          {/* Estado actual */}
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Estado de Conexion
                </Typography>
                <Divider sx={{ my: 2 }} />

                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                      Estado:
                    </Typography>
                    {config?.configured ? (
                      <Chip
                        icon={<CheckCircle />}
                        label="Configurado"
                        color="success"
                        size="small"
                      />
                    ) : (
                      <Chip
                        icon={<ErrorIcon />}
                        label="No configurado"
                        color="default"
                        size="small"
                      />
                    )}
                  </Box>

                  {config?.configured && (
                    <>
                      <Box>
                        <Typography variant="body2" color="text.secondary">
                          URL Moodle:
                        </Typography>
                        <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                          {config.moodleUrl}
                        </Typography>
                      </Box>

                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                          Token:
                        </Typography>
                        {config.hasToken ? (
                          <Chip label="Configurado" color="success" size="small" variant="outlined" />
                        ) : (
                          <Chip label="No configurado" color="warning" size="small" variant="outlined" />
                        )}
                      </Box>

                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                          Activo:
                        </Typography>
                        <Chip
                          label={config.activo ? 'Si' : 'No'}
                          color={config.activo ? 'success' : 'default'}
                          size="small"
                          variant="outlined"
                        />
                      </Box>

                      {config.ultimaConexionExitosa && (
                        <Box>
                          <Typography variant="body2" color="text.secondary">
                            Ultima conexion exitosa:
                          </Typography>
                          <Typography variant="body2">
                            {new Date(config.ultimaConexionExitosa).toLocaleString()}
                          </Typography>
                        </Box>
                      )}
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>

            {/* Resultado del test */}
            {testResult && testResult.success && (
              <Card sx={{ mt: 2 }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="success.main">
                    Conexion Exitosa
                  </Typography>
                  <Divider sx={{ my: 2 }} />
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                    <Typography variant="body2">
                      <strong>Sitio:</strong> {testResult.siteName}
                    </Typography>
                    <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                      <strong>URL:</strong> {testResult.siteUrl}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Version:</strong> {testResult.moodleVersion}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            )}
          </Grid>

          {/* Formulario de configuracion */}
          <Grid item xs={12} md={8}>
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Configuracion de Conexion
              </Typography>
              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                <TextField
                  fullWidth
                  label="URL de Moodle"
                  placeholder="https://tu-moodle.com"
                  value={formData.moodleUrl}
                  onChange={(e) => setFormData(prev => ({ ...prev, moodleUrl: e.target.value }))}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <LinkIcon />
                      </InputAdornment>
                    ),
                  }}
                  helperText="Ingrese la URL base de su instalacion Moodle (sin /webservice/)"
                />

                <TextField
                  fullWidth
                  label="Token de Web Service"
                  type={showToken ? 'text' : 'password'}
                  placeholder={config?.hasToken ? '(Token configurado - ingrese nuevo para cambiar)' : 'Ingrese el token'}
                  value={formData.token}
                  onChange={(e) => setFormData(prev => ({ ...prev, token: e.target.value }))}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Key />
                      </InputAdornment>
                    ),
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          onClick={() => setShowToken(!showToken)}
                          edge="end"
                        >
                          {showToken ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                  helperText="Token de acceso al Web Service de Moodle. Dejelo vacio para mantener el actual."
                />

                <Alert severity="info" sx={{ mt: 1 }}>
                  <Typography variant="body2">
                    <strong>Como obtener el token:</strong>
                  </Typography>
                  <ol style={{ margin: '8px 0', paddingLeft: '20px' }}>
                    <li>Acceda a su Moodle como administrador</li>
                    <li>Vaya a Administracion del sitio &gt; Servidor &gt; Web services &gt; Gestion de tokens (Moodle 4.x)</li>
                    <li>Cree un token para un usuario con permisos de ver cursos y usuarios</li>
                    <li>Copie el token generado y peguelo aqui</li>
                  </ol>
                </Alert>

                <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
                  <Button
                    variant="outlined"
                    startIcon={testing ? <CircularProgress size={20} /> : <Refresh />}
                    onClick={handleTestConnection}
                    disabled={testing || saving || !formData.moodleUrl}
                  >
                    {testing ? 'Probando...' : 'Probar Conexion'}
                  </Button>

                  <Button
                    variant="contained"
                    startIcon={saving ? <CircularProgress size={20} /> : <Save />}
                    onClick={handleSave}
                    disabled={testing || saving || !formData.moodleUrl}
                  >
                    {saving ? 'Guardando...' : 'Guardar Configuracion'}
                  </Button>
                </Box>
              </Box>
            </Paper>
          </Grid>
        </Grid>
      </Box>
    </Layout>
  );
}
