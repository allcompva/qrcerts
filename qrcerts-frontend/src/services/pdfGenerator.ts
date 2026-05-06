import jsPDF from 'jspdf';
import QRCode from 'qrcode';
import { API_BASE_URL } from '../config/api';

export interface CertificateData {
  alumno: {
    id: string;
    nombreApellido: string;
    rut: string;
  };
  curso: {
    id: string;
    nombreReferencia: string;
    nombre_visualizar_certificado?: string;
    fondoPath?: string;
    footer_1?: string;
    footer_2?: string;
    certificate_type?: string;
    contenidoHtml?: string;
    footerHtml?: string;
    extra_content?: string;
  };
}

export class CertificatePDFGenerator {
  static async generateCertificate(data: CertificateData): Promise<Blob> {
    // Crear documento PDF en orientación landscape (A4 apaisado)
    const doc = new jsPDF({
      orientation: 'landscape',
      unit: 'mm',
      format: 'a4'
    });

    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    try {
      // 1. Agregar imagen de fondo si existe
      if (data.curso.fondoPath) {
        try {
          // Construir URL completa hacia el backend usando la configuración de la API
          const baseUrl = API_BASE_URL === '/' ? window.location.origin : API_BASE_URL.replace(/\/$/, '');
          // Agregar extensión .png si no tiene extensión
          let fondoPath = data.curso.fondoPath;
          if (!fondoPath.includes('.')) {
            fondoPath = `${fondoPath}.png`;
          }
          const backgroundImageUrl = `${baseUrl}/uploads/images/${fondoPath}`;
          console.log('Attempting to load background image:', backgroundImageUrl);

          // Cargar imagen como base64
          const response = await fetch(backgroundImageUrl);
          if (response.ok) {
            const blob = await response.blob();
            const reader = new FileReader();

            await new Promise((resolve) => {
              reader.onload = () => {
                const imageData = reader.result as string;
                doc.addImage(imageData, 'JPEG', 0, 0, pageWidth, pageHeight);
                console.log('Background image added successfully');
                resolve(true);
              };
              reader.readAsDataURL(blob);
            });
          } else {
            console.warn('Failed to load background image:', response.status);
          }
        } catch (error) {
          console.warn('Error loading background image:', error);
        }
      }

      // 2. Configurar fuente y color
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(0, 0, 0); // Negro


      // 4. Agregar nombre del alumno - Posición fija en el centro con tamaño de certificado_otorgado
      if (data.alumno.nombreApellido?.trim()) {
        doc.setFontSize(20); // Intercambiado: ahora usa el tamaño que tenía certificado_otorgado
        doc.setFont('helvetica', 'bold');
        const nombreUpper = data.alumno.nombreApellido.trim().toUpperCase();
        const nombreWidth = doc.getTextWidth(nombreUpper);
        const nombreX = (pageWidth - nombreWidth) / 2;
        const nombreY = 90; // Posición fija 100mm desde arriba (centro)
        doc.text(nombreUpper, nombreX, nombreY);
        console.log('Added nombreApellido:', nombreUpper, 'at position:', nombreX, nombreY);
      } else {
        console.log('nombreApellido is empty or undefined:', data.alumno.nombreApellido);
      }
      // 4. Agregar nombre del alumno - Posición fija en el centro con tamaño de certificado_otorgado
      if (data.alumno.rut?.trim()) {
        doc.setFontSize(18); // Intercambiado: ahora usa el tamaño que tenía certificado_otorgado
        doc.setFont('helvetica', 'bold');
        const nombreUpper = 'RUT: ' + data.alumno.rut.trim().toUpperCase();
        const nombreWidth = doc.getTextWidth(nombreUpper);
        const nombreX = (pageWidth - nombreWidth) / 2;
        const nombreY = 100; // Posición fija 100mm desde arriba (centro)
        doc.text(nombreUpper, nombreX, nombreY);
        console.log('Added nombreApellido:', nombreUpper, 'at position:', nombreX, nombreY);
      } else {
        console.log('nombreApellido is empty or undefined:', data.alumno.rut);
      }


      // 6. Agregar nombre del curso - 15px debajo del motivo_entrega (aprox 5.3mm)
      if (data.curso.nombre_visualizar_certificado?.trim()) {
        doc.setFontSize(20); // Mismo tamaño que motivo_entrega (14px)
        doc.setFont('helvetica', 'bold');
        const nombreCurso = data.curso.nombre_visualizar_certificado.trim().toUpperCase();
        const cursoWidth = doc.getTextWidth(nombreCurso);
        const cursoX = (pageWidth - cursoWidth) / 2;
        const cursoY = 125; // 107 + 15px (aprox 5.3mm) = 122mm
        doc.text(nombreCurso, cursoX, cursoY);
        console.log('✅ Added curso nombre:', nombreCurso, 'at position:', cursoX, cursoY);
      } else {
        console.log('❌ curso nombre_visualizar_certificado is empty/undefined/null:', data.curso.nombre_visualizar_certificado);
      }

      // 6. Generar y agregar código QR - ESQUINA SUPERIOR DERECHA
      try {
        const qrData = btoa(`${data.alumno.id},${data.curso.id}`); // Base64 encode
        const qrUrl = `https://certificadosqr.site/#/validar?data=${qrData}`;

        // Generar QR como dataURL
        const qrDataURL = await QRCode.toDataURL(qrUrl, {
          width: 200,
          margin: 1,
          color: {
            dark: '#000000',
            light: '#FFFFFF'
          }
        });

        // Agregar QR en esquina SUPERIOR derecha
        const qrSize = 30; // 30mm
        const qrX = pageWidth - qrSize - 10; // 10mm margen derecho
        const qrY = 20; // 20mm desde arriba (esquina superior)
        doc.addImage(qrDataURL, 'PNG', qrX, qrY, qrSize, qrSize);
        console.log('Added QR code at position:', qrX, qrY);
      } catch (qrError) {
        console.warn('Error generating QR code:', qrError);
      }

      // 7. Agregar footer_1 y footer_2 dinámicos al pie de página
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(11);
      doc.setTextColor(255, 255, 255); // Blanco

      // footer_1 - primera línea del footer
      if (data.curso.footer_1?.trim()) {
        const footer1Text = data.curso.footer_1.trim();
        const footer1Width = doc.getTextWidth(footer1Text);
        const footer1X = (pageWidth - footer1Width) / 2;
        const footer1Y = pageHeight - 14; // 14mm desde abajo
        doc.text(footer1Text, footer1X, footer1Y);
        console.log('Added footer_1:', footer1Text, 'at position:', footer1X, footer1Y);
      }

      // footer_2 - segunda línea del footer (más abajo)
      if (data.curso.footer_2?.trim()) {
        const footer2Text = data.curso.footer_2.trim();
        const footer2Width = doc.getTextWidth(footer2Text);
        const footer2X = (pageWidth - footer2Width) / 2;
        const footer2Y = pageHeight - 6; // 6mm desde abajo
        doc.text(footer2Text, footer2X, footer2Y);
        console.log('Added footer_2:', footer2Text, 'at position:', footer2X, footer2Y);
      }

      // Agregar páginas adicionales si hay extra_content
      if (data.curso.extra_content && data.curso.extra_content.trim()) {
        await this.addExtraContentPages(doc, data.curso.extra_content, 'landscape');
      }

      // Retornar como Blob
      return doc.output('blob');
    } catch (error) {
      console.error('Error generating PDF:', error);

      // PDF de error
      const errorDoc = new jsPDF();
      errorDoc.setFontSize(16);
      errorDoc.text('Error generando certificado', 20, 30);
      errorDoc.setFontSize(12);
      errorDoc.text(`Alumno: ${data.alumno.nombreApellido}`, 20, 50);
      errorDoc.text(`Curso: ${data.curso.nombre_visualizar_certificado}`, 20, 65);
      errorDoc.text(`Error: ${error}`, 20, 80);

      return errorDoc.output('blob');
    }
  }

