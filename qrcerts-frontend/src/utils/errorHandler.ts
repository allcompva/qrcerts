/**
 * Extrae el mensaje de error del backend tal cual viene
 */
export const getBackendError = (error: any): string => {
  // Axios error con response del backend
  if (error.response?.data) {
    const data = error.response.data;
    // Si es string, devolverlo directo
    if (typeof data === 'string') return data;
    // Si es objeto con message
    if (data.message) return data.message;
    // Si es objeto con error
    if (data.error) return data.error;
    // Si es objeto, convertir a JSON
    if (typeof data === 'object') return JSON.stringify(data);
  }
  // Error de fetch con message
  if (error.message) return error.message;
  // Convertir a string como último recurso
  return String(error);
};
