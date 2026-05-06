using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using QRCerts.Api.Services;
using QRCerts.Api.Services.Moodle;
using System.Text;

#nullable enable

var builder = WebApplication.CreateBuilder(args);

// =======================
// EPPlus
// =======================
ExcelPackage.License.SetNonCommercialPersonal("Sistemas");
ExcelPackage.License.SetNonCommercialOrganization("Municipalidad de Villa Allende");

// =======================
// PathBase
// =======================
string? pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
    builder.Services.AddRouting(o => o.AppendTrailingSlash = false);

// =======================
// MVC + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Bearer token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =======================
// AUTH / JWT
// =======================
var jwtSettings = builder.Configuration.GetSection("Jwt");
string secretKey = jwtSettings["Key"] ?? "DefaultSecretKeyForDevelopment123456789";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "QRCerts",
            ValidAudience = jwtSettings["Audience"] ?? "QRCerts",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());

    options.AddPolicy("docker", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// =======================
// SERVICIOS SCOPED
// =======================
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAlumnosService, AlumnosService>();
builder.Services.AddScoped<IConversionService, ConversionService>();
builder.Services.AddScoped<ICertificadosService, CertificadosService>();
builder.Services.AddScoped<ICursosService, CursosService>();
builder.Services.AddScoped<IEmisionService, EmisionService>();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IOtecService, OtecService>();
builder.Services.AddScoped<IOtecUserService, OtecUserService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<IVerifyService, VerifyService>();
builder.Services.AddScoped<IRegistroAlumnosService, RegistroAlumnosService>();
builder.Services.AddScoped<IPlantillaCertificadosService, PlantillaCertificadosService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddSingleton<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<ILibreOfficeConversionService, LibreOfficeConversionService>();

// =======================
// MOODLE INTEGRATION
// =======================
builder.Services.AddHttpClient<IMoodleApiService, MoodleApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// =======================
// JOBS (CLAVE)
// =======================
builder.Services.AddScoped<IPdfJobRepository, PdfJobRepository>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();

// Worker (Singleton por diseño)
builder.Services.AddHostedService<PdfJobWorker>();

// =======================
// BUILD
// =======================
var app = builder.Build();

// =======================
// PIPELINE
// =======================
if (!string.IsNullOrWhiteSpace(pathBase))
    app.UsePathBase(pathBase);

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

var corsPolicy = app.Environment.IsDevelopment() ? "default" : "docker";
app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =======================
// SPA FALLBACK (React)
// =======================
// Landing page en la raíz
app.MapGet("/", async context =>
{
    var landingPath = Path.Combine(app.Environment.WebRootPath, "landing.html");
    if (File.Exists(landingPath))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(landingPath);
        return;
    }
    context.Response.Redirect("/app/");
});

app.MapFallback(async context =>
{
    var path = context.Request.Path.Value ?? "";

    if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { message = "Endpoint no encontrado" });
        return;
    }

    // SPA fallback para rutas bajo /app/
    var indexPath = Path.Combine(app.Environment.WebRootPath, "app", "index.html");
    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(indexPath);
        return;
    }

    context.Response.StatusCode = 404;
    await context.Response.WriteAsJsonAsync(new { message = "Aplicación no encontrada" });
});

app.Run();
