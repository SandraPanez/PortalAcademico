using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Text.Json;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CursosController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, TimeSpan? horarioInicio, TimeSpan? horarioFin)
    {
        List<Curso> cursos;

        // Solo usamos cache si no hay filtros
        if (nombre == null && creditosMin == null && creditosMax == null && horarioInicio == null && horarioFin == null)
        {
            var cached = await _cache.GetStringAsync("cursos_activos");
            if (cached != null)
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cached)!;
            }
            else
            {
                cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();
                await _cache.SetStringAsync("cursos_activos", JsonSerializer.Serialize(cursos),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                    });
            }
        }
        else
        {
            var query = _context.Cursos.Where(c => c.Activo);

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre));
            if (creditosMin.HasValue)
                query = query.Where(c => c.Creditos >= creditosMin.Value);
            if (creditosMax.HasValue)
                query = query.Where(c => c.Creditos <= creditosMax.Value);
            if (horarioInicio.HasValue)
                query = query.Where(c => c.HorarioInicio >= horarioInicio.Value);
            if (horarioFin.HasValue)
                query = query.Where(c => c.HorarioFin <= horarioFin.Value);

            cursos = await query.ToListAsync();
        }

        ViewBag.Nombre = nombre;
        ViewBag.CreditosMin = creditosMin;
        ViewBag.CreditosMax = creditosMax;
        ViewBag.HorarioInicio = horarioInicio?.ToString(@"hh\:mm");
        ViewBag.HorarioFin = horarioFin?.ToString(@"hh\:mm");

        return View(cursos);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        // Guardar último curso visitado en sesión
        HttpContext.Session.SetString("UltimoCursoId", id.ToString());
        HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

        return View(curso);
    }
}