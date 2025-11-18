// Controllers/ClientesController.cs
using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Lab08_JeanLazarinos.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab08_JeanLazarinos.Services;



namespace Lab08_JeanLazarinos.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StoreLdbContext _context;
    private readonly IExcelService _excelService;

    public ClientesController(IUnitOfWork unitOfWork, StoreLdbContext context,IExcelService excelService)
    {
        _unitOfWork = unitOfWork;   
        _context = context;
        _excelService = excelService;
    }

    // GET: api/Clientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClientes()
    {
        var clientes = await _unitOfWork.ClientRepository.GetAllAsync();
        return Ok(clientes);
    }

    // GET: api/Clientes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetCliente(int id)
    {
        var cliente = await _unitOfWork.ClientRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        return Ok(cliente);
    }

    // POST: api/Clientes
    [HttpPost]
    public async Task<ActionResult<Client>> PostCliente(Client cliente)
    {
        await _unitOfWork.ClientRepository.AddAsync(cliente);
        await _unitOfWork.CompleteAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Clientid }, cliente);
    }

    // PUT: api/Clientes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCliente(int id, Client cliente)
    {
        if (id != cliente.Clientid)
        {
            return BadRequest();
        }
        _unitOfWork.ClientRepository.Update(cliente);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    // DELETE: api/Clientes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _unitOfWork.ClientRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }
        _unitOfWork.ClientRepository.Remove(cliente);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }
    
    // GET: api/Clientes/buscar/Juan
    [HttpGet("buscar/{termino}")]
    public async Task<ActionResult<IEnumerable<Client>>> GetClientesPorTermino(string termino)
    {
        // Usamos el método FindAsync con la expresión.
        var clientes = await _unitOfWork.ClientRepository.FindAsync(
            c => c.Name.ToLower().StartsWith(termino.ToLower())
        );

        if (clientes == null || !clientes.Any())
        {
            return NotFound($"No se encontraron clientes cuyo nombre comience con '{termino}'.");
        }

        return Ok(clientes);
    }
    
    // GET: api/Clientes/con-mas-pedidos
    [HttpGet("con-mas-pedidos")]
    public async Task<IActionResult> GetClienteConMasPedidos()
    {
        // Usamos LINQ para agrupar, contar, ordenar y seleccionar.
        var clienteConMasPedidos = await _context.Orders
            .GroupBy(o => o.Clientid) // 1. Agrupa las órdenes por ClientId
            .Select(g => new           // 2. Proyecta un nuevo objeto anónimo
            {
                ClientId = g.Key,
                TotalPedidos = g.Count()
            })
            .OrderByDescending(x => x.TotalPedidos) // 3. Ordena por el total de pedidos
            .FirstOrDefaultAsync();                 // 4. Toma el primero (el que tiene más)

        if (clienteConMasPedidos == null)
        {
            return NotFound("No se encontraron pedidos para ningún cliente.");
        }

        // Opcional: Obtener los datos completos del cliente
        var clienteInfo = await _context.Clients
            .FirstOrDefaultAsync(c => c.Clientid == clienteConMasPedidos.ClientId);

        return Ok(new 
        {
            Cliente = clienteInfo,
            TotalPedidos = clienteConMasPedidos.TotalPedidos
        });
    }
    
    // GET: api/Clientes/1/productos
    [HttpGet("{clientId}/productos")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductosPorCliente(int clientId)
    {
        // Verificamos si el cliente existe.
        var clientExists = await _context.Clients.AnyAsync(c => c.Clientid == clientId);
        if (!clientExists)
        {
            return NotFound($"No se encontró el cliente con ID {clientId}.");
        }

        // Usamos LINQ para navegar desde las órdenes hasta los productos.
        var productos = await _context.Orders
            .Where(o => o.Clientid == clientId) // 1. Filtra las órdenes del cliente
            .SelectMany(o => o.Orderdetails)  // 2. Aplana la lista de detalles de orden
            .Select(od => od.Product)         // 3. Selecciona el producto de cada detalle
            .Distinct()                       // 4. Elimina duplicados
            .ToListAsync();

        return Ok(productos);
    }
    
    // GET: api/Clientes/5/con-pedidos
    [HttpGet("{id}/con-pedidos")]
    public async Task<ActionResult<ClienteConPedidosDto>> GetClienteConPedidos(int id)
    {
        var clienteConPedidos = await _unitOfWork.ClientRepository.GetClienteConPedidosAsync(id);
    
        if (clienteConPedidos == null)
        {
            return NotFound($"No se encontró el cliente con ID {id}.");
        }

        return Ok(clienteConPedidos);
    }
    
    // GET: api/Clientes/con-total-productos
    [HttpGet("con-total-productos")]
    public async Task<ActionResult<IEnumerable<ClienteConTotalProductosDto>>> GetClientesConTotalProductos()
    {
        var clientesConTotales = await _unitOfWork.ClientRepository.GetClientesConTotalProductosAsync();
    
        if (!clientesConTotales.Any())
        {
            return NotFound("No se encontraron clientes.");
        }

        return Ok(clientesConTotales);
    }
    
    // GET: api/Clientes/ventas-por-cliente
    [HttpGet("ventas-por-cliente")]
    public async Task<ActionResult<IEnumerable<VentasPorClienteDto>>> GetVentasPorCliente(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] decimal? montoMinimo = null)
    {
        var ventasPorCliente = await _unitOfWork.ClientRepository
            .GetVentasPorClienteAsync(fechaInicio, fechaFin, montoMinimo);
    
        if (!ventasPorCliente.Any())
        {
            return NotFound("No se encontraron ventas con los filtros especificados.");
        }

        return Ok(ventasPorCliente);
    }

// GET: api/Clientes/top-clientes/5
    [HttpGet("top-clientes/{top}")]
    public async Task<ActionResult<IEnumerable<VentasPorClienteDto>>> GetTopClientes(int top = 10)
    {
        var ventasPorCliente = await _unitOfWork.ClientRepository
            .GetVentasPorClienteAsync();
    
        var topClientes = ventasPorCliente.Take(top);
    
        if (!topClientes.Any())
        {
            return NotFound("No se encontraron clientes.");
        }

        return Ok(topClientes);
    }
    
    // --- NUEVO ENDPOINT REPORTE 1 ---
    [HttpGet("export/ventas")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportVentasPorCliente()
    {
        // 1. LÓGICA DE QUERY 
        // Asumo que tienes un método en tu repositorio que devuelve este DTO
        var ventasData = await _unitOfWork.ClientRepository.GetVentasPorClienteAsync(); 

        // 2. LÓGICA DE EXCEL 
        var fileBytes = _excelService.GenerateVentasPorClienteReport(ventasData);

        // 3. Devolver el archivo
        string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        string fileName = $"ReporteVentasPorCliente_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(fileBytes, mimeType, fileName);
    }
}