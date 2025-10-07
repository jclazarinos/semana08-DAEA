// Controllers/ProductosController.cs
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Asegúrate de tener este 'using'
using Lab08_JeanLazarinos.Data; 


namespace Lab08_JeanLazarinos.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StoreLdbContext _context;

    public ProductosController(IUnitOfWork unitOfWork, StoreLdbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    // GET: api/Productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductos()
    {
        var productos = await _unitOfWork.ProductRepository.GetAllAsync();
        return Ok(productos);
    }
    
    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProducto(int id)
    {
        var producto = await _unitOfWork.ProductRepository.GetByIdAsync(id);

        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto);
    }
    
    // POST: api/Productos
    [HttpPost]
    public async Task<ActionResult<Product>> PostProducto(Product producto)
    {
        await _unitOfWork.ProductRepository.AddAsync(producto);
        await _unitOfWork.CompleteAsync();

        // Retorna un 201 Created con la ubicación del nuevo recurso.
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Productid }, producto);
    }

    // PUT: api/Productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, Product producto)
    {
        if (id != producto.Productid)
        {
            return BadRequest("El ID de la ruta no coincide con el ID del producto.");
        }

        _unitOfWork.ProductRepository.Update(producto);

        try
        {
            await _unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            if (await _unitOfWork.ProductRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"No se encontró el producto con ID {id} para actualizar.");
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // Retorna 204 No Content, que significa "OK, pero no hay nada que devolver".
    }
    
    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (producto == null)
        {
            return NotFound($"No se encontró el producto con ID {id} para eliminar.");
        }

        _unitOfWork.ProductRepository.Remove(producto);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
    
    // GET: api/Productos/buscar/precio-mayor-a/20
    [HttpGet("buscar/precio-mayor-a/{precioMinimo}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductosPorPrecioMayorA(decimal precioMinimo)
    {
        // Usamos FindAsync con una expresión LINQ para la comparación numérica.
        var productos = await _unitOfWork.ProductRepository.FindAsync(
            p => p.Price > precioMinimo
        );

        if (productos == null || !productos.Any())
        {
            return NotFound($"No se encontraron productos con un precio mayor a {precioMinimo}.");
        }

        return Ok(productos);
    }
    
    // GET: api/Productos/mas-caro
    [HttpGet("mas-caro")]
    public async Task<ActionResult<Product>> GetProductoMasCaro()
    {
        // Usamos LINQ para ordenar por precio y tomar el primero.
        var productoMasCaro = await _context.Products
            .OrderByDescending(p => p.Price) // 1. Ordena de mayor a menor por precio
            .FirstOrDefaultAsync();          // 2. Toma el primer elemento de la lista

        if (productoMasCaro == null)
        {
            return NotFound("No se encontraron productos.");
        }

        return Ok(productoMasCaro);
    }
    
    // GET: api/Productos/precio-promedio
    [HttpGet("precio-promedio")]
    public async Task<ActionResult<decimal>> GetPrecioPromedio()
    {
        // Verificamos si hay productos para evitar una división por cero.
        if (!await _context.Products.AnyAsync())
        {
            return Ok(0); // O NotFound("No hay productos para calcular el promedio.")
        }

        // Usamos LINQ para calcular el promedio directamente en la base de datos.
        var precioPromedio = await _context.Products
            .AverageAsync(p => p.Price); // 1. Calcula el promedio de la propiedad Price

        return Ok(precioPromedio);
    }
    
    // GET: api/Productos/sin-descripcion
    [HttpGet("sin-descripcion")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductosSinDescripcion()
    {
        // Usamos LINQ con la función string.IsNullOrEmpty para el filtrado.
        var productos = await _context.Products
            .Where(p => string.IsNullOrEmpty(p.Description)) // 1. Filtra si Description es nulo o vacío
            .ToListAsync();

        if (productos == null || !productos.Any())
        {
            return NotFound("Todos los productos tienen una descripción.");
        }

        return Ok(productos);
    }
    
    // GET: api/Productos/2/clientes
    [HttpGet("{productId}/clientes")]
    public async Task<ActionResult<IEnumerable<Client>>> GetClientesPorProducto(int productId)
    {
        // Verificamos si el producto existe.
        var productExists = await _context.Products.AnyAsync(p => p.Productid == productId);
        if (!productExists)
        {
            return NotFound($"No se encontró el producto con ID {productId}.");
        }

        // Usamos LINQ para navegar desde los detalles de orden hasta los clientes.
        var clientes = await _context.Orderdetails
            .Where(od => od.Productid == productId) // 1. Filtra los detalles por producto
            .Select(od => od.Order.Client)        // 2. Selecciona el cliente de la orden
            .Distinct()                           // 3. Elimina duplicados
            .ToListAsync();

        return Ok(clientes);
    }
}