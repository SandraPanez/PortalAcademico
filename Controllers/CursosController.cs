using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, TimeSpan? horarioInicio, TimeSpan? horarioFin)
    {
        var cursos = _context.Cursos.Where(c => c.Activo);

        if (!string.IsNullOrEmpty(nombre))
            cursos = cursos.Where(c => c.Nombre.Contains(nombre));

        if (creditosMin.HasValue)
            cursos = cursos.Where(c => c.Creditos >= creditosMin.Value);

        if (creditosMax.HasValue)
            cursos = cursos.Where(c => c.Creditos <= creditosMax.Value);

        if (horarioInicio.HasValue)
            cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value);

        if (horarioFin.HasValue)
            cursos = cursos.Where(c => c.HorarioFin <= horarioFin.Value);

        ViewBag.Nombre = nombre;
        ViewBag.CreditosMin = creditosMin;
        ViewBag.CreditosMax = creditosMax;
        ViewBag.HorarioInicio = horarioInicio?.ToString(@"hh\:mm");
        ViewBag.HorarioFin = horarioFin?.ToString(@"hh\:mm");

        return View(await cursos.ToListAsync());
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        return View(curso);
    }
}