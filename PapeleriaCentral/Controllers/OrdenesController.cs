using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PapeleriaCentral.Models;

namespace PapeleriaCentral.Controllers
{
    public class OrdenesController : Controller
    {
        private readonly PapeleriaCentralContext _context;

        public OrdenesController(PapeleriaCentralContext context)
        {
            _context = context;
        }

        // LISTAR Y FILTRAR ÓRDENES
        public async Task<IActionResult> Index(
            int? tipoCliente,
            DateOnly? fechaInicio,
            DateOnly? fechaFin)
        {
            var ordenes = _context.Ordenes
                .Include(o => o.IdTipoClienteNavigation)
                .AsQueryable();

            if (tipoCliente.HasValue)
            {
                ordenes = ordenes.Where(o =>
                    o.IdTipoCliente == tipoCliente.Value);
            }

            if (fechaInicio.HasValue)
            {
                ordenes = ordenes.Where(o =>
                    o.FechaOrden >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                ordenes = ordenes.Where(o =>
                    o.FechaOrden <= fechaFin.Value);
            }

            ViewBag.TiposCliente = new SelectList(
                await _context.TiposClientes.ToListAsync(),
                "IdTipoCliente",
                "Nombre",
                tipoCliente
            );

            ViewBag.TipoSeleccionado = tipoCliente;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(await ordenes
                .OrderByDescending(o => o.FechaOrden)
                .ThenByDescending(o => o.IdOrden)
                .ToListAsync());
        }

        // FORMULARIO PARA CREAR
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarTiposCliente();

            var orden = new Ordene
            {
                FechaOrden = DateOnly.FromDateTime(DateTime.Today)
            };

            return View(orden);
        }

        // GUARDAR NUEVA ORDEN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Ordene orden)
        {
            ValidarOrden(orden);

            bool numeroDuplicado = await _context.Ordenes
                .AnyAsync(o => o.NumeroOrden == orden.NumeroOrden);

            if (numeroDuplicado)
            {
                ModelState.AddModelError(
                    "NumeroOrden",
                    "El número de orden ya existe."
                );
            }

            var tipoCliente = await _context.TiposClientes
                .FirstOrDefaultAsync(t =>
                    t.IdTipoCliente == orden.IdTipoCliente);

            if (tipoCliente == null)
            {
                ModelState.AddModelError(
                    "IdTipoCliente",
                    "Seleccione un tipo de cliente válido."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarTiposCliente(orden.IdTipoCliente);
                return View(orden);
            }

            CalcularDatosOrden(orden, tipoCliente!);

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Orden registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // FORMULARIO PARA EDITAR
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var orden = await _context.Ordenes.FindAsync(id);

            if (orden == null)
            {
                return NotFound();
            }

            await CargarTiposCliente(orden.IdTipoCliente);

            return View(orden);
        }

        // ACTUALIZAR ORDEN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Ordene orden)
        {
            if (id != orden.IdOrden)
            {
                return NotFound();
            }

            ValidarOrden(orden);

            bool numeroDuplicado = await _context.Ordenes
                .AnyAsync(o =>
                    o.NumeroOrden == orden.NumeroOrden &&
                    o.IdOrden != orden.IdOrden);

            if (numeroDuplicado)
            {
                ModelState.AddModelError(
                    "NumeroOrden",
                    "El número de orden ya pertenece a otra orden."
                );
            }

            var tipoCliente = await _context.TiposClientes
                .FirstOrDefaultAsync(t =>
                    t.IdTipoCliente == orden.IdTipoCliente);

            if (tipoCliente == null)
            {
                ModelState.AddModelError(
                    "IdTipoCliente",
                    "Seleccione un tipo de cliente válido."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarTiposCliente(orden.IdTipoCliente);
                return View(orden);
            }

            var ordenBD = await _context.Ordenes
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (ordenBD == null)
            {
                return NotFound();
            }

            ordenBD.NumeroOrden = orden.NumeroOrden;
            ordenBD.FechaOrden = orden.FechaOrden;
            ordenBD.Cliente = orden.Cliente;
            ordenBD.MontoTotal = orden.MontoTotal;
            ordenBD.MetodoPago = orden.MetodoPago;
            ordenBD.IdTipoCliente = orden.IdTipoCliente;

            CalcularDatosOrden(ordenBD, tipoCliente!);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Orden actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // CONFIRMACIÓN PARA ELIMINAR
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.IdTipoClienteNavigation)
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }

        // ELIMINAR DEFINITIVAMENTE
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var orden = await _context.Ordenes.FindAsync(id);

            if (orden == null)
            {
                return NotFound();
            }

            _context.Ordenes.Remove(orden);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Orden eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // VALIDACIONES DEL BACKEND
        private void ValidarOrden(Ordene orden)
        {
            if (string.IsNullOrWhiteSpace(orden.NumeroOrden))
            {
                ModelState.AddModelError(
                    "NumeroOrden",
                    "El número de orden es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(orden.Cliente))
            {
                ModelState.AddModelError(
                    "Cliente",
                    "El nombre del cliente es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(orden.MetodoPago))
            {
                ModelState.AddModelError(
                    "MetodoPago",
                    "Seleccione un método de pago."
                );
            }

            if (orden.MontoTotal <= 0)
            {
                ModelState.AddModelError(
                    "MontoTotal",
                    "El monto total debe ser mayor que cero."
                );
            }

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

            if (orden.FechaOrden > hoy)
            {
                ModelState.AddModelError(
                    "FechaOrden",
                    "La fecha de la orden no puede ser futura."
                );
            }

            if (orden.IdTipoCliente <= 0)
            {
                ModelState.AddModelError(
                    "IdTipoCliente",
                    "Seleccione el tipo de cliente."
                );
            }
        }

        // CALCULAR DESCUENTO, MONTO FINAL Y FECHA DE ENTREGA
        private void CalcularDatosOrden(
            Ordene orden,
            TiposCliente tipoCliente)
        {
            orden.DescuentoAplicado = tipoCliente.Descuento;

            decimal descuento = orden.MontoTotal *
                                (tipoCliente.Descuento / 100m);

            orden.MontoFinal = orden.MontoTotal - descuento;

            if (tipoCliente.Nombre.Equals(
                "Mayorista",
                StringComparison.OrdinalIgnoreCase))
            {
                orden.FechaEntrega = orden.FechaOrden.AddDays(2);
            }
            else if (tipoCliente.Nombre.Equals(
                "Preferencial",
                StringComparison.OrdinalIgnoreCase))
            {
                orden.FechaEntrega = orden.FechaOrden.AddDays(3);
            }
            else
            {
                orden.FechaEntrega =
                    AgregarDiasHabiles(orden.FechaOrden, 5);
            }
        }

        // CALCULAR DÍAS HÁBILES PARA CLIENTE REGULAR
        private DateOnly AgregarDiasHabiles(
            DateOnly fecha,
            int dias)
        {
            int agregados = 0;
            DateOnly resultado = fecha;

            while (agregados < dias)
            {
                resultado = resultado.AddDays(1);

                if (resultado.DayOfWeek != DayOfWeek.Saturday &&
                    resultado.DayOfWeek != DayOfWeek.Sunday)
                {
                    agregados++;
                }
            }

            return resultado;
        }

        // CARGAR COMBO DE TIPOS DE CLIENTE
        private async Task CargarTiposCliente(
            int? seleccionado = null)
        {
            ViewBag.TiposCliente = new SelectList(
                await _context.TiposClientes
                    .OrderBy(t => t.Nombre)
                    .ToListAsync(),
                "IdTipoCliente",
                "Nombre",
                seleccionado
            );
        }
    }
}