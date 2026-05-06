import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Box,
  Menu,
  MenuItem,
  IconButton,
  Chip,
  Tooltip,
} from '@mui/material';
import { AccountCircle, ExitToApp, School } from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../../services/api';

interface QuotaStatus {
  quotaActivo: boolean;
  disponibles: number;
  total: number;
  fechaExpiracion: string | null;
  diasRestantes: number;
  porcentajeUsado: number;
  nivelAlerta: 'green' | 'yellow' | 'red';
}

const alertColors: Record<string, { bg: string; text: string }> = {
  green: { bg: '#e8f5e9', text: '#2e7d32' },
  yellow: { bg: '#fff8e1', text: '#f57f17' },
  red: { bg: '#ffebee', text: '#c62828' },
};

export function Navbar() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [quota, setQuota] = React.useState<QuotaStatus | null>(null);

  React.useEffect(() => {
    if (user?.role === 'otec') {
      apiService.get<QuotaStatus>('/api/app/quota/status')
        .then(setQuota)
        .catch(() => {});
    }
  }, [user]);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
    handleClose();
  };

  const navigateToModule = (path: string) => {
    navigate(path);
  };

  const formatQuotaLabel = (q: QuotaStatus): string => {
    if (q.diasRestantes <= 0) return `${q.disponibles} cert. — Vencido`;
    if (q.disponibles <= 0) return `Sin saldo — ${q.diasRestantes}d`;
    return `${q.disponibles} cert. — ${q.diasRestantes}d`;
  };

  const formatQuotaTooltip = (q: QuotaStatus): string => {
    const usado = `${q.total - q.disponibles} de ${q.total} usados`;
    const vence = q.fechaExpiracion
      ? `Vence: ${new Date(q.fechaExpiracion).toLocaleDateString()}`
      : '';
    return `${usado}. ${vence}`;
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography
          variant="h6"
          component="div"
          sx={{
            flexGrow: 1,
            cursor: 'pointer',
            '&:hover': {
              opacity: 0.8
            }
          }}
          onClick={() => navigate('/dashboard')}
        >
          QRCerts Platform
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {isAdmin && (
            <Button
              color="inherit"
              onClick={() => navigateToModule('/admin')}
            >
              Administración
            </Button>
          )}

          {user?.role === 'otec' && (
            <>
              <Button
                color="inherit"
                onClick={() => navigateToModule('/otec')}
              >
                Mis Cursos
              </Button>
              {user?.otec?.moodleHabilitado && (
                <Button
                  color="inherit"
                  onClick={() => navigateToModule('/otec/moodle')}
                  startIcon={<School />}
                >
                  Moodle
                </Button>
              )}
            </>
          )}

          <Button
            color="inherit"
            href={user?.role === 'admin' ? '/manual-admin.html' : '/manual-otec.html'}
            target="_blank"
            size="small"
            sx={{ fontSize: '0.85rem' }}
          >
            Ayuda
          </Button>

          {quota?.quotaActivo && (
            <Tooltip title={formatQuotaTooltip(quota)} arrow>
              <Chip
                label={formatQuotaLabel(quota)}
                size="small"
                sx={{
                  backgroundColor: alertColors[quota.nivelAlerta].bg,
                  color: alertColors[quota.nivelAlerta].text,
                  fontWeight: 600,
                  fontSize: '0.75rem',
                }}
              />
            </Tooltip>
          )}

          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Typography variant="body2" sx={{ mr: 1 }}>
              {user?.username}
            </Typography>
            <IconButton
              size="large"
              aria-label="account of current user"
              aria-controls="menu-appbar"
              aria-haspopup="true"
              onClick={handleMenu}
              color="inherit"
            >
              <AccountCircle />
            </IconButton>
            <Menu
              id="menu-appbar"
              anchorEl={anchorEl}
              anchorOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
              keepMounted
              transformOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
              open={Boolean(anchorEl)}
              onClose={handleClose}
            >
              <MenuItem onClick={handleLogout}>
                <ExitToApp sx={{ mr: 1 }} />
                Cerrar Sesión
              </MenuItem>
            </Menu>
          </Box>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
