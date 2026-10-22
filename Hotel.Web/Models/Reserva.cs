using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdHuesped { get; set; }

    public int IdHabitacion { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public DateOnly FechaSalida { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public string? Observaciones { get; set; }

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual Huesped IdHuespedNavigation { get; set; } = null!;
}
