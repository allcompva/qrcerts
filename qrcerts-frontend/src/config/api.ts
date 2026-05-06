// En Docker VITE_API_BASE_URL="" significa usar URL relativa (mismo origen)
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export const API_ENDPOINTS = {
  // Auth
  
  ADMIN_LOGIN: '/api/admin/auth/login',
  OTEC_LOGIN: '/api/otec/auth/login',

  // Admin endpoints
  OTECS: '/api/admin/otecs',
  OTEC_USERS: '/api/admin/otec-users',

  // OTEC endpoints
  CURSOS: '/api/app/cursos',
  ALUMNOS: '/api/app/alumnos',
  INSCRIPCIONES: '/api/app/inscripciones',
  CERTIFICADOS: '/api/app/certificados',
  EMISIONES: '/api/app/emisiones',

  // File upload
  UPLOAD_EXCEL: '/api/app/alumnos/upload',

  // Moodle integration
  MOODLE_CONFIG: '/api/app/moodle/config',
  MOODLE_TEST_CONNECTION: '/api/app/moodle/test-connection',
  MOODLE_COURSES: '/api/app/moodle/courses',
  MOODLE_IMPORT: '/api/app/moodle/import',
  MOODLE_IMPORT_HISTORY: '/api/app/moodle/import/history'
} as const;