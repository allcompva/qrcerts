using System;
using System.IO;
namespace QRCerts.Api.Controllers
{
    public class DocumentConverter
    {
        /// <summary>
        /// Lee un archivo de la ruta especificada y lo convierte a una cadena Base64.
        /// </summary>
        /// <param name="filePath">La ruta completa al archivo .docx.</param>
        /// <returns>La cadena Base64 resultante, o null si falla.</returns>
        public static string FileToBase64(string filePath)
        {
            // 1. Verificar si el archivo existe
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: El archivo no fue encontrado en la ruta: {filePath}");
                return null;
            }

            try
            {
                // 2. Leer todos los bytes del archivo .docx
                // La función ReadAllBytes maneja internamente la apertura, lectura y cierre del archivo.
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // 3. Convertir el arreglo de bytes a una cadena Base64
                string base64String = Convert.ToBase64String(fileBytes);

                return base64String;
            }
            catch (IOException ex)
            {
                // Manejo de errores de acceso a archivos (ej. archivo en uso o sin permisos)
                Console.WriteLine($"Error de I/O al leer el archivo: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Otros errores de conversión
                Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
                return null;
            }
        }

        public static void Main(string[] args)
        {
            // **IMPORTANTE:** Reemplaza esta ruta por la ubicación real de tu archivo .docx.
            string docxPath = "C:\\ruta\\a\\tu\\documento.docx";

            string base64Docx = FileToBase64(docxPath);

            if (base64Docx != null)
            {
                Console.WriteLine("Conversión exitosa. La cadena Base64 (los primeros 50 caracteres):");
                Console.WriteLine(base64Docx.Substring(0, Math.Min(50, base64Docx.Length)) + "...");
                // La longitud total puede ser verificada:
                // Console.WriteLine($"Longitud total: {base64Docx.Length}"); 
            }
        }
    }
}