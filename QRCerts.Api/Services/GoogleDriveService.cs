using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Upload;

namespace QRCerts.Api.Services
{
    public interface IGoogleDriveService
    {
        bool IsAuthenticated { get; }
        Task AuthenticateAsync(CancellationToken ct = default);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath, string mimeType = "application/pdf", CancellationToken ct = default);
        Task<Stream> DownloadFileAsync(string fileId, CancellationToken ct = default);
        Task MoveToHistoricoAsync(string fileId, string otecSlug, CancellationToken ct = default);
    }

    public class GoogleDriveService : IGoogleDriveService
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveFile };
        private const string AppName = "QRCerts";
        private const string RootFolderName = "QRCerts";

        private DriveService? _service;
        private readonly string _credentialsPath;
        private readonly string _tokenPath;

        // Cache de folder IDs para no buscar cada vez
        private readonly Dictionary<string, string> _folderCache = new();

        public bool IsAuthenticated => _service != null;

        public GoogleDriveService(IWebHostEnvironment env)
        {
            var basePath = Path.Combine(env.ContentRootPath, "credentials");
            _credentialsPath = Path.Combine(basePath, "credentials.json");
            _tokenPath = basePath; // FileDataStore usa la carpeta
        }

        public async Task AuthenticateAsync(CancellationToken ct = default)
        {
            if (_service != null) return;

            if (!File.Exists(_credentialsPath))
                throw new FileNotFoundException($"No se encontró credentials.json en {_credentialsPath}");

            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "qrcerts-user",
                ct,
                new FileDataStore(_tokenPath, true));

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath, string mimeType = "application/pdf", CancellationToken ct = default)
        {
            await EnsureAuthenticated(ct);

            var folderId = await EnsureFolderPath(folderPath, ct);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { folderId }
            };

            var request = _service!.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id";

            var result = await request.UploadAsync(ct);
            if (result.Status != UploadStatus.Completed)
                throw new Exception($"Error subiendo archivo a Drive: {result.Exception?.Message}");

            var file = request.ResponseBody;

            // Hacer público (anyone con link puede ver)
            await _service.Permissions.Create(
                new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "reader"
                },
                file.Id
            ).ExecuteAsync(ct);

            return file.Id;
        }

        public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken ct = default)
        {
            await EnsureAuthenticated(ct);

            var memStream = new MemoryStream();
            var request = _service!.Files.Get(fileId);
            await request.DownloadAsync(memStream, ct);
            memStream.Position = 0;
            return memStream;
        }

        public async Task MoveToHistoricoAsync(string fileId, string otecSlug, CancellationToken ct = default)
        {
            await EnsureAuthenticated(ct);
            try
            {
                // Obtener el archivo para saber su padre actual
                var getRequest = _service!.Files.Get(fileId);
                getRequest.Fields = "id, parents";
                var file = await getRequest.ExecuteAsync(ct);

                // Asegurar carpeta Historico/{otecSlug}
                var historicoFolderId = await EnsureFolderPath($"Historico/{otecSlug}", ct);

                // Mover: quitar del padre actual, agregar a Historico
                var updateRequest = _service.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
                updateRequest.AddParents = historicoFolderId;
                if (file.Parents != null && file.Parents.Count > 0)
                    updateRequest.RemoveParents = string.Join(",", file.Parents);
                await updateRequest.ExecuteAsync(ct);
            }
            catch { /* Si no existe o falla, ignorar */ }
        }

        private async Task EnsureAuthenticated(CancellationToken ct)
        {
            if (_service == null)
                await AuthenticateAsync(ct);
        }

        /// <summary>
        /// Asegura que exista la jerarquía de carpetas en Drive.
        /// Ej: "Certificados/mi-otec" crea QRCerts/Certificados/mi-otec
        /// </summary>
        private async Task<string> EnsureFolderPath(string path, CancellationToken ct)
        {
            // Siempre empezar desde la carpeta raíz QRCerts
            var rootId = await EnsureFolder(RootFolderName, null, ct);

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var parentId = rootId;

            foreach (var part in parts)
            {
                parentId = await EnsureFolder(part, parentId, ct);
            }

            return parentId;
        }

        private async Task<string> EnsureFolder(string name, string? parentId, CancellationToken ct)
        {
            var cacheKey = $"{parentId ?? "root"}:{name}";
            if (_folderCache.TryGetValue(cacheKey, out var cached))
                return cached;

            // Buscar si ya existe
            var query = $"name='{name}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
            if (parentId != null)
                query += $" and '{parentId}' in parents";

            var list = _service!.Files.List();
            list.Q = query;
            list.Fields = "files(id, name)";
            list.Spaces = "drive";

            var result = await list.ExecuteAsync(ct);
            if (result.Files.Count > 0)
            {
                _folderCache[cacheKey] = result.Files[0].Id;
                return result.Files[0].Id;
            }

            // Crear
            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder",
                Parents = parentId != null ? new List<string> { parentId } : null
            };

            var folder = await _service.Files.Create(folderMetadata).ExecuteAsync(ct);
            _folderCache[cacheKey] = folder.Id;
            return folder.Id;
        }
    }
}
