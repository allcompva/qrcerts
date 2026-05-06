export interface Otec {
  id: string;
  nombre: string;
  slug: string;
  estado: number;
  moodleHabilitado?: boolean;
  createdAt: string;
}

export interface OtecUser {
  id: string;
  otecId: string;
  nombreApellido: string;
  rut: string;
  email: string;
  username: string;
  estado: number;
  createdAt: string;
  otec?: Otec;
}

export interface Curso {
  id: string;
  otecId: string;
  nombreReferencia: string;
  baseUrlPublica: string;
  qrDestino: number;
  fondoPath: string;
  layoutJson?: string;
  estado: number;
  isFondoBloqueado: boolean;
  isBaseUrlBloqueada: boolean;
  isLayoutBloqueado: boolean;
  createdAt: string;
  updatedAt: string;
  otec?: Otec;
  footer_1?: string;
  footer_2?: string;
  nombre_visualizar_certificado?: string;
  certificate_type?: string;
  contenidoHtml?: string;
  footerHtml?: string;
  vencimiento?: string;
  plantillaId?: string;
}

export interface Alumno {
  id: string;
  otecId: string;
  nombreApellido: string;
  rut: string;
  createdAt: string;
  calificacion?: string;
  observaciones?: string;
  certificado_otorgado?: string;
  motivo_entrega?: string;
  certificado: boolean;
  parametros?: string; // JSON string with dynamic parameters for LIBRE certificate types
  otec?: Otec;
}

export interface PlantillaCertificado {
  id: string;
  nombre: string;
  tipo?: 'HORIZONTAL' | 'VERTICAL';
  docxPath?: string;
  variables?: string; // JSON array de variables extraídas (frontend original)
  estado?: number;
  createdAt?: string;
  updatedAt?: string;
  // Campos del DAL (backend actual)
  contenido?: string; // Variables extraídas (JSON o separadas por comas)
  contenido_cursos?: string; // JSON array de variables de curso
  contenido_alumnos?: string; // JSON array de variables de alumno
  path_docx?: string;
  id_otec?: string;
}

export type CertificateType = 'HORIZONTAL' | 'VERTICAL';

export interface Inscripcion {
  id: string;
  cursoId: string;
  alumnoId: string;
  cursoTexto: string;
  texto1: string;
  texto2: string;
  createdAt: string;
  curso?: Curso;
  alumno?: Alumno;
}

export interface Certificado {
  id: string;
  cursoId: string;
  alumnoId: string;
  inscripcionId: string;
  code: string;
  pdfFilename: string;
  issuedAt: string;
  estado: number;
  curso?: Curso;
  alumno?: Alumno;
  inscripcion?: Inscripcion;
}

export interface EmisionLote {
  id: string;
  cursoId: string;
  total: number;
  generados: number;
  estado: number;
  startedAt: string;
  finishedAt?: string;
  log?: string;
  curso?: Curso;
}

export interface User {
  id: string;
  username: string;
  role: 'admin' | 'otec';
  otecId?: string;
  otec?: Otec;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}