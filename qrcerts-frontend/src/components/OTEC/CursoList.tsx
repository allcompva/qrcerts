import React, { useEffect, useRef, useState } from 'react';
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
  Chip,
  Alert,
  Grid,
  Card,
  CardContent,
  CardActions,
  useMediaQuery,
  useTheme,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText
} from '@mui/material';
import { Add, Edit, Delete, People, CardMembership, Upload, ViewList, ViewModule, School } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { Curso } from '../../types';
import { apiService } from '../../services/api';
import { API_BASE_URL, API_ENDPOINTS } from '../../config/api';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import RichTextLite from '../RichTextLite';
import { Tooltip } from '@mui/material';


// --- Helpers para previews HTML (quita tags y compacta espacios)
const stripHtml = (html?: string) =>
  (html || '')
    .replace(/<style[^>]*>[\s\S]*?<\/style>/gi, '')
    .replace(/<script[^>]*>[\s\S]*?<\/script>/gi, '')
    .replace(/<[^>]+>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();

type HtmlPreviewProps = {
  label?: string;
  html?: string;
  lines?: number;       // 2 o 3
  maxWidth?: number;    // px
};

const HtmlPreview: React.FC<HtmlPreviewProps> = ({ label, html, lines = 2, maxWidth = 520 }) => {
  const text = stripHtml(html);
  return (
    <Box sx={{ display: 'flex', gap: 1, alignItems: 'baseline', maxWidth }}>
      {label ? (
        <Box component="span" sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}>
          {label}:
        </Box>
      ) : null}
      <Box
        component="span"
        sx={{
          display: '-webkit-box',
          WebkitLineClamp: lines,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          lineHeight: 1.35,
          wordBreak: 'break-word',
          minWidth: 0,
        }}
        title={text} // tooltip con el texto completo
      >
        {text}
      </Box>
    </Box>
  );
};

type CertType = 'HORIZONTAL' | 'VERTICAL';

