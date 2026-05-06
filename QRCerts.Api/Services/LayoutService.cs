using QRCerts.Api.DAL;
using QRCerts.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace QRCerts.Api.Services
{
  public class LayoutService : ILayoutService
  {
    public async Task<LayoutResponse> GetLayoutAsync(
      Guid cursoId,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<LayoutResponse>((Func<LayoutResponse>) (() =>
      {
        Curso byId = CursosDAL.GetById(cursoId);
        if (byId == null)
          return new LayoutResponse()
          {
            LayoutJson = (string) null,
            IsLocked = false
          };
        return new LayoutResponse()
        {
          LayoutJson = byId.LayoutJson,
          IsLocked = false
        };
      }));
    }

    public async Task<bool> UpdateLayoutAsync(
      Guid cursoId,
      UpdateLayoutRequest request,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return await Task.Run<bool>((Func<bool>) (() =>
      {
        Curso byId = CursosDAL.GetById(cursoId);
        if (byId == null)
          return false;
        byId.LayoutJson = request.LayoutJson;
        byId.UpdatedAt = DateTime.UtcNow;
        CursosDAL.Update(byId);
        return true;
      }));
    }
  }
}
