namespace PortalAcademico.Models;

public enum EstadoMatricula { Pendiente, Confirmada, Cancelada }

public class Matricula
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public EstadoMatricula Estado { get; set; }
}