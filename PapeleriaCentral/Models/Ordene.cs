using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PapeleriaCentral.Models;

public partial class Ordene
{
    public int IdOrden { get; set; }

    public string NumeroOrden { get; set; } = null!;

    public DateOnly FechaOrden { get; set; }

    public string Cliente { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public string MetodoPago { get; set; } = null!;

    public int IdTipoCliente { get; set; }

    public decimal DescuentoAplicado { get; set; }

    public decimal MontoFinal { get; set; }

    public DateOnly FechaEntrega { get; set; }

    [ValidateNever]
    public virtual TiposCliente IdTipoClienteNavigation { get; set; } = null!;
}