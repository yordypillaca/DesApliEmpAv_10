using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class Habitacion
{
    public int IdHabitacion { get; set; }

    public string Numero { get; set; } = null!;

    public int Piso { get; set; }

    public int IdTipoHabitacion { get; set; }

    public bool Activo { get; set; }

    public virtual TipoHabitacion IdTipoHabitacionNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
