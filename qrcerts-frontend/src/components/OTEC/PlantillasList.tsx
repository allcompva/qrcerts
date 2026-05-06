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
  Alert,
  Chip,
  CircularProgress,
} from '@mui/material';
import { Add, Edit, Delete, Description } from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { PlantillaCertificado } from '../../types';
import { apiService } from '../../services/api';
import { useNavigate } from 'react-router-dom';

export function PlantillasList() {
  const navigate = useNavigate();
  const [plantillas, setPlantillas] = useState<PlantillaCertificado[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadPlantillas();
  }, []);

  const loadPlantillas = async () => {
    try {
      setLoading(true);
      const data = await apiService.get<PlantillaCertificado[]>('/api/app/plantillas-certificados');
      setPlantillas(data);
    } catch (err: any) {
      setError(err.message || 'Error al cargar plantillas');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Está seguro de eliminar esta plantilla?')) return;

    try {
      await apiService.delete(`/api/app/plantillas-certificados/${id}`);
      loadPlantillas();
    } catch (err: any) {
      setError(err.message || 'Error al eliminar plantilla');
    }
  };

  const parseVariables = (variablesJson?: string): string[] => {
    if (!variablesJson) return [];
    try {
      return JSON.parse(variablesJson);
    } catch {
      return variablesJson.split(',').map(v => v.trim()).filter(v => v);
    }
  };

  return (
    <Layout>
      <Box sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4">Plantillas de Certificados</Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/otec/plantillas/nueva')}
          >
            Nueva Plantilla
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Nombre</TableCell>
                  <TableCell>Tipo</TableCell>
                  <TableCell>Variables</TableCell>
                  <TableCell>Archivo</TableCell>
                  <TableCell>Fecha Creación</TableCell>
                  <TableCell align="right">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {plantillas.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      <Typography color="text.secondary">No hay plantillas creadas</Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  plantillas.map((plantilla) => (
                    <TableRow key={plantilla.id}>
                      <TableCell>{plantilla.nombre}</TableCell>
                      <TableCell>
                        <Chip
                          label={plantilla.tipo || 'N/A'}
                          size="small"
                          color={plantilla.tipo === 'HORIZONTAL' ? 'primary' : 'secondary'}
                        />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                          {parseVariables(plantilla.variables || plantilla.contenido).slice(0, 5).map((variable, idx) => (
                            <Chip key={idx} label={`{{${variable}}}`} size="small" variant="outlined" />
                          ))}
                          {parseVariables(plantilla.variables || plantilla.contenido).length > 5 && (
                            <Chip label={`+${parseVariables(plantilla.variables || plantilla.contenido).length - 5}`} size="small" />
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip icon={<Description />} label=".docx" size="small" />
                      </TableCell>
                      <TableCell>
                        {plantilla.createdAt ? new Date(plantilla.createdAt).toLocaleDateString() : '-'}
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          onClick={() => navigate(`/otec/plantillas/editar/${plantilla.id}`)}
                          title="Editar"
                        >
                          <Edit />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDelete(plantilla.id)}
                          title="Eliminar"
                          color="error"
                        >
                          <Delete />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Box>
    </Layout>
  );
}
