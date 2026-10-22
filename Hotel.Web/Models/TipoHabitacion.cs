using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class TipoHabitacion
{
    public int IdTipoHabitacion { get; set; }

    public string Nombre { get; set; } = null!;

    public int Capacidad { get; set; }

    public decimal PrecioNoche { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();
}
