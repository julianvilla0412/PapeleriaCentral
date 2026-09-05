using System;
using System.Collections.Generic;

namespace PapeleriaCentral.Models;

public partial class TiposCliente
{
    public int IdTipoCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Descuento { get; set; }

    public int DiasEntrega { get; set; }

    public virtual ICollection<Ordene> Ordenes { get; set; } = new List<Ordene>();
}