  static async generateCertificateHorizontalLibre(data: CertificateData): Promise<Blob> {
    // Crear documento PDF en orientación landscape (A4 apaisado)
    const doc = new jsPDF({
      orientation: 'landscape',
      unit: 'mm',
      format: 'a4'
    });

    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    try {
      // 1. Agregar imagen de fondo si existe
      if (data.curso.fondoPath) {
        try {
          const baseUrl = API_BASE_URL === '/' ? window.location.origin : API_BASE_URL.replace(/\/$/, '');
          // Agregar extensión .png si no tiene extensión
          let fondoPath = data.curso.fondoPath;
          if (!fondoPath.includes('.')) {
            fondoPath = `${fondoPath}.png`;
          }
          const backgroundImageUrl = `${baseUrl}/uploads/images/${fondoPath}`;

          const response = await fetch(backgroundImageUrl);
          if (response.ok) {
            const blob = await response.blob();
            const reader = new FileReader();

            await new Promise((resolve) => {
              reader.onload = () => {
                const imageData = reader.result as string;
                doc.addImage(imageData, 'JPEG', 0, 0, pageWidth, pageHeight);
                resolve(true);
              };
              reader.readAsDataURL(blob);
            });
          }
        } catch (error) {
          console.warn('Error loading background image:', error);
        }
      }

      // 2. Agregar contenido HTML usando html2canvas
      if (data.curso.contenidoHtml) {
        const margin = 20; // 20mm de margen en todos los lados
        const contentWidth = pageWidth - (2 * margin);
        const contentHeight = pageHeight - (2 * margin);

        // Crear contenedor temporal para renderizar HTML
        const container = document.createElement('div');
        container.style.position = 'fixed';
        container.style.left = '-10000px';
        container.style.top = '-10000px';
        container.style.width = `${contentWidth * 3.7795}px`; // Convertir mm a px
        container.style.fontFamily = 'Helvetica, Arial, sans-serif';
        container.innerHTML = data.curso.contenidoHtml;
        document.body.appendChild(container);

        try {
          const { default: html2canvas } = await import('html2canvas');
          const canvas = await html2canvas(container, {
            scale: 2,
            backgroundColor: null,
            useCORS: true
          });

          const imgData = canvas.toDataURL('image/png');
          const imgWidth = contentWidth;
          const imgHeight = (canvas.height * contentWidth) / canvas.width;

          doc.addImage(imgData, 'PNG', margin, margin, imgWidth, Math.min(imgHeight, contentHeight));
        } finally {
          document.body.removeChild(container);
        }
      }

      // 3. Generar y agregar código QR
      try {
        const qrData = btoa(`${data.alumno.id},${data.curso.id}`);
        const qrUrl = `https://certificadosqr.site/#/validar?data=${qrData}`;

        const qrDataURL = await QRCode.toDataURL(qrUrl, {
          width: 200,
          margin: 1,
          color: {
            dark: '#000000',
            light: '#FFFFFF'
          }
        });

        const qrSize = 30;
        const qrX = pageWidth - qrSize - 10;
        const qrY = 20;
        doc.addImage(qrDataURL, 'PNG', qrX, qrY, qrSize, qrSize);
      } catch (qrError) {
        console.warn('Error generating QR code:', qrError);
      }

      // Agregar páginas adicionales si hay extra_content
      if (data.curso.extra_content && data.curso.extra_content.trim()) {
        await this.addExtraContentPages(doc, data.curso.extra_content, 'landscape');
      }

      return doc.output('blob');
    } catch (error) {
      console.error('Error generating PDF:', error);

      const errorDoc = new jsPDF();
      errorDoc.setFontSize(16);
      errorDoc.text('Error generando certificado', 20, 30);
      return errorDoc.output('blob');
    }
  }