export function CursoList() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const [cursos, setCursos] = useState<Curso[]>([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingCurso, setEditingCurso] = useState<Curso | null>(null);

  const [formData, setFormData] = useState({
    nombreReferencia: '',
    baseUrlPublica: '',
    qrDestino: 1,
    fondoPath: '',
    estado: 1,
    footer_1: '',
    footer_2: '',
    nombre_visualizar_certificado: '',
    certificate_type: 'HORIZONTAL' as CertType,
    contenidoHtml: '',
    footerHtml: '',
  });

  // interlineado por tipo (aplicado al guardar envolviendo en <div style="...">)
  const [lineHeightBody, setLineHeightBody] = useState('1.4');
  const [lineHeightFooter, setLineHeightFooter] = useState('1.3');
  // tamaño global opcional (el usuario además puede cambiar tamaños inline desde la toolbar)
  const [fontSizeBody, setFontSizeBody] = useState('16px');
  const [fontSizeFooter, setFontSizeFooter] = useState('12px');

  const [uploadingImage, setUploadingImage] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [originalFondoPath, setOriginalFondoPath] = useState('');
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const [error, setError] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>(isMobile ? 'grid' : 'list');
  const [moodleConfigured, setMoodleConfigured] = useState(false);
  const { user } = useAuth();
  const navigate = useNavigate();

  // Verificar si Moodle está habilitado para esta OTEC
  const moodleHabilitado = user?.otec?.moodleHabilitado ?? false;

  useEffect(() => {
    loadCursos();
    if (moodleHabilitado) {
      checkMoodleConfig();
    }
  }, [moodleHabilitado]);

  const checkMoodleConfig = async () => {
    try {
      const config = await apiService.get<{ configured: boolean; activo?: boolean }>(API_ENDPOINTS.MOODLE_CONFIG);
      setMoodleConfigured(config.configured && config.activo === true);
    } catch {
      setMoodleConfigured(false);
    }
  };

  const loadCursos = async () => {
    try {
      const data = await apiService.get<Curso[]>('/api/app/cursos');
      setCursos(data);
    } catch {
      setError('Error al cargar los cursos');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (curso?: Curso) => {
    if (curso) {
      setEditingCurso(curso);
      setFormData({
        nombreReferencia: curso.nombreReferencia,
        baseUrlPublica: curso.baseUrlPublica,
        qrDestino: (curso as any).qrDestino ?? 1,
        fondoPath: curso.fondoPath,
        estado: curso.estado,
        footer_1: (curso as any).footer_1 || '',
        footer_2: (curso as any).footer_2 || '',
        nombre_visualizar_certificado: curso.nombre_visualizar_certificado || '',
        certificate_type: ((curso as any).certificate_type as CertType) || 'HORIZONTAL',
        contenidoHtml: (curso as any).contenidoHtml || '',
        footerHtml: (curso as any).footerHtml || '',
      });

      // leer estilos globales si vienen envueltos
      const lhBody = extractCssValue((curso as any).contenidoHtml, 'line-height') ?? '1.4';
      const fsBody = extractCssValue((curso as any).contenidoHtml, 'font-size') ?? '16px';
      const lhFooter = extractCssValue((curso as any).footerHtml, 'line-height') ?? '1.3';
      const fsFooter = extractCssValue((curso as any).footerHtml, 'font-size') ?? '12px';

      setLineHeightBody(lhBody);
      setFontSizeBody(fsBody);
      setLineHeightFooter(lhFooter);
      setFontSizeFooter(fsFooter);

      setOriginalFondoPath(curso.fondoPath);
    } else {
      setEditingCurso(null);
      setFormData({
        nombreReferencia: '',
        baseUrlPublica: '',
        qrDestino: 1,
        fondoPath: '',
        estado: 1,
        footer_1: '',
        footer_2: '',
        nombre_visualizar_certificado: '',
        certificate_type: 'HORIZONTAL',
        contenidoHtml: '',
        footerHtml: '',
      });
      setLineHeightBody('1.4');
      setFontSizeBody('16px');
      setLineHeightFooter('1.3');
      setFontSizeFooter('12px');
      setOriginalFondoPath('');
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    if (imagePreviewUrl) URL.revokeObjectURL(imagePreviewUrl);
    setOpenDialog(false);
    setEditingCurso(null);
    setError('');
    setSelectedFile(null);
    setUploadingImage(false);
    setImagePreviewUrl(null);
    if (editingCurso) {
      setFormData(prev => ({ ...prev, fondoPath: originalFondoPath }));
    }
  };

  const handleImageUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setSelectedFile(file);
    setUploadingImage(true);
    setError('');

    const previewUrl = URL.createObjectURL(file);
    setImagePreviewUrl(previewUrl);

    try {
      const response = await apiService.uploadFile('/api/app/upload/image', file);
      setFormData(prev => ({ ...prev, fondoPath: response.fileName }));
    } catch {
      setError('Error al subir la imagen');
      URL.revokeObjectURL(previewUrl);
      setImagePreviewUrl(null);
    } finally {
      setUploadingImage(false);
    }
  };

  // ===== Helpers CSS inline para envolver HTML con estilos globales =====
  function wrapWithStyles(html: string, styles: Record<string, string>): string {
    if (!html?.trim()) return '';
    const trimmed = html.trim();

    const divMatch = trimmed.match(/^<div([^>]*)style="([^"]*)"([^>]*)>([\s\S]*)<\/div>\s*$/i);
    if (divMatch) {
      const beforeProps = `${divMatch[1] || ''} ${divMatch[3] || ''}`.trim();
      let styleStr = divMatch[2] || '';
      Object.entries(styles).forEach(([k, v]) => {
        if (!v) return;
        const re = new RegExp(`${k}\\s*:\\s*[^;"]*`, 'i');
        if (re.test(styleStr)) {
          styleStr = styleStr.replace(re, `${k}:${v}`);
        } else {
          styleStr = `${styleStr}${styleStr.endsWith(';') || styleStr === '' ? '' : ';'}${k}:${v};`;
        }
      });
      return `<div style="${styleStr}" ${beforeProps}>${divMatch[4]}</div>`;
    }

    const finalStyle = Object.entries(styles)
      .filter(([_, v]) => !!v)
      .map(([k, v]) => `${k}:${v}`)
      .join(';');
    return `<div style="${finalStyle}">${html}</div>`;
  }

  function extractCssValue(html?: string, prop?: string): string | null {
    if (!html || !prop) return null;
    const m = html.match(new RegExp(`${prop}\\s*:\\s*([^;"]+)`, 'i'));
    return m ? m[1].trim() : null;
  }
  // =====================================================================

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      let payload: any = { ...formData };

      if (formData.certificate_type === 'HORIZONTAL') {
        payload.contenidoHtml = '';
        payload.footerHtml = '';
      } else {
        // Aplicar interlineado y tamaño global al contenedor (el inline del usuario manda por encima)
        payload.contenidoHtml = wrapWithStyles(formData.contenidoHtml, {
          'line-height': lineHeightBody,
          'font-size': fontSizeBody,
        });
        payload.footerHtml = wrapWithStyles(formData.footerHtml, {
          'line-height': lineHeightFooter,
          'font-size': fontSizeFooter,
        });
        payload.footer_1 = '';
        payload.footer_2 = '';
      }

      if (editingCurso) {
        await apiService.put(`/api/app/cursos/${editingCurso.id}`, payload);
      } else {
        await apiService.post('/api/app/cursos', {
          ...payload,
          otecId: (user as any)?.otecId,
        });
      }

      handleCloseDialog();
      loadCursos();
    } catch {
      setError('Error al guardar el curso');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Está seguro de eliminar este curso?')) return;
    try {
      await apiService.delete(`/api/app/cursos/${id}`);
      loadCursos();
    } catch {
      setError('Error al eliminar el curso');
    }
  };

  const navigateToAlumnos = (cursoId: string) => navigate(`/otec/cursos/${cursoId}/alumnos`);
  const navigateToCertificados = (cursoId: string) => navigate(`/otec/cursos/${cursoId}/certificados`);

  if (loading) {
    return (
      <Layout>
        <Typography>Cargando...</Typography>
      </Layout>
    );
  }

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">Mis Cursos</Typography>

          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Box sx={{ display: 'flex', border: 1, borderColor: 'divider', borderRadius: 1 }}>
              <IconButton
                onClick={() => setViewMode('grid')}
                color={viewMode === 'grid' ? 'primary' : 'default'}
                sx={{
                  borderRadius: 0,
                  backgroundColor: viewMode === 'grid' ? 'primary.light' : 'transparent',
                  color: viewMode === 'grid' ? 'white' : 'inherit',
                  '&:hover': { backgroundColor: viewMode === 'grid' ? 'primary.light' : 'action.hover' }
                }}
              >
                <ViewModule />
              </IconButton>
              <IconButton
                onClick={() => setViewMode('list')}
                color={viewMode === 'list' ? 'primary' : 'default'}
                sx={{
                  borderRadius: 0,
                  backgroundColor: viewMode === 'list' ? 'primary.light' : 'transparent',
                  color: viewMode === 'list' ? 'white' : 'inherit',
                  '&:hover': { backgroundColor: viewMode === 'list' ? 'primary.light' : 'action.hover' }
                }}
              >
                <ViewList />
              </IconButton>
            </Box>

            <Button variant="outlined" startIcon={<Add />} onClick={() => navigate('/otec/plantillas')}>
              Plantillas Certificados
            </Button>
            {moodleHabilitado && moodleConfigured && (
              <Button variant="outlined" startIcon={<School />} onClick={() => navigate('/otec/cursos/moodle')}>
                Desde Moodle
              </Button>
            )}
            <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/otec/cursos/nuevo')}>
              Nuevo Curso
            </Button>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {/* GRID VIEW */}
        {viewMode === 'grid' && (
          <Grid container spacing={3}>
            {cursos.map((curso) => {
              const ctype: CertType = ((curso as any).certificate_type as CertType);
              const cardAspect = ctype === 'VERTICAL' ? '210/297' : '297/210';
              return (
                <Grid item xs={12} md={6} lg={3} key={curso.id}>
                  <Card sx={{
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    border: 'solid 1px #757575ff',
                    borderRadius: '10px',
                    boxShadow: '0px 2px 1px -1px rgb(0 0 0 / 51%), 5px -4px 6px 0px rgb(0 0 0 / 36%), 0px 1px 3px 0px rgba(0, 0, 0, 0.12)'
                  }}>
                    <CardContent sx={{ flexGrow: 1 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="h6" component="h2" gutterBottom sx={{ flex: 1 }}>
                          {curso.nombreReferencia}
                        </Typography>
                        <Chip
                          label={ctype === 'VERTICAL' ? 'VERTICAL' : 'HORIZONTAL'}
                          color={ctype === 'VERTICAL' ? 'info' : 'default'}
                          size="small"
                        />
                      </Box>

                      {imagePreviewUrl && (
                        <Box sx={{ mt: 2 }}>
                          <Typography variant="body2" color="info.main" sx={{ mb: 1 }}>
                            {selectedFile ? `Nueva imagen: ${selectedFile.name}` : 'Vista previa:'}
                          </Typography>
                          <Box
                            component="img"
                            src={imagePreviewUrl}
                            alt="Vista previa"
                            sx={{
                              width: '100%',
                              maxHeight: 200,
                              aspectRatio: ctype === 'VERTICAL' ? '210/297' : '297/210',
                              objectFit: 'contain',
                              border: '2px solid #e0e0e0',
                              borderRadius: 2,
                              backgroundColor: '#f5f5f5',
                              display: 'block'
                            }}
                          />
                        </Box>
                      )}

                      {/* Imagen actual del curso */}
                      {curso.fondoPath && (
                        <Box sx={{ mt: 2, mb: 2 }}>
                          <Box
                            component="img"
                            src={`${API_BASE_URL === '/' ? window.location.origin : API_BASE_URL.replace(/\/$/, '')}/uploads/images/${curso.fondoPath}`}
                            alt="Fondo del certificado"
                            sx={{
                              width: '100%',
                              maxHeight: 100,
                              aspectRatio: cardAspect,
                              objectFit: 'contain',
                              backgroundColor: 'transparent',
                              display: 'block'
                            }}
                            onError={(e) => {
                              (e.currentTarget as HTMLImageElement).style.display = 'none';
                            }}
                          />
                        </Box>
                      )}

                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <Box component="span" fontWeight="bold">Nombre Curso:</Box>{' '}
                        <Box component="span" fontWeight="normal">{curso.nombre_visualizar_certificado}</Box>
                      </Typography>

                      {ctype === 'HORIZONTAL' ? (
                        <>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <Box component="span" fontWeight="bold">Píe certif. 1:</Box>{' '}
                            <Box component="span" fontWeight="normal">{(curso as any).footer_1}</Box>
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <Box component="span" fontWeight="bold">Píe certif. 2:</Box>{' '}
                            <Box component="span" fontWeight="normal">{(curso as any).footer_2}</Box>
                          </Typography>
                        </>
                      ) : (
                        <>
                          <Typography variant="body2" color="text.secondary" gutterBottom noWrap>
                            <Box component="span" fontWeight="bold">HTML Cuerpo:</Box>{' '}
                            <Box component="span" fontWeight="normal">
                              {String((curso as any).contenidoHtml || '').replace(/<[^>]+>/g, '').slice(0, 80)}
                              {String((curso as any).contenidoHtml || '').length > 80 ? '…' : ''}
                            </Box>
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom noWrap>
                            <Box component="span" fontWeight="bold">HTML Footer:</Box>{' '}
                            <Box component="span" fontWeight="normal">
                              {String((curso as any).footerHtml || '').replace(/<[^>]+>/g, '').slice(0, 80)}
                              {String((curso as any).footerHtml || '').length > 80 ? '…' : ''}
                            </Box>
                          </Typography>
                        </>
                      )}

                      <Chip
                        label={curso.estado === 1 ? 'Activo' : 'Inactivo'}
                        color={curso.estado === 1 ? 'success' : 'error'}
                        size="small"
                        sx={{ mt: 1 }}
                      />
                      <Typography variant="caption" display="block" sx={{ mt: 1 }}>
                        Creado: {new Date(curso.createdAt).toLocaleDateString()}
                      </Typography>
                    </CardContent>

                    <CardActions sx={{ justifyContent: 'space-between', p: 2 }}>
                      <Box display='block'>
                        <IconButton size="small" onClick={() => navigate(`/otec/cursos/editar/${curso.id}`)} title="Editar">
                          <Edit />
                        </IconButton>
                        <IconButton size="small" onClick={() => handleDelete(curso.id)} color="error" title="Eliminar">
                          <Delete />
                        </IconButton>
                        <Button size="small" startIcon={<People />} onClick={() => navigateToAlumnos(curso.id)} sx={{ mr: 1 }} />
                        <Button size="small" startIcon={<CardMembership />} onClick={() => navigateToCertificados(curso.id)} sx={{ mr: 1 }} />
                      </Box>
                    </CardActions>
                  </Card>
                </Grid>
              );
            })}
          </Grid>
        )}

        {/* LIST VIEW */}
        {viewMode === 'list' && (
          <TableContainer component={Paper} sx={{ overflowX: 'hidden' }}>
            <Table
              sx={{
                tableLayout: 'fixed',              // fija anchos por columna
                width: '100%',
                '& td, & th': { borderBottom: '1px solid rgba(224,224,224,0.6)' },
              }}
            >
              <TableHead>
                <TableRow>
                  <TableCell sx={{ width: 96 }}>Imagen</TableCell>
                  <TableCell sx={{ width: 280 }}>Curso</TableCell>
                  <TableCell sx={{ width: { xs: 360, lg: 520 }, maxWidth: 520 }}>Contenido / Pie</TableCell>
                  <TableCell sx={{ width: 120 }}>Tipo</TableCell>
                  <TableCell sx={{ width: 140 }}>Fecha Creación</TableCell>
                  <TableCell sx={{ width: 220 }} align="center">Acciones</TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                {cursos.map((curso) => {
                  const ctype: CertType = ((curso as any).certificate_type as CertType);
                  const cardAspect = ctype === 'VERTICAL' ? '210/297' : '297/210';
                  return (
                    <TableRow key={curso.id} hover>
                      <TableCell>
                        {curso.fondoPath && (
                          <Box
                            component="img"
                            src={`${API_BASE_URL === '/' ? window.location.origin : API_BASE_URL.replace(/\/$/, '')}/uploads/images/${curso.fondoPath}`}
                            alt="Fondo del certificado"
                            sx={{
                              maxHeight: 50,
                              maxWidth: 80,
                              aspectRatio: cardAspect,
                              objectFit: 'contain',
                              backgroundColor: 'transparent',
                              display: 'block'
                            }}
                            onError={(e) => {
                              (e.currentTarget as HTMLImageElement).style.display = 'none';
                            }}
                          />
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="subtitle1" fontWeight="bold">
                          <Box component="span" fontWeight="normal" fontSize="18px">
                            {curso.nombreReferencia}
                          </Box>
                        </Typography>
                        <Typography variant="subtitle1" fontWeight="small">
                          <Box component="span" fontWeight="normal" fontSize="10px">
                            {curso.nombre_visualizar_certificado}
                          </Box>
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {ctype === 'HORIZONTAL' ? (
                          <>
                            <Typography variant="body2" color="text.secondary" gutterBottom>
                              <Box component="span" fontWeight="bold">1:</Box>{' '}
                              <Box component="span" fontWeight="normal">{(curso as any).footer_1}</Box>
                            </Typography>
                            <Typography variant="body2" color="text.secondary" gutterBottom>
                              <Box component="span" fontWeight="bold">2:</Box>{' '}
                              <Box component="span" fontWeight="normal">{(curso as any).footer_2}</Box>
                            </Typography>
                          </>
                        ) : (
                          <>
                            <Typography variant="body2" color="text.secondary" gutterBottom noWrap>
                              <Box component="span" fontWeight="bold">HTML Cuerpo:</Box>{' '}
                              <Box component="span" fontWeight="normal">
                                {String((curso as any).contenidoHtml || '').replace(/<[^>]+>/g, '').slice(0, 80)}
                                {String((curso as any).contenidoHtml || '').length > 80 ? '…' : ''}
                              </Box>
                            </Typography>
                            <Typography variant="body2" color="text.secondary" gutterBottom noWrap>
                              <Box component="span" fontWeight="bold">HTML Footer:</Box>{' '}
                              <Box component="span" fontWeight="normal">
                                {String((curso as any).footerHtml || '').replace(/<[^>]+>/g, '').slice(0, 80)}
                                {String((curso as any).footerHtml || '').length > 80 ? '…' : ''}
                              </Box>
                            </Typography>
                          </>
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={ctype === 'VERTICAL' ? 'VERTICAL' : 'HORIZONTAL'}
                          color={ctype === 'VERTICAL' ? 'info' : 'default'}
                          size="small"
                        />
                      </TableCell>

                      <TableCell>
                        <Typography variant="body2">
                          {new Date(curso.createdAt).toLocaleDateString()}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center' }}>
                          <Tooltip title="Editar">
                            <IconButton size="small" onClick={() => navigate(`/otec/cursos/editar/${curso.id}`)}>
                              <Edit />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Eliminar">
                            <IconButton size="small" onClick={() => handleDelete(curso.id)} color="error">
                              <Delete />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Alumnos">
                            <IconButton size="small" onClick={() => navigateToAlumnos(curso.id)}>
                              <People />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Certificados">
                            <IconButton size="small" onClick={() => navigateToCertificados(curso.id)}>
                              <CardMembership />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        )}

        {/* DIALOGO CREAR/EDITAR */}
        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <form onSubmit={handleSubmit}>
            <DialogTitle>{editingCurso ? 'Editar Curso' : 'Nuevo Curso'}</DialogTitle>
            <DialogContent>
              {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

              <TextField
                fullWidth
                label="Nombre de Referencia"
                value={formData.nombreReferencia}
                onChange={(e) => setFormData({ ...formData, nombreReferencia: e.target.value })}
                margin="normal"
                required
              />

              <TextField
                fullWidth
                label="Nombre Curso (visualizar en certificado)"
                value={formData.nombre_visualizar_certificado}
                onChange={(e) => setFormData({ ...formData, nombre_visualizar_certificado: e.target.value })}
                margin="normal"
                required
              />

              <TextField
                fullWidth
                select
                label="Tipo de Certificado"
                value={formData.certificate_type}
                onChange={(e) => setFormData({ ...formData, certificate_type: e.target.value as CertType })}
                margin="normal"
                SelectProps={{ native: true }}
              >
                <option value="HORIZONTAL">Horizontal (A4 apaisado) — 2 líneas de pie</option>
                <option value="VERTICAL">Vertical (A4) — HTML cuerpo + HTML footer</option>
              </TextField>

              {/* HORIZONTAL */}
              {formData.certificate_type === 'HORIZONTAL' && (
                <>
                  <TextField
                    fullWidth
                    label="Texto píe de página 1"
                    value={formData.footer_1}
                    onChange={(e) => setFormData({ ...formData, footer_1: e.target.value })}
                    margin="normal"
                    required
                    helperText="Primera línea del pie del certificado (horizontal)"
                  />
                  <TextField
                    fullWidth
                    label="Texto píe de página 2"
                    value={formData.footer_2}
                    onChange={(e) => setFormData({ ...formData, footer_2: e.target.value })}
                    margin="normal"
                    required
                    helperText="Segunda línea del pie del certificado (horizontal)"
                  />
                </>
              )}

              {/* VERTICAL */}
              {formData.certificate_type === 'VERTICAL' && (
                <>
                  {/* Controles de formato (cuerpo) */}
                  <Box sx={{ mt: 2, mb: 1, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                    <FormControl size="small" sx={{ minWidth: 160 }}>
                      <InputLabel id="lh-body-label">Interlineado (cuerpo)</InputLabel>
                      <Select
                        labelId="lh-body-label"
                        label="Interlineado (cuerpo)"
                        value={lineHeightBody}
                        onChange={(e) => setLineHeightBody(String(e.target.value))}
                      >
                        <MenuItem value="1.0">1.0</MenuItem>
                        <MenuItem value="1.15">1.15</MenuItem>
                        <MenuItem value="1.4">1.4</MenuItem>
                        <MenuItem value="1.5">1.5</MenuItem>
                        <MenuItem value="2.0">2.0</MenuItem>
                      </Select>
                    </FormControl>

                    <FormControl size="small" sx={{ minWidth: 160 }}>
                      <InputLabel id="fs-body-label">Tamaño global (cuerpo)</InputLabel>
                      <Select
                        labelId="fs-body-label"
                        label="Tamaño global (cuerpo)"
                        value={fontSizeBody}
                        onChange={(e) => setFontSizeBody(String(e.target.value))}
                      >
                        <MenuItem value="12px">12 px</MenuItem>
                        <MenuItem value="14px">14 px</MenuItem>
                        <MenuItem value="16px">16 px</MenuItem>
                        <MenuItem value="18px">18 px</MenuItem>
                        <MenuItem value="20px">20 px</MenuItem>
                      </Select>
                    </FormControl>
                  </Box>

                  <RichTextLite
                    value={formData.contenidoHtml}
                    onChange={(html) => setFormData(prev => ({ ...prev, contenidoHtml: html }))}
                    minHeight={200}
                  />
                  <FormHelperText>
                    Usá la toolbar para negrita/ital/underline, listas, alineado y tamaños inline. El interlineado y tamaño global se aplican al guardar.
                  </FormHelperText>

                  {/* Controles de formato (footer) */}
                  <Box sx={{ mt: 3, mb: 1, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                    <FormControl size="small" sx={{ minWidth: 160 }}>
                      <InputLabel id="lh-footer-label">Interlineado (footer)</InputLabel>
                      <Select
                        labelId="lh-footer-label"
                        label="Interlineado (footer)"
                        value={lineHeightFooter}
                        onChange={(e) => setLineHeightFooter(String(e.target.value))}
                      >
                        <MenuItem value="1.0">1.0</MenuItem>
                        <MenuItem value="1.15">1.15</MenuItem>
                        <MenuItem value="1.3">1.3</MenuItem>
                        <MenuItem value="1.5">1.5</MenuItem>
                        <MenuItem value="2.0">2.0</MenuItem>
                      </Select>
                    </FormControl>

                    <FormControl size="small" sx={{ minWidth: 160 }}>
                      <InputLabel id="fs-footer-label">Tamaño global (footer)</InputLabel>
                      <Select
                        labelId="fs-footer-label"
                        label="Tamaño global (footer)"
                        value={fontSizeFooter}
                        onChange={(e) => setFontSizeFooter(String(e.target.value))}
                      >
                        <MenuItem value="10px">10 px</MenuItem>
                        <MenuItem value="12px">12 px</MenuItem>
                        <MenuItem value="14px">14 px</MenuItem>
                        <MenuItem value="16px">16 px</MenuItem>
                      </Select>
                    </FormControl>
                  </Box>

                  <RichTextLite
                    value={formData.footerHtml}
                    onChange={(html) => setFormData(prev => ({ ...prev, footerHtml: html }))}
                    minHeight={130}
                  />
                  <FormHelperText>
                    Si lo dejás vacío, tu generador puede usar <code>footer_2</code> como leyenda QR.
                  </FormHelperText>
                </>
              )}

              {/* Imagen de fondo */}
              <Box sx={{ mb: 2, mt: 2 }}>
                <Typography variant="body2" sx={{ mb: 1 }}>Imagen de Fondo para Certificados *</Typography>
                <Button
                  variant="outlined"
                  component="label"
                  startIcon={<Upload />}
                  disabled={uploadingImage}
                  fullWidth
                  sx={{ mb: 1 }}
                >
                  {uploadingImage ? 'Subiendo...' : 'Seleccionar Imagen'}
                  <input type="file" hidden accept="image/*" onChange={handleImageUpload} />
                </Button>

                {/* Preview nueva */}
                {imagePreviewUrl && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="info.main" sx={{ mb: 1 }}>
                      {selectedFile ? `Nueva imagen: ${selectedFile.name}` : 'Vista previa:'}
                    </Typography>
                    <Box
                      component="img"
                      src={imagePreviewUrl}
                      alt="Vista previa"
                      sx={{
                        width: '100%',
                        maxHeight: 200,
                        aspectRatio: formData.certificate_type === 'VERTICAL' ? '210/297' : '297/210',
                        objectFit: 'contain',
                        border: '2px solid #e0e0e0',
                        borderRadius: 2,
                        backgroundColor: '#f5f5f5',
                        display: 'block'
                      }}
                    />
                  </Box>
                )}

                {/* Preview actual */}
                {formData.fondoPath && !imagePreviewUrl && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                      Imagen actual: {formData.fondoPath}
                    </Typography>
                    <Box
                      component="img"
                      src={`${API_BASE_URL}/uploads/images/${formData.fondoPath}`}
                      alt="Imagen actual"
                      sx={{
                        width: '100%',
                        maxHeight: 200,
                        aspectRatio: formData.certificate_type === 'VERTICAL' ? '210/297' : '297/210',
                        objectFit: 'contain',
                        border: '2px solid #4caf50',
                        borderRadius: 2,
                        backgroundColor: '#f5f5f5',
                        display: 'block'
                      }}
                      onError={(e) => {
                        (e.currentTarget as HTMLImageElement).style.display = 'none';
                      }}
                    />
                  </Box>
                )}
              </Box>

              <TextField
                fullWidth
                select
                label="Destino QR"
                value={formData.qrDestino}
                onChange={(e) => setFormData({ ...formData, qrDestino: Number(e.target.value) })}
                margin="normal"
                SelectProps={{ native: true }}
              >
                <option value={1}>PDF</option>
                <option value={2}>Página de verificación</option>
              </TextField>

              <TextField
                fullWidth
                select
                label="Estado"
                value={formData.estado}
                onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })}
                margin="normal"
                SelectProps={{ native: true }}
              >
                <option value={1}>Activo</option>
                <option value={0}>Inactivo</option>
              </TextField>
            </DialogContent>

            <DialogActions>
              <Button onClick={handleCloseDialog}>Cancelar</Button>
              <Button type="submit" variant="contained">
                {editingCurso ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogActions>
          </form>
        </Dialog>
      </Box>
    </Layout>
  );
}







