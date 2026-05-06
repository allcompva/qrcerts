import React from 'react';
import { Box, Container } from '@mui/material';
import { Navbar } from './Navbar';

interface LayoutProps {
  children: React.ReactNode;
}

export function Layout({ children }: LayoutProps) {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Navbar />
      <Container maxWidth="xl" sx={{ mt: 3, mb: 3, flex: 1 }}>
        {children}
      </Container>
    </Box>
  );
}