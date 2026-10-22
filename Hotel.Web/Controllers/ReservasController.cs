using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _reservas;

    public ReservasController(ReservaRepositorio reservas)
    {
        _reservas = reservas;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _reservas.ListarAsync());
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso?.ToString("yyyy-MM-dd");
        ViewBag.Salida = salida?.ToString("yyyy-MM-dd");

        if (!ingreso.HasValue && !salida.HasValue)
            return View(new List<Habitacion>());

        if (!ingreso.HasValue || !salida.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Indique la fecha de ingreso y la fecha de salida.");
            return View(new List<Habitacion>());
        }

        if (salida.Value <= ingreso.Value)
        {
            ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la de ingreso.");
            return View(new List<Habitacion>());
        }

        ViewBag.Noches = salida.Value.DayNumber - ingreso.Value.DayNumber;
        ViewBag.Consultado = true;
        return View(await _reservas.ListarDisponiblesAsync(ingreso.Value, salida.Value));
    }

    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        return View(new Reserva());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida,Observaciones")] Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(Reserva.Estado));
        await ValidarAsync(reserva);

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
            return View(reserva);
        }

        await _reservas.CrearAsync(reserva);
        TempData["Mensaje"] = "Reserva registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reserva = await _reservas.ObtenerAsync(id);
        if (reserva == null) return NotFound();

        if (!await _reservas.CancelarAsync(id))
        {
            TempData["Error"] = "Solo se pueden cancelar reservas pendientes o confirmadas.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = $"Reserva de la habitación {reserva.IdHabitacionNavigation.Numero} cancelada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Reserva reserva)
    {
        reserva.Observaciones = string.IsNullOrWhiteSpace(reserva.Observaciones)
            ? null
            : reserva.Observaciones.Trim();

        if (reserva.Observaciones?.Length > 200)
            ModelState.AddModelError(nameof(Reserva.Observaciones), "Las observaciones no pueden superar los 200 caracteres.");

        if (reserva.IdHuesped <= 0)
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "Seleccione un huésped.");
        else if (!await _reservas.ExisteHuespedActivoAsync(reserva.IdHuesped))
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "El huésped seleccionado no existe o está inactivo.");

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var fechasCompletas = reserva.FechaIngreso != default && reserva.FechaSalida != default;

        if (reserva.FechaIngreso == default)
        {
            ModelState.Remove(nameof(Reserva.FechaIngreso));
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso es obligatoria.");
        }
        else if (reserva.FechaIngreso < hoy)
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
        }

        if (reserva.FechaSalida == default)
        {
            ModelState.Remove(nameof(Reserva.FechaSalida));
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida es obligatoria.");
        }
        else if (fechasCompletas && reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la de ingreso.");
        }

        if (reserva.IdHabitacion <= 0)
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "Seleccione una habitación.");
        }
        else if (!await _reservas.ExisteHabitacionActivaAsync(reserva.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación seleccionada no existe o está inactiva.");
        }
        else if (fechasCompletas
            && reserva.FechaSalida > reserva.FechaIngreso
            && await _reservas.EstaReservadaAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango de fechas.");
        }
    }

    private async Task CargarListasAsync(int? idHuesped = null, int? idHabitacion = null)
    {
        var huespedes = await _reservas.ListarHuespedesActivosAsync();
        ViewBag.Huespedes = new SelectList(
            huespedes.Select(h => new { h.IdHuesped, Texto = $"{h.Nombres} {h.Apellidos}" }),
            "IdHuesped",
            "Texto",
            idHuesped);

        var habitaciones = await _reservas.ListarHabitacionesActivasAsync();
        ViewBag.Habitaciones = new SelectList(
            habitaciones.Select(h => new { h.IdHabitacion, Texto = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre}" }),
            "IdHabitacion",
            "Texto",
            idHabitacion);
    }
}
