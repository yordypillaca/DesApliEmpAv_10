using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _habitaciones;

    public HabitacionesController(HabitacionRepositorio habitaciones)
    {
        _habitaciones = habitaciones;
    }

    public async Task<IActionResult> Index(int? idTipo)
    {
        await CargarTiposAsync(idTipo);
        return View(await _habitaciones.ListarAsync(idTipo));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _habitaciones.ObtenerAsync(id.Value);
        if (habitacion == null) return NotFound();

        return View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Piso = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));
        await ValidarAsync(habitacion);

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _habitaciones.CrearAsync(habitacion);
        TempData["Mensaje"] = $"Habitación {habitacion.Numero} registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _habitaciones.ObtenerAsync(id.Value);
        if (habitacion == null) return NotFound();

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return NotFound();

        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));
        await ValidarAsync(habitacion);

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        if (!await _habitaciones.ActualizarAsync(habitacion))
            return NotFound();

        TempData["Mensaje"] = $"Habitación {habitacion.Numero} actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var habitacion = await _habitaciones.ObtenerAsync(id.Value);
        if (habitacion == null) return NotFound();

        return View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var habitacion = await _habitaciones.ObtenerAsync(id);
        if (habitacion == null) return NotFound();

        await _habitaciones.DesactivarAsync(id);
        TempData["Mensaje"] = $"Habitación {habitacion.Numero} eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Habitacion habitacion)
    {
        habitacion.Numero = (habitacion.Numero ?? "").Trim();

        if (string.IsNullOrWhiteSpace(habitacion.Numero))
        {
            ModelState.Remove(nameof(Habitacion.Numero));
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número es obligatorio.");
        }
        else if (habitacion.Numero.Length > 10)
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número no puede superar los 10 caracteres.");
        }
        else if (await _habitaciones.ExisteNumeroAsync(habitacion.Numero, habitacion.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con este número.");
        }

        if (habitacion.Piso < 1)
        {
            ModelState.Remove(nameof(Habitacion.Piso));
            ModelState.AddModelError(nameof(Habitacion.Piso), "El piso debe ser mayor o igual a 1.");
        }

        if (habitacion.IdTipoHabitacion <= 0)
            ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "Seleccione un tipo de habitación.");
        else if (!await _habitaciones.ExisteTipoActivoAsync(habitacion.IdTipoHabitacion))
            ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "El tipo de habitación seleccionado no existe o está inactivo.");
    }

    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        var tipos = await _habitaciones.ListarTiposActivosAsync();
        ViewBag.Tipos = new SelectList(tipos, nameof(TipoHabitacion.IdTipoHabitacion), nameof(TipoHabitacion.Nombre), seleccionado);
    }
}
