using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CoordinadorController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // Lista de cursos
    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos.ToListAsync();
        return View(cursos);
    }

    // Crear curso GET
    public IActionResult Crear() => View();

    // Crear curso POST
    [HttpPost]
    public async Task<IActionResult> Crear(Curso curso)
    {
        if (curso.HorarioFin <= curso.HorarioInicio)
            ModelState.AddModelError("", "El horario fin debe ser mayor al horario inicio.");

        if (!ModelState.IsValid) return View(curso);

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cursos_activos");
        return RedirectToAction(nameof(Index));
    }

    // Editar curso GET
    public async Task<IActionResult> Editar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    // Editar curso POST
    [HttpPost]
    public async Task<IActionResult> Editar(Curso curso)
    {
        if (curso.HorarioFin <= curso.HorarioInicio)
            ModelState.AddModelError("", "El horario fin debe ser mayor al horario inicio.");

        if (!ModelState.IsValid) return View(curso);

        _context.Cursos.Update(curso);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cursos_activos");
        return RedirectToAction(nameof(Index));
    }

    // Desactivar curso
    public async Task<IActionResult> Desactivar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        curso.Activo = false;
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cursos_activos");
        return RedirectToAction(nameof(Index));
    }

    // Matrículas por curso
    public async Task<IActionResult> Matriculas(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        var matriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.CursoId == id)
            .ToListAsync();

        ViewBag.Curso = curso;
        return View(matriculas);
    }

    // Confirmar matrícula
    public async Task<IActionResult> Confirmar(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null) return NotFound();
        matricula.Estado = EstadoMatricula.Confirmada;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }

    // Cancelar matrícula
    public async Task<IActionResult> Cancelar(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null) return NotFound();
        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }
}