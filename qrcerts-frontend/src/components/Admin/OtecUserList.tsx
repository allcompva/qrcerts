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
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  CircularProgress,
} from '@mui/material';
import { Add, Edit, Delete, ArrowBack } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { OtecUser, Otec } from '../../types';
import { apiService } from '../../services/api';
import { useNavigate } from 'react-router-dom';

export function OtecUserList() {
  const [users, setUsers] = useState<OtecUser[]>([]);
  const [filteredUsers, setFilteredUsers] = useState<OtecUser[]>([]);
  const [otecs, setOtecs] = useState<Otec[]>([]);
  const [selectedOtecId, setSelectedOtecId] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [loadingUsers, setLoadingUsers] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingUser, setEditingUser] = useState<OtecUser | null>(null);
  const [formData, setFormData] = useState({
    nombreApellido: '',
    rut: '',
    email: '',
    username: '',
    password: '',
    estado: 1,
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (selectedOtecId) {
      loadUsersByOtec(selectedOtecId);
    } else {
      setFilteredUsers([]);
    }
  }, [selectedOtecId]);

  const loadData = async () => {
    try {
      // Solo cargar las OTECs al inicio
      const otecsData = await apiService.get<Otec[]>('/api/admin/otecs');
      setOtecs(otecsData);
    } catch (error) {
      setError('Error al cargar los datos');
    } finally {
      setLoading(false);
    }
  };

  const loadUsersByOtec = async (otecId: string) => {
    try {
      setLoadingUsers(true);
      setError('');
      const userData = await apiService.get<OtecUser[]>(`/api/admin/otecs/${otecId}/users`);
      setFilteredUsers(userData);
    } catch (error) {
      setError('Error al cargar los usuarios de la OTEC');
      setFilteredUsers([]);
    } finally {
      setLoadingUsers(false);
    }
  };

  const handleOpenDialog = (user?: OtecUser) => {
    // Para nuevos usuarios, verificar que haya una OTEC seleccionada
    if (!user && !selectedOtecId) {
      setError('Debe seleccionar una OTEC antes de agregar un usuario');
      return;
    }

    if (user) {
      setEditingUser(user);
      setFormData({
        nombreApellido: user.nombreApellido,
        rut: user.rut,
        email: user.email,
        username: user.username,
        password: '',
        estado: user.estado,
      });
    } else {
      setEditingUser(null);
      setFormData({
        nombreApellido: '',
        rut: '',
        email: '',
        username: '',
        password: '',
        estado: 1,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingUser(null);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const requestData = {
        username: formData.username,
        email: formData.email,
        nombreApellido: formData.nombreApellido,
        rut: formData.rut,
        estado: formData.estado,
        password: formData.password,
        id: editingUser?.id,
        otecId: editingUser?.otecId
      };

      if (editingUser) {
        // Para editar, agregar el ID
        requestData.id = editingUser.id;
        if (formData.password) {
          requestData.password = formData.password;
        }
      } else {
        // Para crear, agregar otecId y password requerida
        requestData.otecId = selectedOtecId;
        requestData.password = formData.password;
      }

      await apiService.post('/api/admin/otec-users', requestData);
      handleCloseDialog();
      // Recargar los usuarios de la OTEC seleccionada
      if (selectedOtecId) {
        loadUsersByOtec(selectedOtecId);
      }
    } catch (error) {
      setError('Error al guardar el usuario');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('¿Está seguro de eliminar este usuario?')) {
      try {
        await apiService.delete(`/api/admin/otec-users/${id}`);
        // Recargar los usuarios de la OTEC seleccionada
        if (selectedOtecId) {
          loadUsersByOtec(selectedOtecId);
        }
      } catch (error) {
        setError('Error al eliminar el usuario');
      }
    }
  };

  if (loading) return <Layout><Typography>Cargando...</Typography></Layout>;

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">
            Usuarios OTEC
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
              disabled={!selectedOtecId}
            >
              Nuevo Usuario
            </Button>
          </Box>
        </Box>

        {/* Selector de OTEC */}
        <Box sx={{ mb: 3 }}>
          <FormControl fullWidth variant="outlined">
            <InputLabel id="otec-select-label">Seleccionar OTEC</InputLabel>
            <Select
              labelId="otec-select-label"
              value={selectedOtecId}
              onChange={(e) => setSelectedOtecId(e.target.value)}
              label="Seleccionar OTEC"
            >
              <MenuItem value="">
                <em>Seleccione una OTEC</em>
              </MenuItem>
              {otecs.map((otec) => (
                <MenuItem key={otec.id} value={otec.id}>
                  {otec.nombre}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {selectedOtecId ? (
          <>
            <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="h6" color="text.secondary">
                {otecs.find(o => o.id === selectedOtecId)?.nombre || 'OTEC'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {filteredUsers.length} usuario{filteredUsers.length !== 1 ? 's' : ''}
              </Typography>
            </Box>
            <TableContainer component={Paper}>
              {loadingUsers ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
                  <CircularProgress />
                </Box>
              ) : (
                <Table>
                  <TableHead>
                <TableRow>
                  <TableCell>Nombre</TableCell>
                  <TableCell>RUT</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Usuario</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell align="center">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredUsers.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>{user.nombreApellido}</TableCell>
                  <TableCell>{user.rut}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>{user.username}</TableCell>
                  <TableCell>
                    <Chip
                      label={user.estado === 1 ? 'Activo' : 'Inactivo'}
                      color={user.estado === 1 ? 'success' : 'error'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      onClick={() => handleOpenDialog(user)}
                    >
                      <Edit />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => handleDelete(user.id)}
                      color="error"
                    >
                      <Delete />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
              </TableBody>
                </Table>
              )}
            </TableContainer>
          </>
        ) : (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              minHeight: 200,
              bgcolor: 'grey.50',
              borderRadius: 1,
              border: '1px dashed',
              borderColor: 'grey.300'
            }}
          >
            <Typography variant="body1" color="text.secondary">
              Selecciona una OTEC para ver y gestionar sus usuarios
            </Typography>
          </Box>
        )}

        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <form onSubmit={handleSubmit}>
            <DialogTitle>
              {editingUser ? 'Editar Usuario' : 'Nuevo Usuario'}
            </DialogTitle>
            <DialogContent>
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}
              <TextField
                fullWidth
                label="Nombre y Apellido"
                value={formData.nombreApellido}
                onChange={(e) =>
                  setFormData({ ...formData, nombreApellido: e.target.value })
                }
                margin="normal"
                required
              />
              <TextField
                fullWidth
                label="RUT"
                value={formData.rut}
                onChange={(e) =>
                  setFormData({ ...formData, rut: e.target.value })
                }
                margin="normal"
                required
              />
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                margin="normal"
                required
              />
              <TextField
                fullWidth
                label="Usuario"
                value={formData.username}
                onChange={(e) =>
                  setFormData({ ...formData, username: e.target.value })
                }
                margin="normal"
                required
              />
              <TextField
                fullWidth
                label={editingUser ? 'Nueva Contraseña (opcional)' : 'Contraseña'}
                type="password"
                value={formData.password}
                onChange={(e) =>
                  setFormData({ ...formData, password: e.target.value })
                }
                margin="normal"
                required={!editingUser}
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
                <option value={1}>Activo</option>
                <option value={0}>Inactivo</option>
              </TextField>
            </DialogContent>
            <DialogActions>
              <Button onClick={handleCloseDialog}>Cancelar</Button>
              <Button type="submit" variant="contained">
                {editingUser ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogActions>
          </form>
        </Dialog>
      </Box>
    </Layout>
  );
}