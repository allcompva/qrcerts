import { useState } from 'react';
import JSZip from 'jszip';
import { apiService } from '../services/api';
import { API_BASE_URL } from '../config/api';
import { CertificatePDFGenerator, CertificateData } from '../services/pdfGenerator';

interface CertificateApiData {
  alumno: {
    id: string;
    nombreApellido: string;
    rut: string;
    observaciones?: string;
  };
  curso: {
    id: string;
    nombreReferencia: string;
    nombre_visualizar_certificado?: string;
    fondoPath?: string;
    footer_1?: string;
    footer_2?: string;
  };
  fileName: string;
  plantillaId?: string;
}

interface CertificateResult {
  blob?: Blob;
  fileName?: string;
  success: boolean;
  error?: string;
}

// Convierte texto a slug (sin acentos, espacios a guiones, minúsculas)
const generateSlug = (text: string): string => {
  if (!text) return 'sin-nombre';
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '') // quitar acentos
    .replace(/[^a-z0-9\s-]/g, '') // solo letras, números, espacios y guiones
    .replace(/\s+/g, '-') // espacios a guiones
    .replace(/-+/g, '-') // múltiples guiones a uno
    .trim()
    .replace(/^-|-$/g, ''); // quitar guiones al inicio/fin
};

// Genera nombre de archivo: nombreAlumno_nombreCurso.pdf
const generatePdfFileName = (nombreAlumno: string, nombreCurso: string): string => {
  const slugAlumno = generateSlug(nombreAlumno) || 'alumno';
  const slugCurso = generateSlug(nombreCurso) || 'certificado';
  return `${slugAlumno}_${slugCurso}.pdf`;
};

// Verifica si el plantillaId es válido (no vacío, no null, no GUID vacío)
const hasValidPlantilla = (plantillaId: string | undefined | null): boolean => {
  if (!plantillaId) return false;
  if (plantillaId === '') return false;
  if (plantillaId === 'null') return false;
  if (plantillaId === '00000000-0000-0000-0000-000000000000') return false;
  return true;
};