  static generateFileName(cursoNombre: string, alumnoRut: string): string {
    // Generar slug del nombre del curso
    const cursoSlug = cursoNombre
      .toLowerCase()
      .replace(/[^a-z0-9\s-]/g, '') // Remover caracteres especiales
      .replace(/\s+/g, '-') // Reemplazar espacios con guiones
      .replace(/-+/g, '-') // Reemplazar múltiples guiones con uno solo
      .trim()
      .replace(/^-+|-+$/g, ''); // Remover guiones al inicio y final

    // Limpiar RUT
    const rutLimpio = alumnoRut.replace(/[.\-\s]/g, '');

    return `${cursoSlug}_${rutLimpio}.pdf`;
  }

  static downloadPDF(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  private static async addExtraContentPages(
    doc: jsPDF,
    extraContent: string,
    orientation: 'landscape' | 'portrait'
  ): Promise<void> {
    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();
    const margin = 20; // 20mm uniform margins
    const contentWidth = pageWidth - (2 * margin);
    const contentHeight = pageHeight - (2 * margin);

    // Create container for rendering HTML
    const container = document.createElement('div');
    container.style.position = 'fixed';
    container.style.left = '-10000px';
    container.style.top = '-10000px';
    container.style.width = `${contentWidth * 3.7795}px`; // mm to px
    container.style.fontFamily = 'Helvetica, Arial, sans-serif';
    container.style.fontSize = '12px';
    container.style.lineHeight = '1.4';
    container.innerHTML = extraContent;
    document.body.appendChild(container);

    try {
      const { default: html2canvas } = await import('html2canvas');
      const canvas = await html2canvas(container, {
        scale: 2,
        backgroundColor: '#FFFFFF',
        useCORS: true
      });

      const imgData = canvas.toDataURL('image/png');
      const imgWidth = contentWidth;
      const imgHeight = (canvas.height * contentWidth) / canvas.width;

      // Calculate how many pages we need
      let remainingHeight = imgHeight;
      let currentY = 0;

      while (remainingHeight > 0) {
        // Add new page
        doc.addPage();

        // Calculate height to render on this page
        const heightOnThisPage = Math.min(remainingHeight, contentHeight);

        // Calculate source position in the canvas
        const sourceY = currentY;
        const sourceHeight = (heightOnThisPage / contentWidth) * canvas.width;

        // Create a cropped canvas for this page
        const pageCanvas = document.createElement('canvas');
        pageCanvas.width = canvas.width;
        pageCanvas.height = sourceHeight;
        const pageCtx = pageCanvas.getContext('2d');

        if (pageCtx) {
          pageCtx.drawImage(
            canvas,
            0, (sourceY / contentWidth) * canvas.width, // source x, y
            canvas.width, sourceHeight, // source width, height
            0, 0, // dest x, y
            canvas.width, sourceHeight // dest width, height
          );

          const pageImgData = pageCanvas.toDataURL('image/png');
          doc.addImage(pageImgData, 'PNG', margin, margin, imgWidth, heightOnThisPage);
        }

        currentY += heightOnThisPage;
        remainingHeight -= heightOnThisPage;
      }
    } finally {
      document.body.removeChild(container);
    }
  }
}