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
  Checkbox,
  Menu,
  MenuItem,
  CircularProgress,
} from '@mui/material';
import {
  ArrowBack,
  MoreVert,
  Download,
  Delete,
} from '@mui/icons-material';
import { Layout } from '../Layout/Layout';
import { Alumno, Curso } from '../../types';
import { apiService } from '../../services/api';
import { useParams, useNavigate } from 'react-router-dom';
import { useCertificatePDF } from '../../hooks/useCertificatePDF';

export function CertificadosList() {
  const { cursoId } = useParams<{ cursoId: string }>();
  const navigate = useNavigate();
  const { downloadCertificate, downloadMultipleCertificates, loading: pdfLoading, error: pdfError } = useCertificatePDF();
  const [curso, setCurso] = useState<Curso | null>(null);
  const [alumnos, setAlumnos] = useState<Alumno[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedAlumnos, setSelectedAlumnos] = useState<string[]>([]);
  const [selectedForDownload, setSelectedForDownload] = useState<string[]>([]);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [selectedAlumno, setSelectedAlumno] = useState<Alumno | null>(null);

  // Función para obtener el nombre del alumno (desde nombreApellido o desde observaciones JSON)
  const getAlumnoNombre = (alumno: Alumno): string => {
    if (alumno.nombreApellido) {
      return alumno.nombreApellido;
    }
    const obsText = alumno.observaciones || '';
    try {
      if (obsText.trim().startsWith('{') && obsText.trim().endsWith('}')) {
        const parsed = JSON.parse(obsText);
        return parsed['Nombre'] || parsed['NOMBRE'] || parsed['nombre'] ||
               parsed['NombreAlumno'] || parsed['NombreApellido'] || '';
      }
    } catch {
      // No es JSON válido
    }
    return '';
  };

  // Función para obtener el RUT del alumno (desde rut o desde observaciones JSON)
  const getAlumnoRut = (alumno: Alumno): string => {
    if (alumno.rut) {
      return alumno.rut;
    }
    const obsText = alumno.observaciones || '';
    try {
      if (obsText.trim().startsWith('{') && obsText.trim().endsWith('}')) {
        const parsed = JSON.parse(obsText);
        return parsed['RUT'] || parsed['Rut'] || parsed['rut'] || '';
      }
    } catch {
      // No es JSON válido
    }
    return '';
  };

  useEffect(() => {
    if (cursoId) {
      loadData();
    }
  }, [cursoId]);

  const loadData = async () => {
    try {
      const [cursoData, alumnosData] = await Promise.all([
        apiService.get<Curso>(`/api/app/cursos/${cursoId}`),
        apiService.get<Alumno[]>(`/api/app/alumnos/GetByCursoId/${cursoId}`),
      ]);

      setCurso(cursoData);
      setAlumnos(alumnosData);
    } catch (error) {
      setError('Error al cargar los datos');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectAlumno = (id: string) => {
    setSelectedAlumnos(prev =>
      prev.includes(id)
        ? prev.filter(i => i !== id)
        : [...prev, id]
    );
  };

  const handleSelectForDownload = (id: string) => {
    setSelectedForDownload(prev =>
      prev.includes(id)
        ? prev.filter(i => i !== id)
        : [...prev, id]
    );
  };

  const handleSelectAll = () => {
    const selectableAlumnos = alumnos.filter(alumno => !alumno.certificado);
    setSelectedAlumnos(
      selectedAlumnos.length === selectableAlumnos.length
        ? []
        : selectableAlumnos.map(a => a.id)
    );
  };

  const handleSelectAllForDownload = () => {
    const downloadableAlumnos = alumnos.filter(alumno => alumno.certificado);
    setSelectedForDownload(
      selectedForDownload.length === downloadableAlumnos.length
        ? []
        : downloadableAlumnos.map(a => a.id)
    );
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, alumno: Alumno) => {
    setAnchorEl(event.currentTarget);
    setSelectedAlumno(alumno);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedAlumno(null);
  };

  const handleDownloadCertificate = async () => {
    if (!selectedAlumno || !cursoId) return;

    try {
      await downloadCertificate(selectedAlumno.id, cursoId);
      handleMenuClose();
    } catch (error) {
      setError('Error al descargar el certificado');
      handleMenuClose();
    }
  };

  const handleDeleteCertificate = async () => {
    if (!selectedAlumno) return;

    if (window.confirm(`¿Está seguro de eliminar el certificado de ${selectedAlumno.nombreApellido}?`)) {
      try {
        await apiService.delete(`/api/app/certificados/${selectedAlumno.id}/${cursoId}`);
        handleMenuClose();
        loadData(); // Recargar datos para actualizar la UI
      } catch (error) {
        setError('Error al eliminar el certificado');
        handleMenuClose();
      }
    }
  };

  const handleGenerateCertificates = async () => {
    if (selectedAlumnos.length === 0) return;

    try {
      setError('');
      await apiService.post(`/api/app/certificados/generate`, {
        cursoId,
        alumnoIds: selectedAlumnos
      });

      setSelectedAlumnos([]); // Limpiar selección
      loadData(); // Recargar datos para mostrar certificados generados
    } catch (error) {
      setError('Error al generar los certificados');
    }
  };

  const handleDownloadSelected = async () => {
    if (selectedForDownload.length === 0 || !cursoId) return;

    try {
      setError('');
      const requests = selectedForDownload.map(alumnoId => ({
        alumnoId,
        cursoId
      }));

      await downloadMultipleCertificates(requests);
      setSelectedForDownload([]); // Limpiar selección después de descargar

    } catch (error) {
      setError('Error al descargar los certificados');
    }
  };

  const selectableAlumnos = alumnos.filter(alumno => !alumno.certificado);
  const downloadableAlumnos = alumnos.filter(alumno => alumno.certificado);

  const isAllSelected = selectedAlumnos.length === selectableAlumnos.length && selectableAlumnos.length > 0;
  const isIndeterminate = selectedAlumnos.length > 0 && selectedAlumnos.length < selectableAlumnos.length;

  const isAllSelectedForDownload = selectedForDownload.length === downloadableAlumnos.length && downloadableAlumnos.length > 0;
  const isIndeterminateForDownload = selectedForDownload.length > 0 && selectedForDownload.length < downloadableAlumnos.length;

  if (loading) return <Layout><Typography>Cargando...</Typography></Layout>;

  return (
    <Layout>
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/otec/cursos')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1" sx={{ flexGrow: 1 }}>
            Certificados - {curso?.nombreReferencia}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              color="primary"
              startIcon={pdfLoading ? <CircularProgress size={20} /> : <Download />}
              onClick={() => handleDownloadSelected()}
              disabled={selectedForDownload.length === 0 || pdfLoading}
              sx={{ color: 'primary.main', borderColor: 'primary.main' }}
            >
              {pdfLoading ? 'Descargando...' : `Descargar Seleccionados (${selectedForDownload.length})`}
            </Button>
            <Button
              variant="contained"
              color="success"
              startIcon={<Download />}
              onClick={() => handleGenerateCertificates()}
              disabled={selectedAlumnos.length === 0}
            >
              Generar Certificados ({selectedAlumnos.length})
            </Button>
          </Box>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {pdfError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {pdfError}
          </Alert>
        )}

        <Typography variant="h6" gutterBottom>
          Alumnos del Curso
        </Typography>
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell padding="checkbox" align="center">
                  <Checkbox
                    checked={isAllSelected}
                    indeterminate={isIndeterminate}
                    onChange={handleSelectAll}
                    disabled={selectableAlumnos.length === 0}
                    sx={{ color: 'success.main', '&.Mui-checked': { color: 'success.main' } }}
                  />
                </TableCell>
                <TableCell padding="checkbox" align="center">
                  <Checkbox
                    checked={isAllSelectedForDownload}
                    indeterminate={isIndeterminateForDownload}
                    onChange={handleSelectAllForDownload}
                    disabled={downloadableAlumnos.length === 0}
                    sx={{ color: 'primary.main', '&.Mui-checked': { color: 'primary.main' } }}
                  />
                </TableCell>
                <TableCell>Alumno</TableCell>
                <TableCell>RUT</TableCell>
                <TableCell>Fecha Inscripción</TableCell>
                <TableCell align="center">Acciones</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {alumnos.map((alumno) => (
                <TableRow key={alumno.id}>
                  <TableCell padding="checkbox" align="center">
                    <Checkbox
                      checked={selectedAlumnos.includes(alumno.id)}
                      onChange={() => handleSelectAlumno(alumno.id)}
                      disabled={alumno.certificado}
                      sx={{ color: 'success.main', '&.Mui-checked': { color: 'success.main' } }}
                    />
                  </TableCell>
                  <TableCell padding="checkbox" align="center">
                    <Checkbox
                      checked={selectedForDownload.includes(alumno.id)}
                      onChange={() => handleSelectForDownload(alumno.id)}
                      disabled={!alumno.certificado}
                      sx={{ color: 'primary.main', '&.Mui-checked': { color: 'primary.main' } }}
                    />
                  </TableCell>
                  <TableCell>{getAlumnoNombre(alumno)}</TableCell>
                  <TableCell>{getAlumnoRut(alumno)}</TableCell>
                  <TableCell>
                    {new Date(alumno.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell align="center">
                    {alumno.certificado && (
                      <IconButton
                        size="small"
                        onClick={(e) => handleMenuOpen(e, alumno)}
                      >
                        <MoreVert />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Menú contextual */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={pdfLoading ? undefined : handleMenuClose}
        >
          <MenuItem onClick={handleDownloadCertificate} disabled={pdfLoading}>
            {pdfLoading ? (
              <CircularProgress size={20} sx={{ mr: 1 }} />
            ) : (
              <Download sx={{ mr: 1 }} />
            )}
            {pdfLoading ? 'Descargando...' : 'Descargar Certificado'}
          </MenuItem>
          <MenuItem onClick={handleDeleteCertificate} disabled={pdfLoading}>
            <Delete sx={{ mr: 1 }} />
            Eliminar Certificado
          </MenuItem>
        </Menu>
      </Box>
    </Layout>
  );
}