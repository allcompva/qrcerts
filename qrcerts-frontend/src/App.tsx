import React from 'react';
import { HashRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/Auth/ProtectedRoute';
import { AdminLogin } from './components/Auth/AdminLogin';
import { OtecLogin } from './components/Auth/OtecLogin';
import { Dashboard } from './components/Dashboard/Dashboard';

// Admin components
import { OtecList } from './components/Admin/OtecList';
import { OtecUserList } from './components/Admin/OtecUserList';
import { QuotaManager } from './components/Admin/QuotaManager';

// OTEC components
import { CursoList } from './components/OTEC/CursoList';
import { CursoWizard } from './components/OTEC/CursoWizard';
import { AlumnosList } from './components/OTEC/AlumnosList';
import { CertificadosList } from './components/OTEC/CertificadosList';
import { PlantillasList } from './components/OTEC/PlantillasList';
import { PlantillaWizard } from './components/OTEC/PlantillaWizard';
import { MoodleConfigPage } from './components/OTEC/MoodleConfigPage';
import { MoodleImportWizard } from './components/OTEC/MoodleImportWizard';

// Public components
import { ValidarCertificado } from './components/ValidarCertificado';

const theme = createTheme({
  palette: {
    primary: {
      main: '#009688',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <Router>
          <Routes>
            {/* Public routes */}
            <Route path="/admin/login" element={<AdminLogin />} />
            <Route path="/otec/login" element={<OtecLogin />} />
            <Route path="/validar" element={<ValidarCertificado />} />
            {/* Legacy route - redirect to otec login */}
            <Route path="/login" element={<OtecLogin />} />

            {/* Protected routes */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              }
            />

            {/* Admin routes */}
            <Route
              path="/admin/otecs"
              element={
                <ProtectedRoute requiredRole="admin">
                  <OtecList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/users"
              element={
                <ProtectedRoute requiredRole="admin">
                  <OtecUserList />
                </ProtectedRoute>
              }
            />

            <Route
              path="/admin/otecs/:otecId/quota"
              element={
                <ProtectedRoute requiredRole="admin">
                  <QuotaManager />
                </ProtectedRoute>
              }
            />

            {/* OTEC routes */}
            <Route
              path="/otec/cursos"
              element={
                <ProtectedRoute requiredRole="otec">
                  <CursoList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/cursos/nuevo"
              element={
                <ProtectedRoute requiredRole="otec">
                  <CursoWizard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/cursos/editar/:cursoId"
              element={
                <ProtectedRoute requiredRole="otec">
                  <CursoWizard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/cursos/:cursoId/alumnos"
              element={
                <ProtectedRoute requiredRole="otec">
                  <AlumnosList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/cursos/:cursoId/certificados"
              element={
                <ProtectedRoute requiredRole="otec">
                  <CertificadosList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/plantillas"
              element={
                <ProtectedRoute requiredRole="otec">
                  <PlantillasList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/plantillas/nueva"
              element={
                <ProtectedRoute requiredRole="otec">
                  <PlantillaWizard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/plantillas/editar/:plantillaId"
              element={
                <ProtectedRoute requiredRole="otec">
                  <PlantillaWizard />
                </ProtectedRoute>
              }
            />

            {/* Moodle Integration routes */}
            <Route
              path="/otec/moodle"
              element={
                <ProtectedRoute requiredRole="otec">
                  <MoodleConfigPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/otec/cursos/moodle"
              element={
                <ProtectedRoute requiredRole="otec">
                  <MoodleImportWizard />
                </ProtectedRoute>
              }
            />

            {/* Redirects */}
            <Route path="/admin" element={<Navigate to="/admin/otecs" replace />} />
            <Route path="/otec" element={<Navigate to="/otec/cursos" replace />} />
            <Route path="/" element={<Navigate to="/otec/login" replace />} />
          </Routes>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
