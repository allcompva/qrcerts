public class PdfJob
{
    public Guid Id { get; set; }
    public Guid OtecId { get; set; }
    public Guid CourseId { get; set; }
    public string AlumnoIdsCsv { get; set; } = "";
    public int TipoJob { get; set; }   // 0=batch(zip), 1=single
    public int Estado { get; set; }    // 0=pending,1=running,2=done,3=error
    public int Prioridad { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}

