// Repositories/UnitOfWork.cs
using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.Models;
using Lab08_JeanLazarinos.Repositories.Interfaces;

namespace Lab08_JeanLazarinos.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreLdbContext _context;
    
    // Usamos 'lazy loading' para instanciar los repositorios solo cuando se necesiten
    public IRepository<Product> ProductRepository { get; private set; }
    private IClientRepository? _clientRepository;
    private IOrderRepository? _orderRepository;
    public IRepository<Orderdetail> OrderDetailRepository { get; private set; }

    public UnitOfWork(StoreLdbContext context)
    {
        _context = context;
        ProductRepository = new Repository<Product>(_context);
        
        OrderDetailRepository = new Repository<Orderdetail>(_context);
    }
    public IClientRepository ClientRepository => 
        _clientRepository ??= new ClientRepository(_context);
    
    public IOrderRepository OrderRepository => 
        _orderRepository ??= new OrderRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}