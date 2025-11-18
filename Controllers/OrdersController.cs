// Controllers/OrdersController.cs
using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.DTOs;
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- MUY IMPORTANTE AÑADIR ESTE USING
using Lab08_JeanLazarinos.Services;

namespace Lab08_JeanLazarinos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        // Inyectamos el DbContext directamente aquí para consultas complejas.
        // Es una práctica aceptable cuando se necesita el poder de IQueryable.
        private readonly StoreLdbContext _context;
        private readonly IExcelService _excelService;

        public OrdersController(IUnitOfWork unitOfWork, StoreLdbContext context, IExcelService excelService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrders()
        {
            // Usamos LINQ con 'Select' para proyectar las entidades a DTOs.
            // '.Include()' le dice a EF Core que cargue los datos relacionados.
            var orders = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Orderdetails)
                .ThenInclude(od => od.Product)
                .Select(o => new GetOrderDto
                {
                    OrderId = o.Orderid,
                    OrderDate = o.Orderdate,
                    ClientName = o.Client.Name,
                    Items = o.Orderdetails.Select(od => new GetOrderDetailDto
                    {
                        ProductName = od.Product.Name,
                        Quantity = od.Quantity,
                        Price = od.Product.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<GetOrderDto>> PostOrder(CreateOrderDto orderDto)
        {
            var clientExists = await _unitOfWork.ClientRepository.GetByIdAsync(orderDto.ClientId);
            if (clientExists == null)
            {
                return BadRequest($"El cliente con ID {orderDto.ClientId} no existe.");
            }

            var newOrder = new Order
            {
                Clientid = orderDto.ClientId,
                Orderdate = DateTime.UtcNow
            };
            
            // Aquí hay un detalle: necesitamos los productos para validarlos.
            // Podríamos hacerlo en un bucle, pero es más eficiente traerlos todos de una vez.
            var productIds = orderDto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Productid)).ToListAsync();
            
            if (products.Count != productIds.Count)
            {
                return BadRequest("Uno o más productos no existen.");
            }

            await _unitOfWork.OrderRepository.AddAsync(newOrder);

            foreach (var itemDto in orderDto.Items)
            {
                var newOrderDetail = new Orderdetail
                {
                    Order = newOrder,
                    Productid = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };
                await _unitOfWork.OrderDetailRepository.AddAsync(newOrderDetail);
            }

            await _unitOfWork.CompleteAsync();

            // Mapeamos la entidad creada al DTO de respuesta para evitar el ciclo.
            var createdOrderDto = new GetOrderDto
            {
                OrderId = newOrder.Orderid,
                OrderDate = newOrder.Orderdate,
                ClientName = clientExists.Name, // Ya lo tenemos cargado
                Items = products.Select(p => {
                    var item = orderDto.Items.First(i => i.ProductId == p.Productid);
                    return new GetOrderDetailDto {
                        ProductName = p.Name,
                        Quantity = item.Quantity,
                        Price = p.Price
                    };
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrders), new { id = newOrder.Orderid }, createdOrderDto);
        }
        
        // GET: api/Orders/1/details
        [HttpGet("{orderId}/details")]
        public async Task<ActionResult<IEnumerable<GetOrderDetailDto>>> GetOrderDetails(int orderId)
        {
            // Primero, verificamos si la orden existe para dar un buen mensaje de error.
            var orderExists = await _context.Orders.AnyAsync(o => o.Orderid == orderId);
            if (!orderExists)
            {
                return NotFound($"No se encontró la orden con ID {orderId}.");
            }

            // Aquí combinamos Include, Where y Select para una consulta LINQ potente.
            var orderDetails = await _context.Orderdetails
                .Include(od => od.Product) // 1. Carga el Producto relacionado
                .Where(od => od.Orderid == orderId) // 2. Filtra por el ID de la orden
                .Select(od => new GetOrderDetailDto // 3. Proyecta el resultado a nuestro DTO
                {
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Product.Price
                })
                .ToListAsync(); // 4. Ejecuta la consulta

            return Ok(orderDetails);
        }
        
        // GET: api/Orders/1/total-productos
        [HttpGet("{orderId}/total-productos")]
        public async Task<ActionResult<int>> GetOrderTotalProducts(int orderId)
        {
            // Verificamos si la orden existe.
            var orderExists = await _context.Orders.AnyAsync(o => o.Orderid == orderId);
            if (!orderExists)
            {
                return NotFound($"No se encontró la orden con ID {orderId}.");
            }

            // Usamos LINQ para filtrar y luego sumar directamente en la base de datos.
            var totalProducts = await _context.Orderdetails
                .Where(od => od.Orderid == orderId) // 1. Filtra por el ID de la orden
                .SumAsync(od => od.Quantity);      // 2. Suma la propiedad Quantity

            return Ok(totalProducts);
        }
        
        // GET: api/Orders/despues-de/2025-05-03
        [HttpGet("despues-de/{fecha}")]
        public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrdersAfterDate(DateTime fecha)
        {
            // Reutilizamos la misma lógica de proyección a DTOs que en GetOrders
            // para evitar errores de referencia circular y devolver un JSON limpio.
            var orders = await _context.Orders
                .Where(o => o.Orderdate.Date > fecha.Date) // 1. Filtra por la fecha
                .Include(o => o.Client)
                .Include(o => o.Orderdetails)
                .ThenInclude(od => od.Product)
                .Select(o => new GetOrderDto
                {
                    OrderId = o.Orderid,
                    OrderDate = o.Orderdate,
                    ClientName = o.Client.Name,
                    Items = o.Orderdetails.Select(od => new GetOrderDetailDto
                    {
                        ProductName = od.Product.Name,
                        Quantity = od.Quantity,
                        Price = od.Product.Price
                    }).ToList()
                })
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound($"No se encontraron órdenes después de la fecha {fecha:yyyy-MM-dd}.");
            }

            return Ok(orders);
        }
        
        // GET: api/Orders/5/con-detalles
        [HttpGet("{id}/con-detalles")]
        public async Task<ActionResult<PedidoConDetallesDto>> GetPedidoConDetalles(int id)
        {
            var pedidoConDetalles = await _unitOfWork.OrderRepository.GetPedidoConDetallesAsync(id);
    
            if (pedidoConDetalles == null)
            {
                return NotFound($"No se encontró el pedido con ID {id}.");
            }

            return Ok(pedidoConDetalles);
        }
        
        // --- NUEVO ENDPOINT REPORTE 2 ---
        [HttpGet("{id}/export")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ExportDetallePedido(int id)
        {
            // 1. LÓGICA DE QUERY 
            // Asumo que tienes un método que devuelve este DTO
            var pedidoData = await _unitOfWork.OrderRepository.GetPedidoConDetallesAsync(id);

            if (pedidoData == null)
            {
                return NotFound("No se encontró el pedido.");
            }

            // 2. LÓGICA DE EXCEL 
            var fileBytes = _excelService.GeneratePedidoDetalladoReport(pedidoData);

            // 3. Devolver el archivo
            string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"DetallePedido_{id}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(fileBytes, mimeType, fileName);
        }
    }
    }