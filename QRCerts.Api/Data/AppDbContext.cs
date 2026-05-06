using Microsoft.EntityFrameworkCore;
using QRCerts.Api.Models;
namespace QRCerts.Api.Data {
  /// <summary>DbContext principal con índices y unicidades según especificación.</summary>
  public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Otec> Otecs => Set<Otec>();
    public DbSet<OtecUser> OtecUsers => Set<OtecUser>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<EmisionLote> Emisiones => Set<EmisionLote>();

    // Moodle Integration
    public DbSet<MoodleConfig> MoodleConfigs => Set<MoodleConfig>();
    public DbSet<MoodleCursoImportado> MoodleCursosImportados => Set<MoodleCursoImportado>();
    public DbSet<MoodleFieldMapping> MoodleFieldMappings => Set<MoodleFieldMapping>();

    protected override void OnModelCreating(ModelBuilder mb){
      base.OnModelCreating(mb);
      mb.Entity<AdminUser>().HasIndex(x=>x.Username).IsUnique();
      mb.Entity<Otec>().HasIndex(x=>x.Slug).IsUnique();
      mb.Entity<OtecUser>().HasIndex(x=>x.Username).IsUnique();
      mb.Entity<Alumno>().HasIndex(x=> new { x.OtecId, x.RUT });
      mb.Entity<Certificado>().HasIndex(x=>x.Code).IsUnique();
      mb.Entity<Certificado>().HasIndex(x=>x.PdfFilename).IsUnique();

      mb.Entity<Certificado>()
        .HasOne<Curso>()
        .WithMany()
        .HasForeignKey(c => c.CursoId)
        .OnDelete(DeleteBehavior.NoAction);

      mb.Entity<Certificado>()
        .HasOne<Alumno>()
        .WithMany()
        .HasForeignKey(c => c.AlumnoId)
        .OnDelete(DeleteBehavior.NoAction);

      // Moodle Config - una por OTEC
      mb.Entity<MoodleConfig>()
        .HasIndex(x => x.OtecId)
        .IsUnique();

      mb.Entity<MoodleConfig>()
        .HasOne(x => x.Otec)
        .WithMany()
        .HasForeignKey(x => x.OtecId)
        .OnDelete(DeleteBehavior.Cascade);

      // Moodle Curso Importado - único por OTEC + MoodleCourseId
      mb.Entity<MoodleCursoImportado>()
        .HasIndex(x => new { x.OtecId, x.MoodleCourseId })
        .IsUnique();

      mb.Entity<MoodleCursoImportado>()
        .HasOne(x => x.CursoLocal)
        .WithMany()
        .HasForeignKey(x => x.CursoLocalId)
        .OnDelete(DeleteBehavior.SetNull);

      // Moodle Field Mapping - único por Curso + CampoMoodle
      mb.Entity<MoodleFieldMapping>()
        .HasIndex(x => new { x.CursoId, x.CampoMoodle })
        .IsUnique();

      mb.Entity<MoodleFieldMapping>()
        .HasOne(x => x.Curso)
        .WithMany()
        .HasForeignKey(x => x.CursoId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
