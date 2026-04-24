using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Security.Claims;

namespace PortalAcademico.Controllers;

[Authorize]
public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatriculasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Inscribirse(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpPost]
    public async Task<IActionResult> Inscribirse(int id, string confirm)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var curso = await _context.Cursos.FindAsync(id);

        if (curso == null) return NotFound();

        // Validar cupo
        var matriculados = await _context.Matriculas
            .CountAsync(m => m.CursoId == id && m.Estado != EstadoMatricula.Cancelada);

        if (matriculados >= curso.CupoMaximo)
        {
            ViewBag.Error = "El curso ya no tiene cupo disponible.";
            return View(curso);
        }

        // Validar que no esté ya matriculado
        var yaMatriculado = await _context.Matriculas
            .AnyAsync(m => m.CursoId == id && m.UsuarioId == usuarioId && m.Estado != EstadoMatricula.Cancelada);

        if (yaMatriculado)
        {
            ViewBag.Error = "Ya estás matriculado en este curso.";
            return View(curso);
        }

        // Validar solapamiento de horario
        var cursosMatriculados = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == usuarioId && m.Estado != EstadoMatricula.Cancelada)
            .Select(m => m.Curso)
            .ToListAsync();

        var solapamiento = cursosMatriculados.Any(c =>
            c.HorarioInicio < curso.HorarioFin && c.HorarioFin > curso.HorarioInicio);

        if (solapamiento)
        {
            ViewBag.Error = "Este curso se solapa con el horario de otro curso en el que estás matriculado.";
            return View(curso);
        }

        // Crear matrícula
        var matricula = new Matricula
        {
            CursoId = id,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.UtcNow,
            Estado = EstadoMatricula.Pendiente
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        ViewBag.Exito = "¡Te has inscrito exitosamente! Tu matrícula está en estado Pendiente.";
        return View(curso);
    }
}