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
  Chip,
  Alert,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import { Add, Edit, Delete, Visibility, ArrowBack, MonetizationOn } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { Otec } from '../../types';
import { apiService } from '../../services/api';
import { useNavigate } from 'react-router-dom';

export function OtecList() {
  const [otecs, setOtecs] = useState<Otec[]>([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingOtec, setEditingOtec] = useState<Otec | null>(null);
  const [formData, setFormData] = useState({
    nombre: '',
    slug: '',
    estado: 1,
    moodleHabilitado: false,
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    loadOtecs();
  }, []);

  const loadOtecs = async () => {
    try {
      const data = await apiService.get<Otec[]>('/api/admin/otecs');
      setOtecs(data);
    } catch (error) {
      setError('Error al cargar las OTECs');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (otec?: Otec) => {
    if (otec) {
      setEditingOtec(otec);
      setFormData({
        nombre: otec.nombre,
        slug: otec.slug,
        estado: otec.estado,
        moodleHabilitado: otec.moodleHabilitado ?? false,
      });
    } else {
      setEditingOtec(null);
      setFormData({
        nombre: '',
        slug: '',
        estado: 1,
        moodleHabilitado: false,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingOtec(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (editingOtec) {
        await apiService.put(`/api/admin/otecs/${editingOtec.id}`, formData);
      } else {
        await apiService.post('/api/admin/otecs', formData);
      }
      handleCloseDialog();
      loadOtecs();
    } catch (error) {
      setError('Error al guardar la OTEC');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('¿Está seguro de eliminar esta OTEC?')) {
      try {
        await apiService.delete(`/api/admin/otecs/${id}`);
        loadOtecs();
      } catch (error) {
        setError('Error al eliminar la OTEC');
      }
    }
  };

  const generateSlug = (nombre: string) => {
    return nombre
      .toLowerCase()
      .replace(/[^a-z0-9]/g, '-')
      .replace(/-+/g, '-')
      .replace(/^-|-$/g, '');
  };

  const handleNombreChange = (nombre: string) => {
    setFormData({
      ...formData,
      nombre,
      slug: generateSlug(nombre),
    });
  };

  if (loading) return <Layout><Typography>Cargando...</Typography></Layout>;

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">
            Gestión de OTECs
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<ArrowBack />}
              onClick={() => navigate('/dashboard')}
            >
              Volver
            </Button>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => handleOpenDialog()}
            >
              Nueva OTEC
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
                <TableCell>Nombre</TableCell>
                <TableCell>Slug</TableCell>
                <TableCell>Estado</TableCell>
                <TableCell>Moodle</TableCell>
                <TableCell>Fecha Creación</TableCell>
                <TableCell align="center">Acciones</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {otecs.map((otec) => (
                <TableRow key={otec.id}>
                  <TableCell>{otec.nombre}</TableCell>
                  <TableCell>{otec.slug}</TableCell>
                  <TableCell>
                    <Chip
                      label={otec.estado === 1 ? 'Activa' : 'Inactiva'}
                      color={otec.estado === 1 ? 'success' : 'error'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={otec.moodleHabilitado ? 'Habilitado' : 'No'}
                      color={otec.moodleHabilitado ? 'info' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    {new Date(otec.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(otec)}
                      title="Editar"
                    >
                      <Edit />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => navigate(`/admin/otecs/${otec.id}/quota`)}
                      color="primary"
                      title="Cuotas"
                    >
                      <MonetizationOn />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(otec.id)}
                      color="error"
                      title="Eliminar"
                    >
                      <Delete />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <form onSubmit={handleSubmit}>
            <DialogTitle>
              {editingOtec ? 'Editar OTEC' : 'Nueva OTEC'}
            </DialogTitle>
            <DialogContent>
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}
              <TextField
                fullWidth
                label="Nombre"
                value={formData.nombre}
                onChange={(e) => handleNombreChange(e.target.value)}
                margin="normal"
                required
              />
              <TextField
                fullWidth
                label="Slug"
                value={formData.slug}
                onChange={(e) =>
                  setFormData({ ...formData, slug: e.target.value })
                }
                margin="normal"
                required
                helperText="URL amigable para la OTEC"
              />
              <TextField
                fullWidth
                select
                label="Estado"
                value={formData.estado}
                onChange={(e) =>
                  setFormData({ ...formData, estado: Number(e.target.value) })
                }
                margin="normal"
                SelectProps={{
                  native: true,
                }}
              >
                <option value={1}>Activa</option>
                <option value={0}>Inactiva</option>
              </TextField>

              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.moodleHabilitado}
                    onChange={(e) =>
                      setFormData({ ...formData, moodleHabilitado: e.target.checked })
                    }
                  />
                }
                label="Habilitar integración Moodle"
                sx={{ mt: 2 }}
              />
            </DialogContent>
            <DialogActions>
              <Button onClick={handleCloseDialog}>Cancelar</Button>
              <Button type="submit" variant="contained">
                {editingOtec ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogActions>
          </form>
        </Dialog>
      </Box>
    </Layout>
  );
}