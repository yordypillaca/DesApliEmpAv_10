using System.Diagnostics;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class HomeController : Controller
{
    private readonly HabitacionRepositorio _habitaciones;
    private readonly ReservaRepositorio _reservas;

    public HomeController(HabitacionRepositorio habitaciones, ReservaRepositorio reservas)
    {
        _habitaciones = habitaciones;
        _reservas = reservas;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Habitaciones = await _habitaciones.ContarActivasAsync();
        ViewBag.Reservas = await _reservas.ContarAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
