// Decompiled with JetBrains decompiler
// Type: QRCerts.Api.Services.AlumnosService
// Assembly: QRCerts.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 727CB6D4-E29D-47B1-9E4A-93BB4836AB5B
// Assembly location: C:\Users\allco\Downloads\QRCerts_full\src\QRCerts.Api\bin\Release\net8.0\publish\QRCerts.Api.dll

using OfficeOpenXml;
using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public class AlumnosService : IAlumnosService
  {
        public async Task<bool> VerificaDuplicado(string rut, string idCurso)
        {
            try
            {
                return DAL.AlumnosDAL.VerificaDuplicado(rut, idCurso);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    private static readonly string[] Headers = new string[2]
    {
      "Nombre",
      "RUT"
    };

    public async Task<List<Alumno>> GetByOtecIdAsync(
      Guid otecId,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<List<Alumno>>((Func<List<Alumno>>) (() => AlumnosDAL.GetByOtecId(otecId)));
    }

    public async Task<List<Alumno>> GetByCursoIdAsync(
      Guid cursoId,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<List<Alumno>>((Func<List<Alumno>>) (() => AlumnosDAL.GetByCursoId(cursoId)));
    }

    public async Task<Alumno?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<Alumno>((Func<Alumno>) (() => AlumnosDAL.GetById(id)));
    }

    public async Task<Alumno> CreateAsync(
      QRCerts.Api.DTOs.CreateAlumnoRequest request,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<Alumno>((Func<Alumno>) (() =>
      {
        Guid guid = string.IsNullOrEmpty(request.OtecId) ? Guid.Empty : Guid.Parse(request.OtecId);
        Alumno async = new Alumno()
        {
          OtecId = guid,
          NombreApellido = request.NombreApellido,
          RUT = request.rut,
        };
        async.Id = AlumnosDAL.Insert(async);
        return async;
      }));
    }

    public async Task<Alumno> UpdateAsync(
      Guid id,
      UpdateAlumnoRequest request,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<Alumno>((Func<Alumno>) (() =>
      {
        Alumno byId = AlumnosDAL.GetById(id);
        if (byId == null)
          throw new ArgumentException("Alumno no encontrado", nameof (id));
        if (request.NombreApellido != null)
          byId.NombreApellido = request.NombreApellido;
        if (request.RUT != null)
          byId.RUT = request.RUT;
        AlumnosDAL.Update(byId);
        return byId;
      }));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<bool>((Func<bool>) (() =>
      {
        Alumno byId = AlumnosDAL.GetById(id);
        if (byId == null)
          return false;
        AlumnosDAL.Delete(byId);
        return true;
      }));
    }

    public async Task<byte[]> GenerateTemplateAsync()
    {
      ExcelPackage.LicenseContext = new LicenseContext?(LicenseContext.NonCommercial);
      byte[] asByteArray;
      using (ExcelPackage excelPackage = new ExcelPackage())
      {
        ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Alumnos");
        for (int index = 0; index < AlumnosService.Headers.Length; ++index)
        {
          excelWorksheet.Cells[1, index + 1].Value = (object) AlumnosService.Headers[index];
          excelWorksheet.Column(index + 1).Width = 35.0;
        }
        excelWorksheet.Cells[1, 1, 1, AlumnosService.Headers.Length].Style.Font.Bold = true;
        asByteArray = excelPackage.GetAsByteArray();
      }
      return asByteArray;
    }

    public async Task<PreviewResult> PreviewImportAsync(
      Guid cursoId,
      Stream fileStream,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      ExcelPackage.LicenseContext = new LicenseContext?(LicenseContext.NonCommercial);
      PreviewResult previewResult = new PreviewResult();
      using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
      {
        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
        if (worksheet == null)
        {
          previewResult.Errores.Add("El XLSX no tiene hojas.");
          return previewResult;
        }
        for (int index = 0; index < AlumnosService.Headers.Length; ++index)
        {
          if (!string.Equals(worksheet.Cells[1, index + 1].Text?.Trim(), AlumnosService.Headers[index], StringComparison.OrdinalIgnoreCase))
          {
            previewResult.Errores.Add("Encabezados inválidos. Se esperaba: " + string.Join(" | ", AlumnosService.Headers));
            return previewResult;
          }
        }
        int Row = 2;
        int num = 0;
        do
        {
          string str1 = worksheet.Cells[Row, 1].Text?.Trim();
          string str2 = worksheet.Cells[Row, 2].Text?.Trim();
          if (!string.IsNullOrWhiteSpace(str1) || !string.IsNullOrWhiteSpace(str2))
          {
            previewResult.Preview.Add(new PreviewRow()
            {
              Fila = Row,
              Nombre = str1,
              RUT = str2
            });
            ++Row;
            ++num;
          }
          else
            break;
        }
        while (num < 50);
        previewResult.Ok = true;
        previewResult.TotalFilas = Row - 2;
        return previewResult;
      }
    }

    public async Task<ImportResult> CommitImportAsync(
      Guid cursoId,
      Stream fileStream,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<ImportResult>((Func<ImportResult>) (() =>
      {
        ImportResult importResult1 = new ImportResult();
        Curso byId = CursosDAL.GetById(cursoId);
        if (byId == null)
        {
          importResult1.Message = "Curso no encontrado.";
          return importResult1;
        }
          // Quitar ESTA línea vieja (obsoleta):
          // ExcelPackage.LicenseContext = new LicenseContext?(LicenseContext.NonCommercial);

          using (var excelPackage = new ExcelPackage(fileStream))
          {
              var worksheet = excelPackage.Workbook.Worksheets[0];

              // Validación de encabezados
              for (int i = 0; i < AlumnosService.Headers.Length; i++)
              {
                  var cell = worksheet.Cells[1, i + 1].GetValue<string>()?.Trim();
                  if (!string.Equals(cell, AlumnosService.Headers[i], StringComparison.OrdinalIgnoreCase))
                  {
                      importResult1.Message = "Encabezados inválidos. Se esperaba: " +
                                              string.Join(" | ", AlumnosService.Headers);
                      return importResult1;
                  }
              }

              int inserted = 0;
              int omitted = 0;
              int row = 2;

              while (true)
              {
                  var nombre = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
                  var rut = worksheet.Cells[row, 2].GetValue<string>()?.Trim();

                  // Fin de datos
                  if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(rut))
                      break;

                  // Si no hay RUT, omitir fila
                  if (string.IsNullOrWhiteSpace(rut))
                  {
                      omitted++;
                      row++;
                      continue;
                  }

                  var otecId = byId.OtecId;

                  // Verificar si ya está inscrito en este curso
                  if (AlumnosDAL.VerificaDuplicado(rut, cursoId.ToString()))
                  {
                      omitted++;
                      row++;
                      continue;
                  }

                  // Buscar alumno existente por RUT + OtecId para no duplicar
                  var alumnoExistente = AlumnosDAL.GetByOtecIdAndRut(otecId, rut);
                  Guid alumnoId;

                  if (alumnoExistente != null)
                  {
                      // Alumno ya existe, reusar su Id
                      alumnoId = alumnoExistente.Id;
                      // Actualizar nombre si cambió
                      if (!string.IsNullOrWhiteSpace(nombre) && alumnoExistente.NombreApellido != nombre)
                      {
                          alumnoExistente.NombreApellido = nombre;
                          AlumnosDAL.Update(alumnoExistente);
                      }
                  }
                  else
                  {
                      // Crear nuevo alumno solo si no existe
                      alumnoId = AlumnosDAL.Insert(new Alumno
                      {
                          OtecId = otecId,
                          NombreApellido = nombre ?? string.Empty,
                          RUT = rut
                      });
                  }

                  // Crear registro en REGISTRO_ALUMNOS para vincular al curso
                  REGISTRO_ALUMNOS.Insert(new REGISTRO_ALUMNOS
                  {
                      id_alumno = alumnoId,
                      id_curso = cursoId
                  });

                  inserted++;
                  row++;
              }

              importResult1.Ok = true;
              importResult1.Inserted = inserted;
              importResult1.Omitted = omitted;
              importResult1.Message = $"Proceso completado. Insertados: {inserted}, Omitidos: {omitted}";
              return importResult1;
          }

      }));
    }

    async Task<Alumno?> IAlumnosService.GetByIdCertAsync(Guid idAlumno, Guid idCurso)
    {
      return await Task.Run<Alumno>((Func<Alumno>) (() => AlumnosDAL.GetByIdCert(idAlumno, idCurso)));
    }
  }
}