export const useCertificatePDF = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getBaseUrl = (): string => {
    return API_BASE_URL === '/' ? window.location.origin : API_BASE_URL.replace(/\/$/, '');
  };

  const downloadFromBackend = async (cursoId: string, alumnoId: string): Promise<Blob> => {
    const baseUrl = getBaseUrl();
    const pdfUrl = `${baseUrl}/api/app/upload/generate-pdf-by-ids?courseId=${cursoId}&alumnoId=${alumnoId}`;

    const pdfResponse = await fetch(pdfUrl);
    if (!pdfResponse.ok) {
      const errorText = await pdfResponse.text();
      throw new Error(errorText);
    }

    return await pdfResponse.blob();
  };

  const generateInFrontend = async (response: CertificateApiData): Promise<Blob> => {
    const certificateData: CertificateData = {
      alumno: response.alumno,
      curso: {
        id: response.curso.id,
        nombreReferencia: response.curso.nombreReferencia,
        nombre_visualizar_certificado: response.curso.nombre_visualizar_certificado,
        fondoPath: response.curso.fondoPath,
        footer_1: response.curso.footer_1,
        footer_2: response.curso.footer_2
      }
    };

    return await CertificatePDFGenerator.generateCertificate(certificateData);
  };

  const downloadCertificate = async (alumnoId: string, cursoId: string) => {
    try {
      setLoading(true);
      setError(null);

      const response = await apiService.get<CertificateApiData>(
        `/api/app/certificados/data/${alumnoId}/${cursoId}`
      );

      console.log('Certificate API Response:', response);

      let pdfBlob: Blob;

      if (hasValidPlantilla(response.plantillaId)) {
        console.log('Curso con plantilla, generando PDF desde backend...');
        pdfBlob = await downloadFromBackend(cursoId, alumnoId);
      } else {
        console.log('Curso sin plantilla, generando PDF en frontend...');
        pdfBlob = await generateInFrontend(response);
      }

      const fileName = generatePdfFileName(response.alumno.nombreApellido, response.curso.nombre_visualizar_certificado || response.curso.nombreReferencia);
      CertificatePDFGenerator.downloadPDF(pdfBlob, fileName);
    } catch (err: any) {
      // Mostrar error del backend tal cual viene
      const errorMessage = err.response?.data || err.message || String(err);
      setError(errorMessage);
      console.error('Error downloading certificate:', err);
    } finally {
      setLoading(false);
    }
  };

  const downloadMultipleCertificates = async (requests: { alumnoId: string; cursoId: string }[]) => {
    try {
      setLoading(true);
      setError(null);

      if (requests.length === 0) {
        setError('No hay certificados seleccionados');
        return;
      }

      // Consultar el primer certificado para determinar si tiene plantillaId
      const firstRequest = requests[0];
      const firstResponse = await apiService.get<CertificateApiData>(
        `/api/app/certificados/data/${firstRequest.alumnoId}/${firstRequest.cursoId}`
      );

      const cursoNombre = firstResponse.curso.nombre_visualizar_certificado || 'certificados';

      // Si tiene plantillaId válido, descargar cada PDF del backend secuencialmente y crear ZIP en frontend
      if (hasValidPlantilla(firstResponse.plantillaId)) {
        const zip = new JSZip();
        const results: CertificateResult[] = [];

        // Procesar secuencialmente (uno a uno, esperando que termine cada uno)
        for (const { alumnoId, cursoId } of requests) {
          try {
            // Obtener datos del certificado para el nombre del archivo
            const certData = await apiService.get<CertificateApiData>(
              `/api/app/certificados/data/${alumnoId}/${cursoId}`
            );

            // Descargar PDF del backend
            const pdfBlob = await downloadFromBackend(cursoId, alumnoId);

            const fileName = generatePdfFileName(certData.alumno.nombreApellido, certData.curso.nombre_visualizar_certificado || certData.curso.nombreReferencia);
            results.push({
              blob: pdfBlob,
              fileName: fileName,
              success: true
            });
          } catch (err: any) {
            console.error(`Error generating certificate for ${alumnoId}:`, err);
            results.push({
              error: err.response?.data || err.message || String(err),
              success: false
            });
          }
        }

        const successfulResults = results.filter(r => r.success && r.blob && r.fileName);

        successfulResults.forEach((result) => {
          if (result.fileName && result.blob) {
            const fileName = result.fileName.endsWith('.pdf') ? result.fileName : `${result.fileName}.pdf`;
            zip.file(fileName, result.blob);
          }
        });

        if (successfulResults.length > 0) {
          const zipBlob = await zip.generateAsync({ type: 'blob' });
          const zipFileName = `certificados_${cursoNombre.replace(/[^a-zA-Z0-9]/g, '_')}.zip`;
          CertificatePDFGenerator.downloadPDF(zipBlob, zipFileName);
        }

        const failedResults = results.filter(r => !r.success);
        if (failedResults.length > 0) {
          setError(`${successfulResults.length} generados, ${failedResults.length} fallaron. Error: ${failedResults[0].error}`);
        }

      } else {
        // Sin plantilla: generar en frontend y crear ZIP con JSZip
        const zip = new JSZip();

        const results: CertificateResult[] = await Promise.all(
          requests.map(async ({ alumnoId, cursoId }): Promise<CertificateResult> => {
            try {
              const response = await apiService.get<CertificateApiData>(
                `/api/app/certificados/data/${alumnoId}/${cursoId}`
              );

              const pdfBlob = await generateInFrontend(response);
              const fileName = generatePdfFileName(response.alumno.nombreApellido, response.curso.nombre_visualizar_certificado || response.curso.nombreReferencia);

              return {
                blob: pdfBlob,
                fileName: fileName,
                success: true
              };

            } catch (err: any) {
              console.error(`Error generating certificate for ${alumnoId}:`, err);
              return {
                error: err.response?.data || err.message || String(err),
                success: false
              };
            }
          })
        );

        const successfulResults = results.filter(r => r.success && r.blob && r.fileName);

        successfulResults.forEach((result) => {
          if (result.fileName && result.blob) {
            const fileName = result.fileName.endsWith('.pdf') ? result.fileName : `${result.fileName}.pdf`;
            zip.file(fileName, result.blob);
          }
        });

        if (successfulResults.length > 0) {
          const zipBlob = await zip.generateAsync({ type: 'blob' });
          const zipFileName = `certificados_${cursoNombre.replace(/[^a-zA-Z0-9]/g, '_')}.zip`;
          CertificatePDFGenerator.downloadPDF(zipBlob, zipFileName);
        }

        const failedResults = results.filter(r => !r.success);
        if (failedResults.length > 0) {
          setError(`${successfulResults.length} generados, ${failedResults.length} fallaron. Error: ${failedResults[0].error}`);
        }
      }

    } catch (err: any) {
      const errorMessage = err.response?.data || err.message || String(err);
      setError(errorMessage);
      console.error('Error downloading multiple certificates:', err);
    } finally {
      setLoading(false);
    }
  };

  return {
    downloadCertificate,
    downloadMultipleCertificates,
    loading,
    error,
    clearError: () => setError(null)
  };
};
