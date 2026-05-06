import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  Alert,
  TextField,
  Switch,
  FormControlLabel,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Card,
  CardContent,
  Grid,
  Divider,
} from '@mui/material';
import { ArrowBack, Add } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { useNavigate, useParams } from 'react-router-dom';
import { apiService } from '../../services/api';

interface QuotaConfig {
  quotaActivo: boolean;
  ordenActiva: OrdenCompra | null;
}

interface OrdenCompra {
  id: string;
  otecId: string;
  cantidadComprada: number;
  cantidadUsada: number;
  disponibles: number;
  fechaExpiracion: string;
  activa: boolean;
  expirada: boolean;
  creadaPor: string;
  notas: string | null;
  createdAt: string;
}

interface HistorialEntry {
  id: string;
  ordenCompraId: string;
  evento: string;
  detalle: string | null;
  creadaPor: string;
  createdAt: string;
}

export function QuotaManager() {
  const { otecId } = useParams<{ otecId: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [quotaActivo, setQuotaActivo] = useState(false);
  const [ordenActiva, setOrdenActiva] = useState<OrdenCompra | null>(null);
  const [ordenes, setOrdenes] = useState<OrdenCompra[]>([]);
  const [historial, setHistorial] = useState<HistorialEntry[]>([]);

  const [nuevaOrden, setNuevaOrden] = useState({
    cantidad: '',
    fechaExpiracion: '',
    notas: '',
  });

  useEffect(() => {
    loadData();
  }, [otecId]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [config, ordenesData, historialData] = await Promise.all([
        apiService.get<QuotaConfig>(`/api/admin/otecs/${otecId}/quota`),
        apiService.get<OrdenCompra[]>(`/api/admin/otecs/${otecId}/quota/ordenes`),
        apiService.get<HistorialEntry[]>(`/api/admin/otecs/${otecId}/quota/historial`),
      ]);
      setQuotaActivo(config.quotaActivo);
      setOrdenActiva(config.ordenActiva);
      setOrdenes(ordenesData);
      setHistorial(historialData);
    } catch (err: any) {
      setError('Error al cargar datos de cuota');
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = async () => {
    try {
      setError('');
      const newValue = !quotaActivo;
      await apiService.put(`/api/admin/otecs/${otecId}/quota/toggle`, { quotaActivo: newValue });
      setQuotaActivo(newValue);
      setSuccess(newValue ? 'Cuota activada' : 'Cuota desactivada (emisión libre)');
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: any) {
      setError('Error al cambiar estado de cuota');
    }
  };

  const handleCrearOrden = async () => {
    try {
      setError('');
      if (!nuevaOrden.cantidad || !nuevaOrden.fechaExpiracion) {
        setError('Complete cantidad y fecha de expiración');
        return;
      }
      const cantidad = parseInt(nuevaOrden.cantidad);
      if (cantidad <= 0) {
        setError('La cantidad debe ser mayor a 0');
        return;
      }
      await apiService.post(`/api/admin/otecs/${otecId}/quota/ordenes`, {
        cantidad,
        fechaExpiracion: nuevaOrden.fechaExpiracion,
        notas: nuevaOrden.notas || null,
      });
      setNuevaOrden({ cantidad: '', fechaExpiracion: '', notas: '' });
      setSuccess('Orden creada exitosamente');
      setTimeout(() => setSuccess(''), 3000);
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear orden');
    }
  };

  const getNivelColor = (orden: OrdenCompra) => {
    if (orden.expirada || orden.disponibles <= 0) return 'error';
    const diasRestantes = Math.max(0, Math.floor((new Date(orden.fechaExpiracion).getTime() - Date.now()) / 86400000));
    const porcentaje = orden.cantidadComprada > 0 ? (orden.cantidadUsada / orden.cantidadComprada) * 100 : 100;
    if (diasRestantes <= 3 || porcentaje >= 90) return 'error';
    if (diasRestantes < 7 || porcentaje > 70) return 'warning';
    return 'success';
  };

  if (loading) return <Layout><Typography>Cargando...</Typography></Layout>;

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4">Gestión de Cuotas</Typography>
          <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate('/admin/otecs')}>
            Volver
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

        {/* Toggle */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <FormControlLabel
              control={<Switch checked={quotaActivo} onChange={handleToggle} color="primary" />}
              label={
                <Box>
                  <Typography variant="h6">
                    Sistema de cuotas: {quotaActivo ? 'Activado' : 'Desactivado'}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {quotaActivo
                      ? 'La empresa solo puede emitir certificados con saldo disponible'
                      : 'La empresa puede emitir certificados sin límite'}
                  </Typography>
                </Box>
              }
            />
          </CardContent>
        </Card>

        {quotaActivo && (
          <>
            {/* Orden Activa */}
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>Orden Activa</Typography>
                {ordenActiva ? (
                  <Grid container spacing={2}>
                    <Grid item xs={3}>
                      <Typography variant="body2" color="text.secondary">Disponibles</Typography>
                      <Typography variant="h4">
                        <Chip
                          label={`${ordenActiva.disponibles} / ${ordenActiva.cantidadComprada}`}
                          color={getNivelColor(ordenActiva) as any}
                          size="medium"
                        />
                      </Typography>
                    </Grid>
                    <Grid item xs={3}>
                      <Typography variant="body2" color="text.secondary">Usados</Typography>
                      <Typography variant="h5">{ordenActiva.cantidadUsada}</Typography>
                    </Grid>
                    <Grid item xs={3}>
                      <Typography variant="body2" color="text.secondary">Vencimiento</Typography>
                      <Typography variant="h6">
                        {new Date(ordenActiva.fechaExpiracion).toLocaleDateString()}
                      </Typography>
                      {ordenActiva.expirada && <Chip label="VENCIDA" color="error" size="small" />}
                    </Grid>
                    <Grid item xs={3}>
                      <Typography variant="body2" color="text.secondary">Creada por</Typography>
                      <Typography>{ordenActiva.creadaPor}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(ordenActiva.createdAt).toLocaleDateString()}
                      </Typography>
                    </Grid>
                  </Grid>
                ) : (
                  <Alert severity="warning">No hay orden activa. Cree una para que la empresa pueda emitir.</Alert>
                )}
              </CardContent>
            </Card>

            {/* Nueva Orden */}
            <Card sx={{ mb: 3 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>Nueva Orden de Compra</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  Al crear una nueva orden, la anterior se desactiva automáticamente y pasa al historial.
                </Typography>
                <Grid container spacing={2} alignItems="center">
                  <Grid item xs={3}>
                    <TextField
                      fullWidth
                      type="number"
                      label="Cantidad de certificados"
                      value={nuevaOrden.cantidad}
                      onChange={(e) => setNuevaOrden({ ...nuevaOrden, cantidad: e.target.value })}
                    />
                  </Grid>
                  <Grid item xs={3}>
                    <TextField
                      fullWidth
                      type="date"
                      label="Fecha de vencimiento"
                      value={nuevaOrden.fechaExpiracion}
                      onChange={(e) => setNuevaOrden({ ...nuevaOrden, fechaExpiracion: e.target.value })}
                      InputLabelProps={{ shrink: true }}
                    />
                  </Grid>
                  <Grid item xs={4}>
                    <TextField
                      fullWidth
                      label="Notas (opcional)"
                      value={nuevaOrden.notas}
                      onChange={(e) => setNuevaOrden({ ...nuevaOrden, notas: e.target.value })}
                    />
                  </Grid>
                  <Grid item xs={2}>
                    <Button
                      fullWidth
                      variant="contained"
                      startIcon={<Add />}
                      onClick={handleCrearOrden}
                      sx={{ height: 56 }}
                    >
                      Crear
                    </Button>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>

            {/* Historial de Ordenes */}
            <Typography variant="h6" gutterBottom>Historial de Órdenes</Typography>
            <TableContainer component={Paper} sx={{ mb: 3 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Fecha</TableCell>
                    <TableCell>Cantidad</TableCell>
                    <TableCell>Usados</TableCell>
                    <TableCell>Vencimiento</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell>Creada por</TableCell>
                    <TableCell>Notas</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {ordenes.map((orden) => (
                    <TableRow key={orden.id}>
                      <TableCell>{new Date(orden.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell>{orden.cantidadComprada}</TableCell>
                      <TableCell>{orden.cantidadUsada}</TableCell>
                      <TableCell>{new Date(orden.fechaExpiracion).toLocaleDateString()}</TableCell>
                      <TableCell>
                        <Chip
                          label={orden.activa ? 'Activa' : orden.expirada ? 'Vencida' : 'Reemplazada'}
                          color={orden.activa ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{orden.creadaPor}</TableCell>
                      <TableCell>{orden.notas || '-'}</TableCell>
                    </TableRow>
                  ))}
                  {ordenes.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={7} align="center">Sin órdenes</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>

            {/* Auditoría */}
            <Typography variant="h6" gutterBottom>Registro de Auditoría</Typography>
            <TableContainer component={Paper}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Fecha</TableCell>
                    <TableCell>Evento</TableCell>
                    <TableCell>Detalle</TableCell>
                    <TableCell>Usuario</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {historial.map((h) => (
                    <TableRow key={h.id}>
                      <TableCell>{new Date(h.createdAt).toLocaleString()}</TableCell>
                      <TableCell>
                        <Chip
                          label={h.evento}
                          size="small"
                          color={
                            h.evento === 'CREATED' ? 'success' :
                            h.evento === 'SUPERSEDED' ? 'default' :
                            h.evento === 'QUOTA_CONSUMED' ? 'info' : 'warning'
                          }
                        />
                      </TableCell>
                      <TableCell>{h.detalle || '-'}</TableCell>
                      <TableCell>{h.creadaPor}</TableCell>
                    </TableRow>
                  ))}
                  {historial.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">Sin registros</TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </>
        )}
      </Box>
    </Layout>
  );
}
