import React, { useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Button,
  CardActions,
} from '@mui/material';
import {
  Business,
  School,
  CardMembership,
  People,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Layout } from '../Layout/Layout';

export function Dashboard() {
  const { user, isAdmin } = useAuth();
  const navigate = useNavigate();

  // Auto-redirect OTEC users directly to courses
  useEffect(() => {
    if (!isAdmin && user) {
      navigate('/otec/cursos', { replace: true });
    }
  }, [isAdmin, user, navigate]);

  const adminCards = [
    {
      title: 'Gestión de OTECs',
      description: 'Crear, editar y administrar organizaciones de capacitación',
      icon: <Business sx={{ fontSize: 40 }} />,
      action: () => navigate('/admin/otecs'),
      color: '#009688',
    },
    {
      title: 'Usuarios OTEC',
      description: 'Administrar usuarios y permisos de las OTECs',
      icon: <People sx={{ fontSize: 40 }} />,
      action: () => navigate('/admin/users'),
      color: '#2e7d32',
    },
  ];

  // OTEC users are redirected automatically, so no cards needed
  const cards = isAdmin ? adminCards : [];

  return (
    <Layout>
      <Box>
        <Typography variant="h4" component="h1" gutterBottom>
          Panel de Control
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          Bienvenido, {user?.username}
          {user?.otec && ` - ${user.otec.nombre}`}
        </Typography>

        <Grid container spacing={3}>
          {cards.map((card, index) => (
            <Grid item xs={12} sm={6} md={4} key={index}>
              <Card
                sx={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  transition: 'transform 0.2s',
                  '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: 3,
                  },
                }}
              >
                <CardContent sx={{ flexGrow: 1, textAlign: 'center' }}>
                  <Box
                    sx={{
                      color: card.color,
                      mb: 2,
                    }}
                  >
                    {card.icon}
                  </Box>
                  <Typography gutterBottom variant="h6" component="h2">
                    {card.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {card.description}
                  </Typography>
                </CardContent>
                <CardActions sx={{ justifyContent: 'center', pb: 2 }}>
                  <Button
                    variant="contained"
                    onClick={card.action}
                    sx={{ backgroundColor: card.color }}
                  >
                    Acceder
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Box>
    </Layout>
  );
}